using System.CommandLine;
using CLI.Commands;
using Core.Models;
using Core.Services;
using Core.Storage;

var orderStorage = new JsonDataStorage<Order>("./data/orders.json");

var orderService = new OrderService(orderStorage);
await orderService.LoadAsync();

var root = new RootCommand("Transport Manager CLI");

root.Subcommands.Add(OrderServiceCommands.Create(orderService));

await root.Parse(args).InvokeAsync();