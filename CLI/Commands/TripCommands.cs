using System.CommandLine;
using Core.Order;
using Core.OSRM;
using Core.Trip;

namespace CLI.Commands;

public static class TripCommands
{
    public static Command Create(TripService tripService, OrderService orderService, IRouteService routeService)
    {
        var command = new Command("trips", "Manage trips");

        command.Subcommands.Add(BuildTrip(tripService, orderService));
        command.Subcommands.Add(ListTrips(tripService));
        command.Subcommands.Add(ShowTrip(tripService, orderService));
        command.Subcommands.Add(DeleteTrip(tripService, orderService));
        command.Subcommands.Add(StartTrip(tripService, orderService));
        command.Subcommands.Add(CompleteTrip(tripService, orderService));
        command.Subcommands.Add(AddOrderToTrip(tripService, orderService));
        command.Subcommands.Add(RemoveOrderFromTrip(tripService, orderService));
        command.Subcommands.Add(SwapOrders(tripService));
        command.Subcommands.Add(SuggestOrders(tripService, orderService, routeService));

        return command;
    }

    private static List<Order> ResolveOrders(Trip trip, OrderService orderService) =>
        trip.OrderIds.Select(id => orderService.GetById(id)).OfType<Order>().ToList();

    private static Command BuildTrip(TripService tripService, OrderService orderService)
    {
        var command = new Command("build", "Build a new trip from orders");
        var dateArg = command.AddArgument<DateOnly>("date");
        var ordersOption = new Option<Guid[]>("--orders", "-o") { AllowMultipleArgumentsPerToken = true };
        command.Options.Add(ordersOption);

        command.SetAction(async parseResult =>
        {
            var date = parseResult.GetRequiredValue(dateArg);
            var orderIds = parseResult.GetValue(ordersOption) ?? [];

            foreach (var id in orderIds)
                if (orderService.GetById(id) is null) { Console.WriteLine($"Order {id} not found."); return; }

            var trip = new Trip(date, [..orderIds]);
            await tripService.AddAsync(trip);
            foreach (var id in orderIds)
                await orderService.UpdateAsync(id, o => o.Status = OrderStatus.Assigned);

            Console.WriteLine($"Trip built with {orderIds.Length} order(s).");
            PrintTrip(trip);
        });
        return command;
    }

    private static Command ListTrips(TripService tripService)
    {
        var command = new Command("list", "List all trips");
        command.SetAction(_ => tripService.GetAll().ForEach(PrintTrip));
        return command;
    }

    private static Command ShowTrip(TripService tripService, OrderService orderService)
    {
        var command = new Command("show", "Show trip details with orders");
        var idArg = command.AddArgument<Guid>("id");

        command.SetAction(parseResult =>
        {
            var trip = tripService.GetById(parseResult.GetRequiredValue(idArg));
            if (trip is null) { Console.WriteLine("Trip not found."); return; }

            PrintTrip(trip);
            Console.WriteLine("Orders:");
            var orders = ResolveOrders(trip, orderService);
            foreach (var (order, i) in orders.Select((o, i) => (o, i)))
                Console.WriteLine($"  [{i}] {order.Id} | {order.Status,-10} | {order.Origin.DisplayName} -> {order.Destination.DisplayName}");
        });
        return command;
    }

    private static Command DeleteTrip(TripService tripService, OrderService orderService)
    {
        var command = new Command("delete", "Delete a trip and reset its orders");
        var idArg = command.AddArgument<Guid>("id");

        command.SetAction(async parseResult =>
        {
            var id = parseResult.GetRequiredValue(idArg);
            var trip = tripService.GetById(id);
            if (trip is null) { Console.WriteLine("Trip not found."); return; }

            foreach (var orderId in trip.OrderIds)
                await orderService.UpdateAsync(orderId, o => o.Status = OrderStatus.New);

            await tripService.DeleteAsync(id);
        });
        return command;
    }

    private static Command StartTrip(TripService tripService, OrderService orderService)
    {
        var command = new Command("start", "Start a scheduled trip");
        var idArg = command.AddArgument<Guid>("id");

        command.SetAction(async parseResult =>
        {
            var id = parseResult.GetRequiredValue(idArg);
            if (tripService.GetById(id) is null) { Console.WriteLine("Trip not found."); return; }
            try
            {
                await tripService.UpdateAsync(id, t => t.Start());
                var trip = tripService.GetById(id)!;
                foreach (var orderId in trip.OrderIds)
                    await orderService.UpdateAsync(orderId, o => o.Status = OrderStatus.EnRoute);
            }
            catch (InvalidOperationException e) { Console.WriteLine(e.Message); }
        });
        return command;
    }

