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
            string search = "/Content/";
            int contentIndex = uri.IndexOf(search);
            string fullPath;
            if (contentIndex == -1)
            {
                fullPath = Path.GetFullPath(Path.Combine(modelDirRelativeToContent, uri));
                fullPath = fullPath.Substring(Environment.CurrentDirectory.Length + 1);
            }
            else
            {
                // not sure if this is the best fix for externally referenced textures
                // f.ex.
                // "uri":"../../../../../../../../Users/Julian/Desktop/stuntboost_tools/SE/Content/Models/Resources/Room_Protagonist/walls_darken.exr"
                fullPath = uri.Substring(contentIndex + search.Length);
            }
            return fullPath.Replace('\\', '/');
        }
    }
}
