namespace Blueprint.Blue
{
    using AVSearch.Interfaces;
    using AVSearch.Model.Expressions;
    using AVSearch.Model.Results;
    using AVXLib;
    using AVXLib.Framework;
    using AVXLib.Memory;
    using Blueprint.Blue;
    using Blueprint.Model.Implicit;
    using Pinshot.PEG;
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;
    using YamlDotNet.Core.Tokens;
    using YamlDotNet.Serialization;
    using static AVXLib.Framework.Numerics;
    using static Blueprint.Model.Implicit.QFormat;
    using static System.Net.Mime.MediaTypeNames;

    public enum FileCreateMode
    {
        StreamingWithContext = -2,
        Streaming = -1,
        CreateNew = 0,
        Overwrite = 1,
        Append = 2
    }
    public record WordFeatures // hijacked from WordRendering class
    {
        private bool _ignore; // needed for deserialization
        public UInt32 WordKey;
        public byte Wc;
//      public BCVW Coordinates;
        public PNPOS PnPos;
        public string NuPos;
        public string Text;   // KJV
        public string Modern; // AVX
        public bool Modernized { get => !this.Text.Equals(this.Modern, StringComparison.InvariantCultureIgnoreCase); set => this._ignore = value; }
        public byte Punctuation;
        public Dictionary<UInt32, string>? Triggers;

        public WordFeatures()
        {
            this.WordKey = 0;
//          this.Coordinates = new();
            this.Wc = 0;
            this.Text = string.Empty;
            this.Modern = string.Empty;
            this.Punctuation = 0;
            this.PnPos = new();
            this.NuPos = string.Empty;
            this.Triggers = null;
        }
        public WordFeatures(AVXLib.Memory.Written writ, Dictionary<UInt32, QueryMatch>? matches = null)
        {
            this.WordKey = writ.WordKey;
//          this.Coordinates = writ.BCVWc;
            this.Wc = writ.BCVWc.WC;
            this.Text = ObjectTable.AVXObjects.lexicon.GetLexDisplay(writ.WordKey);
            this.Modern = ObjectTable.AVXObjects.lexicon.GetLexModern(writ.WordKey, writ.Lemma);
            this.Punctuation = writ.Punctuation;
            this.PnPos = QPartOfSpeech.GetPnPos(writ.pnPOS12);
            this.NuPos = FiveBitEncoding.DecodePOS(writ.POS32);

            if (matches != null && matches.Count > 0)
            {
                var spans = matches.Where(tag => writ.BCVWc >= tag.Value.Start && writ.BCVWc <= tag.Value.Until);

                foreach (var span in spans)
                {
                    foreach (QueryTag tag in span.Value.Highlights)
                    {
                        if (tag.Coordinates == writ.BCVWc)
                        {
                            if (this.Triggers == null)
                            {
                                this.Triggers = new();
                                this.Triggers[1] = tag.Feature.Text;
                            }
                            else if (!this.Triggers.ContainsKey(span.Key))
                            {
                                UInt32 key = (UInt32)(this.Triggers.Count + 1);
                                this.Triggers[key] = tag.Feature.Text;
                            }
                        }
                    }
                }
            }
        }
    }
    public abstract class ExportDirective : Dictionary<byte, Dictionary<byte, Dictionary<byte, List<WordFeatures>>>> // b.c.v: other members of this class are very opaque for serialization purposes
    {
        protected ReadOnlySpan<Book> Books {  get => ObjectTable.AVXObjects.Mem.Book.Slice(0, 67).Span; }
        protected QContext? Context { get; set; }
        protected QSettings Settings { get => this.Context != null && this.Context.Statement.Commands != null && this.Context.Statement.Commands.SelectionCriteria != null ? this.Context.Statement.Commands.SelectionCriteria.Settings : new(); }
        protected string FileSpec { get; set; }
        public FileCreateMode CreationMode { get; protected set; }
        public QFormatVal ContentFormat { get; protected set; }
        protected virtual bool UsesAugmentation { get => false; }
        public bool ScopeOnlyExport { get => this.Context != null && this.Context.Statement.Commands != null && this.Context.Statement.Commands.SelectionCriteria != null && this.Context.Statement.Commands.SelectionCriteria.SearchExpression != null
                ? this.Context.Statement.Commands.SelectionCriteria.SearchExpression.Expression == null
                : true; }

