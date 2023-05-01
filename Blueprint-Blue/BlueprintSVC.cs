using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;

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

                // Wait for a client to connect
                Console.Write("Waiting for client connection...");
                await pipe.WaitForConnectionAsync();
                Console.WriteLine("Client connected.");

                for (string? command = ""; command != null; /**/)
                {
                    try
                    {
                        var cadence = new byte[4];
                        UInt32 length = 0;
                        if (pipe.Read(cadence) == 4)
                        {
                            length = Convert.ToUInt32(cadence);
                            var message = new byte[length];

                            if (pipe.Read(message) == length)
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
                            // initially, this test server is just an echo server
                            //
                            // Eventually, we will:
                            // 1) read a quelle-command
                            // 2) Convert it to a blueprint using Blue-Print-Lib
                            // 3) Return it as a flatfuffer (preceded by a u32 length
                            pipe.Write(cadence);
                            pipe.Write(Encoding.UTF8.GetBytes(command));
                            // MSTests will be adapted to utilize this layer (the only reason to have a dotnet client)
                        }
                    }
                    // Catch the IOException that is raised if the pipe is broken
                    // or disconnected.
                    catch (IOException e)
                    {
                        Console.WriteLine("ERROR: {0}", e.Message);
                    }
                }
            }
        }
    }
}
