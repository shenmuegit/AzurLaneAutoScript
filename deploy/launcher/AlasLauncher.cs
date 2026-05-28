using System;
using System.Diagnostics;
using System.IO;

namespace AlasLauncher
{
    internal static class Program
    {
        private static int Main()
        {
            string root = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
            Directory.SetCurrentDirectory(root);
            Console.Title = "Alas Updater";
            Console.WriteLine("\"" + root);

            string path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            string prefix = string.Join(";",
                Path.Combine(root, "toolkit", "alias"),
                Path.Combine(root, "toolkit", "command"),
                Path.Combine(root, "toolkit"),
                Path.Combine(root, "toolkit", "Scripts"),
                Path.Combine(root, "toolkit", "Git", "mingw64", "bin"),
                Path.Combine(root, "toolkit", "Lib", "site-packages", "adbutils", "binaries"));
            Environment.SetEnvironmentVariable("PATH", prefix + ";" + path);

            int exitCode = Run(root, "python", "-m deploy.installer");
            if (exitCode != 0)
            {
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
                return exitCode;
            }

            string webapp = Path.Combine(root, "toolkit", "webapp", "alas.exe");
            if (!File.Exists(webapp))
            {
                Console.WriteLine("Unable to find " + webapp);
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
                return 1;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = webapp,
                WorkingDirectory = root
            });
            return 0;
        }

        private static int Run(string root, string fileName, string arguments)
        {
            using (Process process = Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = root,
                UseShellExecute = false
            }))
            {
                process.WaitForExit();
                return process.ExitCode;
            }
        }
    }
}
