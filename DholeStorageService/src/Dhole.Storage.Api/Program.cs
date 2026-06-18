using CustomCodeFramework.Api.DependencyInjection;
using CustomCodeFramework.Api.Swagger;
using CustomCodeFramework.Core.Abstractions;
using Dhole.Storage.Api.Endpoints;
using Dhole.Storage.Application.DependencyInjection;
using Dhole.Storage.Infrastructure.DependencyInjection;
using Dhole.Storage.Infrastructure.Time;
using Dhole.Storage.Persistence.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

builder.Services.AddCustomCodeApiWithSwagger(title: "Dhole Storage Service", version: "v1");

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseCustomCodeApi();

if (app.Environment.IsDevelopment())
{
    app.UseCustomCodeSwagger();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapStorageEndpoints();
app.MapProviderEndpoints();

app.Run();
