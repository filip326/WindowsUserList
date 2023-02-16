using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schulliste
{
    internal class User
    {

        public string username;
        public string plainOutput;
        private Process p = new Process();
        public bool ready = false;

        public User(string username, bool domain = false)
        {
            this.username = username;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.FileName = "net";
            p.StartInfo.Arguments = "user " + username;
            if (domain) p.StartInfo.Arguments += " /domain";
            p.EnableRaisingEvents = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.Exited += Exited;
            p.Start();
        }

        private void Exited(object sender, EventArgs e)
        {
            ready = true;
            plainOutput = p.StandardOutput.ReadToEnd();
            parseData();
        }

        public async Task<bool> waitforReady()
        {
            if (!ready) await Task.Run(() =>
            {
                p.WaitForExit();
            });
            ready = true;
            plainOutput = p.StandardOutput.ReadToEnd();
            parseData();
            return true;
        }

        public void parseData()
        {
            string[] lines = plainOutput.Split('\n');
            string[] tokens =
            {
                "Benutzername", "Vollständiger Name", "Beschreibung", "Benutzerbeschreibung", "Länder-/Regionscode",
                "Konto aktiv", "Konto abgelaufen", "Letztes Setzen des Kennworts", "Kennwort läuft ab", "Kennwort änderbar",
                "Kennwort erforderlich", "Benutzer kann Kennwort ändern", "Erlaubte Arbeitsstationen", "Anmeldeskript",
                "Benutzerprofil", "Basisverzeichnis", "Letzte Anmeldung", "Erlaubte Anmeldezeiten", "Lokale Gruppenmitglieschaften",
                "Globale Gruppenmitglieschaften", "Der Befehl wurde erfolgreich ausgeführt."
            };
            
        }
    }
}
