using AVXLib.Framework;

namespace Blueprint.Blue
{
    using AVXLib.Framework;
    using Pinshot.PEG;
    using XBlueprint;

    public class QPartOfSpeech : QFeature, IFeature
    {
        public UInt16 PnPos12 { get; private set; }
        public UInt32 Pos32   { get; private set; }
        public bool Negate { get; private set; }

        private static Dictionary<string, (UInt16 pnpos, bool negate)> PnPosTable { get; set; } = new()
        {
            { "/1p/",           (Numerics.PersonNumber.Person1st, false) },
            { "/2p/",           (Numerics.PersonNumber.Person2nd, false) },
            { "/3p/",           (Numerics.PersonNumber.Person3rd, false) },
            { "/noun/",         (Numerics.POS12.Noun, false) },
            { "/n/",            (Numerics.POS12.Noun, false) },
            { "/verb/",         (Numerics.POS12.Verb, false) },
            { "/v/",            (Numerics.POS12.Verb, false) },
            { "/pronoun/",      (Numerics.POS12.Pronoun, false) },
            { "/pn/",           (Numerics.POS12.Pronoun, false) },
            { "/adjective/",    (Numerics.POS12.Adjective, false) },
            { "/adj/",          (Numerics.POS12.Adjective, false) },
            { "/adverb/",       (Numerics.POS12.Adverb, false) },
            { "/adv/",          (Numerics.POS12.Adverb, false) },
            { "/determiner/",   (Numerics.POS12.Determiner, false) },
            { "/det/",          (Numerics.POS12.Determiner, false) },
            { "/particle/",     (Numerics.POS12.Particle, false) },
            { "/part/",         (Numerics.POS12.Particle, false) },
            { "/wh/",           (Numerics.PersonNumber.WH, false) },
            { "/singular/",     (Numerics.PersonNumber.Singular, false) },
            { "/pural/",        (Numerics.PersonNumber.Plural, false) },
                               
            { "/preposition/",  (Numerics.POS12.Preposition, false) },
            { "/prep/",         (Numerics.POS12.Preposition, false) },
            { "/interjection/", (Numerics.POS12.Interjection, false) },
            { "/inter/",        (Numerics.POS12.Interjection, false) },
            { "/conjunction/",  (Numerics.POS12.Conjunction, false) },
            { "/conj/",         (Numerics.POS12.Conjunction, false) },
            { "/numeric/",      (Numerics.POS12.Numeric, false) },
            { "/num/",          (Numerics.POS12.Numeric, false) },

            { "/pn_neuter/",    (Numerics.POS12.Pronoun_Neuter, false) },
            { "/pn_masculine/", (Numerics.POS12.Pronoun_Masculine, false) },
            { "/pn_male/",      (Numerics.POS12.Pronoun_Masculine, false) },
            { "/pn_feminine/",  (Numerics.POS12.Pronoun_Feminine, false) },
            { "/pn_fem/",       (Numerics.POS12.Pronoun_Feminine, false) },

            { "/pn_genitive/",  (Numerics.POS12.PronounOrNoun_Genitive, false) },
            { "/pn_gen/",       (Numerics.POS12.PronounOrNoun_Genitive, false) },
            { "/pn_nominative/",(Numerics.POS12.Pronoun_Nominative, false) },
            { "/pn_nom/",       (Numerics.POS12.Pronoun_Nominative, false) },
            { "/pn_objective/", (Numerics.POS12.Pronoun_Objective, false) },
            { "/pn_obj/",       (Numerics.POS12.Pronoun_Objective, false) },
            { "/pn_reflexive/", (Numerics.POS12.Pronoun_Reflexive, false) },
            { "/pn_rfx/",       (Numerics.POS12.Pronoun_Reflexive, false) }

        };
        static QPartOfSpeech()
        {
            var negations = new Dictionary<string, (UInt16 pnpos, bool negate)>();

            foreach (var kv in QPartOfSpeech.PnPosTable)
            {
                var negate = "/!" + kv.Key.Substring(1);
                negations[negate] = (kv.Value.pnpos, true);
            }
            foreach (var kv in negations)
            {
                QPartOfSpeech.PnPosTable[kv.Key] = kv.Value;
            }
        }

        public static ((UInt16 pnpos, bool negate) result, bool found) Lookup(string pnpos)
        {
            ((UInt16 pnpos, bool negate) result, bool found) entry = ((0, false), QPartOfSpeech.PnPosTable.ContainsKey(pnpos));

            if (entry.found)
                entry.result = QPartOfSpeech.PnPosTable[pnpos];

            return entry;
        }

        public QPartOfSpeech(QFind search, string text, Parsed parse) : base(search, text, parse)
        {
            var pnpos = text.ToLower().Replace(" ", "");

            this.PnPos12 = 0;
            this.Pos32 = 0;
            this.Negate = false;

            var entry = Lookup(pnpos);

            if (entry.found)
            {
                this.PnPos12 = entry.result.pnpos;
                this.Negate = entry.result.negate;
            }
            else if (parse.children.Length == 1)
            {
                var child = parse.children[0];
                if (child.text.StartsWith('#'))
                {
                    var hex = child.text.Substring(1);
                    try
                    {
                        if (child.rule == "pos32")
                        {
                            this.Pos32 = UInt32.Parse(hex);
                        }
                        else if (child.rule == "pn_pos12")
                        {
                            this.PnPos12 = UInt16.Parse(hex);
                        }
                    }
                    catch
                    {
                        ;
                    }
                }
            }
            if (this.Pos32 == 0 && this.PnPos12 == 0)
            {
                search.Context.AddError("Unable to determine part-ofospeech from: \"" + text + "\"");
            }
        }
        public override IEnumerable<string> AsYaml()
        {
            yield return "- feature: " + this.Text;
            if (this.PnPos12 != 0)
            {
                yield return "  negate: " + this.Negate.ToString().ToLower();
                yield return "  pos16: 0x" + this.PnPos12.ToString("X");
            }
            else if (this.Pos32 != 0)
            {
                yield return "  negate: " + this.Negate.ToString().ToLower();
                yield return "  pos32: 0x" + this.PnPos12.ToString("X");
            }
        }
        public override XFeature AsMessage()
        {
            XCompare compare;
            if (this.Pos32 != 0)
            {
                var pos = new XPOS32() { Pos = this.Pos32 };
                compare = new XCompare(pos);
            }
            else
            {
                var pos = new XPOS16() { Pnpos = this.PnPos12 };
                compare = new XCompare(pos);
            }
            var feature = new XFeature { Feature = this.Text, Negate = this.Negate, Rule = "decoration", Match = compare };

            return feature;
            
        }
    }
}