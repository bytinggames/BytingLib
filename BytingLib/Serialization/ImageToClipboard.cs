using System.Diagnostics;

namespace BytingLib
{
    internal class ImageToClipboard
    {
        /// <summary>Only works on windows</summary>
        internal static void Set(string imagePath, bool fileOrBitmapData = false)
        {
#if WINDOWS
            string script;
            if (fileOrBitmapData)
            {
                script = $@"
Set-Clipboard -Path '{imagePath}'
";
            }
            else
            {
                script = $@"
Set-Clipboard -Path '{imagePath}'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$image = [System.Drawing.Image]::FromFile('{imagePath}')
[System.Windows.Forms.Clipboard]::SetImage($image)
                ";
            }
            var psi = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-NoProfile -Command \"{script}\"",
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                return;
            }
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                Console.WriteLine(error);
            }
#endif
        }
    }
}
