using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using ClickableTransparentOverlay;
using CS2.Core;
using CS2.Enums;
using CS2.Modules.Combat;
using CS2.Modules.Visual;
using CS2.UI;
using ImGuiNET;


namespace CS2
{
    public class Renderer : Overlay
    {
        private static readonly FieldInfo? OverlayWindowField = typeof(Overlay).GetField("window", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly FieldInfo? OverlayWindowHandleField = OverlayWindowField?.FieldType.GetField("Handle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private readonly Menu _menu;
        private bool _menuOpen = true;
        private bool _focusMenuNextFrame;
        private bool _lastInteractiveState;
        private Vector2 _activeListPosition = new(1570, 40);
        private Vector2 _activeListSize;
        private bool _activeListVisibleThisFrame;
        private int _enableHotkey;
        private int _noRecoilHotkey;
        private int _showFovHotkey;
        private int _saveConfigHotkey;
        private string _configName = "default";
        private string _lastAction = "Ready";
        public readonly GameContext _game = new();
        public readonly AimBot _aimFeature;
        public readonly NoRecoil _noRecoilFeature;
        public readonly AntiAim _antiAimFeature;
        public readonly TriggerBot _triggetFeature;
        public readonly Fovchanger _fovFeature;
        public readonly AntiBang _antibangFeature;
        public readonly ESP _espFeature;
        public Renderer()
        {
            _menuOpen = _game.Settings.Menu.Open;
            _aimFeature = new AimBot(_game);
            _noRecoilFeature = new NoRecoil(_game, _aimFeature);
            _antiAimFeature = new AntiAim(_game);
            _triggetFeature = new TriggerBot(_game);
            _fovFeature = new Fovchanger(_game);
            _antibangFeature = new AntiBang(_game);
            _espFeature = new ESP(_game);
            _menu = new Menu("Strike", new Vector2(1920 / 4, 1080 / 4), new Vector2(700, 460));

            MenuPage combat = _menu.Page("Combat", "Combat");
            MenuTab aimTab = combat.Tab("Aim");
            combat.Tab("Aimbot")
                .Group("Aimbot", 0)
                .Toggle("Enable", () => _game.Settings.Combat.AimBot.Enabled, value => _game.Settings.Combat.AimBot.Enabled = value, "Ativa ou desativa este modulo. Clique com o botao direito para vincular uma tecla.", () => _enableHotkey, value => _enableHotkey = value)
                .Keybind("Aimbot Keybind", () => _game.Settings.Combat.AimBot.Key, value => _game.Settings.Combat.AimBot.Key = value, () => { }, "Tecla para segurar enquanto o Aimbot esta habilitado.")
                .Toggle("Use FOV Target", () => _game.Settings.Combat.AimBot.FovEnabled, value => _game.Settings.Combat.AimBot.FovEnabled = value, "Prioriza o alvo mais perto do centro da tela.")
                .Toggle("Show Fov", () => _game.Settings.Visuals.ShowFov, value => _game.Settings.Visuals.ShowFov = value, "Mostra o circulo de FOV no overlay.", () => _showFovHotkey, value => _showFovHotkey = value)
                .SliderInt("Aimbot FOV", () => _game.Settings.Combat.AimBot.Fov, value => _game.Settings.Combat.AimBot.Fov = value, 0, 180, "Tamanho do campo usado como referencia visual.")
                .Slider("Smoothing", () => _game.Settings.Combat.AimBot.Smoothing, value => _game.Settings.Combat.AimBot.Smoothing = value, 1f, 50f, "0.000", "Suavizacao visual do ajuste.")
                .ComboEnum("Target Bone", () => _game.Settings.Combat.AimBot.TargetBone, value => _game.Settings.Combat.AimBot.TargetBone = value, "Bone usado como alvo do aimbot.")
                .Checkbox("Aim Team", () => _game.Settings.Combat.AimBot.AimTeam, value => _game.Settings.Combat.AimBot.AimTeam = value, "Filtro visual/funcional para sua integracao depois.");

            aimTab
                .Group("Triggerbot", 0)
                .Toggle("Enabled", () => _game.Settings.Combat.TriggetBot.Enabled, value => _game.Settings.Combat.TriggetBot.Enabled = value)
                .Checkbox("Shoot Team", () => _game.Settings.Combat.TriggetBot.ShootTeam, value => _game.Settings.Combat.TriggetBot.ShootTeam = value, "Atirar no próprio time.");

            aimTab
                .Group("No Recoil", 1)
                .Toggle(
                    "Enabled",
                    () => _game.Settings.Combat.NoRecoil.Enabled,
                    value => _game.Settings.Combat.NoRecoil.Enabled = value,
                    "Compensa o recuo da arma. Clique com o botao direito para vincular uma tecla.",
                    () => _noRecoilHotkey,
                    value => _noRecoilHotkey = value
                )
                .Slider(
                    "Strength",
                    () => _game.Settings.Combat.NoRecoil.Strength,
                    value => _game.Settings.Combat.NoRecoil.Strength = value,
                    0f,
                    2f,
                    "0.00",
                    "Intensidade da compensacao. 1.00 corresponde ao punch visual completo."
                );

            combat.Tab("Friends")
                .Group("Friend List", 0)
                .Toggle("Show", () => _game.Settings.Visuals.FriendList, value => _game.Settings.Visuals.FriendList = value)
                .TextInput("Config Name", () => _configName, value => _configName = value, 48)
                .Button("Save Config", () => _lastAction = $"Saved {_configName}", "Executa uma acao simples. Clique com o botao direito para vincular uma tecla.", () => _saveConfigHotkey, value => _saveConfigHotkey = value);
            MenuPage visual = _menu.Page("Visual", "Visual");
            MenuTab camera = visual.Tab("Camera");

            camera.Group("Fov Changer")
                .Toggle("Enable", () => _game.Settings.Visuals.FovChanger.Enabled, value => _game.Settings.Visuals.FovChanger.Enabled = value)
                .Slider("FOV", () => _game.Settings.Visuals.FovChanger.Fov, value => _game.Settings.Visuals.FovChanger.Fov = value, 10f, 170f);

            camera.Group("Antibang").Toggle("Enable", () => _game.Settings.Visuals.Antibang.Enabled, value => _game.Settings.Visuals.Antibang.Enabled = value);

            visual.Tab("ESP")
                .Group("ESP", 0)
                .Toggle("Enable", () => _game.Settings.Visuals.Esp.Enabled, value => _game.Settings.Visuals.Esp.Enabled = value, "Mostra ou esconde o ESP.")
                .Toggle("Team", () => _game.Settings.Visuals.Esp.Team, value => _game.Settings.Visuals.Esp.Team = value, "Tambem desenha jogadores do seu time.")
                .Toggle("NameTags", () => _game.Settings.Visuals.Esp.NameTags, value => _game.Settings.Visuals.Esp.NameTags = value, "Mostra nome acima do jogador.")
                .Toggle("Bones", () => _game.Settings.Visuals.Esp.Bones, value => _game.Settings.Visuals.Esp.Bones = value, "Desenha o esqueleto.")
                .Toggle("Health Bar", () => _game.Settings.Visuals.Esp.HealthBar, value => _game.Settings.Visuals.Esp.HealthBar = value, "Mostra barra de vida.")
                .Toggle("Box", () => _game.Settings.Visuals.Esp.Box, value => _game.Settings.Visuals.Esp.Box = value, "Desenha caixa no jogador.")
                .Toggle("View Line", () => _game.Settings.Visuals.Esp.ViewLine, value => _game.Settings.Visuals.Esp.ViewLine = value, "Mostra uma pequena linha na direcao em que o jogador olha.")
                .Toggle("Tracer", () => _game.Settings.Visuals.Esp.Tracer, value => _game.Settings.Visuals.Esp.Tracer = value, "Desenha uma linha ate o jogador.");

            MenuTab espColorsTab = visual.Tab("ESP Colors");

            espColorsTab
                .Group("Relation", 0)
                .ColorEdit("Enemy", () => _game.Settings.Visuals.Esp.EnemyColor, value => _game.Settings.Visuals.Esp.EnemyColor = value)
                .ColorEdit("Team", () => _game.Settings.Visuals.Esp.TeamColor, value => _game.Settings.Visuals.Esp.TeamColor = value);

            espColorsTab
                .Group("Elements", 1)
                .ColorEdit("Box", () => _game.Settings.Visuals.Esp.BoxColor, value => _game.Settings.Visuals.Esp.BoxColor = value)
                .ColorEdit("Bones", () => _game.Settings.Visuals.Esp.BoneColor, value => _game.Settings.Visuals.Esp.BoneColor = value)
                .ColorEdit("NameTags", () => _game.Settings.Visuals.Esp.NameColor, value => _game.Settings.Visuals.Esp.NameColor = value)
                .ColorEdit("Health Bar", () => _game.Settings.Visuals.Esp.HealthBarColor, value => _game.Settings.Visuals.Esp.HealthBarColor = value)
                .ColorEdit("View Line", () => _game.Settings.Visuals.Esp.ViewLineColor, value => _game.Settings.Visuals.Esp.ViewLineColor = value)
                .ColorEdit("Tracer", () => _game.Settings.Visuals.Esp.TracerColor, value => _game.Settings.Visuals.Esp.TracerColor = value);

            _menu.Page("Players", "P")
                .Tab("Visuals")
                .Group("ESP", 0)
                .Toggle("Enabled", () => _game.Settings.Visuals.FriendList, value => _game.Settings.Visuals.FriendList = value)
                .Toggle("Active List", () => _game.Settings.Visuals.ActiveList, value => _game.Settings.Visuals.ActiveList = value, "Mostra a lista de opcoes habilitadas no overlay.")
                .Keybind("Panic Key", () => _game.Settings.Menu.PanicKey, value => _game.Settings.Menu.PanicKey = value, HideMenu, "Fecha o menu imediatamente.")
                .TextInput("Status", () => _lastAction, value => _lastAction = value, 64);

            _menu.Page("Vehicles", "V").Tab("Main").Group("Vehicle Options");
            _menu.Page("Weapon", "W").Tab("Main").Group("Weapon Options");
        }
        protected override void Render()
        {
            Input.Update();
            _game.SetScreenSize(ImGui.GetIO().DisplaySize);

            if (Input.IsPressed(_game.Settings.Menu.PanicKey))
                Environment.Exit(0);

            if (Input.IsPressed(0x2D))
            {
                _menuOpen = !_menuOpen;
                _game.Settings.Menu.Open = _menuOpen;
                _focusMenuNextFrame = _menuOpen;
            }

            SetOverlayInteractive(_menuOpen);

            _menu.UpdateHotkeys();

            DrawAimbotFovCircle();
            _espFeature.Update();
            DrawActiveList();

            if (_menuOpen)
            {
                _menu.Render(_focusMenuNextFrame);
                _focusMenuNextFrame = false;
            }

            if (_menuOpen && AnyMouseClicked() && !MouseInsideMenuOrActiveList())
                HideMenu();

            SetOverlayInteractive(_menuOpen);
        }

        private void DrawAimbotFovCircle()
        {
            if (!_game.Settings.Visuals.ShowFov)
                return;

            if (!_game.Settings.Combat.AimBot.FovEnabled)
                return;

            float radius = Math.Clamp(_game.Settings.Combat.AimBot.Fov, 1, 2000);
            Vector2 screenSize = _game.ScreenSize;
            if (screenSize.X <= 0 || screenSize.Y <= 0)
                return;

            Vector2 center = screenSize / 2f;
            ImDrawListPtr draw = ImGui.GetBackgroundDrawList();

            for (int i = 4; i >= 1; i--)
            {
                float spread = i * 2.5f;
                float alpha = 0.08f / i;
                draw.AddCircle(
                    center,
                    radius + spread,
                    Theme.Color(Theme.WithAlpha(Theme.Glow, alpha)),
                    160,
                    2.4f);
            }

            draw.AddCircle(center, radius, Theme.Color(Theme.WithAlpha(Theme.AccentHot, 0.88f)), 160, 1.8f);
            draw.AddCircle(center, radius + 1.5f, Theme.Color(Theme.WithAlpha(Theme.Accent, 0.28f)), 160, 1f);
        }

        private void DrawActiveList()
        {
            _activeListVisibleThisFrame = false;
            _activeListSize = Vector2.Zero;

            if (!_game.Settings.Visuals.ActiveList)
                return;

            List<string> activeItems = new();
            if (_game.Settings.Combat.AimBot.Enabled)
                activeItems.Add("Aimbot");
            if (_game.Settings.Combat.NoRecoil.Enabled)
                activeItems.Add("No Recoil");
            if (_game.Settings.Visuals.ShowFov)
                activeItems.Add("FOV");
            if (_game.Settings.Visuals.FriendList)
                activeItems.Add("Friend List");
            if (_game.Settings.Visuals.VisibleCheck)
                activeItems.Add("Visible Check");

            if (activeItems.Count == 0)
                return;

            Vector2 size = new(170, 32 + (activeItems.Count * 22));
            _activeListSize = size;
            _activeListVisibleThisFrame = true;

            ImGui.SetNextWindowPos(_activeListPosition);
            ImGui.SetNextWindowSize(size);
            ImGui.Begin("##active-list", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

            ImDrawListPtr draw = ImGui.GetWindowDrawList();
            Vector2 pos = ImGui.GetWindowPos();
            Vector2 end = pos + size;
            Theme.AddGlow(draw, pos, end, Theme.Glow, Theme.Rounding, 0.35f);
            draw.AddRectFilled(pos, end, Theme.Color(Theme.WithAlpha(Theme.Window, 0.88f)), Theme.Rounding);
            draw.AddRect(pos, end, Theme.Color(Theme.Border), Theme.Rounding);
            draw.AddRectFilled(pos, pos + new Vector2(size.X, 26), Theme.Color(Theme.AccentDim), Theme.Rounding, ImDrawFlags.RoundCornersTop);
            draw.AddText(pos + new Vector2(10, 6), Theme.Color(Theme.Text), "Enabled");

            for (int i = 0; i < activeItems.Count; i++)
            {
                Vector2 itemPos = pos + new Vector2(12, 34 + (i * 22));
                draw.AddCircleFilled(itemPos + new Vector2(3, 7), 3f, Theme.Color(Theme.AccentHot));
                draw.AddText(itemPos + new Vector2(14, 0), Theme.Color(Theme.TextMuted), activeItems[i]);
            }

            ImGui.SetCursorScreenPos(pos);
            ImGui.InvisibleButton("##active-list-drag", size);
            if (ImGui.IsItemActive() && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
                _activeListPosition += ImGui.GetIO().MouseDelta;

            ImGui.End();
        }

        private void HideMenu()
        {
            _menuOpen = false;
            _game.Settings.Menu.Open = false;
            _focusMenuNextFrame = false;
            _menu.CancelCapture();
            SetOverlayInteractive(false);
        }

        private void SetOverlayInteractive(bool interactive)
        {
            ImGui.GetIO().MouseDrawCursor = interactive;

            IntPtr overlayHandle = GetOverlayHandle();
            if (overlayHandle == IntPtr.Zero)
                return;

            int style = Win32.GetWindowLong(overlayHandle, Win32.GWL_EXSTYLE);
            int nextStyle = style | Win32.WS_EX_LAYERED | Win32.WS_EX_TOPMOST;

            if (interactive)
                nextStyle &= ~Win32.WS_EX_TRANSPARENT;
            else
                nextStyle |= Win32.WS_EX_TRANSPARENT;

            if (nextStyle != style)
                Win32.SetWindowLong(overlayHandle, Win32.GWL_EXSTYLE, nextStyle);

            if (interactive && !_lastInteractiveState)
            {
                Win32.ClipCursor(IntPtr.Zero);
                Win32.SetForegroundWindow(overlayHandle);
                Win32.ShowCursor(true);
            }

            _lastInteractiveState = interactive;
        }

        private IntPtr GetOverlayHandle()
        {
            object? overlayWindow = OverlayWindowField?.GetValue(this);
            object? handle = overlayWindow == null ? null : OverlayWindowHandleField?.GetValue(overlayWindow);

            return handle is IntPtr ptr ? ptr : IntPtr.Zero;
        }

        private bool AnyMouseClicked()
        {
            return ImGui.IsMouseClicked(ImGuiMouseButton.Left) ||
                   ImGui.IsMouseClicked(ImGuiMouseButton.Right) ||
                   ImGui.IsMouseClicked(ImGuiMouseButton.Middle);
        }

        private bool MouseInsideMenuOrActiveList()
        {
            Vector2 mouse = ImGui.GetIO().MousePos;

            if (_menu.Contains(mouse))
                return true;

            return _activeListVisibleThisFrame &&
                   mouse.X >= _activeListPosition.X &&
                   mouse.X <= _activeListPosition.X + _activeListSize.X &&
                   mouse.Y >= _activeListPosition.Y &&
                   mouse.Y <= _activeListPosition.Y + _activeListSize.Y;
        }
       

    }
}
