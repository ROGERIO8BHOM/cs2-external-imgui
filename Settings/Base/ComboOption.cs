public class ComboOption<T>
{
    public string Name { get; }
    public T Value { get; }

    public ComboOption(string name, T value)
    {
        Name = name;
        Value = value;
    }
}