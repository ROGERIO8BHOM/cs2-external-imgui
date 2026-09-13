using CS2.UI.Widgets;
using ImGuiNET;
using System.Numerics;

namespace CS2.UI
{
    public sealed class Menu
    {
        private readonly List<MenuPage> _pages = new();
        private readonly string _title;
        private int _activePage;
        private int _activeTab;
        private Vector2 _position;
        public Vector2 _size;
        private bool _dragging;

        public Vector2 Position => _position;
        public Vector2 Size => _size;
        public Vector2 Min => _position;
        public Vector2 Max => _position + _size;

        public Menu(string title, Vector2 position, Vector2 size)
        {
            _title = title;
            _position = position;
            _size = size;
        }

        public MenuPage Page(string title, string icon = "")
        {
            MenuPage page = new(title, icon);
            _pages.Add(page);
            return page;
        }

        public void UpdateHotkeys()
        {
            foreach (MenuPage page in _pages)
                page.UpdateHotkeys();
        }

        public void CancelCapture()
        {
            MenuControls.CancelCapture();
        }

        public bool Contains(Vector2 point)
        {
            return point.X >= _position.X &&
                   point.X <= _position.X + _size.X &&
                   point.Y >= _position.Y &&
                   point.Y <= _position.Y + _size.Y;
        }

