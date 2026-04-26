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
        command.Subcommands.Add(DeleteOrder(service));
        command.Subcommands.Add(UpdateOrder(service));
        
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
            var origin = a.GetRequiredValue(originOption);
            var destination = a.GetRequiredValue(destinationOption);
            var note = a.GetValue(noteOption);

            var order = new Order(origin, destination, note);
            
            _ = service.AddAsync(order);
        });
        return command;
    }

    private static Command ListAll(OrderService service)
    {
        var command = new Command("list", "List all orders");
        
        command.SetAction(_ =>
        {
            PrintOrder(service.GetAll());
        });
        
        command.Subcommands.Add(ListByOrigin(service));
        command.Subcommands.Add(ListByDestination(service));
        command.Subcommands.Add(ListByStatus(service));
        
        return command;
    }

    private static Command ListByOrigin(OrderService service)
    {
        var command = new Command("origin", "List orders sorted by origin");

        var originArgument = new Argument<string>("origin")
        {
            Description = "Name of origin to list all orders.",
        };
        
        command.Arguments.Add(originArgument);
        
        command.SetAction(parseResult =>
        {
            var origin = parseResult.GetRequiredValue(originArgument);
            PrintOrder(service.GetByOrigin(origin));
        });
        
        return command;
    }

    private static Command ListByDestination(OrderService service)
    {
        var command = new Command("destination", "List orders sorted by destination");
        var destinationArgument = new Argument<string>("destination")
        {
            Description = "Name of destination to list all orders.",
        };
        command.Arguments.Add(destinationArgument);
        
        command.SetAction(parseResult =>
        {
            var destination = parseResult.GetRequiredValue(destinationArgument);
            PrintOrder(service.GetByDestination(destination));
        });
        
        return command;
    }

    private static Command ListByStatus(OrderService service)
    {
        var command = new Command("status", "List orders sorted by status");
        var statusArgument = new Argument<OrderStatus>("status")
        {
            Description = "Name of status to list all orders.",
        };
        command.Arguments.Add(statusArgument);
        
        command.SetAction(parseResult =>
        {
            var status = parseResult.GetRequiredValue(statusArgument);
            PrintOrder(service.GetByStatus(status));
        });
        
        return command;
    }

    private static Command DeleteOrder(OrderService service)
    {
        var command = new Command("delete", "Delete order");
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "GUID to delete orders.",
        };
        command.Arguments.Add(guidArgument);
        
        command.SetAction(parseResult =>
        {
            var guid = parseResult.GetRequiredValue(guidArgument);
            _ = service.DeleteAsync(guid);
        });

        return command;
    }

    private static Command UpdateOrder(OrderService service)
    {
        var command = new Command("update", "Update order");

        var originOptin = new Option<string>("--origin", "-o")
        {
            Required = true,
        };
        var destinationOptin = new Option<string>("--destination", "-d")
        {
            Required = true,
        };
        var statusOptin = new Option<OrderStatus>("--status", "-s")
        {
            Required = true,
        };
        
        command.Options.Add(originOptin);
        command.Options.Add(destinationOptin);
        command.Options.Add(statusOptin);
        
        command.SetAction(parseResult =>
        {
            var origin = parseResult.GetRequiredValue(originOptin);
            var destination = parseResult.GetRequiredValue(destinationOptin);
            var status = parseResult.GetRequiredValue(statusOptin);
            
            var allOrders = service.GetAll();
            var searched = allOrders.Where(o => o.Origin == origin && o.Destination == destination && o.Status == status);
            // TODO: FINISH UPDATE COMMAND
            PrintOrder(searched.ToList());
        });
        
        return command;
    }

    private static void PrintOrder(List<Order> orders)
    {
        foreach (var order in orders)
        {
            PrintOrder(order);
        }
    }

    private static void PrintOrder(Order order)
    {
        Console.WriteLine($"{order.Id} - {order.Origin} - {order.Destination}  - {order.Note}");
    }
}