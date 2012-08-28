using System;
using Windows.Foundation;

namespace OsmMapControlLibrary
{
    /// <summary>
    /// This class contains some helper methods for
    /// the openstreetmap-project
    /// </summary>
    internal static class OsmHelper
    {
        /// <summary>
        /// Converts the coordinates of a point to a certain tilenumber
        /// </summary>
        /// <param name="lat">Latitude position</param>
        /// <param name="lon">Longitude position</param>
        /// <param name="zoom">Zooml level</param>
        /// <returns>Tilenumber of tile containing the coordinate</returns>
        public static Point ConvertToTileNumber(double lat, double lon, long zoom)
        {
            return new Point(
                (Math.Floor((lon + 180.0) / 360.0 * Math.Pow(2.0, zoom))),
                (Math.Floor((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) + 1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * Math.Pow(2.0, zoom))));
        }
        /// <summary>
        /// Converts the coordinates of a point to a certain tileposition
        /// </summary>
        /// <param name="lon">Longitude position</param>
        /// <param name="lat">Latitude position</param>
        /// <param name="zoom">Zooml level</param>
        /// <returns>Tilenumber of tile containing the coordinate</returns>
        public static Point ConvertToTilePosition(double lon, double lat, int zoom)
        {
            /*return new Point(
                (((lon + 180.0) / 360.0 * Math.Pow(2.0, zoom))),
                (((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) + 1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * Math.Pow(2.0, zoom))));*/

            Point p = new Point();
            p.X = (float)((lon + 180.0) / 360.0 * (1 << zoom));
            p.Y = (float)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) +
                1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom));

            return p;

        }

        /// <summary>
        /// Converts the 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="zoom"></param>
        /// <returns></returns>
        public static Point ConvertToCoordinate(double x, double y, long zoom)
        {            
            var n = Math.PI - 2.0 * Math.PI * y / Math.Pow(2.0, zoom);
            return new Point(
                x / Math.Pow(2.0, zoom) * 360.0 - 180,
                -180.0 / Math.PI * Math.Atan(0.5 * (Math.Exp(n) - Math.Exp(-n))));
        }
    }
}
