![Logo](resources/images/logo.png)

> Demos of the talk I did in the Virtual **[NetCoreConf 2021](https://netcoreconf.com/)** in Spain (26th - 27th of February)


# **gRPC - Introducción a desarrolladores ASP.NET Core**

## **Presentation (pdf in Spanish)**

[NetCoreConf 2021 - Presentation](presentation.pdf)

## **Samples**

### **[GrpcHelloWorld](src/GrpcHelloWorld)**
 
> Typical gRPC *Hello World* sample that is shown in the talk (using **Unary**)
```
Client => .NET Core 3.1
Server => .NET 5
```

[protobuf file](src/GrpcHelloWorld/GrpcHelloWorldService/Protos/greet.proto)

**Startup.cs**

Important parts in the sample:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddGrpc();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // MORE CODE HERE

    app.UseStaticFiles();
    app.UseRouting();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapGrpcService<GreeterService>();

        // MORE CODE HERE
    });
}
```


### **[GrpcDemo](src/GrpcDemo)**
> gRPC sample that is shown in the talk (using **Unary, Server Streaming, Client Streaming, Bidirectional Streaming**)
```
Client => .NET 5
Server => .NET 5
```

[protobuf file](src/GrpcDemo/GrpcDemoService/Protos/store.proto)

**Startup.cs**

Important parts in the sample:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddGrpc();
    services.AddSingleton<StoreRepository>();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // MORE CODE HERE

    app.UseRouting();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapGrpcService<StoreService>();

        // MORE CODE HERE
    });
}
```
