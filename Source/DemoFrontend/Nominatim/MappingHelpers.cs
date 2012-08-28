using System;
using System.Globalization;

namespace DemoFrontend.Nominatim
{
    internal static class MappingHelpers
    {
        internal static double ConvertDouble(string dblString)
        {
            double result = 0.0f;

            bool parseOk = Double.TryParse(dblString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result);

            return result;
        }
    }
}
