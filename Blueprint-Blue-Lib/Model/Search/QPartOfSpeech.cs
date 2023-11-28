using AVXLib.Framework;

namespace Blueprint.Blue
{
    using AVXLib.Framework;
    using Pinshot.PEG;
    using System;
    using System.Collections.Generic;
    using XBlueprintBlue;

    public class QPartOfSpeech : QFeature, IFeature
    {
        override public string Type { get => QFeature.GetTypeName(this); }
        public UInt16 PnPos12 { get; private set; }
        public UInt32 Pos32   { get; private set; }

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
            ;
        }

        public static ((UInt16 pnpos, bool negate) result, bool found) Lookup(string pnpos)
        {
            ((UInt16 pnpos, bool negate) result, bool found) entry = ((0, false), QPartOfSpeech.PnPosTable.ContainsKey(pnpos));

            if (entry.found)
                entry.result = QPartOfSpeech.PnPosTable[pnpos];

            return entry;
        }

        public QPartOfSpeech(QFind search, string text, Parsed parse, bool negate) : base(search, text, parse, negate)
        {
            var pnpos = this.Text.ToLower().Replace(" ", "");

            this.PnPos12 = 0;
            this.Pos32 = 0;

            var entry = Lookup(pnpos);

            if (entry.found)
            {
                this.PnPos12 = entry.result.pnpos;
            }
            else if (parse.children.Length == 1)
            {
                var child = parse.children[0];
                if (child.text.StartsWith('#'))
                {
                    var pos = child.text.Substring(1);
                    try
                    {
                        if (child.rule == "nupos")
                        {
                            this.Pos32 = AVText.FiveBitEncoding.EncodePOS(pos);
                        }
                        else if (child.rule == "pos32") // do we really want to support numeric representation of nupos strings?
                        {
                            this.Pos32 = UInt32.Parse(pos);
                        }
                        else if (child.rule == "pn_pos12")
                        {
                            this.PnPos12 = UInt16.Parse(pos);
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