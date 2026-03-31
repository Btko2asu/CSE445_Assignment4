using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Newtonsoft.Json.Linq;

namespace ConsoleApp1
{
    public class Submission
    {
        public static string xmlURL = "https://raw.githubusercontent.com/Btko2asu/CSE445_Assignment4/CSE445_Assignment4/NationalParks.xml";
        public static string xmlErrorURL = "https://raw.githubusercontent.com/Btko2asu/CSE445_Assignment4/CSE445_Assignment4/NationalParksErrors.xml";
        public static string xsdURL = "https://raw.githubusercontent.com/Btko2asu/CSE445_Assignment4/CSE445_Assignment4/NationalParks.xsd";

        public static void Main(string[] args)
        {
            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine(result);


            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(result);


            result = Xml2Json(xmlURL);
            Console.WriteLine(result);
        }

        // Q2.1
        public static string Verification(string xmlUrl, string xsdUrl)
        {
            try
            {
                bool isValid = true;
                string msg = "No errors are found";
               XmlSchemaSet parks = new XmlSchemaSet();
               parks.Add(null, xsdUrl);

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Schemas = parks;
                settings.ValidationType = ValidationType.Schema;

                settings.ValidationEventHandler += (sender, e) =>
                {
                    isValid = false;
                    msg += e.Severity + ": " + e.Message;
                };

                using (XmlReader reader = XmlReader.Create(xmlUrl, settings))
                {
                    while (reader.Read()) { }
                }
                return isValid ? "No errors are found" : msg;

            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }

        }

        public static string Xml2Json(string xmlUrl)
        {
            // The returned jsonText needs to be de-serializable by Newtonsoft.Json package. (JsonConvert.DeserializeXmlNode(jsonText))
            try
            {
                string xmlContent = DownloadContent(xmlUrl);
                XDocument doc = XDocument.Parse(xmlContent);
                XElement root = doc.Root;

                // create a JSON array to store park information
                JArray parkArray = new JArray();

                // loop through all NationalPark data and create JSON objects for each element
                foreach (XElement park in root.Elements("NationalPark"))
                {
                    JObject parkObject = new JObject();

                    XElement nameElement = park.Element("Name");
                    if (nameElement != null)
                    {
                        parkObject["Name"] = nameElement.Value;
                    }

                    // create JSON array to store multiple phone numbers elements
                    JArray phoneArray = new JArray();
                    foreach (XElement phone in park.Elements("Phone"))
                    {
                        phoneArray.Add(phone.Value);
                    }
                    parkObject["Phone"] = phoneArray;

                    //create JSON object for address to store all address information
                    XElement address = park.Element("Address");
                    if (address != null)
                    {
                        JObject addressObject = new JObject();

                        XElement number = address.Element("Number");
                        XElement street = address.Element("Street");
                        XElement city = address.Element("City");
                        XElement state = address.Element("State");
                        XElement zip = address.Element("Zip");

                        if (number != null) addressObject["Number"] = number.Value;
                        if (street != null) addressObject["Street"] = street.Value;
                        if (city != null) addressObject["City"] = city.Value;
                        if (state != null) addressObject["State"] = state.Value;
                        if (zip != null) addressObject["Zip"] = zip.Value;

                        //add attribute nearestAirport to address object
                        XAttribute nearestAirport = address.Attribute("NearestAirport");
                        if (nearestAirport != null) addressObject["@NearestAirport"] = nearestAirport.Value;
                        parkObject["Address"] = addressObject;
                    }
                    //add attribute rating to park object
                    XAttribute rating = park.Attribute("Rating");
                    if (rating != null) parkObject["@Rating"] = rating.Value;
                    parkArray.Add(parkObject);
                }
                JObject finalObject = new JObject(
                    new JProperty("NationalParks",
                        new JObject(
                            new JProperty("NationalPark", parkArray)
                        )
                    )
                );
                return finalObject.ToString(Newtonsoft.Json.Formatting.None);
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }
        // Helper method to download content from URL
        private static string DownloadContent(string url)
        {
            using (System.Net.WebClient client = new System.Net.WebClient())
            {
                return client.DownloadString(url);
            }
        }
    }

}