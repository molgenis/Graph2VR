public class Utils
{
    // Get the TUI from the URI
    static public string GetShortLabelFromUri(string uri)
    {
        var list = uri.Split('/', '#');
        if (list.Length > 0) {
            return list[list.Length - 1];
        }
        return uri;
    }

}
