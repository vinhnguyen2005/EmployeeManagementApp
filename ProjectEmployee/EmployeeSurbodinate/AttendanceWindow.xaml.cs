using ProjectEmployee.Models;
using ProjectEmployee.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using FaceRecognitionDotNet;

namespace ProjectEmployee.EmployeeSurbodinate
{
    public partial class AttendanceWindow : Window
    {
        // Class member variables remain the same
        private readonly ApContext _context;
        private readonly User _currentUser;
        private readonly Employee _currentEmployee;
        private VideoCapture _capture;
        private DispatcherTimer _timer;
        private FaceRecognition _faceRecognition;
        private Image<Bgr, byte> _lastFrame;

        public AttendanceWindow(User currentUser)
        {
            InitializeComponent();
            _context = new ApContext();
            _currentUser = currentUser;
            _currentEmployee = _context.Employees.FirstOrDefault(e => e.EmployeeId == currentUser.EmployeeId);

            if (_currentEmployee == null)
            {
                MessageBox.Show("Could not find employee data. Closing window.");
                this.Close();
                return;
            }

            InitializeAiEngine();
            LoadProfilePicture();
            InitializeCamera();
            this.Closing += AttendanceWindow_Closing;
        }

        private void InitializeAiEngine()
        {
            try
            {
                _faceRecognition = FaceRecognition.Create("model");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize AI engine. Error: {ex.Message}");
                this.Close();
            }
        }

        private void InitializeCamera()
        {
            _capture = new VideoCapture(0);
            if (!_capture.IsOpened)
            {
                MessageBox.Show("Could not open camera.");
                this.Close();
                return;
            }
            _timer = new DispatcherTimer();
            _timer.Tick += Timer_Tick;
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 33);
            _timer.Start();
            txtStatus.Text = "Camera initialized. Ready to check in.";
        }

        // FIX 1: Explicitly use 'System.Drawing.Bitmap' to resolve ambiguity
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_capture == null || !_capture.IsOpened) return;
            Mat frame = new Mat();
            _capture.Read(frame);
            if (!frame.IsEmpty)
            {
                _lastFrame = frame.ToImage<Bgr, byte>();
                using (System.Drawing.Bitmap bmp = _lastFrame.ToBitmap())
                {
                    CameraImage.Source = ConvertBitmapToBitmapImage(bmp);
                }
            }
        }

        // FIX 1 (continued): Explicitly use 'System.Drawing.Bitmap' here too
        public static BitmapImage ConvertBitmapToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }

        private void LoadProfilePicture()
        {
            string imagePath = _currentEmployee.ProfilePicturePath;
            Uri imageUri;

            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                imageUri = new Uri(imagePath, UriKind.Absolute);

                if (_currentEmployee.FaceEncoding == null)
                {
                    txtStatus.Text = "Analyzing profile picture for the first time...";
                    try
                    {
                        using (var img = FaceRecognition.LoadImageFile(imagePath))
                        {
                            var encodings = _faceRecognition.FaceEncodings(img).ToList();
                            if (encodings.Any())
                            {
                                // FIX 2: Correct way to get the encoding data as byte[]
                                double[] encodingArray = encodings.First().GetRawEncoding();
                                _currentEmployee.FaceEncoding = ConvertDoubleArrayToByteArray(encodingArray);

                                _context.SaveChanges();
                                txtStatus.Text = "Profile picture analysis complete. Ready.";
                            }
                            else
                            {
                                MessageBox.Show("Could not detect a face in the profile picture.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred while analyzing the profile picture: {ex.Message}");
                    }
                }
            }
            else
            {
                imageUri = new Uri("pack://application:,,,/Image/Shinosuke.jpg");
            }

            ProfileImageBrush.ImageSource = new BitmapImage(imageUri);
        }

        private async void CheckIn_Click(object sender, RoutedEventArgs e)
        {
            btnCheckIn.IsEnabled = false;
            txtStatus.Text = "Analyzing... Please wait.";

            var frameToProcess = _lastFrame?.Clone();
            var knownEncodingData = _currentEmployee.FaceEncoding;

            if (frameToProcess == null || knownEncodingData == null)
            {
                txtStatus.Text = "Error: Camera frame or profile data is missing.";
                btnCheckIn.IsEnabled = true;
                return;
            }

            // FIX 3: Explicitly define the types for the deconstruction
            (bool isSuccess, double similarity) = await Task.Run(() =>
            {
                try
                {
                    // FIX 2 (continued): Correct way to load the encoding from byte[]
                    double[] knownEncodingArray = ConvertByteArrayToDoubleArray(knownEncodingData);
                    using (var knownEncoding = FaceRecognition.LoadFaceEncoding(knownEncodingArray))
                    using (var unknownImage = FaceRecognition.LoadImage(frameToProcess.ToBitmap()))
                    {
                        var unknownEncodings = _faceRecognition.FaceEncodings(unknownImage).ToList();

                        if (unknownEncodings.Any())
                        {
                            var unknownEncoding = unknownEncodings.First();
                            bool match = FaceRecognition.CompareFace(knownEncoding, unknownEncoding, 0.6);
                            double distance = FaceRecognition.FaceDistance(knownEncoding, unknownEncoding);
                            double sim = (1 - distance) * 100;
                            return (match, sim);
                        }
                    }
                }
                catch (Exception) { /* Handle or log error if needed */ }

                return (false, 0.0);
            });

            if (isSuccess)
            {
                txtStatus.Text = $"Check-in successful! Similarity: {similarity:F1}%";
                LogAttendance("Success", similarity);
                AuditLogger.Log("Attendance Check-in", _currentUser, $"Check-in successful with similarity score: {similarity:F1}%.");
            }
            else
            {
                txtStatus.Text = "Face not recognized. Please try again.";
                LogAttendance("Failed", similarity);
                AuditLogger.Log("Attendance Check-in Failed", _currentUser, "Face not recognized during check-in attempt.");
            }

            btnCheckIn.IsEnabled = true;
        }

        private void LogAttendance(string status, double similarity)
        {
            var log = new AttendanceLog
            {
                EmployeeId = _currentEmployee.EmployeeId,
                CheckInTime = DateTime.Now,
                Status = status,
                SimilarityScore = similarity
            };
            _context.AttendanceLogs.Add(log);
            _context.SaveChanges();
        }

        // Helper methods to convert between double[] and byte[]
        private byte[] ConvertDoubleArrayToByteArray(double[] doubleArray)
        {
            var byteArray = new byte[doubleArray.Length * sizeof(double)];
            Buffer.BlockCopy(doubleArray, 0, byteArray, 0, byteArray.Length);
            return byteArray;
        }

        private double[] ConvertByteArrayToDoubleArray(byte[] byteArray)
        {
            var doubleArray = new double[byteArray.Length / sizeof(double)];
            Buffer.BlockCopy(byteArray, 0, doubleArray, 0, byteArray.Length);
            return doubleArray;
        }

        private void AttendanceWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _timer?.Stop();
            _capture?.Dispose();
            _faceRecognition?.Dispose();
        }
    }
}