using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Global Variables
        List<Item> lstItems = new List<Item>();
        private DispatcherTimer animationTimer;
        private const double gravityConst = 0.1;
        private Dictionary<Item, Ellipse> itemEllipseMap = new Dictionary<Item, Ellipse>();
        bool isStart = false;
        private bool isDrawing = false;
        private Point startPoints;
        private Ellipse drwEllipse; 
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            ItemDataGrid.ItemsSource = lstItems;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            double x = ParseDouble(txtX.Text);
            double y = ParseDouble(txtY.Text);
            double size = ParseDouble(txtSize.Text);

            if (double.IsNaN(x) || double.IsNaN(y) || double.IsNaN(size))
            {
                MessageBox.Show("Error, check the inputs");
                return;
            }

            int newId = lstItems.Count > 0 ? lstItems.Max(item => item.id) + 1 : 1; // Determine the new id

            Item newItem = new Item { id = newId, X = x, Y = y, Size = size };

            newItem = SetPosition(newItem);
            var ellipse = DrawItem(newItem.X, newItem.Y, newItem.Size);
            lstItems.Add(newItem);
            itemEllipseMap.Add(newItem, ellipse); 
            UpdateDataGrid();
        }
     
        #region Position
        private Item SetPosition(Item newItem)
        {
            foreach (var referenceItem in lstItems)
            {
                if (lstItems.Count <= 1)
                {
                    newItem = AdjustPosition(newItem, referenceItem);
                }
                else
                {
                    foreach (var item in lstItems)
                    {
                        newItem = AdjustPosition(newItem, item);
                    }
                }
            }
            return newItem;
        }
        private Item AdjustPosition(Item newItem, Item existingItem)
        {
            bool overlap = CheckOverlap(newItem, existingItem);
            if (overlap)
            {
                // Find the midpoint between the two items
                double x = (newItem.X + existingItem.X) / 2;
                double y = (newItem.Y + existingItem.Y) / 2;

                // Generate a random offset
                double offset = newItem.Size + 5; //GenerateRandomOffset(existingItem.Size, newItem.Size);

                int direction = new Random().Next(2);

                if (direction == 0)
                {
                    x += offset;
                }
                else
                {
                    y += offset;
                }

                double canvasX = Canvas.GetLeft(ItemCanvas);
                double canvasY = Canvas.GetTop(ItemCanvas);


                if (x < canvasX && x > 0 && y < canvasY && y > 0)
                {
                    newItem.X = x;
                    newItem.Y = y;
                }
                else
                {
                    // If the new position is outside the canvas, reverse the direction
                    if (x < 0)
                    {
                        direction = 1;
                    }
                    else
                    {
                        direction = 0;
                    }

                    x += offset * direction;
                    y += offset * direction;

                    newItem.X = x;
                    newItem.Y = y;
                }
            }
            return newItem;
        }
        private bool CheckOverlap(Item newItem, Item existingItem)
        {
            double distance = Math.Sqrt(Math.Pow(newItem.X - existingItem.X, 2) + Math.Pow(newItem.Y - existingItem.Y, 2));
            double minDistance = (newItem.Size + existingItem.Size) / 2;
            return distance < minDistance;
        }
         #endregion
     
        //private double GenerateRandomOffset(double size1, double size2)
        //{
        //    double maxSize = Math.Max(size1, size2);
        //    return (new Random().NextDouble() + 1) * maxSize;
        //}
        private Ellipse DrawItem(double x, double y, double size)
        {
            Random random = new Random();
            Color color = Color.FromArgb(255, (byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));
            Brush brush = new SolidColorBrush(color);
            var ellipse = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = null,
                Stroke = brush,
                StrokeThickness = 3
            };

            Canvas.SetLeft(ellipse, x - (size / 2));
            Canvas.SetTop(ellipse, y - (size / 2));

            ItemCanvas.Children.Add(ellipse);
            return ellipse;
        }
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (isStart == false)
            {
                animationTimer = new DispatcherTimer();
                animationTimer.Interval = TimeSpan.FromMilliseconds(30); // Set your desired interval
                animationTimer.Tick += AnimationTimer_Tick;
                animationTimer.Start();
                isStart = true;
                lblTimer.Content = "Start";
            }
            else
            {
                animationTimer.Stop();
                isStart = false;
                lblTimer.Content = "Stop";
            }
        }
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            foreach (var item in lstItems)
            {
                foreach (var otherItem in lstItems)
                {
                    if (item != otherItem)
                    {
                        double distance = Math.Sqrt(Math.Pow(otherItem.X - item.X, 2) + Math.Pow(otherItem.Y - item.Y, 2));
                        if (distance >= 0)
                        {
                            double force = gravityConst * item.Size * otherItem.Size / (distance * distance);
                            double angle = Math.Atan2(otherItem.Y - item.Y, otherItem.X - item.X);
                            double forceX = force * Math.Cos(angle);
                            double forceY = force * Math.Sin(angle);

                            item.X += forceX; // Update X position
                            item.Y += forceY; // Update Y position
                        }

                        if (CheckOverlap(item, otherItem))
                        {
                            var itemnew = AdjustPosition(item, otherItem);
                            item.X = itemnew.X;
                            item.Y = itemnew.Y;
                        }
                    }
                }
                if (itemEllipseMap.TryGetValue(item, out Ellipse ellipse))
                {
                    Canvas.SetLeft(ellipse, item.X - item.Size / 2);
                    Canvas.SetTop(ellipse, item.Y - item.Size / 2);
                }
            }
        }
        private void drawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            drwEllipse = null;
            startPoints = e.GetPosition(drawingCanvas);
        }
        private void drawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                double x = startPoints.X;
                double y = startPoints.Y;

                double width = e.GetPosition(drawingCanvas).X - startPoints.X;
                double height = e.GetPosition(drawingCanvas).Y - startPoints.Y;

                if (width < 0)
                {
                    x = e.GetPosition(drawingCanvas).X;
                    width = -width;
                }

                if (height < 0)
                {
                    y = e.GetPosition(drawingCanvas).Y;
                    height = -height;
                }

                if (drwEllipse == null)
                {
                    drwEllipse = new Ellipse
                    {
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        Fill = Brushes.LightGray
                    };

                    Canvas.SetLeft(drwEllipse, x);
                    Canvas.SetTop(drwEllipse, y);

                    drawingCanvas.Children.Add(drwEllipse);
                }

                drwEllipse.Width = width;
                drwEllipse.Height = height;
            }
        }
        private void drawingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (drwEllipse != null)
            {
                double width = drwEllipse.Width;
                double height = drwEllipse.Height;

                double centerX = Canvas.GetLeft(drwEllipse) + width / 2;
                double centerY = Canvas.GetTop(drwEllipse) + height / 2;

                double size = Math.Max(width, height);

                txtX.Text = centerX.ToString();
                txtY.Text = centerY.ToString();
                txtSize.Text = size.ToString();
                AddButton_Click(null,null);
                drwEllipse = null;
            }
        }
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            txtX.Text = "";
            txtY.Text = "";
            txtSize.Text = "";
            ItemCanvas.Children.Clear();
            lstItems.Clear();
            itemEllipseMap.Clear();
            UpdateDataGrid();
        }
        private double ParseDouble(string value)
        {
            if (double.TryParse(value, out double result))
            {
                return result;
            }
            return double.NaN;
        }
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && textBox.Text == "")
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
            }
        }
        private void UpdateDataGrid()
        {
            ItemDataGrid.Items.Refresh();
        }
    }
}



