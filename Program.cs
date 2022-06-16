using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraphQLSerializerNRE;

public class Program
{
    public const int MaxDegreeOfParallelism = 10;

    static void Main()
    {
        Console.WriteLine("Starting");

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = MaxDegreeOfParallelism
        };
        Parallel.For(0, 100, options, async i =>
        {
            await Step(i);
            Thread.Sleep(100);
        });
    }

    public static JsonSerializerSettings GraphQlJsonSettings { get; } = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy(),
        },
    };

    public static async Task Step(int i)
    {
        try
        {
            var serializer = new NewtonsoftJsonSerializer(GraphQlJsonSettings);
            var client = new GraphQLHttpClient("https://invalid.zip.co", serializer);
            var request = new GraphQLRequest
            {
                Query = "Doesnt matter",
                OperationName = "paymentSessionResolve",
                Variables = new { id = "123", },
            };

            await client.SendQueryAsync<object>(request);
        }
        catch (NullReferenceException)
        {
            Console.WriteLine($"NRE occurred on step {i}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred on step {i}: {ex.Message}");
        }
    }
}
