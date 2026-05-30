using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Retimana.Functions.Gtfs.Configuration;
using Retimana.Functions.Gtfs.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights();

        services
            .AddOptions<OdptSettings>()
            .Bind(context.Configuration.GetSection(OdptSettings.SectionName));

        services
            .AddOptions<CosmosSettings>()
            .Bind(context.Configuration.GetSection(CosmosSettings.SectionName));

        services
            .AddOptions<GtfsSettings>()
            .Bind(context.Configuration.GetSection(GtfsSettings.SectionName));

        // CosmosClient (シングルトン推奨)
        services.AddSingleton<CosmosClient>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<CosmosSettings>>().Value;
            var clientOptions = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                },
                ConnectionMode = ConnectionMode.Direct,
                AllowBulkExecution = true,
                // ローカル Cosmos Emulator は自己署名証明書のため検証スキップ
                HttpClientFactory = () => new HttpClient(new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                })
            };
            return new CosmosClient(opts.Endpoint, opts.Key, clientOptions);
        });

        services.AddHttpClient<IGtfsDownloader, GtfsDownloader>(client =>
        {
            client.Timeout = TimeSpan.FromMinutes(10);
        });

        services.AddSingleton<IGtfsExtractor, GtfsExtractor>();
        services.AddSingleton<IGtfsCsvParser, GtfsCsvParser>();
        services.AddSingleton<ICosmosBulkImporter, CosmosBulkImporter>();
        services.AddScoped<IGtfsIngestionPipeline, GtfsIngestionPipeline>();
    })
    .Build();

host.Run();
