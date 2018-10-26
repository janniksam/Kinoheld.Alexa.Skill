using System;
using Alexa.NET.Response;
using Kinoheld.Api.Client.Model;

namespace Kinoheld.Application.Services
{
    public class GetOverviewInput
    {
        public SkillResponse PendingResponse { get; set; }

        public DateTime SelectedDate { get; set; }

        public string SelectedDayTime { get; set; }

        public Cinema SelectedCinema { get; set; }

        public string SelectedCity { get; set; }
    }
}