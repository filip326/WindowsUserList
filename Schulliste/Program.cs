using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Schulliste
{
    class Program
    {

        static async Task Main(string[] args)
        {
            Console.WriteLine("Domänen oder lokale Benutzer abfragen?");
            Console.WriteLine("* Domänenbenutzer");
            Console.WriteLine("  lokale Benutzer");

            int selection = 0;

            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey();
                selection = (key.Key == ConsoleKey.UpArrow) ? 0 : selection;
                selection = (key.Key == ConsoleKey.DownArrow) ? 1 : selection;
                Console.CursorTop -= 2;
                Console.WriteLine((selection == 0) ? "* Domänenbenutzer" : "  Domänenbenutzer");
                Console.WriteLine((selection == 1) ? "* lokale Benutzer" : "  lokale Benutzer");

            } while (key.Key != ConsoleKey.Enter);

            string[] usernames = Benutzernamen(selection == 0);

            User[] users = usernames.Select(u => new User(u, selection == 0)).ToArray();

            Console.WriteLine("Waiting 10 sec. for data parsing:\n");

            for (int i = 1; i<= 10; i++)
            {
                Console.CursorTop--;
                Console.WriteLine(loadingBar(i, 10));
                Thread.Sleep(1000);
            }

            Console.WriteLine("Parsing data... This might take a while...\n");

            for (int i = 0; i < users.Length; i++)
            {
                Console.CursorTop--;
                Console.Write(new String(' ', Console.WindowWidth));
                Console.CursorLeft = 0;
                Console.WriteLine($"Parsing {users[i].username}");
                Console.Write(new String(' ', Console.WindowWidth));
                Console.CursorLeft = 0;
                Console.WriteLine(loadingBar(i, users.Length));
                Console.CursorTop-= 2;
                Console.CursorLeft = $"Parsing {users[i].username}".Length;
                await users[i].waitforReady();
                Thread.Sleep(500);
                Console.WriteLine($" ... Done");
                Console.WriteLine(loadingBar(i, users.Length));
            }

            Console.CursorTop--;
            Console.WriteLine(loadingBar(users.Length, users.Length));


        }

        static string BenutzerKlasse(string username, bool domain) // Gibt die Klasse des Benutzers zurück
        {
            string output = runCmd("net", "user " + username + (domain ? " /domain" : ""));
            // Console.WriteLine(output);

            string[] outputLines = output.Split("\n");
            // Entferne alle Leerzeichen vorne und hinten von jedem Eintrag
            for (int i = 0; i < outputLines.Length; i++)
            {
                outputLines[i] = outputLines[i].Trim();
            }

            // Überspringe alle leeren Einträge
            outputLines = outputLines.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            outputLines = outputLines.Skip(19).Take(outputLines.Length - (outputLines.Length - 1)).ToArray();

            string userinfo = new string("");
            string[] userinfo_split = outputLines[0].Split(' ');
            for (int i = 0; i < userinfo_split.Length; i++)
            {
                userinfo_split[i] = userinfo_split[i].Trim();
                userinfo += userinfo_split[i];
            }

            if (userinfo.Length > "Globale Gruppenmitgliedschaften".ToString().Length)
            {
                userinfo = userinfo.Substring("Globale Gruppenmitgliedschaften".ToString().Length);
            }
            else
            {
                userinfo = "Keine Klasse angegeben";
            }
            // ToDo
            // Klasse extrahieren.
            return userinfo;
        }

        static string BenutzerName(string username, bool domain) // Gibt den Vollständigen Namen des Benutzers zurück
        {
            string output = runCmd("net", "user " + username + (domain ? " /domain" : ""));
            // Console.WriteLine(output);

            string[] outputLines = output.Split("\n");
            // Entferne alle Leerzeichen vorne und hinten von jedem Eintrag
            for (int i = 0; i < outputLines.Length; i++)
            {
                outputLines[i] = outputLines[i].Trim();
            }

            // Überspringe alle leeren Einträge
            outputLines = outputLines.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            outputLines = outputLines.Skip(1).Take(outputLines.Length - (outputLines.Length - 1)).ToArray();

            string userinfo = new string("");

            string[] userinfo_split = outputLines[0].Split(' ');
            for (int i = 0; i < userinfo_split.Length; i++)
            {
                userinfo_split[i] = userinfo_split[i].Trim();
                userinfo += userinfo_split[i];
            }

            if (userinfo.Length > "VollständigerName".ToString().Length)
            {
                userinfo = userinfo.Substring("VollständigerName".ToString().Length);
            }
            else
            {
                userinfo = "Kein Name angegeben";
            }

            return userinfo;
        }

        static string[] Benutzernamen(bool domain)
        {

            string output = runCmd("net", "user" + (domain ? " /domain" : ""));

            // Console.WriteLine(output);

            string[] outputLines = output.Split("\n");

            // Entferne alle Leerzeichen vorne und hinten von jedem Eintrag
            for (int i = 0; i < outputLines.Length; i++)
            {
                outputLines[i] = outputLines[i].Trim();
            }

            if (outputLines.Length < 4)
            {
                return new string[] { };
            }

            // Überspringe alle leeren Einträge
            outputLines = outputLines.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            outputLines = outputLines.Skip(2).Take(outputLines.Length - 3).ToArray();

            string[] usernames = { };


            foreach (string line in outputLines)
            {
                AddElementsToArray(ref usernames, line.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            }

            return usernames;
        }
        static public string runCmd(string cmd, string cmdArg)
        {
            Console.WriteLine("Running " + cmd + " " + cmdArg);
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = cmd;
            p.StartInfo.Arguments = cmdArg;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            if (p.ExitCode == 0) return output;
            else return "\n\n\n\n\n";
        }
        static void AddElementsToArray(ref string[] originalArray, string[] elementsToAdd)
        {
            int originalLength = originalArray.Length;
            Array.Resize(ref originalArray, originalLength + elementsToAdd.Length);
            Array.Copy(elementsToAdd, 0, originalArray, originalLength, elementsToAdd.Length);
        }

        static string loadingBar(double progress, double goal=100)
        {
            string progressString = " " + progress.ToString() + " / " + goal.ToString() + " ";
            int width = Console.WindowWidth - 6 - progressString.Length;
            int filledPart = Convert.ToInt32(progress / goal * width);
            return " [" + new String('#', filledPart) + new String('_', width - filledPart) + "] " + progressString;
        }
    }
}