using ImGuiNET;
using System.Numerics;

namespace CS2.UI
{
    public static class Theme
    {
        public static readonly Vector4 Window = Rgba(9, 11, 14);
        public static readonly Vector4 Sidebar = Rgba(12, 15, 20);
        public static readonly Vector4 Topbar = Rgba(14, 17, 22);
        public static readonly Vector4 Panel = Rgba(15, 18, 23);
        public static readonly Vector4 PanelLight = Rgba(19, 23, 30);
        public static readonly Vector4 Border = Rgba(35, 42, 54);
        public static readonly Vector4 Accent = Rgba(44, 132, 255);
        public static readonly Vector4 AccentHot = Rgba(92, 166, 255);
        public static readonly Vector4 AccentDim = Rgba(24, 70, 130);
        public static readonly Vector4 Glow = Rgba(42, 126, 255, 95);
        public static readonly Vector4 Text = Rgba(235, 239, 245);
        public static readonly Vector4 TextMuted = Rgba(127, 139, 153);
        public static readonly Vector4 TextDisabled = Rgba(72, 81, 93);
        public static readonly Vector4 Input = Rgba(10, 13, 17);
        public static readonly Vector4 Success = Rgba(52, 211, 153);

        public const float Rounding = 20f;
        public const float SmallRounding = 5f;
        public const float Padding = 14f;
        public const float RowHeight = 32f;
        public const float SidebarWidth = 86f;
        public const float TopbarHeight = 52f;
        public const float TabHeight = 38f;
        public const float GroupHeaderHeight = 34f;

        public static uint Color(Vector4 color)
        {
            return ImGui.GetColorU32(color);
        }

        public static Vector4 WithAlpha(Vector4 color, float alpha)
        {
            return new Vector4(color.X, color.Y, color.Z, alpha);
        }

        public static Vector4 Lerp(Vector4 from, Vector4 to, float amount)
        {
            amount = Math.Clamp(amount, 0f, 1f);
            return new Vector4(
                from.X + ((to.X - from.X) * amount),
                from.Y + ((to.Y - from.Y) * amount),
                from.Z + ((to.Z - from.Z) * amount),
                from.W + ((to.W - from.W) * amount));
        }

        public static Vector4 Rgba(int r, int g, int b, int a = 255)
        {
            return new Vector4(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        public static void AddGlow(ImDrawListPtr draw, Vector2 min, Vector2 max, Vector4 color, float rounding, float strength = 1f)
        {
            strength = Math.Clamp(strength, 0f, 1f);
            if (strength <= 0.01f)
                return;

            for (int i = 3; i >= 1; i--)
            {
                float spread = i * 3f;
                float alpha = color.W * strength * (0.12f / i);
                draw.AddRect(
                    min - new Vector2(spread),
                    max + new Vector2(spread),
                    Color(WithAlpha(color, alpha)),
                    rounding + spread,
                    ImDrawFlags.RoundCornersAll,
                    2f);
            }
        }
    }
}
