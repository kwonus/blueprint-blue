namespace BlueprintBlue
{
    public class QHistory
    {
        public required uint Sequence { get; set; }
        public required Int64 Time   { get; set; }
        public string Statement       { get; set; }

        public DateTime GetDateTime()
        {
            DateTimeOffset offset = DateTimeOffset.FromUnixTimeMilliseconds(this.Time);
            return offset.DateTime;
        }
    }
}
