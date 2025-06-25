using HolaMyFrontend.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Đăng ký ApiSettings
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiUrl"));
builder.Services.AddHttpClient(); // Rất quan trọng để IHttpClientFactory hoạt động
builder.Services.AddHttpContextAccessor(); // Cần thiết để lấy Antiforgery token
builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken"); // Cấu hình antiforgery

// Đăng ký HttpClient với IHttpClientFactory
builder.Services.AddHttpClient("ApiClient", client =>
{
    var apiSettings = builder.Configuration.GetSection("ApiUrl").Get<ApiSettings>();
    client.BaseAddress = new Uri(apiSettings.BaseUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
