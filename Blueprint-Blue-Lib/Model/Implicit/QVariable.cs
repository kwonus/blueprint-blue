// AVX-Quelle specification: 2.0.3.701
//
namespace Blueprint.Blue
{
    using Pinshot.PEG;
    using System;
    using System.Runtime.CompilerServices;
    using static global::Blueprint.Blue.QLexicalDomain;

    internal enum SIMILARITY { NONE = 0, FUZZY_MIN = 33, FUZZY_MAX = 99, EXACT = 100 }

    public class QSpan
    {
        public const UInt16 VERSE = 0;    // zero means verse-scope
        public const UInt16 DEFAULT = QSpan.VERSE;
        public UInt16 Value { get; set; }
        public QSpan()
        {
            this.Value = QSpan.VERSE;
        }
        public QSpan(UInt16 val)
        {
            this.Value = val;
        }
        public QSpan(string val)
        {
            this.Value = QSpan.FromString(val);
        }
        public static UInt16 FromString(string val)
        {
            string test = val.Trim();
            if (test.Equals("verse", StringComparison.InvariantCultureIgnoreCase))
            {
                return QSpan.VERSE;
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
            return QSpan.VERSE;
        }
        public override string ToString()
        {
            return this.Value.ToString();
        }
        public string AsYaml()
        {
            return "span: " + this.ToString();
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
            this.Value = QSimilarity.DEFAULT;
            this.EnableLemmaMatching = false;
        }
        public QSimilarity(byte val)
        {
            this.Value = val >= (byte)SIMILARITY.FUZZY_MIN && val <= (byte)SIMILARITY.EXACT ? val : (byte) 0;
            this.EnableLemmaMatching = (val >= 33);
        }
        public QSimilarity(string val)
        {
            var result = QSimilarity.FromString(val);
            this.EnableLemmaMatching = result.automaticLemmaMatching;
            this.Value = result.threshold;
        }
        public static (bool automaticLemmaMatching, byte threshold) FromString(string val)
        {
            (bool enableLemmaMatching, byte threshold) result;

            result.enableLemmaMatching = !val.EndsWith('!');
            string value = !result.enableLemmaMatching ? val : val.Substring(0, val.Length - 1).Trim();

            if (value.Equals("none", StringComparison.InvariantCultureIgnoreCase))
                return (false, 0);
            if (value.Equals("exact", StringComparison.InvariantCultureIgnoreCase))
                return (false, 100);
            if (value.Length != 2)
                return (false, QSimilarity.DEFAULT);
            try
            {
                result.threshold = (byte)UInt16.Parse(value);
                return result;
            }
            catch
            {
                return (false, QSimilarity.DEFAULT);
            }
        }
        public override string ToString()
        {
            if (this.Value < (byte)SIMILARITY.FUZZY_MIN || this.Value > (byte)SIMILARITY.EXACT)
                return "none";

            string result = this.Value.ToString();

            return !this.EnableLemmaMatching ? result : result + '!';
        }
        public string AsYaml()
        {
            return "similarity: " + this.ToString();
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
            this.Value = QLexicalDomain.DEFAULT;
        }
        public QLexicalDomain(QLexiconVal val)
        {
            this.Value = val;
        }
        public QLexicalDomain(string val)
        {
            this.Value = QLexicalDomain.FromString(val);
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
                default:     return QLexicalDomain.DEFAULT;
            }
        }
        public static string ToString(QLexiconVal val)
        {
            switch (val)
            {
                case QLexiconVal.AV:  return "av";
                case QLexiconVal.AVX: return "avx";
                default:             return QLexicalDomain.DEFAULT.ToString();
            }
        }
        public override string ToString()
        {
            return QLexicalDomain.ToString(this.Value);
        }
        public string AsYaml()
        {
            return "lexicon: " + this.ToString();
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
            this.Value = QLexicalDisplay.DEFAULT;
        }
        public QLexicalDisplay(QDisplayVal val)
        {
            this.Value = val;
        }
        public QLexicalDisplay(string val)
        {
            this.Value = QLexicalDisplay.FromString(val);
        }
        public static QDisplayVal FromString(string val)
        {
            switch (val.Trim().ToUpper())
            {
                case "KJV":
                case "AV":  return QDisplayVal.AV;
                case "MODERN":
                case "MOD":
                case "AVX": return QDisplayVal.AVX;
                default:    return QLexicalDisplay.DEFAULT;
            }
        }
        public static string ToString(QDisplayVal val)
        {
            switch (val)
            {
                case QDisplayVal.AV: return "av";
                case QDisplayVal.AVX: return "avx";
                default: return QLexicalDisplay.DEFAULT.ToString();
            }
        }
        public override string ToString()
        {
            return QLexicalDisplay.ToString(this.Value);
        }
        public string AsYaml()
        {
            return "display: " + this.ToString();
        }
    }
    public class QFormat
    {
        public enum QFormatVal
        {
            JSON = 0,
            TEXT = 1,
            HTML = 2,
            MD = 3
        }
        public const QFormatVal DEFAULT = QFormatVal.JSON;
        public QFormatVal Value { get; set; }
        public QFormat()
        {
            this.Value = QFormat.DEFAULT;
        }
        public QFormat(QFormatVal val)
        {
            this.Value = val;
        }
        public QFormat(string val)
        {
            this.Value = QFormat.FromString(val);
        }
        public static QFormatVal FromString(string val)
        {
            switch (val.Trim().ToUpper())
            {
                case "JSON": return QFormatVal.JSON;
                case "TEXT": return QFormatVal.TEXT;
                case "HTML": return QFormatVal.HTML;
                case "MD":   return QFormatVal.MD;
                default:     return QFormat.DEFAULT;
            }
        }
        public static string ToString(QFormatVal val)
        {
            switch (val)
            {
                case QFormatVal.JSON: return "json";
                case QFormatVal.TEXT: return "text";
                case QFormatVal.HTML: return "html";
                case QFormatVal.MD:   return "md";
                default:              return QFormat.DEFAULT.ToString();
            }
        }
        public override string ToString()
        {
            return QFormat.ToString(this.Value);
        }
        public string AsYaml()
        {
            return "format: " + this.ToString();
        }
    }
    public abstract class QVariable : QImplicitCommand, ICommand
    {
        public string Key { get; protected set; }
        public string Value { get; protected set; }
        public bool Persistent { get; protected set; }

