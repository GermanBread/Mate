using Mate.Variables;

namespace Mate.Extra
{
    public sealed class ApiCommands
    {
        public void stop() {
            _ = GlobalVariables.DiscordBot.Quit();
        }
        public void kill() {
            _ = GlobalVariables.DiscordBot.Abort();
        }
    }
}