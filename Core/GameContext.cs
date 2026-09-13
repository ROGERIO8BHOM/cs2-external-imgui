using CS2.Config;
using CS2.Helpers;
using Swed64;
using System.Numerics;

namespace CS2.Core
{
    public sealed class GameContext
    {
        public AppSettings Settings { get; } = new();
        public Swed Memory { get; } = new Swed("cs2");
        public IntPtr ClientDll { get; private set; }
        public Vector2 ScreenSize { get; private set; } = new(1920, 1080);
        public ViewMatrix ViewMatrix { get; private set; } = new();
        public EntityCache Entities { get; }
        public Entity? LocalPlayer => Entities.LocalPlayer;

        public GameContext()
        {
            ClientDll = Memory.GetModuleBase("client.dll");
            Entities = new EntityCache(this);
        }

        public void Update()
        {
            ViewMatrix = new ViewMatrix().ReadMatrix(
                Memory,
                ClientDll + CS2Dumper.Offsets.ClientDll.dwViewMatrix
            );

            Entities.Update();
        }

        public void SetScreenSize(Vector2 screenSize)
        {
            if (screenSize.X > 0 && screenSize.Y > 0)
                ScreenSize = screenSize;
        }
    }
}
