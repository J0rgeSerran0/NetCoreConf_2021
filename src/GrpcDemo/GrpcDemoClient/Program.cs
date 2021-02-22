using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcDemo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace GrpcDemoClient
{
    public class Program
    {
        private static readonly string _endPoint = "https://localhost:5001";
        private static readonly GrpcChannel _grpcChannel = GrpcChannel.ForAddress(_endPoint);

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Pulse cuando el servicio esté arrancado");
            Console.ReadKey();
            Console.WriteLine();

            var storeClient = new Store.StoreClient(_grpcChannel);
            StoreSummaryReply storeSummaryReply;

            // GetByName (Unary)
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Unary");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("GetByName");
            storeSummaryReply = await storeClient.GetByNameAsync(new StoreRequest() { Name = "lemons" });
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Result: {storeSummaryReply.Name} {storeSummaryReply.Price}");
            Console.WriteLine();

            // AddImage (Client Streaming)
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Client Streaming");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("AddImage");
            Console.ForegroundColor = ConsoleColor.Yellow;
            var metadata = new Metadata();
            metadata.Add("name", "pears.png");
            var exeFilePath = Assembly.GetExecutingAssembly().Location;
            var workPath = Path.GetDirectoryName(exeFilePath);
            FileStream fileStream = File.OpenRead(workPath + @"\Images\pears.png");
            using (var call = storeClient.AddImage(metadata))
            {
                var stream = call.RequestStream;
                while (true)
                {
                    byte[] buffer = new byte[64 * 1024];
                    int numberRead = await fileStream.ReadAsync(buffer, 0, buffer.Length);
                    if (numberRead == 0)
                        break;

                    if (numberRead < buffer.Length)
                        Array.Resize(ref buffer, numberRead);

                    await stream.WriteAsync(new ImageRequest()
                    {
                        Image = ByteString.CopyFrom(buffer)
                    });
                }

                await stream.CompleteAsync();

                var response = await call.ResponseAsync;
                Console.WriteLine(response.IsOk);
            }
            Console.WriteLine();

            // GetAll (Server Streaming)
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Server Streaming");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("GetAll");
            Console.ForegroundColor = ConsoleColor.Yellow;
            using (var storeReply = storeClient.GetAll(new Empty()))
                while (await storeReply.ResponseStream.MoveNext())
                    Console.WriteLine($"Result: {storeReply.ResponseStream.Current.Name} {storeReply.ResponseStream.Current.Price} {storeReply.ResponseStream.Current.CurrencyCode}");
            Console.WriteLine();

            // AddItems (Bidirectional Streaming)
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Bidirectional Streaming");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("AddItems");
            Console.ForegroundColor = ConsoleColor.Yellow;
            var items = new List<NewItemRequest>()
            {
                new NewItemRequest { Name = "Kiwis", Price = 4, CurrencyCode = "EUR" },
                new NewItemRequest { Name = "Bananas", Price = 3, CurrencyCode = "EUR" }
            };

            using (var call = storeClient.AddItems())
            {
                var responseTask = Task.Run(async () =>
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;

                    await foreach (var message in call.ResponseStream.ReadAllAsync())
                        Console.WriteLine($"Get from Server: {message.Name} {message.Price}");
                });

                foreach (var item in items)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"Send to Server: {item.Name} {item.Price} {item.CurrencyCode}");

                    await call.RequestStream.WriteAsync(new NewItemRequest()
                    {
                        Name = item.Name,
                        Price = item.Price,
                        CurrencyCode = item.CurrencyCode
                    });
                }

                await call.RequestStream.CompleteAsync();
                await responseTask;
            }
            Console.WriteLine();

            Console.ResetColor();
            Console.WriteLine("Finalizado");
            Console.ReadKey();
        }
    }
}