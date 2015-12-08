//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.DepthBasics
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Map depth range to byte range
        /// </summary>
        private const int MapDepthToByte = 1;

        private double theUntouchables = 0;

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Reader for depth frames
        /// </summary>
        private DepthFrameReader depthFrameReader = null;

        /// <summary>
        /// Description of the data contained in the depth frame
        /// </summary>
        private FrameDescription depthFrameDescription = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap depthBitmap = null;

        /// <summary>
        /// Intermediate storage for frame data converted to color
        /// </summary>
        private byte[] depthPixels = null;
        private ushort[] depthStuff = null;

        /// <summary>
        /// Current status text to display
        /// </summary>
        private string statusText = null;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            // get the kinectSensor object
            this.kinectSensor = KinectSensor.GetDefault();

            // open the reader for the depth frames
            this.depthFrameReader = this.kinectSensor.DepthFrameSource.OpenReader();

            // wire handler for frame arrival
            this.depthFrameReader.FrameArrived += this.Reader_FrameArrived;

            // get FrameDescription from DepthFrameSource
            this.depthFrameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;

            // allocate space to put the pixels being received and converted
            this.depthPixels = new byte[this.depthFrameDescription.Width * this.depthFrameDescription.Height];
            this.depthStuff = new ushort[this.depthFrameDescription.Width * this.depthFrameDescription.Height];

            // create the bitmap to display
            this.depthBitmap = new WriteableBitmap(this.depthFrameDescription.Width, this.depthFrameDescription.Height, 96.0, 96.0, PixelFormats.Gray8, null);

            // set IsAvailableChanged event notifier
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            // open the sensor
            this.kinectSensor.Open();

            // set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.NoSensorStatusText;

            // use the window object as the view model in this simple example
            this.DataContext = this;

            // initialize the components (controls) of the window
            this.InitializeComponent();
        }

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource ImageSource
        {
            get
            {
                return this.depthBitmap;
            }
        }

        /// <summary>
        /// Gets or sets the current status text to display
        /// </summary>
        public string StatusText
        {
            get
            {
                return this.statusText;
            }

            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;

                    // notify any bound elements that the text has changed
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.depthFrameReader != null)
            {
                // DepthFrameReader is IDisposable
                this.depthFrameReader.Dispose();
                this.depthFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }

        /// <summary>
        /// Handles the user clicking on the screenshot button
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void ScreenshotButton_Click(object sender, RoutedEventArgs e)
        {

            string time = System.DateTime.UtcNow.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

            try
            {
                double[,] pixels = new double[this.depthFrameDescription.Width, this.depthFrameDescription.Height];
                double[,] correctionMatrix = new double[this.depthFrameDescription.Width, this.depthFrameDescription.Height];

                int i = 0;
                int j = 0;
                int k = depthPixels.Length - 1;

                for (i = this.depthFrameDescription.Height - 1; i >= 0; i--)
                {
                    for (j = 0; j < this.depthFrameDescription.Width; j++)
                    {
                        pixels[j, i] = this.depthStuff[k];
                        double pix = pixels[j, i];

                        if (depthStuff[k] == 0)
                        {
                            theUntouchables++;
                        }

                        k--;
                    }
                }

                int c, d;

                //Correct for the x-axis
                for (c = 0; c < depthFrameDescription.Width; c++)
                {
                    for (d = 0; d < depthFrameDescription.Height; d++)
                    {
                        /*
                        f(x) = p1*x^5 + p2*x^4 + p3*x^3 + p4*x^2 + p5*x + p6
                        Coefficients (with 95% confidence bounds):
                        p1 =   5.237e-12  (3.014e-12, 7.459e-12)
                        p2 =  -7.065e-09  (-9.924e-09, -4.207e-09)
                        p3 =   3.218e-06  (1.89e-06, 4.545e-06)
                        p4 =  -0.0004947  (-0.0007625, -0.000227)
                        p5 =    -0.03215  (-0.05422, -0.01009)
                        p6 =       9.665  (9.107, 10.22)
                        */

                        double correction = 5.237e-12 * Math.Pow(c, 5) +
                            -7.065e-09 * Math.Pow(c, 4) +
                            3.218e-06 * Math.Pow(c, 3) + 
                            (-0.0004947) * Math.Pow(c, 2) + 
                            (-0.03215) * c +
                            9.665;

                        correctionMatrix[c, d] = correction;
                        double pix = pixels[c, d];
                    }
                }

                //Correct for the y-axis
                for (d = 0; d < depthFrameDescription.Height; d++)
                {
                    for (c = 0; c < depthFrameDescription.Width; c++)
                    {
                        /*
                        Linear model Poly4:
                        f4(x) = p1 * x ^ 4 + p2 * x ^ 3 + p3 * x ^ 2 + p4 * x + p5
                        Coefficients(with 95 % confidence bounds):
                        p1 = 2.085e-09(1.394e-09, 2.776e-09)
                        p2 = -2.043e-06(-2.756e-06, -1.33e-06)
                        p3 = 0.000717(0.0004741, 0.0009599)
                        p4 = -0.1262(-0.1567, -0.09566)
                        p5 = 9.532(8.406, 10.66)
                        */
                        double correction = 2.085e-09 * Math.Pow(d, 4) +
                            -2.043e-06 * Math.Pow(d, 3) +
                            0.000717 * Math.Pow(d, 2) +
                            -0.1262 * Math.Pow(d, 1) +
                            9.532;

                        double avg = correctionMatrix[c, d];
                        double pix = pixels[c, d];

                        avg += correction;
                        avg /= 2;

                        correctionMatrix[c, d] = correction;
                    }
                }

                //Apply the correction matrix to the depth data
                for (c = 0; c < depthFrameDescription.Width; c++)
                {
                    for (d = 0; d < depthFrameDescription.Height; d++)
                    {
                        double abc = pixels[c, d];
                        abc += correctionMatrix[c, d];
                        if (abc < 0) { abc = 0; }
                        pixels[c, d] = abc;
                    }

                }

                //Create the .CSV file
                double csvValue = 0.0;
                using (StreamWriter outfile = new StreamWriter("C:\\KinectData\\KinectScreenshot-Depth-" + time + "-Output.csv"))
                {
                    for (int t = 0; t < this.depthFrameDescription.Height; t++)
                    {
                        string content = "";
                        for (int s = this.depthFrameDescription.Width - 1; s >= 0; s--)
                        {
                            csvValue = (pixels[s, t]);
                            content += csvValue + ",";
                        }
                        outfile.WriteLine(content);
                    }
                }

                //Create the information text file
                double kinectX = (pixels[depthFrameDescription.Width/2, depthFrameDescription.Height/2] + 
                    pixels[(depthFrameDescription.Width/2)+1, depthFrameDescription.Height/2] + 
                    pixels[depthFrameDescription.Width/2, (depthFrameDescription.Height/2)+1] + 
                    pixels[(depthFrameDescription.Width/2)+1, (depthFrameDescription.Height/2)+1]) / 4;

                using (StreamWriter file = new StreamWriter("C:\\KinectData\\KinectScreenshot-Depth-" + time + "-Output.txt"))
                {

                    file.Write("Fraction unreliable: " + theUntouchables + " / " + (depthFrameDescription.Height * depthFrameDescription.Width) + "\n");
                    file.Write("Percentage unreliable: " + (theUntouchables / (depthFrameDescription.Height * depthFrameDescription.Width)) * 100 + "%\n");
                    file.Write("Center distance: " + kinectX + "\n");
                    file.Write("\n");
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.ToString());
            }

            if (this.depthBitmap != null)
            {
                // create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoder = new PngBitmapEncoder();

                // create frame from the writable bitmap and add to encoder
                encoder.Frames.Add(BitmapFrame.Create(this.depthBitmap));

                string path = "C:\\KinectData\\KinectScreenshot-Depth-" + time + "-Image.png";

                // write the new file to disk
                try
                {
                    // FileStream is IDisposable
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        encoder.Save(fs);
                    }

                    this.StatusText = string.Format(CultureInfo.CurrentCulture, Properties.Resources.SavedScreenshotStatusTextFormat, path);
                }
                catch (IOException)
                {
                    this.StatusText = string.Format(CultureInfo.CurrentCulture, Properties.Resources.FailedScreenshotStatusTextFormat, path);
                }
            }
        }

        /// <summary>
        /// Handles the depth frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            bool depthFrameProcessed = false;

            using (DepthFrame depthFrame = e.FrameReference.AcquireFrame())
            {
                if (depthFrame != null)
                {
                    depthFrame.CopyFrameDataToArray(depthStuff);
                    // the fastest way to process the body index data is to directly access 
                    // the underlying buffer
                    using (Microsoft.Kinect.KinectBuffer depthBuffer = depthFrame.LockImageBuffer())
                    {
                        // verify data and write the color data to the display bitmap
                        if (((this.depthFrameDescription.Width * this.depthFrameDescription.Height) == (depthBuffer.Size / this.depthFrameDescription.BytesPerPixel)) &&
                            (this.depthFrameDescription.Width == this.depthBitmap.PixelWidth) && (this.depthFrameDescription.Height == this.depthBitmap.PixelHeight))
                        {
                            // Note: In order to see the full range of depth (including the less reliable far field depth)
                            // we are setting maxDepth to the extreme potential depth threshold
                            ushort maxDepth = ushort.MaxValue;

                            // If you wish to filter by reliable depth distance, uncomment the following line:
                            //// maxDepth = depthFrame.DepthMaxReliableDistance

                            this.ProcessDepthFrameData(depthBuffer.UnderlyingBuffer, depthBuffer.Size, depthFrame.DepthMinReliableDistance, maxDepth);
                            depthFrameProcessed = true;
                        }
                    }
                }
            }

            if (depthFrameProcessed)
            {
                this.RenderDepthPixels();
            }
        }

        /// <summary>
        /// Directly accesses the underlying image buffer of the DepthFrame to 
        /// create a displayable bitmap.
        /// This function requires the /unsafe compiler option as we make use of direct
        /// access to the native memory pointed to by the depthFrameData pointer.
        /// </summary>
        /// <param name="depthFrameData">Pointer to the DepthFrame image data</param>
        /// <param name="depthFrameDataSize">Size of the DepthFrame image data</param>
        /// <param name="minDepth">The minimum reliable depth value for the frame</param>
        /// <param name="maxDepth">The maximum reliable depth value for the frame</param>
        private unsafe void ProcessDepthFrameData(IntPtr depthFrameData, uint depthFrameDataSize, ushort minDepth, ushort maxDepth)
        {
            // depth frame data is a 16 bit value
            ushort* frameData = (ushort*)depthFrameData;

            // convert depth to a visual representation
            for (int i = 0; i < (int)(depthFrameDataSize / this.depthFrameDescription.BytesPerPixel); ++i)
            {
                // Get the depth for this pixel
                ushort depth = frameData[i];

                // To convert to a byte, we're mapping the depth value to the byte range.
                // Values outside the reliable depth range are mapped to 0 (black).
                this.depthPixels[i] = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByte) : 0);
            }
        }

        /// <summary>
        /// Renders color pixels into the writeableBitmap.
        /// </summary>
        private void RenderDepthPixels()
        {
            this.depthBitmap.WritePixels(
                new Int32Rect(0, 0, this.depthBitmap.PixelWidth, this.depthBitmap.PixelHeight),
                this.depthPixels,
                this.depthBitmap.PixelWidth,
                0);
        }

        /// <summary>
        /// Handles the event which the sensor becomes unavailable (E.g. paused, closed, unplugged).
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            // on failure, set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.SensorNotAvailableStatusText;
        }
    }
}
