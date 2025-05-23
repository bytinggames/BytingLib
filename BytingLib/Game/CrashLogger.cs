using System.Reflection;

namespace BytingLib
{
    public class CrashLogger
    {
        public static void Catch(Exception exception, string crashLogFilePath, string fontAssetName)
        {
            Catch(exception.ToString(), crashLogFilePath, fontAssetName);
        }
        public static void Catch(string message, string crashLogFilePath, string fontAssetName)
        {
            message = "Game crashed (" + DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm:ss") + " UTC)\n\n" + message;
            AppendLog(crashLogFilePath, message);

            try
            {
                string displayMessage = GetPopupMessage(message, crashLogFilePath);
                Console.WriteLine(displayMessage); // this gets printed to linux terminal
                using var messageBox = new MessageBox(displayMessage, fontAssetName);
                messageBox.Run();
            }
            catch (Exception e2)
            {
                message = "MessageBox crashed:\n" + e2;
                AppendLog(crashLogFilePath, message);
            }
        }

        private static string GetPopupMessage(string message, string crashLogFilePath)
        {
            return message + "\n\nlogged to file: " + crashLogFilePath + "\n\nv" + Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        }

        static void AppendLog(string logFile, string message)
        {
            File.AppendAllText(logFile, "\n" + message + "\n");
        }


        public static void TryRun(string crashLogFilePath, string fontAssetName, Action run)
        {
            try
            {
                run();
            }
            catch (Exception e)
            {
                string message = DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm:ss") + " UTC: Game crashed!\n" + e;
                AppendLog(crashLogFilePath, message);

                try
                {
                    string displayMessage = GetPopupMessage(message, crashLogFilePath);
                    Console.WriteLine(displayMessage); // this gets printed to linux terminal
                    using var messageBox = new MessageBox(displayMessage, fontAssetName);
                    messageBox.Run();
                }
                catch (Exception e2)
                {
                    message = "MessageBox crashed:\n" + e2;
                    AppendLog(crashLogFilePath, message);
                }
            }
        }
    }
}
