using HuginAndMunin;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using static CredChecker.CredChecker;
using static LegalEntityChecker.LegalEntityChecker;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.WebHost.ConfigureKestrel(kestrel =>
    kestrel.ConfigureEndpointDefaults(listen => listen.Protocols = HttpProtocols.Http2));

var config = builder.Configuration;
var appOptions = config.GetSection(AppOptions.Name).Get<AppOptions>() ?? throw new InvalidOperationException();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();
builder.Services.AddHealthChecks().AddCheck<HealthCheck>("Health");
builder.Services.Configure<AppOptions>(config.GetSection(AppOptions.Name));
builder.Services.AddGrpc();
builder.Services.AddGrpcClient<LegalEntityCheckerClient>(o => o.Address = new Uri(appOptions.LegalEntitiesCheckerUri));
builder.Services.AddGrpcClient<CredCheckerClient>(o => o.Address = new Uri(appOptions.CredCheckerUri));

var app = builder.Build();

app.MapGrpcService<HuginAndMunin.ReportProvider>();
app.MapHealthChecks("/health");

await app.RunAsync();
