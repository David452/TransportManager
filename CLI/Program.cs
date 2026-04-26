using System.CommandLine;
using CLI.Commands;
using Core.Geocoding;
using Core.Order;
using Core.Storage;

var httpClient = new HttpClient();

var orderStorage = new JsonDataStorage<Order>("./data/orders.json");

var externalGeocodingService = new NominatimGeocodingService(httpClient);

var orderService = new OrderService(orderStorage);
await orderService.LoadAsync();


var root = new RootCommand("Transport Manager CLI");

root.Subcommands.Add(OrderServiceCommands.Create(orderService, externalGeocodingService));

await root.Parse(args).InvokeAsync();