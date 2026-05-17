namespace Web.Helpers;

public class StatusFilter<TStatus, TItem> where TStatus : struct, Enum
{
    private readonly Dictionary<string, TStatus?> _map;

    public string SelectedKey { get; private set; }

    public StatusFilter(Dictionary<string, TStatus?> map)
    {
        _map = map;
        SelectedKey = map.Keys.First();
    }

    public void Select(string key) => SelectedKey = key;

    public List<TItem> GetFiltered(Func<List<TItem>> getAll, Func<TStatus, List<TItem>> getByStatus)
        => _map[SelectedKey] is { } s ? getByStatus(s) : getAll();

    public List<(string Label, int Count)> GetButtonGroupData(Func<List<TItem>> getAll, Func<TStatus, List<TItem>> getByStatus)
        => _map.Keys
            .Select(key => (key, _map[key] is { } s ? getByStatus(s).Count : getAll().Count))
            .ToList();
}
