using System.CommandLine;
using Core.Customer;

namespace CLI.Commands;

public static class CustomerServiceCommands
{
    public static Command Create(CustomerService service)
    {
        var command = new Command("customers", "Manage customers");

        command.Subcommands.Add(AddCustomer(service));
        command.Subcommands.Add(ListAll(service));
        command.Subcommands.Add(DeleteCustomer(service));
        command.Subcommands.Add(UpdateCustomer(service));

        return command;
    }

    private static Command AddCustomer(CustomerService service)
    {
        var command     = new Command("add", "Add a new customer");
        var firstNameOption  = command.AddRequiredOption<string>("--first-name", "-f");
        var lastNameOption   = command.AddRequiredOption<string>("--last-name", "-l");
        var phoneOption      = command.AddRequiredOption<string>("--phone", "-p");
        var emailOption      = command.AddOption<string>("--email", "-e");
        var companyOption    = command.AddOption<string>("--company-name", "-c");
        var icoOption        = command.AddOption<string>("--ico", "-i");
        var dicOption        = command.AddOption<string>("--dic", "-d");

        command.SetAction(async parseResult =>
        {
            var customer = new Customer
            {
                FirstName   = parseResult.GetRequiredValue(firstNameOption),
                LastName    = parseResult.GetRequiredValue(lastNameOption),
                PhoneNumber = parseResult.GetRequiredValue(phoneOption),
                Email       = parseResult.GetValue(emailOption),
                CompanyName = parseResult.GetValue(companyOption),
                Ico         = parseResult.GetValue(icoOption),
                Dic         = parseResult.GetValue(dicOption),
            };
            await service.AddAsync(customer);
            Console.WriteLine("Customer successfully added.");
            PrintCustomer(customer);
        });
        return command;
    }

    private static Command ListAll(CustomerService service)
    {
        var command = new Command("list", "List customers");
        command.SetAction(_ => PrintCustomers(service.GetAll()));
        command.Subcommands.Add(SearchCustomers(service));
        return command;
    }

    private static Command SearchCustomers(CustomerService service)
    {
        var command = new Command("search", "Search customers");
        var queryArg = command.AddArgument<string>("query");
        command.SetAction(parseResult => PrintCustomers(service.Search(parseResult.GetRequiredValue(queryArg))));
        return command;
    }

    private static Command DeleteCustomer(CustomerService service)
    {
        var command = new Command("delete", "Delete a customer");
        var idArg = command.AddArgument<Guid>("id");

        command.SetAction(async parseResult =>
        {
            var id = parseResult.GetRequiredValue(idArg);
            if (service.GetById(id) is null)
            {
                Console.WriteLine($"Customer {id} not found.");
                return;
            }
            await service.DeleteAsync(id);
        });
        return command;
    }

    private static Command UpdateCustomer(CustomerService service)
    {
        var command          = new Command("update", "Update a customer");
        var idArg            = command.AddArgument<Guid>("id");
        var firstNameOption  = command.AddOption<string>("--first-name", "-f");
        var lastNameOption   = command.AddOption<string>("--last-name", "-l");
        var phoneOption      = command.AddOption<string>("--phone", "-p");
        var emailOption      = command.AddOption<string>("--email", "-e");
        var companyOption    = command.AddOption<string>("--company-name", "-c");
        var icoOption        = command.AddOption<string>("--ico", "-i");
        var dicOption        = command.AddOption<string>("--dic", "-d");

        command.SetAction(async parseResult =>
        {
            var id = parseResult.GetRequiredValue(idArg);
            var customer = service.GetById(id);
            if (customer is null)
            {
                Console.WriteLine($"Customer {id} not found.");
                return;
            }

            customer.FirstName   = parseResult.GetValue(firstNameOption)  ?? customer.FirstName;
            customer.LastName    = parseResult.GetValue(lastNameOption)    ?? customer.LastName;
            customer.PhoneNumber = parseResult.GetValue(phoneOption)       ?? customer.PhoneNumber;
            customer.Email       = parseResult.GetValue(emailOption)       ?? customer.Email;
            customer.CompanyName = parseResult.GetValue(companyOption)     ?? customer.CompanyName;
            customer.Ico         = parseResult.GetValue(icoOption)         ?? customer.Ico;
            customer.Dic         = parseResult.GetValue(dicOption)         ?? customer.Dic;

            await service.UpdateAsync(customer.Id, c =>
            {
                c.FirstName   = customer.FirstName;
                c.LastName    = customer.LastName;
                c.PhoneNumber = customer.PhoneNumber;
                c.Email       = customer.Email;
                c.CompanyName = customer.CompanyName;
                c.Ico         = customer.Ico;
                c.Dic         = customer.Dic;
            });
        });
        return command;
    }

    private static void PrintCustomers(List<Customer> customers) =>
        customers.ForEach(PrintCustomer);

    private static void PrintCustomer(Customer c) =>
        Console.WriteLine($"{c.Id} | {c.LastName,-15} {c.FirstName,-15} | {c.PhoneNumber,-15} | {c.Email ?? "-",-25} | {c.CompanyName ?? "-"}");
}
