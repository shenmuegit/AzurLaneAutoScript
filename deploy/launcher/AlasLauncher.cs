using System;
using System.Diagnostics;
using System.IO;
using System.Text;

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

            DisableElectronUpdater(root);

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

        private static void DisableElectronUpdater(string root)
        {
            string appAsar = Path.Combine(root, "toolkit", "webapp", "resources", "app.asar");
            if (!File.Exists(appAsar))
            {
                return;
            }

            byte[] content = File.ReadAllBytes(appAsar);

            bool patched = false;
            patched |= ReplaceInPlace(
                content,
                "autoUpdater.checkForUpdatesAndNotify()",
                "Promise.resolve()" + new string(' ', 21));
            patched |= ReplaceInPlace(
                content,
                "e.checkForUpdatesAndNotify()",
                "Promise.resolve()" + new string(' ', 11));

            if (!patched)
            {
                return;
            }

            File.WriteAllBytes(appAsar, content);
            Console.WriteLine("Electron updater disabled");
        }

        private static bool ReplaceInPlace(byte[] content, string searchText, string replaceText)
        {
            byte[] search = Encoding.UTF8.GetBytes(searchText);
            byte[] replace = Encoding.UTF8.GetBytes(replaceText);
            if (search.Length != replace.Length)
            {
                throw new InvalidOperationException("Replacement must preserve byte length");
            }

            int index = IndexOf(content, search);
            if (index < 0)
            {
                return false;
            }

            Buffer.BlockCopy(replace, 0, content, index, replace.Length);
            return true;
        }

        private static int IndexOf(byte[] content, byte[] search)
        {
            for (int i = 0; i <= content.Length - search.Length; i++)
            {
                int j = 0;
                for (; j < search.Length; j++)
                {
                    if (content[i + j] != search[j])
                    {
                        break;
                    }
                }
                if (j == search.Length)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
