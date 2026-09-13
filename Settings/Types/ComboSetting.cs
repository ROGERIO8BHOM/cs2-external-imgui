public class ComboSetting<T> : Setting
{
    public ComboOption<T>[] Options { get; }

    public int SelectedIndex { get; set; }

    public ComboOption<T> SelectedOption => Options[SelectedIndex];

    

    private T _selectedValue;

    public T SelectedValue
    {
        get => _selectedValue;
        set
        {
            if (!Equals(_selectedValue, value))
            {
                _selectedValue = value;

                // Dispara o evento
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public event EventHandler? ValueChanged;

    public ComboSetting(
        string name,
        ComboOption<T>[] options,
        int selected = 0,
        string description = "")
        : base(name, description)
    {
        Options = options;
        SelectedIndex = selected;
    }


}