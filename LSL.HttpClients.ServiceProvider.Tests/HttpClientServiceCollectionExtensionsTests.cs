using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using NUnit.Framework;
using FluentAssertions;
using FluentAssertions.Execution;
using LSL.HttpClients.ServiceProvider.Tests.Extra;

namespace LSL.HttpClients.ServiceProvider.Tests
{
    public class HttpClientServiceCollectionExtensionsTests
    {
        [Test]
        public void AddHttpClientForClientsFromAssembly_GivenATypeSelectorThatFindsNothing_ThenNoClientsShouldBeRegistered()
        {
            var serviceCollection = new ServiceCollection();
            using var assertionScope = new AssertionScope();

            new Action(() =>
                serviceCollection.AddHttpClientForClientsFromAssembly(
                    typeof(HttpClientServiceCollectionExtensionsTests).Assembly,
                    t => t.IsAssignableTo(typeof(string)),
                    t => null
            ))
            .Should()
            .Throw<ArgumentException>();

            serviceCollection
                .Should()
                .HaveCount(0, "we have registered no HttpClient and no clients");
        }

        [Test]
        public void AddHttpClientForClientsFromAssembly_GivenNoServiceTypesAreDiscovered_ThenNoClientsShouldBeRegistered()
        {
            var serviceCollection = new ServiceCollection();
            using var assertionScope = new AssertionScope();

            new Action(() =>
                serviceCollection.AddHttpClientForClientsFromAssembly(
                    typeof(HttpClientServiceCollectionExtensionsTests).Assembly,
                    t => t.IsAssignableTo(typeof(IClient)),
                    t => null
            ))
            .Should()
            .Throw<ArgumentException>();

            serviceCollection
                .Should()
                .HaveCount(0, "we have registered no HttpClient and no clients");
        }

        [Test]
        public void AddHttpClientForClientsFromAssembly_GivenNoImplementationTypesAreDiscovered_ThenNoClientsShouldBeRegistered()
        {
            var serviceCollection = new ServiceCollection();
            using var assertionScope = new AssertionScope();

            new Action(() =>
                serviceCollection.AddHttpClientForClientsFromAssembly(
                    typeof(HttpClientServiceCollectionExtensionsTests).Assembly,
                    t => t.IsAssignableTo(typeof(IClient)),
                    t => t,
                    t => null
            ))
            .Should()
            .Throw<ArgumentException>();

            serviceCollection
                .Should()
                .HaveCount(0, "we have registered no HttpClient and no clients");
        }

        [Test]
        public void AddHttpClientForClientsFromAssembly_GivenImplementationTypesAreDiscovered_ThenItShouldRegisterTheExpectedHttpClientAndServices()
        {
            var serviceCollection = new ServiceCollection();
            using var assertionScope = new AssertionScope();

            serviceCollection.AddHttpClientForClientsFromAssembly(
                typeof(HttpClientServiceCollectionExtensionsTests).Assembly,
                t => t.IsAssignableTo(typeof(IClient)),
                t => t,
                t => t);

            var matchedServices = serviceCollection
                .Where(sd => typeof(IClient).IsAssignableFrom(sd.ServiceType) || typeof(HttpClient).IsAssignableFrom(sd.ServiceType));

            matchedServices
                .Should()
                .HaveCount(3, "we have registered an HttpClient and a client by both its service type and implemtation type");
        }
        
        [Test]
        public void AddHttpClientForClientsFromAssemblyOf_GivenATypeSelectorThatFindsNothing_ThenNoClientsShouldBeRegistered()
        {
            var serviceCollection = new ServiceCollection();
            using var assertionScope = new AssertionScope();

            new Action(() =>
                serviceCollection.AddHttpClientForClientsFromAssemblyOf<IClient>(
                    t => t.IsAssignableTo(typeof(string)),
                    t => null
            ))
            .Should()
            .Throw<ArgumentException>();

            serviceCollection
                .Should()
                .HaveCount(0, "we have registered no HttpClient and no clients");
        }

