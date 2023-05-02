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
        public BlueprintSVC() { }
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
                for (string? command = ""; command != null; /**/)
                {
                    try
                    {
                        while (pipe.CanRead == false)
                        {
                            await Task.Delay(1250);
                        }
                        UInt32 length = 0;
                        length = br.ReadUInt32();
                        if (length > 0)
                        {
                            var message = new byte[length];

                            if (br.Read(message) == length)
                            {
                                command = Encoding.UTF8.GetString(message, 0, (int)length);
                            }
                            else command = null;
                        }
                        if (length == 0)
                        {
                            command = null;
                        }
                        if (!string.IsNullOrEmpty(command))
                        {
                            var result = QStatement.Parse(command);
                            var xblueprint = result.blueprint.Blueprint;

                            int maxBytesNeeded = XBlueprint.Serializer.GetMaxSize(xblueprint);
                            byte[] bytes = new byte[maxBytesNeeded];
                            int bytesWritten = XBlueprint.Serializer.Write(bytes, xblueprint);

                            byte[] size = BitConverter.GetBytes((UInt32)maxBytesNeeded);

                            pipe.Write(size);
                            pipe.Write(bytes);
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
