using System.CommandLine;
using CLI.Commands;
using Core.Customer;
using Core.Geocoding;
using Core.Order;
using Core.OSRM;
using Core.Storage;
using Core.Trip;

var httpClient = new HttpClient();

var orderStorage    = new JsonDataStorage<Order>("./data/orders.json");
var tripStorage     = new JsonDataStorage<Trip>("./data/trips.json");
var customerStorage = new JsonDataStorage<Customer>("./data/customers.json");

var geocodingService = new NominatimGeocodingService(httpClient);
var routeService = new OsrmRouteService(httpClient);

var orderService    = new OrderService(orderStorage);
var tripService     = new TripService(tripStorage);
var customerService = new CustomerService(customerStorage);

await orderService.LoadAsync();
await tripService.LoadAsync();
await customerService.LoadAsync();

var root = new RootCommand("Transport Manager CLI");

root.Subcommands.Add(OrderServiceCommands.Create(orderService, geocodingService));
root.Subcommands.Add(TripCommands.Create(tripService, orderService, routeService));
root.Subcommands.Add(CustomerServiceCommands.Create(customerService));

await root.Parse(args).InvokeAsync();