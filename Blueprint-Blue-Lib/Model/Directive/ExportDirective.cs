namespace Blueprint.Blue
{
    using AVSearch.Interfaces;
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
        CreateNew = 0,
        Overwrite = 1,
        Append = 2
    }
    public record WordFeatures // hijacked from WordRendering class
    {
        public UInt32 WordKey;
        public BCVW Coordinates;
        public PNPOS PnPos;
        public string NuPos;
        public string Text;   // KJV
        public string Modern; // AVX
        public bool Modernized { get => !this.Text.Equals(this.Modern, StringComparison.InvariantCultureIgnoreCase); }
        public byte Punctuation;
        public Dictionary<UInt32, string>? Triggers;

        public WordFeatures()
        {
            this.WordKey = 0;
            this.Coordinates = new();
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
            this.Coordinates = writ.BCVWc;
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
        protected FileCreateMode CreationMode { get; set; }
        protected QFormatVal ContentFormat { get; set; }
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

        public DirectiveResultType Retrieve()
        {
            return UsesAugmentation ? Deserialize() : ValidateTextFile();
        }

        private DirectiveResultType ValidateTextFile()
        {
            if (Directory.Exists(Path.GetDirectoryName(this.FileSpec))
            && (this.CreationMode != FileCreateMode.CreateNew || !File.Exists(this.FileSpec)))
                return DirectiveResultType.ExportReady;
            return DirectiveResultType.ExportNotReady;
        }
        private DirectiveResultType Deserialize()    // this is used ONLY for Yaml and Json formats ... note: formats can be dynamically migrated
        {
            if (!UsesAugmentation)
            {
                return DirectiveResultType.ExportNotReady;
            }
            if ((this.CreationMode != FileCreateMode.Append))
            {
                return DirectiveResultType.ExportReady;
            }
            if (Path.Exists(this.FileSpec))
            {
                bool? json = this.IsJson();
                if (json.HasValue)
                {
                    ExportJson? entries = null;
                    try
                    {
                        using (StreamReader file = File.OpenText(this.FileSpec))
                        {
                            if (json.Value)
                            {
                                entries = System.Text.Json.JsonSerializer.Deserialize<ExportJson>(file.BaseStream);
                            }
                            else // yaml
                            {
                                var deserializer = new DeserializerBuilder();
                                var builder = deserializer.Build();

                                string text = file.ReadToEnd();
                                var input = new StringReader(text);
                                entries = builder.Deserialize<ExportJson>(input);
                            }
                        }
                        if (entries != null)
                        {
                            foreach (var key in entries.Keys)
                            {
                                this[key] = entries[key];
                            }
                            return DirectiveResultType.ExportReady;
                        }
                    }
                    catch
                    {
                        ;
                    }
                }
            }
            return DirectiveResultType.ExportNotReady;
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
        protected virtual DirectiveResultType RenderViaScope(TextWriter writer) // not-Applicable to yaml or json (this base-method are never called)
        {
            return DirectiveResultType.NotApplicable;
        }
        protected virtual DirectiveResultType RenderViaSearch(TextWriter writer) // not-Applicable to yaml or json (this base-method are never called)
        {
            return DirectiveResultType.NotApplicable;
        }

        protected ExportDirective(): base() // for deserialization
        {
            this.Context = null;
            this.FileSpec = string.Empty;
            this.CreationMode = FileCreateMode.CreateNew;
            this.ContentFormat = QFormatVal.TEXT;
        }
        protected ExportDirective(QContext env, string text, string spec, QFormatVal format, FileCreateMode mode): base()
        {
            this.Context = env;
            this.FileSpec = spec;
            this.CreationMode = mode;
            this.ContentFormat = format;
        }
        public static ExportDirective? Create(QContext env, string text, Parsed[] args, QSelectionCriteria selection)
        {
            if (args.Length == 1 && args[0].children.Length == 1 && args[0].children[0].rule.Equals("filename", StringComparison.InvariantCultureIgnoreCase))
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
                    case QFormatVal.JSON: return new ExportHtml(env, text, spec, format.Value, mode);
                    case QFormatVal.YAML: return new ExportYaml(env, text, spec, format.Value, mode);
                    case QFormatVal.TEXT: return new ExportText(env, text, spec, format.Value, mode);
                    case QFormatVal.HTML: return new ExportHtml(env, text, spec, format.Value, mode);
                    case QFormatVal.MD:   return new ExportMarkdown(env, text, spec, format.Value, mode);
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

    }
    public class ExportJson : ExportDirective
    {
        protected override bool UsesAugmentation { get => true; }

        public ExportJson(): base()
        {
            ;
        }
        internal ExportJson(QContext env, string text, string spec, QFormatVal format, FileCreateMode mode) : base(env, text, spec, format, mode)
        {
            ;
        }
        public override DirectiveResultType Update()
        {
            try
            {
                using (Stream stream = File.Create(this.FileSpec))
                {
                    System.Text.Json.JsonSerializer.Serialize<ExportJson>(stream, this);
                    return DirectiveResultType.ExportSuccessful;
                }
            }
            catch
            {
                ;
            }

            return DirectiveResultType.ExportFailed;
        }
    }
    public class ExportYaml : ExportDirective
    {
        protected override bool UsesAugmentation { get => true; }

        internal ExportYaml(QContext env, string text, string spec, QFormatVal format, FileCreateMode mode) : base(env, text, spec, format, mode)
        {
            ;
        }
        public override DirectiveResultType Update()
        {
            try
            {
                using (TextWriter writer = File.CreateText(this.FileSpec))
                {
                    var serializer = new SerializerBuilder();
                    var builder = serializer.Build();

                    string yaml = builder.Serialize(this);
                    writer.Write(yaml);

                    return DirectiveResultType.ExportSuccessful;
                }
            }
            catch
            {
                ;
            }

            return DirectiveResultType.ExportFailed;
        }
    }
    public class ExportText : ExportDirective
    {
        internal ExportText(QContext env, string text, string spec, QFormatVal format, FileCreateMode mode) : base(env, text, spec, format, mode)
        {
            ;
        }
        public override DirectiveResultType Update()
        {
            bool br = false;
            using (TextWriter writer = this.CreateFile())
            {
                try
                {
                    foreach (byte b in this.Keys)
                    {
                        if (this[b].Count == 0)
                            continue;

                        var BOOK = ObjectTable.AVXObjects.Mem.Book.Slice((int)b, 1).Span[0];
                        var CHAP = ObjectTable.AVXObjects.Mem.Chapter.Slice(BOOK.chapterIdx, BOOK.chapterCnt).Span;

                        foreach (byte c in this[b].Keys)
                        {
                            bool nc = true;
                            if (br)
                                writer.WriteLine();

                            if (this.ScopeOnlyExport)
                            {
                                var chapter = CHAP[c - 1];
                                var writ = ObjectTable.AVXObjects.Mem.Written.Slice((int)chapter.writIdx, (int)chapter.writCnt).Span;

                                List<WordFeatures> words = new();
                                for (int w = 0; w < (int)chapter.writCnt; w++)
                                {
                                    WordFeatures word = new WordFeatures(writ[w]);
                                    words.Add(word);

                                    byte wc = writ[w].BCVWc.WC;

                                    if (wc == 1)
                                    {
                                        byte v = writ[w].BCVWc.V;
                                        this[b][c][v] = words;
                                        RenderVerse(writer, b, c, v, br, nc);
                                        words.Clear();
                                        nc = false;
                                    }
                                }
                            }
                            else
                            {
                                foreach (byte v in this[b][c].Keys)
                                {
                                    RenderVerse(writer, b, c, v, br, nc);
                                }
                            }
                            br = true;
                        }
                    }
                    return DirectiveResultType.ExportSuccessful;
                }
                catch
                {
                    ;
                }
            }
            return DirectiveResultType.ExportFailed;
        }
        private void RenderVerse(TextWriter writer, byte b, byte c, byte v, bool newline, bool newchapter)
        {
            if (this.ContainsKey(b) && this[b].ContainsKey(c) && this[b][c].ContainsKey(v) && this[b][c][v].Count > 0)
            {
                List<WordFeatures> words = this[b][c][v];
                if (newchapter)
                {
                    if (newline)
                        writer.WriteLine();

                    writer.WriteLine(this.Books[b].name.ToString().ToUpper() + " " + c.ToString());
                }
                writer.Write(v.ToString());
                writer.Write(' ');

                byte previousPunctuation = 0;
                bool space = false;

                foreach (WordFeatures word in words)
                {
                    bool italics = (byte)(word.Punctuation & Punctuation.Italics) == Punctuation.Italics;

                    string entry = this.Settings.RenderAsAV ? word.Text : word.Modern;
                    bool s = entry.EndsWith("s", StringComparison.InvariantCultureIgnoreCase);

                    if (space)
                        writer.Write(' ');
                    else
                        space = true;

                    ExportDirective.AddPrefixPunctuation(writer, previousPunctuation, word.Punctuation);

                    if (italics)
                        writer.Write('[');
                    writer.Write(entry);
                    ExportDirective.ConditionallyMakePossessive(writer, word.Punctuation, s);
                    if (italics)
                        writer.Write(']');

                    ExportDirective.AddPostfixPunctuation(writer, word.Punctuation);

                    previousPunctuation = word.Punctuation;
                }
            }
        }
    }
    public class ExportHtml : ExportDirective
    {
        internal ExportHtml(QContext env, string text, string spec, QFormatVal format, FileCreateMode mode) : base(env, text, spec, format, mode)
        {
            ;
        }
        public override DirectiveResultType Update()
        {
            bool br = false;
            using (TextWriter writer = this.CreateFile())
            {
                try
                {
                    foreach (byte b in this.Keys)
                    {
                        if (this[b].Count == 0)
                            continue;

                        var BOOK = ObjectTable.AVXObjects.Mem.Book.Slice((int)b, 1).Span[0];
                        var CHAP = ObjectTable.AVXObjects.Mem.Chapter.Slice(BOOK.chapterIdx, BOOK.chapterCnt).Span;

                        foreach (byte c in this[b].Keys)
                        {
                            bool nc = true;

                            if (this.ScopeOnlyExport)
                            {
                                var chapter = CHAP[c - 1];
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
                                        RenderVerse(writer, b, c, v, br, nc);
                                        words.Clear();
                                        nc = false;
                                    }
                                }
                            }
                            else
                            {
                                foreach (byte v in this[b][c].Keys)
                                {
                                    RenderVerse(writer, b, c, v, br, nc);
                                }
                            }
                            writer.WriteLine("</span>"); // EoC
                            br = true;
                        }
                    }
                    return DirectiveResultType.ExportSuccessful;
                }
                catch
                {
                    ;
                }
            }
            return DirectiveResultType.ExportFailed;
        }
        private void RenderVerse(TextWriter writer, byte b, byte c, byte v, bool newline, bool newchapter)
        {
            if (this.ContainsKey(b) && this[b].ContainsKey(c) && this[b][c].ContainsKey(v) && this[b][c][v].Count > 0)
            {
                List<WordFeatures> words = this[b][c][v];
                if (newchapter)
                {
                    if (newline)
                        writer.WriteLine("<br/>");

                    writer.WriteLine("<span class='chapter' id='C" + c.ToString() + "'>" +this.Books[b].name.ToString() + " " + c.ToString() + "<br/>");
                }

                byte previousPunctuation = 0;
                bool space = false;

                bool bov = true;
                foreach (WordFeatures word in words)
                {
                    if (bov)
                    {
                        writer.WriteLine("<span class='verse' id='V" + (word.Coordinates.elements / 0x100).ToString() + "'><b>" + word.Coordinates.V.ToString() + "</b>");
                        bov = false;
                    }

                    bool italics = (byte)(word.Punctuation & Punctuation.Italics) == Punctuation.Italics;

                    string entry = this.Settings.RenderAsAV ? word.Text : word.Modern;
                    bool s = entry.EndsWith("s", StringComparison.InvariantCultureIgnoreCase);

                    if (space)
                        writer.Write(' ');
                    else
                        space = true;

                    ExportDirective.AddPrefixPunctuation(writer, previousPunctuation, word.Punctuation);

                    if (italics)
                        writer.Write("<em>");
                    writer.Write("<span id='X");
                    writer.Write(word.Coordinates.elements.ToString("X"));
                    writer.Write(" 'class='W");
                    writer.Write(word.WordKey.ToString("X"));
                    writer.Write("' diff='");
                    writer.Write(word.Modernized ? "true" : "false");
                    writer.Write("'>");
                    writer.Write(entry);
                    ExportDirective.ConditionallyMakePossessive(writer, word.Punctuation, s);
                    writer.Write("</span>");
                    if (italics)
                        writer.Write("</em>");

                    ExportDirective.AddPostfixPunctuation(writer, word.Punctuation);

                    previousPunctuation = word.Punctuation;
                }
                writer.WriteLine("</span>"); // EoV
            }
        }
    }
    public class ExportMarkdown : ExportDirective
    {
        internal ExportMarkdown(QContext env, string text, string spec, QFormatVal format, FileCreateMode mode) : base(env, text, spec, format, mode)
        {
            ;
        }
        public override DirectiveResultType Update()
        {
            bool br = false;
            using (TextWriter writer = this.CreateFile())
            {
                try
                {
                    foreach (byte b in this.Keys)
                    {
                        if (this[b].Count == 0)
                            continue;

                        var BOOK = ObjectTable.AVXObjects.Mem.Book.Slice((int)b, 1).Span[0];
                        var CHAP = ObjectTable.AVXObjects.Mem.Chapter.Slice(BOOK.chapterIdx, BOOK.chapterCnt).Span;

                        foreach (byte c in this[b].Keys)
                        {
                            bool nc = true;

                            if (this.ScopeOnlyExport)
                            {
                                var chapter = CHAP[c - 1];
                                var writ = ObjectTable.AVXObjects.Mem.Written.Slice((int)chapter.writIdx, (int)chapter.writCnt).Span;

                                List<WordFeatures> words = new();
                                for (int w = 0; w < (int)chapter.writCnt; w++)
                                {
                                    WordFeatures word = new WordFeatures(writ[w]);
                                    words.Add(word);

                                    byte wc = writ[w].BCVWc.WC;

                                    if (wc == 1)
                                    {
                                        byte v = writ[w].BCVWc.V;
                                        this[b][c][v] = words;
                                        RenderVerse(writer, b, c, v, br, nc);
                                        words.Clear();
                                        nc = false;
                                    }
                                }
                            }
                            else
                            {
                                foreach (byte v in this[b][c].Keys)
                                {
                                    RenderVerse(writer, b, c, v, br, nc);
                                    if (this.ScopeOnlyExport)
                                    {
                                        this[b][c].Clear();
                                    }
                                }
                            }
                            br = true;
                        }
                    }
                    return DirectiveResultType.ExportSuccessful;
                }
                catch
                {
                    ;
                }
            }
            return DirectiveResultType.ExportFailed;
        }
        private void RenderVerse(TextWriter writer, byte b, byte c, byte v, bool newline, bool newchapter)
        {
            if (this.ContainsKey(b) && this[b].ContainsKey(c) && this[b][c].ContainsKey(v) && this[b][c][v].Count > 0)
            {
                List<WordFeatures> words = this[b][c][v];
                if (newchapter)
                {
                    if (newline)
                        writer.WriteLine();

                    writer.WriteLine("### " + this.Books[b].name.ToString() + " " + c.ToString());
                }
                else
                {
                    writer.Write(' ');
                }
                writer.Write("**" + v.ToString() + "**");
                writer.Write(' ');

                byte previousPunctuation = 0;
                bool space = false;

                foreach (WordFeatures word in words)
                {
                    bool italics = (byte)(word.Punctuation & Punctuation.Italics) == Punctuation.Italics;

                    string entry = this.Settings.RenderAsAV ? word.Text : word.Modern;
                    bool s = entry.EndsWith("s", StringComparison.InvariantCultureIgnoreCase);

                    if (space)
                        writer.Write(' ');
                    else
                        space = true;

                    ExportDirective.AddPrefixPunctuation(writer, previousPunctuation, word.Punctuation);

                    if (italics)
                        writer.Write('*');
                    writer.Write(entry);
                    ExportDirective.ConditionallyMakePossessive(writer, word.Punctuation, s);
                    if (italics)
                        writer.Write('*');

                    ExportDirective.AddPostfixPunctuation(writer, word.Punctuation);

                    previousPunctuation = word.Punctuation;
                }
            }
        }
    }
}