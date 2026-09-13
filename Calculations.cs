using CS2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CS2
{
    public static class Calculations
    {
        public static Vector3 SmoothAngles(Vector3 from, Vector3 to, float smoothing)
        {

            return from + ((to - from) / smoothing);
        }
        public static Vector2 CalculateAngles(Vector3 from, Vector3 to)
        {
            float deltaX = to.X - from.X;
            float deltaY = to.Y - from.Y;
            float deltaZ = to.Z - from.Z;

            float yaw = (float)(Math.Atan2(deltaY, deltaX) * 180 / Math.PI);
            double distance = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));
            float pitch = -(float)(Math.Atan2(deltaZ, distance) * 180 / Math.PI);

            return new Vector2(yaw, pitch);
        }

        public static Vector3 CalculateAngleVector(Vector3 from, Vector3 to)
        {
            Vector2 angles = CalculateAngles(from, to);
            return new Vector3(angles.Y, angles.X, 0.0f);
        }

        public static Vector2 WorldToScreen(ViewMatrix viewMatrix, Vector3 position, Vector2 screenSize)
        {
            float screenW = (viewMatrix.m41 * position.X) + (viewMatrix.m42 * position.Y) + (viewMatrix.m43 * position.Z) + viewMatrix.m44;

            if (screenW > 0.001f)
            {
                float screenX = (viewMatrix.m11 * position.X) + (viewMatrix.m12 * position.Y) + (viewMatrix.m13 * position.Z) + viewMatrix.m14;
                float screenY = (viewMatrix.m21 * position.X) + (viewMatrix.m22 * position.Y) + (viewMatrix.m23 * position.Z) + viewMatrix.m24;

                float camX = screenSize.X / 2;
                float camY = screenSize.Y / 2;

                float X = camX + (camX * screenX / screenW);
                float Y = camY - (camY * screenY / screenW);

                return new Vector2(X, Y);
            } else
            {
                return new Vector2(-99, -99);
            }
        }
    
    }
}
