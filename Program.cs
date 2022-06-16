using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraphQLSerializerNRE;

public class Program
{
    static void Main()
    {
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = 10
        };

        // Simulate some simultaneous requests/commands
        Parallel.For(0, 100, options, async i =>
        {
            await SimulatedRequest(i);
            Thread.Sleep(100);
        });
    }

    // Same setup as gateway
    public static JsonSerializerSettings GraphQlJsonSettings { get; } = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy(),
        },
    };

    public static async Task SimulatedRequest(int i)
    {
        try
        {
            // The issue occurs in this constructor, which modifies the above GraphQlJsonSettings
            // in a non-thread-safe way, corrupting the state
            var serializer = new NewtonsoftJsonSerializer(GraphQlJsonSettings);
            var client = new GraphQLHttpClient("https://invalid.whatever.com", serializer);
            var request = new GraphQLRequest
            {
                Query = "Doesnt matter",
                OperationName = "paymentSessionResolve",
                Variables = new { id = "123", },
            };

            // The DD traces for the NREs stopped before the GraphQL.HttpClient made the request to 
            // shopify's api, therefore the issue must have occured during the in the request processing
            // rather than the response processing.
            // When the client attepmts to use the serializer which has the corrupted settings, it will
            // sometimes throw an NRE (check the console logs)
            await client.SendQueryAsync<object>(request);
        }
        catch (NullReferenceException ex)
        {
            Console.WriteLine($"NRE occurred on step {i}: {ex}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred on step {i}: {ex.Message}");
        }
    }
}
