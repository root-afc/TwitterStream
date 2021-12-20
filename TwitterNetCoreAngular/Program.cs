
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.



builder.Services.AddSingleton<ServiceHub>();
builder.Services.AddHostedService<ConfigurationService>();

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<TweetsHub>("/dataHub");
});

app.Run();
