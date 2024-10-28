namespace BytingLib
{
    public class DirectoryExtension
    {
        public static void CopyDirectory(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourceDir, targetDir));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string filePath in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
            {
                string targetFile = Path.Combine(targetDir, Path.GetRelativePath(sourceDir, filePath));
                File.Copy(filePath, targetFile, true);
            }
        }
    }
}
