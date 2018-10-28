using System.Linq;
using System.Text;
using Kinoheld.Application.Model;
using Kinoheld.Application.ResponseMessages;
using Kinoheld.Base;
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
                builder.AppendLine($"<h2>{movie.Name}</h2>");
                builder.AppendLine("<table style=\"width:100%\">");
                builder.AppendLine("    <tr>");
                builder.AppendLine("        <td style=\"width:15%;\">");
                builder.AppendLine($"           <img align=\"top\" style=\"width: 100px;\" src=\"{movie.ThumbnailUrl}\" alt=\"movie-poster\" />");
                builder.AppendLine("        </td>");
                builder.AppendLine("    </tr>");
                builder.AppendLine("    <tr style=\"height:10px;\">");
                builder.AppendLine("    </tr>");
                builder.AppendLine("    <tr>");
                builder.AppendLine("        <td>");
                builder.AppendLine("            <h4>Beschreibung</h4>");
                builder.AppendLine("            <details>");
                builder.AppendLine("                <summary>Beschreibung lesen...</summary>");
                builder.AppendLine($"               <p>{movie.Description}</p");
                builder.AppendLine("            </details>");
                builder.AppendLine("            <h4>Vorstellungen</h4>");
                builder.AppendLine("            <ul>");
                foreach (var vorstellung in movie.Vorstellungen)
                {
                    var urlVorstellung = vorstellung.DetailUrl;
                    builder.AppendLine($"           <li>{vorstellung.VorstellungTime:hh\\:mm} (<a href=\"{urlVorstellung}\">reservieren/kaufen</a>)</li>");
                }
                builder.AppendLine("            </ul>");
                builder.AppendLine("        </td>");
                builder.AppendLine("    </tr>");
                builder.AppendLine("</table>");
            }

            var unsubscibeUrl = string.Format(Constants.BaseUnsubscribeUrlFormat, overview.AlexaId);
            builder.AppendLine($"<p style=\"font-size:12px;\">Diese E-Mail wurde automatisch generiert durch den Alexa-Skill &quot;{m_messages.SkillName}&quot;.</p>");
            builder.AppendLine($"<p style=\"font-size:12px;\">Um zukünftig keine E-Mails mehr zu erhalten, kannst du dem Skill sagen: \"Alexa sage {m_messages.InvocationName}, Email-Einstellungen ändern.\".</p>");
            builder.AppendLine($"<p style=\"font-size:12px;\">Alternativ klicke auf folgenden Link: <a href='{unsubscibeUrl}'>Emails abbestellen</a></p></html>");

            return builder.ToString();
        }
    }
}