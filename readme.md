[![Build status](https://img.shields.io/appveyor/ci/alunacjones/lsl-httpclient-serviceprovider.svg)](https://ci.appveyor.com/project/alunacjones/lsl-httpclient-serviceprovider)
[![Coveralls branch](https://img.shields.io/coverallsCoverage/github/alunacjones/LSL.HttpClients.ServiceProvider)](https://coveralls.io/github/alunacjones/LSL.HttpClients.ServiceProvider)
[![NuGet](https://img.shields.io/nuget/v/LSL.HttpClients.ServiceProvider.svg)](https://www.nuget.org/packages/LSL.HttpClients.ServiceProvider/)

# LSL.HttpClients.ServiceProvider

Allows for easy registration of multiple clients for a single `HttpClient`.

## Quickstart

Registering by assembly for every type that implements `IClient`:

```csharp
using LSL.HttpClients.ServiceProvider;
...
serviceCollection.AddHttpClientForClientsFromAssembly(
    typeof(HttpClientServiceCollectionExtensionsTests).Assembly,
    t => t.IsAssignableTo(typeof(IClient)),
    t => t);
```

Registering by type from an assembly of the type `IClient`:

```csharp
serviceCollection.AddHttpClientForClientsFromAssemblyOf<IClient>(
    t => t.IsAssignableTo(typeof(IClient)),
    t => t);
```
