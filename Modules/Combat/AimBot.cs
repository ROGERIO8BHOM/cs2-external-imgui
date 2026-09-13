using CS2.Core;
using CS2.Helpers;
using CS2Dumper.Offsets;
using System.Numerics;

namespace CS2.Modules.Combat
{
    public class AimBot : ModuleBase
    {
        private readonly TargetSelector _targetSelector;
        public bool IsControllingViewAngles { get; private set; }

        public AimBot(GameContext gameContext) : base(gameContext)
        {
            _targetSelector = new TargetSelector(gameContext);
        }

        public override void Update()
        {
            IsControllingViewAngles = false;

            if (!Settings.Combat.AimBot.Enabled)
                return;

            if (!Input.IsDown(Settings.Combat.AimBot.Key))
                return;

            float smooth = MathF.Max(Settings.Combat.AimBot.Smoothing, 1f);

            Entity? target = Settings.Combat.AimBot.FovEnabled
                ? _targetSelector.GetBestByFov(
                    Settings.Combat.AimBot.Fov,
                    Settings.Combat.AimBot.AimTeam,
                    Settings.Combat.AimBot.TargetBone
                )
                : _targetSelector.GetNearest(
                    Settings.Combat.AimBot.AimTeam,
                    Settings.Combat.AimBot.TargetBone
                );

            if (target == null)
                return;

            Vector3 selectedBonePosition = target.Skeleton.Get((int)Settings.Combat.AimBot.TargetBone);
            if (selectedBonePosition == Vector3.Zero)
                return;

            Entity? localPlayer = LocalPlayer;
            if (localPlayer == null)
                return;

            Vector3 targetAngles = Calculations.CalculateAngleVector(localPlayer.Skeleton.Head, selectedBonePosition);
            IntPtr viewAnglesAddress = ClientDll + CS2Dumper.Offsets.ClientDll.dwViewAngles;
            Vector3 currentViewAngles = Memory.ReadVec(viewAnglesAddress);
            Vector3 currentRecoil = GetCurrentRecoil(localPlayer);

            // Enquanto o aimbot controla a mira, trabalha no angulo visual
            // (view angles + recoil) e remove o recoil antes de escrever.
            Vector3 currentVisualAngles = NoRecoil.NormalizeAngles(currentViewAngles + currentRecoil);
            Vector3 smoothedVisualAngles = Calculations.SmoothAngles(
                currentVisualAngles,
                targetAngles,
                smooth
            );
            Vector3 finalViewAngles = NoRecoil.NormalizeAngles(smoothedVisualAngles - currentRecoil);

            Memory.WriteVec(viewAnglesAddress, finalViewAngles);
            IsControllingViewAngles = true;
        }

        private Vector3 GetCurrentRecoil(Entity localPlayer)
        {
            if (!Settings.Combat.NoRecoil.Enabled)
                return Vector3.Zero;

            int shotsFired = Memory.ReadInt(
                localPlayer.PawnAddress + CS2Dumper.Schemas.client_dll.C_CSPlayerPawn.m_iShotsFired
            );

            if (shotsFired <= 0 ||
                !NoRecoil.TryReadCurrentPunch(GameContext, localPlayer, out Vector3 currentPunch))
            {
                return Vector3.Zero;
            }

            float strength = Math.Clamp(Settings.Combat.NoRecoil.Strength, 0f, 2f);
            return currentPunch * strength;
        }
    }
}
