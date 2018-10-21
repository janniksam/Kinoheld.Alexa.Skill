﻿namespace Kinoheld.Base
{
    public class Constants
    {
        public static class Intents
        {
            //Basic stuff
            public const string Stop = "AMAZON.StopIntent";
            public const string Cancel = "AMAZON.CancelIntent";
            public const string Help = "AMAZON.HelpIntent";

            //Overviews
            public const string GetOverviewDay = "GetOverviewDay";

            //Preferences
            public const string SetUserPreferences = "SetUserPreferences";
            public const string ResetUserPreferences = "ResetUserPreferences";
            public const string ToggleEmailSettings = "ToggleEmailSettings";
        }

        public static class Session
        {
            public const string Cinemas = "cinema";
        }
    }
}