namespace Blueprint.Blue
{
    using AVSearch.Interfaces;
    using AVSearch.Model.Expressions;
    using AVXLib;
    using Pinshot.PEG;
    using System;
    using System.Data;
    using System.Runtime.CompilerServices;
    using System.Runtime.ExceptionServices;
    using YamlDotNet.Core;
    using static System.Net.Mime.MediaTypeNames;

    public class UpdatableChapterRange : ChapterRange
    {
        public UpdatableChapterRange(Parsed[] range)
        {
            switch (range.Length)
            {
                case 1:
                    this.From = byte.Parse(range[0].text);
                    this.Unto = null;
                    break;

                case 2:
                    this.From = byte.Parse(range[0].text);
                    this.Unto = byte.Parse(range[1].text);
                    break;
            }
        }
    }

    public class QFilter
    {
        public string RawText { get; private set; }
        public string Textual { get; private set; }
        public ChapterRange[] Ranges { get; private set; }

        private QFilter(string textual, string rawText)
        {
            this.RawText = rawText;
            this.Textual = textual;
            this.Ranges = new ChapterRange[0];
            RawText = rawText;
        }
        private QFilter(string textual, string rawText, IEnumerable<ChapterRange> ranges)
        {
            this.RawText = rawText;
            this.Textual = textual;
            this.Ranges = ranges.ToArray();
        }

        public static QFilter? Create(Parsed filter, IDiagnostic diagnostics)
        {
            if (filter.rule.Equals("filter", StringComparison.InvariantCultureIgnoreCase))
            {
                string rawText = filter.text.Trim();
                if (filter.children.Length >= 1)
                {
                    string text = (filter.children[0].rule == "filter_spec") ? filter.children[0].text.Trim() : string.Empty;
                    if (!string.IsNullOrEmpty(text))
                    {
                        if (filter.children.Length == 1)
                            return new QFilter(text, rawText);

                        List<ChapterRange> ranges = new();
                        for (int r = 1; r < filter.children.Length; r++)
                        {
                            Parsed range = filter.children[r];
                            if (range.rule == "chapter_range" && range.children.Length >= 1 && range.children.Length <= 2)
                            {
                                try
                                {
                                    ranges.Add(new UpdatableChapterRange(range.children));
                                }
                                catch
                                {
                                    diagnostics.AddWarning("A scoping filter was provided, but the range could not be parsed; ignoring filter.");
                                }
                            }
                        }
                        if (ranges.Count >= 1)
                        {
                            return new QFilter(text, rawText, ranges);
                        }
                    }
                    else diagnostics.AddWarning("A scoping filter was provided, but it was nonsensical; ignoring filter.");

                    return null;
                }
            }
            diagnostics.AddWarning("A scoping filter was provided, but it could not be parsed; ignoring filter.");
            return null;
        }
    }
}