using CS2.Core;
using CS2.Modules.Combat;
using CS2Dumper.Schemas;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CS2.Offsetssd;

namespace CS2.Modules.Visual
{
    public class Fovchanger : ModuleBase
    {

        public Fovchanger(GameContext gameContext) : base(gameContext)
        {            
        }

        private void SetFov(uint desiredFov)
        {
            var localPlayer = LocalPlayer;
            if (localPlayer is null)
                return;

            IntPtr cameraServices = Memory.ReadPointer(
                localPlayer.PawnAddress +
                client_dll.C_BasePlayerPawn.m_pCameraServices
            );

            if (cameraServices == IntPtr.Zero)
                return;

            bool isScoped = Memory.ReadBool(
                localPlayer.PawnAddress +
                client_dll.C_CSPlayerPawn.m_bIsScoped
            );

            if (isScoped)
                return;

            IntPtr fovAddress =
                cameraServices +
                client_dll.CCSPlayerBase_CameraServices.m_iFOV;

            uint currentFov = Memory.ReadUInt(fovAddress);

            if (currentFov != desiredFov)
                Memory.WriteUInt(fovAddress, desiredFov);
        }


        public void EnableFov()
        {
            SetFov((uint)Settings.Visuals.FovChanger.Fov);
        }

        public void DisableFov()
        {
            SetFov(100);
        }

        public override void Update()
        {

            if (Settings.Visuals.FovChanger.Enabled)
                EnableFov();
            else
                DisableFov();
        }
    }
}
