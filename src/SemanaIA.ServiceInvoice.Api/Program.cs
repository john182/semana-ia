using System.Reflection;
using SemanaIA.ServiceInvoice.Api.Swagger.Filters;
using SemanaIA.ServiceInvoice.Application;
using SemanaIA.ServiceInvoice.XmlGeneration.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    o.IncludeXmlComments(xmlPath);
    o.OperationFilter<NfseExamplesOperationFilter>();
});

builder.Services.AddScoped<GenerateNfseXmlUseCase>();
builder.Services.AddScoped<NationalNfseXmlSerializer>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();