using System.Collections.Generic;

namespace CarDetection
{
    public class Camera
    {
        private IList<Place> places;

        public string Url { get; set; }

        public int Number { get; set; }

        public string Description { get; set; }

        public IList<Place> Places
        {
            get => this.places ?? (this.places = new List<Place>());
            set => this.places = value;
        }
    }
}