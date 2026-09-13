namespace CS2.UI.Widgets
{
    public sealed class MenuControlMeta
    {
        public static readonly MenuControlMeta Empty = new();

        public string Tooltip { get; }
        public Func<int>? GetHotkey { get; }
        public Action<int>? SetHotkey { get; }

        public bool HasTooltip => !string.IsNullOrWhiteSpace(Tooltip);
        public bool HasHotkey => GetHotkey is not null && SetHotkey is not null;

        public MenuControlMeta(string tooltip = "", Func<int>? getHotkey = null, Action<int>? setHotkey = null)
        {
            Tooltip = tooltip;
            GetHotkey = getHotkey;
            SetHotkey = setHotkey;
        }
    }
}
