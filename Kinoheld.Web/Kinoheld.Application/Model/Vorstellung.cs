using System;

namespace Kinoheld.Application.Model
{
    public class Vorstellung
    {
        public Vorstellung(long id, string name, TimeSpan vorstellungTime, string detailUrl)
        {
            Id = id;
            Name = name;
            VorstellungTime = vorstellungTime;
            DetailUrl = detailUrl;
        }

        public long Id { get; }

        public string Name { get; }

        public TimeSpan VorstellungTime { get; }
        
        public string DetailUrl { get; }
    }
}