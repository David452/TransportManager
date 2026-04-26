using System.CommandLine;
using Core.Models;
using Core.Services;

namespace CLI.Commands;

public static class OrderServiceCommands
{
    public static Command Create(OrderService service)
    {
        var command = new Command("orders", "Manage orders");

        command.Subcommands.Add(AddOrder(service));
        command.Subcommands.Add(ListAll(service));
        command.Subcommands.Add(DeleteOrder(service));
        command.Subcommands.Add(UpdateOrder(service));

        return command;
    }

    private static Command AddOrder(OrderService service)
    {
        var command = new Command("add", "Add a new order");
        var originOption      = command.AddRequiredOption<string>("--origin", "-o");
        var destinationOption = command.AddRequiredOption<string>("--destination", "-d");
        var noteOption        = command.AddOption<string>("--note", "-n");

        command.SetAction(async (parseResult, ct) =>
        {
            var order = new Order(
                parseResult.GetRequiredValue(originOption),
                parseResult.GetRequiredValue(destinationOption),
                parseResult.GetValue(noteOption)
            );
            await service.AddAsync(order);
        });
        return command;
    }

    private static Command ListAll(OrderService service)
    {
        var command = new Command("list", "List orders");

        command.SetAction(_ => PrintOrders(service.GetAll()));

        command.Subcommands.Add(ListByOrigin(service));
        command.Subcommands.Add(ListByDestination(service));
        command.Subcommands.Add(ListByStatus(service));

        return command;
    }

    private static Command ListByOrigin(OrderService service)
    {
        var command = new Command("origin", "Filter by origin");
        var arg = command.AddArgument<string>("origin");
        command.SetAction(r => PrintOrders(service.GetByOrigin(r.GetRequiredValue(arg))));
        return command;
    }

    private static Command ListByDestination(OrderService service)
    {
        var command = new Command("destination", "Filter by destination");
        var arg = command.AddArgument<string>("destination");
        command.SetAction(r => PrintOrders(service.GetByDestination(r.GetRequiredValue(arg))));
        return command;
    }

    private static Command ListByStatus(OrderService service)
    {
        var command = new Command("status", "Filter by status");
        var arg = command.AddArgument<OrderStatus>("status");
        command.SetAction(r => PrintOrders(service.GetByStatus(r.GetRequiredValue(arg))));
        return command;
    }

    private static Command DeleteOrder(OrderService service)
    {
        var command = new Command("delete", "Delete an order");
        var idArg = command.AddArgument<Guid>("id");

        command.SetAction(async parseResult =>
        {
            await service.DeleteAsync(parseResult.GetRequiredValue(idArg));
        });
        return command;
    }

    private static Command UpdateOrder(OrderService service)
    {
        var command = new Command("update", "Update an order");
        var idArg= command.AddArgument<Guid>("id");
        var originOption = command.AddOption<string>("--origin", "-o");
        var destinationOption = command.AddOption<string>("--destination", "-d");
        var statusOption = command.AddOption<OrderStatus?>("--status", "-s");
        var noteOption = command.AddOption<string>("--note", "-n");

        command.SetAction(async parseResult =>
        {
            var id = parseResult.GetRequiredValue(idArg);
            var order = service.GetById(id);
            if (order is null)
            {
                Console.WriteLine($"Order {id} not found.");
                return;
            }

            order.Origin      = parseResult.GetValue(originOption)      ?? order.Origin;
            order.Destination = parseResult.GetValue(destinationOption) ?? order.Destination;
            order.Status      = parseResult.GetValue(statusOption)      ?? order.Status;
            order.Note        = parseResult.GetValue(noteOption)        ?? order.Note;

            await service.UpdateAsync(order);
        });
        return command;
    }

    private static void PrintOrders(List<Order> orders) =>
        orders.ForEach(PrintOrder);

    private static void PrintOrder(Order o) =>
        Console.WriteLine($"{o.Id} | {o.Status,-10} | {o.Origin} -> {o.Destination}{(o.Note is null ? "" : $" ({o.Note})")}");
}
