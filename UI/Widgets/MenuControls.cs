using ImGuiNET;
using System.Numerics;

namespace CS2.UI.Widgets
{
    public static class MenuControls
    {
        private static string? _activeKeybindId;
        private static string? _activeActionHotkeyId;
        private static double _captureStartedAt;
        private const int CancelCaptureKey = -2;

        public static void CancelCapture()
        {
            _activeKeybindId = null;
            _activeActionHotkeyId = null;
        }

        public static bool Button(string label, Action onClick, MenuGroupLayout layout, MenuControlMeta? meta = null)
        {
            meta ??= MenuControlMeta.Empty;
            Vector2 pos = layout.Cursor;
            ReserveControlRow(pos, layout.Width);
            Vector2 buttonPos = pos + new Vector2(layout.Width * 0.45f, 2);
            Vector2 buttonSize = new(layout.Width * 0.55f, 27);
            string id = $"button:{label}:{pos.X}:{pos.Y}";
            bool assigning = HandleMeta(id, pos, layout.Width, meta);

            ImGui.SetCursorScreenPos(buttonPos);
            ImGui.InvisibleButton(id, buttonSize);

            bool hovered = ImGui.IsItemHovered();
            bool active = ImGui.IsItemActive();
            bool clicked = ImGui.IsItemClicked();

            if (clicked)
                onClick();

            float hoverT = Animations.Float($"{id}:hover", hovered ? 1f : 0f, 14f);
            float pressT = Animations.Float($"{id}:press", active ? 1f : 0f, 22f);
            Vector4 color = Theme.Lerp(Theme.AccentDim, Theme.AccentHot, hoverT);
            color = Theme.Lerp(color, Theme.Accent, pressT);
            Vector2 pressOffset = new(0, pressT);

            DrawLabel(label, id, pos, layout.Width, meta);
            Theme.AddGlow(UIContext.Draw, buttonPos + pressOffset, buttonPos + buttonSize + pressOffset, Theme.Glow, Theme.SmallRounding, hoverT * 0.75f);
            UIContext.Draw.AddRectFilled(buttonPos + pressOffset, buttonPos + buttonSize + pressOffset, Theme.Color(color), Theme.SmallRounding);
            UIContext.Draw.AddRect(buttonPos + pressOffset, buttonPos + buttonSize + pressOffset, Theme.Color(Theme.Lerp(Theme.Border, Theme.AccentHot, hoverT)), Theme.SmallRounding);

            string buttonText = assigning ? "Press key..." : label;
            Vector2 textSize = ImGui.CalcTextSize(buttonText);
            UIContext.Draw.AddText(buttonPos + pressOffset + ((buttonSize - textSize) * 0.5f), Theme.Color(Theme.Text), buttonText);
            DrawHotkeyHint(buttonPos, buttonSize, meta);

            layout.Next();
            return clicked;
        }

        public static bool Toggle(string label, Func<bool> getValue, Action<bool> setValue, MenuGroupLayout layout, MenuControlMeta? meta = null)
        {
            meta ??= MenuControlMeta.Empty;
            bool value = getValue();
            Vector2 pos = layout.Cursor;
            ReserveControlRow(pos, layout.Width);
            Vector2 switchSize = new(38, 20);
            Vector2 switchPos = pos + new Vector2(layout.Width - switchSize.X, 5);
            string id = $"toggle:{label}:{pos.X}:{pos.Y}";
            bool assigning = HandleMeta(id, pos, layout.Width, meta);

            ImGui.SetCursorScreenPos(switchPos);
            ImGui.InvisibleButton(id, switchSize);
            bool hovered = ImGui.IsItemHovered();
            bool clicked = ImGui.IsItemClicked();

            if (clicked)
            {
                value = !value;
                setValue(value);
            }

            float t = Animations.Float(id, value ? 1f : 0f, 18f);
            float hoverT = Animations.Float($"{id}:hover", hovered ? 1f : 0f, 14f);
            Vector4 trackColor = Animations.Color($"{id}:track", value ? Theme.Accent : Theme.Input, 18f);

            DrawLabel(label, id, pos, layout.Width, meta);
            if (assigning)
                DrawAssigningText(switchPos - new Vector2(88, -3));
            else
                DrawHotkeyHint(switchPos - new Vector2(74, 3), new Vector2(66, 14), meta);

            Theme.AddGlow(UIContext.Draw, switchPos, switchPos + switchSize, Theme.Glow, switchSize.Y * 0.5f, (value ? 0.55f : 0.2f) + (hoverT * 0.25f));
            UIContext.Draw.AddRectFilled(switchPos, switchPos + switchSize, Theme.Color(trackColor), switchSize.Y * 0.5f);
            UIContext.Draw.AddRect(switchPos, switchPos + switchSize, Theme.Color(Theme.Lerp(Theme.Border, Theme.AccentHot, Math.Max(t, hoverT))), switchSize.Y * 0.5f);
            UIContext.Draw.AddCircleFilled(switchPos + new Vector2(10 + (18 * t), 10), 7f, Theme.Color(Theme.Text));

            layout.Next();
            return clicked;
        }