    private static Command CompleteTrip(TripService tripService, OrderService orderService)
    {
        var command = new Command("complete", "Complete an ongoing trip");
        var idArg = command.AddArgument<Guid>("id");

        command.SetAction(async parseResult =>
        {
            var id = parseResult.GetRequiredValue(idArg);
            if (tripService.GetById(id) is null) { Console.WriteLine("Trip not found."); return; }
            try
            {
                await tripService.UpdateAsync(id, t => t.Complete());
                var trip = tripService.GetById(id)!;
                foreach (var orderId in trip.OrderIds)
                    await orderService.UpdateAsync(orderId, o => o.Status = OrderStatus.Delivered);
            }
            catch (InvalidOperationException e) { Console.WriteLine(e.Message); }
        });
        return command;
    }

    private static Command AddOrderToTrip(TripService tripService, OrderService orderService)
    {
        var command = new Command("add-order", "Add an order to a trip");
        var tripIdArg = command.AddArgument<Guid>("tripId");
        var orderIdArg = command.AddArgument<Guid>("orderId");

        command.SetAction(async parseResult =>
        {
            var tripId = parseResult.GetRequiredValue(tripIdArg);
            var orderId = parseResult.GetRequiredValue(orderIdArg);

            if (orderService.GetById(orderId) is null) { Console.WriteLine($"Order {orderId} not found."); return; }
            if (tripService.GetById(tripId) is null) { Console.WriteLine($"Trip {tripId} not found."); return; }

            await tripService.UpdateAsync(tripId, t => t.OrderIds.Add(orderId));
            await orderService.UpdateAsync(orderId, o => o.Status = OrderStatus.Assigned);
        });
        return command;
    }

    private static Command RemoveOrderFromTrip(TripService tripService, OrderService orderService)
    {
        var command = new Command("remove-order", "Remove an order from a trip");
        var tripIdArg = command.AddArgument<Guid>("tripId");
        var orderIdArg = command.AddArgument<Guid>("orderId");

        command.SetAction(async parseResult =>
        {
            var tripId = parseResult.GetRequiredValue(tripIdArg);
            var orderId = parseResult.GetRequiredValue(orderIdArg);

            await tripService.UpdateAsync(tripId, t => t.OrderIds.Remove(orderId));
            await orderService.UpdateAsync(orderId, o => o.Status = OrderStatus.New);
        });
        return command;
    }

    private static Command SwapOrders(TripService tripService)
    {
        var command = new Command("swap", "Swap two orders in a trip by index");
        var tripIdArg = command.AddArgument<Guid>("tripId");
        var firstArg = command.AddArgument<int>("firstIndex");
        var secondArg = command.AddArgument<int>("secondIndex");

        command.SetAction(async parseResult =>
        {
            var tripId = parseResult.GetRequiredValue(tripIdArg);
            var first = parseResult.GetRequiredValue(firstArg);
            var second = parseResult.GetRequiredValue(secondArg);

            var trip = tripService.GetById(tripId);
            if (trip is null) { Console.WriteLine("Trip not found."); return; }

            if (first < 0 || second < 0 || first >= trip.OrderIds.Count || second >= trip.OrderIds.Count)
            {
                Console.WriteLine($"Invalid index. Trip has {trip.OrderIds.Count} order(s).");
                return;
            }

            await tripService.UpdateAsync(tripId, t =>
                (t.OrderIds[first], t.OrderIds[second]) = (t.OrderIds[second], t.OrderIds[first]));
        });
        return command;
    }

    private static Command SuggestOrders(TripService tripService, OrderService orderService, IRouteService routeService)
    {
        var command = new Command("suggest", "Suggest nearby unassigned orders for a trip");
        var tripIdArg = command.AddArgument<Guid>("tripId");
        var thresholdOption = command.AddRequiredOption<double>("--threshold", "-t");

        command.SetAction(async parseResult =>
        {
            var tripId = parseResult.GetRequiredValue(tripIdArg);
            var threshold = parseResult.GetRequiredValue(thresholdOption);

            var trip = tripService.GetById(tripId);
            if (trip is null) { Console.WriteLine("Trip not found."); return; }

            var builder = new TripBuilderService(routeService);
            builder.LoadFromTrip(ResolveOrders(trip, orderService));

            var candidates = orderService.GetAll().Where(o => o.Status == OrderStatus.New).ToList();
            var suggestions = await builder.SuggestNearbyOrderAsync(candidates, threshold);

            foreach (var s in suggestions)
                Console.WriteLine($"{s.Order.Id} | {s.Order.Origin.DisplayName} -> {s.Order.Destination.DisplayName} | {s.DistanceFromRouteKm:F1} km from route");
        });
        return command;
    }

    private static void PrintTrip(Trip t) =>
        Console.WriteLine($"{t.Id} | {t.DepartureDate} | {t.Status,-10} | {t.OrderIds.Count} order(s)");
}
