using Microsoft.AspNetCore.Authentication.Cookies;

using HolaMyFrontend.Models;
using HolaMyFrontend.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient("ApiClient", client =>
{
    var apiSettings = builder.Configuration.GetSection("ApiUrl").Get<ApiSettings>();
    client.BaseAddress = new Uri(apiSettings.BaseUrl);
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiUrl"));
builder.Services.AddScoped<ApiClientService>();
builder.Services.AddHttpClient();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/HomePage/Login";
        options.AccessDeniedPath = "/HomePage/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
       
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();