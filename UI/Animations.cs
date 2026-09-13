using ImGuiNET;
using System.Numerics;

namespace CS2.UI
{
    public static class Animations
    {
        private static readonly Dictionary<string, float> Floats = new();
        private static readonly Dictionary<string, Vector4> Colors = new();

        public static float Float(string id, float target, float speed = 12f)
        {
            float current = Floats.GetValueOrDefault(id, target);
            float amount = 1f - MathF.Exp(-speed * ImGui.GetIO().DeltaTime);
            current += (target - current) * amount;
            Floats[id] = current;
            return current;
        }

        public static Vector4 Color(string id, Vector4 target, float speed = 12f)
        {
            Vector4 current = Colors.GetValueOrDefault(id, target);
            float amount = 1f - MathF.Exp(-speed * ImGui.GetIO().DeltaTime);
            current = Theme.Lerp(current, target, amount);
            Colors[id] = current;
            return current;
        }

        public static float Pulse(float speed = 3f, float min = 0.35f, float max = 1f)
        {
            float wave = (MathF.Sin((float)ImGui.GetTime() * speed) + 1f) * 0.5f;
            return min + ((max - min) * wave);
        }

        public static void Clear()
        {
            Floats.Clear();
            Colors.Clear();
        }
    }
}
