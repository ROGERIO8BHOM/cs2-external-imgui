using CS2.Core;
using CS2Dumper.Schemas;


namespace CS2.Modules.Visual
{
    public class AntiBang : ModuleBase
    {
        public AntiBang(GameContext gameContext) :  base (gameContext)
        {

        }

        public override void Update()
        {
            if (Settings.Visuals.Antibang.Enabled)
            {
                float flashTime = Memory.ReadFloat(LocalPlayer!.PawnAddress + client_dll.C_CSPlayerPawnBase.m_flFlashBangTime);
                if (flashTime > 0)
                    Memory.WriteFloat(LocalPlayer!.PawnAddress + client_dll.C_CSPlayerPawnBase.m_flFlashBangTime, 0);
            }
        }
    }
}
