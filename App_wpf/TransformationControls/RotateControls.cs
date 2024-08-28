using Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace App_wpf.TransformationControls
{
    public class RotateControls : Controls
    {
        public UIElement Controls { get; private set; }
        public CheckBox Backward { get; private set; }
        public ComboBox IntepolationType { get; private set; }
        public Slider Slider { get; private set; }
        public Button CloseBtn { get; private set; }

        public RotateControls()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            // create stackpanel for controls
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            this.Controls = grid;

            // create checkbox for rotation type: forward or backward and add to the stackpanel
            CheckBox checkBox = new CheckBox();
            checkBox.Content = "Andvanced";
            checkBox.IsChecked = true;
            Grid.SetColumn(checkBox, 0);
            checkBox.HorizontalAlignment = HorizontalAlignment.Center;
            checkBox.VerticalAlignment = VerticalAlignment.Center;
            checkBox.Margin = new Thickness(5, 0, 5, 0);
            grid.Children.Add(checkBox);
            this.Backward = checkBox;

            // create separator line between checkbox and label
            Line line = new Line() { X1 = 0, X2 = 0, Y1 = 2, Y2 = 23 };
            line.Stroke = new SolidColorBrush(Colors.Gray);
            line.StrokeThickness = 1;
            line.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(line, 1);
            grid.Children.Add(line);

            // CENTER inputs    

            // create label for interpolation types
            Label label = new Label();
            label.Content = "Interpolation";
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.VerticalAlignment = VerticalAlignment.Center;
            label.Margin = new Thickness(2, 0, 2, 0);
            Grid.SetColumn(label, 2);
            grid.Children.Add(label);

            // create combobox for interpolation options
            ComboBox comboBox = new ComboBox();
            comboBox.Items.Add(InterpolationTypes.None.ToString());
            comboBox.Items.Add(InterpolationTypes.Floating_FromSource.ToString());
            comboBox.Items.Add(InterpolationTypes.Dir4_FromSource.ToString());
            comboBox.Items.Add(InterpolationTypes.Dir8_FromSource.ToString());
            comboBox.Items.Add(InterpolationTypes.Dir4_ToDestination.ToString());
            comboBox.Items.Add(InterpolationTypes.Dir8_ToDestination.ToString());
            comboBox.SelectedIndex = (int)InterpolationTypes.Floating_FromSource;
            comboBox.HorizontalAlignment = HorizontalAlignment.Center;
            comboBox.VerticalAlignment = VerticalAlignment.Center;
            comboBox.Margin = new Thickness(2, 0, 2, 0);
            Grid.SetColumn(comboBox, 3);
            grid.Children.Add(comboBox);
            this.IntepolationType = comboBox;

            // visibility changes
            checkBox.Click += (s, e) =>
            {
                if(checkBox.IsChecked == true)
                {
                    comboBox.Visibility = Visibility.Visible;
                    label.Visibility = Visibility.Visible;
                }
                else
                {
                    comboBox.Visibility = Visibility.Collapsed;
                    label.Visibility = Visibility.Collapsed;
                }
            };

            // create slider to set rotation angle
            Slider slider = new Slider();
            slider.Minimum = -180;
            slider.Maximum = 180;
            slider.TickFrequency = 1;
            slider.IsSnapToTickEnabled = true;
            slider.Value = 0;
            slider.HorizontalAlignment = HorizontalAlignment.Stretch;
            slider.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(slider, 4);
            grid.Children.Add(slider);
            this.Slider = slider;

            // create label for actual value in degree
            Label labelDegreeValue = new Label();
            Binding bindingSlider = new Binding("Value")
            {
                Source = slider,
                Mode = BindingMode.OneWay,
            };
            labelDegreeValue.SetBinding(Label.ContentProperty, bindingSlider);
            labelDegreeValue.Margin = new Thickness(2, 0, 2, 0);
            Grid.SetColumn(labelDegreeValue, 5);
            grid.Children.Add(labelDegreeValue);

            // create close button
            Button button = new Button();
            button.Content = "Close";
            button.Margin = new Thickness(0,0,2,0);
            Grid.SetColumn(button, 6);
            grid.Children.Add(button);
            this.CloseBtn = button;
        }
    }
}
