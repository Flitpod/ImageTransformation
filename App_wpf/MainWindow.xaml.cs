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
        TransformationControls transformationControls;

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
                this.Title = $"PhotoshApp  |  {openFileDialog.SafeFileName} - (width/col = {bitmapSrc.Width}) x (height/row = {bitmapSrc.Height})";
            }
        }

        private void click_SaveFileDialog(object sender, EventArgs e)
        {
            if (this.bitmapDst == null) return;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                this.bitmapDst.Save(saveFileDialog.FileName);
            }
        }
        private void click_CloseFile(object sender, RoutedEventArgs e)
        {
            this.Title = "PhotoshApp";
            this.transformationControls = null;
            this.controlsGrid.Children.Clear();
            this.bitmapDst = null;
            this.bitmapSrc = null;
            this.imageResult.Source = null;
            this.imageSource.Source = null;
        }
        private void RefreshImages()
        {
            // result image
            imageResult.Source = bitmapDst.GetBitmapSource();
        }

        // create rotate transformation toolbar
        private void click_TransformationRotate(object sender, RoutedEventArgs e)
        {
            if (IsToolbarControlOpen()) return;

            // create control elements in a stackpanel
            RotateControls rotateControls = new RotateControls();

            // create eventhandlers for slider, combobox, checkbox valuechanged event
            rotateControls.Slider.ValueChanged += (s, e) => ExecuteRotateTransformation();
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

        // create general transformation toolbar
        private void click_TransformationGeneral(object sender, RoutedEventArgs e)
        {
            if (IsToolbarControlOpen()) return;

            // create control elements in a stackpanel
            GeneralMatrixControls generalMatrixControls = new GeneralMatrixControls(matrix_size: 3);

            // event handler for controls close button click
            generalMatrixControls.CloseBtn.Click += (sender, e) =>
            {
                this.transformationControls = null;
                controlsGrid.Children.Clear();
            };

            generalMatrixControls.Backward.Click += (s, e) => ExecuteGeneralTransformation();

            // event handler for controls execute button click
            generalMatrixControls.ExecuteBtn.Click += (s, e) => ExecuteGeneralTransformation();

            // add the stackpanel to the grid
            this.transformationControls = generalMatrixControls;
            this.controlsGrid.Children.Add(generalMatrixControls.Controls);
        }

        // create projective rectification transformation toolbar and set canvas to it
        private void click_TransformationProjectiveRectification(object sender, RoutedEventArgs e)
        {
            if (IsToolbarControlOpen() || this.imageSource.Source == null) return;

            // create control elements for the control grid
            ProjectiveTransformationControls projTransfControls = new ProjectiveTransformationControls(this.canvasSource, this.imageSource);

            // event handler for controls close button click
            projTransfControls.CloseBtn.Click += (s, e) =>
            {
                projTransfControls.Detach_Canvas();
                this.transformationControls = null;
                this.controlsGrid.Children.Clear();
            };

            projTransfControls.ExecuteBtn.Click += (s, e) =>
            {
                ExecuteProjectiveRectificationTransformation();
            };

            //// create control elements for the control grid
            //ProjRectControls projRectControls = new ProjRectControls(this.canvasSource, this.imageSource);

            //// event handler for controls close button click
            //projRectControls.CloseBtn.Click += (s, e) =>
            //{
            //    projRectControls.Detach();
            //    this.transformationControls = null;
            //    this.controlsGrid.Children.Clear();
            //};

            //projRectControls.ExecuteBtn.Click += (s, e) =>
            //{
            //    ExecuteProjectiveRectificationTransformation();
            //};

            this.transformationControls = projTransfControls;
            this.controlsGrid.Children.Add(projTransfControls.Controls);
        }

        private void click_FilterCanny(object sender, RoutedEventArgs e)
        {
            if (imageSource.Source == null)
            {
                return;
            }
            Filters.ApplyCanny(source: bitmapSrc, destination: ref bitmapDst);
            RefreshImages();
        }

        // get transformation matrix for rotate transformation
        private void ExecuteRotateTransformation()
        {
            if (bitmapSrc == null) return;
            RotateControls controls = (transformationControls as RotateControls);
            Core.Matrix transformation = Transformations.Rotation(controls.Slider.Value, bitmapSrc.Height / 2.0, bitmapSrc.Width / 2.0, Dimension.D3);

            ExecuteTransformation(transformation, (InterpolationTypes)controls.IntepolationType.SelectedIndex);
        }

        // get transformation matrix for general transformation
        private void ExecuteGeneralTransformation()
        {
            if (bitmapSrc == null) return;
            GeneralMatrixControls controls = (transformationControls as GeneralMatrixControls);
            Core.Matrix transformation = controls.GetTransformation();

            ExecuteTransformation(transformation);
        }

        // get transformation matrix for projective rectification
        private void ExecuteProjectiveRectificationTransformation()
        {
            if (bitmapSrc == null) return;
            ProjectiveTransformationControls controls = (transformationControls as ProjectiveTransformationControls);
            Core.Matrix transformation = controls.GetTransformation();

            ExecuteTransformation(transformation);
        }

        // execute transformation
        private void ExecuteTransformation(Core.Matrix transformation, InterpolationTypes interpolationType = InterpolationTypes.Floating_FromSource)
        {
            try
            {
                if (transformationControls.Backward.IsChecked == false)
                {
                    TransformBitmap.ExecuteForward(bitmapSrc, ref bitmapDst, transformation);
                }
                else
                {
                    TransformBitmap.ExecuteBackward(bitmapSrc, ref bitmapDst, transformation, InterpolationTypes.Floating_FromSource);
                }
                RefreshImages();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // helper method for avoid open controls onto each other
        private bool IsToolbarControlOpen()
        {
            return transformationControls != null;
        }

    }
}