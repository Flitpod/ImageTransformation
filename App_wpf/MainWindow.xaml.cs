using App_wpf;
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
                this.bitmapSrc = new Bitmap(openFileDialog.FileName);
                this.bitmapDst = new Bitmap(this.bitmapSrc);
                this.imageSource.Source = this.bitmapSrc.GetBitmapSource();
                this.imageResult.Source = this.bitmapDst.GetBitmapSource();
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
            // this.transformation = Transformations.Rotation(this.sliderRotation.Value);
            // TransformBitmap.ExecuteForward(this.bitmapSrc, ref this.bitmapDst, this.transformation);
            //TransformBitmap.ExecuteBackward(this.bitmapSrc, ref this.bitmapDst, this.transformation, InterpolationTypes.Floating_FromSource, weightCloserNeighbour: 0.25);
            Core.Matrix transformation = Transformations.Rotation(this.sliderRotation.Value, this.bitmapSrc.Height / 2.0, this.bitmapSrc.Width / 2.0);
            TransformBitmap.ExecuteBackward_FloatingInterpolation(bitmapSrc, ref bitmapDst, transformation);
            RefreshImages();
        }

        private void RefreshImages()
        {
            // result image
            this.imageResult.Source = this.bitmapDst.GetBitmapSource();
        }
    }
}