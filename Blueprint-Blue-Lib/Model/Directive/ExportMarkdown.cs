namespace Blueprint.Blue
{
    using AVXLib;
    using System;
    using static AVXLib.Framework.Numerics;
    using static Blueprint.Model.Implicit.QFormat;

    public class ExportMarkdown : ExportDirective
    {
        internal ExportMarkdown(QContext env, string spec, FileCreateMode mode) : base(env, spec, QFormatVal.MD, mode)
        {
            ;
        }
        public override DirectiveResultType Update()
        {
            bool br = false;
            TextWriter? writer = null;

            try
            {
                writer = (this.CreationMode == FileCreateMode.Streaming)
                    ? this.Context != null ? new StreamWriter(this.Context.InternalExportStream) : null
                    : File.CreateText(this.FileSpec);

                foreach (byte b in this.Keys)
                {
                    if (this[b].Count == 0)
                        continue;

                    var BOOK = ObjectTable.AVXObjects.Mem.Book.Slice((int)b, 1).Span[0];
                    var CHAP = ObjectTable.AVXObjects.Mem.Chapter.Slice(BOOK.chapterIdx, BOOK.chapterCnt).Span;

                    foreach (byte c in this[b].Keys)
                    {
                        bool nc = true;

                        if (this.ScopeOnlyExport)
                        {
                            var chapter = CHAP[c - 1];
                            var writ = ObjectTable.AVXObjects.Mem.Written.Slice((int)chapter.writIdx, (int)chapter.writCnt).Span;

                            List<WordFeatures> words = new();
                            for (int w = 0; w < (int)chapter.writCnt; w++)
                            {
                                WordFeatures word = new WordFeatures(writ[w]);
                                words.Add(word);

                                byte wc = writ[w].BCVWc.WC;

                                if (wc == 1)
                                {
                                    byte v = writ[w].BCVWc.V;
                                    this[b][c][v] = words;
                                    RenderVerse(writer, b, c, v, br, nc);
                                    words.Clear();
                                    nc = false;
                                }
                            }
                        }
                        else
                        {
                            foreach (byte v in this[b][c].Keys)
                            {
                                RenderVerse(writer, b, c, v, br, nc);
                                if (this.ScopeOnlyExport)
                                {
                                    this[b][c].Clear();
                                }
                            }
                        }
                        br = true;
                    }
                    if (writer != null && this.CreationMode != FileCreateMode.Streaming)
                    {
                        writer.Close();
                    }
                    return DirectiveResultType.ExportSuccessful;
                }
            }
            catch
            {
                ;
            }
            if (writer != null)
            {
                writer.Close();
            }
            return DirectiveResultType.ExportFailed;
        }
        private void RenderVerse(TextWriter writer, byte b, byte c, byte v, bool newline, bool newchapter)
        {
            if (this.ContainsKey(b) && this[b].ContainsKey(c) && this[b][c].ContainsKey(v) && this[b][c][v].Count > 0)
            {
                List<WordFeatures> words = this[b][c][v];
                if (newchapter)
                {
                    if (newline)
                        writer.WriteLine();

                    writer.WriteLine("### " + this.Books[b].name.ToString() + " " + c.ToString());
                }
                else
                {
                    writer.Write(' ');
                }
                writer.Write("**" + v.ToString() + "**");
                writer.Write(' ');

                byte previousPunctuation = 0;
                bool space = false;

                foreach (WordFeatures word in words)
                {
                    bool italics = (byte)(word.Punctuation & Punctuation.Italics) == Punctuation.Italics;

                    string entry = this.Settings.RenderAsAV ? word.Text : word.Modern;
                    bool s = entry.EndsWith("s", StringComparison.InvariantCultureIgnoreCase);

                    if (space)
                        writer.Write(' ');
                    else
                        space = true;

                    ExportDirective.AddPrefixPunctuation(writer, previousPunctuation, word.Punctuation);

                    if (italics)
                        writer.Write('*');
                    writer.Write(entry);
                    ExportDirective.ConditionallyMakePossessive(writer, word.Punctuation, s);
                    if (italics)
                        writer.Write('*');

                    ExportDirective.AddPostfixPunctuation(writer, word.Punctuation);

                    previousPunctuation = word.Punctuation;
                }
            }
        }
    }
}