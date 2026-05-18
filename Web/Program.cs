using Core.Customer;
using Core.Geocoding;
using Core.Order;
using Core.OSRM;
using Core.Storage;
using Core.Trip;
using Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IDataStorage<Order>>(_ => new JsonDataStorage<Order>("data/orders.json"));
builder.Services.AddSingleton<OrderService>();

builder.Services.AddSingleton<IDataStorage<Trip>>(_ => new JsonDataStorage<Trip>("data/trips.json"));
builder.Services.AddSingleton<TripService>();

builder.Services.AddSingleton<IDataStorage<Customer>>(_ => new JsonDataStorage<Customer>("data/customers.json"));
builder.Services.AddSingleton<CustomerService>();

builder.Services.AddHttpClient<IRouteService, OsrmRouteService>();
builder.Services.AddHttpClient<IGeocodingService, NominatimGeocodingService>();

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
