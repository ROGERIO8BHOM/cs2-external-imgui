using System.Numerics;
using CS2Dumper.Schemas;
using Swed64;

namespace CS2.Core;

public sealed class Skeleton
{
    private const int BoneArrayOffset = 0x1C0;
    private const int BoneStride = 0x20;

    private readonly Swed _memory;

    public IntPtr SceneNode { get; }
    public IntPtr BoneArray { get; }

    public Vector3[] Positions { get; } = new Vector3[128];

    public Vector3 Head => Positions[7];
    public Vector3 Neck => Positions[6];
    public Vector3 Pelvis => Positions[1];

    public Skeleton(Swed memory, IntPtr pawn)
    {
        _memory = memory;

        SceneNode = memory.ReadPointer(
            pawn + client_dll.C_BaseEntity.m_pGameSceneNode
        );

        if (SceneNode == IntPtr.Zero)
            return;

        BoneArray = memory.ReadPointer(SceneNode + BoneArrayOffset);

        Update();
    }

    public void Update()
    {
        if (BoneArray == IntPtr.Zero)
            return;

        for (int i = 0; i < Positions.Length; i++)
        {
            Positions[i] =
                _memory.ReadVec(BoneArray + i * BoneStride);
        }
    }

    public Vector3 Get(int bone)
    {
        if ((uint)bone >= Positions.Length)
            return Vector3.Zero;

        return Positions[bone];
    }
}