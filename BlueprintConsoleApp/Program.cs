
namespace BlueprintConsoleApp
{
    using Pinshot.PEG;
    using Blueprint.Blue;
    using System;
    using FlatSharp;
    using Blueprint;

    internal class Program
    {
        static void Print(IEnumerable<Parsed> parses, int level)
        {
            foreach (var parse in parses)
            {
                int paren = parse.rule.IndexOf('(');
                string rule = paren > 0 ? parse.rule.Substring(0, paren).Trim() : parse.rule.Trim();
                for (int i = 0; i < level; i++)
                    Console.Write("  ");

                Console.WriteLine(rule + ":\t" + parse.text.Trim());

                if (parse.children.Length > 0)
                {
                    Program.Print(parse.children, level + 1);
                }
            }
        }
//      const string TestStmt = "\"\\foo\\ ... [he said] ... /pronoun/&/3p/\" + bar + x|y&z a&b&c > xfile < genesis 1:1";
//      static string[] TestStmt = { "@Help find", "format=@", "help", "please + help ... time of|for&/noun/ need + greetings" };
//      static string[] TestStmt = { "%exact = 1 || Exacto", "$Exacto", "-- spoke", @"%exact = default %exact::1 %format::json ""help ... time [of need]"" + please + help time of|for&/noun/ need + greetings < Genesis [1 2 10] => c:\filename" };

        static AVXLib.ObjectTable AVX = QContext.AVXObjects;
        static void Main(string[] args)
        {
            try
            {
                Console.Write("> ");
                for (string line = Console.ReadLine(); true; line = Console.ReadLine())
                {
                    var result = QStatement.Parse(line);
                    
                    if (result.blueprint != null)
                    {
                        var yaml = new List<string>();
                        
                        QStatement blueprint = result.blueprint;

                        if (blueprint.Errors.Count > 0)
                        {
                            foreach (string error in blueprint.Errors)
                            {
                                Console.Error.WriteLine(error);
                            }
                            blueprint.Errors.Clear();
                        }
                        else
                        {
                            foreach (string warning in blueprint.Warnings)
                            {
                                Console.Error.WriteLine(warning);
                            }
                            blueprint.Warnings.Clear();

                            if (blueprint.Singleton != null)
                            {
                                var type = blueprint.Singleton.GetType();

                                if (type == typeof(QExit))
                                    goto DONE;

                                ProcessSingletonCommand(blueprint.Singleton);
                            }
                            else if (blueprint.Commands != null)
                            {
                                if (blueprint.Commands.Macro != null)
                                {
                                    ProcessMacro(blueprint.Commands);
                                }
                                else
                                {
                                    if (blueprint.Commands.Assignments.Any())
                                    {
                                        foreach (var detail in blueprint.Commands.Assignments)
                                        {
                                            // process all assignments in QContext object
                                        }
                                    }
                                    yaml = result.blueprint.Context.AsYaml();

                                    var call_cfunc = false;

                                    if (blueprint.Commands.Filters.Any())
                                    {
                                        call_cfunc = true;
                                        yaml.Add("scope:");
                                        foreach (var detail in blueprint.Commands.Filters)
                                        {
                                            foreach (var entry in detail.AsYaml())
                                                yaml.Add("  " + entry);
                                        }
                                        ProcessSearch(blueprint.Commands);
                                    }
                                    if (blueprint.Commands.Searches.Any())
                                    {
                                        call_cfunc = true;
                                        yaml.Add("search:");
                                        foreach (var detail in blueprint.Commands.Searches)
                                        {
                                            foreach (var entry in detail.AsYaml())
                                                yaml.Add("  " + entry);
                                        }
                                        ProcessSearch(blueprint.Commands);
                                    }
                                    else
                                    {
                                        ProcessOther(blueprint.Commands);
                                    }
                                    if (!blueprint.Commands.Context.Statement.IsValid)
                                    {
                                        ;
                                    }
                                    else if (call_cfunc)
                                    {
                                        XRequest request = blueprint.Commands.AsSearchRequest();
                                        int maxBytesNeeded = XRequest.Serializer.GetMaxSize(request);
                                        byte[] bytes = new byte[maxBytesNeeded];
                                        int bytesWritten = XRequest.Serializer.Write(bytes, request);
                                    }
                                }
                            }
                            else
                            {
                                Console.Error.WriteLine("Possible logic error");
                            }
                        }
                        foreach (var entry in yaml)
                        {
                            Console.WriteLine(entry);
                        }
                    }
                    else if (result.pinshot != null)
                    {
                        RootParse pinshot = result.pinshot;
                        if (!string.IsNullOrEmpty(pinshot.error))
                        {
                            Console.WriteLine(pinshot.error);
                        }
                        else
                        {
                            Program.Print(pinshot.result, 0);
                        }
                    }
                    Console.Write("> ");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("EXCEPTION:\t" + e.Message);
            }
        DONE:
            return;

        }
        static void ProcessSingletonCommand(QExplicitCommand singleton)
        {
            ;
        }
        static void ProcessSearch(QImplicitCommands vector)
        {
            foreach (var record in Find(vector))
            {
                var book = AVX.Mem.Book.Slice((int)record.b, 1).Span[0];
                Console.WriteLine(book.name + " " + record.c.ToString() + ":" + record.v.ToString());
            }
        }
        static void ProcessMacro(QImplicitCommands vector)
        {
            ;
        }
        static void ProcessOther(QImplicitCommands vector)
        {
            ;
        }
        static IEnumerable<(byte b, byte c, byte v, byte w, UInt16 len)> Find(QImplicitCommands vector)
        {
            var methods = AVX.written;
            var written = AVX.Mem.Written.ToArray();

            for (int w = 0; w < written.Length; w++)
            {
                var writ = written[w];
                // First just landle a single clause/segment
                //
                foreach (var clause in vector.Searches)
                {
                    foreach (var segment in clause.Segments)
                    {
                        int frags = 0;
                        
                        foreach (var fragment in segment.Fragments) // and'ed
                        {
                            bool hit = false;
                            foreach (var feature in fragment.Features) // or'ed
                            {
                                var ftype = feature.GetType();
                                if (ftype == typeof(QWord))
                                {
                                    hit = (writ.WordKey & 0x3FFF) == (uint)((QWord)feature).WordKeys[0];    // todo: wordkeys is now multi-valued
                                }
                                else if (ftype == typeof(QLemma))
                                {
                                    foreach (var lemma in ((QLemma)feature).Lemmata)
                                    {
                                        hit = writ.Lemma == lemma;
                                        if (hit)
                                            break;
                                    }
                                }
                                else if (ftype == typeof(QPartOfSpeech))
                                {
                                    var fpos = ((QPartOfSpeech)feature);

                                    if (fpos.Pos32 != 0)
                                        hit = writ.POS32 == fpos.Pos32;
                                    else if (fpos.PnPos12 != 0)
                                        hit = (writ.pnPOS12 & fpos.PnPos12) == fpos.PnPos12;
                                    if (fpos.Negate)
                                        hit = !hit;
                                    if (hit)
                                        break;
                                }
                            }
                            if (!hit)
                                break;
                            frags++;
                        }
                        if (frags == segment.Fragments.Count)
                            yield return (writ.BCVWc.B, writ.BCVWc.C, writ.BCVWc.V, writ.BCVWc.WC, 1);
                    }
                }
            }
        }
    }
}