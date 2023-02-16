using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace Schulliste
{
    class Program
    {
        static void Main(string[] args)
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

            Benutzernamen(selection == 0);
        }

        static string[] Benutzernamen(bool domain)
        {

            string output = runCmd("net", "user" + (domain ? " /domain" : ""));

            Console.WriteLine(output);

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


            Console.WriteLine("Benutzernamen: " + string.Join(", ", usernames));

            return new string[] {};
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