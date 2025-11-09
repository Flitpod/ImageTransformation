using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Core;
using Core.Detection;
using ImageTransformation.Core;
using Point = System.Windows.Point;

namespace App_wpf.TransformationControls
{
    internal class WrapPoint
    {
        public WrapPoint(Point point)
        {
            X = point.X;
            Y = point.Y;
        }

        public double X { get; set; }
        public double Y { get; set; }

        public Point GetPoint()
        {
            return new Point(X, Y);
        }
    }

    public class ProjectiveTransformationControls : TransformationControls, IDisposable
    {
        private readonly Canvas _canvas;
        private readonly System.Windows.Controls.Image _image;
        private System.Drawing.Bitmap _bitmapSource;
        private TextBlock _textBlock_MousePosition;
        private readonly List<WrapPoint> _points_relative;
        private WrapPoint _dragged_point;
        private bool _isDragging;
        private bool _pointSelected;
        public Button ExecuteBtn { get; private set; }
        private Button _detectCornerButton;

        // Constants
        private const double _POINT_SELECTION_THRESHOLD = 10.0 / 400.0;
        private const double _ELLIPSE_SIZE = 4.0;

        public ProjectiveTransformationControls(Canvas canvas, System.Windows.Controls.Image image, System.Drawing.Bitmap bitmapSource)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
            _image = image ?? throw new ArgumentNullException(nameof(image));
            _bitmapSource = bitmapSource ?? throw new ArgumentNullException(nameof(bitmapSource));
            _points_relative = InitializePoints();

            CreateLayout();
            InitializeCanvas();
        }

        private List<WrapPoint> InitializePoints() => new List<WrapPoint>
        {
            new WrapPoint(new Point(0, 0)),
            new WrapPoint(new Point(1, 0)),
            new WrapPoint(new Point(1, 1)),
            new WrapPoint(new Point(0, 1)),
        };

        private void InitializeCanvas()
        {
            _canvas.MouseDown += Handler_MouseDown;
            _canvas.MouseMove += Handler_MouseMove;
            _canvas.MouseUp += Handler_MouseUp;
            _canvas.SizeChanged += Handler_SizeChanged;
        }

        // init methods
        private void CreateLayout()
        {
            // create main grid
            Grid mainGrid = new Grid();
            mainGrid.MaxHeight = 25;
            mainGrid.Height = 25;
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            this.Controls = mainGrid;

            // create mouse move textblock
            TextBlock textBlock = new TextBlock();
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Margin = new Thickness(5, 0, 5, 0);
            textBlock.Width = 100;
            Grid.SetColumn(textBlock, 0);
            this._textBlock_MousePosition = textBlock;
            mainGrid.Children.Add(textBlock);

            // create backward checkbox
            CheckBox checkBox = new CheckBox();
            checkBox.Content = "Backward";
            checkBox.IsChecked = true;
            Grid.SetColumn(checkBox, 1);
            checkBox.HorizontalAlignment = HorizontalAlignment.Center;
            checkBox.VerticalAlignment = VerticalAlignment.Center;
            checkBox.Margin = new Thickness(5, 0, 5, 0);
            mainGrid.Children.Add(checkBox);
            this.Backward = checkBox;

            // create execute button
            Button executeButton = new Button();
            executeButton.Content = "Execute";
            Grid.SetColumn(executeButton, 2);
            executeButton.HorizontalAlignment = HorizontalAlignment.Center;
            executeButton.VerticalAlignment = VerticalAlignment.Center;
            mainGrid.Children.Add(executeButton);
            this.ExecuteBtn = executeButton;

            // create detect corners button
            Button detectCornerButton = new Button();
            detectCornerButton.Content = "Detect Corners";
            Grid.SetColumn(detectCornerButton, 3);
            detectCornerButton.HorizontalAlignment = HorizontalAlignment.Center;
            detectCornerButton.VerticalAlignment = VerticalAlignment.Center;
            mainGrid.Children.Add(detectCornerButton);
            detectCornerButton.Click += DetectCornerButton_Click;
            this._detectCornerButton = detectCornerButton;

            // create close button
            Button closeButton = new Button();
            closeButton.Content = "Close";
            closeButton.Margin = new Thickness(0, 0, 2, 0);
            Grid.SetColumn(closeButton, 5);
            mainGrid.Children.Add(closeButton);
            this.CloseBtn = closeButton;
        }


