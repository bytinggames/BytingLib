using System;
using System.IO;
using System.Reflection;

namespace BytingLib
{
    public class CrashLogger
    {
        public static void TryRun(string crashLogFilePath, Action run)
        {
            try
            {
                run();
            }
            catch (Exception e)
            {
                string message = DateTime.Now.ToString("dd.MM.yyy HH:mm:ss") + ": Game crashed!\n" + e;
                AppendLog(crashLogFilePath, message);

                try
                {
                    string displayMessage = message + "\n\nlogged to file: " + crashLogFilePath;
                    Console.WriteLine(displayMessage); // this gets printed to linux terminal
                    using var messageBox = new MessageBox(message, "Fonts/MessageBox");
                    messageBox.Run();
                }
                catch (Exception e2)
                {
                    message = "MessageBox crashed:\n" + e2;
                    AppendLog(crashLogFilePath, message);
                }
            }
        }
        static void AppendLog(string logFile, string message)
        {
            File.AppendAllText(logFile, "\n" + message + "\n");
        }
    }
}
