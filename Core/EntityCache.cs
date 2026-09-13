using CS2.Helpers;
using CS2Dumper.Offsets;
using CS2Dumper.Schemas;

namespace CS2.Core
{
    public sealed class EntityCache
    {
        private readonly object _sync = new();
        private readonly GameContext _gameContext;
        private Entity[] _entities = Array.Empty<Entity>();
        private Entity? _localPlayer;

        public IReadOnlyList<Entity> All
        {
            get
            {
                lock (_sync)
                    return _entities;
            }
        }

        public Entity? LocalPlayer
        {
            get
            {
                lock (_sync)
                    return _localPlayer;
            }
        }

        public EntityCache(GameContext gameContext)
        {
            _gameContext = gameContext;
        }

        public void Update()
        {
            IntPtr localPawnAddress = _gameContext.Memory.ReadPointer(
                _gameContext.ClientDll + ClientDll.dwLocalPlayerPawn
            );

            if (localPawnAddress == IntPtr.Zero)
            {
                Publish(Array.Empty<Entity>(), null);
                return;
            }

            Entity localPlayer = new(_gameContext.Memory, localPawnAddress, null);
            List<Entity> nextEntities = new(64);

            IntPtr entityList = _gameContext.Memory.ReadPointer(
                _gameContext.ClientDll + ClientDll.dwEntityList
            );

            if (entityList == IntPtr.Zero)
            {
                Publish(Array.Empty<Entity>(), localPlayer);
                return;
            }

            IntPtr controllerListEntry = _gameContext.Memory.ReadPointer(entityList + 0x10);
            if (controllerListEntry == IntPtr.Zero)
            {
                Publish(Array.Empty<Entity>(), localPlayer);
                return;
            }

            for (int i = 0; i < 64; i++)
            {
                IntPtr controller = _gameContext.Memory.ReadPointer(controllerListEntry + i * 0x70);
                if (controller == IntPtr.Zero)
                    continue;

                int pawnHandle = _gameContext.Memory.ReadInt(
                    controller + client_dll.CCSPlayerController.m_hPlayerPawn
                );

                if (pawnHandle == 0)
                    continue;

                IntPtr pawnListEntry = _gameContext.Memory.ReadPointer(
                    entityList + 0x8 * ((pawnHandle & 0x7FF) >> 9) + 0x10
                );

                if (pawnListEntry == IntPtr.Zero)
                    continue;

                IntPtr pawnAddress = _gameContext.Memory.ReadPointer(
                    pawnListEntry + 0x70 * (pawnHandle & 0x1FF)
                );

                if (pawnAddress == IntPtr.Zero || pawnAddress == localPawnAddress)
                    continue;

                nextEntities.Add(new Entity(_gameContext.Memory, pawnAddress, controller));
            }

            Publish(nextEntities.ToArray(), localPlayer);
        }

        public Entity[] Snapshot()
        {
            lock (_sync)
                return _entities;
        }

        private void Publish(Entity[] entities, Entity? localPlayer)
        {
            lock (_sync)
            {
                _entities = entities;
                _localPlayer = localPlayer;
            }
        }
    }
}
