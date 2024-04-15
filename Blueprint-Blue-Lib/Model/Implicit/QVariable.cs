// AVX-Quelle specification: 2.0.3.701
//
namespace Blueprint.Model.Implicit
{
    using AVSearch.Interfaces;
    using Blueprint.Blue;
    using Pinshot.PEG;
    using System;
    using System.Data;
    using System.Runtime.Intrinsics.X86;
    using YamlDotNet.Serialization;
    using static Blueprint.Model.Implicit.QLexicon;
    using static global::Blueprint.Model.Implicit.QLexicalDomain;
    using static System.Runtime.InteropServices.JavaScript.JSType;

    internal enum SIMILARITY { NONE = 0, FUZZY_MIN = 33, FUZZY_MAX = 99, EXACT = 100 }

    public class QSpan: ISetting
    {
        public static readonly string Name = typeof(QSpan).Name.Substring(1).ToLower();
        public string SettingName { get => Name; }
        public const ushort VERSE = 0;    // zero means verse-scope
        public const ushort DEFAULT = VERSE;

        public ushort Value { get; set; }
        public QSpan()
        {
            this.Value = VERSE;
        }
        public QSpan(UInt16 span)
        {
            this.Update(span);
        }
        public QSpan(string span)
        {
            UInt16 ispan = FromString(span);
            this.Update(ispan);
        }
        public static UInt16 FromString(string val)
        {
            string test = val.Trim();
            if (test.Equals("verse", StringComparison.InvariantCultureIgnoreCase))
            {
                return VERSE;
            }
            else
            {
                try
                {
                    return UInt16.Parse(val);
                }
                catch
                {
                    ;
                }
            }
            return VERSE;
        }
        public override string ToString()
        {
            return this.Value == 0 ? "verse" : this.Value.ToString();
        }
        public string AsYaml()
        {
            return "span: " + ToString();
        }
        internal void Update(UInt16 span)
        {
            this.Value = span <= 999 ? span : (UInt16)999;
        }
    }
    public class QSimilarity: ISetting
    {
        public QSimilarityWord  Word  { get; internal set; }
        public QSimilarityLemma Lemma { get; internal set; }

        public static readonly string Name = typeof(QSimilarity).Name.Substring(1).ToLower();
        public string SettingName { get => Name; }
        public static (byte word, byte lemma) DEFAULT { get => (QSimilarityWord.DEFAULT, QSimilarityLemma.DEFAULT); }

        public (byte word, byte lemma) Value { get => (this.Word.Value, this.Lemma.Value); }

        public QSimilarity(byte? word = null, byte? lemma = null, ISettings? baseline = null)
        {
            this.Word = word != null ? new QSimilarityWord(word.Value, baseline) : new QSimilarityWord();
            this.Lemma = lemma != null ? new QSimilarityLemma(lemma.Value, baseline) : new QSimilarityLemma();
        }
        private QSimilarity(QSimilarityWord word, QSimilarityLemma lemma)
        {
            this.Word = word;
            this.Lemma = lemma;
        }
        public static QSimilarity Create(QSimilarityWord? word, QSimilarityLemma? lemma)
        {
            return new (word != null ? word : new QSimilarityWord(), lemma != null ? lemma : new QSimilarityLemma());
        }
        public QSimilarity(string val, ISettings? baseline = null)
        {
            this.Word = new QSimilarityWord(val, baseline);
            this.Lemma = new QSimilarityLemma(val, baseline);
        }

