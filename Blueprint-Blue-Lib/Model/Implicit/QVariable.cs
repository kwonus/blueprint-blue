// AVX-Quelle specification: 2.0.3.701
//
namespace Blueprint.Model.Implicit
{
    using Blueprint.Blue;
    using Pinshot.PEG;
    using System;
    using System.Data;
    using static global::Blueprint.Model.Implicit.QLexicalDomain;

    internal enum SIMILARITY { NONE = 0, FUZZY_MIN = 33, FUZZY_MAX = 99, EXACT = 100 }

    public class QSpan
    {
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
    public class QSimilarity
    {
        public static byte DEFAULT
        {
            get
            {
                return (byte)SIMILARITY.NONE;
            }
        }
        public byte Value { get; private set; }
        public bool EnableLemmaMatching { get; private set; }
        public QSimilarity()
        {
            Value = DEFAULT;
            EnableLemmaMatching = false;
        }
        public QSimilarity(byte val)
        {
            Value = val >= (byte)SIMILARITY.FUZZY_MIN && val <= (byte)SIMILARITY.EXACT ? val : (byte)0;
            EnableLemmaMatching = val >= 33;
        }
        public QSimilarity(string val)
        {
            var result = FromString(val);
            EnableLemmaMatching = result.automaticLemmaMatching;
            Value = result.threshold;
        }
        public static (bool automaticLemmaMatching, byte threshold) FromString(string val)
        {
            (bool enableLemmaMatching, byte threshold) result;

            result.enableLemmaMatching = val.EndsWith('!');
            string value = result.enableLemmaMatching ? val.Substring(0, val.Length - 1).Trim() : val;

            if (value.Equals("none", StringComparison.InvariantCultureIgnoreCase))
                return (false, 0);
            if (value.Equals("exact", StringComparison.InvariantCultureIgnoreCase))
                return (false, 100);
            if (value.Length != 2)
                return (false, DEFAULT);
            try
            {
                result.threshold = (byte)ushort.Parse(value);
                return result;
            }
            catch
            {
                return (false, DEFAULT);
            }
        }
        public override string ToString()
        {
            string result = this.Value == 0 ? "exact" : this.Value.ToString();

            if (Value < (byte)SIMILARITY.FUZZY_MIN || Value > (byte)SIMILARITY.EXACT)
                result = "exact";

            return this.EnableLemmaMatching && result != "exact" ? result + '!' : result;
        }
        public string AsYaml()
        {
            return "similarity: " + ToString();
        }
    }
    public class QLexicalDomain
    {
        public enum QLexiconVal
        {
            UNDEFINED = 0,
            AV = 1,
            AVX = 2,
            BOTH = 3
        }
        public const QLexiconVal DEFAULT = QLexiconVal.BOTH;
        public QLexiconVal Value { get; set; }
        public QLexicalDomain()
        {
            Value = DEFAULT;
        }
        public QLexicalDomain(QLexiconVal val)
        {
            Value = val;
        }
        public QLexicalDomain(string val)
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
                case QLexiconVal.AV:   return "av";
                case QLexiconVal.AVX:  return "avx";
                case QLexiconVal.BOTH: return "both";
                default:               return ToString(QLexicalDomain.DEFAULT);
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
    public class QLexicalDisplay
    {
        public enum QDisplayVal
        {
            UNDEFINED,
            AV = 1,
            AVX = 2
        }
        public const QDisplayVal DEFAULT = QDisplayVal.AV;
        public QDisplayVal Value { get; set; }
        public QLexicalDisplay()
        {
            Value = DEFAULT;
        }
        public QLexicalDisplay(QDisplayVal val)
        {
            Value = val;
        }
        public QLexicalDisplay(string val)
        {
            Value = FromString(val);
        }
        public static QDisplayVal FromString(string val)
        {
            switch (val.Trim().ToUpper())
            {
                case "KJV":
                case "AV": return QDisplayVal.AV;
                case "MODERN":
                case "MOD":
                case "AVX": return QDisplayVal.AVX;
                default: return DEFAULT;
            }
        }
        public static string ToString(QDisplayVal val)
        {
            switch (val)
            {
                case QDisplayVal.AV:  return "av";
                case QDisplayVal.AVX: return "avx";
                default: return ToString(QLexicalDisplay.DEFAULT);
            }
        }
        public override string ToString()
        {
            return ToString(this.Value);
        }
        public string AsYaml()
        {
            return "display: " + ToString();
        }
    }
    public class QFormat
    {
        public enum QFormatVal
        {
            JSON = 0,
            YAML = 1,
            TEXT = 2,
            HTML = 3,
            MD = 4
        }
        public const QFormatVal DEFAULT = QFormatVal.JSON;
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
                case "lexicon": return QLexicalDomain.DEFAULT.ToString();
                case "display": return QLexicalDomain.DEFAULT.ToString();
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