using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Core;
using ImageTransformation.Core;
using static System.Net.Mime.MediaTypeNames;

namespace App_wpf.TransformationControls
{
    class GeneralMatrixControls : TransformationControls
    {
        public Button ExecuteBtn { get; private set; }

        // Array to hold the TextBox controls for each transformation matrix
        private TextBox[][,] TransformationValues { get; set; }
        private int _matrix_size;

        public GeneralMatrixControls(int matrix_size)
        {
            _matrix_size = matrix_size;
            TransformationValues = new TextBox[3][,];
            TransformationValues[0] = new TextBox[matrix_size, matrix_size];
            TransformationValues[1] = new TextBox[matrix_size, matrix_size];
            TransformationValues[2] = new TextBox[matrix_size, matrix_size];
            CreateLayout();
        }

        private void CreateLayout()
        {
            // Main Grid
            Grid mainGrid = new Grid();

            // Define column definitions
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // First column
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Separator
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Transform 1
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Space
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Transform 2
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // * Symbol
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Transform 3
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // * Symbol
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Pixel vector
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Execute button
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Close button

            // Add explanation to matrix rows
            Grid matrixDescriptionGrid = CreateMatrixDescriptionGrid();
            Grid.SetColumn(matrixDescriptionGrid, 0);
            mainGrid.Children.Add(matrixDescriptionGrid);

            // Add Transformation 1 grid
            Grid transformation1Grid = CreateTransformationGrid("Transformation 1", 0);
            Grid.SetColumn(transformation1Grid, 1);
            mainGrid.Children.Add(transformation1Grid);

            // Add Multiply Symbol 1
            Grid multiplySymbol1 = CreateMultiplySymbol();
            Grid.SetColumn(multiplySymbol1, 2);
            mainGrid.Children.Add(multiplySymbol1);

            // Add Transformation 2 grid
            Grid transformation2Grid = CreateTransformationGrid("Transformation 2", 1);
            Grid.SetColumn(transformation2Grid, 3);
            mainGrid.Children.Add(transformation2Grid);

            // Add Multiply Symbol 2
            Grid multiplySymbol2 = CreateMultiplySymbol();
            Grid.SetColumn(multiplySymbol2, 4);
            mainGrid.Children.Add(multiplySymbol2);

            // Add Transformation 3 grid
            Grid transformation3Grid = CreateTransformationGrid("Transformation 3", 2);
            Grid.SetColumn(transformation3Grid, 5);
            mainGrid.Children.Add(transformation3Grid);

            // Add Multiply Symbol 3
            Grid multiplySymbol3 = CreateMultiplySymbol();
            Grid.SetColumn(multiplySymbol3, 6);
            mainGrid.Children.Add(multiplySymbol3);

            // Add Pixel Vector
            Grid pixelVectorGrid = CreatePixelVectorGrid();
            Grid.SetColumn(pixelVectorGrid, 7);
            mainGrid.Children.Add(pixelVectorGrid);

            // Add Execute button and settings
            Grid settingsGrid = CreateSettingsGrid();
            Grid.SetColumn(settingsGrid, 8);
            mainGrid.Children.Add(settingsGrid);

            // Add Close button
            Button closeButton = new Button
            {
                Content = "Close",
                Margin = new Thickness(5),
                Padding = new Thickness(5)
            };
            Grid.SetColumn(closeButton, 10);
            mainGrid.Children.Add(closeButton);
            this.CloseBtn = closeButton;

            // Set the content of the window
            this.Controls = mainGrid;
        }

        private Grid CreateMatrixDescriptionGrid()
        {
            string[] texts = new string[] { "Row", "Col", "Homogenous", "Scale" };

            Grid grid = new Grid
            {
                Margin = new Thickness(5),
            };

            for (int i = 0; i <= _matrix_size; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }

            for (int i = 0; i < _matrix_size; i++)
            {
                TextBlock textBlock = new TextBlock
                {
                    Margin = new Thickness(5),
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Text = texts[i]
                };

                Grid.SetRow(textBlock, i + 1);
                grid.Children.Add(textBlock);
            }

            return grid;
        }

        private Grid CreateTransformationGrid(string title, int transformationMatrixIdx)
        {
            Grid grid = new Grid
            {
                Margin = new Thickness(5)
            };

            // Define rows and columns
            for (int i = 0; i <= _matrix_size; i++)
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            for (int i = 0; i <= _matrix_size; i++)
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Add title
            TextBlock titleBlock = new TextBlock
            {
                Text = title,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5)
            };
            Grid.SetRow(titleBlock, 0);
            Grid.SetColumnSpan(titleBlock, _matrix_size);
            grid.Children.Add(titleBlock);

            // Add textboxes with initial values
            Matrix identity = Transformations.Identity((Dimension)_matrix_size);