        [Test]
        public void AddHttpClientForClientsFromAssemblyOf_GivenNoServiceTypesAreDiscovered_ThenNoClientsShouldBeRegistered()
        {
            var serviceCollection = new ServiceCollection();
            using var assertionScope = new AssertionScope();

            new Action(() =>
                serviceCollection.AddHttpClientForClientsFromAssemblyOf<IClient>(
                    t => t.IsAssignableTo(typeof(IClient)),
                    t => null
            ))
            .Should()
            .Throw<ArgumentException>();

            serviceCollection
                .Should()
                .HaveCount(0, "we have registered no HttpClient and no clients");
        }

        [Test]
        public void AddHttpClientForClientsFromAssemblyOf_GivenNoImplementationTypesAreDiscovered_ThenNoClientsShouldBeRegistered()
        {
            var serviceCollection = new ServiceCollection();
            using var assertionScope = new AssertionScope();

            new Action(() =>
                serviceCollection.AddHttpClientForClientsFromAssemblyOf<IClient>(
                    t => t.IsAssignableTo(typeof(IClient)),
                    t => t,
                    t => null
            ))
            .Should()
            .Throw<ArgumentException>();

            serviceCollection
                .Should()
                .HaveCount(0, "we have registered no HttpClient and no clients");
        }

        [Test]
        public void AddHttpClientForClientsFromAssemblyOf_GivenImplementationTypesAreDiscovered_ThenItShouldRegisterTheExpectedHttpClientAndServices()
        {
            var serviceCollection = new ServiceCollection();
            using var assertionScope = new AssertionScope();

            serviceCollection.AddHttpClientForClientsFromAssemblyOf<IClient>(
                t => t.IsAssignableTo(typeof(IClient)),
                t => t,
                t => t);

            var matchedServices = serviceCollection
                .Where(sd => typeof(IClient).IsAssignableFrom(sd.ServiceType) || typeof(HttpClient).IsAssignableFrom(sd.ServiceType));

            matchedServices
                .Should()
                .HaveCount(3, "we have registered an HttpClient and a client by both its service type and implemtation type");
        }

        [Test]
        public void AddHttpClientForClientsFromAssemblyOf_GivenMultipleAssembliesToScan_ThenItShouldRegisterTheExpectedHttpClientAndServices()
        {
            var serviceCollection = new ServiceCollection();
            using var assertionScope = new AssertionScope();

            var assembly1Uri1 = new Uri("http://assembly1.com");
            var assembly1Uri2 = new Uri("http://assembly2.com");

            serviceCollection.AddHttpClientForClientsFromAssemblyOf<IClient>(
                t => t.IsAssignableTo(typeof(IClient)) && !t.IsAbstract,
                t => t,
                t => t)
                .ConfigureHttpClient(c => c.BaseAddress = assembly1Uri1);

            serviceCollection.AddHttpClientForClientsFromAssemblyOf<IAnotherClient>(
                t => t.IsAssignableTo(typeof(IAnotherClient)) && !t.IsAbstract,
                t => t,
                t => t)
                .ConfigureHttpClient(c => c.BaseAddress = assembly1Uri2);

            var matchedServices = serviceCollection
                .Where(sd => typeof(IAnotherClient).IsAssignableFrom(sd.ServiceType) || typeof(IClient).IsAssignableFrom(sd.ServiceType) || typeof(HttpClient).IsAssignableFrom(sd.ServiceType));

            matchedServices
                .Should()
                .HaveCount(3, "we have registered an HttpClient and a client by both its service type and implemtation type");

            var serviceProvider = serviceCollection.BuildServiceProvider();

            serviceProvider.GetRequiredService<Client>()
                .HttpClient
                .BaseAddress
                .Should()
                .Be(assembly1Uri1);

            serviceProvider.GetRequiredService<AnotherClient>()
                .HttpClient
                .BaseAddress
                .Should()
                .Be(assembly1Uri2);
        }

        private interface IClient { HttpClient HttpClient { get; } }  

        private class Client : IClient
        {
            public Client(HttpClient httpClient)
            {
                HttpClient = httpClient;
            }

            public HttpClient HttpClient { get; }
        }         
    }
}
