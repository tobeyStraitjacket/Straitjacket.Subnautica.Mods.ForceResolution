using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Tobey.ForceResolution.Patches;
internal static class DevConsolePatch
{
    private static Color commandColor = new(1, 4.67f, 0);
    private static Color requiredColor = Color.red;
    private static Color optionalColor = Color.yellow;

    private static string GetNames(Type enumType) => Enum.GetNames(enumType)
                                                        .Select(name => $"\"{name}\"")
                                                        .Join(delimiter: " | ");

    private const string forceresCommand = "forceres";
    private const string forceres_serviceCommand = "forceres.service";
    private const string forceres_saveCommand = "forceres.save";
    private const string forceres_applyCommand = "forceres.apply";

    private static Lazy<string> forceresUsage = new(() => $"{Colorise(forceresCommand, commandColor)} command expects parameters:\n" +
                                                            $"  {Colorise("width", requiredColor)} [number]\n" +
                                                            $"  {Colorise("height", requiredColor)} [number]\n" +
                                                            $"  {Colorise("fullscreenMode", optionalColor)} [optional: {GetNames(typeof(Config.FullscreenMode))}]\n" +
                                                            $"  {Colorise("refreshRate", optionalColor)} [optional: number]");

    private static Lazy<string> forceres_serviceUsage = new(() => $"{Colorise(forceres_serviceCommand, commandColor)} command expects parameters:\n" +
                                                            $"  {Colorise("serviceMode", requiredColor)} [{GetNames(typeof(Config.ServiceMode))}]");

    [HarmonyPatch(typeof(DevConsole), "Submit")]
    [HarmonyPrefix]
    public static bool SubmitPrefix(string value, out bool __result)
    {
        var parts = GetParts(value);

        (var command, __result) = GetCommand(parts)?.ToLowerInvariant() switch
        {
            forceresCommand => (HandleSetResolutionCommand, true),
            forceres_serviceCommand => (HandleSetServiceCommand, true),
            forceres_saveCommand => ((_) => ForceResolution.Instance.SaveResolutionCommand(), true),
            forceres_applyCommand => ((_) => ForceResolution.Instance.ApplySavedResolutionCommand(), true),

            _ => ((Action<IEnumerable<string>>)((_) => { }), false)
        };

        command.Invoke(GetParameters(parts));

        return !__result; // whether the original method should run or not is the inverse of whether we have handled the command
    }

    private static IEnumerable<string> GetParts(string command) => string.IsNullOrWhiteSpace(command) switch
    {
        true => Enumerable.Empty<string>(),
        false => command.Trim().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
    };

    private static string GetCommand(IEnumerable<string> parts) => parts.FirstOrDefault();
    private static IEnumerable<string> GetParameters(IEnumerable<string> parts) => parts.Skip(1);

    private static void HandleSetResolutionCommand(IEnumerable<string> parameters)
    {
        // required params
        if (parameters.ElementAtOrDefault(0) is not string widthStr || !double.TryParse(widthStr, out var width)
            || parameters.ElementAtOrDefault(1) is not string heightStr || !double.TryParse(heightStr, out var height))
        {   // required params not set or cannot be parsed to required types
            HandleError(forceresUsage.Value);
            return;
        }

        // validate optional fullscreenMode param
        string fullscreenModeStr = parameters.ElementAtOrDefault(2);
        bool fullscreenModeValid = Enum.TryParse<Config.FullscreenMode>(fullscreenModeStr, true, out var fullscreenMode);
        if (fullscreenModeStr is not null && !fullscreenModeValid)
        {   // fullscreenMode is set but cannot be parsed as Config.FullscreenMode
            HandleError(forceresUsage.Value);
            return;
        }

        // validate optional refreshRate param
        string refreshRateStr = parameters.ElementAtOrDefault(3);
        bool refreshRateValid = double.TryParse(refreshRateStr, out var refreshRate);
        if (refreshRateStr is not null && !refreshRateValid)
        {   // refreshRate is set but cannot be parsed as number
            HandleError(forceresUsage.Value);
            return;
        }

        ForceResolution.Instance.SetResolutionCommand(width: Convert.ToInt32(width),
                                                      height: Convert.ToInt32(height),
                                                      fullscreenMode: fullscreenModeStr switch
                                                      {
                                                          not null when fullscreenModeValid => fullscreenMode,
                                                          _ => null
                                                      },
                                                      refreshRate: refreshRateStr switch
                                                      {
                                                          not null when refreshRateValid => Convert.ToInt32(refreshRate),
                                                          _ => null
                                                      });
    }

    private static void HandleSetServiceCommand(IEnumerable<string> parameters)
    {
        // required params
        if (parameters.ElementAtOrDefault(0) is not string serviceModeStr 
            || !Enum.TryParse<Config.ServiceMode>(serviceModeStr, true, out var serviceMode))
        {   // required params not set or cannot be parsed to required types
            HandleError(forceres_serviceUsage.Value);
            return;
        }

        ForceResolution.Instance.SetServiceCommand(serviceMode);
    }

    private static void HandleError(string message)
    {
        ForceResolution.Log.LogError(message.StripXML());
        ErrorMessage.AddError(message);
    }

    private static string Colorise(string str, Color color) => $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{str}</color>";
    private static Lazy<Regex> xmlRegex = new(() => new("<.*?>", RegexOptions.Compiled));
    private static string StripXML(this string str) => xmlRegex.Value.Replace(str, string.Empty);
}
