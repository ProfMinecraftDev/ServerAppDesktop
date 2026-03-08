namespace ServerAppDesktop.Models;

public sealed class ChatRestriction
{
    public string Name { get; set; }
    public string Value { get; set; }

    public ChatRestriction(string name, string value)
    {
        Name = name;
        Value = value;
    }
}
