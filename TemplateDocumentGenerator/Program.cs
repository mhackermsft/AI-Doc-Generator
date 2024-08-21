using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using TemplateDocumentGenerator.Data;
using TemplateDocumentGenerator.Models;
using TemplateDocumentGenerator.Services;
using TemplateDocumentGenerator.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<AppSettings>(builder.Configuration);
builder.Services.AddHttpClient("retryHttpClient").AddPolicyHandler(RetryHelper.GetRetryPolicy());
builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddSingleton<AIService>();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = int.MaxValue;
});

var app = builder.Build();

//setup EF database and migrate to latest version
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

//Before we start the app, ensure that the KNN folder exists on the filesystem
if (!Directory.Exists("KNN"))
{
    Directory.CreateDirectory("KNN");
}

//Before we start the app, ensure that the Templates folder exists on the filesystem
if (!Directory.Exists("Templates"))
{
    Directory.CreateDirectory("Templates");
}
app.Run();
