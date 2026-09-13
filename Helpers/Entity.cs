using System.Numerics;
using CS2.Core;
using CS2Dumper.Schemas;
using Swed64;

namespace CS2.Helpers
{
    public class Entity
    {
        public IntPtr PawnAddress { get; }

        public Vector3 Origin { get; }
        public Vector3 View { get; }
        public Vector3 Head { get; }

        public Vector2 Head2D { get; set; }
        public Skeleton Skeleton {  get; set; }
        public string? Name { get; set; }
        public int Health { get; }
        public int Team { get; }
        public uint LifeState { get; }
        public float Distance { get; set; }
        public float PixelDistance { get; set; }

        public bool IsAlive => Health > 0 && LifeState == 256;

        public bool IsEnemy(Entity localPlayer) => Team != localPlayer.Team;

        public Entity(Swed memory, IntPtr pawnAddress, IntPtr? controllerAddress)
        {   
            PawnAddress = pawnAddress;

            Health = memory.ReadInt(PawnAddress + client_dll.C_BaseEntity.m_iHealth);
            Team = memory.ReadInt(PawnAddress + client_dll.C_BaseEntity.m_iTeamNum);
            LifeState = memory.ReadUInt(PawnAddress + client_dll.C_BaseEntity.m_lifeState);

            Origin = memory.ReadVec(PawnAddress + client_dll.C_BasePlayerPawn.m_vOldOrigin);
            View = memory.ReadVec(PawnAddress + client_dll.C_BaseModelEntity.m_vecViewOffset);
            Head = Origin + View;
            if (controllerAddress.HasValue)
            {
                Name = memory.ReadString(
                    controllerAddress.Value + client_dll.CBasePlayerController.m_iszPlayerName,
                    16
                );
            }
            Skeleton = new Skeleton(memory, pawnAddress);
        }

    }
}