        public static bool Checkbox(string label, Func<bool> getValue, Action<bool> setValue, MenuGroupLayout layout, MenuControlMeta? meta = null)
        {
            meta ??= MenuControlMeta.Empty;
            bool value = getValue();
            Vector2 pos = layout.Cursor;
            ReserveControlRow(pos, layout.Width);
            string id = $"checkbox-row:{label}:{pos.X}:{pos.Y}";
            bool assigning = HandleMeta(id, pos, layout.Width, meta);

            UIContext.Draw.AddText(pos + new Vector2(28, 7), Theme.Color(Theme.Text), label);
            DrawLabelHelp(id, pos + new Vector2(28 + ImGui.CalcTextSize(label).X + 7, 7), meta);
            if (assigning)
                DrawAssigningText(pos + new Vector2(layout.Width - 78, 7));
            else
                DrawHotkeyHint(pos + new Vector2(layout.Width - 78, 7), new Vector2(70, 14), meta);
            ImGui.SetCursorScreenPos(pos + new Vector2(0, 7));

            bool changed = Widgets.Checkbox.Draw($"##{label}:{pos.X}:{pos.Y}", ref value);
            if (changed)
                setValue(value);

            layout.Next();
            return changed;
        }

        public static bool Slider(string label, Func<float> getValue, Action<float> setValue, float min, float max, string format, MenuGroupLayout layout, MenuControlMeta? meta = null)
        {
            meta ??= MenuControlMeta.Empty;
            Vector2 pos = layout.Cursor;
            ReserveControlRow(pos, layout.Width);
            Vector2 sliderPos = pos + new Vector2(layout.Width * 0.45f, 13);
            Vector2 sliderSize = new(layout.Width * 0.38f, 6);
            string id = $"slider:{label}:{pos.X}:{pos.Y}";
            bool assigning = HandleMeta(id, pos, layout.Width, meta);
            float value = Math.Clamp(getValue(), min, max);
            float percent = max <= min ? 0f : (value - min) / (max - min);

            ImGui.SetCursorScreenPos(sliderPos - new Vector2(0, 7));
            ImGui.InvisibleButton(id, new Vector2(sliderSize.X, 22));
            bool hovered = ImGui.IsItemHovered();
            bool changed = false;

            if (ImGui.IsItemActive())
            {
                float mouseX = ImGui.GetIO().MousePos.X;
                percent = Math.Clamp((mouseX - sliderPos.X) / sliderSize.X, 0f, 1f);
                value = min + ((max - min) * percent);
                setValue(value);
                changed = true;
            }

            string valueText = value.ToString(format);
            Vector2 valueSize = ImGui.CalcTextSize(valueText);
            float hoverT = Animations.Float($"{id}:hover", hovered ? 1f : 0f, 14f);
            float visualPercent = Animations.Float($"{id}:value", percent, 16f);
            DrawLabel(label, id, pos, layout.Width, meta);
            UIContext.Draw.AddRectFilled(sliderPos, sliderPos + sliderSize, Theme.Color(Theme.Input), 3f);
            Theme.AddGlow(UIContext.Draw, sliderPos, sliderPos + new Vector2(sliderSize.X * visualPercent, sliderSize.Y), Theme.Glow, 3f, 0.4f + (hoverT * 0.35f));
            UIContext.Draw.AddRectFilled(sliderPos, sliderPos + new Vector2(sliderSize.X * visualPercent, sliderSize.Y), Theme.Color(Theme.Lerp(Theme.Accent, Theme.AccentHot, hoverT)), 3f);
            UIContext.Draw.AddCircleFilled(sliderPos + new Vector2(sliderSize.X * visualPercent, sliderSize.Y * 0.5f), 5f + (hoverT * 1.5f), Theme.Color(Theme.Text));
            if (assigning)
                DrawAssigningText(pos + new Vector2(layout.Width - 78, 7));
            else
                UIContext.Draw.AddText(pos + new Vector2(layout.Width - valueSize.X, 7), Theme.Color(Theme.TextMuted), valueText);

            layout.Next();
            return changed;
        }

