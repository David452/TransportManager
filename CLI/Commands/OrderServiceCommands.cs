using System.CommandLine;
using Core.Models;
using Core.Services;

namespace CLI.Commands;

public static class OrderServiceCommands
{
    public static Command Create(OrderService service)
    {
        var command = new Command("orders", "");
        
        command.Subcommands.Add(AddOrder(service));
        command.Subcommands.Add(ListAll(service));
        
        return command;
    }

    private static Command AddOrder(OrderService service)
    {
        var command = new Command("add", "Add Order");
        var originOption = new Option<string>("--origin", "-o")
        {
            Required = true,
        };
        var destinationOption = new Option<string>("--destination", "-d")
        {
            Required = true,
        };
        var noteOption = new Option<string>("--note", "-n");
        
        command.Options.Add(originOption);
        command.Options.Add(destinationOption);
        command.Options.Add(noteOption);
        
        command.SetAction(a =>
        {
            var origin = a.GetValue(originOption) ?? throw new ArgumentNullException();
            var destination = a.GetValue(destinationOption) ?? throw new ArgumentNullException();
            var note = a.GetValue(noteOption);

            var order = new Order(origin, destination, note);
            
            _ = service.AddAsync(order);
        });
        return command;
    }

    private static Command ListAll(OrderService service)
    {
        var command = new Command("list", "List all orders");
        
        command.SetAction(a =>
        {
            foreach (var order in service.GetAll())
            {
                Console.WriteLine($"{order.Id} - {order.Origin} - {order.Destination}  - {order.Note}");
            }
        });
        
        return command;
    }
}