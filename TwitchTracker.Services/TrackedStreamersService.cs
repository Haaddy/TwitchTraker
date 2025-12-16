namespace TwitchTracker.Services;

public class TrackedStreamersService //хранение списка стримеров, за которыми нужно следить,
{
    private readonly HashSet<string> _logins = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<string> Streamers => _logins;  //можно только читать элементы, но нельзя добавлять или удалять напрямую.

    public bool Add(string login)
    {
        if (string.IsNullOrWhiteSpace(login))
            return false;

        return _logins.Add(login);
    }

    public bool Remove(string login)
    {
        return _logins.Remove(login);
    }

    public bool Contains(string login)
    {
        return _logins.Contains(login);
    }
}