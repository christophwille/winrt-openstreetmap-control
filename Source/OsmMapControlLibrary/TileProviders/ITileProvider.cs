using System;

namespace OsmMapControlLibrary.TileProviders
{
    public interface ITileProvider
    {
        Uri GetTileUri(int zoom, int x, int y);
    }
}