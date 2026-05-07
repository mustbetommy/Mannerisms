using Dalamud.Game.Chat;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ECommons;
using ECommons.Automation;
using ECommons.DalamudServices;
using Mannerisms.Data;
using Mannerisms.Gui;
using Mannerisms.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dalamud.Bindings.ImGui;
using JetBrains.Annotations;

namespace Mannerisms;

[UsedImplicitly]
public partial class Plugin : IDalamudPlugin
{
    internal static bool IsDebug = false;
    public const string Name = "Mannerisms";
    internal const string Version = "1.0.0";

    private bool _isInitialized;
    public PluginConfig Config { get; }
    public CharacterData? CurrentCharacterData;

    private readonly ConfigWindow _configWindow;
    private readonly EmotePickerWindow _emotePickerWindow;

    private readonly Queue<GestureBase> _emoteQueue = new();
    public readonly EmoteQueueRuntime EmoteQueue = new();

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<PluginService>();
        ECommonsMain.Init(pluginInterface, this);

        Config = pluginInterface.GetPluginConfig() as PluginConfig ?? new PluginConfig();

        var windowSystem = new WindowSystem(Assembly.GetExecutingAssembly().FullName);
        _configWindow = new ConfigWindow(this);
        windowSystem.AddWindow(_configWindow);
        _emotePickerWindow = new EmotePickerWindow(this);
        windowSystem.AddWindow(_emotePickerWindow);

        pluginInterface.UiBuilder.Draw += windowSystem.Draw;
        pluginInterface.UiBuilder.OpenConfigUi += () => OnCommand(string.Empty, string.Empty);

        SetupHandlers();

        Svc.Chat.ChatMessage += OnChatMessage;
        Svc.Framework.Update += OnFirstFrameworkUpdate;
        Svc.Framework.Update += OnFrameworkUpdate;
        Svc.ClientState.Login += OnLogin;
        Svc.ClientState.Logout += OnLogout;

        Svc.Log.Info("Mannerisms plugin initialized.");

        #if DEBUG
        IsDebug = true;
        #endif
    }

    public void ReloadCurrentCharacter()
    {
        Svc.Log.Debug("Reloading current character.");
        CharacterUtils.TryGetCurrent(out var name, out var world);

        if (Config.TryLoadCharacter(name, world, out var character))
        {
            CurrentCharacterData = character;
        } else
        {
            Svc.Log.Debug("Current character not found.");
            CurrentCharacterData = null;
        }
    }

    private void SetupHandlers()
    {
        PluginService.Commands.AddHandler("/manners", new CommandInfo(OnCommand)
        {
            HelpMessage = $"Open the {Name} config window",
            ShowInHelp = true
        });

        PluginService.Commands.AddHandler("/manners playidle {name}", new CommandInfo(OnCommand)
        {
            HelpMessage = "Play an idle profile by name",
            ShowInHelp = true
        });

        PluginService.Commands.AddHandler("/manners stopidle", new CommandInfo(OnCommand)
        {
            HelpMessage = "Stop the current idle profile",
            ShowInHelp = true
        });

        PluginService.Commands.AddHandler("/manners idles", new CommandInfo(OnCommand)
        {
            HelpMessage = "Toggle the quick access window",
            ShowInHelp = true
        });
    }

    public void Dispose()
    {
        ECommonsMain.Dispose();

        Svc.Chat.ChatMessage -= OnChatMessage;
        Svc.Framework.Update -= OnFirstFrameworkUpdate;
        Svc.Framework.Update -= OnFrameworkUpdate;
        Svc.ClientState.Login -= OnLogin;
        Svc.ClientState.Logout -= OnLogout;

        PluginService.PluginInterface.SavePluginConfig(Config);
    }

    public void OpenConfig()
    {
        _configWindow.IsOpen = true;
    }

    private void OnCommand(string command, string args)
    {
        _configWindow.IsOpen = !_configWindow.IsOpen;
    }

    private void OnLogin()
    {
        _isInitialized = false;
        PluginService.Framework.Update += OnFirstFrameworkUpdate;
    }

    private void OnLogout(int type, int code)
    {
        CurrentCharacterData = null;
    }

    private void OnFirstFrameworkUpdate(IFramework framework)
    {
        if (_isInitialized) return;
        if (PluginService.Objects.LocalPlayer == null) return;

        _isInitialized = true;
        PluginService.Framework.Update -= OnFirstFrameworkUpdate;

        ReloadCurrentCharacter();
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        if (_emoteQueue.Count > 0)
        {
            try
            {
                var gesture = _emoteQueue.Dequeue();
                if (!gesture.Command.IsNullOrEmpty())
                {
                    Svc.Log.Debug($"Enqueuing emote: '{gesture.Command}'");
                    EmoteQueue.TryAddEmote(gesture.Command, (float)framework.UpdateDelta.TotalSeconds + Config.SuggestionTimeout, "");
                }
            }
            catch (Exception e)
            {
                Svc.Log.Error($"Failed to enqueue emote: ${e}: ${e.Message}");
            }
            
        }

        _emotePickerWindow.IsOpen = Config.KeepSuggestionsOpen || EmoteQueue.Emotes.Count > 0;
        
        if (_emotePickerWindow.IsOpen)
        {
            EmoteQueue.UpdateTimers((float)framework.UpdateDelta.TotalSeconds);
            
            // Possibly accept the top suggestion.
            if (Config.AcceptSuggestionKeybind.IsPressed(true) && EmoteQueue.Emotes.Count > 0)
            {
                var top = EmoteQueue.Emotes[0];
                Chat.ExecuteCommand($"/{top.Emote.Command.TrimStart('/')} motion");
                EmoteQueue.Emotes.RemoveAt(0);
            }
        
            // Possibly dismiss the top suggestion.
            try
            {
                if (Config.DismissSuggestionKeybind.IsPressed(true) && EmoteQueue.Emotes.Count > 0)
                {
                    EmoteQueue.Emotes.RemoveAt(0);
                }
            }
            catch (Exception e)
            {
                Svc.Log.Error($"Error with keybinds: {e} : {e.Message}");
            }
        }
    }

    private void OnChatMessage(IHandleableChatMessage message)
    {
        if (!CharacterUtils.IsAvailable() || CurrentCharacterData == null) return;

        if (
            message.LogKind != XivChatType.Say &&
            message.LogKind != XivChatType.TellOutgoing &&
            message.LogKind != XivChatType.Party &&
            message.LogKind != XivChatType.Alliance)
        {
            return;
        }

        var sanitizedSenderName = CharacterUtils.SanitizeName(message.Sender.TextValue);

        // Bail early if the message was not sent by the player
        if (
            sanitizedSenderName != ECommons.GameHelpers.Player.Name &&
            message.LogKind != XivChatType.TellOutgoing)
        {
            return;
        }
        
        // Process common gestures
        foreach (var gesture in CurrentCharacterData.CommonGestures.Where(
                     gesture => gesture.Value.IsMatch(message, sanitizedSenderName)))
        {
            _emoteQueue.Enqueue(gesture.Value);
        }

        // Process simple and advanced gestures
        foreach (var gesture in CurrentCharacterData.SimpleGestures.Where(
                     gesture => gesture.IsMatch(message, sanitizedSenderName)))
        {
            _emoteQueue.Enqueue(gesture);
        }
        
        foreach (var gesture in CurrentCharacterData.AdvancedGestures.Where(
                     gesture => gesture.IsMatch(message, sanitizedSenderName)))
        {
            _emoteQueue.Enqueue(gesture);
        }
    }
}