        public static bool Keybind(string label, Func<int> getKey, Action<int> setKey, MenuGroupLayout layout, MenuControlMeta? meta = null)
        {
            meta ??= MenuControlMeta.Empty;
            Vector2 pos = layout.Cursor;
            ReserveControlRow(pos, layout.Width);
            Vector2 buttonPos = pos + new Vector2(layout.Width * 0.45f, 2);
            Vector2 buttonSize = new(layout.Width * 0.55f, 27);
            string id = $"keybind:{label}:{pos.X}:{pos.Y}";
            bool listening = _activeKeybindId == id;
            bool changed = false;

            ImGui.SetCursorScreenPos(buttonPos);
            ImGui.InvisibleButton(id, buttonSize);
            bool hovered = ImGui.IsItemHovered();

            if (ImGui.IsItemClicked())
            {
                _activeKeybindId = id;
                _captureStartedAt = ImGui.GetTime();
                listening = true;
            }

            if (listening && ImGui.GetTime() - _captureStartedAt > 0.12)
            {
                int captured = CaptureKey();
                if (captured == CancelCaptureKey)
                {
                    _activeKeybindId = null;
                    listening = false;
                }
                else if (captured != int.MinValue)
                {
                    setKey(captured);
                    _activeKeybindId = null;
                    changed = true;
                    listening = false;
                }
            }

            float hoverT = Animations.Float($"{id}:hover", hovered || listening ? 1f : 0f, 14f);
            float pulse = listening ? Animations.Pulse(6f, 0.45f, 1f) : 0f;
            Vector4 frame = listening ? Theme.Lerp(Theme.Accent, Theme.AccentHot, pulse) : Theme.Lerp(Theme.Border, Theme.Accent, hoverT);
            string text = listening ? "Press key..." : KeyName(getKey());
            Vector2 textSize = ImGui.CalcTextSize(text);

            DrawLabel(label, id, pos, layout.Width, meta);
            Theme.AddGlow(UIContext.Draw, buttonPos, buttonPos + buttonSize, Theme.Glow, Theme.SmallRounding, listening ? pulse : hoverT * 0.55f);
            UIContext.Draw.AddRectFilled(buttonPos, buttonPos + buttonSize, Theme.Color(Theme.Lerp(Theme.Input, Theme.Panel, hoverT)), Theme.SmallRounding);
            UIContext.Draw.AddRect(buttonPos, buttonPos + buttonSize, Theme.Color(frame), Theme.SmallRounding);
            UIContext.Draw.AddText(buttonPos + ((buttonSize - textSize) * 0.5f), Theme.Color(listening ? Theme.Text : Theme.TextMuted), text);

            layout.Next();
            return changed;
        }

        public static bool SliderInt(string label, Func<int> getValue, Action<int> setValue, int min, int max, MenuGroupLayout layout, MenuControlMeta? meta = null)
        {
            return Slider(label, () => getValue(), value => setValue((int)MathF.Round(value)), min, max, "0", layout, meta);
        }

