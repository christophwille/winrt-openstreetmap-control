using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace DemoFrontend.Nominatim
{
    //
    // http://open.mapquestapi.com/nominatim/
    //
    // Copy/Paste Sample: http://open.mapquestapi.com/nominatim/v1/search?format=xml&q=Leoben&addressdetails=0&limit=3
    //
    public class NominatimProxy
    {
        public async Task<List<GeocodeResult>> ExecuteQuery(string query)
        {
            query = Uri.EscapeUriString(query);

            string URL =
                String.Format(
                    "http://open.mapquestapi.com/nominatim/v1/search?format=xml&q={0}&addressdetails=0&limit={1}&exclude_place_ids=613609",
                    query,
                    15);    // max. results to return, possibly make this configurable

            var client = new HttpClient();
            var response = await client.GetStringAsync(URL);

            var searchresults = XElement.Parse(response);
            var mapped = searchresults.Elements("place")
                .Select(e => new GeocodeResult()
                {
                    Name = (string)e.Attribute("display_name"),
                    Longitude = MappingHelpers.ConvertDouble((string)e.Attribute("lon")),
                    Latitude = MappingHelpers.ConvertDouble((string)e.Attribute("lat")),
                })
                .ToList();

            return mapped;
        }


        // 
        // Copy/Paste Sample: http://open.mapquestapi.com/nominatim/v1/reverse?format=xml&lat=51.521435&lon=-0.162714
        //
        public async Task<GeocodeResult> ReverseGeocode(double latitude, double longitude)
        {
            string URL =
                String.Format(
                    "http://open.mapquestapi.com/nominatim/v1/reverse?format=xml&lat={0}&lon={1}",
                    latitude.ToString(CultureInfo.InvariantCulture),
                    longitude.ToString(CultureInfo.InvariantCulture));

            var client = new HttpClient();
            var response = await client.GetStringAsync(URL);

            var searchresults = XElement.Parse(response);
            var resElement = searchresults.Elements("result").FirstOrDefault();
            var addressElement = searchresults.Elements("addressparts").FirstOrDefault();

            if (resElement != null && addressElement != null)
            {
                var countryCode = (string)addressElement.Element("country_code");

                if (0 == String.Compare("at", countryCode, StringComparison.OrdinalIgnoreCase))
                {
                    // Only if City or Town is not available, we will fall back to the actual location name
                    string locationName = (string)addressElement.Element("city");
                    if (String.IsNullOrWhiteSpace(locationName))
                    {
                        locationName = (string)addressElement.Element("town");

                        if (String.IsNullOrWhiteSpace(locationName))
                        {
                            locationName = (string)resElement;
                        }
                    }

                    var result = new GeocodeResult()
                                     {
                                         Name = locationName,
                                         Latitude = MappingHelpers.ConvertDouble((string) resElement.Attribute("lat")),
                                         Longitude = MappingHelpers.ConvertDouble((string) resElement.Attribute("lon"))
                                     };
                    
                    return result;
                }
            }

            return null;
        }
    }
}
