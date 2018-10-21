using System;
using System.Linq;
using System.Text;
using Kinoheld.Application.Model;
using Kinoheld.Base.Formatter;

namespace Kinoheld.Application.Formatter
{
    [Obsolete("Old format, which is not as easy to follow as a listener")]
    public class AlexaOverviewByTimeResponseFormatter : ISsmlMessageFormatter<DayOverview>
    {
        public string Format(DayOverview overview)
        {
            if (overview?.Cinema == null)
            {
                return "<speak>Fehler bei der Generierung der Übersicht</speak>";
            }

            if (overview.Movies == null ||
                overview.Movies.Count == 0)
            {
                return $"<speak>Es wurden keine Vorstellungen gefunden für den <say-as interpret-as=\"date\">{overview.Date.Date.ToShortDateString()}</say-as></speak>";
            }
            
            var stringBuilder =
                new StringBuilder($"<speak><p>Hier ist deine Übersicht zu {overview.Cinema.Name}:</p>");

            var vorstellungsZeiten = overview.Movies.SelectMany(p => p.Vorstellungen).Select(p => p.VorstellungTime).Distinct().OrderBy(p => p);
            foreach (var vorstellungszeit in vorstellungsZeiten)
            {
                if (vorstellungszeit.Minutes != 0)
                {
                    stringBuilder.AppendFormat("<p>Um {0} Uhr {1} läuft: ", vorstellungszeit.Hours,
                        vorstellungszeit.Minutes);
                }
                else
                {
                    stringBuilder.AppendFormat("<p>Um {0} Uhr läuft: ", vorstellungszeit.Hours);
                }

                var moviesAtThatTime = overview.Movies
                    .Where(m => m.Vorstellungen.Any(p => p.VorstellungTime == vorstellungszeit))
                    .ToList();

                if (moviesAtThatTime.Count == 1)
                {
                    stringBuilder.AppendFormat("{0}.</p>", moviesAtThatTime.First().Name);
                }
                else
                {
                    var orderedMovies = moviesAtThatTime.OrderBy(p => p.Name).Select(p => p.Name).ToList();
                    stringBuilder.Append(string.Join(", ", orderedMovies.Take(orderedMovies.Count - 1)));
                    stringBuilder.AppendFormat(" und {0}.</p>", orderedMovies.Last());
                }
            }

            stringBuilder.Append("</speak>");
            return stringBuilder.ToString();
        }
    }
}