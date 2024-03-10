using System.Collections.Immutable;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfAnimatedGif;

namespace MultiWindows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static PointF GlobalCenter { get; set; }

        public DebugWindow DebugWindow { get; set; }

        public static List<MainWindow> Windows { get; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        private ImageSource _imageS;
        public ImageSource ImageS { get => _imageS; set { _imageS = value; PropertyChanged?.Invoke(this, new(nameof(ImageS))); } }

        public static int Counter = 1;

        public static System.Timers.Timer Timer { get; set; }

        public static ImmutableArray<BitmapImage> Frames { get; set; }

        public static float GlobalWidth;
        public static float GlobalHeight;
        public static float HalfWidth;
        public static float HalfHeight;

        public delegate void ChangePosHandler();

        public MainWindow()
        {
            InitializeComponent();

            if (Frames == null)
            {
                Frames = Enumerable.Range(1, 61).Select(x => new BitmapImage(new Uri($"pack://application:,,,/Frames/F ({x}).gif"))).ToImmutableArray();
            }

            if (GlobalCenter.IsEmpty)
            {
                var centerX = (float)System.Windows.SystemParameters.PrimaryScreenWidth / 2f;
                var centerY = (float)System.Windows.SystemParameters.PrimaryScreenHeight / 2f;
                GlobalCenter = new PointF(centerX, centerY);
            }

            Windows.Add(this);

            if (Timer == null)
            {
                Timer = new System.Timers.Timer(25);

                Timer.Elapsed += (sender, e) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        for (int i = 0; i < Windows.Count; i++)
                        {
                            var window = Windows[i];
                            window.ImageS = Frames[Counter - 1];
                        }

                        if (Counter == 61)
                            Counter = 1;

                        Counter++;
                    });
                };

                Timer.Start();
            }

            GlobalWidth = (float)Frames[0].Width;
            GlobalHeight = (float)Frames[0].Height;
            HalfWidth = (float)Frames[0].Width / 2;
            HalfHeight = (float)Frames[0].Height / 2;
        }


        public void ChangePos()
        {
            var localX = GlobalCenter.X - Left - HalfWidth;
            var localY = GlobalCenter.Y - Top - HalfHeight;

            //  this.ImagePlace.RenderTransform = new TranslateTransform(localX, localY);
            Canvas.SetLeft(this.ImagePlace, localX);
            Canvas.SetTop(this.ImagePlace, localY);
            Canvas.SetRight(this.ImagePlace, this.Width - GlobalWidth - localX);
            Canvas.SetBottom(this.ImagePlace, this.Height - GlobalHeight - localY);

        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            ChangePos();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ChangePos();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var window = new MainWindow();
            Windows.Add(window);
            window.Show();
            window.ChangePos();
        }
    }
}