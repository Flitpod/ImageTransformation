using Core;
using ImageTransformation.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

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

    public class ProjectiveTransformationControls : TransformationControls
    {
        // members
        private Canvas _canvas;
        private Image _image;
        private TextBlock _textBlock_MousePosition;
        private List<WrapPoint> _points_relative;
        private WrapPoint _dragged_point;
        private volatile bool _isDragging;
        public Button ExecuteBtn { get; private set; }

        // ctor
        public ProjectiveTransformationControls(Canvas canvas, Image image)
        {
            _image = image;
            this._points_relative = new List<WrapPoint>() // { topleft, topright, bottomright, bottomleft }
            {
                new WrapPoint(new Point(x: 0,y: 0)),
                new WrapPoint (new Point(x : 1, y : 0)),
                new WrapPoint (new Point(x : 1, y : 1)),
                new WrapPoint (new Point(x : 0, y : 1)),
            };
            CreateLayout();
            Canvas_Init(canvas);
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

            // create close button
            Button closeButton = new Button();
            closeButton.Content = "Close";
            closeButton.Margin = new Thickness(0, 0, 2, 0);
            Grid.SetColumn(closeButton, 4);
            mainGrid.Children.Add(closeButton);
            this.CloseBtn = closeButton;
        }

        private void Canvas_Init(Canvas canvas)
        {
            _canvas = canvas;
            _canvas.MouseDown += Handler_MouseDown;
            _canvas.MouseMove += Handler_MouseMove;
            _canvas.MouseUp += Handler_MouseUp;
            _canvas.SizeChanged += Handler_SizeChanged;
        }
        public void Detach_Canvas()
        {
            _canvas.MouseDown -= Handler_MouseDown;
            _canvas.MouseMove -= Handler_MouseMove;
            _canvas.MouseUp -= Handler_MouseUp;
            _canvas.SizeChanged -= Handler_SizeChanged;
        }

        // event handlers
        private void Handler_MouseDown(object sender, MouseEventArgs mouseEventArgs)
        {
            CheckIsDragging(mouseEventArgs);
            UpdateDraggedPoint(mouseEventArgs);
            DrawWeb();
        }
        private void Handler_MouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            var point = MousePositionOverImage(mouseEventArgs);
            this._textBlock_MousePosition.Text = $"col: {Math.Round(point.X, 0)}, row: {Math.Round(point.Y, 0)}";
            if (_isDragging)
            {
                UpdateDraggedPoint(mouseEventArgs);
                DrawWeb();
            }
        }
        private void Handler_MouseUp(object sender, MouseEventArgs mouseEventArgs)
        {
            MouseUp_DragHandler(mouseEventArgs);
        }
        private void Handler_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawWeb();
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

            // calculate the offsets of visual elements related to image to position the over
            double diff_w = Math.Abs(_canvas.ActualWidth - _image.ActualWidth);
            double diff_h = Math.Abs(_canvas.ActualHeight - _image.ActualHeight);
            double offset_w = diff_w / 2;
            double offset_h = diff_h / 2;

            // create visual elements and add the to the canvas
            double ellipse_half_size = 5;
            Polygon polygon = new Polygon()
            {
                StrokeThickness = 3,
                Stroke = Brushes.Red,
            };
            Canvas.SetLeft(polygon, offset_w);
            Canvas.SetTop(polygon, offset_h);

            foreach (var item in _points_relative)
            {
                // calculate absolut positions based on actual height and width from relative points
                double col = item.X * _image.ActualWidth;
                double row = item.Y * _image.ActualHeight;

                Ellipse ellipse = new Ellipse()
                {
                    Fill = Brushes.Red,
                    Width = 2 * ellipse_half_size,
                    Height = 2 * ellipse_half_size,
                };

                Canvas.SetLeft(ellipse, offset_w + col - ellipse_half_size);
                Canvas.SetTop(ellipse, offset_h + row - ellipse_half_size);

                polygon.Points.Add(new Point(x: col, y: row));
                _canvas.Children.Add(ellipse);
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
        private double GetDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        // drag & drop behavior of the polygons
        private void CheckIsDragging(MouseEventArgs mouseEventArgs)
        {
            if (_isDragging || mouseEventArgs.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            Point cursorPos = MousePositionOverImage(mouseEventArgs);
            Point normalized_cursorPosition = GetNormalizedPosition(cursorPos);
            double normalized_distance = 10 / 400.0;

            foreach (var point in _points_relative)
            {
                if (GetDistance(point.GetPoint(), normalized_cursorPosition) <= normalized_distance)
                {
                    _isDragging = true;
                    _dragged_point = point;
                    return;
                }
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
            ;
        }
        private void MouseUp_DragHandler(MouseEventArgs mouseEventArgs)
        {
            if (mouseEventArgs.LeftButton == MouseButtonState.Released)
            {
                UpdateDraggedPoint(mouseEventArgs);
                DrawWeb();
                _isDragging = false;
                _dragged_point = null;
                mouseEventArgs.Handled = true;
            }
        }
    }
}