        public void Render(bool focus = false)
        {
            ImGui.SetNextWindowSize(_size);
            ImGui.SetNextWindowPos(_position);

            if (focus)
                ImGui.SetNextWindowFocus();

            ImGui.Begin($"##{_title}", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

            UIContext.Draw = ImGui.GetWindowDrawList();
            UIContext.Reset(ImGui.GetWindowPos(), _size);

            DrawShell();
            DrawPages();
            DrawTabsAndContent();
            DragWindow();

            ImGui.End();
        }

        private void DrawShell()
        {
            Vector2 pos = UIContext.WindowPosition;
            Vector2 end = pos + _size;
            Theme.AddGlow(UIContext.Draw, pos, end, Theme.Glow, Theme.Rounding, 0.35f);
            UIContext.Draw.AddRectFilled(pos, end, Theme.Color(Theme.Window), Theme.Rounding);
            UIContext.Draw.AddRect(pos, end, Theme.Color(Theme.Border), Theme.Rounding);
            UIContext.Draw.AddRectFilled(pos, pos + new Vector2(Theme.SidebarWidth, _size.Y), Theme.Color(Theme.Sidebar), Theme.Rounding, ImDrawFlags.RoundCornersLeft);
            UIContext.Draw.AddRectFilled(pos + new Vector2(Theme.SidebarWidth, 0), pos + new Vector2(_size.X, Theme.TopbarHeight), Theme.Color(Theme.Topbar), Theme.Rounding, ImDrawFlags.RoundCornersTopRight);
            UIContext.Draw.AddRectFilledMultiColor(
                pos + new Vector2(Theme.SidebarWidth, Theme.TopbarHeight - 1),
                pos + new Vector2(_size.X, Theme.TopbarHeight + 1),
                Theme.Color(Theme.WithAlpha(Theme.Accent, 0.5f)),
                Theme.Color(Theme.WithAlpha(Theme.Accent, 0.05f)),
                Theme.Color(Theme.WithAlpha(Theme.Accent, 0.05f)),
                Theme.Color(Theme.WithAlpha(Theme.Accent, 0.5f)));
            UIContext.Draw.AddText(pos + new Vector2(18, 18), Theme.Color(Theme.Text), _title);
        }

        private void DragWindow()
        {
            Vector2 pos = UIContext.WindowPosition;
            Vector2 mouse = ImGui.GetIO().MousePos;
            Vector2 mouseDelta = ImGui.GetIO().MouseDelta;
            bool insideMenu = mouse.X >= pos.X && mouse.X <= pos.X + _size.X && mouse.Y >= pos.Y && mouse.Y <= pos.Y + _size.Y;
            bool controlActive = ImGui.IsAnyItemActive();
            bool popupOpen = ImGui.IsPopupOpen(string.Empty, ImGuiPopupFlags.AnyPopupId);
            bool canStartDrag = insideMenu &&
                                !controlActive &&
                                !popupOpen &&
                                !UIContext.IsDragBlocked(mouse);

            if (canStartDrag && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                _dragging = true;

            if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
                _dragging = false;

            if (_dragging)
                _position += mouseDelta;
        }

        private void DrawPages()
        {
            Vector2 start = UIContext.WindowPosition + new Vector2(14, Theme.TopbarHeight + 18);

            for (int i = 0; i < _pages.Count; i++)
            {
                MenuPage page = _pages[i];
                Vector2 pos = start + new Vector2(0, i * 54);
                Vector2 size = new(Theme.SidebarWidth - 28, 42);
                bool selected = i == _activePage;

                UIContext.BlockDrag(pos, pos + size);
                ImGui.SetCursorScreenPos(pos);
                ImGui.InvisibleButton($"##page-{i}-{page.Title}", size);
                if (ImGui.IsItemClicked())
                {
                    _activePage = i;
                    _activeTab = 0;
                }

                float t = Animations.Float($"page:{i}", selected ? 1f : 0f, 14f);
                float hoverT = Animations.Float($"page:hover:{i}", ImGui.IsItemHovered() ? 1f : 0f, 14f);
                Vector4 color = Theme.Lerp(Theme.Panel, Theme.AccentDim, t);
                color = Theme.Lerp(color, Theme.PanelLight, hoverT * 0.4f);
                Theme.AddGlow(UIContext.Draw, pos, pos + size, Theme.Glow, Theme.SmallRounding, selected ? 0.55f : hoverT * 0.25f);
                UIContext.Draw.AddRectFilled(pos, pos + size, Theme.Color(color), Theme.SmallRounding);

                string label = string.IsNullOrWhiteSpace(page.Icon) ? page.Title[..Math.Min(2, page.Title.Length)] : page.Icon;
                Vector2 labelSize = ImGui.CalcTextSize(label);
                UIContext.Draw.AddText(pos + ((size - labelSize) * 0.5f), Theme.Color(selected ? Theme.Text : Theme.TextMuted), label);
            }
        }

        private void DrawTabsAndContent()
        {
            if (_pages.Count == 0)
                return;

            MenuPage page = _pages[Math.Clamp(_activePage, 0, _pages.Count - 1)];
            Vector2 tabStart = UIContext.WindowPosition + new Vector2(Theme.SidebarWidth + 18, 8);
            Vector2 contentPos = UIContext.WindowPosition + new Vector2(Theme.SidebarWidth + 18, Theme.TopbarHeight + 16);
            Vector2 contentSize = new(_size.X - Theme.SidebarWidth - 36, _size.Y - Theme.TopbarHeight - 30);
            float tabOffset = 0f;

            for (int i = 0; i < page.Tabs.Count; i++)
            {
                MenuTab tab = page.Tabs[i];
                Vector2 labelSize = ImGui.CalcTextSize(tab.Title);
                Vector2 size = new(Math.Max(96, labelSize.X + 32), Theme.TabHeight);
                Vector2 pos = tabStart + new Vector2(tabOffset, 0);
                bool selected = i == _activeTab;
                tabOffset += size.X + 8;

                UIContext.BlockDrag(pos, pos + size);
                ImGui.SetCursorScreenPos(pos);
                ImGui.InvisibleButton($"##tab-{page.Title}-{tab.Title}-{i}", size);
                if (ImGui.IsItemClicked())
                    _activeTab = i;

                float t = Animations.Float($"tab:{_activePage}:{i}", selected ? 1f : 0f, 14f);
                float hoverT = Animations.Float($"tab:hover:{_activePage}:{i}", ImGui.IsItemHovered() ? 1f : 0f, 14f);
                Theme.AddGlow(UIContext.Draw, pos, pos + size, Theme.Glow, Theme.SmallRounding, selected ? 0.3f : hoverT * 0.18f);
                UIContext.Draw.AddRectFilled(pos, pos + size, Theme.Color(Theme.Lerp(Theme.Topbar, Theme.PanelLight, t)), Theme.SmallRounding);
                UIContext.Draw.AddText(pos + new Vector2(16, 10), Theme.Color(selected ? Theme.Text : Theme.TextMuted), tab.Title);
                if (selected)
                    UIContext.Draw.AddRectFilled(pos + new Vector2(14, size.Y - 3), pos + new Vector2(size.X - 14, size.Y), Theme.Color(Theme.Accent), 2f);
            }

            UIContext.Draw.AddRectFilled(contentPos, contentPos + contentSize, Theme.Color(Theme.Panel), Theme.Rounding);
            UIContext.Draw.AddRect(contentPos, contentPos + contentSize, Theme.Color(Theme.Border), Theme.Rounding);

            if (page.Tabs.Count == 0)
                return;

            MenuTab activeTab = page.Tabs[Math.Clamp(_activeTab, 0, page.Tabs.Count - 1)];
            activeTab.Render(contentPos + new Vector2(Theme.Padding), contentSize - new Vector2(Theme.Padding * 2));
        }
    }

    public sealed class MenuPage
    {
        internal List<MenuTab> Tabs { get; } = new();
        internal string Icon { get; }
        internal string Title { get; }

        internal MenuPage(string title, string icon)
        {
            Title = title;
            Icon = icon;
        }

        public MenuTab Tab(string title)
        {
            MenuTab tab = new(title);
            Tabs.Add(tab);
            return tab;
        }

        internal void UpdateHotkeys()
        {
            foreach (MenuTab tab in Tabs)
                tab.UpdateHotkeys();
        }
    }

    public sealed class MenuTab
    {
        private readonly List<MenuGroup> _groups = new();
        internal string Title { get; }

        internal MenuTab(string title)
        {
            Title = title;
        }

        public MenuGroup Group(string title, int column = 0)
        {
            MenuGroup group = new(title, column);
            _groups.Add(group);
            return group;
        }

        internal void UpdateHotkeys()
        {
            foreach (MenuGroup group in _groups)
                group.UpdateHotkeys();
        }

        internal void Render(Vector2 position, Vector2 size)
        {
            int columns = Math.Max(1, _groups.Select(group => group.Column).DefaultIfEmpty(0).Max() + 1);
            float gap = 12f;
            float columnWidth = (size.X - (gap * (columns - 1))) / columns;
            float[] offsets = new float[columns];

            foreach (MenuGroup group in _groups)
            {
                int column = Math.Clamp(group.Column, 0, columns - 1);
                Vector2 groupPos = position + new Vector2(column * (columnWidth + gap), offsets[column]);
                float height = group.MeasureHeight();
                group.Render(groupPos, new Vector2(columnWidth, height));
                offsets[column] += height + gap;
            }
        }
    }

    public sealed class MenuGroup
    {
        private readonly List<Action<MenuGroupLayout>> _controls = new();
        private readonly List<Action> _hotkeyActions = new();
        internal int Column { get; }
        internal string Title { get; }

        internal MenuGroup(string title, int column)
        {
            Title = title;
            Column = column;
        }

        public MenuGroup Button(string label, Action onClick, string tooltip = "", Func<int>? getHotkey = null, Action<int>? setHotkey = null)
        {
            MenuControlMeta meta = new(tooltip, getHotkey, setHotkey);
            _controls.Add(layout => MenuControls.Button(label, onClick, layout, meta));
            _hotkeyActions.Add(() => MenuControls.UpdateHotkey(meta, onClick));
            return this;
        }

        public MenuGroup Keybind(string label, Func<int> getKey, Action<int> setKey, string tooltip = "")
        {
            MenuControlMeta meta = new(tooltip);
            _controls.Add(layout => MenuControls.Keybind(label, getKey, setKey, layout, meta));
            return this;
        }

        public MenuGroup Keybind(string label, Func<int> getKey, Action<int> setKey, Action onPressed, string tooltip = "")
        {
            MenuControlMeta meta = new(tooltip);
            _controls.Add(layout => MenuControls.Keybind(label, getKey, setKey, layout, meta));
            _hotkeyActions.Add(() => MenuControls.UpdateBoundKey(getKey, onPressed));
            return this;
        }

        public MenuGroup Toggle(string label, Func<bool> getValue, Action<bool> setValue, string tooltip = "", Func<int>? getHotkey = null, Action<int>? setHotkey = null)
        {
            MenuControlMeta meta = new(tooltip, getHotkey, setHotkey);
            _controls.Add(layout => MenuControls.Toggle(label, getValue, setValue, layout, meta));
            _hotkeyActions.Add(() => MenuControls.UpdateHotkey(meta, () => setValue(!getValue())));
            return this;
        }

        public MenuGroup Checkbox(string label, Func<bool> getValue, Action<bool> setValue, string tooltip = "", Func<int>? getHotkey = null, Action<int>? setHotkey = null)
        {
            MenuControlMeta meta = new(tooltip, getHotkey, setHotkey);
            _controls.Add(layout => MenuControls.Checkbox(label, getValue, setValue, layout, meta));
            _hotkeyActions.Add(() => MenuControls.UpdateHotkey(meta, () => setValue(!getValue())));
            return this;
        }

        public MenuGroup Slider(string label, Func<float> getValue, Action<float> setValue, float min, float max, string format = "0.000", string tooltip = "", Func<int>? getHotkey = null, Action<int>? setHotkey = null)
        {
            MenuControlMeta meta = new(tooltip, getHotkey, setHotkey);
            _controls.Add(layout => MenuControls.Slider(label, getValue, setValue, min, max, format, layout, meta));
            _hotkeyActions.Add(() => MenuControls.UpdateHotkey(meta, () => setValue(max)));
            return this;
        }

        public MenuGroup SliderInt(string label, Func<int> getValue, Action<int> setValue, int min, int max, string tooltip = "", Func<int>? getHotkey = null, Action<int>? setHotkey = null)
        {
            MenuControlMeta meta = new(tooltip, getHotkey, setHotkey);
            _controls.Add(layout => MenuControls.SliderInt(label, getValue, setValue, min, max, layout, meta));
            _hotkeyActions.Add(() => MenuControls.UpdateHotkey(meta, () => setValue(max)));
            return this;
        }

        public MenuGroup TextInput(string label, Func<string> getValue, Action<string> setValue, uint maxLength = 64, string tooltip = "")
        {
            MenuControlMeta meta = new(tooltip);
            _controls.Add(layout => MenuControls.TextInput(label, getValue, setValue, maxLength, layout, meta));
            return this;
        }

        public MenuGroup Combo(string label, Func<int> getValue, Action<int> setValue, IReadOnlyList<string> items, string tooltip = "")
        {
            MenuControlMeta meta = new(tooltip);
            _controls.Add(layout => MenuControls.Combo(label, getValue, setValue, items, layout, meta));
            return this;
        }

        public MenuGroup ComboEnum<TEnum>(string label, Func<TEnum> getValue, Action<TEnum> setValue, string tooltip = "") where TEnum : struct, Enum
        {
            MenuControlMeta meta = new(tooltip);
            _controls.Add(layout => MenuControls.ComboEnum(label, getValue, setValue, layout, meta));
            return this;
        }

        public MenuGroup ColorEdit(string label, Func<Vector4> getValue, Action<Vector4> setValue, string tooltip = "")
        {
            MenuControlMeta meta = new(tooltip);
            _controls.Add(layout => MenuControls.ColorEdit(label, getValue, setValue, layout, meta));
            return this;
        }

        internal float MeasureHeight()
        {
            return Theme.GroupHeaderHeight + Theme.Padding + (_controls.Count * Theme.RowHeight) + Theme.Padding;
        }

        internal void UpdateHotkeys()
        {
            foreach (Action hotkeyAction in _hotkeyActions)
                hotkeyAction();
        }

        internal void Render(Vector2 position, Vector2 size)
        {
            Theme.AddGlow(UIContext.Draw, position, position + size, Theme.Glow, Theme.Rounding, 0.12f);
            UIContext.Draw.AddRectFilled(position, position + size, Theme.Color(Theme.PanelLight), Theme.Rounding);
            UIContext.Draw.AddRect(position, position + size, Theme.Color(Theme.Border), Theme.Rounding);
            UIContext.Draw.AddRectFilledMultiColor(
                position,
                position + new Vector2(size.X, Theme.GroupHeaderHeight),
                Theme.Color(Theme.AccentDim),
                Theme.Color(Theme.WithAlpha(Theme.AccentDim, 0.15f)),
                Theme.Color(Theme.WithAlpha(Theme.AccentDim, 0.15f)),
                Theme.Color(Theme.AccentDim));
            UIContext.Draw.AddText(position + new Vector2(12, 9), Theme.Color(Theme.Text), Title);

            MenuGroupLayout layout = new(position + new Vector2(12, Theme.GroupHeaderHeight + 10), size.X - 24);
            foreach (Action<MenuGroupLayout> control in _controls)
                control(layout);
        }
    }

    public sealed class MenuGroupLayout
    {
        public Vector2 Cursor { get; private set; }
        public float Width { get; }

        internal MenuGroupLayout(Vector2 cursor, float width)
        {
            Cursor = cursor;
            Width = width;
        }

        public void Next()
        {
            Cursor += new Vector2(0, Theme.RowHeight);
        }
    }
}
