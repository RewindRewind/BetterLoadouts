using System;
using System.Diagnostics;
using BepInEx;
using BepInEx.Configuration;
using SPT.Reflection.Patching;

namespace BetterLoadouts;

[BepInPlugin("io.cebarks.betterloadouts", "Better Loadouts", "1.0.0")]
[BepInDependency("com.SPT.custom", "4.0.0")]
public class Plugin : BaseUnityPlugin
{
    // public const int TarkovVersion = 40087;
    public const int TarkovVersion = 40743;
    internal static ConfigEntry<bool> DebugLogging { get; private set; } = null!;

    internal void Awake()
    {
        DebugLogging = Config.Bind("Logging", "DebugLogging", false,
            "Enable logging.");

        var current = FileVersionInfo.GetVersionInfo(BepInEx.Paths.ExecutablePath).FilePrivatePart;
        if (current != TarkovVersion)
            throw new Exception($"BetterLoadouts: built for Tarkov {TarkovVersion}, running {current}");

        DontDestroyOnLoad(this);

        new PatchManager(this, true).EnablePatches();
    }
}
