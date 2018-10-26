using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Win32;

// [Replace Notepad With Notepad++ or Any Other Text Editor of Your Choice » Winhelponline](https://www.winhelponline.com/blog/replace-notepad-text-editor-notepad-plus-association/)
// [Notepad2 ― Replacing Windows Notepad](http://www.flos-freeware.ch/doc/notepad2-Replacement.html)

namespace ReplaceNotepad
{
    class Program
    {
        private static readonly string notepadKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\notepad.exe";

        private static void Help()
        {
            Console.WriteLine("ReplaceNotepad");
            Console.WriteLine("A tool that replaces Notepad with another editor");
            Console.WriteLine("using the Registry hack on \"Image File Execution Options\"");
            Console.WriteLine("(administrator rights required)");
            Console.WriteLine("Syntax:");
            Console.WriteLine("ReplaceNotepad /install <Editor Path>");
            Console.WriteLine("    - installs <Editor Path> as replacement for Notepad");
            Console.WriteLine("ReplaceNotepad /uninstall");
            Console.WriteLine("    - uninstalls current replacement for Notepad");
        }

        static void Main(string[] args)
        {
            Console.WriteLine("#args = {0}", args.Length);

            if (args.Length == 0)
            {
                Help();
                return;
            }
            switch (args[0].ToLower())
            {
                case "/install":
                    if (!ProxyInstall(args[1]))
                    {
                        Help();
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Notepad replaced by:\n {0}", args [1]);
                    }
                    break;
                case "/uninstall":
                    if (!ProxyUninstall())
                    {
                        Help();
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Original Notepad restored");
                    }
                    break;
                default:
                    ProxyCall(args);
                    break;
            }
        }

        private static bool ProxyInstall(string editorPath)
        {
            bool installed = false;
            if (File.Exists(editorPath))
            {
                // Get path of current exe
                string proxyPath = string.Format("\"{0}\\{1}\"", GetExecutingDirectoryName(), "ReplaceNotepad.exe");

                // Store the editor path in a file in user directory
                File.WriteAllText(GetUserDirectory() + @"\ReplaceNotepad.ini", editorPath);

                // Create registry key for Notepad replacement
                RegistryKey rkHKLM = Registry.LocalMachine;
                RegistryKey rkNotepad = rkHKLM.CreateSubKey(notepadKey);
                rkNotepad.SetValue("Debugger", proxyPath, RegistryValueKind.String);

                installed = true;
            }
            else
            {
                Console.WriteLine("Editor '{0}' does not exist !");
            }
            return installed;
        }

        private static bool ProxyUninstall()
        {
            bool uninstalled = false;

            // Create registry key for Notepad replacement
            RegistryKey rkHKLM = Registry.LocalMachine;
            rkHKLM.DeleteSubKey(notepadKey);
            uninstalled = true;

            return uninstalled;
        }

        private static bool ProxyCall(string[] args)
        {
            bool called = false;

            // Retrieve the editor path in a file in user directory
            string iniFilePath = GetUserDirectory() + @"\ReplaceNotepad.ini";
            Console.WriteLine("Ini file path: {0}", iniFilePath);
            string editorPath = File.ReadAllText(iniFilePath);
            if (File.Exists(editorPath))
            {
                // Compose argument list shifted by one
                string arguments = "";
                for (int i = 1; i<args.Length; i++)
                {
                    if (arguments.Length > 0)
                        arguments += " ";
                    arguments += args[i];
                }
                arguments = "\"" + arguments + "\"";

                Console.WriteLine("Editor path: {0}", editorPath);

                // Start editor with arguments shifted by one
                Process editorProcess = new Process();
                editorProcess.StartInfo.FileName = editorPath;
                editorProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(editorPath);
                editorProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                editorProcess.StartInfo.Arguments = arguments;
                editorProcess.Start();             

                called = true;
            }
            else
                Console.WriteLine("ERROR: Ini file '{0}' does not exist !", iniFilePath);
            return called;
        }

        public static string GetUserDirectory()
        {
            string path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
            if (Environment.OSVersion.Version.Major >= 6)
            {
                path = Directory.GetParent(path).ToString();
            }
            return path;
        }

        public static string GetExecutingDirectoryName()
        {
            var location = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase);
            string escaped = new FileInfo(location.AbsolutePath).Directory.FullName;
            return Uri.UnescapeDataString(escaped);
        }
    }
}
