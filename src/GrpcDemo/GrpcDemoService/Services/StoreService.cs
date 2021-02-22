using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcDemo;
using GrpcDemoService.Entities;
using GrpcDemoService.Repository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace GrpcDemoService
{
    public class StoreService : Store.StoreBase
    {
        private readonly ILogger<StoreService> _logger;
        private readonly StoreRepository _repository;

        public StoreService(StoreRepository repository, ILogger<StoreService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // Unary
        public override async Task<StoreSummaryReply> GetByName(StoreRequest request, ServerCallContext context)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Unary");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{nameof(GetByName)} - {request.Name}");
            Console.ResetColor();

            var store = await _repository.GetByNameAsync(request.Name);
            return new StoreSummaryReply() { Name = store.Name, Price = $"{store.Price} {store.CurrencyCode}" };
        }

        // Client Streaming
        public override async Task<StatusReply> AddImage(IAsyncStreamReader<ImageRequest> requestStream, ServerCallContext context)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Client Streaming");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{nameof(AddImage)}");
            Console.ResetColor();

            var metadata = context.RequestHeaders;

            var name = String.Empty;
            foreach (var item in metadata)
            {
                if (item.Key.Equals("name", StringComparison.CurrentCultureIgnoreCase))
                    name = item.Value;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Receiving... {name}");

            var data = new List<byte>();
            while (await requestStream.MoveNext())
            {
                Console.WriteLine($"\t{nameof(AddImage)} - Received {requestStream.Current.Image.Length} bytes");
                data.AddRange(requestStream.Current.Image);
            }

            Console.WriteLine($"\t{nameof(AddImage)} - Received file with {data.Count} bytes");

            var exeFilePath = Assembly.GetExecutingAssembly().Location;
            var workPath = Path.GetDirectoryName(exeFilePath);

            using (var fileStream = File.OpenWrite(workPath + @"\Store\Images\" + name))
            {
                using (var binaryWriter = new BinaryWriter(fileStream))
                {
                    foreach (var bytes in data)
                        binaryWriter.Write(bytes);
                }

                Console.WriteLine($"\t{nameof(AddImage)} - File saved at {workPath + @"\Store\Images\" + name}");
            }

            Console.ResetColor();

            return new StatusReply() { IsOk = true };
        }

        // Server Streaming
        public override async Task GetAll(Empty request, IServerStreamWriter<StoreReply> responseStream, ServerCallContext context)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Server Streaming");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{nameof(GetAll)}");
            Console.ResetColor();

            foreach (var store in _repository.GetAll())
            {
                await responseStream.WriteAsync(new StoreReply()
                {
                    Name = store.Name,
                    Price = store.Price,
                    CurrencyCode = store.CurrencyCode,
                });
            }
        }

        // Bidirectional Streaming
        public override async Task AddItems(IAsyncStreamReader<NewItemRequest> requestStream, IServerStreamWriter<StoreSummaryReply> responseStream, ServerCallContext context)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Bidirectional Streaming");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{nameof(AddItems)}");
            Console.ResetColor();

            try
            {
                while (await requestStream.MoveNext())
                {
                    var name = requestStream.Current.Name;
                    var price = requestStream.Current.Price;
                    var currencyCode = requestStream.Current.CurrencyCode;

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Get from Client - {name} {price} {currencyCode}");

                    _repository.Add(new StoreEntity(name, price, currencyCode));
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }

            foreach (var item in _repository.GetAll())
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Send to Client: {item.Name} {item.Price} {item.CurrencyCode}");

                await responseStream.WriteAsync(new StoreSummaryReply()
                {
                    Name = item.Name,
                    Price = $"{item.Price} {item.CurrencyCode}"
                });
            }

            Console.ResetColor();
        }
    }
}