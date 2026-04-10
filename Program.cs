using CurrencyConverterApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IExchangeRateService, ExchangeRateService>();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Currency Converter API v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
