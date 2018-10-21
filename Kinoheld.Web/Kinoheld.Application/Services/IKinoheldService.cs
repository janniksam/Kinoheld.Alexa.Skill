using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kinoheld.Api.Client.Model;
using Kinoheld.Application.Model;

namespace Kinoheld.Application.Services
{
    public interface IKinoheldService
    {
        Task<List<Cinema>> GetCinemas(string city);

        Task<DayOverview> GetDayOverviewForCinema(Cinema cinema, DateTime showDate, string alexaChosenTime);
    }
}