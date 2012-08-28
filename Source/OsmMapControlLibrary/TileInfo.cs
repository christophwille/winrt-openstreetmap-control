using System;
using System.Globalization;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

namespace OsmMapControlLibrary
{
    /// <summary>
    /// This class stores information about a tile
    /// </summary>
    public class TileInfo
    {
        /// <summary>
        /// This event is called when loading has been finished
        /// </summary>
        public event EventHandler LoadingFinished;

        /// <summary>
        /// Stores the maximum allowed zoomvalue
        /// </summary>
        public const int MaxZoom = 16;

        /// <summary>
        /// Gets or sets the x-coordinate of the tile
        /// </summary>
        public int TileX
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the y-coordinate of the tile
        /// </summary>
        public int TileY
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the zoom of the tile
        /// </summary>
        public int Zoom
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the image of the tile
        /// </summary>
        public Image TileImage
        {
            get;
            set;
        }

        /// <summary>
        /// Loads the image by tileposition
        /// </summary>
        public void LoadImage()
        {
            var tileX = this.TileX;
            var tileY = this.TileY;
            var localZoom = (int)Math.Pow(2, this.Zoom);
            var currentZoom = this.Zoom;

            while (currentZoom > MaxZoom)
            {
                currentZoom--;
                tileX /= 2;
                tileY /= 2;
            }

            while (tileX < 0)
            {
                tileX += localZoom;
            }

            while (tileY < 0)
            {
                tileY += localZoom;
            }

            while (tileX >= localZoom)
            {
                tileX -= localZoom;
            }

            while (tileY >= localZoom)
            {
                tileY -= localZoom;
            }

            var url = string.Format(
                "http://tile.openstreetmap.org/{0}/{1}/{2}.png",
                this.Zoom.ToString(CultureInfo.InvariantCulture),
                tileX.ToString(CultureInfo.InvariantCulture),
                tileY.ToString(CultureInfo.InvariantCulture));

            var image = new Image();
            var source = new BitmapImage(new Uri(url, UriKind.Absolute));
            image.Opacity = 0.0;
            source.ImageOpened += (x,y) =>
                {
                    var animation = new DoubleAnimation();
                    animation.From = 0.0;
                    animation.To = 1.0;
                    animation.Duration = TimeSpan.FromSeconds(0.1);

                    var storyboard = new Storyboard();
                    storyboard.Children.Add(animation);
                    Storyboard.SetTarget(storyboard, image);

                    // http://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.xaml.media.animation.storyboard.settargetproperty
                    // http://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.xaml.propertypath.propertypath
                    Storyboard.SetTargetProperty(storyboard, "Image.Opacity");
                    storyboard.Begin();

                    if (this.LoadingFinished != null)
                    {
                        this.LoadingFinished(this, EventArgs.Empty);
                    }
                };

            image.Source = source;

            Canvas.SetZIndex(image, this.Zoom);
            this.TileImage = image;
        }

        /// <summary>
        /// Gets the coordinate of the image
        /// </summary>
        /// <returns>Position of the image</returns>
        public Point GetCoordinates(double zoom)
        {
            var divisor = zoom; //  Math.Pow(2, this.Zoom);
            return new Point(
                (this.TileX / divisor),
                (this.TileY / divisor));
        }

        public override bool Equals(object obj)
        {
            var item = obj as TileInfo;
            if (item == null)
            {
                return false;
            }

            return this.TileX == item.TileX 
                && this.TileY == item.TileY 
                && this.Zoom == item.Zoom;
        }

        public override int GetHashCode()
        {
            return this.TileX.GetHashCode() ^ this.TileY.GetHashCode() ^ this.Zoom.GetHashCode();
        }

    }
}
