using FedexServiceReference;
using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Services.Auth;
using KontrolaPakowania.API.Services.Packing;
using KontrolaPakowania.API.Services.Shipment;
using KontrolaPakowania.API.Services.Shipment.DPD;
using KontrolaPakowania.API.Services.Shipment.DPD.DTOs;
using KontrolaPakowania.API.Services.Shipment.Fedex;
using KontrolaPakowania.API.Services.Shipment.Fedex.DTOs;
using KontrolaPakowania.API.Services.Shipment.Fedex.Strategies;
using KontrolaPakowania.API.Services.Shipment.GLS;
using KontrolaPakowania.API.Services.Shipment.Mapping;
using KontrolaPakowania.API.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =====================
// Settings
// =====================
builder.Services.Configure<XlApiSettings>(builder.Configuration.GetSection("XlApiSettings"));
builder.Services.Configure<CourierSettings>(builder.Configuration.GetSection("CourierApis"));

// =====================
// HttpClients
// =====================

// DPD REST client
builder.Services.AddHttpClient<DpdService>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<CourierSettings>>().Value.DPD;
    client.BaseAddress = new Uri(settings.BaseUrl);
    var byteArray = Encoding.ASCII.GetBytes($"{settings.Username}:{settings.Password}");
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
    client.DefaultRequestHeaders.Add("x-dpd-fid", settings.MasterFID);
});

// FedEx REST client
builder.Services.AddHttpClient("FedexRestClient", (sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<CourierSettings>>().Value.Fedex.Rest;
    client.BaseAddress = new Uri(settings.BaseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// =====================
// Mappers
// =====================
builder.Services.AddSingleton<IParcelMapper<cConsign>, GlsParcelMapper>();
builder.Services.AddSingleton<IParcelMapper<FedexShipmentRequest>, FedexRestParcelMapper>();
builder.Services.AddSingleton<IParcelMapper<listV2>, FedexSoapParcelMapper>();
builder.Services.AddSingleton<IParcelMapper<DpdCreatePackageRequest>, DpdPackageMapper>();

// =====================
// Services
// =====================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDbExecutor, DapperDbExecutor>();
builder.Services.AddScoped<IPackingService, PackingService>();
builder.Services.AddScoped<Ade2PortTypeClient>();
builder.Services.AddScoped<IGlsClientWrapper, GlsClientWrapper>();
builder.Services.AddScoped<GlsService>();
builder.Services.AddScoped<CourierFactory>();
builder.Services.AddScoped<IShipmentService, ShipmentService>();

builder.Services.AddScoped<IFedexTokenService, FedexTokenService>();
builder.Services.AddScoped<IklServiceClient>();
builder.Services.AddScoped<IFedexClientWrapper, FedexClientWrapper>();
builder.Services.AddScoped<FedexService>();
builder.Services.AddScoped<ICourierService>(sp => sp.GetRequiredService<FedexService>());
builder.Services.AddScoped<FedexSoapStrategy>();
builder.Services.AddScoped<FedexRestStrategy>(sp =>
{
    var clientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var client = clientFactory.CreateClient("FedexRestClient");
    var tokenService = sp.GetRequiredService<IFedexTokenService>();
    var mapper = sp.GetRequiredService<IParcelMapper<FedexShipmentRequest>>();
    var courierSettings = sp.GetRequiredService<IOptions<CourierSettings>>();

    return new FedexRestStrategy(client, tokenService, mapper, courierSettings);
});

// =====================
// Build app
// =====================
var app = builder.Build();

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();