        public static bool TextInput(string label, Func<string> getValue, Action<string> setValue, uint maxLength, MenuGroupLayout layout, MenuControlMeta? meta = null)
        {
            meta ??= MenuControlMeta.Empty;
            Vector2 pos = layout.Cursor;
            ReserveControlRow(pos, layout.Width);
            Vector2 inputPos = pos + new Vector2(layout.Width * 0.45f, 3);
            string value = getValue();
            string id = $"##input:{label}:{pos.X}:{pos.Y}";

            DrawLabel(label, id, pos, layout.Width, meta);

            ImGui.SetCursorScreenPos(inputPos);
            ImGui.SetNextItemWidth(layout.Width * 0.55f);
            ImGui.PushStyleColor(ImGuiCol.FrameBg, Theme.Input);
            ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, Theme.Panel);
            ImGui.PushStyleColor(ImGuiCol.FrameBgActive, Theme.Panel);
            ImGui.PushStyleColor(ImGuiCol.Text, Theme.Text);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, Theme.SmallRounding);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(8, 5));

            bool changed = ImGui.InputText(id, ref value, maxLength);

            ImGui.PopStyleVar(2);
            ImGui.PopStyleColor(4);

            if (changed)
                setValue(value);

            layout.Next();
            return changed;
        }

        public static bool Combo(string label, Func<int> getValue, Action<int> setValue, IReadOnlyList<string> items, MenuGroupLayout layout, MenuControlMeta? meta = null)
        {
            meta ??= MenuControlMeta.Empty;
            Vector2 pos = layout.Cursor;
            ReserveControlRow(pos, layout.Width);
            Vector2 comboPos = pos + new Vector2(layout.Width * 0.45f, 3);
            int selectedIndex = Math.Clamp(getValue(), 0, Math.Max(0, items.Count - 1));
            string preview = items.Count == 0 ? string.Empty : items[selectedIndex];
            bool changed = false;

            DrawLabel(label, $"combo:{label}:{pos.X}:{pos.Y}", pos, layout.Width, meta);

            ImGui.SetCursorScreenPos(comboPos);
            ImGui.SetNextItemWidth(layout.Width * 0.55f);
            ImGui.PushStyleColor(ImGuiCol.FrameBg, Theme.Input);
            ImGui.PushStyleColor(ImGuiCol.PopupBg, Theme.PanelLight);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, Theme.SmallRounding);

            if (ImGui.BeginCombo($"##combo:{label}:{pos.X}:{pos.Y}", preview))
            {
                for (int i = 0; i < items.Count; i++)
                {
                    bool selected = i == selectedIndex;
                    if (ImGui.Selectable(items[i], selected))
                    {
                        selectedIndex = i;
                        setValue(i);
                        changed = true;
                    }

                    if (selected)
                        ImGui.SetItemDefaultFocus();
                }

                ImGui.EndCombo();
            }

            ImGui.PopStyleVar();
            ImGui.PopStyleColor(2);

            layout.Next();
            return changed;
        }

        public static bool ComboEnum<TEnum>(
            string label,
            Func<TEnum> getValue,
            Action<TEnum> setValue,
            MenuGroupLayout layout,
            MenuControlMeta? meta = null
        ) where TEnum : struct, Enum
        {
            TEnum[] values = Enum.GetValues<TEnum>();
            string[] items = values.Select(value => value.ToString()).ToArray();

            int selectedIndex = Array.IndexOf(values, getValue());
            if (selectedIndex < 0)
                selectedIndex = 0;

            bool changed = Combo(
                label,
                () => selectedIndex,
                index =>
                {
                    if (index >= 0 && index < values.Length)
                        setValue(values[index]);
                },
                items,
                layout,
                meta
            );

            return changed;
        }

        public static bool ColorEdit(string label, Func<Vector4> getValue, Action<Vector4> setValue, MenuGroupLayout layout, MenuControlMeta? meta = null)
        {
            meta ??= MenuControlMeta.Empty;
            Vector2 pos = layout.Cursor;
            ReserveControlRow(pos, layout.Width);
            Vector2 colorPos = pos + new Vector2(layout.Width * 0.45f, 4);
            Vector4 value = getValue();
            string id = $"##color:{label}:{pos.X}:{pos.Y}";

            DrawLabel(label, id, pos, layout.Width, meta);

            ImGui.SetCursorScreenPos(colorPos);
            ImGui.SetNextItemWidth(layout.Width * 0.55f);
            bool changed = ImGui.ColorEdit4(id, ref value, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar);

            if (changed)
                setValue(value);

            layout.Next();
            return changed;
        }

        private static void ReserveControlRow(Vector2 pos, float width)
        {
            UIContext.BlockDrag(pos, pos + new Vector2(width, Theme.RowHeight));
        }

        public static bool UpdateHotkey(MenuControlMeta meta, Action hotkeyAction)
        {
            if (_activeKeybindId is not null || _activeActionHotkeyId is not null)
                return false;

            if (!meta.HasHotkey || meta.GetHotkey is null)
                return false;

            int hotkey = meta.GetHotkey();
            if (hotkey <= 0 || !IsPressed(hotkey))
                return false;

            hotkeyAction();
            return true;
        }

        public static bool UpdateBoundKey(Func<int> getKey, Action keyAction)
        {
            if (_activeKeybindId is not null || _activeActionHotkeyId is not null)
                return false;

            int key = getKey();
            if (key <= 0 || !IsPressed(key))
                return false;

            keyAction();
            return true;
        }

        private static bool HandleMeta(string id, Vector2 pos, float width, MenuControlMeta meta)
        {
            bool assigning = _activeActionHotkeyId == id;

            if (meta.HasHotkey && ImGui.IsMouseHoveringRect(pos, pos + new Vector2(width, Theme.RowHeight)) && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            {
                _activeActionHotkeyId = id;
                _captureStartedAt = ImGui.GetTime();
                assigning = true;
            }

            if (assigning && ImGui.GetTime() - _captureStartedAt > 0.12)
            {
                int captured = CaptureKey();
                if (captured == CancelCaptureKey)
                {
                    _activeActionHotkeyId = null;
                    assigning = false;
                }
                else if (captured != int.MinValue)
                {
                    meta.SetHotkey?.Invoke(captured);
                    _activeActionHotkeyId = null;
                    assigning = false;
                }
            }

            return assigning;
        }

        private static void DrawLabel(string label, string id, Vector2 pos, float width, MenuControlMeta meta)
        {
            UIContext.Draw.AddText(pos + new Vector2(0, 7), Theme.Color(Theme.Text), label);
            DrawLabelHelp(id, pos + new Vector2(ImGui.CalcTextSize(label).X + 7, 7), meta);
        }

        private static void DrawLabelHelp(string id, Vector2 pos, MenuControlMeta meta)
        {
            if (!meta.HasTooltip)
                return;

            Vector2 size = ImGui.CalcTextSize("[?]");
            bool hovered = ImGui.IsMouseHoveringRect(pos, pos + size);
            float t = Animations.Float($"{id}:tip", hovered ? 1f : 0f, 18f);
            UIContext.Draw.AddText(pos, Theme.Color(Theme.Lerp(Theme.TextMuted, Theme.AccentHot, t)), "[?]");

            if (hovered)
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(260f);
                ImGui.TextUnformatted(meta.Tooltip);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }

        private static void DrawHotkeyHint(Vector2 pos, Vector2 size, MenuControlMeta meta)
        {
            if (!meta.HasHotkey || meta.GetHotkey is null)
                return;

            string text = KeyName(meta.GetHotkey());
            if (text == "None")
                return;

            Vector2 textSize = ImGui.CalcTextSize(text);
            UIContext.Draw.AddText(pos + new Vector2(size.X - textSize.X, (size.Y - textSize.Y) * 0.5f), Theme.Color(Theme.WithAlpha(Theme.TextMuted, 0.58f)), text);
        }

        private static void DrawAssigningText(Vector2 pos)
        {
            float pulse = Animations.Pulse(6f, 0.5f, 1f);
            UIContext.Draw.AddText(pos, Theme.Color(Theme.WithAlpha(Theme.AccentHot, pulse)), "Press key");
        }

        private static bool IsPressed(int key)
        {
            return key > 0 && key < 256 && global::Input.IsPressed(key);
        }

        private static int CaptureKey()
        {
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                return 0x02;

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Middle))
                return 0x04;

            for (int key = 0x08; key <= 0xFE; key++)
            {
                if ((Win32.GetAsyncKeyState(key) & 0x8000) != 0)
                    return key == 0x1B ? CancelCaptureKey : key;
            }

            return int.MinValue;
        }

        private static string KeyName(int key)
        {
            return key switch
            {
                0 => "None",
                0x01 => "Mouse 1",
                0x02 => "Mouse 2",
                0x04 => "Mouse 3",
                0x05 => "Mouse 4",
                0x06 => "Mouse 5",
                0x08 => "Backspace",
                0x09 => "Tab",
                0x0D => "Enter",
                0x10 => "Shift",
                0x11 => "Ctrl",
                0x12 => "Alt",
                0x14 => "Caps",
                0x20 => "Space",
                0x25 => "Left",
                0x26 => "Up",
                0x27 => "Right",
                0x28 => "Down",
                >= 0x30 and <= 0x39 => ((char)key).ToString(),
                >= 0x41 and <= 0x5A => ((char)key).ToString(),
                >= 0x60 and <= 0x69 => $"Num {key - 0x60}",
                >= 0x70 and <= 0x87 => $"F{key - 0x6F}",
                _ => $"0x{key:X2}"
            };
        }
    }
}
