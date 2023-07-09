using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace LSL.HttpClients.ServiceProvider
{
    /// <summary>
    /// HttpClientServiceProviderExtensions
    /// </summary>
    public static class HttpClientServiceProviderExtensions
    {
        /// <summary>
        /// Adds all matched clients from the given assembly
        /// </summary>
        /// <param name="source">The IServiceCollection to add the clients to</param>
        /// <param name="apiAssembly">The assembly to scan for potential clients</param>
        /// <param name="typeFilter">The delegate that filters down to the types we need to register</param>
        /// <param name="serviceTypeSelector">The delegate that selects the service type from a Type</param>
        /// <param name="implementationSelector">[Optional] The delegate that selects the implementing type for a service from a Type (defaults to the type that was found by the typeFilter)</param>
        /// <returns>An IHttpClientBuilder to allow for continued configuration</returns>
        /// <exception cref="System.ArgumentException">Thrown if no clients could be found<exception>
        public static IHttpClientBuilder AddHttpClientForClientsFromAssembly(
            this IServiceCollection source,
            Assembly apiAssembly,
            Func<Type, bool> typeFilter,
            Func<Type, Type> serviceTypeSelector,
            Func<Type, Type> implementationSelector = null)
        {
            Type DefaultImplementationSelector(Type t) => t;

            implementationSelector = implementationSelector ?? DefaultImplementationSelector;

            var genericMethod = typeof(HttpClientBuilderExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.Name == "AddTypedClient" && m.GetGenericArguments().Length == 2);

            var eligibleTypes = apiAssembly
                .GetTypes()
                .Where(typeFilter)
                .Select(t => new { ImplementingType = implementationSelector(t), ServiceType = serviceTypeSelector(t) })
                .Where(t => t.ServiceType != null && t.ImplementingType != null);

            var httpClientBuilder = eligibleTypes.Any() 
                ? source.AddHttpClient(apiAssembly.GetName().Name)
                : throw new ArgumentException("No eligible types were found in the given assembly");

            foreach (var type in eligibleTypes)
            {
                var @delegate = genericMethod
                    .MakeGenericMethod(
                        new[] { type.ServiceType, type.ImplementingType }
                    )
                    .CreateDelegate(typeof(Func<IHttpClientBuilder, IHttpClientBuilder>));

                ((Func<IHttpClientBuilder, IHttpClientBuilder>)@delegate)(httpClientBuilder);
            }

            return httpClientBuilder;           
        }

        /// <summary>
        /// Adds all matched clients from the assembly of the given generic type. Each type must require an HttpClient in its constructor.
        /// </summary>
        /// <param name="source">The IServiceCollection to add the clients to</param>
        /// <param name="typeFilter">The delegate that filters down to the types we need to register</param>
        /// <param name="serviceTypeSelector">The delegate that selects the service type from a Type</param>
        /// <param name="implementationSelector">[Optional] The delegate that selects the implementing type for a service from a Type (defaults to the type that was found by the typeFilter)</param>
        /// <typeparam name="T">The type whose assembly will be scanned for eligible types</typeparam>
        /// <returns>An IHttpClientBuilder to allow for continued configuration</returns>
        /// <exception cref="System.ArgumentException">Thrown if no clients could be found<exception>
        public static IHttpClientBuilder AddHttpClientForClientsFromAssemblyOf<T>(
            this IServiceCollection source,
            Func<Type, bool> typeFilter,
            Func<Type, Type> serviceTypeSelector,
            Func<Type, Type> implementationSelector = null)
        {
            return source.AddHttpClientForClientsFromAssembly(
                typeof(T).Assembly,
                typeFilter,
                serviceTypeSelector,
                implementationSelector);
        }
    }
}