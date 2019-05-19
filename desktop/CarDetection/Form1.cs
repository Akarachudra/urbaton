using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AForge;
using AForge.Imaging.Filters;
using AForge.Video;
using Refit;
using Point = System.Drawing.Point;

namespace CarDetection
{
    public partial class Form1 : Form
    {
        private NewFrameEventHandler frameHandler;
        private JPEGStream videoSource;
        private int cameraIndex;
        private readonly object locker = new object();
        private readonly IDetectionApi detectionApi;

        public Form1()
        {
            InitializeComponent();
            this.detectionApi = RestService.For<IDetectionApi>("http://10.33.102.133:12512");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var cameras = this.detectionApi.GetAllCamerasAsync().Result;
            Cache.Cameras = cameras;
            foreach (var camera in cameras)
            {
                comboBox1.Items.Add(camera.Description);
            }

            comboBox1.SelectedIndex = 0;
            SetCamera(0);
            RefreshFeedbacks();
        }

        private void SetCamera(int index)
        {
            if (videoSource != null)
            {
                videoSource.Stop();
                videoSource.NewFrame -= frameHandler;
            }

            var camera = Cache.Cameras[index];

            videoSource = new JPEGStream(camera.Url);

            frameHandler = new NewFrameEventHandler(video_NewFrame);
            videoSource.NewFrame += frameHandler;
            videoSource.Start();

            RefreshPlaces();
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

            this.pictureBox1.Invoke(
                (MethodInvoker)delegate
                {
                    lock (locker)
                    {
                        // Running on the UI thread
                        try
                        {
                            this.pictureBox1.Width = img.Width;
                            this.pictureBox1.Height = img.Height;
                            this.Height = this.pictureBox1.Height + 100;
                            this.Width = this.pictureBox1.Width + this.listBox1.Width;
                            var drawingBitmap = new Bitmap(img.Width, img.Height);
                            var graphics = Graphics.FromImage(drawingBitmap);
                            graphics.DrawImage(img, new Point(0, 0));
                            DrawPlaces(graphics, img);
                            this.pictureBox1.Image = drawingBitmap;
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                });
        }

        private void DrawPlaces(Graphics graphics, Bitmap grayBmp)
        {
            var index = -1;
            foreach (var place in Cache.Cameras[cameraIndex].Places)
            {
                index++;
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
                var pen = new Pen(Color.Chartreuse, 2);
                if (percent < 90)
                {
                    pen = new Pen(Color.Red, 2);
                }

                if (listBox1.SelectedIndex == index)
                {
                    pen = new Pen(Color.DodgerBlue, 3);
                }

                graphics.DrawRectangle(pen, new Rectangle(place.X, place.Y, place.Width, place.Height));
                graphics.DrawString(
                    percent.ToString("0.##"),
                    new Font(FontFamily.GenericMonospace, 10.0f),
                    new SolidBrush(Color.Aqua),
                    place.X + place.Width / 2,
                    place.Y + place.Height / 2);
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
                    var place = Cache.Cameras[cameraIndex].Places[index];
                    place.X = Convert.ToInt32(this.xBox.Text);
                    place.Y = Convert.ToInt32(this.yBox.Text);
                    place.Width = Convert.ToInt32(this.widthBox.Text);
                    place.Height = Convert.ToInt32(this.heightBox.Text);
                }
                else
                {
                    Cache.Cameras[cameraIndex]
                         .Places.Add(
                             new Place
                             {
                                 Id = Cache.Cameras[cameraIndex].Places.Count + 1,
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
            this.listBox1.Items.Clear();
            foreach (var place in Cache.Cameras[cameraIndex].Places)
            {
                this.listBox1.Items.Add(place.ToString());
            }
        }

        private void listBox1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                var index = listBox1.SelectedIndex;
                var place = Cache.Cameras[cameraIndex].Places[index];
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
                Cache.Cameras[cameraIndex]
                     .Places.Add(
                         new Place
                         {
                             Id = Cache.Cameras[cameraIndex].Places.Count + 1,
                             X = Convert.ToInt32(this.xBox.Text),
                             Y = Convert.ToInt32(this.yBox.Text),
                             Width = Convert.ToInt32(this.widthBox.Text),
                             Height = Convert.ToInt32(this.heightBox.Text)
                         });

                RefreshPlaces();
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
            }
        }

        private void sendToServer_Click(object sender, EventArgs e)
        {
            lock (this.locker)
            {
                var placesCopy = new List<Place>(Cache.Cameras[cameraIndex].Places.Count);
                foreach (var place in Cache.Cameras[cameraIndex].Places)
                {
                    placesCopy.Add(place);
                }

                this.detectionApi.PutCameraAsync(
                        new Camera
                        {
                            Number = cameraIndex + 1,
                            Places = placesCopy
                        })
                    .Wait();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            RefreshFeedbacks();
        }

        private void RefreshFeedbacks()
        {
            var feedbacks = this.detectionApi.GetAllFeedbacksAsync().Result;
            if (Cache.Feedbacks != null && Cache.Feedbacks.Count != feedbacks.Count)
            {
                Cache.Feedbacks = feedbacks;
                listBox2.Items.Clear();
                foreach (var feedback in feedbacks)
                {
                    var title = feedback.Title;
                    if (string.IsNullOrWhiteSpace(feedback.Title))
                    {
                        title = "<NO TITLE>";
                    }

                    listBox2.Items.Add(title);
                }
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            var index = listBox2.SelectedIndex;
            if (index != -1)
            {
                var feedback = Cache.Feedbacks[index];
                MessageBox.Show(feedback.Text, feedback.Title);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != -1)
            {
                lock (locker)
                {
                    cameraIndex = comboBox1.SelectedIndex;
                    SetCamera(cameraIndex);
                }
            }
        }

        private void listBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (listBox1.SelectedIndex != -1)
                {
                    Cache.Cameras[cameraIndex].Places.RemoveAt(listBox1.SelectedIndex);
                    RefreshPlaces();
                }
            }
        }

        private bool isDrawing;
        private Point startPos;
        private Point currentPos;

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            startPos = e.Location;
            currentPos = e.Location;
            if (e.Button == MouseButtons.Left)
            {
                isDrawing = true;
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            currentPos = e.Location;
            if (isDrawing)
            {
                pictureBox1.Invalidate();
            }
        }

        private Rectangle GetRectangle()
        {
            return new Rectangle(
                Math.Min(startPos.X, currentPos.X),
                Math.Min(startPos.Y, currentPos.Y),
                Math.Abs(startPos.X - currentPos.X),
                Math.Abs(startPos.Y - currentPos.Y));
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isDrawing)
            {
                isDrawing = false;
                this.pictureBox1.Invalidate();
                var rectangle = GetRectangle();
                lock (locker)
                {
                    Cache.Cameras[cameraIndex]
                         .Places.Add(
                             new Place
                             {
                                 Id = Cache.Cameras[cameraIndex].Places.Count + 1,
                                 X = Convert.ToInt32(rectangle.X),
                                 Y = Convert.ToInt32(rectangle.Y),
                                 Width = Convert.ToInt32(rectangle.Width),
                                 Height = Convert.ToInt32(rectangle.Height)
                             });

                    RefreshPlaces();
                    listBox1.SelectedIndex = listBox1.Items.Count - 1;
                }
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (isDrawing)
            {
                var rectangle = GetRectangle();
                e.Graphics.DrawRectangle(new Pen(Color.CornflowerBlue, 2), rectangle);
            }
        }
    }
}