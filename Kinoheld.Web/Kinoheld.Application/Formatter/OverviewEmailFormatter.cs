using System.Linq;
using System.Text;
using Kinoheld.Application.Model;
using Kinoheld.Application.ResponseMessages;
using Kinoheld.Base.Formatter;

namespace Kinoheld.Application.Formatter
{
    public class OverviewEmailFormatter : IEmailBodyFormatter<DayOverview>
    {
        private readonly IMessages m_messages;

        public OverviewEmailFormatter(IMessages messages)
        {
            m_messages = messages;
        }

        public string Format(DayOverview overview)
        {
            if (overview?.Movies == null)
            {
                return null;
            }

            if (overview.Movies.Count == 0)
            {
                return null;
            }
            
            var builder = new StringBuilder($"<html><p>Hallo,<br/><br/>nachfolgend findest du deine Übersicht zum {overview.Date:dd.MM.yyyy}.</p><h1>Übersicht {overview.Cinema.Name}</h1>");

            foreach (var movie in overview.Movies.OrderBy(p => p.Name))
            {
                builder.Append($"<h2>{movie.Name}</h2><h4>Vorstellungen:</h4><ul>");

                foreach (var vorstellung in movie.Vorstellungen)
                {
                    var urlVorstellung = vorstellung.DetailUrl;
                    builder.Append($"<li>{vorstellung.VorstellungTime:hh\\:mm} (<a href=\"{urlVorstellung}\">reservieren/kaufen</a>)</li>");
                }
                builder.Append("</ul>");
            }

            builder.Append($"<p>Diese E-Mail wurde automatisch generiert durch den Alexa-Skill &quot;{m_messages.SkillName}&quot;.</p><p>Um zukünftig keine E-Mails mehr zu erhalten, kannst du dem Skill sagen: \"Alexa sage {m_messages.InvocationName}, Email-Einstellungen ändern.\"</p></html>");

            return builder.ToString();
        }
    }
}