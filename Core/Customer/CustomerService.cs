using Core.Storage;

namespace Core.Customer;

public class CustomerService(IDataStorage<Customer> dataStorage) : DataService<Customer>(dataStorage)
{
    public List<Customer> Search(string query)
    {
        var q = query.Trim();
        return Items.Where(c =>
            c.FirstName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            c.LastName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            (c.Email?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
            c.PhoneNumber.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            (c.CompanyName?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (c.Ico?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (c.Dic?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
        ).ToList();
    }
}
