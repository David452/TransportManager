using Core.Order;
using Core.Storage;
using Core.Trip;
using Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IDataStorage<Order>>(_ => new JsonDataStorage<Order>("data/orders.json"));
builder.Services.AddSingleton<OrderService>();

builder.Services.AddSingleton<IDataStorage<Trip>>(_ => new JsonDataStorage<Trip>("data/trips.json"));
builder.Services.AddSingleton<TripService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
