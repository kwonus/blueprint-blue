using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;
using Blueprint.Blue;
using XBlueprintBlue;
using FlatSharp;

namespace Blueprint_Blue
{
    internal class BlueprintSVC
    {
        private Dictionary<Int64, (string stmt, string expd)> CommandSequence;

        public BlueprintSVC()
        {
            var forceInitialization = QContext.AVXObjects;

            this.CommandSequence = new();
        }
        public async void Run()
        {
            using (NamedPipeServerStream pipe =
                new NamedPipeServerStream("Blueprint-Blue-Service", PipeDirection.InOut, 1, PipeTransmissionMode.Byte))
            {
                Console.WriteLine("NamedPipeServerStream object created.");

                BinaryReader br = new BinaryReader(pipe);

                // Wait for a client to connect
                Console.Write("Waiting for client connection...");
                await pipe.WaitForConnectionAsync();
                Console.WriteLine("Client connected.");

                // 1) read a quelle-command
                // 2) Convert it to a blueprint using Blue-Print-Lib
                // 3) Return it as a flatfuffer (preceded by a u32 length)
                for (string? content = ""; content != null; /**/)
                {
                    try
                    {
                        while (pipe.CanRead == false)
                        {
                            await Task.Delay(1250);
                        }
                        Int64 time = 0;
                        UInt32 length = 0;
                        try
                        {
                            time = br.ReadInt64();
                            length = br.ReadUInt32();
                        }
                        catch
                        {
                            // TO DO: logging and recovery
                            continue;
                        }
                        if (length > 0)
                        {
                            var message = new byte[length];

                            if (br.Read(message) == length)
                            {
                                content = Encoding.UTF8.GetString(message, 0, (int)length);
                            }
                            else content = null;
                        }
                        if (length == 0)
                        {
                            content = null;
                        }
                        if (!string.IsNullOrEmpty(content))
                        {
                            if (time == 0)  // this flags the message as a Quelle statement 
                            {
                                Int64 seq = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                                while (this.CommandSequence.ContainsKey(seq))
                                    seq++; // this is very unlikely, but avoids collisions (single-threaded)
                                var result = QStatement.Parse(content);

                                if (result.blueprint != null && result.blueprint.IsValid)
                                {
                                    var statement = result.blueprint.Text;
                                    var expandedText = result.blueprint.Commands != null ? result.blueprint.Commands.ExpandedText : statement;
                                    this.CommandSequence[seq] = (statement, expandedText);
                                }
                                if (result.blueprint != null)
                                {
                                    var xblueprint = result.blueprint.Blueprint;

                                    int maxBytesNeeded = XBlueprint.Serializer.GetMaxSize(xblueprint);
                                    byte[] bytes = new byte[maxBytesNeeded];
                                    int bytesWritten = XBlueprint.Serializer.Write(bytes, xblueprint);

                                    byte[] size = BitConverter.GetBytes((UInt32)maxBytesNeeded);
                                    byte[] sequence = BitConverter.GetBytes(seq);

                                    pipe.Write(sequence);
                                    pipe.Write(size);
                                    pipe.Write(bytes);
                                }
                            }
                            else // non-zero value indicates that the message contains a summarization performed by the client
                            {
                                if (this.CommandSequence.ContainsKey(time))
                                {
                                    var statement = this.CommandSequence[time];
                                    // to do: add the command with summary to the command-history
                                    // (this is a refactoring effort, because history is already captured prior to making this a named-pipe service)
                                    //

                                    this.CommandSequence.Remove(time);
                                }
                            }
                        }
                    }
                    // Catch the IOException that is raised if the pipe is broken
                    // or disconnected.
                    catch (IOException e)
                    {
                        Console.WriteLine("ERROR: {0}", e.Message);
                        break;
                    }
                }
            }
            this.Run();
        }
    }
}
