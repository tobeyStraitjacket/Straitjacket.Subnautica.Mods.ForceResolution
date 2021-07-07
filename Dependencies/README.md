# Dependencies
This mod is game agnostic and at present has no game-specific build configurations, so you may use the assemblies from either game and it should be fine.

Place the following assemblies in this folder:
- `0Harmony.dll`, found in `[game path]\BepInEx\core`
- `Assembly-CSharp.dll`, found in `[game path]\Subnautica_Data\Managed` for Subnautica, or `[game path]\SubnauticaZero_Data\Managed` for Subnautica: Below Zero
- `Assembly-CSharp-firstpass.dll`, found in `[game path]\Subnautica_Data\Managed` for Subnautica, or `[game path]\SubnauticaZero_Data\Managed` for Subnautica: Below Zero
- `BepInEx.dll`, found in `[game path]\BepInEx\core`
- `QModInstaller.dll`, found in `[game path]\BepInEx\plugins\QModManager`
- `SMLHelper.dll`, found in `[game path]\QMods\Modding Helper` for Subnautica, or `[game path]\QMods\SMLHelper_BZ` for Subnautica: Below Zero
- `UnityEngine.CoreModule.dll`, found in `[game path]\Subnautica_Data\Managed` for Subnautica, or `[game path]\SubnauticaZero_Data\Managed` for Subnautica: Below Zero
- `UnityEngine.dll`, found in `[game path]\Subnautica_Data\Managed` for Subnautica, or `[game path]\SubnauticaZero_Data\Managed` for Subnautica: Below Zero

Feel free to also copy the `*.xml` XMLDocs for each assembly into this folder as well so you can get IDE hinting - the `.gitignore` will exclude them from commits.