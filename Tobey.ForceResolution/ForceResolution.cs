using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

namespace Tobey.ForceResolution;

using Patches;
using UnityEngine.SceneManagement;
using static Config;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class ForceResolution : BaseUnityPlugin
{
    public static ForceResolution Instance { get; private set; }
    internal static ManualLogSource Log => Instance.Logger;

    public Harmony Harmony { get; } = new Harmony(PluginInfo.PLUGIN_GUID);

    private void Awake()
    {
        // enforce singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
            return;
        }
    }

    private void OnEnable()
    {
        InitTomlTypeConverters();
        ResolutionServiceMode_SettingChanged(this, EventArgs.Empty);
        Bind();
        Harmony.PatchAll(typeof(DevConsolePatch));
    }

    private void InitTomlTypeConverters()
    {
        if (!TomlTypeConverter.GetSupportedTypes().Contains(typeof(Resolution)))
        {
            TomlTypeConverter.AddConverter(typeof(Resolution), new()
            {
                ConvertToString = (obj, _) => ((Resolution)obj).ToString(),
                ConvertToObject = (string str, Type _) =>
                {
                    try
                    {
                        var parts = str.Remove(str.Length - 2).Split(new[] { ' ', 'x', '@' }, StringSplitOptions.RemoveEmptyEntries);
                        return new Resolution
                        {
                            width = int.Parse(parts.ElementAt(0)),
                            height = int.Parse(parts.ElementAt(1)),
                            refreshRate = int.Parse(parts.ElementAt(2))
                        };
                    }
                    catch (Exception e)
                    {
                        Logger.LogError($"Failed to read resolution from settings: {e.Message}");
                        return default;
                    }
                }
            });
        }
    }

    private void Bind() => General.ResolutionServiceMode.SettingChanged += ResolutionServiceMode_SettingChanged;

    private void ResolutionServiceMode_SettingChanged(object _, EventArgs __)
    {
        switch (General.ResolutionServiceMode.Value)
        {
            case ServiceMode.AlwaysOn:
                ResolutionService.Instance.gameObject.EnsureComponent<SceneCleanerPreserve>();
                DontDestroyOnLoad(ResolutionService.Instance.gameObject);
                break;
            case ServiceMode.MainMenuOnly when FindObjectOfType<Player>() == null:
                foreach (var preserver in ResolutionService.Instance.GetComponents<SceneCleanerPreserve>().ToList())
                {
                    Destroy(preserver);
                }
                SceneManager.MoveGameObjectToScene(ResolutionService.Instance.gameObject, SceneManager.GetActiveScene());
                break;
            case ServiceMode.MainMenuOnly:
            case ServiceMode.Off:
                Destroy(ResolutionService.Instance.gameObject);
                break;
        }
    }

    private void Unbind() => General.ResolutionServiceMode.SettingChanged += ResolutionServiceMode_SettingChanged;

    private void OnDisable()
    {
        Unbind();
        Harmony.UnpatchSelf();
        Destroy(ResolutionService.Instance.gameObject);
    }

    public void SetResolution(Resolution resolution, Config.FullscreenMode fullscreenMode)
    {
        if (resolution.Equals(default(Resolution)))
        {
            return;
        }

        FullScreenMode mode = (FullScreenMode)fullscreenMode;

        if (Screen.fullScreenMode != mode
            || Screen.currentResolution.width != resolution.width
            || Screen.currentResolution.height != resolution.height
            || Screen.currentResolution.refreshRate != resolution.refreshRate)
        {
            Screen.SetResolution(width: resolution.width,
                                 height: resolution.height,
                                 preferredRefreshRate: resolution.refreshRate,
                                 fullscreenMode: mode);
        }
    }

    public void SetResolutionCommand(int width, int height, Config.FullscreenMode? fullscreenMode = null, int? refreshRate = null)
    {
        Resolution resolution = new()
        {
            width = width,
            height = height,
            refreshRate = !refreshRate.HasValue switch
            {
                true when General.DesiredResolution.Value.refreshRate == 0 => Screen.currentResolution.refreshRate,
                true => General.DesiredResolution.Value.refreshRate,
                false => refreshRate.Value
            }
        };

        General.DesiredResolution.Value = resolution;

        if (fullscreenMode.HasValue)
        {
            General.DesiredFullscreenMode.Value = fullscreenMode.Value;
        }

        ApplySavedResolutionCommand();
    }

    public void SetServiceCommand(ServiceMode serviceMode)
    {
        General.ResolutionServiceMode.Value = serviceMode;
        string message = $"Set service mode: {General.ResolutionServiceMode.Value}";
        Logger.LogInfo(message);
        ErrorMessage.AddMessage(message);
    }

    public void SaveResolutionCommand()
    {
        string message = $"Saving resolution: {Screen.currentResolution}";
        Logger.LogInfo(message);
        ErrorMessage.AddMessage(message);
        General.DesiredResolution.Value = Screen.currentResolution;
    }

    public void ApplySavedResolutionCommand()
    {
        SetResolution(General.DesiredResolution.Value, General.DesiredFullscreenMode.Value);
        string message = $"Set resolution: {General.DesiredResolution.Value}, {General.DesiredFullscreenMode.Value}";
        Logger.LogInfo(message);
        ErrorMessage.AddMessage(message);
    }
}