        // event handlers
        private void Handler_MouseDown(object sender, MouseEventArgs mouseEventArgs)
        {
            ResetDragState();
            CheckIsDragging(mouseEventArgs);

            if (_pointSelected)
            {
                UpdateDraggedPoint(mouseEventArgs);
                DrawWeb();
            }
        }
        private void Handler_MouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            UpdateMousePositionText(mouseEventArgs);

            if (_isDragging && _pointSelected && mouseEventArgs.LeftButton == MouseButtonState.Pressed)
            {
                UpdateDraggedPoint(mouseEventArgs);
                DrawWeb();
            }
        }
        private void Handler_MouseUp(object sender, MouseEventArgs mouseEventArgs)
        {
            if (_pointSelected)
            {
                UpdateDraggedPoint(mouseEventArgs);
                DrawWeb();
            }
            ResetDragState();
        }
        private void Handler_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawWeb();
        }
        private void DetectCornerButton_Click(object sender, RoutedEventArgs e)
        {
            // find corners on the source image
            bool cornersFound = QuadrangleDetector.FindCorners(
                source: this._bitmapSource,
                corners: out IEnumerable<System.Drawing.Point> rawCorners
            );

            // check if there are 4 found corner points
            if (!cornersFound)
            {
                MessageBox.Show(
                    messageBoxText: "No detected quadrengle!",
                    caption: "Warning!",
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Warning
                );
                return;
            }

            // bitmap image sizes for normalization
            int height = this._bitmapSource.Height;
            int width = this._bitmapSource.Width;

            // find the matching points (topleft, topright, bottomright, bottomleft)
            var cornersQuadrangle = rawCorners
                .Select(p => new Point(x: p.X, y: p.Y))
                .Select(p => GetNormalizedPosition(point: p, width: width, height: height));
            var cornersImage = new Point[]
            {
                new Point(x: 0, y: 0),
                new Point(x: 1, y: 0),
                new Point(x: 1, y: 1),
                new Point(x: 0, y: 1),
            };

            // calculate the min distances from the image corner points and save in the the given order
            this._points_relative.Clear();
            this._pointSelected = false;
            this._dragged_point = null;

            foreach (var imageCornerPoint in cornersImage)
            {
                Dictionary<double, Point> distancesForPoints = new Dictionary<double, Point>();
                foreach (var quadrangleCornerPoint in cornersQuadrangle)
                {
                    // build up the map of distances relative to the current image corner point
                    distancesForPoints.Add(
                        key: GetDistance(p1: imageCornerPoint, p2: quadrangleCornerPoint),
                        value: quadrangleCornerPoint
                    );
                }

                // find the minimum distance
                double minDistance = distancesForPoints.Keys.Min();

                // add the found matching point to the points_relative collection
                WrapPoint matchingPoint = new WrapPoint(distancesForPoints[minDistance]);
                this._points_relative.Add(matchingPoint);
            }

            // redraw the web
            DrawWeb();
        }

        private void ResetDragState()
        {
            _pointSelected = false;
            _isDragging = false;
            _dragged_point = null;
        }

        private void UpdateMousePositionText(MouseEventArgs mouseEventArgs)
        {
            var point = MousePositionOverImage(mouseEventArgs);
            _textBlock_MousePosition.Text = $"col: {Math.Round(point.X)}, row: {Math.Round(point.Y)}";
        }

        // get transformation method
        public ImageTransformation.Core.Matrix GetTransformation()
        {
            // based on actual positions of children
            double imageWidth = _image.Source.Width; // x dir
            double imageHeight = _image.Source.Height; // y dir

            // create the point pairs for transformation calculations
            // topleft, topright, bottomright, bottomleft
            var pointPairs = new List<Tuple<Tuple<double, double>, Tuple<double, double>>>();
            var cornerPoints = new Point[]
            {
                new Point(x: 0, y: 0),
                new Point(x: 1, y: 0),
                new Point(x: 1, y: 1),
                new Point(x: 0, y: 1),
            };
            var actualPoints = _points_relative.ToArray();

            // calculate the ratio true relative point pairs
            for (int i = 0; i < 4; i++)
            {
                var p_corner = cornerPoints[i];
                var p_actual = actualPoints[i];

                // for transformation: [((p1x, p1y), (p1x', p1y')), ...], but for windows media Point it is fliped, Point.X -> col = py, Point.Y -> row = px
                var tuple_orig = new Tuple<double, double>(p_corner.Y * imageHeight, p_corner.X * imageWidth);
                var tuple_act = new Tuple<double, double>(p_actual.Y * imageHeight, p_actual.X * imageWidth);
                var tuple_pair = new Tuple<Tuple<double, double>, Tuple<double, double>>(tuple_orig, tuple_act);
                pointPairs.Add(tuple_pair);
            }

            // calculate the transformation
            ImageTransformation.Core.Matrix transformation = Transformations.GetProjectionMatrix(pointPairs);

            return transformation.GetInverse();
        }

        // methods for interaction with canvas
        private void DrawWeb()
        {
            _canvas.Children.Clear();

            var (offsetW, offsetH) = CalculateOffsets();
            var polygon = CreatePolygon(offsetW, offsetH);

            foreach (var point in _points_relative)
            {
                var (col, row) = GetAbsolutePosition(point);
                AddPointEllipse(offsetW, offsetH, col, row);
                polygon.Points.Add(new Point(col, row));
            }

            _canvas.Children.Add(polygon);
        }
        private Point MousePositionOverImage(MouseEventArgs mouseEvent)
        {
            Point point_over_image = mouseEvent.GetPosition(_image);
            double x = point_over_image.X;
            double y = point_over_image.Y;

            if (x < 0)
            {
                x = 0;
            }
            else if (x > _image.ActualWidth)
            {
                x = _image.ActualWidth;
            }

            if (y < 0)
            {
                y = 0;
            }
            else if (y > _image.ActualHeight)
            {
                y = _image.ActualHeight;
            }
            return new Point(x: x, y: y);
        }
        private Point GetNormalizedPosition(Point point)
        {
            return new Point(x: point.X / _image.ActualWidth, y: point.Y / _image.ActualHeight);
        }
        private Point GetNormalizedPosition(Point point, double width, double height)
        {
            return new Point(x: point.X / width, y: point.Y / height);
        }
        private double GetDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        private void CheckIsDragging(MouseEventArgs mouseEventArgs)
        {
            if (mouseEventArgs.LeftButton != MouseButtonState.Pressed)
                return;

            Point cursorPos = MousePositionOverImage(mouseEventArgs);
            Point normalizedCursorPosition = GetNormalizedPosition(cursorPos);

            _dragged_point = _points_relative.FirstOrDefault(point =>
                GetDistance(point.GetPoint(), normalizedCursorPosition) <= _POINT_SELECTION_THRESHOLD);

            if (_dragged_point != null)
            {
                _pointSelected = true;
                _isDragging = true;
            }
        }
        private void UpdateDraggedPoint(MouseEventArgs mouseEventArgs)
        {
            if (!_isDragging || mouseEventArgs.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            Point cursorPos = MousePositionOverImage(mouseEventArgs);
            Point norm_pos = GetNormalizedPosition(cursorPos);

            this._dragged_point.X = norm_pos.X;
            this._dragged_point.Y = norm_pos.Y;
        }

        private (double offsetW, double offsetH) CalculateOffsets()
        {
            double diffW = Math.Abs(_canvas.ActualWidth - _image.ActualWidth);
            double diffH = Math.Abs(_canvas.ActualHeight - _image.ActualHeight);
            return (diffW / 2, diffH / 2);
        }

        private Polygon CreatePolygon(double offsetW, double offsetH)
        {
            var polygon = new Polygon
            {
                StrokeThickness = 1.5,
                Stroke = System.Windows.Media.Brushes.Red
            };
            Canvas.SetLeft(polygon, offsetW);
            Canvas.SetTop(polygon, offsetH);
            return polygon;
        }

        private void AddPointEllipse(double offsetW, double offsetH, double col, double row)
        {
            var ellipse = new Ellipse
            {
                Fill = System.Windows.Media.Brushes.Red,
                Width = 2 * _ELLIPSE_SIZE,
                Height = 2 * _ELLIPSE_SIZE
            };

            Canvas.SetLeft(ellipse, offsetW + col - _ELLIPSE_SIZE);
            Canvas.SetTop(ellipse, offsetH + row - _ELLIPSE_SIZE);
            _canvas.Children.Add(ellipse);
        }

        private (double col, double row) GetAbsolutePosition(WrapPoint point) =>
            (point.X * _image.ActualWidth, point.Y * _image.ActualHeight);

        public void Dispose()
        {
            Detach_Canvas();
            GC.SuppressFinalize(this);
        }

        public void Detach_Canvas()
        {
            if (_canvas != null)
            {
                _canvas.MouseDown -= Handler_MouseDown;
                _canvas.MouseMove -= Handler_MouseMove;
                _canvas.MouseUp -= Handler_MouseUp;
                _canvas.SizeChanged -= Handler_SizeChanged;

                _canvas.Children.Clear();
            }

            _textBlock_MousePosition = null;
            _points_relative?.Clear();
            _dragged_point = null;
        }
    }
}
