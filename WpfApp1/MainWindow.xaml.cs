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
        List<Planet> lstPlanet = new List<Planet>();
        private DispatcherTimer animationTimer;
        private const double Gravity = 0.1;
        private Dictionary<Planet, Ellipse> itemEllipseMap = new Dictionary<Planet, Ellipse>();
        bool isStart = false;
        private bool isDrawing = false;
        private Point startPoints;
        private Ellipse? drwEllipse; 
        #endregion
        public MainWindow()
        {
            InitializeComponent();
            planetDG.ItemsSource = lstPlanet;
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            double x = ParseDouble(txtX.Text);
            double y = ParseDouble(txtY.Text);
            double size = ParseDouble(txtSize.Text);

            if (double.IsNaN(x) || double.IsNaN(y) || double.IsNaN(size))
            {
                MessageBox.Show("Error, check the inputs");
                return;
            }

            int newId = lstPlanet.Count > 0 ? lstPlanet.Max(Planet => Planet.id) + 1 : 1; // Determine the new id

            Planet newPlanet = new Planet { id = newId, X = x, Y = y, Size = size };

            newPlanet = SetPosition(newPlanet);
            var ellipse = DrawItem(newPlanet.X, newPlanet.Y, newPlanet.Size);
            lstPlanet.Add(newPlanet);
            itemEllipseMap.Add(newPlanet, ellipse);
            UpdateDataGrid();
        }
        private Planet SetPosition(Planet newPlanet)
        {
            foreach (var referenceItem in lstPlanet)
            {
                if (lstPlanet.Count <= 1)
                {
                    newPlanet = AdjustPosition(newPlanet, referenceItem);
                }
                else
                {
                    foreach (var Planet in lstPlanet)
                    {
                        newPlanet = AdjustPosition(newPlanet, Planet);
                    }
                }
            }
            return newPlanet;
        }
        private Planet AdjustPosition(Planet newPlanet, Planet existingPlanet)
        {
            bool overlap = CheckOverlap(newPlanet, existingPlanet);
            if (overlap)
            {
                // Find the midpoint between the two items
                double x = (newPlanet.X + existingPlanet.X) / 2;
                double y = (newPlanet.Y + existingPlanet.Y) / 2;
                double offset = newPlanet.Size + 5; 
                int direction = new Random().Next(2);
                if (direction == 0)
                {
                    x += offset;
                }
                else
                {
                    y += offset;
                }

                double left = Canvas.GetLeft(planetCanvas);
                double right = Canvas.GetRight(planetCanvas);
                double top = Canvas.GetTop(planetCanvas);
                double bottom = Canvas.GetBottom(planetCanvas);


                if ( (x+ offset > left && x + offset < right) && (y + offset > top && y + offset < bottom))
                {
                    newPlanet.X = x;
                    newPlanet.Y = y;
                }
                else
                {
                    x += offset * -1;
                    y += offset * -1;
                    newPlanet.X = x;
                    newPlanet.Y = y;
                }
            }
            return newPlanet;
        }
        private bool CheckOverlap(Planet newPlanet, Planet existingPlanet)
        {
            double distance = Distance(newPlanet, existingPlanet);
            double minDistance = (newPlanet.Size + existingPlanet.Size) / 2;
            return distance < minDistance;
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
                Fill = null,
                Stroke = brush,
                StrokeThickness = 3
            };

            Canvas.SetLeft(ellipse, x - (size / 2));
            Canvas.SetTop(ellipse, y - (size / 2));

            planetCanvas.Children.Add(ellipse);
            return ellipse;
        }
        private void btnPay_Click(object sender, RoutedEventArgs e)
        {
            if (isStart == false)
            {
                animationTimer = new DispatcherTimer();
                animationTimer.Interval = TimeSpan.FromMilliseconds(30); // Set your desired interval
                animationTimer.Tick += AnimationTimer_Tick;
                animationTimer.Start();
                isStart = true;
                btnPlay.Content = "Stop";
            }
            else
            {
                animationTimer.Stop();
                isStart = false;
                btnPlay.Content = "Play";
            }
        }
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            foreach (var currentPlanet in lstPlanet)
            {
                foreach (var OtherPlanet in lstPlanet)
                {
                    if (currentPlanet != OtherPlanet)
                    {
                        double distance = Distance(currentPlanet, OtherPlanet);
                        if (distance >= 0)
                        {
                            CalculateForce(currentPlanet, OtherPlanet, out double forceX, out double forceY);
                            currentPlanet.X += forceX;
                            currentPlanet.Y += forceY;
                        }
                        if (CheckOverlap(currentPlanet, OtherPlanet))
                        {
                            var newLocation = AdjustPosition(currentPlanet, OtherPlanet);
                            currentPlanet.X = newLocation.X;
                            currentPlanet.Y = newLocation.Y;
                        }
                    }
                }
                if (itemEllipseMap.TryGetValue(currentPlanet, out Ellipse ellipse))
                {
                    Canvas.SetLeft(ellipse, currentPlanet.X - currentPlanet.Size / 2);
                    Canvas.SetTop(ellipse, currentPlanet.Y - currentPlanet.Size / 2);
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
                btnAdd_Click(null,null);
                drwEllipse = null;
            }
        }
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            txtX.Text = "";
            txtY.Text = "";
            txtSize.Text = "";
            planetCanvas.Children.Clear();
            lstPlanet.Clear();
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
            planetDG.Items.Refresh();
        }
        private double Distance(Planet Planet1, Planet Planet12)
        {
            //distance = √((x2 - x1) ^ 2 + (y2 - y1) ^ 2)
            return Math.Sqrt(Math.Pow(Planet1.X - Planet12.X, 2) + Math.Pow(Planet1.Y - Planet12.Y, 2));
        }
        private void CalculateForce(Planet currentPlanet, Planet otherPlanet, out double forceX, out double forceY)
        {
            double distance = Distance(currentPlanet, otherPlanet);
            if (distance >= 0)
            {
                //calculating the gravitational force between two objects => (Gravity * mass1 * mass2) / (distance^2)
                double force = Gravity * currentPlanet.Size * otherPlanet.Size / (distance * distance);
                //calculating the angle θ between these two points => angle = atan2(y2 - y1, x2 - x1)
                double angle = Math.Atan2(otherPlanet.Y - currentPlanet.Y, otherPlanet.X - currentPlanet.X);
                forceX = force * Math.Cos(angle);
                forceY = force * Math.Sin(angle);
            }
            else
            {
                forceX = 0;
                forceY = 0;
            }
        }
    }
}
