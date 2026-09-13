namespace CS2.Settings.Types
{
    public class BoolSetting : Setting
    {
        public bool Value { get; set; }

        public BoolSetting(string name, bool value = false, string description = "")
            : base(name, description)
        {
            Value = value;
        }
    }
}
