namespace Blueprint.FuzzyLex
{
    using AVXLib;
    using AVXLib.Memory;
    using Blueprint.Blue;
    using PhonemeEmbeddings;
    using System;
    using System.Collections.Generic;

    public interface ILexicalComparitor
    {
        UInt16 wkey { get; }
        string Lex { get; }
        string Mod { get; }

        byte[] ELex { get; }    // E is shorthand for Embeddings
        byte[] EMod { get; }    // E is shorthand for Embeddings

        string PLex { get; }    // P is shorthand for Phonetics
        string PMod { get; }    // P is shorthand for Phonetics
    }
    public class BlueprintLemma : ILexicalComparitor // Blueprint Lemma is always a modern Lemma, unless it is identical to the Lex entry (not sure if the data has those conditions, but it doesn't matter for comparisons, because we don't look at archaic lemmas in our comparison logic)
    {
        public static byte MaxELemmaLen { get; private set; } = 0;

        public UInt16 wkey { get; private set; }
        private string Lemma;
        private byte[] ELemma;
        private string PLemma;
        public string Lex { get => this.Lemma; }
        public string Mod { get => this.Lemma; }
        public byte[] ELex { get => this.ELemma; }  // E is shorthand for Embeddings
        public string PLex { get => this.PLemma; }  // P is shorthand for Phonetics
        public byte[] EMod { get => this.ELemma; }  // E is shorthand for Embeddings
        public string PMod { get => this.PLemma; }  // P is shorthand for Phonetics
        public static byte MaxLen { get; private set; } = 0;
        public static Dictionary<byte, Dictionary<UInt16, ILexicalComparitor>> LemmaPartition { get; private set; } = new(); // shared between Lex & Mod
        public static Dictionary<UInt16, string> OOVLemma { get; private set; } = new(); // shared between Lex & Mod
        public static Dictionary<string, BlueprintLemma> OOVMap { get; private set; } = new();
        public static void Initialize(AVXLib.ObjectTable avxobjects)
        {
            if (BlueprintLemma.OOVMap.Count == 0)
            {
                foreach (var key in avxobjects.oov.Keys)
                {
                    var entry = avxobjects.oov.GetEntry(key);
                    if (entry.valid)
                        new BlueprintLemma(key, entry.oov.text.ToString());
                }
                BlueprintLex.Initialize(avxobjects); // safe reciprocle inits
            }
        }
        private BlueprintLemma(UInt16 key, string text)
        {
            if (text.Length > 0 && key != 0)
            {
                this.wkey = key;
                this.Lemma = text;
                var lemma = new NUPhoneGen(text);
                this.PLemma = lemma.Phonetic;
                var elen = this.PLemma.Length;

                if (elen > 0 && elen <= byte.MaxValue)
                {
                    var blen = (byte)elen;

                    if (elen > BlueprintLemma.MaxELemmaLen)
                        BlueprintLemma.MaxELemmaLen = blen;
                    this.ELemma = lemma.Embeddings;

                    if (!BlueprintLemma.LemmaPartition.ContainsKey(blen))
                        BlueprintLemma.LemmaPartition[blen] = new();
                    BlueprintLemma.LemmaPartition[blen][this.wkey] = this;
                }
                else
                {
                    this.ELemma = new byte[0];
                }
            }
            else
            {
                this.wkey = 0;
                this.Lemma = string.Empty;
                this.PLemma = string.Empty;
                this.ELemma = new byte[0];
            }
        }
    }
    public class BlueprintLex: ILexicalComparitor
    {
        public static byte MaxELexLen { get; private set; } = 0;
        public static byte MaxEModLen { get; private set; } = 0;
        public static void Initialize(AVXLib.ObjectTable avxobjects)
        {
            if (BlueprintLex.LexGlobal.Count == 0)
            {
                UInt16 cnt = avxobjects.lexicon.RecordCount;
                for (UInt16 wkey = 1; wkey < cnt; wkey++)
                {
                    try
                    {
                        var record = avxobjects.lexicon.GetRecord(wkey);
                        if (record.valid)
                            new BlueprintLex(wkey, record.entry);
                    }
                    catch
                    {
                        Console.WriteLine(wkey.ToString());
                    }
                }
                BlueprintLemma.Initialize(avxobjects); // safe reciprocle inits
            }
        }
        public static Dictionary<UInt16, BlueprintLex> LexGlobal { get; private set; } = new();
        public static Dictionary<byte, Dictionary<UInt16, BlueprintLex>> LexPartition { get; private set; } = new();
        public static Dictionary<byte, Dictionary<UInt16, BlueprintLex>> ModPartition { get; private set; } = new();
        public static Dictionary<string, BlueprintLex> LexMap { get; private set; } = new();
        public static Dictionary<string, BlueprintLex> ModMap { get; private set; } = new();
        public UInt16  wkey { get; private set; }
        public  UInt16  Entities { get; private set; }
        public  ReadOnlyMemory<UInt32> POS { get; private set; }
        public  string  Lex { get; private set; }
        public  string  Mod { get; private set; }
        public bool ModernOrthography { get; private set; }
        public bool ModernPhonetics { get; private set; }
        private string  Display; // display

