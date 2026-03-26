using System.Reflection;
using SemanaIA.ServiceInvoice.Api.Swagger.Filters;
using SemanaIA.ServiceInvoice.Application;
using SemanaIA.ServiceInvoice.Infrastructure.DependencyInjection;
using SemanaIA.ServiceInvoice.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    o.IncludeXmlComments(xmlPath);
    o.OperationFilter<NfseExamplesOperationFilter>();
    o.OperationFilter<ProviderManagementExamplesFilter>();
    o.OperationFilter<RuleExamplesFilter>();
});

builder.Services.AddNfseInfrastructure(builder.Configuration);
builder.Services.AddScoped<GenerateNfseXmlUseCase>();

// ProviderManagementService requires MongoDB — only register when configured
var mongoConfigured = !string.IsNullOrWhiteSpace(
    builder.Configuration.GetSection("MongoDb")["ConnectionString"]);
if (mongoConfigured)
    builder.Services.AddScoped<ProviderManagementService>();

var app = builder.Build();

if (mongoConfigured)
{
    using var scope = app.Services.CreateScope();
    var database = scope.ServiceProvider.GetRequiredService<MongoDB.Driver.IMongoDatabase>();
    await new MongoProviderIndexSetup(database).ApplyAsync();
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();