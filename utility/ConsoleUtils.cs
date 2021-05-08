using System;

namespace Mate.Utility
{
    public static class ConsoleUtils
    {
        private static bool wasNewLine = false;
        /// <summary>
        /// Writes the value of this object to Console
        /// </summary>
        public static void Write(this object value, ConsoleColor? color = null, ConsoleColor? background = null) {
            Console.ResetColor();
            if (color.HasValue)
                Console.ForegroundColor = color.Value;
            if (background.HasValue)
                Console.BackgroundColor = background.Value;
            Console.Write(value);
            Console.ResetColor();
            wasNewLine = false;
        }
        /// <summary>
        /// Writes the value of this object to Console, then makes a line-break
        /// </summary>
        public static void WriteLine(this object value, ConsoleColor? color = null, ConsoleColor? background = null) {
            Console.ResetColor();
            if (color.HasValue)
                Console.ForegroundColor = color.Value;
            if (background.HasValue)
                Console.BackgroundColor = background.Value;
            Console.WriteLine(value);
            Console.ResetColor();
            wasNewLine = true;
        }
        /// <summary>
        /// Overwrites the last element in console with the value of the object
        /// </summary>
        public static void Overwrite(this object value, ConsoleColor? color = null, ConsoleColor? background = null, int offset = 0) {
            if (wasNewLine) Console.CursorTop--;
            Console.CursorLeft = offset;
            value.Write(color, background);
            wasNewLine = false;
        }
        /// <summary>
        /// Overwrites the last element in console with the value of the object, then adds a new-break
        /// </summary>
        public static void OverwriteLine(this object value, ConsoleColor? color = null, ConsoleColor? background = null, int offset = 0) {
            if (wasNewLine) Console.CursorTop--;
            Console.CursorLeft = offset;
            value.WriteLine(color, background);
            wasNewLine = true;
        }
    }
}