using CS2.Core;

namespace CS2.Modules.Combat
{
    public class AntiAim : ModuleBase
    {
        public AntiAim(GameContext gameContext) : base(gameContext)
        {
        }

        public override void Update()
        {
            //if (!Settings.Combat.AimbotEnabled)
            //    return;

            //Entity? localPlayer = LocalPlayer;
            //if (localPlayer == null || !localPlayer.IsAlive)
            //    return;

            //Vector3 oldAngle = new Vector3(-90, 10, 0);

            ////client.dll + AD4CB0 - F2 0F11 81
            //byte[] oldBytes = Memory.ReadBytes(ClientDll + 0xAD4CB0, 8);
            //Memory.WriteBytes(ClientDll + 0xAD4CB0, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            //Memory.WriteVec(ClientDll + CS2Dumper.Offsets.ClientDll.dwViewAngles, new Vector3(-45, 0, 0));
            //while (Settings.Combat.AimbotEnabled)
            //{
            //    oldAngle -= new Vector3(0, 10, 0);
            //    Memory.WriteVec(localPlayer.PawnAddress + CS2Dumper.Schemas.client_dll.C_BasePlayerPawn.v_angle, oldAngle);
            //    Thread.Sleep(1);
            //}
            //Memory.WriteBytes(ClientDll + 0xAD4CB0, oldBytes);
        }
    }
}