        public QSimilarity(string sword, string slemma, ISettings? baseline = null)
        {
            this.Word = new QSimilarityWord(sword, baseline);
            this.Lemma = new QSimilarityLemma(slemma, baseline);
        }
        public static byte SimilarityFromString(string val, ISettings? baseline = null)
        {
            byte result;

            if (val.Equals("off", StringComparison.InvariantCultureIgnoreCase))
                return (byte)0;
            try
            {
                result = val.EndsWith('%') && (val.Length >= 2) ? (byte)UInt16.Parse(val.Substring(0, val.Length-1)) : (byte)UInt16.Parse(val);
                return result >= 0 && result <= 100 ? result : DEFAULT.word;
            }
            catch
            {
                return DEFAULT.word;
            }
        }
        public static string SimilarityToString(byte similarity)
        {
            string result;

            if (similarity <= (byte)SIMILARITY.FUZZY_MIN)
                result = "off";
            else if (similarity >= (byte)SIMILARITY.EXACT)
                result = "100%";
            else
                result = similarity.ToString() + '%';

            return result;
        }
        public override string ToString()
        {
            string result = "word: "  + this.Value.word.ToString() + ", "
                          + "lemma: " + this.Value.lemma.ToString();

            return result;
        }
        public string AsYaml()
        {
            string yaml = this.Word.AsYaml() + "\n" + this.Lemma.AsYaml();
            return yaml;
        }
    }
    public class QSimilarityWord : ISetting
    {
        public static readonly string Name = "similarity.word";
        public string SettingName { get => Name; }
        public static byte DEFAULT { get => (byte)SIMILARITY.NONE; }
        public byte Value { get; private set; }
        public QSimilarityWord()
        {
            Value = DEFAULT;
        }
        public QSimilarityWord(byte val, ISettings? baseline = null)
        {
            this.Value = val >= (byte)SIMILARITY.FUZZY_MIN && val <= (byte)SIMILARITY.EXACT ? val : baseline != null ? baseline.SearchSimilarity.word : DEFAULT;
        }
        public QSimilarityWord(string val, ISettings? baseline = null)
        {
            var result = QSimilarity.SimilarityFromString(val, baseline);
            this.Value = result >= (byte)SIMILARITY.FUZZY_MIN && result <= (byte)SIMILARITY.EXACT ? result : baseline != null ? baseline.SearchSimilarity.word : DEFAULT;
        }
        public override string ToString()
        {
            string result = QSimilarity.SimilarityToString(this.Value);
            return result;
        }
        public string AsYaml()
        {
            return QSimilarityWord.Name + ": " + this.ToString();
        }
    }
    public class QSimilarityLemma : ISetting
    {
        public static readonly string Name = "similarity.lemma";
        public string SettingName { get => Name; }
        public static byte DEFAULT { get => (byte)SIMILARITY.NONE; }
        public byte Value { get; private set; }
        public QSimilarityLemma()
        {
            Value = DEFAULT;
        }
        public QSimilarityLemma(byte val, ISettings? baseline = null)
        {
            this.Value = val >= (byte)SIMILARITY.FUZZY_MIN && val <= (byte)SIMILARITY.EXACT ? val : baseline != null ? baseline.SearchSimilarity.word : DEFAULT;
        }

        public QSimilarityLemma(string val, ISettings? baseline = null)
        {
            var result = QSimilarity.SimilarityFromString(val, baseline);
            this.Value = result >= (byte)SIMILARITY.FUZZY_MIN && result <= (byte)SIMILARITY.EXACT ? result : baseline != null ? baseline.SearchSimilarity.word : DEFAULT;
        }
        public override string ToString()
        {
            string result = QSimilarity.SimilarityToString(this.Value);
            return result;
        }
        public string AsYaml()
        {
            return "similarity.lemma: " + this.ToString();
        }
    }
    public class QLexicon : ISetting
    {
        public static readonly string Name = typeof(QLexicon).Name.Substring(8).ToLower();
        public string SettingName { get => Name; }
        public enum QLexiconVal
        {
            UNDEFINED = ISettings.Lexion_UNDEFINED,
            AV = ISettings.Lexion_AV,
            AVX = ISettings.Lexion_AVX,
            BOTH = ISettings.Lexion_BOTH
        }
        public static readonly (QLexiconVal search, QLexiconVal display) DEFAULT = (QLexicalDomain.DEFAULT, QLexicalDisplay.DEFAULT);

        public QLexicalDomain  Domain { get; private set; } // search domain
        public QLexicalDisplay Render { get; private set; } // render/display

