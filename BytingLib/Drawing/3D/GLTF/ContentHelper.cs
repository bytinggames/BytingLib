namespace BytingLib
{
    class ContentHelper
    {
        internal static string UriToContentFile(string uri, string modelDirRelativeToContent)
        {
            string ext = Path.GetExtension(uri);
            return UriToContentFileWithExtension(uri.Remove(uri.Length - ext.Length), modelDirRelativeToContent);
        }
        internal static string UriToContentFileWithExtension(string uri, string modelDirRelativeToContent)
        {
            string fullPath = Path.GetFullPath(Path.Combine(modelDirRelativeToContent, uri));
            fullPath = fullPath.Substring(Environment.CurrentDirectory.Length + 1);
            return fullPath.Replace('\\', '/');
        }
    }
}
