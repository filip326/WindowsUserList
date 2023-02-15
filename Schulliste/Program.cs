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
        static void Main(string[] args)
        {
            Console.WriteLine("Domänen oder lokale Benutzer abfragen? ");
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

            string[] daten = new string[] { };

            foreach (string username in usernames)
            {
                try
                {
                    string vorname = BenutzerName(username, selection == 0);
                    string klasse = BenutzerKlasse(username, selection == 0);
                    AddElementsToArray(ref daten, (vorname + ";" + klasse + '/').Split('/'));
                } catch (Exception err)
                {
                    Console.WriteLine(err);
                    continue;
                }
            }
            for (int i = 0; i < daten.Length; i++)
            {
                try
                {
                    daten[i] = daten[i].Trim();
                }
                catch (Exception err)
                {
                    Console.WriteLine(err);
                    continue;
                }
            }
            try
            {
                Console.WriteLine("Pfad zur Output TextDatei: ");

                string pfad = Console.ReadLine() ?? "Error";
                File.WriteAllLines(pfad, daten);
            } catch (Exception err)
            {
                Console.WriteLine(err);
            }
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
    }
}