        [YamlIgnore ]
        public (QLexiconVal search, QLexiconVal render) Value { get => (this.Domain.Value, this.Render.Value); }
        public QLexicon()
        {
            this.Domain = new();
            this.Render = new();
        }
        public QLexicon(QLexiconVal val)
        {
            this.Domain = new(val);
            this.Render = new(val);
        }
        public QLexicon(string val)
        {
            this.Domain = new(val);
            this.Render = new(val);
        }
        public QLexicon(QLexiconVal search, QLexiconVal render)
        {
            this.Domain = new(search);
            this.Render = new(render);
        }
        public QLexicon(string search, string render)
        {
            this.Domain = new(search);
            this.Render = new(render);
        }
        public static QLexiconVal FromString(string val)
        {
            switch (val.Trim().ToUpper())
            {
                case "KJV":
                case "AV":   return QLexiconVal.AV;
                case "MODERN":
                case "MOD":
                case "AVX":  return QLexiconVal.AVX;
                case "BOTH":
                case "DUAL": return QLexiconVal.BOTH;
                default:     return QLexiconVal.UNDEFINED;
            }
        }
        public static string ToString(QLexiconVal val)
        {
            switch (val)
            {
                case QLexiconVal.AV:   return "av";
                case QLexiconVal.AVX:  return "avx";
                case QLexiconVal.BOTH: return "both";
                default:               return string.Empty;
            }
        }
        public override string ToString()
        {
            return "search: " + this.Domain.ToString() + ", render: " + this.Render.ToString();
        }
        public string AsYaml()
        {
            return this.Domain.AsYaml() + "\n" + this.Render.AsYaml();
        }
    }
    public class QLexicalDomain: ISetting
    {
        public static readonly string Name = "lexicon.search";
        public string SettingName { get => Name; }
        public const QLexicon.QLexiconVal DEFAULT = QLexicon.QLexiconVal.BOTH;
        public QLexicon.QLexiconVal Value { get; set; }
        public QLexicalDomain()
        {
            Value = DEFAULT;
        }
        public QLexicalDomain(QLexicon.QLexiconVal val)
        {
            Value = val;
        }
        public QLexicalDomain(string val)
        {
            Value = FromString(val);
        }
        public static QLexicon.QLexiconVal FromString(string val)
        {
            switch (val.Trim().ToUpper())
            {
                case "KJV":
                case "AV": return QLexicon.QLexiconVal.AV;
                case "MODERN":
                case "MOD":
                case "AVX": return QLexicon.QLexiconVal.AVX;
                case "BOTH":
                case "DUAL": return QLexicon.QLexiconVal.BOTH;
                default: return DEFAULT;
            }
        }
        public static string ToString(QLexicon.QLexiconVal val)
        {
            switch (val)
            {
                case QLexicon.QLexiconVal.AV:   return "av";
                case QLexicon.QLexiconVal.AVX:  return "avx";
                case QLexicon.QLexiconVal.BOTH: return "both";
                default:                        return ToString(QLexicalDomain.DEFAULT);
            }
        }
        public override string ToString()
        {
            return ToString(this.Value);
        }
        public string AsYaml()
        {
            return "lexicon.search: " + ToString();
        }
    }
    public class QLexicalDisplay: ISetting
    {
        public static readonly string Name = "lexicon.render";
        public string SettingName { get => Name; }

