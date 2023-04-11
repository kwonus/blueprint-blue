using Pinshot.PEG;
using System.Net.NetworkInformation;
using System.Text.Json.Nodes;
using static Blueprint.Blue.QDomain;

namespace Blueprint.Blue
{
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
        public static string ToString(QDomainVal val)
        {
            return val.ToString();
        }
        public override string ToString()
        {
            return this.Value.ToString();
        }
    }
    public class QExact
    {
        public const bool DEFAULT = false;
        public bool Value { get; set; }
        public QExact()
        {
            this.Value = QExact.DEFAULT;
        }
        public QExact(bool val)
        {
            this.Value = val;
        }
        public QExact(string val)
        {
            this.Value = QExact.FromString(val);
        }
        public static bool FromString(string val)
        {
            string test = val.Trim().ToLower();
            return test == "f"
                || test == "false"
                || test == "y"
                || test == "yes"
                || test == "1";
        }
        public static string ToString(bool val)
        {
            return val ? "true" : "false";
        }
        public override string ToString()
        {
            return this.Value ? "true" : "false";
        }
    }
    public class QDomain
    {
        public enum QDomainVal
        {
            AV = 0,
            AVX = 1
        }
        public const QDomainVal DEFAULT = QDomainVal.AV;
        public QDomainVal Value { get; set; }
        public QDomain()
        {
            this.Value = QDomain.DEFAULT;
        }
        public QDomain(QDomainVal val)
        {
            this.Value = val;
        }
        public QDomain(string val)
        {
            this.Value = QDomain.FromString(val);
        }
        public static QDomainVal FromString(string val)
        {
            switch (val.Trim().ToUpper())
            {
                case "AV":  return QDomainVal.AV;
                case "AVX": return QDomainVal.AVX;
                default:    return QDomain.DEFAULT;
            }
        }
        public static string ToString(QDomainVal val)
        {
            switch (val)
            {
                case QDomainVal.AV:  return "av";
                case QDomainVal.AVX: return "avx";
                default:             return QDomain.DEFAULT.ToString();
            }
        }
        public override string ToString()
        {
            return QDomain.ToString(this.Value);
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
                case "span":   return QSpan.DEFAULT.ToString();
                case "domain": return QDomain.DEFAULT.ToString();
                case "exact":  return QExact.DEFAULT.ToString();
                case "format": return QFormat.DEFAULT.ToString();
            }
            return string.Empty;
        }
        public static QVariable? Create(QContext env, string text, Parsed[] args)
        {
            if (args.Length == 1)
            { 
                if (args[0].rule == "global_reset")
                {
                    return new QClear(env, text, args[0].rule, true);
                }
                else if (args[0].rule == "local_reset")
                {
                    return new QClear(env, text, args[0].rule, false);
                }
                else if (args[0].rule == "local_current")
                {
                    return new QCurrent(env, text, args[0].rule, false);
                }
                else if((args[0].children.Length == 2)
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
    }
}