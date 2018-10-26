using System;
using System.Collections.Generic;
using Kinoheld.Api.Client.Model;

namespace Kinoheld.Application.Model
{
    public class DayOverview
    {
        public DayOverview()
        {
            Movies = new List<Movie>();
        }

        public string AlexaId { get; set; }

        public Cinema Cinema { get; set; }

        public DateTime Date { get; set; }

        public List<Movie> Movies { get; }
    }
}