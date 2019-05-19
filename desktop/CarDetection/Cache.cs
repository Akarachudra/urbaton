using System.Collections.Generic;

namespace CarDetection
{
    public static class Cache
    {
        public static IList<Camera> Cameras = new List<Camera>();

        public static IList<Feedback> Feedbacks = new List<Feedback>();
    }
}