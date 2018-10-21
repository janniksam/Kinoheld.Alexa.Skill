namespace Kinoheld.Application.ResponseMessages
{
    internal class Messages : IMessages
    {
        public Messages(string language)
        {
            Language = language;
        }

        public string Language { get; }

        public string SkillName { get; set; }

        public string InvocationName { get; set; }

        public string HelpMessage { get; set; }

        public string StopMessage { get; set; }

        public string Launch { get; set; }
        
        public string UserPreferencesResetted { get; set; }

        public string UserPreferencesResetAbort { get; set; }

        public string UserPreferenceChanged { get; set; }

        public string EmailUnsubscribed { get; set; }

        public string EmailSubscribed { get; set; }

        public string EmailSettingsAbort { get; set; }

        public string ErrorNotFound { get; set; }

        public string ErrorNotFoundIntent { get; set; }

        public string ErrorNotFoundUser { get; set; }

        public string ErrorNoValidCity { get; set; }

        public string ErrorRequestTypeNotSupported { get; set; }

        public string ErrorMissingSessionCinemaObject { get; set; }

        public string ErrorRetrievingShows { get; set; }

        public string ErrorNoCinemaInCity { get; set; }

        public string ErrorCreatingOverview { get; set; }

        public string ErrorNoShowsFoundForDateFormat { get; set; }

        public string DayOverviewFormat { get; set; }
    }
}
