using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge;
using AForge.Imaging.Filters;
using AForge.Video;
using Newtonsoft.Json;
using Refit;
using Point = System.Drawing.Point;

namespace CarDetection
{
    public partial class Form1 : Form
    {
        private const string PlacesPath = "Places.json";
        private NewFrameEventHandler frameHandler;
        private JPEGStream videoSource;
        private readonly object locker = new object();
        private readonly IDetectionApi detectionApi;

        public Form1()
        {
            InitializeComponent();
            this.detectionApi = RestService.For<IDetectionApi>("http://localhost:12512");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            videoSource = new JPEGStream("http://94.72.19.56/jpg/image.jpg?size=3");

            frameHandler = new NewFrameEventHandler(video_NewFrame);
            videoSource.NewFrame += frameHandler;
            videoSource.Start();
            if (File.Exists(PlacesPath))
            {
                DataCache.Places = JsonConvert.DeserializeObject<IList<Place>>(File.ReadAllText(PlacesPath));
            }

            RefreshPlaces();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            var imag = Image.FromFile("C:\\2.png");
            var grayscale = new Grayscale(0.2125, 0.7154, 0.0721);
            var grayImage = grayscale.Apply(new Bitmap(imag));

            const int x = 560;
            const int y = 550;
            const int deltaX = 80;
            const int deltaY = 30;
            var rectPoint = new Rectangle(new Point(x, y), new Size(deltaX, deltaY));
            e.Graphics.DrawImage(grayImage, new Point(0, 0));
            e.Graphics.DrawRectangle(new Pen(Color.Yellow, 2), rectPoint);
            var count = 0;
            long pixelsSummaryColor = 0;
            for (var i = x; i <= x + deltaX; i++)
            {
                for (var j = y; j <= y + deltaY; j++)
                {
                    count++;
                    var pixel = grayImage.GetPixel(i, j);
                    pixelsSummaryColor += pixel.ToArgb();
                }
            }

            var mediumColor = pixelsSummaryColor / count;
            const int colorDelta = 350000;
            var closeCount = 0;
            for (var i = x; i <= x + deltaX; i++)
            {
                for (var j = y; j <= y + deltaY; j++)
                {
                    var pixel = grayImage.GetPixel(i, j).ToArgb();
                    if (Math.Abs(mediumColor - pixel) < colorDelta)
                    {
                        closeCount++;
                    }
                }
            }

            var percent = (double)closeCount / count * 100;
            e.Graphics.DrawString(percent.ToString(), new Font(FontFamily.GenericMonospace, 14.0f), new SolidBrush(Color.Aqua), 200, 200);
        }

        public void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            var frame = eventArgs.Frame;
            //var img = frame;
            ColorFiltering filter = new ColorFiltering();
            // set color ranges to keep
            filter.Red = new IntRange(150, 200);
            filter.Green = new IntRange(150, 200);
            filter.Blue = new IntRange(150, 200);
            // apply the filter
            //filter.ApplyInPlace(img);
            var grayscale = new Grayscale(0.2125, 0.7154, 0.0721);
            var img = grayscale.Apply(frame);

            //do processing here
            using (var graphics = pictureBox1.CreateGraphics())
            {
                graphics.DrawImage(img, new Point(0, 0));
                this.pictureBox1.Invoke(
                    (MethodInvoker)delegate
                    {
                        // Running on the UI thread
                        this.pictureBox1.Width = img.Width;
                        this.pictureBox1.Height = img.Height;
                        this.Height = this.pictureBox1.Height + 100;
                        this.Width = this.pictureBox1.Width + this.listBox1.Width;
                        DrawPlaces(graphics, img);
                    });
            }
        }

        private void DrawPlaces(Graphics graphics, Bitmap grayBmp)
        {
            lock (locker)
            {
                var pen = new Pen(Color.Yellow, 2);
                foreach (var place in DataCache.Places)
                {
                    graphics.DrawRectangle(pen, new Rectangle(place.X, place.Y, place.Width, place.Height));
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

                    var percent = (double)closeCount / count * 100;
                    graphics.DrawString(
                        percent.ToString(),
                        new Font(FontFamily.GenericMonospace, 10.0f),
                        new SolidBrush(Color.Aqua),
                        place.X + place.Width / 2,
                        place.Y + place.Height / 2);
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            videoSource.Stop();
        }

        private void savePlaceButton_Click(object sender, EventArgs e)
        {
            lock (locker)
            {
                var index = listBox1.SelectedIndex;
                if (index != -1)
                {
                    var place = DataCache.Places[index];
                    place.X = Convert.ToInt32(this.xBox.Text);
                    place.Y = Convert.ToInt32(this.yBox.Text);
                    place.Width = Convert.ToInt32(this.widthBox.Text);
                    place.Height = Convert.ToInt32(this.heightBox.Text);
                }
                else
                {
                    DataCache.Places.Add(
                        new Place
                        {
                            Id = DataCache.Places.Count + 1,
                            X = Convert.ToInt32(this.xBox.Text),
                            Y = Convert.ToInt32(this.yBox.Text),
                            Width = Convert.ToInt32(this.widthBox.Text),
                            Height = Convert.ToInt32(this.heightBox.Text)
                        });
                }

                RefreshPlaces();
                listBox1.SelectedIndex = index;
            }
        }

        private void RefreshPlaces()
        {
            var serialized = JsonConvert.SerializeObject(DataCache.Places);
            File.WriteAllText(PlacesPath, serialized);
            this.listBox1.Items.Clear();
            foreach (var place in DataCache.Places)
            {
                this.listBox1.Items.Add(place.ToString());
            }
        }

        private void listBox1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                var index = listBox1.SelectedIndex;
                var place = DataCache.Places[index];
                xBox.Text = place.X.ToString();
                yBox.Text = place.Y.ToString();
                widthBox.Text = place.Width.ToString();
                heightBox.Text = place.Height.ToString();
            }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            lock (locker)
            {
                DataCache.Places.Add(
                    new Place
                    {
                        Id = DataCache.Places.Count + 1,
                        X = Convert.ToInt32(this.xBox.Text),
                        Y = Convert.ToInt32(this.yBox.Text),
                        Width = Convert.ToInt32(this.widthBox.Text),
                        Height = Convert.ToInt32(this.heightBox.Text)
                    });

                RefreshPlaces();
            }
        }

        private void sendToServer_Click(object sender, EventArgs e)
        {
            IList<Place> placesCopy;
            lock (this.locker)
            {
                placesCopy = new List<Place>(DataCache.Places.Count);
                foreach (var place in DataCache.Places)
                {
                    placesCopy.Add(place);
                }
            }

            this.detectionApi.PutCameraAsync(
                    new Camera
                    {
                        Number = 1,
                        Places = placesCopy
                    })
                .Wait();
        }
    }
}