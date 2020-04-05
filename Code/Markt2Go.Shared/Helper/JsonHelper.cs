using System.Text.Json;

namespace Markt2Go.Shared.Helper
{
    public class JsonHelper
    {
        public static bool IsValidJson(string json)
        {
            try
            {
                var jsonObject = JsonDocument.Parse(json);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}