            // Create and store TextBoxes in the correct array index
            for (int row = 0; row < _matrix_size; row++)
            {
                for (int col = 0; col < _matrix_size; col++)
                {
                    TextBox textBox = new TextBox
                    {
                        Name = $"transformation{transformationMatrixIdx}_{row}{col}_value",
                        Width = 45,
                        Height = 25,
                        Margin = new Thickness(2),
                        TextAlignment = TextAlignment.Right,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Text = identity[row, col].ToString()
                    };
                    textBox.PreviewTextInput += NumberValidationTextBox;
                    if (row >= 3)
                    {
                        textBox.IsReadOnly = true;
                    }

                    Grid.SetRow(textBox, row + 1);
                    Grid.SetColumn(textBox, col);
                    grid.Children.Add(textBox);

                    // Store in the correct matrix index
                    TransformationValues[transformationMatrixIdx][row, col] = textBox;
                }
            }

            return grid;
        }

        private Grid CreateMultiplySymbol()
        {
            Grid grid = new Grid()
            {
                Margin = new Thickness(5)
            };

            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0.9, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

            TextBlock textBlock = new TextBlock
            {
                Text = "*",
                FontSize = 20,
                Margin = new Thickness(5),
                VerticalAlignment = VerticalAlignment.Top
            };

            Grid.SetRow(textBlock, 1);
            grid.Children.Add(textBlock);

            return grid;
        }

        private Grid CreatePixelVectorGrid()
        {
            Grid grid = new Grid
            {
                Margin = new Thickness(5, 5, 20, 5)
            };

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Add pixel vector text
            TextBlock vectorLabel = new TextBlock
            {
                Text = "Pixel vector",
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            Grid.SetRow(vectorLabel, 0);
            Grid.SetColumnSpan(vectorLabel, 3);
            grid.Children.Add(vectorLabel);

            // Add [p]
            TextBlock leftBracket = new TextBlock { Text = "[", FontSize = 40 };
            TextBlock pixelValue = new TextBlock { Text = "p", FontSize = 30 };
            TextBlock rightBracket = new TextBlock { Text = "]", FontSize = 40 };

            Grid.SetRow(leftBracket, 1);
            Grid.SetColumn(leftBracket, 0);

            Grid.SetRow(pixelValue, 1);
            Grid.SetColumn(pixelValue, 1);

            Grid.SetRow(rightBracket, 1);
            Grid.SetColumn(rightBracket, 2);

            grid.Children.Add(leftBracket);
            grid.Children.Add(pixelValue);
            grid.Children.Add(rightBracket);

            return grid;
        }

        private Grid CreateSettingsGrid()
        {
            Grid grid = new Grid
            {
                Margin = new Thickness(5)
            };

            // Define rows
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Add Transformation text
            TextBlock transformationText = new TextBlock
            {
                Text = "Transformation",
                Margin = new Thickness(5),
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(transformationText, 0);
            grid.Children.Add(transformationText);

            // Add Backward checkbox
            CheckBox backwardCheckbox = new CheckBox
            {
                Content = "Backward",
                Margin = new Thickness(5),
                Name = "checkBox_Backward",
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(backwardCheckbox, 1);
            grid.Children.Add(backwardCheckbox);
            this.Backward = backwardCheckbox;

            // Add Execute button
            Button executeButton = new Button
            {
                Content = "Execute",
                Margin = new Thickness(5),
                Padding = new Thickness(5),
                FontSize = 16,
                Name = "executeButton",
                VerticalAlignment = VerticalAlignment.Center
            };

            Grid.SetRow(executeButton, 2);
            grid.Children.Add(executeButton);
            this.ExecuteBtn = executeButton;

            return grid;
        }

        private void CreateInitialValues()
        {
            Matrix identity = Transformations.Identity(Dimension.D4);
            for (int k = 0; k < 3; k++)
            {
                for (int row = 0; row < _matrix_size; row++)
                {
                    for (int col = 0; col < _matrix_size; col++)
                    {
                        //TransformationValues[k][row, col].SetValue(TextBox.TextProperty, identity[row,col].ToString());
                        TransformationValues[k][row, col].Text = identity[row, col].ToString();
                    }
                }
            }
        }


        // Numeric validation for textboxes
        private void NumberValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            //e.Handled = !double.TryParse(e.Text, out _);

            // Allow numbers, decimal point, and minus sign
            e.Handled = !Regex.IsMatch(e.Text, @"^[-0-9.]$");
        }

        // Helper method to get matrix values
        public Matrix GetTransformationMatrix(int index, Dimension dimension)
        {
            TextBox[,] sourceArray = index switch
            {
                (0) => TransformationValues[0],
                (1) => TransformationValues[1],
                (2) => TransformationValues[2],
                _ => throw new ArgumentException("Invalid transformation index")
            };

            int dim = (int)dimension;
            Matrix matrix = new Matrix(dim, dim);
            matrix[dim - 1, dim - 1] = 1;
            for (int i = 0; i < _matrix_size; i++)
            {
                for (int j = 0; j < _matrix_size; j++)
                {
                    if (double.TryParse(sourceArray[i, j].Text, out double value))
                    {
                        matrix[i, j] = value;
                    }
                }
            }
            return matrix;
        }

        // get aggregated transformation matrix
        public Matrix GetTransformation()
        {
            Dimension dimension = (Dimension)_matrix_size;
            Matrix left = GetTransformationMatrix(0, dimension);
            Matrix middle = GetTransformationMatrix(1, dimension);
            Matrix right = GetTransformationMatrix(2, dimension);
            return (left * (middle * right));
        }
    }
}
