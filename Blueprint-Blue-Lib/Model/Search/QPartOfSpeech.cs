namespace Blueprint.Blue
{
    using AVSearch.Model.Features;
    using AVXLib.Framework;
    using Pinshot.PEG;
    using System;
    using System.Collections.Generic;
    using static AVXLib.Framework.Numerics;

    public class QPartOfSpeech : FeaturePartOfSpeech
    {
        private static Dictionary<string, (UInt16 value, UInt16 mask)> PnPosTable { get; set; } = new()
        {
            { "/1p/",           (Numerics.PersonNumber.Person1st,   Numerics.PersonNumber.PersonBits) },
            { "/2p/",           (Numerics.PersonNumber.Person2nd,   Numerics.PersonNumber.PersonBits) },
            { "/3p/",           (Numerics.PersonNumber.Person3rd,   Numerics.PersonNumber.PersonBits) },
            { "/noun/",         (Numerics.POS12.Noun,               Numerics.POS12.Noun)              },
            { "/n/",            (Numerics.POS12.Noun,               Numerics.POS12.Noun)              },
            { "/verb/",         (Numerics.POS12.Verb,               Numerics.POS12.Verb)              },
            { "/v/",            (Numerics.POS12.Verb,               Numerics.POS12.Verb)              },
            { "/pronoun/",      (Numerics.POS12.Pronoun,            Numerics.POS12.Pronoun)           },
            { "/pn/",           (Numerics.POS12.Pronoun,            Numerics.POS12.Pronoun)           },
            { "/adjective/",    (Numerics.POS12.Adjective,          Numerics.POS12.Adjective)         },
            { "/adj/",          (Numerics.POS12.Adjective,          Numerics.POS12.Adjective)         },
            { "/adverb/",       (Numerics.POS12.Adverb,             Numerics.POS12.Adverb)            },
            { "/adv/",          (Numerics.POS12.Adverb,             Numerics.POS12.Adverb)            },
            { "/determiner/",   (Numerics.POS12.Determiner,         Numerics.POS12.Determiner)        },
            { "/det/",          (Numerics.POS12.Determiner,         Numerics.POS12.Determiner)        },
            { "/particle/",     (Numerics.POS12.Particle,           Numerics.POS12.Particle)          },
            { "/part/",         (Numerics.POS12.Particle,           Numerics.POS12.Particle)          },
            { "/wh/",           (Numerics.PersonNumber.WH,          Numerics.PersonNumber.NumberBits) },
            { "/singular/",     (Numerics.PersonNumber.Singular,    Numerics.PersonNumber.NumberBits) },
            { "/pural/",        (Numerics.PersonNumber.Plural,      Numerics.PersonNumber.NumberBits) },

            { "/preposition/",  (Numerics.POS12.Preposition,        Numerics.POS12.Preposition)       },
            { "/prep/",         (Numerics.POS12.Preposition,        Numerics.POS12.Preposition)       },
            { "/interjection/", (Numerics.POS12.Interjection,       Numerics.POS12.Interjection)      },
            { "/inter/",        (Numerics.POS12.Interjection,       Numerics.POS12.Interjection)      },
            { "/conjunction/",  (Numerics.POS12.Conjunction,        Numerics.POS12.Conjunction)       },
            { "/conj/",         (Numerics.POS12.Conjunction,        Numerics.POS12.Conjunction)       },
            { "/numeric/",      (Numerics.POS12.Numeric,            Numerics.POS12.Numeric)           },
            { "/num/",          (Numerics.POS12.Numeric,            Numerics.POS12.Numeric)           },

            { "/pn_neuter/",    (Numerics.POS12.Pronoun_Neuter,     Numerics.POS12.Pronoun_Neuter)    },
            { "/pn_masculine/", (Numerics.POS12.Pronoun_Masculine,  Numerics.POS12.Pronoun_Masculine) },
            { "/pn_male/",      (Numerics.POS12.Pronoun_Masculine,  Numerics.POS12.Pronoun_Masculine) },
            { "/pn_feminine/",  (Numerics.POS12.Pronoun_Feminine,   Numerics.POS12.Pronoun_Feminine)  },
            { "/pn_fem/",       (Numerics.POS12.Pronoun_Feminine,   Numerics.POS12.Pronoun_Feminine)  },

            { "/genitive/",     (Numerics.POS12.PronounOrNoun_Genitive, Numerics.POS12.PronounOrNoun_Genitive)  },
            { "/any_gen/",      (Numerics.POS12.PronounOrNoun_Genitive, Numerics.POS12.PronounOrNoun_Genitive)  },
            { "/noun_genitive/",(Numerics.POS12.Noun_Genitive,      Numerics.POS12.Noun_Genitive)               },
            { "/n_gen/",        (Numerics.POS12.Noun_Genitive,      Numerics.POS12.Noun_Genitive)               },
            { "/pn_genitive/",  (Numerics.POS12.Pronoun_Genitive,   Numerics.POS12.PronounOrNoun_Genitive_MASK) },
            { "/pn_gen/",       (Numerics.POS12.Pronoun_Genitive,   Numerics.POS12.PronounOrNoun_Genitive_MASK) },
            { "/pn_nominative/",(Numerics.POS12.Pronoun_Nominative, Numerics.POS12.Pronoun_Nominative)},
            { "/pn_nom/",       (Numerics.POS12.Pronoun_Nominative, Numerics.POS12.Pronoun_Nominative)},
            { "/pn_objective/", (Numerics.POS12.Pronoun_Objective,  Numerics.POS12.Pronoun_Objective) },
            { "/pn_obj/",       (Numerics.POS12.Pronoun_Objective,  Numerics.POS12.Pronoun_Objective) },
            { "/pn_reflexive/", (Numerics.POS12.Pronoun_Reflexive,  Numerics.POS12.Pronoun_Reflexive) },
            { "/pn_rfx/",       (Numerics.POS12.Pronoun_Reflexive,  Numerics.POS12.Pronoun_Reflexive) }
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