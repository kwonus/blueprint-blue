/*
 * Copyright 2023 Kevin Wonus
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace BlueprintBlue
{
    using System.Threading;
    using System.Threading.Tasks;
    using XBlueprintBlue;
    using Grpc.Core;
    using FlatSharp;
    using System;
    using System.Collections.Generic;

    public class BlueprintBlue : IBlueprintBlueSVC
    {
        public void Run()
        {
            {
                // The echo service often returns back the same item that it received. We should configure it to enable shared strings
                // and other optimizations.
                Action<SerializerSettings> settingsConfig = s =>
                    s.UseMemoryCopySerialization()
                     .UseLazyDeserialization()
                     .UseDefaultSharedStringWriter();

                // Update the serializers used for the echo service based on what we configured.
                BlueprintSVC.Serializer<QuelleRequest>.Value = BlueprintSVC.Serializer<QuelleRequest>.Value.WithSettings(settingsConfig);
                BlueprintSVC.Serializer<XBlueprint>.Value = BlueprintSVC.Serializer<XBlueprint>.Value.WithSettings(settingsConfig);
            }

            Server server = new Server();
            server.Ports.Add("127.0.0.1", 50001, ServerCredentials.Insecure);
            server.Services.Add(BlueprintSVC.BindService(new ServerImpl()));
            server.Start();

            Thread.Sleep(1000);

            BlueprintSVC.BlueprintSVCClient client = new(new Channel("127.0.0.1", 50001, ChannelCredentials.Insecure));

            // We can use the client as a traditional gRPC client:
            GrpcOperations(client).GetAwaiter().GetResult();

            Thread.Sleep(1000);

            server.ShutdownAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// This example uses the Grpc operations on the stats service client. The Grpc methods allow specifying
        /// Grpc Headers and are tightly coupled with the Grpc interfaces.
        /// </summary>
        private static async Task GrpcOperations(BlueprintSVC.BlueprintSVCClient client)
        {
            // Unary operation
            {
                XBlueprint blueprint = await client.CreateBlueprint(new QuelleRequest { Command = "hi there" }, default);
                Console.WriteLine($"EchoUnary: {blueprint.Message}");
            }
        }

        private class ServerImpl : BlueprintSVC.BlueprintSVCServerBase
        {
            public override Task<XBlueprint> CreateBlueprint(QuelleRequest request, ServerCallContext callContext)
            {
                var todo_item = new XBlueprint() { Settings = new XSettings(), Message = "ok" };
                return Task.FromResult(todo_item);
            }
        }
    }
}