using HandyControl.Controls;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.MessageBox;

namespace BlankApp1.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Screenshot.Snapped += Screenshot_Snapped;
            this.Closed+= (s, e) =>
            {
                Screenshot.Snapped -= Screenshot_Snapped;
            };
        }

        private void Screenshot_Snapped(object sender, HandyControl.Data.FunctionEventArgs<ImageSource> e)
        {
            ImageSource imageSource = e.Info;
            
            var image = Image.Load<Rgba32>(ImageSourceToBytes(imageSource));
            //using var image = Image.Load("Images/3.png");
            var font = SixLabors.Fonts.SystemFonts.CreateFont("Arial", 56, SixLabors.Fonts.FontStyle.Bold);
            var textOptions = new SixLabors.Fonts.TextOptions(font)
            {
                Origin = new SixLabors.ImageSharp.Point(image.Width - 400, image.Height - 100), // 右下角
                HorizontalAlignment = SixLabors.Fonts.HorizontalAlignment.Right,
                VerticalAlignment = SixLabors.Fonts.VerticalAlignment.Bottom
            };
            image.Mutate(ctx => ctx.DrawText(
                "textOptions",
                font,
                 SixLabors.ImageSharp.Color.Black, new SixLabors.ImageSharp.Point(image.Width - 400, image.Height - 100)
                ));
            image.Save("Images/4.png");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new Screenshot().Start();



        }
        public byte[] ImageSourceToBytes(ImageSource imageSource)
        {
            byte[] bytes = null;
            var bitmapSource = imageSource as BitmapSource;
            if (bitmapSource != null)
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    bytes = stream.ToArray();
                }
            }
            return bytes;
        }
        private  Channel<string> channelVideoAnalysis;
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
          
           await channelVideoAnalysis.Writer.WriteAsync("test");
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            channelVideoAnalysis = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = true,
                SingleReader = true,
                SingleWriter = false
            });
            await foreach (var item in channelVideoAnalysis.Reader.ReadAllAsync().ConfigureAwait(false))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("11");
                });
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            channelVideoAnalysis.Writer.Complete();
            
            
        }
    }
}
