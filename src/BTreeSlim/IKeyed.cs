namespace BTreeSlim;

public interface IKeyed<out T>
{
    T Key { get; }
}