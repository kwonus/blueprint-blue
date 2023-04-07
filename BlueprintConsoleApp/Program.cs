namespace BlueprintConsoleApp
{
    using Pinshot.Blue;
    using Pinshot.PEG;
    using Blueprint.Blue;
    using BlueprintBlue;
    using static System.Runtime.InteropServices.JavaScript.JSType;

    internal class Program
    {
        static void Print(IEnumerable<Parsed> parses, int level)
        {
            foreach(var parse in parses)
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
        static string[] TestStmt = { "%exact = 1 || Exacto", "$Exacto", "-- spoke", @"%exact = default %exact::1 %format::json ""help ... time [of need]"" + please + help time of|for&/noun/ need + greetings < Genesis [1 2 10] => c:\filename" };

        static void Main(string[] args)
        {
            foreach (string stmt in TestStmt)
            {
                var result = QStatement.Parse(stmt);
                if (result.blueprint != null)
                { 
                    QStatement blueprint = result.blueprint;

                    if (blueprint.Errors != null)
                    {
                        foreach (string error in blueprint.Errors)
                        {
                            Console.WriteLine(error);
                        }
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
            }
        }
    }
}