using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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
                foreach (var e in Cache.Cameras)
                {
                    var camera = e.Value;
                    using (var webClient = new WebClient())
                    {
                        var data = webClient.DownloadData(camera.Url);

                        using (var mem = new MemoryStream(data))
                        {
                            using (var image = Image.FromStream(mem))
                            {
                                var bmp = new Bitmap(image);
                                lock (Cache.ImageLocker)
                                {
                                    image.Save($"{camera.Number}.jpg", ImageFormat.Jpeg);
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
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                },
                ct,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }
    }
}