        public const QLexiconVal DEFAULT = QLexiconVal.AV;
        public QLexiconVal Value { get; set; }
        public QLexicalDisplay()
        {
            Value = DEFAULT;
        }
        public QLexicalDisplay(QLexiconVal val)
        {
            Value = val;
        }
        public QLexicalDisplay(string val)
        {
            Value = FromString(val);
        }
        public static QLexiconVal FromString(string val)
        {
            switch (val.Trim().ToUpper())
            {
                case "KJV":
                case "AV": return QLexiconVal.AV;
                case "MODERN":
                case "MOD":
                case "AVX": return QLexiconVal.AVX;
                case "BOTH":
                case "DUAL": return QLexiconVal.BOTH;
                default: return DEFAULT;
            }
        }
        public static string ToString(QLexiconVal val)
        {
            switch (val)
            {
                case QLexiconVal.AV:    return "av";
                case QLexiconVal.AVX:   return "avx";
                case QLexiconVal.BOTH:  return "both";
                default:                return ToString(QLexicalDisplay.DEFAULT);
            }
        }
        public override string ToString()
        {
            return ToString(this.Value);
        }
        public string AsYaml()
        {
            return "lexicon.render: " + ToString();
        }
    }
    public class QFormat: ISetting
    {
        public static readonly string Name = typeof(QFormat).Name.Substring(1).ToLower();
        public string SettingName { get => Name; }
        public enum QFormatVal
        {
            JSON = ISettings.Formatting_JSON,
            YAML = ISettings.Formatting_YAML,
            TEXT = ISettings.Formatting_TEXT,
            HTML = ISettings.Formatting_HTML,
            MD   = ISettings.Formatting_MD
        }
        public const QFormatVal DEFAULT = QFormatVal.TEXT;
        public QFormatVal Value { get; set; }
        public QFormat()
        {
            Value = DEFAULT;
        }
        public QFormat(QFormatVal val)
        {
            Value = val;
        }
        public QFormat(string val)
        {
            Value = FromString(val);
        }
        public static QFormatVal FromString(string val)
        {
            switch (val.Trim().ToUpper())
            {
                case "JSON": return QFormatVal.JSON;
                case "YAML": return QFormatVal.YAML;
                case "TEXTUAL":
                case "TXT":
                case "UTF": 
                case "UTF8":
                case "TEXT": return QFormatVal.TEXT;
                case "HTML": return QFormatVal.HTML;
                case "MARKDOWN":
                case "MD": return QFormatVal.MD;
                default: return DEFAULT;
            }
        }
        public static string ToString(QFormatVal val)
        {
            switch (val)
            {
                case QFormatVal.JSON: return "json";
                case QFormatVal.YAML: return "yaml";
                case QFormatVal.TEXT: return "text";
                case QFormatVal.HTML: return "html";
                case QFormatVal.MD:   return "md";
                default: return DEFAULT.ToString();
            }
        }
        public override string ToString()
        {
            return ToString(this.Value);
        }
        public string AsYaml()
        {
            return "format: " + ToString();
        }
    }
    public abstract class QVariable : QCommand
    {
        public string Key { get; protected set; }
        public string Value { get; protected set; }

        protected QVariable(QContext env, string text, string verb, string key, string value = "") : base (env, key + "=" + value, "assign")
        {
            this.Key = key;

            if (string.IsNullOrWhiteSpace(value))
            {
                this.Value = GetDefault(Key);
            }
            else
            {
                this.Value = value;
            }
        }
        private static string GetDefault(string setting)
        {
            switch (setting.Trim().ToLower())
            {
                case "span":            return QSpan.DEFAULT.ToString();
                case "lexicon":         return QLexicon.DEFAULT.ToString();
                case "lexicon.search":
                case "search.lexicon":
                case "search":          return QLexicalDomain.DEFAULT.ToString();
                case "lexicon.render":
                case "render.lexicon":
                case "render":          return QLexicalDisplay.DEFAULT.ToString();
                case "format":          return QFormat.DEFAULT.ToString();
                case "similarity":      return QSimilarity.DEFAULT.ToString();
                case "similarity.lemma":return QSimilarityLemma.DEFAULT.ToString();
                case "similarity.word": return QSimilarityWord.DEFAULT.ToString();
            }
            return string.Empty;
        }
        public static QAssign? CreateAssignment(QContext env, string text, Parsed arg)
        {
            if (arg.rule.EndsWith("_var"))
            {
                if (arg.children.Length == 2
                && arg.children[0].rule.EndsWith("_key", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new QAssign(env, text, arg.children[0].text, arg.children[1].text);
                }
            }
            return null;
        }
        internal static QAssign CreateAssignment(QContext env, string text, string key, string value)
        {
            return new QAssign(env, text, key, value);
        }
    }
}