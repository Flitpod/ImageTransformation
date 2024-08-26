using App_wpf;
using App_wpf.TransformationControls;
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
        Controls transformationControls;

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
                label_MetaData.Content = $"{openFileDialog.SafeFileName} - {bitmapSrc.Width} x {bitmapSrc.Height} ";
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

        private void ExecuteRotateTransformation()
        {
            if (bitmapSrc == null) return;
            RotateControls controls = (transformationControls as RotateControls);
            Core.Matrix transformation = Transformations.Rotation(controls.Slider.Value, bitmapSrc.Height / 2.0, bitmapSrc.Width / 2.0);
            if (controls.Backward.IsChecked == false)
            {
                TransformBitmap.ExecuteForward(bitmapSrc, ref bitmapDst, transformation);
            }
            else
            {
                TransformBitmap.ExecuteBackward(bitmapSrc, ref bitmapDst, transformation, (InterpolationTypes)controls.IntepolationType.SelectedIndex);
            }
            RefreshImages();
        }

        private void RefreshImages()
        {
            // result image
            imageResult.Source = bitmapDst.GetBitmapSource();
        }

        private void click_TransformationRotate(object sender, RoutedEventArgs e)
        {
            if (transformationControls != null) return;

            // create control elements in a stackpanel
            RotateControls rotateControls = new RotateControls();

            // create eventhandlers for slider, combobox, checkbox valuechanged event
            rotateControls.Slider.ValueChanged += (s,e) => ExecuteRotateTransformation();
            rotateControls.IntepolationType.SelectionChanged += (s, e) => ExecuteRotateTransformation();
            rotateControls.Backward.Click += (s, e) => ExecuteRotateTransformation();

            // event handler for controls close button click
            rotateControls.CloseBtn.Click += (sender, e) =>
            {
                this.transformationControls = null;
                controlsGrid.Children.Clear();   
            };

            // add the stackpanel to the grid
            this.transformationControls = rotateControls;
            this.controlsGrid.Children.Add(rotateControls.Controls);
        }
    }
}