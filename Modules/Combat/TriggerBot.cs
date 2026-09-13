using CS2.Core;
using CS2.Helpers;
using CS2Dumper;
using CS2Dumper.Schemas;

namespace CS2.Modules.Combat
{
    public class TriggerBot : ModuleBase
    {
        public TriggerBot(GameContext gameContext) : base(gameContext)
        {
        }

        public override void Update()
        {
            if (!Settings.Combat.TriggetBot.Enabled)
                return;

            Entity? localPlayer = LocalPlayer;
            if (localPlayer == null || !localPlayer.IsAlive)
                return;

            int crosshairId = Memory.ReadInt(
                localPlayer.PawnAddress + client_dll.C_CSPlayerPawn.m_iIDEntIndex
            );

            if (crosshairId <= 0)
                return;

            IntPtr entityList = Memory.ReadPointer(ClientDll + CS2Dumper.Offsets.ClientDll.dwEntityList);
            if (entityList == IntPtr.Zero)
                return;

            IntPtr listEntry = Memory.ReadPointer(entityList + (0x8 * ((crosshairId & 0x7FFF) >> 9) + 0x10));
            if (listEntry == IntPtr.Zero)
                return;

            IntPtr entityPawn = Memory.ReadPointer(listEntry + (0x70 * (crosshairId & 0x1FF)));
            if (entityPawn == IntPtr.Zero)
                return;

            uint lifeState = Memory.ReadUInt(entityPawn + client_dll.C_BaseEntity.m_lifeState);
            if (lifeState != 256)
                return;

            int entTeam = Memory.ReadInt(entityPawn + client_dll.C_BaseEntity.m_iTeamNum);
            int health = Memory.ReadInt(entityPawn + client_dll.C_BaseEntity.m_iHealth);
            if (entTeam == localPlayer.Team && !Settings.Combat.TriggetBot.ShootTeam)
                return;

            if (health <= 0)
                return;

            Memory.WriteInt(ClientDll + Buttons.attack, 65537);
            Thread.Sleep(10);
            Memory.WriteInt(ClientDll + Buttons.attack, 256);
        }
    }
}
