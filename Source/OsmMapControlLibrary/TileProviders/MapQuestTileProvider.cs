using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmMapControlLibrary.TileProviders
{
    public class MapQuestTileProvider : ITileProvider
    {
        // http://developer.mapquest.com/web/products/open/map
        // See the note on zoom levels 11+ outside USA (this is ignored for this provider)

        private const string BaseFormatUrl = "http://otile{0}.mqcdn.com/tiles/1.0.0/osm/{1}/{2}/{3}.jpg";
        private int _otile = 1;


        public Uri GetTileUri(int zoom, int x, int y)
        {
            var url = String.Format(BaseFormatUrl,
                            _otile,
                            zoom.ToString(CultureInfo.InvariantCulture),
                            x.ToString(CultureInfo.InvariantCulture),
                            y.ToString(CultureInfo.InvariantCulture));

            if (++_otile == 5) 
                _otile = 1;

            return new Uri(url);
        }
    }
}
