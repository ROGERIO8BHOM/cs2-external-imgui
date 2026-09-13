using CS2.Config;
using CS2.Core;
using CS2.Helpers;
using Swed64;

namespace CS2.Modules
{
    public abstract class ModuleBase
    {
        protected readonly GameContext GameContext;

        protected AppSettings Settings => GameContext.Settings;
        protected Swed Memory => GameContext.Memory;
        protected IntPtr ClientDll => GameContext.ClientDll;
        protected Entity? LocalPlayer => GameContext.LocalPlayer;

        protected ModuleBase(GameContext gameContext)
        {
            GameContext = gameContext;
        }

        public abstract void Update();
    }
}
