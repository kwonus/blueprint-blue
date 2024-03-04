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
    using static Blueprint.Model.Implicit.QLexicon;
    using static global::Blueprint.Model.Implicit.QLexicalDomain;

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
        public static readonly string Name = typeof(QSimilarity).Name.Substring(1).ToLower();
        public string SettingName { get => Name; }
        public static (byte word, byte lemma) DEFAULT
        {
            get
            {
                return ((byte)SIMILARITY.NONE, (byte)SIMILARITY.NONE);
            }
        }
        public (byte word, byte lemma) Value { get; private set; }
        public QSimilarity()
        {
            Value = DEFAULT;
        }
        public QSimilarity(byte val, ISettings? baseline = null)
        {
            this.Value = (
                val >= (byte)SIMILARITY.FUZZY_MIN && val <= (byte)SIMILARITY.EXACT ? val : baseline != null ? baseline.SearchSimilarity.word  : DEFAULT.word,
                val >= (byte)SIMILARITY.FUZZY_MIN && val <= (byte)SIMILARITY.EXACT ? val : baseline != null ? baseline.SearchSimilarity.lemma : DEFAULT.lemma);
        }
        public QSimilarity(byte word, byte lemma, ISettings? baseline = null)
        {
            this.Value = (
                word  >= (byte)SIMILARITY.FUZZY_MIN && word  <= (byte)SIMILARITY.EXACT ? word  : baseline != null ? baseline.SearchSimilarity.word  : DEFAULT.word,
                lemma >= (byte)SIMILARITY.FUZZY_MIN && lemma <= (byte)SIMILARITY.EXACT ? lemma : baseline != null ? baseline.SearchSimilarity.lemma : DEFAULT.lemma);
        }
        public QSimilarity(string val, ISettings? baseline = null)
        {
            string input = val.ToLower().Replace("word", ",word").Replace("lemma", ",lemma").Replace(" ", string.Empty).Replace("\t", string.Empty);
            string[] parts = val.Split(',', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
            {
                if (!parts[0].Contains(':'))
                {
                    var result = QSimilarity.SimilarityFromString(parts[0], baseline);
                    this.Value = (
                        result >= (byte)SIMILARITY.FUZZY_MIN && result <= (byte)SIMILARITY.EXACT ? result : baseline != null ? baseline.SearchSimilarity.word  : DEFAULT.word,
                        result >= (byte)SIMILARITY.FUZZY_MIN && result <= (byte)SIMILARITY.EXACT ? result : baseline != null ? baseline.SearchSimilarity.lemma : DEFAULT.lemma);
                    return;
                }
                parts = parts[0].Split(':', StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    byte result = QSimilarity.SimilarityFromString(parts[1], baseline);
                    switch (parts[0])
                    {
                        case "word":
                            this.Value = (
                                result >= (byte)SIMILARITY.FUZZY_MIN && result <= (byte)SIMILARITY.EXACT ? result : baseline != null ? baseline.SearchSimilarity.word : DEFAULT.word,
                                baseline != null ? baseline.SearchSimilarity.lemma : DEFAULT.lemma);
                            break;

                        case "lemma":
                            this.Value = (
                                baseline != null ? baseline.SearchSimilarity.word : DEFAULT.word,
                                result >= (byte)SIMILARITY.FUZZY_MIN && result <= (byte)SIMILARITY.EXACT ? result : baseline != null ? baseline.SearchSimilarity.lemma : DEFAULT.lemma);
                            break;
                    }
                    return;
                }
                this.Value = DEFAULT;
                return;
            }
        }

        public QSimilarity(string sword, string slemma, ISettings? baseline = null)
        {
            var word  = QSimilarity.SimilarityFromString(sword);
            var lemma = QSimilarity.SimilarityFromString(slemma);

            this.Value = (
                word  >= (byte)SIMILARITY.FUZZY_MIN && word  <= (byte)SIMILARITY.EXACT ? word  : baseline != null ? baseline.SearchSimilarity.word  : DEFAULT.word,
                lemma >= (byte)SIMILARITY.FUZZY_MIN && lemma <= (byte)SIMILARITY.EXACT ? lemma : baseline != null ? baseline.SearchSimilarity.lemma : DEFAULT.lemma);
        }
        public static byte SimilarityFromString(string val, ISettings? baseline = null)
        {
            byte result;

            if (val.Equals("none", StringComparison.InvariantCultureIgnoreCase))
                return (byte)0;
            if (val.Equals("exact", StringComparison.InvariantCultureIgnoreCase))
                return (byte)100;
            if (val.Length != 2)
                return (byte)DEFAULT.word;
            try
            {
                result = (byte)ushort.Parse(val);
                return result;
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
                result = "none";
            else if (similarity >= (byte)SIMILARITY.EXACT)
                result = "exact";
            else
                result = similarity.ToString();

            return result;
        }
        public override string ToString()
        {
            string result = "word: "  + QSimilarity.SimilarityToString(this.Value.word) + ", "
                          + "lemma: " + QSimilarity.SimilarityToString(this.Value.lemma);

            return result;
        }
        public string AsYaml()
        {
            return "similarity: " + this.ToString();
        }
    }
    public class QSimilarityWord : ISetting
    {
        public static readonly string Name = "similarity.word";
        public string SettingName { get => Name; }
        public static (byte word, byte lemma) DEFAULT
        {
            get
            {
                return ((byte)SIMILARITY.NONE, (byte)SIMILARITY.NONE);
            }
        }
        public (byte word, byte lemma) Value { get; private set; }
        public QSimilarityWord()
        {
            Value = DEFAULT;
        }
        public QSimilarityWord(byte val, ISettings? baseline = null)
        {
            this.Value = (
                val >= (byte)SIMILARITY.FUZZY_MIN && val <= (byte)SIMILARITY.EXACT ? val : baseline != null ? baseline.SearchSimilarity.word : DEFAULT.word,
                val >= (byte)SIMILARITY.FUZZY_MIN && val <= (byte)SIMILARITY.EXACT ? val : baseline != null ? baseline.SearchSimilarity.lemma : DEFAULT.lemma);
        }
        public QSimilarityWord(byte word, byte lemma, ISettings? baseline = null)
        {
            this.Value = (
                word >= (byte)SIMILARITY.FUZZY_MIN && word <= (byte)SIMILARITY.EXACT ? word : baseline != null ? baseline.SearchSimilarity.word : DEFAULT.word,
                lemma >= (byte)SIMILARITY.FUZZY_MIN && lemma <= (byte)SIMILARITY.EXACT ? lemma : baseline != null ? baseline.SearchSimilarity.lemma : DEFAULT.lemma);
        }
        public QSimilarityWord(string val, ISettings? baseline = null)
        {
            string input = val.ToLower().Replace("word", ",word").Replace("lemma", ",lemma").Replace(" ", string.Empty).Replace("\t", string.Empty);
            string[] parts = val.Split(',', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
            {
                if (!parts[0].Contains(':'))
                {
                    var result = QSimilarity.SimilarityFromString(parts[0], baseline);
                    this.Value = (
                        result >= (byte)SIMILARITY.FUZZY_MIN && result <= (byte)SIMILARITY.EXACT ? result : baseline != null ? baseline.SearchSimilarity.word : DEFAULT.word,
                        result >= (byte)SIMILARITY.FUZZY_MIN && result <= (byte)SIMILARITY.EXACT ? result : baseline != null ? baseline.SearchSimilarity.lemma : DEFAULT.lemma);
                    return;
                }
                parts = parts[0].Split(':', StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    byte result = QSimilarity.SimilarityFromString(parts[1], baseline);
                    switch (parts[0])
                    {
                        case "word":
                            this.Value = (
                                result >= (byte)SIMILARITY.FUZZY_MIN && result <= (byte)SIMILARITY.EXACT ? result : baseline != null ? baseline.SearchSimilarity.word : DEFAULT.word,
                                baseline != null ? baseline.SearchSimilarity.lemma : DEFAULT.lemma);
                            break;

                        case "lemma":
                            this.Value = (
                                baseline != null ? baseline.SearchSimilarity.word : DEFAULT.word,
                                result >= (byte)SIMILARITY.FUZZY_MIN && result <= (byte)SIMILARITY.EXACT ? result : baseline != null ? baseline.SearchSimilarity.lemma : DEFAULT.lemma);
                            break;
                    }
                    return;
                }
                this.Value = DEFAULT;
                return;
            }
        }

        public QSimilarityWord(string sword, string slemma, ISettings? baseline = null)
        {
            var word = QSimilarity.SimilarityFromString(sword);
            var lemma = QSimilarity.SimilarityFromString(slemma);

            this.Value = (
                word >= (byte)SIMILARITY.FUZZY_MIN && word <= (byte)SIMILARITY.EXACT ? word : baseline != null ? baseline.SearchSimilarity.word : DEFAULT.word,
                lemma >= (byte)SIMILARITY.FUZZY_MIN && lemma <= (byte)SIMILARITY.EXACT ? lemma : baseline != null ? baseline.SearchSimilarity.lemma : DEFAULT.lemma);
        }
        public override string ToString()
        {
            string result = "word: " + QSimilarity.SimilarityToString(this.Value.word) + ", "
                          + "lemma: " + QSimilarity.SimilarityToString(this.Value.lemma);

            return result;
        }
        public string AsYaml()
        {
            return "similarity: " + this.ToString();
        }
    }
    public class QSimilarityLemma : ISetting
    {
        public static readonly string Name = "similarity.lemma";
        public string SettingName { get => Name; }
        public static (byte word, byte lemma) DEFAULT
        {
            get
            {
                return ((byte)SIMILARITY.NONE, (byte)SIMILARITY.NONE);
            }
        }
        public (byte word, byte lemma) Value { get; private set; }
        public QSimilarityLemma()
        {
            Value = DEFAULT;
        }
        public QSimilarityLemma(byte val, ISettings? baseline = null)
        {
            this.Value = (
                val >= (byte)SIMILARITY.FUZZY_MIN && val <= (byte)SIMILARITY.EXACT ? val : baseline != null ? baseline.SearchSimilarity.word : DEFAULT.word,
                val >= (byte)SIMILARITY.FUZZY_MIN && val <= (byte)SIMILARITY.EXACT ? val : baseline != null ? baseline.SearchSimilarity.lemma : DEFAULT.lemma);
        }
        public QSimilarityLemma(byte word, byte lemma, ISettings? baseline = null)
        {
            this.Value = (
                word >= (byte)SIMILARITY.FUZZY_MIN && word <= (byte)SIMILARITY.EXACT ? word : baseline != null ? baseline.SearchSimilarity.word : DEFAULT.word,
                lemma >= (byte)SIMILARITY.FUZZY_MIN && lemma <= (byte)SIMILARITY.EXACT ? lemma : baseline != null ? baseline.SearchSimilarity.lemma : DEFAULT.lemma);
        }
        public QSimilarityLemma(string val, ISettings? baseline = null)
        {
            string input = val.ToLower().Replace("word", ",word").Replace("lemma", ",lemma").Replace(" ", string.Empty).Replace("\t", string.Empty);
            string[] parts = val.Split(',', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
            {
                if (!parts[0].Contains(':'))
                {
                    var result = QSimilarity.SimilarityFromString(parts[0], baseline);
                    this.Value = (
                        result >= (byte)SIMILARITY.FUZZY_MIN && result <= (byte)SIMILARITY.EXACT ? result : baseline != null ? baseline.SearchSimilarity.word : DEFAULT.word,
                        result >= (byte)SIMILARITY.FUZZY_MIN && result <= (byte)SIMILARITY.EXACT ? result : baseline != null ? baseline.SearchSimilarity.lemma : DEFAULT.lemma);
                    return;
                }
                parts = parts[0].Split(':', StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    byte result = QSimilarity.SimilarityFromString(parts[1], baseline);
                    switch (parts[0])
                    {
                        case "word":
                            this.Value = (
                                result >= (byte)SIMILARITY.FUZZY_MIN && result <= (byte)SIMILARITY.EXACT ? result : baseline != null ? baseline.SearchSimilarity.word : DEFAULT.word,
                                baseline != null ? baseline.SearchSimilarity.lemma : DEFAULT.lemma);
                            break;

                        case "lemma":
                            this.Value = (
                                baseline != null ? baseline.SearchSimilarity.word : DEFAULT.word,
                                result >= (byte)SIMILARITY.FUZZY_MIN && result <= (byte)SIMILARITY.EXACT ? result : baseline != null ? baseline.SearchSimilarity.lemma : DEFAULT.lemma);
                            break;
                    }
                    return;
                }
                this.Value = DEFAULT;
                return;
            }
        }

        public QSimilarityLemma(string sword, string slemma, ISettings? baseline = null)
        {
            var word = QSimilarity.SimilarityFromString(sword);
            var lemma = QSimilarity.SimilarityFromString(slemma);

            this.Value = (
                word >= (byte)SIMILARITY.FUZZY_MIN && word <= (byte)SIMILARITY.EXACT ? word : baseline != null ? baseline.SearchSimilarity.word : DEFAULT.word,
                lemma >= (byte)SIMILARITY.FUZZY_MIN && lemma <= (byte)SIMILARITY.EXACT ? lemma : baseline != null ? baseline.SearchSimilarity.lemma : DEFAULT.lemma);
        }
        public override string ToString()
        {
            string result = "word: " + QSimilarity.SimilarityToString(this.Value.word) + ", "
                          + "lemma: " + QSimilarity.SimilarityToString(this.Value.lemma);

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
        public const QLexiconVal DEFAULT = QLexiconVal.BOTH;
        public QLexiconVal Value { get; set; }
        public QLexicon()
        {
            Value = DEFAULT;
        }
        public QLexicon(QLexiconVal val)
        {
            Value = val;
        }
        public QLexicon(string val)
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
                case QLexiconVal.AV: return "av";
                case QLexiconVal.AVX: return "avx";
                case QLexiconVal.BOTH: return "both";
                default: return ToString(QLexicalDomain.DEFAULT);
            }
        }
        public override string ToString()
        {
            return ToString(this.Value);
        }
        public string AsYaml()
        {
            return "lexicon: " + ToString();
        }
    }
    public class QLexicalDomain: ISetting
    {
        public static readonly string Name = typeof(QLexicalDomain).Name.Substring(8).ToLower();
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
        public static readonly string Name = "render";
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
                case "span": return QSpan.DEFAULT.ToString();
                case "lexicon.search":
                case "search": return QLexicalDomain.DEFAULT.ToString();
                case "lexicon.render":
                case "render": return QLexicalDisplay.DEFAULT.ToString();
                case "format": return QFormat.DEFAULT.ToString();
                case "similarity": return QSimilarity.DEFAULT.ToString();
            }
            return string.Empty;
        }
        public static QAssign? CreateAssignment(QContext env, string text, Parsed[] args)
        {
            if (args.Length == 1)
            {
                if (args[0].children.Length == 2
                && args[0].children[0].rule.EndsWith("_key", StringComparison.InvariantCultureIgnoreCase)
                && args[0].children[1].rule.EndsWith("_option", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new QAssign(env, text, args[0].children[0].text, args[0].children[1].text);
                }
            }
            return null;
        }
    }
}