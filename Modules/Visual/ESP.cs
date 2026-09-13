using CS2.Core;
using CS2.Helpers;
using ImGuiNET;
using System.Numerics;

namespace CS2.Modules.Visual
{
    public sealed class ESP : ModuleBase
    {
        private static readonly int[,] BonePairs =
        {
            { 7, 6 },
            { 6, 5 },
            { 5, 4 },
            { 4, 3 },
            { 3, 2 },
            { 2, 1 },
            { 5, 8 },
            { 8, 9 },
            { 9, 10 },
            { 10, 11 },
            { 5, 12 },
            { 12, 13 },
            { 13, 14 },
            { 14, 15 },
            { 1, 17 },
            { 17, 18 },
            { 18, 19 },
            { 1, 20 },
            { 20, 21 },
            { 21, 22 }
        };

        public ESP(GameContext gameContext) : base(gameContext)
        {
        }

        public override void Update()
        {
            if (!Settings.Visuals.Esp.Enabled)
                return;

            Entity? localPlayer = LocalPlayer;
            if (localPlayer == null)
                return;

            ImDrawListPtr draw = ImGui.GetBackgroundDrawList();
            Entity[] entities = GameContext.Entities.Snapshot();

            foreach (Entity entity in entities)
            {
                if (!entity.IsAlive)
                    continue;

                bool isEnemy = entity.IsEnemy(localPlayer);
                if (!isEnemy && !Settings.Visuals.Esp.Team)
                    continue;

                DrawEntity(draw, entity, isEnemy);
            }
        }

        private void DrawEntity(ImDrawListPtr draw, Entity entity, bool isEnemy)
        {
            Vector3 headWorld = entity.Skeleton.Head == Vector3.Zero ? entity.Head : entity.Skeleton.Head;
            Vector2 head = Calculations.WorldToScreen(GameContext.ViewMatrix, headWorld, GameContext.ScreenSize);
            Vector2 feet = Calculations.WorldToScreen(GameContext.ViewMatrix, entity.Origin, GameContext.ScreenSize);

            if (!IsDrawablePoint(head) || !IsDrawablePoint(feet))
                return;

            float height = MathF.Abs(feet.Y - head.Y);
            if (height < 8f)
                return;

            float width = height * 0.42f;
            Vector2 boxMin = new(feet.X - width * 0.5f, head.Y - height * 0.08f);
            Vector2 boxMax = new(feet.X + width * 0.5f, feet.Y);

            Vector4 relationColor = isEnemy ? Settings.Visuals.Esp.EnemyColor : Settings.Visuals.Esp.TeamColor;
            uint boxColor = ImGui.GetColorU32(Tint(Settings.Visuals.Esp.BoxColor, relationColor));
            uint boneColor = ImGui.GetColorU32(Tint(Settings.Visuals.Esp.BoneColor, relationColor));
            uint nameColor = ImGui.GetColorU32(Tint(Settings.Visuals.Esp.NameColor, relationColor));
            uint tracerColor = ImGui.GetColorU32(Tint(Settings.Visuals.Esp.TracerColor, relationColor));
            uint viewLineColor = ImGui.GetColorU32(Tint(Settings.Visuals.Esp.ViewLineColor, relationColor));

            if (Settings.Visuals.Esp.Box)
                DrawBox(draw, boxMin, boxMax, boxColor);

            if (Settings.Visuals.Esp.HealthBar)
                DrawHealthBar(draw, entity.Health, boxMin, boxMax);

            if (Settings.Visuals.Esp.NameTags)
                DrawNameTag(draw, entity.Name ?? (isEnemy ? "Enemy" : "Team"), boxMin, boxMax, nameColor);

            if (Settings.Visuals.Esp.Bones)
                DrawBones(draw, entity, boneColor);

            if (Settings.Visuals.Esp.ViewLine)
                DrawViewLine(draw, entity, headWorld, height, viewLineColor);

            if (Settings.Visuals.Esp.Tracer)
                DrawTracer(draw, feet, tracerColor);
        }

        private static void DrawBox(ImDrawListPtr draw, Vector2 min, Vector2 max, uint color)
        {
            draw.AddRect(min, max, color, 0f, ImDrawFlags.None, 1.4f);
        }

