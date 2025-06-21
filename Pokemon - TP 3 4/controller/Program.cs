using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection; 
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Add services to the container.
//builder.Services.AddControllers();
builder.Services.AddPokemonServices();
builder.Services.AddPokemonController();

// Enregistrer les ActionFilters pour l'injection de dépendance
builder.Services.AddScoped<ExecutionTimeActionFilter>();
builder.Services.AddScoped<ActionStatisticsFilter>();

builder.Services.AddResponseCaching();

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

var app = builder.Build();

// Ajouter les middlewares de tracing AVANT les autres middlewares
app.UseExecutionTimeTracing();
app.UseRequestStatistics();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

}
    app.UseSwagger();
    app.UseSwaggerUI();
app.UseResponseCompression();
app.UseResponseCaching();


app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
