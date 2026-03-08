namespace ServerAppDesktop.Controls;

public partial class TrayIcon : IEquatable<TrayIcon>
{
    private nint ThisNint;

    public bool Equals(TrayIcon? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return ThisNint == other.ThisNint;
    }

    public override bool Equals(object? obj) => Equals(obj as TrayIcon);
    public override int GetHashCode() => ThisNint.GetHashCode();
}