        private void DrawHealthBar(ImDrawListPtr draw, int health, Vector2 boxMin, Vector2 boxMax)
        {
            float clampedHealth = Math.Clamp(health, 0, 100);
            float boxHeight = boxMax.Y - boxMin.Y;
            float filledHeight = boxHeight * (clampedHealth / 100f);

            Vector2 barMin = new(boxMin.X - 7, boxMin.Y);
            Vector2 barMax = new(boxMin.X - 3, boxMax.Y);
            Vector2 fillMin = new(barMin.X, barMax.Y - filledHeight);

            Vector4 baseColor = clampedHealth > 60
                ? new Vector4(0.25f, 1f, 0.45f, 0.95f)
                : clampedHealth > 30
                    ? new Vector4(1f, 0.75f, 0.22f, 0.95f)
                    : new Vector4(1f, 0.25f, 0.25f, 0.95f);
            Vector4 healthColor = new(
                (baseColor.X +  Settings.Visuals.Esp.HealthBarColor.X) * 0.5f,
                (baseColor.Y + Settings.Visuals.Esp.HealthBarColor.Y) * 0.5f,
                (baseColor.Z + Settings.Visuals.Esp.HealthBarColor.Z) * 0.5f,
                Settings.Visuals.Esp.HealthBarColor.W
            );

            draw.AddRectFilled(barMin, barMax, ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 0.65f)));
            draw.AddRectFilled(fillMin, barMax, ImGui.GetColorU32(healthColor));
        }

        private static void DrawNameTag(ImDrawListPtr draw, string name, Vector2 boxMin, Vector2 boxMax, uint textColor)
        {
            Vector2 textSize = ImGui.CalcTextSize(name);
            float centerX = (boxMin.X + boxMax.X) * 0.5f;
            Vector2 textPos = new(centerX - (textSize.X * 0.5f), boxMin.Y - textSize.Y - 4);

            draw.AddText(textPos, textColor, name);
        }

        private void DrawBones(ImDrawListPtr draw, Entity entity, uint color)
        {
            for (int i = 0; i < BonePairs.GetLength(0); i++)
            {
                Vector3 fromWorld = entity.Skeleton.Get(BonePairs[i, 0]);
                Vector3 toWorld = entity.Skeleton.Get(BonePairs[i, 1]);

                if (fromWorld == Vector3.Zero || toWorld == Vector3.Zero)
                    continue;

                Vector2 from = Calculations.WorldToScreen(GameContext.ViewMatrix, fromWorld, GameContext.ScreenSize);
                Vector2 to = Calculations.WorldToScreen(GameContext.ViewMatrix, toWorld, GameContext.ScreenSize);

                if (!IsDrawablePoint(from) || !IsDrawablePoint(to))
                    continue;

                draw.AddLine(from, to, color, 1.35f);
            }
        }

        private void DrawViewLine(ImDrawListPtr draw, Entity entity, Vector3 headWorld, float skeletonHeight, uint color)
        {
            Vector3 directionStart = entity.Skeleton.Get(26);
            Vector3 directionTarget = entity.Skeleton.Get(27);
            Vector3 direction = directionTarget - directionStart;

            if (directionStart == Vector3.Zero ||
                directionTarget == Vector3.Zero ||
                direction.LengthSquared() < 0.0001f)
            {
                return;
            }

            direction = Vector3.Normalize(direction);
            Vector2 from = Calculations.WorldToScreen(GameContext.ViewMatrix, headWorld, GameContext.ScreenSize);
            Vector2 projectedTarget = Calculations.WorldToScreen(
                GameContext.ViewMatrix,
                headWorld + direction * 12f,
                GameContext.ScreenSize
            );

            if (!IsDrawablePoint(from) || !IsDrawablePoint(projectedTarget))
                return;

            Vector2 screenDirection = projectedTarget - from;
            if (screenDirection.LengthSquared() < 0.01f)
                return;

            float length = Math.Clamp(skeletonHeight * 0.12f, 7f, 18f);
            Vector2 to = from + Vector2.Normalize(screenDirection) * length;
            draw.AddLine(from, to, color, 1.25f);
        }

        private void DrawTracer(ImDrawListPtr draw, Vector2 feet, uint color)
        {
            Vector2 from = new(GameContext.ScreenSize.X * 0.5f, GameContext.ScreenSize.Y - 2f);

            draw.AddLine(from, feet, color, 1.2f);
        }

        private static Vector4 Tint(Vector4 optionColor, Vector4 relationColor)
        {
            return new Vector4(
                optionColor.X * relationColor.X,
                optionColor.Y * relationColor.Y,
                optionColor.Z * relationColor.Z,
                optionColor.W * relationColor.W
            );
        }

        private bool IsDrawablePoint(Vector2 point)
        {
            return float.IsFinite(point.X) &&
                   float.IsFinite(point.Y) &&
                   !(point.X == -99f && point.Y == -99f) &&
                   point.X > -2000 &&
                   point.Y > -2000 &&
                   point.X <= GameContext.ScreenSize.X + 2000 &&
                   point.Y <= GameContext.ScreenSize.Y + 2000;
        }
    }
}
