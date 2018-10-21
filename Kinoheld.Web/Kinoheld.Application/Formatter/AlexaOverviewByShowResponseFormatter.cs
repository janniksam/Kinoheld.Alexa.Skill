using System.Linq;
using System.Text;
using Kinoheld.Application.Model;
using Kinoheld.Application.ResponseMessages;
using Kinoheld.Base.Formatter;
using Kinoheld.Base.Utils;

namespace Kinoheld.Application.Formatter
{
    public class AlexaOverviewByShowResponseFormatter : ISsmlMessageFormatter<DayOverview>
    {
        private readonly IMessages m_messages;
        private readonly IRandomGenerator m_randomGenerator;

        public AlexaOverviewByShowResponseFormatter(IMessages messages, IRandomGenerator randomGenerator)
        {
            m_messages = messages;
            m_randomGenerator = randomGenerator;
        }

        public string Format(DayOverview overview)
        {
            if (overview?.Cinema == null)
            {
                return m_messages.ErrorCreatingOverview;
            }

            if (overview.Movies == null ||
                overview.Movies.Count == 0)
            {
                return string.Format(m_messages.ErrorNoShowsFoundForDateFormat, overview.Date.Date.ToShortDateString());
            }
            
            var dayOverviewSsml = m_messages.DayOverviewFormat;

            var stringBuilder = new StringBuilder();
            foreach (var movie in overview.Movies.OrderBy(p => p.Name))
            {
                stringBuilder.Append(FormatMovie(movie));
            }

            return string.Format(dayOverviewSsml, overview.Cinema.Name, stringBuilder.ToString());
        }

        private string FormatMovie(Movie movie)
        {
            var pickSentenceStyle = m_randomGenerator.Generate(0, 5);
            string format;
            switch (pickSentenceStyle)
            {
                default:
                {
                    format = $"<p>{movie.Name} <break strength=\"x-strong\" time=\"850ms\" /> läuft um {{0}}.</p>";
                    break;
                }
                case 2:
                case 3:
                {
                    format = $"<p>{movie.Name} <break strength=\"x-strong\" time=\"850ms\" /> wird um {{0}} gezeigt.</p>";
                    break;
                }
                case 4:
                {
                    format = $"<p>{movie.Name} <break strength=\"x-strong\" time=\"850ms\" /> beginnt um {{0}}.</p>";
                    break;
                }
                case 5:
                {
                    format = $"<p>{movie.Name} <break strength=\"x-strong\" time=\"850ms\" /> startet um {{0}}.</p>";
                    break;
                }
            }
            
            var showTimes = movie.Vorstellungen.OrderBy(p => p.VorstellungTime)
                .Select(p => MarkAsTime(p.VorstellungTime.ToString("hh\\:mm"))).ToList();

            string times;
            if (showTimes.Count > 1)
            {
                times = string.Format($"{string.Join(", ", showTimes.Take(showTimes.Count - 1))} und {showTimes.Last()}");
            }
            else
            {
                times = showTimes[0];
            }

            return string.Format(format, times);
        }

        private string MarkAsTime(string time)
        {
            return $"<say-as interpret-as=\"time\">{time}</say-as>";
        }
    }
}