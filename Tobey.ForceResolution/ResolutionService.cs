using System.Collections;
using UnityEngine;

namespace Tobey.ForceResolution;

using static Config;

[DisallowMultipleComponent]
public class ResolutionService : MonoBehaviour
{
    private static ResolutionService instance;
    internal static ResolutionService Instance => (instance == null) switch
    {
        true => instance = new GameObject("ResolutionService").EnsureComponent<ResolutionService>(),
        false => instance
    };

    private IEnumerator Start()
    {
        while (isActiveAndEnabled)
        {
            yield return new WaitForSecondsRealtime(.25f);
            if (Application.isFocused)
            {
                ForceResolution.Instance.SetResolution(General.DesiredResolution.Value, General.DesiredFullscreenMode.Value);
            }

            if (General.ResolutionServiceMode.Value == ServiceMode.MainMenuOnly && FindObjectOfType<Player>() != null)
            {
                break;
            }
        }

        Destroy(gameObject);
    }
}
