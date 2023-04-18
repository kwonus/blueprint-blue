namespace Blueprint.Blue
{
    using Pinshot.PEG;
    public class QStrongs : QFeature, IFeature
    {
        public (UInt16 strongs, char lang) Strongs { get; set; }

        public QStrongs(QFind search, string text, Parsed parse) : base(search, text, parse)
        {
            var parts = text.Split(':');
            if (parts.Length == 2)
            {
                try
                {
                    string lang = parts[1].ToUpper().Trim();

                    if (lang.Length == 1)
                    {
                        (UInt16 strongs, char lang) result = (UInt16.Parse(parts[0].Trim()), lang[0]);
                        if (result.lang == 'G' || result.lang == 'H')
                        {
                            this.Strongs = result;
                            return;
                        }
                    }
                }
                catch
                {
                    ;
                }
            }
            this.Strongs = (0, 'X');
        }
        public override IEnumerable<string> AsYaml()
        {
            yield return "- feature: " + this.Text;
            yield return "  strongs: " + this.Strongs.lang + this.Strongs.strongs;
        }
    }
}
