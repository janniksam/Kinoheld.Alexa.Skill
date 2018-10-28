using System.Collections.Generic;

namespace Kinoheld.Application.Model
{
    public class Movie
    {
        public Movie()
        {
            Vorstellungen = new List<Vorstellung>();
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public string ThumbnailUrl { get; set; }

        public List<Vorstellung> Vorstellungen { get; }
    }
}