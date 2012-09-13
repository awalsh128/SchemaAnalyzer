using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SchemaKeys
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Schema _schema;
        private bool _schemaCreated;

        public MainWindow()
        {
            InitializeComponent();

            _schemaCreated = false;
            _SyncGridStates();

            for (int i = 0; i < Schema.AllAttributes.Length; i++)
            {
                this.schemaArityComboBox.Items.Add(i + 1);
            }
            this.schemaArityComboBox.IsEditable = false;
            this.schemaArityComboBox.SelectedIndex = 0;
            this.schemaArityComboBox.Focus();
            this.listsGrid.MaxHeight = System.Windows.SystemParameters.VirtualScreenHeight - 300;
        }

        private void _CreateSchemaFromFile(string filename)
        {
            string attributes;
            bool error;
            List<string> errors = new List<string>();
            string[] sides;
            string line;
            string[] lines = System.IO.File.ReadAllLines(filename);

            if (lines.Length > 0)
            {
                attributes = (new System.Text.RegularExpressions.Regex("[^a-zA-Z]")).Replace(lines[0], String.Empty);
                if (attributes.Length > 0)
                {
                    _schema = new Schema(attributes);

                    for (int i = 1; i < lines.Length; i++)
                    {
                        line = lines[i];
                        if (line.Contains(','))
                        {
                            sides = line.Split(',');
                            if (sides.Length == 2)
                            {
                                error = false;
                                for (int j = 0; j < 2; j++)
                                {
                                    for (int k = 0; k < sides[j].Length; k++)
                                    {
                                        if (!_schema.Attributes.Contains(sides[j][k]))
                                        {
                                            errors.Add("Line " + i + " does not have valid attributes on one of the sides ('" + line + "').");
                                            error = true;
                                            break;
                                        }
                                    }
                                }
                                if (!error) _schema.AddDependency(sides[0], sides[1]);
                            }
                            else
                            {
                                errors.Add("Line " + i + " does not have 2 sides ('" + line + "').");
                            }
                        }
                        else
                        {
                            errors.Add("Line " + i + " is not comma delimited ('" + line + "').");
                        }
                    }

                    if (errors.Count > 0)
                    {
                        MessageBox.Show(String.Join(Environment.NewLine, errors), "Schema Import Errors", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    if (errors.Count < lines.Length)
                    {
                        _CreateSchema();
                    }
                }
                else
                {
                    MessageBox.Show("No valid attributes found in the first line of the file.", "Schema Import Errors", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("No valid FD's found in the file.", "Schema Import Errors", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void _CreateSchema()
        {
            this.schemaRelationTextBlock.Text = "R(" + _schema.Attributes + ")";
            this.closureFilterTextBox.Text = _schema.Attributes;
            _schemaCreated = true;
            _SyncGridStates();
            _SyncUiData();
            this.fdXTextBox.Clear();
            this.fdYTextBox.Clear();
            this.fdXTextBox.Focus();
        }

        private void _FilterTextBox(TextBox box)
        {
            List<char> text = box.Text.ToUpper().ToCharArray().Distinct().ToList();
            for (int i = text.Count - 1; i >= 0; i--)
            {
                if (!_schema.Attributes.Contains(text[i])) text.RemoveAt(i);
            }
            text.Sort();
            box.Text = String.Join("", text);
        }

        private void _SyncUiData()
        {
            Paragraph closureContainer;
            TextBlock closure;
            AttributeSet filter = new AttributeSet(this.closureFilterTextBox.Text);

            RadialGradientBrush greenLed = new RadialGradientBrush();
            RadialGradientBrush redLed = new RadialGradientBrush();

            greenLed.GradientStops.Add(new GradientStop(Colors.Lime, 0.0));
            greenLed.GradientStops.Add(new GradientStop(Colors.Green, 2.0));
            redLed.GradientStops.Add(new GradientStop(Colors.Red, 0.0));
            redLed.GradientStops.Add(new GradientStop(Colors.Firebrick, 2.0));

            this.fdDocument.Blocks.Clear();
            this.closureDocument.Blocks.Clear();

            foreach (FunctionalDependency fd in _schema.Dependencies)
            {
                this.fdDocument.Blocks.Add(_CreateDependencyEntry(fd));
            }

            foreach (KeyValuePair<AttributeSet, AttributeSet> pair in _schema.Closures)
            {
                if (filter.Contains(pair.Key) && 
                    (this.closureTrivialCheckBox.IsChecked.Value || (pair.Key.Count != pair.Value.Count)))
                {
                    closure = new TextBlock();
                    closure.Inlines.Add(pair.Key.ToString());
                    closure.Inlines.Add(new Run("+") { FontSize = 10, BaselineAlignment = BaselineAlignment.Top });
                    closure.Inlines.Add(" = ");
                    closure.Inlines.Add(pair.Value.ToString());

                    if (_schema.CandidateKeys.Contains(pair.Key))
                    {
                        closure.Foreground = Brushes.ForestGreen;
                        closure.FontWeight = FontWeights.Bold;
                    }
                    else if (_schema.SuperKeys.Contains(pair.Key))
                    {
                        closure.Foreground = Brushes.DarkBlue;
                        closure.FontWeight = FontWeights.Bold;
                    }

                    closureContainer = new Paragraph();
                    closureContainer.Margin = new Thickness(10.0, 2.5, 5.0, 2.5);
                    closureContainer.Inlines.Add(closure);

                    this.closureDocument.Blocks.Add(closureContainer);
                }
            }

            if (_schema.IsBoyceCoddNormalForm())
            {
                this.bcnfLed.Background = greenLed;
            }
            else
            {
                this.bcnfLed.Background = redLed;
            }
            if (_schema.IsThreeNormalForm())
            {
                this.tnfLed.Background = greenLed;
            }
            else
            {
                this.tnfLed.Background = redLed;
            }
        }

        private Paragraph _CreateDependencyEntry(FunctionalDependency fd)
        {
            Paragraph block;
            InlineUIContainer container;
            LinearGradientBrush removeBrush;
            Button remove;
            Path arrow;
            GeometryGroup arrowLines;

            removeBrush = new LinearGradientBrush(Colors.White, Colors.DarkGray, new Point(0.5, 0.0), new Point(0.5, 1.0));
            removeBrush.GradientStops[0].Offset = 0.0;
            removeBrush.GradientStops[1].Offset = 0.2;

            remove = new Button();
            remove.Content = "X";
            remove.Tag = fd.GetHashCode();
            remove.FontSize = 8;
            remove.FontWeight = FontWeights.Bold;
            remove.Foreground = Brushes.White;
            remove.Margin = new Thickness(0.0, 0.0, 7.0, 0.0);
            remove.Height = 15;
            remove.Width = 15;
            remove.VerticalContentAlignment = System.Windows.VerticalAlignment.Stretch;
            remove.Background = removeBrush;
            remove.ToolTip = "Remove this FD.";
            remove.Click += new RoutedEventHandler(removeFdButton_Click);

            container = new InlineUIContainer(remove);
            container.BaselineAlignment = BaselineAlignment.Center;

            block = new Paragraph();
            block.Tag = fd.GetHashCode();
            block.Inlines.Add(container);
            block.Inlines.Add(fd.X.ToString());

            arrow = new Path();
            arrow.Margin = new Thickness(3.0, 0.0, 3.0, 0.0);
            arrow.SnapsToDevicePixels = true;
            arrow.Stroke = Brushes.Black;
            arrow.StrokeThickness = 1.0;
            arrow.FlowDirection = System.Windows.FlowDirection.LeftToRight;
            arrow.HorizontalAlignment = HorizontalAlignment.Center;
            arrow.VerticalAlignment = VerticalAlignment.Center;
            arrowLines = new GeometryGroup();
            arrowLines.Children.Add(new LineGeometry(new Point(10.0, 2.0), new Point(15.0, 6.0)));
            arrowLines.Children.Add(new LineGeometry(new Point(0.0, 6.0), new Point(15.0, 6.0)));
            arrowLines.Children.Add(new LineGeometry(new Point(10.0, 10.0), new Point(15.0, 6.0)));
            arrow.Data = arrowLines;
            block.Inlines.Add(arrow);

            block.Inlines.Add(fd.Y.ToString());

            return block;
        }

        private void _SyncGridStates()
        {
            if (_schemaCreated)
            {
                this.schemaCreateStackPanel.Visibility = Visibility.Collapsed;
                this.schemaViewBorder.Visibility = Visibility.Visible;
                this.addFdBorder.Visibility = Visibility.Visible;
                this.listsGrid.Visibility = Visibility.Visible;
                this.formBorder.Visibility = Visibility.Visible;
                this.ResizeMode = System.Windows.ResizeMode.CanResize;
            }
            else
            {
                this.schemaCreateStackPanel.Visibility = Visibility.Visible;
                this.schemaViewBorder.Visibility = Visibility.Collapsed;
                this.addFdBorder.Visibility = Visibility.Collapsed;
                this.listsGrid.Visibility = Visibility.Collapsed;
                this.formBorder.Visibility = Visibility.Collapsed;
                this.ResizeMode = System.Windows.ResizeMode.NoResize;
            }
        }

        private void removeFdButton_Click(object sender, RoutedEventArgs e)
        {
            int fdHash = (int)((Button)sender).Tag;
            foreach (FunctionalDependency fd in _schema.Dependencies)
            {
                if (fd.GetHashCode() == fdHash)
                {
                    _schema.Dependencies.Remove(fd);
                    break;
                }
            }
            foreach (Block item in this.fdDocument.Blocks)
            {
                if ((int)(item.Tag) == fdHash)
                {
                    this.fdDocument.Blocks.Remove(item);
                    break;
                }
            }
            _schema.Reevaluate();
            _SyncUiData();
        }

        private void fdAddButton_Click(object sender, RoutedEventArgs e)
        {
            if ((this.fdXTextBox.Text.Length + this.fdYTextBox.Text.Length) > 0)
            {
                _schema.AddDependency(fdXTextBox.Text, fdYTextBox.Text);

                _SyncUiData();

                this.fdXTextBox.Clear();
                this.fdYTextBox.Clear();
                this.fdAddButton.IsEnabled = false;
                this.fdXTextBox.Focus();
            }
        }

        private void schemaCreateButton_Click(object sender, RoutedEventArgs e)
        {
            _schema = new Schema(Int32.Parse(this.schemaArityComboBox.SelectedItem.ToString()));
            _CreateSchema();
        }

        private void schemaClearButton_Click(object sender, RoutedEventArgs e)
        {
            this.fdDocument.Blocks.Clear();
            this.closureDocument.Blocks.Clear();
            _schemaCreated = false;
            _SyncGridStates();
            this.schemaArityComboBox.Focus();
        }

        private void fdTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            char letter = e.Key.ToString().ToUpper().ToCharArray()[0];
            if (!_schema.Attributes.Contains(letter))
            {
                if (e.Key != Key.Tab) e.Handled = true;
            }
        }

        private void fdTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _FilterTextBox((TextBox)sender);
            this.fdAddButton.IsEnabled = (this.fdXTextBox.Text.Length + this.fdYTextBox.Text.Length) > 0;
        }

        private void fileDropBorder_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                _CreateSchemaFromFile(files[0]);
            }
        }

        private void closureFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _FilterTextBox((TextBox)sender);
            _SyncUiData();
        }

        private void closureFilterTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            char letter = e.Key.ToString().ToUpper().ToCharArray()[0];
            if (!_schema.Attributes.Contains(letter))
            {
                if (e.Key != Key.Tab) e.Handled = true;
            }
        }

        private void closureTrivialCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            _SyncUiData();
        }
    }
}
