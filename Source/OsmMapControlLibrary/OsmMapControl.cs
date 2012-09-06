using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace OsmMapControlLibrary
{
    public class OsmMapControl : Canvas
    {
        /// <summary>
        /// Initializes a new instance of the MainPage class.
        /// </summary>
        public OsmMapControl()
        {
            this.CurrentPosition = new Point(0.476317347467935, 0.669774812152535);
            this.TargetPosition = new Point(0.476317347467935, 0.669774812152535);
            this.CurrentSpeed = new Point(0.00, 0.00);
            this.MoveToPosition = null;
            this.CurrentZoomLevel = 9;
            this.CurrentZoom = 250000;
            this.TargetZoom = 300000;

            // do not show any rendering in design mode
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                CompositionTarget.Rendering += CompositionTarget_Rendering;

                this.PointerPressed += Map_PointerPressed;
                this.PointerReleased += Map_PointerReleased;
                this.PointerMoved += Map_PointerMoved;
                this.PointerWheelChanged += Map_PointerWheelChanged;

                // this.KeyDown += Map_KeyDown;
                // this.AddHandler(KeyDownEvent, new KeyEventHandler(Map_KeyDown), true);

                this.Loaded += OnLoaded;
                this.SizeChanged += OnSizeChanged;
            }
        }

        // TODO: not working as expected, thus not being used at the moment
        private void Map_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Add)
            {
                this.TargetZoom *= 2;
                this.UpdateAllTiles();
            }
            else if (e.Key == VirtualKey.Subtract)
            {
                this.TargetZoom /= 2;
                this.UpdateAllTiles();
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            ClipToBounds();
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            ClipToBounds();
        }

        private void ClipToBounds()
        {
            this.Clip = new RectangleGeometry()
                                {
                                    Rect = new Rect(0, 0, this.ActualWidth, this.ActualHeight)
                                };
        }

        /// <summary>
        /// Base zoom to be used to make the map bigger
        /// </summary>
        public const double BaseZoom = 1;

        /// <summary>
        /// Number of images, that are currently loading
        /// </summary>
        private int currentlyLoading = 0;

        /// <summary>
        /// Gets or sets the current position
        /// </summary>
        protected Point CurrentPosition
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the current position
        /// </summary>
        protected Point CurrentSpeed
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the current zoom by GUI
        /// </summary>
        protected double CurrentZoom
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the current zoom by GUI
        /// </summary>
        private double _targetZoom;
        protected double TargetZoom
        {
            get { return _targetZoom; }
            set
            {
                if (value < 100)
                {
                    _targetZoom = 100;
                }
                else if (value > 100000000)
                {
                    _targetZoom = 100000000;
                }
                else
                {
                    _targetZoom = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the target position DURING the button
        /// down of the left mouse button. If mouse button is 
        /// released the position won't be regarded
        /// </summary>
        protected Point TargetPosition
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the position to move to without
        /// regarding any other speed or position argument
        /// </summary>
        protected Point? MoveToPosition
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the current zoom level
        /// </summary>
        private int _currentZoomLevel;
        protected int CurrentZoomLevel
        {
            get { return _currentZoomLevel; }
            set
            {
                if (value > TileInfo.MaxZoom)
                {
                    _currentZoomLevel = TileInfo.MaxZoom;
                }
                else if (value < 0)
                {
                    _currentZoomLevel = 0;
                }
                else
                {
                    _currentZoomLevel = value;
                }
            }
        }

        /// <summary>
        /// Stores a list of all tiles, that shall be loading or have been 
        /// Loading
        /// </summary>
        List<TileInfo> addedTiles = new List<TileInfo>();

        /// <summary>
        /// Stores a list of all tiles, that have not been loaded yet
        /// </summary>
        List<TileInfo> notLoadedTiles = new List<TileInfo>();

        protected int TilesRetrieved = 0;

        /// <summary>
        /// Stores the value when the datetime have been clicked last
        /// </summary>
        private DateTime lastClick = DateTime.MinValue;

        /// <summary>
        /// Stores whether the mouse is currently down
        /// </summary>
        private bool isMouseDown = false;

        private Point? lastPosition = null;

        /// Defines the energymanager, storing the power available for scrolling
        private ScrollEnergyManager scrollEnergyManager = new ScrollEnergyManager();

        private void CompositionTarget_Rendering(object sender, object o)
        {
            if (SuspendRendering) return;

            // Debug.WriteLine("Rendering " + DateTime.Now.ToString());

            this.scrollEnergyManager.Recharge();

            var change = false;
            var ratio = (CurrentZoom / TargetZoom);

            if (ratio < 0.98 || ratio > 1.02)
            {
                //var diff = this.CurrentZoom - this.TargetZoom;
                this.CurrentZoom /= Math.Pow(ratio, 1 / 10.0);
                this.CurrentZoomLevel = ConvertZoomToZoomLevel(CurrentZoom);

                change = true;
            }

            if (MoveToPosition.HasValue)
            {
                // Cinematic movement
                var posDiffX = this.CurrentPosition.X - this.MoveToPosition.Value.X;
                var posDiffY = this.CurrentPosition.Y - this.MoveToPosition.Value.Y;

                this.CurrentPosition = new Point(
                    this.CurrentPosition.X - posDiffX * 0.15,
                    this.CurrentPosition.Y - posDiffY * 0.15);

                if (Math.Abs(posDiffX) + Math.Abs(posDiffY)
                    < 1 / this.TargetZoom)
                {
                    this.MoveToPosition = null;
                }

                change = true;
            }
            else
            {
                // Cinematic movement
                var posDiffX = 0.0;
                var posDiffY = 0.0;

                if (this.isMouseDown)
                {
                    posDiffX = this.CurrentPosition.X - this.TargetPosition.X;
                    posDiffY = this.CurrentPosition.Y - this.TargetPosition.Y;
                }

                var springFactor = 0.7;
                var friction = 0.999;
                this.CurrentSpeed = new Point(
                    (this.CurrentSpeed.X * friction - posDiffX) * springFactor,
                    (this.CurrentSpeed.Y * friction - posDiffY) * springFactor);

                if ((Math.Abs(this.CurrentSpeed.X) + Math.Abs(this.CurrentSpeed.Y))
                    > 1 / this.TargetZoom)
                {
                    var timeStep = 0.1;
                    this.CurrentPosition = new Point(
                        (this.CurrentPosition.X + this.CurrentSpeed.X * timeStep),
                        (this.CurrentPosition.Y + this.CurrentSpeed.Y * timeStep));

                    change = true;
                }
            }

            if (change)
            {
                this.UpdateAllTiles();
            }

            // Check, if we can load a tile
            if (this.currentlyLoading <= 3 && this.notLoadedTiles.Count > 0)
            {
                var notLoadedTile = this.notLoadedTiles.Last();

                notLoadedTile.LoadImage();
                this.UpdateTile(notLoadedTile);
                this.Children.Add(notLoadedTile.TileImage);

                this.notLoadedTiles.Remove(notLoadedTile);
                this.currentlyLoading++;
                notLoadedTile.LoadingFinished += (x, y) =>
                {
                    this.currentlyLoading--;
                };
            }
        }

        protected void UpdateAllTiles()
        {
            // Convert upper left image of window to tile coordinates
            // this.CurrentZoom   
            LoadAllTilesForZoomLevel(this.CurrentZoomLevel, true);

            foreach (var tileInfo in addedTiles.Where(x => x.TileImage != null).ToList())
            {
                UpdateTile(tileInfo);
            }
        }

        private void UpdateTile(TileInfo tileInfo)
        {
            if ((tileInfo.Zoom > (this.CurrentZoomLevel + 1))
                || (tileInfo.Zoom < (this.CurrentZoomLevel - 2)))
            {
                // Hide unused images
                this.RemoveImage(tileInfo);
            }

            var zoom = this.CurrentZoomLevel;
            var localZoom = Math.Pow(2, zoom);
            localZoom = Math.Pow(2, tileInfo.Zoom);

            var position = tileInfo.GetCoordinates(localZoom);
            Canvas.SetLeft(
                tileInfo.TileImage,
                (position.X + this.CurrentPosition.X)
                    * this.CurrentZoom * BaseZoom
                    + this.ActualWidth / 2);
            Canvas.SetTop(
                tileInfo.TileImage,
                (position.Y + this.CurrentPosition.Y)
                    * this.CurrentZoom * BaseZoom
                    + this.ActualHeight / 2);
            tileInfo.TileImage.Width = (this.CurrentZoom / localZoom) + 0.5;
            tileInfo.TileImage.Height = (this.CurrentZoom / localZoom) + 0.5;
        }

        /// <summary>
        /// Removes an image from tile list
        /// </summary>
        /// <param name="tileInfo">Image to be removed</param>
        private void RemoveImage(TileInfo tileInfo)
        {
            var animation = new DoubleAnimation();
            animation.From = 1.0;
            animation.To = 0.0;
            animation.Duration = TimeSpan.FromSeconds(0.1);

            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(storyboard, tileInfo.TileImage);
            Storyboard.SetTargetProperty(storyboard, "Image.Opacity");
            storyboard.Begin();

            storyboard.Completed += (x, y) =>
            {
                this.Children.Remove(tileInfo.TileImage);
                this.notLoadedTiles.Remove(tileInfo);
                this.addedTiles.Remove(tileInfo);
            };
        }

        /// <summary>
        /// Loads all tiles for a specific zoom level
        /// </summary>
        /// <param name="requiredZoom">Required zoomlevel</param>
        /// <param name="visible">Flag whether the tiles are visible</param>
        private void LoadAllTilesForZoomLevel(int requiredZoom, bool visible)
        {
            if (requiredZoom < 0)
            {
                // Nothing to load
                return;
            }

            var toLoadTiles = new List<TileInfo>();
            for (var zoom = requiredZoom - 1; zoom <= requiredZoom; zoom++)
            {
                var localZoom = Math.Pow(2, zoom);
                var left =
                    ((-this.ActualWidth / 1.8 / this.CurrentZoom) -
                    this.CurrentPosition.X) / BaseZoom;
                var right =
                    ((this.ActualWidth / 1.8 / this.CurrentZoom) -
                    this.CurrentPosition.X) / BaseZoom;
                var top =
                    ((-this.ActualHeight / 1.8 / this.CurrentZoom) -
                    this.CurrentPosition.Y) / BaseZoom;
                var bottom =
                    ((this.ActualHeight / 1.8 / this.CurrentZoom) -
                    this.CurrentPosition.Y) / BaseZoom;

                // Loads all images
                for (var x = (int)Math.Floor(left * localZoom);
                    x <= (int)Math.Ceiling(right * localZoom);
                    x++)
                {
                    for (var y = (int)Math.Floor(top * localZoom);
                        y <= (int)Math.Ceiling(bottom * localZoom);
                        y++)
                    {
                        toLoadTiles.Add(this.ShowTile(x, y, requiredZoom));
                    }
                }
            }

            // Check for all tiles, that are in notLoadedList and not in loadedTiles
            foreach (var tile in this.notLoadedTiles.ToList())
            {
                if (!toLoadTiles.Contains(tile))
                {
                    this.notLoadedTiles.Remove(tile);
                    this.addedTiles.Remove(tile);
                }
            }
        }

        /// <summary>
        /// Shows a specific tile
        /// </summary>
        /// <param name="tileX">X-Coordinate of tile</param>
        /// <param name="tileY">Y-Coordinate of tile</param>
        /// <param name="zoom">Zoomlevel of tile</param>
        /// <param name="visible">Flag whether tiles are visible</param>
        protected TileInfo ShowTile(int tileX, int tileY, int zoom)
        {
            // Shows if shown tile is in loadedtiles
            var found = this.addedTiles.FirstOrDefault(
                x => x.TileX == tileX && x.TileY == tileY && x.Zoom == zoom);

            if (found != null)
            {
                if (found.TileImage != null)
                {
                    if (zoom <= this.CurrentZoom &&
                        found.TileImage.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
                    {
                        // Switch visibility flag
                        found.TileImage.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    }
                }

                // Already loaded
                return found;
            }

            // Creates images if necessary
            var tileInfo = new TileInfo();
            tileInfo.TileX = tileX;
            tileInfo.TileY = tileY;
            tileInfo.Zoom = zoom;

            this.addedTiles.Add(tileInfo);
            this.notLoadedTiles.Add(tileInfo);
            this.TilesRetrieved++;

            return tileInfo;
        }

        private void Map_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var delta = e.GetCurrentPoint(this).Properties.MouseWheelDelta;

            var factor = Math.Exp(this.scrollEnergyManager.RequestEnergy(((double)delta) / 120));
            
            if (Double.IsNaN(factor))
            {
                Debug.WriteLine("factor = Double.NaN");
            }
            else
            {
                this.TargetZoom *= factor;
            }
        }


        private void Map_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var currentPoint = e.GetCurrentPoint(this);
            bool isLeftButton = currentPoint.Properties.IsLeftButtonPressed;
            if (!isLeftButton) return;

            var now = DateTime.Now;
            if ((now - this.lastClick) < TimeSpan.FromSeconds(0.2))
            {
                // Double click
                this.TargetZoom *= 2;

                // Inverse position
                var x = (currentPoint.Position.X - this.ActualWidth / 2)
                     / this.CurrentZoom / BaseZoom
                     - this.CurrentPosition.X;
                var y = (currentPoint.Position.Y - this.ActualHeight / 2)
                     / this.CurrentZoom / BaseZoom
                     - this.CurrentPosition.Y;

                this.MoveToPosition = new Point(-x, -y);
            }

            this.lastClick = now;

            // Gets position of mouse
            this.TargetPosition = this.CurrentPosition;
            this.lastPosition = e.GetCurrentPoint(this).Position;

            this.isMouseDown = true;
        }

        private void Map_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            this.lastPosition = e.GetCurrentPoint(this).Position;
            this.isMouseDown = false;
        }

        private void Map_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (this.isMouseDown)
            {
                var position = e.GetCurrentPoint(this).Position;

                if (this.lastPosition.HasValue)
                {
                    var deltaX = position.X - lastPosition.Value.X;
                    var deltaY = position.Y - lastPosition.Value.Y;

                    deltaX /= this.CurrentZoom;
                    deltaY /= this.CurrentZoom;

                    this.TargetPosition =
                        new Point(
                            this.TargetPosition.X + deltaX,
                            this.TargetPosition.Y + deltaY);

                    this.UpdateAllTiles();
                }

                this.lastPosition = position;
            }
        }

        protected int ConvertZoomToZoomLevel(double zoom)
        {
            return (int)Math.Round(Math.Log(zoom, 2.0) - 7.9);   
        }

        protected double ConvertZoomLevelToZoom(double zoomlevel)
        {
            return Math.Pow(2, zoomlevel + 7.9);
        }

        protected bool SuspendRendering { get; set; }

        // Public Interface of the Control

        public void SetView(double latitude, double longitude, int zoomlevel)
        {
            if (zoomlevel < 0 || zoomlevel > TileInfo.MaxZoom)
                throw new ArgumentOutOfRangeException("Zoom Level is out of range");

            // Temporarily suspend rendering until all variables are set
            SuspendRendering = true;

            TargetZoom = ConvertZoomLevelToZoom(zoomlevel);
            MoveToPosition = OsmHelper.ConvertToTilePosition(-longitude, -latitude, 0);
            
            SuspendRendering = false;
        }
    }
}
