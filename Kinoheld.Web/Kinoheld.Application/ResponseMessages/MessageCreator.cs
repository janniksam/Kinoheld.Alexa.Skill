namespace Kinoheld.Application.ResponseMessages
{
    public static class MessageCreator
    {
        public static IMessages CreateMessages()
        {
            var deDeResource = new Messages("de-de")
            {
                SkillName = "Kino Vorschau",
                InvocationName = "Kino Vorschau",
                HelpMessage =
                    "Ich kann dir eine Übersicht zu anstehenden Vorstellungen eines beliebigen Kinos geben. " +
                    "Frage mich zum Beispiel: Alexa frage Kino Vorschau was läuft im Kino. " +
                    "Ein weiteres Beispiel: Alexa, frage Kino Vorschau, was heute abend im Kino Aurich läuft.",
                StopMessage = "Bis bald!",

                Launch = "Kino Vorschau wurde gestartet. " +
                         "Du kannst mich nach einer Übersicht zu einem bestimmten Kino und einem bestimmten Datum fragen. " +
                         "Frage jetzt einfach: Was läuft im Kino.",

                DayOverviewFormat = "<speak><p>Hier ist deine Übersicht zu {0}:</p>{1}</speak>",

                UserPreferenceChanged = "Deine Stadt {0} wurde als Vorbelegung für zukünftige Anfragen erfolgreich übernommen!",
                UserPreferencesResetted = "Deine Benutzereinstellungen wurden erfolgreich zurückgesetzt!",
                UserPreferencesResetAbort = "OK. Komme wieder, wenn du es dir anders überlegst.",

                EmailSubscribed = "Deine E-Mail-Einstellungen wurden erfolgreich geändert. Du wirst zukünftig wieder Nachrichten erhalten.",
                EmailUnsubscribed = "Deine E-Mail-Einstellungen wurden erfolgreich geändert. " +
                                    "Du wirst zukünftig keine E-Mails mehr von uns erhalten. " +
                                    "Du kannst dies jederzeit wieder ändern, indem du erneut E-Mail-Einstellungen ändern aufrufst.",
                EmailSettingsAbort = "OK. Komme wieder, wenn du es dir anders überlegst.",

                ErrorNotFound = "Entschuldigung, das habe ich nicht verstanden.",
                ErrorNotFoundIntent = "Der Intent wurde nicht gefunden.",
                ErrorNotFoundUser = "Es wurde keine Benutzerkennung übermittelt.",
                ErrorRequestTypeNotSupported = "Der Requesttype \"{0}\" wird von diesem Skill nicht unterstützt.",
                ErrorMissingSessionCinemaObject = "Fehler bei der Skill-Verarbeitung. In der aktuellen Session fehlt das Objekt \"{0}\".",
                ErrorNoValidCity = "Es muss eine gültige Stadt ausgewählt werden.",
                ErrorRetrievingShows = "Beim Ermitteln der Vorstellung ist ein Fehler aufgetreten.",
                ErrorNoCinemaInCity = "Es wurden keine Kinos in der Nähe von {0} gefunden",
                ErrorCreatingOverview = "<speak>Fehler bei der Generierung der Übersicht</speak>",
                ErrorNoShowsFoundForDateFormat = "<speak>Es wurden keine Vorstellungen für den <say-as interpret-as=\"date\">{0}</say-as> gefunden</speak>",
            };

            return deDeResource;
        }
    }
}
