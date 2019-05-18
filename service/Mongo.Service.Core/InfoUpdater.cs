using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AForge;
using AForge.Imaging.Filters;
using Mongo.Service.Core.Storable;
using Mongo.Service.Core.Storage;

namespace Mongo.Service.Core
{
    public class InfoUpdater : IInfoUpdater
    {
        private readonly IEntityStorage<Info> infoRepository;

        public InfoUpdater(IEntityStorage<Info> infoRepository)
        {
            this.infoRepository = infoRepository;
        }

        public async Task UpdateInfosAsync()
        {
            try
            {
                var cameras = new List<Camera>();
                lock (Cache.Locker)
                {
                    foreach (var e in Cache.Cameras)
                    {
                        cameras.Add(e.Value);
                    }
                }

                foreach (var camera in cameras)
                {
                    using (var webClient = new WebClient())
                    {
                        var data = webClient.DownloadData(camera.Url);

                        using (var mem = new MemoryStream(data))
                        {
                            using (var image = Image.FromStream(mem))
                            {
                                var bmp = new Bitmap(image);
                                using (var graphics = Graphics.FromImage(image))
                                {
                                    ApplyGrayFilter(bmp);
                                    var info = DrawPlaces(graphics, bmp, camera.Places);
                                    info.CameraNumber = camera.Number;
                                    info.Description = camera.Description;
                                    lock (Cache.Locker)
                                    {
                                        image.Save($"{camera.Number}.jpg", ImageFormat.Jpeg);
                                    }

                                    var infoFromBase = (await this.infoRepository.ReadAsync(x => x.CameraNumber == camera.Number)).SingleOrDefault();
                                    if (infoFromBase == null)
                                    {
                                        infoFromBase = new Info { Id = Guid.NewGuid() };
                                    }

                                    info.Id = infoFromBase.Id;
                                    await this.infoRepository.WriteAsync(info).ConfigureAwait(false);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Start()
        {
            var ct = new CancellationToken();
            Task.Factory.StartNew(
                async () =>
                {
                    while (!ct.IsCancellationRequested)
                    {
                        await this.UpdateInfosAsync().ConfigureAwait(false);
                        await Task.Delay(TimeSpan.FromSeconds(2), ct).ConfigureAwait(false);
                    }
                },
                ct,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        public void ApplyGrayFilter(Bitmap bmp)
        {
            var filter = new ColorFiltering();
            filter.Red = new IntRange(150, 200);
            filter.Green = new IntRange(150, 200);
            filter.Blue = new IntRange(150, 200);
            //filter.ApplyInPlace(img);
            var grayscale = new Grayscale(0.2125, 0.7154, 0.0721);
            var img = grayscale.Apply(bmp);
        }

        private Info DrawPlaces(Graphics graphics, Bitmap grayBmp, IList<Place> places)
        {
            var info = new Info();
            foreach (var place in places)
            {
                var count = 0;
                long pixelsSummaryColor = 0;
                for (var i = place.X; i <= place.X + place.Width; i++)
                {
                    for (var j = place.Y; j <= place.Y + place.Height; j++)
                    {
                        count++;
                        var pixel = grayBmp.GetPixel(i, j);
                        pixelsSummaryColor += pixel.ToArgb();
                    }
                }

                var mediumColor = pixelsSummaryColor / count;
                const int colorDelta = 1500000;
                var closeCount = 0;
                for (var i = place.X; i <= place.X + place.Width; i++)
                {
                    for (var j = place.Y; j <= place.Y + place.Height; j++)
                    {
                        var pixel = grayBmp.GetPixel(i, j).ToArgb();
                        if (Math.Abs(mediumColor - pixel) < colorDelta)
                        {
                            closeCount++;
                        }
                    }
                }

                var pen = new Pen(Color.Chartreuse, 2);
                var percent = (double)closeCount / count * 100;
                if (percent < 90)
                {
                    info.OccupiedPlaces++;
                    pen = new Pen(Color.Red, 2);
                }
                else
                {
                    info.FreePlaces++;
                }

                info.TotalPlaces++;

                graphics.DrawRectangle(pen, new Rectangle(place.X, place.Y, place.Width, place.Height));
            }

            return info;
        }
    }
}