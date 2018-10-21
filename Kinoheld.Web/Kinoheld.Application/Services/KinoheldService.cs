using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kinoheld.Api.Client;
using Kinoheld.Api.Client.Model;
using Kinoheld.Application.Model;
using Kinoheld.Base.Utils;
using Microsoft.Extensions.Logging;

namespace Kinoheld.Application.Services
{
    public class KinoheldService : IKinoheldService
    {
        private const int TimeoutApiCallsMs = 500;
        private const int MaxRetriesApiCalls = 20;

        private static readonly SemaphoreSlim m_semaphoreSlim = new SemaphoreSlim(2, 2);
        private readonly ILogger<KinoheldService> m_logger;

        public KinoheldService(ILogger<KinoheldService> logger)
        {
            m_logger = logger;
        }

        public async Task<List<Cinema>> GetCinemas(string city)
        {
            try
            {
                var cinemas = await TaskRetryHelper.WithRetry(() => GetCinemasFromApi(city), MaxRetriesApiCalls).ConfigureAwait(false);
                return cinemas;
            }
            catch (TaskCanceledException e)
            {
                m_logger.LogError(e, $"Max Timeout Number exceeded the limit of {MaxRetriesApiCalls}");
                throw;
            }
        }

        public async Task<DayOverview> GetDayOverviewForCinema(Cinema cinema, DateTime showDate, string alexaChosenTime)
        {
            try
            {
                var shows = await TaskRetryHelper.WithRetry(() => GetShowsFromApi(cinema, showDate), MaxRetriesApiCalls).ConfigureAwait(false);

                m_logger.LogDebug("Parsing day overview");
                var dayOverview = ParseDayOverview(cinema, shows, showDate, alexaChosenTime);
                return dayOverview;
            }
            catch (TaskCanceledException e)
            {
                m_logger.LogError(e, $"Max Timeout Number exceeded the limit of {MaxRetriesApiCalls}");
                throw;
            }
        }

        private async Task<List<Show>> GetShowsFromApi(Cinema cinema, DateTime showDate)
        {
            await m_semaphoreSlim.WaitAsync();
            try
            {
                var client = new KinoheldClient();

                m_logger.LogDebug("Retrieving shows for selected cinema");
                var cts = new CancellationTokenSource(TimeoutApiCallsMs);
                var shows = (await client.GetShows(cinema.Id, showDate, cancellationToken: cts.Token).ConfigureAwait(false)).ToList();
                m_logger.LogDebug($"{shows.Count} shows were found.");

                return shows;
            }
            catch (Exception e)
            {
                m_logger.LogError(e, "Error retrieving shows for selected cinema");
                throw;
            }
            finally
            {
                m_semaphoreSlim.Release();
            }
        }

        private async Task<List<Cinema>> GetCinemasFromApi(string city)
        {
            await m_semaphoreSlim.WaitAsync();
            try
            {
                m_logger.LogDebug("Retrieving cinemas near selected city");
                var cts = new CancellationTokenSource(TimeoutApiCallsMs);
                var client = new KinoheldClient();
                var cinemas = await client.GetCinemas(city, cancellationToken: cts.Token).ConfigureAwait(false);
                return cinemas.ToList();
            }
            catch (Exception e)
            {
                m_logger.LogError(e, "Error retrieving shows for selected cinema");
                throw;
            }
            finally
            {
                m_semaphoreSlim.Release();
            }
        }

        private DayOverview ParseDayOverview(Cinema cinema, IEnumerable<Show> shows, DateTime vorstellungsdatum, string vorstellungszeit)
        {
            if (shows == null)
            {
                m_logger.LogError("shows is NULL");
                return null;
            }

            m_logger.LogDebug("Getting all relevant showtimes");
            var timeNow = DateTime.UtcNow;
            var showsOnDate = shows.Where(p => p.Beginning.GetDateTime().Date == vorstellungsdatum.Date && 
                                               p.Beginning.GetDateTime() >= timeNow);

            var dayOverview = new DayOverview
            {
                Cinema = cinema,
                Date = vorstellungsdatum.Date.ToLocalTime()
            };

            m_logger.LogDebug("Generating day overview");
            var filmGroup = showsOnDate.GroupBy(p => p.MovieInfo.Id);
            foreach (var movieGroup in filmGroup)
            {
                var movie = new Movie
                {
                    Name = FormatMovieName(movieGroup.First().MovieInfo.Title)
                };

                foreach (var movieVorstellung in movieGroup)
                {
                    var showTime = movieVorstellung.Beginning.GetDateTime().ToLocalTime();
                    if (!string.IsNullOrEmpty(vorstellungszeit))
                    {
                        switch (vorstellungszeit)
                        {
                            case "EV" when showTime.Hour < 18:
                                {
                                    continue;
                                }
                            case "AF" when showTime.Hour > 18:
                                {
                                    continue;
                                }
                        }
                    }

                    movie.Vorstellungen.Add(
                        new Vorstellung(movieVorstellung.Id, movieVorstellung.Name, showTime.TimeOfDay, movieVorstellung.DetailUrl.AbsoluteUrl));
                }

                if (movie.Vorstellungen.Count == 0)
                {
                    continue;
                }

                dayOverview.Movies.Add(movie);
            }

            m_logger.LogDebug("Parsing done");
            return dayOverview;
        }

        private static string FormatMovieName(string movieName)
        {
            if (movieName.EndsWith(", Die"))
            {
                movieName = $"Die {movieName.Substring(0, movieName.Length - 5)}";
            }
            if (movieName.EndsWith(", Der"))
            {
                movieName = $"Der {movieName.Substring(0, movieName.Length - 5)}";
            }
            if (movieName.EndsWith(", Das"))
            {
                movieName = $"Das {movieName.Substring(0, movieName.Length - 5)}";
            }
            if (movieName.EndsWith("!"))
            {
                movieName = movieName.Substring(0, movieName.Length - 1);
            }

            return movieName;
        }
    }
}