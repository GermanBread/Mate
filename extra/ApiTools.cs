// System
using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading;
using System.Text.Json;
using System.Reflection;

// Mate
using Mate.Variables;

namespace Mate.Extra
{
    public static class ApiTools
    {
        public static string HandleRequest(this HttpListenerContext context) {
            string methodName = Path.GetFileNameWithoutExtension(context.Request.Url.LocalPath);
            /*MethodBase[] commands = typeof(ApiCommands).GetMethods().ToArray();
            MethodBase command = commands.First(x => x.Name.Equals(methodName));
            command.Invoke(context.GetType(), null);*/ // Welp, it was an attempt, maybe I'll try later...
            // Old reliable
            switch (methodName)
            {
                case "stop":
                    _ = GlobalVariables.DiscordBot.Quit();
                    break;
                case "kill":
                    _ = GlobalVariables.DiscordBot.Abort();
                    break;
                case "logs":
                    return JsonSerializer.Serialize(GlobalVariables.Logs);
                default:
                    throw new InvalidOperationException("Invalid request");
            }
            return "";
        }
    }
}