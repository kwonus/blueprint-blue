namespace Blueprint.Blue
{
    using AVSearch.Interfaces;
    using AVSearch.Model.Features;
    using Pinshot.PEG;
    using System;

    public class QStrongs : FeatureStrongs
    {
        public QStrongs(QFind search, string text, Parsed parse, bool negate) : base(text, negate, search.Settings)
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
    }
}
