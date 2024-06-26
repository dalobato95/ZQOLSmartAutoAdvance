using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using SmartAutoAdvance.Windows;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Party;
using Dalamud.Plugin.Services;

namespace SmartAutoAdvance
{
    public sealed class SmartAutoAdvancePlugin : IDalamudPlugin
    {
        public string Name => "Smart text auto-advance";
        private const string ShortCommandName = "/staa";
        private const string LongCommandName = "/smarttextautoadvance";

        private DalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        private CommandInfo CommandInfo { get; init; }
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("SmartAutoAdvancePlugin");

        private ConfigWindow ConfigWindow { get; init; }

        [PluginService]
        public SigScanner SigScanner { get; init; } = null!;

        [PluginService]
        public ICondition Condition { get; init; } = null!;

        [PluginService]
        public IPartyList PartyList { get; init; } = null!;

        public Listener Listener { get; }

        public SmartAutoAdvancePlugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ICommandManager commandManager,
            [RequiredVersion("1.0")] SigScanner sigScanner,
            [RequiredVersion("1.0")] ICondition condition,
            [RequiredVersion("1.0")] IPartyList partyList)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
            this.SigScanner = sigScanner;
            this.Condition = condition;
            this.PartyList = partyList;

            // common CommandInfo for all aliases
            this.CommandInfo = new CommandInfo(OnCommand)
            {
                HelpMessage = "Opens the Smart Text Auto-Advance config window.\nUse /staa toggle to manually toggle Auto-Advance"
            };

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            this.ConfigWindow = new ConfigWindow(this);

            this.WindowSystem.AddWindow(this.ConfigWindow);

            this.Listener = new Listener(this);
            if (this.Configuration.Enabled)
            {
                this.Listener.Enable();
            }

            // command aliases
            this.CommandManager.AddHandler(ShortCommandName, this.CommandInfo);
            this.CommandManager.AddHandler(LongCommandName, this.CommandInfo);

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();
            
            this.ConfigWindow.Dispose();
            
            this.CommandManager.RemoveHandler(ShortCommandName);
            this.CommandManager.RemoveHandler(LongCommandName);

            this.PluginInterface.UiBuilder.Draw -= this.DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi -=  DrawConfigUI;

            this.Listener.Dispose();
        }

        private void OnCommand(string command, string args)
        {
            // Toggle config window when no args are used
            if (args == "")
            {
                this.ConfigWindow.IsOpen = !this.ConfigWindow.IsOpen;
            }

            var subCommand = args.ToLower();

            if (subCommand == "toggle")
            {
                this.Listener.ToggleAutoAdvance();
            }

            else if (subCommand == "log")
            {
                this.Listener.LogAutoAdvance();
            }
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            this.ConfigWindow.IsOpen = !this.ConfigWindow.IsOpen;
        }
    }
}   
