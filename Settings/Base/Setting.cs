public abstract class Setting
{
    public string Name { get; set; }
    public string Description { get; set; }

    public bool Visible { get; set; } = true;
    public bool Enabled { get; set; } = true;

    public string Tooltip { get; set; }

    protected Setting(string name, string description = "")
    {
        Name = name;
        Description = description;
    }
}