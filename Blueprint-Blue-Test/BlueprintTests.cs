namespace Blueprint_Blue_Test
{
    using Pinshot.Blue;
    using Pinshot.PEG;
    using Blueprint.Blue;
    using static System.Runtime.InteropServices.JavaScript.JSType;

    [TestClass]
    public class BlueprintTests
    {
        private PinshotLib lib_pinshot;
        private Blueprint lib_blueprint;

        public BlueprintTests()
        {
            this.lib_pinshot = new PinshotLib();
            this.lib_blueprint = new Blueprint("");
        }
        private (RootParse? pinshot, QStatement? blueprint, string fatal) Parse(string stmt)
        {
            (RootParse? pinshot, QStatement? blueprint, string fatal) root = (this.lib_pinshot.Parse(stmt).root, null, string.Empty);

            if (root.pinshot != null)
            {
                if (root.pinshot.error != null)
                {
                    root.blueprint = this.lib_blueprint.Create(root.pinshot);
                    root.blueprint.Context.AddHistory(root.blueprint.Text);
                }
            }
            else
            {
                root.fatal = "Unable to produce a parse using pin-shot-avx library";
            }
            return root;
        }
        [TestCleanup]
        public void TestTeardown()
        {
            ;
        }
        [TestMethod]
        public void VerifyDependecyLoading()
        {
            string stmt = "@Help find";

            var root = this.Parse(stmt);

            Assert.IsNotNull(root.fatal);
            Assert.IsTrue(string.IsNullOrEmpty(root.fatal));
            Assert.IsNotNull(root.blueprint);

            Assert.IsNotNull(QContext.AVXObjects);

            Assert.IsNotNull(QContext.AVXObjects.written);
            Assert.IsNotNull(QContext.AVXObjects.lexicon);
            Assert.IsNotNull(QContext.AVXObjects.lemmata);
            Assert.IsNotNull(QContext.AVXObjects.oov);

            Assert.IsTrue(QContext.AVXObjects.written.GetLexRecord(1).found);
            //Assert.IsTrue(QContext.AVXObjects.lexicon.); // TODO: TO DO: refactor lexical methods from written to lexicon object
            Assert.IsTrue(QContext.AVXObjects.lemmata.FindLemmataUsingWordKey(1).Length > 0);
            Assert.IsTrue(QContext.AVXObjects.oov.GetReverseEntry("elm") != 0);
        }
        [TestMethod]
        public void HelpWithArgument()
        {
            string stmt = "@Help find";

            var root = this.Parse(stmt);

            Assert.IsNotNull(root.fatal);
            Assert.IsTrue(string.IsNullOrEmpty(root.fatal));
            Assert.IsNotNull(root.blueprint);

            Assert.IsTrue(root.blueprint.IsValid);
            Assert.IsNotNull(root.blueprint.Singleton);
            var singleton = root.blueprint.Singleton;
            if (singleton != null) // silence the compiler of warnings
            {
                Assert.IsTrue(singleton.GetType() == typeof(QHelp));
                var help = (QHelp)singleton;
                Assert.IsTrue(help.Topic == "find");
            }
        }
        [TestMethod]
        public void CreateMacro()
        {
            string stmt = "%exact = 1 || Extacto";

            var root = this.Parse(stmt);

            Assert.IsNotNull(root.fatal);
            Assert.IsTrue(string.IsNullOrEmpty(root.fatal));
            Assert.IsNotNull(root.blueprint);

            Assert.IsTrue(root.blueprint.IsValid);
            Assert.IsNotNull(root.blueprint.Commands);

            if (root.blueprint.Commands != null) // silence the compiler of warnings
            {
                var commands = root.blueprint.Commands;
                Assert.IsNotNull(commands.Macro);
                if (commands.Macro != null)
                    Assert.IsTrue(commands.Macro.Label == "Exacto");

                var parts = commands.ExpandedParts;
                Assert.IsTrue(parts.Count == 1);
                var part = parts[0];
                Assert.IsTrue(part.Verb == "set");
                var setter = (QSet)part;
                Assert.IsTrue(setter.Key == "exact");
                Assert.IsTrue(setter.Value == "1");
            }
        }
        [TestMethod]
        public void ExecuteMacro()
        {
            string stmt = "%exact = 1 || Extacto";
            this.Parse(stmt);   // ignore result; just make sure that maco is defined as thus

            stmt = "$Exacto";
            var root = this.Parse(stmt);

            Assert.IsNotNull(root.fatal);
            Assert.IsTrue(string.IsNullOrEmpty(root.fatal));
            Assert.IsNotNull(root.blueprint);

            Assert.IsTrue(root.blueprint.IsValid);
            Assert.IsNotNull(root.blueprint.Commands);

            if (root.blueprint.Commands != null) // silence the compiler of warnings
            {
                ; // requires macro exapansion implementation prior to making assertions
            }
        }
        [TestMethod]
        public void NegateTem()
        {
            string stmt = "-- spoke";
        }
        [TestMethod]
        public void CompoundStatement()
        {
            string stmt = @"%exact = default %exact::1 %format::json ""help ... time [of need]"" + please + help time of|for&/noun/ need + greetings < Genesis [1 2 10]";
        }
        [TestMethod]
        public void CompoundStatementWithOutput()
        {
            string stmt = @"%exact = default %exact::1 %format::json ""help ... time [of need]"" + please + help time of|for&/noun/ need + greetings < Genesis [1 2 10] => c:\filename";
        }
        [TestMethod]
        public void SearchWithPOS()
        {
            string stmt = "/noun/ /verb/ /!pn_obj/";
            var root = this.Parse(stmt);

            Assert.IsNotNull(root.fatal);
            Assert.IsTrue(string.IsNullOrEmpty(root.fatal));
            Assert.IsNotNull(root.blueprint);

            Assert.IsTrue(root.blueprint.IsValid);
            Assert.IsNotNull(root.blueprint.Commands);

            if (root.blueprint.Commands != null) // silence the compiler of warnings
            {
                ; // requires macro exapansion implementation prior to making assertions
            }

        }
    }
}