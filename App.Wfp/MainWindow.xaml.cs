using Core;
using ImageTransformation.Core;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageTransformation.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // fields
        Bitmap bitmapSrc;
        Bitmap bitmapDst;
        Core.Matrix transformation;

        // ctor
        public MainWindow()
        {
            InitializeComponent();
        }

        // methods


        // event handlers
        private void click_OpenFileDialog(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                this.imageSource.Source = new BitmapImage(new Uri("result.png", UriKind.RelativeOrAbsolute));
                this.bitmapSrc = new Bitmap(openFileDialog.FileName);
                this.bitmapDst = new Bitmap("result.png");
                this.imageSource.Source = Imaging.CreateBitmapSourceFromHBitmap(this.bitmapSrc.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                this.imageResult.Source = Imaging.CreateBitmapSourceFromHBitmap(this.bitmapDst.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
        }

        private void click_SaveFileDialog(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                this.bitmapDst.Save(saveFileDialog.FileName);
            }
        }

        private void sliderRotation_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bitmapSrc == null) return;
            this.transformation = Transformation.Rotation(this.sliderRotation.Value);
            TransformBitmap.ExecuteForward(this.bitmapSrc, ref this.bitmapDst, this.transformation);
            RefreshImages();
            ;
        }

        private void RefreshImages()
        {
            // result image
            // this.imageResult.Source = Imaging.CreateBitmapSourceFromHBitmap(this.bitmapDst.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            this.bitmapDst.Save("result.png", ImageFormat.Png);
            this.imageResult.Source = new BitmapImage(new Uri("result.png", UriKind.RelativeOrAbsolute));
        }
    }
}