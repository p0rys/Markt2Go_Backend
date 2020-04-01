using Newtonsoft.Json.Linq;

namespace Markt2Go.Shared.Helper
{
    public class JsonHelper
    {
        public static bool TryParseJSON(string json, out JToken jObject)
        {
            try
            {
                jObject = JToken.Parse(json);
                return true;
            }
            catch
            {
                jObject = null;
                return false;
            }
        }
    }
}