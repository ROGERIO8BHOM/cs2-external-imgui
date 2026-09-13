using CS2.Core;
using CS2.Helpers;
using CS2Dumper.Schemas;
using System.Numerics;

namespace CS2.Modules.Combat
{
    public sealed class Nothing : ModuleBase
    {
        private readonly AimBot _aimBot;
        private Vector3 _previousPunch;

        public Nothing(GameContext gameContext, AimBot aimBot) : base(gameContext)
        {
            _aimBot = aimBot;
        }

        public override void Update()
        {
            Entity? localPlayer = LocalPlayer;

            if (!Settings.Combat.NoRecoil.Enabled ||
                localPlayer == null ||
                !localPlayer.IsAlive)
            {
                Reset();
                return;
            }

            if (_aimBot.IsControllingViewAngles)
            {
                _previousPunch = TryReadCurrentPunch(GameContext, localPlayer, out Vector3 activePunch)
                    ? activePunch
                    : Vector3.Zero;
                return;
            }

            int shotsFired = Memory.ReadInt(
                localPlayer.PawnAddress + client_dll.C_CSPlayerPawn.m_iShotsFired
            );

            if (shotsFired <= 0)
            {
                Reset();
                return;
            }

            if (!TryReadCurrentPunch(GameContext, localPlayer, out Vector3 currentPunch))
            {
                Reset();
                return;
            }

            float strength = Math.Clamp(Settings.Combat.NoRecoil.Strength, 0f, 2f);
            Vector3 punchDelta = (currentPunch - _previousPunch) * strength;
            IntPtr viewAnglesAddress = ClientDll + CS2Dumper.Offsets.ClientDll.dwViewAngles;
            Vector3 viewAngles = Memory.ReadVec(viewAnglesAddress);
            Vector3 correctedAngles = NormalizeAngles(viewAngles - punchDelta);

            Memory.WriteVec(viewAnglesAddress, correctedAngles);
            _previousPunch = currentPunch;
        }

        internal static bool TryReadCurrentPunch(GameContext gameContext, Entity localPlayer, out Vector3 punch)
        {
            IntPtr cameraServices = gameContext.Memory.ReadPointer(
                localPlayer.PawnAddress + client_dll.C_BasePlayerPawn.m_pCameraServices
            );

            if (cameraServices != IntPtr.Zero)
            {
                Vector3 viewPunch = gameContext.Memory.ReadVec(
                    cameraServices + client_dll.CPlayer_CameraServices.m_vecCsViewPunchAngle
                );

                if (IsValidPunch(viewPunch) && viewPunch.LengthSquared() > 0.000001f)
                {
                    punch = viewPunch;
                    return true;
                }
            }

            IntPtr aimPunchServices = gameContext.Memory.ReadPointer(
                localPlayer.PawnAddress + client_dll.C_CSPlayerPawn.m_pAimPunchServices
            );

            if (aimPunchServices != IntPtr.Zero)
            {
                Vector3 unpredictablePunch = gameContext.Memory.ReadVec(
                    aimPunchServices + client_dll.CCSPlayer_AimPunchServices.m_unpredictableBaseAngle
                );

                if (IsValidPunch(unpredictablePunch) && unpredictablePunch.LengthSquared() > 0.000001f)
                {
                    punch = unpredictablePunch;
                    return true;
                }

                Vector3 predictablePunch = gameContext.Memory.ReadVec(
                    aimPunchServices + client_dll.CCSPlayer_AimPunchServices.m_predictableBaseAngle
                );

                if (IsValidPunch(predictablePunch))
                {
                    punch = predictablePunch;
                    return true;
                }
            }

            punch = Vector3.Zero;
            return false;
        }

        private static bool IsValidPunch(Vector3 punch)
        {
            return float.IsFinite(punch.X) &&
                   float.IsFinite(punch.Y) &&
                   float.IsFinite(punch.Z) &&
                   MathF.Abs(punch.X) <= 89f &&
                   MathF.Abs(punch.Y) <= 180f;
        }

        private void Reset()
        {
            _previousPunch = Vector3.Zero;
        }

        internal static Vector3 NormalizeAngles(Vector3 angles)
        {
            angles.X = Math.Clamp(angles.X, -89f, 89f);

            while (angles.Y > 180f)
                angles.Y -= 360f;

            while (angles.Y < -180f)
                angles.Y += 360f;

            angles.Z = 0f;
            return angles;
        }
    }
}
