using ImGuiNET;
using System.Numerics;

namespace CS2.UI
{
    public static class UIContext
    {

        public static ImDrawListPtr Draw;

        public static Vector2 Cursor = new Vector2(0, 0);
        public static Vector2 Position = new Vector2(0, 0);
        public static Vector2 ContentMin = new Vector2(0, 0);
        public static Vector2 ContentMax = new Vector2(0, 0);
        private static readonly List<(Vector2 Min, Vector2 Max)> DragBlockedRects = new();

        private static Vector2 _windowPos;
        public static Vector2 WindowPosition { get => _windowPos; set  {

                if (!Equals(_windowPos, value))
                {
                    _windowPos = value;
                    UpdatePosition();
                }
            } 
        }

        public static Vector2 Size;

        public static void Reset(Vector2 windowPosition, Vector2 size)
        {
            WindowPosition = windowPosition;
            Size = size;
            Cursor = Vector2.Zero;
            Position = windowPosition;
            ContentMin = windowPosition;
            ContentMax = windowPosition + size;
            DragBlockedRects.Clear();
        }

        public static void BlockDrag(Vector2 min, Vector2 max)
        {
            DragBlockedRects.Add((min, max));
        }

        public static bool IsDragBlocked(Vector2 point)
        {
            foreach ((Vector2 min, Vector2 max) in DragBlockedRects)
            {
                if (point.X >= min.X && point.X <= max.X && point.Y >= min.Y && point.Y <= max.Y)
                    return true;
            }

            return false;
        }
        
        public static void Next(float x, float y)
        {
            Cursor.X += x;
            Cursor.Y += y;
            UpdatePosition();
        }

        public static void NextX(float x)
        {
            Cursor.X += x;
            UpdatePosition();
        }

        public static void NextY(float y)
        {
            Cursor.Y += y;
            UpdatePosition();
        }

        private static void UpdatePosition()
        {
            Position = _windowPos + Cursor;
        }  

    }
}
