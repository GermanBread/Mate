namespace Mate.Utility
{
    public struct LogAttributes
    {
        public bool IsSupressed { get; set; }
        public bool IsSilent { get; set; }
        public LogAttributes(bool silent, bool supressed) {
            IsSilent = silent;
            IsSupressed = supressed;
        }
    }
}