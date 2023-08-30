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

        List<Item> lstItems = new List<Item>();
        private DispatcherTimer animationTimer;
        private const double gravityConst = 0.1;
        private Dictionary<Item, Ellipse> itemEllipseMap = new Dictionary<Item, Ellipse>();
        bool isStart = false;
        private bool isDrawing = false;
        private Point startPoint;
        private Ellipse drawingEllipse;

        public MainWindow()
        {
            InitializeComponent();
            ItemDataGrid.ItemsSource = lstItems;
            //ItemCanvas.MouseEnter += ItemCanvas_MouseEnter;
            //ItemCanvas.MouseLeave += ItemCanvas_MouseLeave;
            //ItemCanvas.MouseLeftButtonDown += ItemCanvas_MouseLeftButtonDown;
            //ItemCanvas.MouseLeftButtonUp += ItemCanvas_MouseLeftButtonUp;
            //ItemCanvas.MouseMove += ItemCanvas_MouseMove;

        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            double x = ParseDouble(XTextBox.Text);
            double y = ParseDouble(YTextBox.Text);
            double size = ParseDouble(SizeTextBox.Text);

            if (double.IsNaN(x) || double.IsNaN(y) || double.IsNaN(size))
            {
                MessageBox.Show("Error");
                return;
            }

            int newId = lstItems.Count > 0 ? lstItems.Max(item => item.id) + 1 : 1; // Determine the new id

            Item newItem = new Item { id = newId, X = x, Y = y, Size = size };

            newItem = AdjustShapePosition(newItem);
         
            var ellipse=DrawItem(newItem.X, newItem.Y, newItem.Size);
            lstItems.Add(newItem);
            itemEllipseMap.Add(newItem, ellipse); // Add to the dictionary
            UpdateDataGrid();
        }

        private Item AdjustShapePosition(Item newItem)
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
                double offset = newItem.Size+5;// GenerateRandomOffset(existingItem.Size, newItem.Size);

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
                    newItem.Size = newItem.Size;
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
                    newItem.Size = newItem.Size;
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


        //private double GenerateRandomOffset(double size1, double size2)
        //{
        //    double maxSize = Math.Max(size1, size2);
        //    return (new Random().NextDouble() + 1) * maxSize;
        //}

        private double ParseDouble(string value)
        {
            if (double.TryParse(value, out double result))
            {
                return result;
            }
            return double.NaN;
        }

        private void UpdateDataGrid()
        {
            ItemDataGrid.Items.Refresh(); 
        }

        private Ellipse DrawItem(double x, double y, double size)
        {
            Random random = new Random();
            Color color = Color.FromArgb(255, (byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));
            Brush brush = new SolidColorBrush(color);
            var ellipse = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = brush,
                Stroke = brush, 
                StrokeThickness = 3 
            };

            Canvas.SetLeft(ellipse, x - (size / 2));
            Canvas.SetTop(ellipse, y - (size / 2));

            ItemCanvas.Children.Add(ellipse);
            return ellipse;
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



        //private void ItemCanvas_MouseEnter(object sender, MouseEventArgs e)
        //{
        //  //  if (!isDrawing)
        //  //  {
        //        Cursor = Cursors.Cross;
        //  //  }
        //}

        //private void ItemCanvas_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    if (!isDrawing)
        //    {
        //        Cursor = Cursors.Arrow;
        //    }
        //}

        //private void ItemCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (!isDrawing)
        //    {
        //        isDrawing = true;
        //        startPoint = e.GetPosition(ItemCanvas);
        //        drawingEllipse = new Ellipse
        //        {
        //            Stroke = Brushes.Black,
        //            StrokeThickness = 2,
        //            Width = 0,
        //            Height = 0
        //        };
        //        Canvas.SetLeft(drawingEllipse, startPoint.X);
        //        Canvas.SetTop(drawingEllipse, startPoint.Y);
        //        ItemCanvas.Children.Add(drawingEllipse);
        //    }
        //}

        //private void ItemCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    if (isDrawing)
        //    {
        //        isDrawing = false;
        //        double width = Math.Abs(e.GetPosition(ItemCanvas).X - startPoint.X);
        //        double height = Math.Abs(e.GetPosition(ItemCanvas).Y - startPoint.Y);

        //        if (width > 0 && height > 0)
        //        {
        //            Brush brush = Brushes.Transparent; // Set your desired brush
        //            var newItem = new Item { id = lstItems.Count + 1, X = startPoint.X, Y = startPoint.Y, Size = Math.Max(width, height) };
        //            newItem = AdjustShapePosition(newItem);
        //            var ellipse = DrawItem(newItem.X, newItem.Y, newItem.Size);
        //            lstItems.Add(newItem);
        //            itemEllipseMap.Add(newItem, ellipse);
        //            UpdateDataGrid();
        //        }

        //        ItemCanvas.Children.Remove(drawingEllipse);
        //    }
        //}

        //private void ItemCanvas_MouseMove(object sender, MouseEventArgs e)
        //{
        //    if (isDrawing)
        //    {
        //        double width = Math.Abs(e.GetPosition(ItemCanvas).X - startPoint.X);
        //        double height = Math.Abs(e.GetPosition(ItemCanvas).Y - startPoint.Y);
        //        Canvas.SetLeft(drawingEllipse, Math.Min(startPoint.X, e.GetPosition(ItemCanvas).X));
        //        Canvas.SetTop(drawingEllipse, Math.Min(startPoint.Y, e.GetPosition(ItemCanvas).Y));
        //        drawingEllipse.Width = width;
        //        drawingEllipse.Height = height;
        //    }
        //}

    }
}



