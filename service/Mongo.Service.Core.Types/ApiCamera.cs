﻿using System.Collections.Generic;

namespace Mongo.Service.Core.Types
{
    public class ApiCamera
    {
        private IList<ApiPlace> places;

        public string Url { get; set; }

        public int Number { get; set; }

        public string Description { get; set; }

        public IList<ApiPlace> Places
        {
            get => this.places ?? (this.places = new List<ApiPlace>());
            set => this.places = value;
        }
    }
}