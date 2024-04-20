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
                    case QFormatVal.JSON: return new ExportJson(env, text, spec, format.Value, mode);
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
}