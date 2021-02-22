using Grpc.Net.Client;
using GrpcHelloWorld;
using System;

namespace GrpcHelloWorldClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Pulsar cuanto el servicio esté listo");
            Console.ReadKey();

            var endPoint = "https://localhost:5001";
            var grpcChannel = GrpcChannel.ForAddress(endPoint);

            var grpcClient = new Greeter.GreeterClient(grpcChannel);
            var helloRequest = new HelloRequest() { Name = "Jorge", Age = 23 };
            var response = grpcClient.SayHello(helloRequest);

            Console.WriteLine(response.Message);
        }
    }
}
