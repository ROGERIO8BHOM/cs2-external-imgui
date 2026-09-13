using ImGuiNET;
using System.Numerics;

namespace CS2.UI.Widgets
{
    public static class Checkbox
    {
        public static bool Draw(string label, ref bool value)
        {
            Vector2 pos = ImGui.GetCursorScreenPos();
            Vector2 size = new(18, 18);
            string id = $"checkbox:{label}";

            ImGui.InvisibleButton(id, size);
            bool clicked = ImGui.IsItemClicked();

            if (clicked)
                value = !value;

            float t = Animations.Float(id, value ? 1f : 0f, 18f);
            Vector4 fill = Animations.Color($"{id}:color", value ? Theme.Accent : Theme.Input, 18f);

            UIContext.Draw.AddRectFilled(pos, pos + size, Theme.Color(fill), Theme.SmallRounding);
            UIContext.Draw.AddRect(pos, pos + size, Theme.Color(value ? Theme.Accent : Theme.Border), Theme.SmallRounding);

            if (t > 0.05f)
            {
                Vector2 a = pos + new Vector2(4, 9);
                Vector2 b = pos + new Vector2(8, 13);
                Vector2 c = pos + new Vector2(14, 5);
                UIContext.Draw.AddLine(a, b, Theme.Color(Theme.Text), 2f);
                UIContext.Draw.AddLine(b, c, Theme.Color(Theme.Text), 2f);
            }

            if (!string.IsNullOrWhiteSpace(label) && !label.StartsWith("##"))
                UIContext.Draw.AddText(pos + new Vector2(28, 0), Theme.Color(Theme.Text), label);

            return clicked;
        }
    }
}
