using System.CommandLine;
using Core.Trip;

namespace CLI.Commands;

public static class TripBuilderServiceCommands
{
    public static Command Create(TripService service)
    {
        var command = new Command("trip", "Build trip from orders");

        return command;
    }
}