using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OsmMapControlLibrary.TileProviders
{
    public class OsmTileProvider : ITileProvider
    {
        private string FormatUrl(int zoom, int x, int y)
        {
            return String.Format("http://tile.openstreetmap.org/{0}/{1}/{2}.png",
                            zoom.ToString(CultureInfo.InvariantCulture),
                            x.ToString(CultureInfo.InvariantCulture),
                            y.ToString(CultureInfo.InvariantCulture));
        }

        public Uri GetTileUri(int zoom, int x, int y)
        {
            return new Uri(FormatUrl(zoom, x, y));
        }

        //public async Task<byte[]> LoadTileAsync(int zoom, int x, int y)
        //{
        //    var url = FormatUrl(zoom, x, y);

        //    var client = new HttpClient();
        //    var response = await client.GetByteArrayAsync(url);

        //    return response;
        //}
    }
}
