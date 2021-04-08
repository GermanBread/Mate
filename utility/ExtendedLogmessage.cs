using Discord;

namespace Mate.Utility
{
    public struct ExtendedLogMessage
    {
        public static ExtendedLogMessage Empty { get => new ExtendedLogMessage(); }
        public LogMessage Message { get; set; }
        public LogAttributes? Attributes { get; set; }
        public ExtendedLogMessage(LogMessage log, LogAttributes? attributes) {
            Message = log;
            Attributes = attributes;
        }
    }
}