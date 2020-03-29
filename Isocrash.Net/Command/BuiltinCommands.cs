﻿using System;
using System.Diagnostics;
using System.Timers;
using Isocrash.Net;

namespace Isocrash.Net
{
    public class BuiltinCommands
    {
        [RegisterCommand("help", "Show all commands available", "Use /help [command] to show command help (i.e. /help say)")]
        public static bool Help(Player sender, params string[] args)
        {
            string msg = "";
            // Show command help
            if (args.Length > 0)
            {
                Command cmd = Command.Get(args[0]);
                    
                // If command exists
                if (cmd != null)
                {
                    msg = cmd.Name + ": " + cmd.Help;
                }

                // If command does not exist
                else
                {
                    msg = $"Cannot help on command \"/{args[0]}\", it does not exits.";
                }
            }

            // If args, show all commands
            else
            {
                int i = 0;
                foreach (Command cmd in Command.Commands)
                {
                    msg += cmd.Name + ": " + cmd.Description;
                    if (i != Command.Commands.Count - 1)
                    {
                        msg += "\n";
                    }
                    
                    i++;
                }
            }
            
            Console.WriteLine(msg);

            return true;
        }
        [RegisterCommand("stop", "Shutdown the server", "Use /stop to shutdown the server")]
        public static bool Stop(Player sender, params string[] args)
        {
            Server.Shutdown();
            return true;
        }
        [RegisterCommand("say", "Say a message in the chat", "Use /say [message] to say something")]
        public static bool Say(Player sender, params string[] args)
        {
            string msg = "[Server]";

            // Error, nothing to say.
            if (args.Length < 1)
            {
                return false;
            }

            for (int i = 0; i < args.Length; i++)
            {
                msg += " " + args[i];
            }

            NetMessage nm = new NetMessage(msg, ConsoleColor.Magenta);
            nm.WriteConsole();
            //Server.SendMessage(nm);
            return true;
        }
        [RegisterCommand("clear", "Clears the console buffer", "Use /clear to clear the console buffer")]
        public static bool Clear(Player sender, params string[] args)
        {
            try
            {
                Console.Clear();
                return true;
            }

            catch
            {
                return false;
            }
        }
        [RegisterCommand("plugins", "Shows all the plugins loaded", "Use /plugin to show loaded plugins list")]
        public static bool Plugin(Player sender, params string[] args)
        {
            // If plugin name in args 
            /*if (args.Length > 0)
            {
                Plugin plugin = Server.Plugin.Get(args[0]);

                if (plugin != null)
                {
                    Console.WriteLine(FileVersionInfo.GetVersionInfo(plugin.CorrespondingAssembly.Location).FileVersion);
                    return true;
                }

                else
                {
                    return false;
                }
            }

            // If none, display all plugins
            else
            {*/
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            if (Net.Plugin.Plugins.Count > 0)
            {
                
                string msg = "Displaying " + Net.Plugin.Plugins.Count + " loaded plugins:\n";

                foreach (Plugin plugin in Net.Plugin.Plugins)
                {
                    msg += plugin.Name + " " +
                           FileVersionInfo.GetVersionInfo(plugin.CorrespondingAssembly.Location).FileVersion + " " +
                           Environment
                               .NewLine;
                    
                }

                Console.WriteLine(msg);
                
            }

            else
            {
                Console.WriteLine("No plugin loaded");
            }
            
            Console.ResetColor();

            return true;
        }
        [RegisterCommand("kick", "Kicks a player", "Use /kick [player] to kick a player out of the server.")]
        public static bool Kick(Player sender, params string[] args)
        {
            string playerName = args[0];
            
            Player player = null;
            foreach (Player p in Server.ConnectedPlayers)
            {
                if (p.Nickname == playerName)
                {
                    player = p;
                    break;
                }
            }

            if (player != null)
            {
                NetMessage kickmsg = new NetMessage("Kicked from server by admin.", ConsoleColor.Red);
                NetObject.SendContent(kickmsg, player.TCP.Client);

                player.TCP.Close();
                Net.Server.Log("Player kicked");
                return true;
            }
            else
            {
                Net.Server.Log("Player not found.");
                return false;
            }
        }
    }
}