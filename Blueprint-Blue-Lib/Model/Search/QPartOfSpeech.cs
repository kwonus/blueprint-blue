namespace Blueprint.Blue
{
    using AVSearch.Interfaces;
    using AVSearch.Model.Features;
    using AVXLib.Framework;
    using Pinshot.PEG;
    using System;
    using System.Collections.Generic;

    public class PNPOS
    {
        public byte person;
        public NUMBER number;
        public GENDER gender;
        public CASE case_;
        public string pos;

        public PNPOS()
        {
            this.person = 0;
            this.number = NUMBER.Unknown;
            this.gender = GENDER.Unknown;
            this.case_ = CASE.Unknown;
            this.pos = string.Empty;
        }
    }

    public enum NUMBER
    {
        Unknown = 0,
        Singular = 1,
        Plural = 2,
    };
    public enum GENDER
    {
        Neuter = -1,
        Unknown = 0,
        Male = 1,
        Female = 2,
    };
    public enum CASE
    {
        Unknown = 0,
        Genitive = 1,
        Nominative = 2,
        Objective = 3,
        Reflexive = 4,
    };

    public class QPartOfSpeech : FeaturePartOfSpeech
    { 
        public static PNPOS GetPnPos(UInt16 pnpos)
        {
            PNPOS result = new();

            foreach (var item in from x in QPartOfSpeech.PnPosTable
                                 where x.Value.mask == Numerics.PersonNumber.PersonBits && (x.Value.mask & pnpos) == x.Value.value
                                 select x.Value.value)
            {
                if (item == Numerics.PersonNumber.Person1st)
                    result.person = 1;
                else if (item == Numerics.PersonNumber.Person2nd)
                    result.person = 2;
                else if (item == Numerics.PersonNumber.Person3rd)
                    result.person = 3;
                break;
            }
            foreach (var item in from x in QPartOfSpeech.PnPosTable
                                 where x.Value.mask == Numerics.PersonNumber.NumberBits && (x.Value.mask & pnpos) == x.Value.value
                                 select x)
            {
                if (item.Value.value == Numerics.PersonNumber.WH)
                    result.pos = item.Key;
                else if (item.Value.value == Numerics.PersonNumber.Plural)
                    result.number = NUMBER.Plural;
                else if (item.Value.value == Numerics.PersonNumber.Singular)
                    result.number = NUMBER.Singular;
                break;
            }

            foreach (var item in from x in QPartOfSpeech.PnPosTable
                                 where (x.Value.mask & Numerics.PersonNumber.PersonBits) == 0
                                    && (x.Value.mask & Numerics.PersonNumber.NumberBits) == 0
                                    && (x.Value.mask & pnpos) == x.Value.value
                                 select x)
            {
                if (item.Value.value == Numerics.POS12.PronounOrNoun_Genitive)
                {
                    result.case_ = CASE.Genitive;
                }
                else if (result.pos.Length == 0)
                {
                    int index = item.Key.IndexOf("_");

                    if (index == 0 || index == item.Key.Length-1) // There MUST be two parts
                        continue;

                    if (index < 0)
                    {
                        if (item.Key == "/genitive/")
                            result.case_ = CASE.Genitive;
                        else
                            result.pos = item.Key;
                    }
                    else
                    {
                        if (item.Key == "/any_gen/")
                        {
                            result.case_ = CASE.Genitive;
                            continue;
                        }

                        result.pos = item.Key.Substring(0, index);
                        if (result.pos == "/pn")
                            result.pos = "/pronoun/";
                        else
                            result.pos += '/';

                        switch (item.Key.Substring(index + 1))
                        {
                            case "neuter/":     result.gender = GENDER.Neuter; break;

                            case "masculine/":
                            case "male/":       result.gender = GENDER.Male; break;

                            case "feminine/":
                            case "fem/":        result.gender = GENDER.Female; break;

                            case "genitive/":
                            case "gen/":        result.case_ = CASE.Genitive; break;

                            case "nominative/":
                            case "nom/":        result.case_ = CASE.Nominative; break;

                            case "objective/":
                            case "obj/":        result.case_ = CASE.Objective; break;

                            case "reflexive/":
                            case "rfx/":        result.case_ = CASE.Reflexive; break;
                        }
                        break;
                    }
                }
            }
            return result;
        }
        private static Dictionary<string, (UInt16 value, UInt16 mask)> PnPosTable { get; set; } = new()
        {
            { "/1p/",           (Numerics.PersonNumber.Person1st,   Numerics.PersonNumber.PersonBits) },
            { "/2p/",           (Numerics.PersonNumber.Person2nd,   Numerics.PersonNumber.PersonBits) },
            { "/3p/",           (Numerics.PersonNumber.Person3rd,   Numerics.PersonNumber.PersonBits) },
            { "/noun/",         (Numerics.POS12.Noun,               Numerics.POS12.NounOrPronoun)     },
            { "/n/",            (Numerics.POS12.Noun,               Numerics.POS12.NounOrPronoun)     },
            { "/verb/",         (Numerics.POS12.Verb,               Numerics.POS12.NonNoun)           },
            { "/v/",            (Numerics.POS12.Verb,               Numerics.POS12.NonNoun)           },
            { "/pronoun/",      (Numerics.POS12.Pronoun,            Numerics.POS12.NounOrPronoun)     },
            { "/pn/",           (Numerics.POS12.Pronoun,            Numerics.POS12.NounOrPronoun)     },
            { "/adjective/",    (Numerics.POS12.Adjective,          Numerics.POS12.NonNoun)           },
            { "/adj/",          (Numerics.POS12.Adjective,          Numerics.POS12.NonNoun)           },
            { "/adverb/",       (Numerics.POS12.Adverb,             Numerics.POS12.NonNoun)           },
            { "/adv/",          (Numerics.POS12.Adverb,             Numerics.POS12.NonNoun)           },
            { "/determiner/",   (Numerics.POS12.Determiner,         Numerics.POS12.NonNoun)           },
            { "/det/",          (Numerics.POS12.Determiner,         Numerics.POS12.NonNoun)           },
            { "/particle/",     (Numerics.POS12.Particle,           Numerics.POS12.NonNoun)           },
            { "/part/",         (Numerics.POS12.Particle,           Numerics.POS12.NonNoun)           },
            { "/wh/",           (Numerics.PersonNumber.WH,          Numerics.PersonNumber.NumberBits) },
            { "/singular/",     (Numerics.PersonNumber.Singular,    Numerics.PersonNumber.NumberBits) },
            { "/pural/",        (Numerics.PersonNumber.Plural,      Numerics.PersonNumber.NumberBits) },

            { "/preposition/",  (Numerics.POS12.Preposition,        Numerics.POS12.NonNoun)           },
            { "/prep/",         (Numerics.POS12.Preposition,        Numerics.POS12.NonNoun)           },
            { "/interjection/", (Numerics.POS12.Interjection,       Numerics.POS12.NonNoun)           },
            { "/inter/",        (Numerics.POS12.Interjection,       Numerics.POS12.NonNoun)           },
            { "/conjunction/",  (Numerics.POS12.Conjunction,        Numerics.POS12.NonNoun)           },
            { "/conj/",         (Numerics.POS12.Conjunction,        Numerics.POS12.NonNoun)           },
            { "/numeric/",      (Numerics.POS12.Numeric,            Numerics.POS12.NonNoun)           },
            { "/num/",          (Numerics.POS12.Numeric,            Numerics.POS12.NonNoun)           },

            { "/pn_neuter/",    (Numerics.POS12.Pronoun_Neuter,     Numerics.POS12.Pronoun_Neuter     | Numerics.POS12.NounOrPronoun) },
            { "/pn_masculine/", (Numerics.POS12.Pronoun_Masculine,  Numerics.POS12.Pronoun_Masculine  | Numerics.POS12.NounOrPronoun) },
            { "/pn_male/",      (Numerics.POS12.Pronoun_Masculine,  Numerics.POS12.Pronoun_Masculine  | Numerics.POS12.NounOrPronoun) },
            { "/pn_feminine/",  (Numerics.POS12.Pronoun_Feminine,   Numerics.POS12.Pronoun_Feminine   | Numerics.POS12.NounOrPronoun) },
            { "/pn_fem/",       (Numerics.POS12.Pronoun_Feminine,   Numerics.POS12.Pronoun_Feminine   | Numerics.POS12.NounOrPronoun) },

            { "/genitive/",     (Numerics.POS12.PronounOrNoun_Genitive, Numerics.POS12.PronounOrNoun_Genitive)  },
            { "/any_gen/",      (Numerics.POS12.PronounOrNoun_Genitive, Numerics.POS12.PronounOrNoun_Genitive)  },
            { "/noun_genitive/",(Numerics.POS12.Noun_Genitive,          Numerics.POS12.PronounOrNoun_Genitive)  },
            { "/n_gen/",        (Numerics.POS12.Noun_Genitive,          Numerics.POS12.PronounOrNoun_Genitive)  },
            { "/pn_genitive/",  (Numerics.POS12.Pronoun_Genitive,       Numerics.POS12.PronounOrNoun_Genitive_MASK) },
            { "/pn_gen/",       (Numerics.POS12.Pronoun_Genitive,       Numerics.POS12.PronounOrNoun_Genitive_MASK) },
            { "/pn_nominative/",(Numerics.POS12.Pronoun_Nominative,     Numerics.POS12.Pronoun_Nominative | Numerics.POS12.NounOrPronoun) },
            { "/pn_nom/",       (Numerics.POS12.Pronoun_Nominative,     Numerics.POS12.Pronoun_Nominative | Numerics.POS12.NounOrPronoun) },
            { "/pn_objective/", (Numerics.POS12.Pronoun_Objective,      Numerics.POS12.Pronoun_Objective  | Numerics.POS12.NounOrPronoun) },
            { "/pn_obj/",       (Numerics.POS12.Pronoun_Objective,      Numerics.POS12.Pronoun_Objective  | Numerics.POS12.NounOrPronoun) },
            { "/pn_reflexive/", (Numerics.POS12.Pronoun_Reflexive,      Numerics.POS12.Pronoun_Reflexive  | Numerics.POS12.NounOrPronoun) },
            { "/pn_rfx/",       (Numerics.POS12.Pronoun_Reflexive,      Numerics.POS12.Pronoun_Reflexive  | Numerics.POS12.NounOrPronoun) }
        };
        static QPartOfSpeech()
        {
            ;
        }

        public static ((UInt16 value, UInt16 mask) pnpos, bool found, bool numeric) Lookup(string pnpos)
        {
            if (!string.IsNullOrEmpty(pnpos))
            {
                ((UInt16 value, UInt16 mask) pnpos, bool found, bool numeric) entry = ((0, 0), QPartOfSpeech.PnPosTable.ContainsKey(pnpos), false);

                if (entry.found)
                {
                    entry.pnpos = QPartOfSpeech.PnPosTable[pnpos];
                }
                else if (pnpos.StartsWith('#') && pnpos.Length == 5) // #FFFF
                {
                    foreach (char c in pnpos.Substring(1))
                    {
                        entry.numeric = char.IsAsciiHexDigit(c);

                        if (!entry.numeric)
                            break;
                    }
                    if (entry.numeric)
                    {
                        entry.found = true;
                        UInt16 hex = UInt16.Parse(pnpos, System.Globalization.NumberStyles.HexNumber);
                        entry.pnpos = (hex, hex);
                    }
                }
                return entry;
            }
            return ((0, 0), false, false);
        }

        public static (UInt32 pos32, bool found, bool numeric) Decode(string pos32)
        {
            if (!string.IsNullOrEmpty(pos32))
            {
                (UInt32 pos32, bool found, bool numeric) entry = (0, !string.IsNullOrEmpty(pos32), false);

                if (entry.found)
                {
                    entry.pos32 = FiveBitEncoding.EncodePOS(pos32);
                    entry.found = (entry.pos32 > 0);
                }
                if (!entry.found)
                {
                    if (pos32.StartsWith('#') && pos32.Length == 9) // #FFFFFFFF
                    {
                        foreach (char c in pos32.Substring(1))
                        {
                            entry.numeric = char.IsAsciiHexDigit(c);

                            if (!entry.numeric)
                                break;
                        }
                        if (entry.numeric)
                        {
                            entry.found = true;
                            entry.pos32 = UInt32.Parse(pos32, System.Globalization.NumberStyles.HexNumber);
                        }
                    }
                }
                return entry;
            }
            return (0, false, false);
        }

        public QPartOfSpeech(QFind search, string text, Parsed parse, bool negate) : base(text, negate, search.Settings)
        {
            var pos = this.Text.ToLower().Replace(" ", "");

            this.PnPos12 = (0, 0);
            this.Pos32 = 0;

            var entry16 = QPartOfSpeech.Lookup(pos);

            if (entry16.found)
            {
                this.PnPos12 = entry16.pnpos;
            }
            else if (parse.children.Length == 1)
            {
                var child = parse.children[0];

                var entry32 = QPartOfSpeech.Decode(child.text);

                if (entry32.found)
                {
                    this.Pos32 = entry32.pos32;
                }
            }
            if (this.Pos32 == 0 && (this.PnPos12.value == 0 || this.PnPos12.mask == 0))
            {
                search.AddError("Unable to determine part-ofospeech from: \"" + text + "\"");
            }
        }
    }
}