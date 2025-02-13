using System.Text.RegularExpressions;

namespace HotChocolate.Utils
{
    public static class TextFormat
    {
        public static string ToDisplayName(string text)
        {
            return Regex.Replace(text, "([a-z])([A-Z])", "$1 $2");
        }
    }
}
