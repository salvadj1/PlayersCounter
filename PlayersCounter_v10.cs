using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Fougerite;
using System.IO;


namespace PlayersCounter
{
    public class PlayersCounter_v10 : Fougerite.Module
    {
        public static System.IO.StreamWriter file;
        public static string rutalog;
        public static IniParser Settings;
        private int totaljugadores = 0;
        public int MinutesToLog = 5;
        
        public override void Initialize()
        {
            if (!File.Exists(Path.Combine(ModuleFolder, "Log.log"))) { File.Create(Path.Combine(ModuleFolder, "Log.log")).Dispose(); }
            if (!File.Exists(Path.Combine(ModuleFolder, "Settings.ini")))
            {
                File.Create(Path.Combine(ModuleFolder, "Settings.ini")).Dispose();
                Settings = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
                Settings.AddSetting("Settings", "MinutesToLog", "5");
                Settings.Save();
            }
            else
            {
                Settings = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
            }
            loadconfig();
       
            Fougerite.Hooks.OnServerInit += OnServerInit;
            Fougerite.Hooks.OnCommand += OnCommand;
        }
        public override void DeInitialize()
        {
            Fougerite.Hooks.OnServerInit -= OnServerInit;
            Fougerite.Hooks.OnCommand -= OnCommand;
        }
        public void OnServerInit()
        {
            CreateParallelTimer(60000, null).Start();
        }
        private void loadconfig()
        {
            MinutesToLog = int.Parse(Settings.GetSetting("Settings", "MinutesToLog"));
            rutalog = Path.Combine(ModuleFolder, "Log.log");
            return;
        }

        public TimedEvent CreateParallelTimer(int timeoutDelay, Dictionary<string, object> args)
        {
            TimedEvent timedEvent = new TimedEvent(timeoutDelay);
            timedEvent.Args = args;
            timedEvent.OnFire += Callback;
            return timedEvent;
        }
        public void Callback(TimedEvent e)
        {
            var dict = e.Args;
            e.Kill();
            totaljugadores = Server.GetServer().Players.Count();
            string line = "TOTAL PLAYERS: " + totaljugadores + " [" + DateTime.Now + "]";
            file = new System.IO.StreamWriter(rutalog, true);
            file.WriteLine(line);
            file.Close();
            CreateParallelTimer(MinutesToLog * 60000, null).Start();
        }
        public void OnCommand(Fougerite.Player player, string cmd, string[] args)
        {
            if (cmd == "pclogtime")
            {
                if (!player.Admin)
                {
                    player.MessageFrom("PlayersCounter", "Only Admins can use this command");
                }
                else
                {
                    if (args.Length == 0)
                    {
                        player.MessageFrom("PlayersCounter", "You must choose the frequency in minutes you want to generate a log");
                        player.MessageFrom("PlayersCounter", "Example: /pclogtime 10, will record every 10min the number of active players");
                        return;
                    }
                    
                    string s = string.Join(" ", args);
                    Settings.AddSetting("Settings", "MinutesToLog", s);
                    Settings.Save();
                    player.MessageFrom("PlayersCounter", "Done! , Every " + s + " minute will be recorded in the log a player count.");
                    loadconfig();
                }
            }
        }
        public override string Name
        {
            get { return "PlayersCounter"; }
        }

        public override string Author
        {
            get { return "Salva/juli"; }
        }

        public override string Description
        {
            get { return "PlayersCounter"; }
        }

        public override Version Version
        {
            get { return new Version("1.0"); }
        }
    }
}