        protected QVariable(QContext env, string text, string verb, string key, bool persistent, string value = "") : base(env, text, verb)
        {
            this.Key = key;
            this.Persistent = persistent;

            if (string.IsNullOrWhiteSpace(value))
            {
                this.Value = QVariable.GetDefault(this.Key);
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
                case "span":    return QSpan.DEFAULT.ToString();
                case "lexicon": return QLexicalDomain.DEFAULT.ToString();
                case "display": return QLexicalDomain.DEFAULT.ToString();
                case "format":  return QFormat.DEFAULT.ToString();
                case "similarity": return QSimilarity.DEFAULT.ToString();
            }
            return string.Empty;
        }
        public static QVariable? Create(QContext env, string text, Parsed[] args)
        {
            if (args.Length == 1)
            { 
                if((args[0].children.Length == 2)
                &&  args[0].children[0].rule.EndsWith("_key", StringComparison.InvariantCultureIgnoreCase)
                &&  args[0].children[1].rule.EndsWith("_option", StringComparison.InvariantCultureIgnoreCase))
                {
                    switch (args[0].children[1].text.ToLower())
                    {
                        case "default":
                            if (args[0].rule.EndsWith("_set"))
                                return new QClear(env, text, args[0].children[0].text, true);
                            else if (args[0].rule.EndsWith("_var"))
                                return new QClear(env, text, args[0].children[0].text, true);
                            break;

                        default:
                            if (args[0].rule.EndsWith("_set"))
                                return new QSet(env, text, args[0].children[0].text, args[0].children[1].text, true);
                            else if (args[0].rule.EndsWith("_var"))
                                return new QSet(env, text, args[0].children[0].text, args[0].children[1].text, false);
                            break;
                    }
                }
            }
            return null;
        }
        public override List<string> AsYaml()
        {
            return ICommand.YamlSerializer(this);
        }
    }
}