        public  byte[]  ELex { get; private set; }
        private byte[]? emod;

        public  string  PLex { get; private set; } // phonetical
        private string? pmod; // phonetical

        public byte[] EMod
        {
            get => this.emod != null ? this.emod : this.ELex;
        }
        public string PMod
        {
            get => this.pmod != null ? this.pmod : this.PLex;
        }
        private BlueprintLex(UInt16 wkey, AVXLib.Memory.Lexicon entry)
        {
            this.wkey = wkey;
            this.Lex = LEXICON.ToSearchString(entry);
            this.Mod = LEXICON.IsHyphenated(entry) ? LEXICON.ToSearchString(entry) : LEXICON.ToModernString(entry);
            if (!LEXICON.IsModernSameAsDisplay(entry))
                this.Mod = this.Mod.Replace(" ", ""); // e.g. vilest => most vile
            this.ModernOrthography = !LEXICON.IsModernSameAsDisplay(entry);
            this.Display = LEXICON.ToDisplayString(entry);
            this.POS = entry.POS;
            this.Entities = entry.Entities;

            var search = new NUPhoneGen(this.Lex);
            var elexLen = (byte) search.Phonetic.Length;
            this.PLex = search.Phonetic;
            this.ELex = search.Embeddings;

            byte emodLen = 0;
            if (this.ModernOrthography)
            {
                var modern = new NUPhoneGen(this.Mod);
                emodLen = (byte)modern.Phonetic.Length;
                this.ModernPhonetics = (search.Phonetic != modern.Phonetic);
                this.pmod = this.ModernPhonetics ? modern.Phonetic : null;
                this.emod = this.ModernPhonetics ? modern.Embeddings : null;
            }
            else
            {
                emodLen = elexLen;
                this.ModernPhonetics = false;
                this.pmod = null;
                this.emod = null;
            }

            BlueprintLex.LexGlobal[this.wkey] = this;
            BlueprintLex.LexMap[this.Lex] = this;
            BlueprintLex.ModMap[this.Mod] = this;

            if (elexLen > 0 && elexLen <= byte.MaxValue)
            {
                if (!BlueprintLex.LexPartition.ContainsKey(elexLen))
                    BlueprintLex.LexPartition[elexLen] = new();
                BlueprintLex.LexPartition[elexLen][this.wkey] = this;

                if (elexLen > BlueprintLex.MaxELexLen)
                    BlueprintLex.MaxELexLen = elexLen;
            }
            if (emodLen > 0 && emodLen <= byte.MaxValue)
            {
                if (!BlueprintLex.ModPartition.ContainsKey(emodLen))
                    BlueprintLex.ModPartition[emodLen] = new();
                BlueprintLex.ModPartition[emodLen][this.wkey] = this;

                if (emodLen > BlueprintLex.MaxEModLen)
                    BlueprintLex.MaxEModLen = emodLen;
            }
        }
    }
}
