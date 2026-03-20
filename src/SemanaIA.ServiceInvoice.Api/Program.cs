using SemanaIA.ServiceInvoice.Application;
using SemanaIA.ServiceInvoice.XmlGeneration.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<GenerateNfseXmlUseCase>();
builder.Services.AddScoped<NationalNfseXmlSerializer>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();