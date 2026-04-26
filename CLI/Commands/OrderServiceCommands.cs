using System.CommandLine;
using Core.Geocoding;
using Core.Order;

namespace CLI.Commands;

public static class OrderServiceCommands
{
    public static Command Create(OrderService service, IGeocodingService geocoding)
    {
        var command = new Command("orders", "Manage orders");

        command.Subcommands.Add(AddOrder(service, geocoding));
        command.Subcommands.Add(ListAll(service, geocoding));
        command.Subcommands.Add(DeleteOrder(service));
        command.Subcommands.Add(UpdateOrder(service, geocoding));

        return command;
    }

    private static Command AddOrder(OrderService service, IGeocodingService geocoding)
    {
        var command = new Command("add", "Add a new order");
        var originOption      = command.AddRequiredOption<string>("--origin", "-o");
        var destinationOption = command.AddRequiredOption<string>("--destination", "-d");
        var noteOption        = command.AddOption<string>("--note", "-n");

        command.SetAction(async parseResult =>
        {
            var origin      = await geocoding.GeocodeAsync(parseResult.GetRequiredValue(originOption));
            var destination = await geocoding.GeocodeAsync(parseResult.GetRequiredValue(destinationOption));

            if (origin is null || destination is null)
            {
                Console.WriteLine("Could not geocode one or both locations.");
                return;
            }

            var result = new Order(origin, destination, parseResult.GetValue(noteOption));
            await service.AddAsync(result);
            Console.WriteLine($"Order successfully added.");
            PrintOrder(result);
        });
        return command;
    }

    private static Command ListAll(OrderService service, IGeocodingService geocoding)
    {
        var command = new Command("list", "List orders");

        command.SetAction(_ => PrintOrders(service.GetAll()));

        command.Subcommands.Add(ListByOrigin(service, geocoding));
        command.Subcommands.Add(ListByDestination(service, geocoding));
        command.Subcommands.Add(ListByStatus(service));

        return command;
    }

    private static Command ListByOrigin(OrderService service, IGeocodingService geocoding)
    {
        var command = new Command("origin", "Filter by origin");
        var arg = command.AddArgument<string>("origin");
        command.SetAction(async parseResult =>
        {
            var geo = await geocoding.GeocodeAsync(parseResult.GetRequiredValue(arg));
            if (geo is null) { Console.WriteLine("Could not geocode origin."); return; }
            PrintOrders(service.GetByOrigin(geo));
        });
        return command;
    }

    private static Command ListByDestination(OrderService service, IGeocodingService geocoding)
    {
        var command = new Command("destination", "Filter by destination");
        var arg = command.AddArgument<string>("destination");
        command.SetAction(async parseResult =>
        {
            var geo = await geocoding.GeocodeAsync(parseResult.GetRequiredValue(arg));
            if (geo is null) { Console.WriteLine("Could not geocode destination."); return; }
            PrintOrders(service.GetByDestination(geo));
        });
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

    private static Command UpdateOrder(OrderService service, IGeocodingService geocoding)
    {
        var command = new Command("update", "Update an order");
        var idArg             = command.AddArgument<Guid>("id");
        var originOption      = command.AddOption<string>("--origin", "-o");
        var destinationOption = command.AddOption<string>("--destination", "-d");
        var statusOption      = command.AddOption<OrderStatus?>("--status", "-s");
        var noteOption        = command.AddOption<string>("--note", "-n");

        command.SetAction(async parseResult =>
        {
            var id = parseResult.GetRequiredValue(idArg);
            var order = service.GetById(id);
            if (order is null)
            {
                Console.WriteLine($"Order {id} not found.");
                return;
            }

            var originQuery      = parseResult.GetValue(originOption);
            var destinationQuery = parseResult.GetValue(destinationOption);

            if (originQuery is not null)
            {
                var geo = await geocoding.GeocodeAsync(originQuery);
                if (geo is null) { Console.WriteLine($"Could not geocode origin '{originQuery}'."); return; }
                order.Origin = geo;
            }
            if (destinationQuery is not null)
            {
                var geo = await geocoding.GeocodeAsync(destinationQuery);
                if (geo is null) { Console.WriteLine($"Could not geocode destination '{destinationQuery}'."); return; }
                order.Destination = geo;
            }

            order.Status = parseResult.GetValue(statusOption) ?? order.Status;
            order.Note   = parseResult.GetValue(noteOption)   ?? order.Note;

            await service.UpdateAsync(order);
        });
        return command;
    }

    private static void PrintOrders(List<Order> orders) =>
        orders.ForEach(PrintOrder);

    private static void PrintOrder(Order o) =>
        Console.WriteLine($"{o.Id} | {o.Status,-10} | {o.Origin.DisplayName} -> {o.Destination.DisplayName}{(o.Note is null ? "" : $" ({o.Note})")}");
}
