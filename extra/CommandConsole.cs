using System.Runtime.CompilerServices;
// System
using System;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

// Mate
using Mate.Utility;
using Mate.Variables;

// Discord
using Discord;

namespace Mate.Extra
{
    public class CommandConsole
    {
        public CancellationTokenSource ctx { get; private set; }
        public List<MethodInfo> consoleCommands { get; private set; }
        private List<string> commandHistory = new List<string>();
        private int commandIndex = -1;
        public bool isAlive = false;
        
        /// <summary>
        /// Initializes the console for user input.
        /// </summary>
        /// <param name="commandClass">A static class containing static methods</param>
        public CommandConsole(Type commandClass) {
            consoleCommands = GetStaticMethodsByBlacklist(commandClass);
            Logger.Log(new LogMessage(LogSeverity.Info, "Console", "Press CONTROL + E to start a console session"));
        }

        /// <summary>
        /// Uses the list provided in the GlobalVariables class to filter inbuilt methods
        /// </summary>
        private List<MethodInfo> GetStaticMethodsByBlacklist(Type reflectedType)
         => reflectedType.GetMethods().Where(method
             => !GlobalVariables.GenericMethodNames.Contains(method.Name) & method.IsStatic).ToList();

        /// <summary>
        /// Starts the console service. DO NOT AWAIT
        /// </summary>
        /// <returns>TRUE if successful</returns>
        public async Task<bool> Start() {
            await Logger.Log(new LogMessage(LogSeverity.Info, "Console", "Console session started"));
            "To get started with using the command-line, type \"help\" and press ENTER".WriteLine(ConsoleColor.Green);

            // Get a new token source, because this console can be restarted
            ctx = new CancellationTokenSource();
            
            isAlive = true;

            // Only allow errors
            if (Logger.MinimumPriority == 99) Logger.MinimumPriority = 1;

            // Initialize the input field
            DrawInput("");

            string input = "";
            while (!ctx.IsCancellationRequested) {
                ConsoleKeyInfo key = Console.ReadKey();

                // Cancellation has been requested = exit
                if (ctx.IsCancellationRequested) return false;
                
                // If it's enter, handle the command typed
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        HandleCommand(input);
                        input = "";
                        break;
                    case ConsoleKey.Backspace:
                        input = "";
                        break;
                    case ConsoleKey.UpArrow:
                        commandIndex++;
                        break;
                    case ConsoleKey.DownArrow:
                        commandIndex--;
                        break;
                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.RightArrow:
                        break;
                    case ConsoleKey.Tab:
                        SuggestCompletions(input);
                        break;
                    default:
                        input += key.KeyChar;
                        break;
                }

                // Handy shortcut for clearing the console
                if (key.Modifiers == ConsoleModifiers.Control) {
                    switch (key.Key)
                    {
                        case ConsoleKey.L:
                            Console.Clear();
                            input = "";
                            break;
                        
                        case ConsoleKey.D:
                            HandleCommand("logout");
                            break;
                    }
                }

                commandIndex = Math.Clamp(commandIndex, -1, commandHistory.Count - 1);
                if (commandIndex >= 0) input = commandHistory[commandIndex];
                DrawInput(input);
            }

            return true;
        }

        /// <summary>
        /// Draw the use input to console
        /// </summary>
        private void DrawInput(string input) {          
            if (ctx.IsCancellationRequested) return;
            
            int leftMargin = 2;
            
            Console.CursorLeft = 0;

            "# ".Write(ConsoleColor.Red);
            input.Write(consoleCommands.Any(command
                => command.Name.ToLower() == input.ToLower()) ? ConsoleColor.White : ConsoleColor.DarkGray);
            new string(' ', Console.WindowWidth - input.Length - leftMargin).WriteLine();
            
            Console.CursorTop--;
            Console.CursorLeft = input.Length + leftMargin;
        }

        private void SuggestCompletions(string input) {
            Console.CursorTop++;
            int cLeft = Console.CursorLeft;
            Console.CursorLeft = 0;

            if (ctx.IsCancellationRequested) return;
            
            "Possible completions:".WriteLine(ConsoleColor.White);

            var matches = consoleCommands.Where(command => command.Name.ToLower().Contains(input.ToLower()));
            
            // Show each possible completion
            foreach (var command in matches) {
                // FYI .Write() is a custom extension
                command.Name.Write(ConsoleColor.Green);
                new string(' ', Console.WindowWidth - command.Name.Length).WriteLine();
            }

            // Move the cursor back
            Console.CursorLeft = cLeft;
        }

        /// <summary>
        /// Executes commands
        /// </summary>
        /// <param name="input">The command to execute</param>
        /// <returns>TRUE if execution was successful</returns>
        private bool HandleCommand(string input) {
            // Save this command in the history
            commandHistory.Insert(0, input);
            commandIndex = -1;
            
            // Move the cursor down
            Console.CursorTop++;
            Console.CursorLeft = 0;

            // Check if there are matches
            if (consoleCommands.Any(x => x.Name.ToLower().Equals(input.ToLower()))) {
                Logger.Log(new LogMessage(LogSeverity.Info, "Console", $"Executing \"{input}\""));
                try {
                    MethodInfo command = consoleCommands.First(method
                     => method.Name.ToLower() == input.ToLower());
                    object[] commandParameters = null;

                    if (command.GetParameters().Any(parameter => parameter.ParameterType == this.GetType()))
                        commandParameters = new object[] {
                            this /* this console */
                        };
                    
                    command.Invoke(this, commandParameters);
                    return true;
                } catch (Exception ex) {
                    Logger.Log(new LogMessage(LogSeverity.Error, "Console", "Failed to invoke command", ex));
                    return false;
                }
            } else {
                // Don't clog up the console when nothing has been typed
                if (!string.IsNullOrEmpty(input)) "No matching command found".WriteLine(ConsoleColor.Yellow);
                return false;
            }
        }
        
        /// <summary>
        /// Stop the console interface
        /// </summary>
        /// <returns>TRUE is succesful</returns>
        public bool Close() {
            if (Logger.MinimumPriority == 1 || Logger.MinimumPriority == -1) Logger.MinimumPriority = 99;
            
            Logger.Log(new LogMessage(LogSeverity.Info, "Console", "Stopping"));
            
            // Check if the console has been instantiated
            if (ctx != null) ctx.Cancel();

            isAlive = false;

            Logger.Log(new LogMessage(LogSeverity.Info, "Console", "Press CONTROL + E to start a new console session"));
            
            return true;
        }
    }
}