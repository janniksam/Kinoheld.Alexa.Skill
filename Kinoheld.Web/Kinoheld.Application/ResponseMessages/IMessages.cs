using System;

namespace Kinoheld.Application.ResponseMessages
{
    public interface IMessages
    {
        string Language { get; }

        string SkillName { get; set; }

        string InvocationName { get; set; }

        string HelpMessage { get; set; }

        string StopMessage { get; set; }

        string Launch { get; set; }

        string UserPreferencesResetted { get; }

        string UserPreferencesResetAbort { get; }

        string UserPreferenceChanged { get; }

        string EmailUnsubscribed { get; }

        string EmailSubscribed { get; }

        string EmailSettingsAbort { get; }

        string ErrorNotFound { get; }

        string ErrorNotFoundIntent { get; }

        string ErrorNoValidCity { get; }

        string ErrorRequestTypeNotSupported { get; }

        string ErrorMissingSessionCinemaObject { get; }

        string ErrorRetrievingShows { get; }

        string ErrorNoCinemaInCity { get; }

        string ErrorCreatingOverview { get; }

        string ErrorNoShowsFoundForDateFormat { get; }

        string DayOverviewFormat { get; }
    }
}