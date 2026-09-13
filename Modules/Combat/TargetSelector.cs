using CS2.Core;
using CS2.Enums;
using CS2.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CS2.Modules.Combat
{
    public sealed class TargetSelector
    {
        private GameContext _gameContext { get; }
        public TargetSelector(GameContext gameContext)
        {
            _gameContext = gameContext;
        }

        private List<Entity> GetTargets(bool aimTeam, AimBone targetBone)
        {
            Entity? localPlayer = _gameContext.LocalPlayer;

            if (localPlayer == null || !localPlayer.IsAlive)
                return new List<Entity>();

            List<Entity> targets = new();

            foreach (Entity entity in _gameContext.Entities.All)
            {
                if (!entity.IsAlive)
                    continue;

                if (!aimTeam && !entity.IsEnemy(localPlayer))
                    continue;

                Vector3 targetBonePosition = entity.Skeleton.Get(
                    (int)targetBone
                );

                if (targetBonePosition == Vector3.Zero)
                    continue;

                entity.Head2D = Calculations.WorldToScreen(
                    _gameContext.ViewMatrix,
                    targetBonePosition,
                    _gameContext.ScreenSize
                );

                entity.PixelDistance = Vector2.Distance(
                    _gameContext.ScreenSize / 2,
                    entity.Head2D
                );

                targets.Add(entity);
            }

            return targets;
        }

        public Entity? GetBestByFov(float maxFov, bool aimTeam, AimBone targetBone)
        {
            List<Entity> targets = GetTargets(aimTeam, targetBone);

            Entity? target = targets
            .Where(e => e.PixelDistance <= maxFov)
            .OrderBy(e => Vector3.Distance(_gameContext.LocalPlayer!.Origin, e.Origin))
            .FirstOrDefault();

            return target;
        }

        public Entity? GetNearest(bool aimTeam, AimBone targetBone)
        {
            List<Entity> targets = GetTargets(aimTeam, targetBone);

            return targets.MinBy(e => Vector3.Distance(_gameContext.LocalPlayer!.Origin, e.Origin));
        }

        
    }
}