        public FileCreateMode GetCreationMode() => this.CreationMode;
        public QFormatVal GetContentFormat() => this.ContentFormat;
        protected bool? IsJson()
        {
            if (File.Exists(this.FileSpec))
            {
                try
                {
                    using (StreamReader file = File.OpenText(this.FileSpec))
                    {
                        for (string? line = file.ReadLine(); line != null; line = file.ReadLine())
                        {
                            string chars = line.Trim();
                            if (chars.Length > 0)
                            {
                                return chars[0] == '{';
                            }
                        }
                    }
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }
        protected bool IsStreamingMode()
        {
            return (this.CreationMode == FileCreateMode.Streaming) || (this.CreationMode == FileCreateMode.StreamingWithContext);
        }
        protected bool IsContextIncluded()
        {
            return (this.CreationMode == FileCreateMode.StreamingWithContext);
        }
        public DirectiveResultType Retrieve()
        {
            return UsesAugmentation ? Deserialize() : ValidateTextFile();
        }

        private DirectiveResultType ValidateTextFile()
        {
            if (this.IsStreamingMode())
                return DirectiveResultType.ExportReady;
            if (Directory.Exists(Path.GetDirectoryName(this.FileSpec))
            && (this.CreationMode != FileCreateMode.CreateNew || !File.Exists(this.FileSpec)))
                return DirectiveResultType.ExportReady;
            return DirectiveResultType.ExportNotReady;
        }
        private IEnumerable<byte> GetAllVerses(byte verseCnt)
        {
            for (byte v = 1; v <= verseCnt; v++)
            {
                yield return v;
            }
        }
        public void Merge(IEnumerable<ScopingFilter> filters)
        {
            var results = this.Context != null ? this.Context.Statement.Commands != null ? this.Context.Statement.Commands.Results : null : null;
            if (this.ScopeOnlyExport)
            {
                foreach (ScopingFilter scope in from bk in filters orderby bk.Book select bk)
                {
                    byte b = scope.Book;

                    if (this.ContainsKey(b))
                    {
                        foreach (byte c in from ch in scope.Chapters orderby ch select ch)
                        {
                            if (!this[b].ContainsKey(c))
                            {
                                this[b][c] = new();  // non-augmented will never enter into this block
                            }
                        }    
                    }
                    else
                    {
                        this[b] = new();
                        foreach (byte c in from ch in scope.Chapters orderby ch select ch)
                        {
                            this[b][c] = new();  // this is all that we need for non-augmented
                        }
                    }
                    if (UsesAugmentation) // we need to populate HashMap now // instead of deferring 
                    {
                        var BOOK = ObjectTable.AVXObjects.Mem.Book.Slice((int)b, 1).Span[0];
                        var CHAP = ObjectTable.AVXObjects.Mem.Chapter.Slice(BOOK.chapterIdx, BOOK.chapterCnt).Span;

                        foreach (byte c in from ch in this[b].Keys orderby ch select ch)
                        {
                            var chapter = CHAP[c-1];
                            var writ = ObjectTable.AVXObjects.Mem.Written.Slice((int)chapter.writIdx, (int)chapter.writCnt).Span;

                            List<WordFeatures> words = new();
                            for (int w = 0; w < (int)chapter.writCnt; w++)
                            {
                                WordFeatures word = new WordFeatures(writ[w]);
                                words.Add(word);

                                byte wc = writ[w].BCVWc.WC;

                                if (wc == 1) // 1 means last word in the verse
                                {
                                    byte v = writ[w].BCVWc.V;
                                    this[b][c][v] = words;
                                }
                            }
                        }
                    }
                }
            }
            else if (results != null && results.Expression != null)
            {
                foreach (QueryBook book in from bk in results.Expression.Books where bk.Value.Matches.Count > 0 orderby bk.Key select bk.Value)
                {
                    byte b = book.BookNum;
                    if (!this.ContainsKey(b))
                        this[b] = new();
                    var BOOK = ObjectTable.AVXObjects.Mem.Book.Slice((int)b, 1).Span[0];
                    var CHAP = ObjectTable.AVXObjects.Mem.Chapter.Slice(BOOK.chapterIdx, BOOK.chapterCnt).Span;
                    var writ = ObjectTable.AVXObjects.Mem.Written.Slice((int)BOOK.writIdx, (int)BOOK.writCnt).Span;

                    var filterList = filters.ToList();

                    bool validatedContext = this.IsContextIncluded() && (filterList.Count == 1) && (filterList[0].Book == b) && (filterList[0].Chapters.Count == 1);
                    if (validatedContext)
                    {
                        byte c = filterList[0].Chapters.First();
                        var chap = CHAP[c-1];
                        byte verseCnt = chap.verseCnt;

                        var chapter = CHAP[c-1];

                        List<WordFeatures> words = new();
                        for (int w = chapter.writIdx; w < (int)chapter.writCnt; w++)
                        {
                            WordFeatures word = new WordFeatures(writ[w], book.Matches);
                            words.Add(word);

                            byte wc = writ[w].BCVWc.WC;

                            if (wc == 1) // 1 means last word in the verse
                            {
                                byte v = writ[w].BCVWc.V;
                                this[b][c][v] = words;
                            }
                        }

                    }
                    else if (!this.IsContextIncluded())
                    {
                        foreach (var match in from m in book.Matches.Values orderby m.Start.V select m)
                        {
                            byte v;
                            byte c;

                            for (int n = 1; n <= 2; n++)
                            {
                                if (n == 1)
                                {
                                    v = match.Start.V;
                                    c = match.Start.C;
                                }
                                else
                                {
                                    v = match.Until.V;
                                    c = match.Until.C;
                                }
                                if (!this[b].ContainsKey(c))
                                    this[b][c] = new();

                                if (!this[b][c].ContainsKey(v))
                                {
                                    this[b][c][v] = new();

                                    UInt32 w = CHAP[c - 1].writIdx;
                                    for (/**/; writ[(int)w].BCVWc.V < v; w++)
                                        ;
                                    for (/**/; writ[(int)w].BCVWc.V == v; w++)
                                    {
                                        WordFeatures word = new(writ[(int)w], book.Matches);
                                        this[b][c][v].Add(word);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private static char[] VerseDelims = [ ' ', '|', ':' ];
        private DirectiveResultType Deserialize()    // this is used ONLY for Yaml and Json formats ... note: formats can be dynamically migrated
        {
            DirectiveResultType result = DirectiveResultType.ExportNotReady;
            if (!UsesAugmentation)
            {
                return DirectiveResultType.ExportNotReady;
            }
            if (Path.Exists(this.FileSpec) && (this.CreationMode == FileCreateMode.CreateNew))
            {
                return DirectiveResultType.ExportNotReady;
            }
            string? line = null;
            result = DirectiveResultType.ExportReady;
            if (Path.Exists(this.FileSpec) && (this.CreationMode == FileCreateMode.Append))
            {
                bool? json = this.IsJson();
                if (json.HasValue)
                {
                    if (json.Value)
                    {
                        return DirectiveResultType.ExportFailed; // JSON deserialization is not implemented yet.
                    }
                    try
                    {
                        using (TextReader reader = File.OpenText(this.FileSpec))
                        {
                            if (json.Value)
                            {
                                ;// entries = System.Text.Json.JsonSerializer.Deserialize<Dictionary<byte, Dictionary<byte, Dictionary<byte, List<WordFeatures>>>>>(file.BaseStream);
                            }
                            else // yaml
                            {
                                var deserializer = new DeserializerBuilder();
                                var builder = deserializer.Build();

                                StringBuilder content = new();

                                string coordinates = string.Empty;
                                do
                                {
                                    line = reader.ReadLine();

                                    if ((line != null) && line.Length > 0 && (line[0] == ' ' || line[0] == '-'))
                                    {
                                        content.AppendLine(line);
                                    }
                                    else
                                    {
                                        if (content.Length > 0)
                                        {
                                            string yaml = content.ToString();
                                            var verse = builder.Deserialize<List<WordFeatures>>(yaml);
                                            content.Clear();

                                            if (verse.Count > 0)
                                            {
                                                byte b = 0;
                                                byte c = 0;
                                                byte v = 0;
                                                // This should work, but BCVWc is broken
                                                /*
                                                byte b = verse[0].Coordinates.B;
                                                byte c = verse[0].Coordinates.C;
                                                byte v = verse[0].Coordinates.V;
                                                */
                                                string[] parts = coordinates.Split(VerseDelims, StringSplitOptions.RemoveEmptyEntries);
                                                if (parts.Length == 3)
                                                {
                                                    b = GetBookNum(parts[0]);
                                                    c = byte.Parse(parts[1]);
                                                    v = byte.Parse(parts[2]);
                                                }
                                                if (!this.ContainsKey(b))
                                                    this[b] = new();
                                                if (!this[b].ContainsKey(c))
                                                    this[b][c] = new();
                                                this[b][c][v] = verse;
                                            }
                                        }
                                        coordinates = line != null ? line : string.Empty;
                                    }
                                }   while (line != null);
                            }
                        }
                        result = DirectiveResultType.ExportReady;
                    }
                    catch (Exception ex)
                    {
                        result = DirectiveResultType.ExportFailed;
                    }
                }
            }
            return result;
        }
        public abstract DirectiveResultType Update();

        protected TextWriter CreateFile()   // DO NOT USE FOR YAML or JSON !!!!
        {
            if (Directory.Exists(Path.GetDirectoryName(this.FileSpec)))
            {
                if (this.CreationMode != FileCreateMode.CreateNew || !File.Exists(this.FileSpec))
                {
                    TextWriter writer = this.CreationMode == FileCreateMode.Append
                            ? File.AppendText(this.FileSpec)
                            : File.CreateText(this.FileSpec);
                    return writer;
                }
            }
            throw new Exception("Unable to open file; supplied directory does not exist.");
        }

        protected ExportDirective(): base() // for deserialization
        {
            this.Context = null;
            this.FileSpec = string.Empty;
            this.CreationMode = FileCreateMode.CreateNew;
            this.ContentFormat = QFormatVal.TEXT;
        }
        protected ExportDirective(QContext env, string spec, QFormatVal format, FileCreateMode mode): base()
        {
            this.Context = env;
            this.FileSpec = spec;
            this.CreationMode = mode;
            this.ContentFormat = format;
        }
        public static ExportDirective? Create(QContext env, string text, Parsed[] args, QSelectionCriteria selection)
        {
            if (args.Length == 1 && args[0].children.Length == 1)
            {
                if (args[0].children[0].rule.Equals("filename", StringComparison.InvariantCultureIgnoreCase))
                {
                    string spec = args[0].children[0].text;

                    FileCreateMode mode = FileCreateMode.CreateNew;
                    QFormat format = selection.Settings.Format;

                    if (args[0].rule.Equals("overwrite", StringComparison.InvariantCultureIgnoreCase))
                        mode = FileCreateMode.Overwrite;
                    else if (args[0].rule.Equals("append", StringComparison.InvariantCultureIgnoreCase))
                        mode = FileCreateMode.Append;

                    switch (format.Value)
                    {
//                      case QFormatVal.JSON: return new ExportJson(env, spec, mode);
                        case QFormatVal.YAML: return new ExportYaml(env, spec, mode);
                        case QFormatVal.TEXT: return new ExportText(env, spec, mode);
                        case QFormatVal.HTML: return new ExportHtml(env, spec, mode);
                        case QFormatVal.MD:   return new ExportMarkdown(env, spec, mode);
                    }
                }
                else if (args[0].rule.StartsWith("stream", StringComparison.InvariantCultureIgnoreCase))
                {
                    string format = args[0].children[0].rule;

                    FileCreateMode mode = args[0].rule.Equals("stream", StringComparison.InvariantCultureIgnoreCase)
                        ? FileCreateMode.Streaming
                        : FileCreateMode.StreamingWithContext;

                        switch (format.ToLower())
                    {
                        case "yaml":     return new ExportYaml(env, format, mode);
                        case "textual":  return new ExportText(env, format, mode);
                        case "html":     return new ExportHtml(env, format, mode);
                        case "markdown": return new ExportMarkdown(env, format, mode);
                    }
                }
            }
            return null;
        }
        protected static void ConditionallyMakePossessive(TextWriter writer, ushort currentPunctuation, bool s)
        {
            bool posses = (currentPunctuation & Punctuation.Possessive) == Punctuation.Possessive;

            if (posses)
            {
                if (!s)
                    writer.Write("'s");
                else
                    writer.Write('\'');
            }
        }

        protected static void AddPrefixPunctuation(TextWriter writer, ushort previousPunctuation, ushort currentPunctuation)
        {
            bool prevParen = (previousPunctuation & Punctuation.Parenthetical) != 0;
            bool thisParen = (currentPunctuation & Punctuation.Parenthetical) != 0;

            if (thisParen && !prevParen)
            {
                writer.Write('(');
            }
        }
        protected static void AddPostfixPunctuation(TextWriter writer, ushort currentPunctuation, bool? s = null)
        {
            bool eparen = (currentPunctuation & Punctuation.CloseParen) == Punctuation.CloseParen;
            //          bool posses  = (currentPunctuation & Punctuation.Possessive) == Punctuation.Possessive;
            bool exclaim = (currentPunctuation & Punctuation.Clause) == Punctuation.Exclamatory;
            bool declare = (currentPunctuation & Punctuation.Clause) == Punctuation.Declarative;
            bool dash = (currentPunctuation & Punctuation.Clause) == Punctuation.Dash;
            bool semi = (currentPunctuation & Punctuation.Clause) == Punctuation.Semicolon;
            bool colon = (currentPunctuation & Punctuation.Clause) == Punctuation.Colon;
            bool comma = (currentPunctuation & Punctuation.Clause) == Punctuation.Comma;
            bool quest = (currentPunctuation & Punctuation.Clause) == Punctuation.Interrogative;

            if (s != null)
            {
                ConditionallyMakePossessive(writer, currentPunctuation, s.Value);
            }
            if (eparen)
                writer.Write(')');
            if (declare)
                writer.Write('.');
            else if (comma)
                writer.Write(',');
            else if (semi)
                writer.Write(';');
            else if (colon)
                writer.Write(':');
            else if (quest)
                writer.Write('?');
            else if (exclaim)
                writer.Write('!');
            else if (dash)
                writer.Write("--");
        }

        public static byte GetBookNum(string text)
        {
            string unspaced = text.Replace(" ", "");
            var books = ObjectTable.AVXObjects.Mem.Book.Slice(0, 67).Span;

            for (byte b = 1; b <= 66; b++)
            {
                string name = books[b].name.ToString();
                if (name.Equals(text, StringComparison.InvariantCultureIgnoreCase))
                {
                    return b;
                }
                if (name.Replace(" ", "").Equals(unspaced, StringComparison.InvariantCultureIgnoreCase))
                {
                    return b;
                }
            }
            for (byte b = 1; b <= 66; b++)
            {
                string alt = books[b].abbr2.ToString();
                if (alt.Length == 0)
                    continue;
                if (alt.StartsWith(unspaced, StringComparison.InvariantCultureIgnoreCase))
                {
                    return b;
                }
            }

            for (byte b = 1; b <= 66; b++)
            {
                string alt = books[b].abbr3.ToString();
                if (alt.Length == 0)
                    continue;
                if (alt.StartsWith(unspaced, StringComparison.InvariantCultureIgnoreCase))
                {
                    return b;
                }
            }

            for (byte b = 1; b <= 66; b++)
            {
                string alt = books[b].abbr4.ToString();
                if (alt.Equals(unspaced, StringComparison.InvariantCultureIgnoreCase))
                {
                    return b;
                }
            }

            for (byte b = 1; b <= 66; b++)
            {
                string alt = books[b].abbrAlternates.ToString(); // at this point, we only handle the first alternate if it exists
                if (alt.Length == 0)
                    continue;
                if (alt.Equals(unspaced, StringComparison.InvariantCultureIgnoreCase))
                {
                    return b;
                }
            }
            return 0;
        }


    }
}