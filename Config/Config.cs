using CS2.Enums;
using System.Numerics;

namespace CS2.Config
{
    public sealed class AppSettings
    {
        public CombatSettings Combat { get; } = new();
        public VisualSettings Visuals { get; } = new();
        public MenuSettings Menu { get; } = new();
    }

    public class BaseSetting
    {
        public bool Enabled { get; set; }
        public int Key { get; set; }

        protected BaseSetting(bool enabled = false, int key = 0)
        {
            Enabled = enabled;
            Key = key;
        }
    }

    public sealed class CombatSettings
    {
        public AimBotSettings AimBot { get; } = new();
        public TriggetBotSettings TriggetBot { get; } = new();
        public NoRecoilSettings NoRecoil { get; } = new();
    }

    public sealed class AimBotSettings : BaseSetting
    {
        public AimBotSettings() : base(enabled: false, key: 0x46)
        {
        }

        public bool FovEnabled { get; set; } = true;
        public bool AimTeam { get; set; } = false;
        public int Fov { get; set; } = 80;
        public float Smoothing { get; set; } = 20f;
        public AimBone TargetBone { get; set; } = AimBone.Head;
    }

    public sealed class TriggetBotSettings : BaseSetting
    {
        public TriggetBotSettings() : base(enabled: false, key: 0x05)
        {
        }
        public bool ShootTeam { get; set; } = false;
    }

    public sealed class NoRecoilSettings : BaseSetting
    {
        public NoRecoilSettings() : base(enabled: false)
        {
        }

        public float Strength { get; set; } = 1f;
    }

    public sealed class VisualSettings
    {
        public FovChangerSettings FovChanger { get;  } = new();
        public Antibang Antibang { get; } = new();
        public EspSettings Esp { get; } = new();
        public bool ShowFov { get; set; } = true;
        public bool ActiveList { get; set; } = true;
        public bool VisibleCheck { get; set; }
        public bool FriendList { get; set; }
    }

    public sealed class EspSettings : BaseSetting
    {
        public EspSettings() : base(enabled: true)
        {
        }

        public bool Team { get; set; } = false;
        public bool NameTags { get; set; } = true;
        public bool Bones { get; set; } = false;
        public bool HealthBar { get; set; } = true;
        public bool Box { get; set; } = true;
        public bool Tracer { get; set; } = false;
        public bool ViewLine { get; set; } = false;

        public Vector4 EnemyColor { get; set; } = new(0.25f, 0.65f, 1f, 0.92f);
        public Vector4 TeamColor { get; set; } = new(0.35f, 1f, 0.55f, 0.82f);
        public Vector4 BoxColor { get; set; } = new(0.25f, 0.65f, 1f, 0.92f);
        public Vector4 BoneColor { get; set; } = new(0.92f, 0.96f, 1f, 0.92f);
        public Vector4 NameColor { get; set; } = new(0.92f, 0.96f, 1f, 0.95f);
        public Vector4 TracerColor { get; set; } = new(1f, 1f, 1f, 0.62f);
        public Vector4 ViewLineColor { get; set; } = new(1f, 1f, 1f, 0.82f);
        public Vector4 HealthBarColor { get; set; } = new(0.25f, 1f, 0.45f, 0.95f);
    }

    public sealed class FovChangerSettings : BaseSetting
    {
        public FovChangerSettings() : base(enabled: false){}
        public float Fov { get; set; } = 50f;
    }

    public sealed class Antibang : BaseSetting
    {
        public Antibang() : base(enabled: false){}
    }

    public sealed class MenuSettings
    {
        public int PanicKey { get; set; } = 0x23;
        public bool Open { get; set; } = true;
    }
}
