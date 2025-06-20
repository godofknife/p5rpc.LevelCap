using p5rpc.lib.interfaces;
using p5rpc.LevelCap.Configuration;
using p5rpc.LevelCap.Template;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.X64;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;
using p5rpc.levelCap;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace p5rpc.LevelCap
{
    /// <summary>
    /// Your mod logic goes here.
    /// </summary>
    public class Mod : ModBase // <= Do not Remove.
    {
        private IP5RLib _p5rLib;
        private IAsmHook _levelcapjoker;
        private IAsmHook _levelcappartymembers;
        /// <summary>
        /// Provides access to the mod loader API.
        /// </summary>
        private readonly IModLoader _modLoader;

        /// <summary>
        /// Provides access to the Reloaded.Hooks API.
        /// </summary>
        /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
        private readonly IReloadedHooks? _hooks;

        /// <summary>
        /// Provides access to the Reloaded logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Entry point into the mod, instance that created this class.
        /// </summary>
        private readonly IMod _owner;

        /// <summary>
        /// Provides access to this mod's configuration.
        /// </summary>
        private Config _configuration;

        /// <summary>
        /// The configuration of the currently executing mod.
        /// </summary>
        private readonly IModConfig _modConfig;

        public Mod(ModContext context)
        {
            //Debugger.Launch();
            _modLoader = context.ModLoader;
            _hooks = context.Hooks;
            _logger = context.Logger;
            _owner = context.Owner;
            _configuration = context.Configuration;
            _modConfig = context.ModConfig;

            Utils.Initialise(_logger, _configuration);
            var startupScannerController = _modLoader.GetController<IStartupScanner>();
            if (startupScannerController == null || !startupScannerController.TryGetTarget(out var startupScanner))
            {
                Utils.LogError($"Unable to get controller for Reloaded SigScan Library, aborting initialisation");
                return;
            }

            var p5rLibController = _modLoader.GetController<IP5RLib>();
            if (p5rLibController == null || !p5rLibController.TryGetTarget(out _p5rLib))
            {
                Utils.LogError($"Unable to get controller for P5R Lib, aborting initialisation");
                return;
            }

            string shouldCapCall = _hooks.Utilities.GetAbsoluteCallMnemonics(testingcaplevel, out _testingcaplevelReverseWrapper);
            startupScanner.AddMainModuleScan("03 59 ?? 48 8D 15 ?? ?? ?? ?? B9 01 00 00 00", (result) =>
            {
                
                if (!result.Found)
                {
                    Utils.LogError("Unable to find Joker's Level Cap.");
                    return;
                }
                var levelAddress = Utils.GetGlobalAddress((nuint)Utils.BaseAddress + (nuint)result.Offset - 7) - 4;
                Utils.LogDebug($"Found Joker Level Cap Function at 0x{result.Offset + Utils.BaseAddress:X}");


                string[] function =
                {
                    "use64",
                    "push rax",
                    $"{Utils.PushXmm(0)}\n{Utils.PushXmm(1)}\n{Utils.PushXmm(2)}\n{Utils.PushXmm(3)}",
                    "push r8 \npush r9 \npush r10 \npush r11",
                    "push rcx",
                    "push rdx",
                    $"mov rcx, [qword {_hooks.Utilities.WritePointer((nint)levelAddress)}]", // Write a pointer to the level since it's too far away
                    $"mov rcx, [rcx]", // Put Joker's level in rcx
                    "sub rsp, 40", // Make shadow space
                    $"{shouldCapCall}",
                    //$"{_hooks.Utilities.GetAbsoluteCallMnemonics(testingcaplevel, out _testingcaplevelReverseWrapper)}",
                    "add rsp, 40", // Restore stack                  
                    "pop rdx",                   
                    "pop rcx",
                    "pop r11 \npop r10 \npop r9 \npop r8",
                    $"{Utils.PopXmm(3)}\n{Utils.PopXmm(2)}\n{Utils.PopXmm(1)}\n{Utils.PopXmm(0)}",
                    "cmp rax, 0", // Check if we should cap
                    "pop rax",
                    "je endHook", // If we shouldn't cap skip the subtraction
                    "sub ebx, dword [rcx + 4]", // Remove the gained exp
                    "label endHook",
                };

                _levelcapjoker = _hooks.CreateAsmHook(function, result.Offset + Utils.BaseAddress, AsmHookBehaviour.ExecuteAfter).Activate();
            });
            startupScanner.AddMainModuleScan("48 C1 E2 04 46 01 54 ?? ??", (result) =>
            {
                if (!result.Found)
                {
                    Utils.LogError("Unable to find Party Members' Level Cap.");
                    return;
                }
                Utils.LogDebug($"Found Party Members Level Cap Function at 0x{result.Offset + Utils.BaseAddress:X}");
                string[] function =
                {
                    "use64",
                    "push r8 \npush r9 \npush r10 \npush r11",
                    "push rcx",
                    "push rax",
                    "push rdx",
                    "mov rcx, [rdx+r8+0x48]", // Move the party member's level into rcx
                    "sub rsp, 40", // Make shadow space
                    $"{shouldCapCall}",
                    "add rsp, 40", // Make shadow space
                    "cmp rax, 0", // Check if we should cap
                    "pop rdx",
                    "pop rax",
                    "pop rcx",
                    "pop r11 \npop r10 \npop r9 \npop r8",
                    "je endHook",
                    "mov r10d, 0", // Set the gained exp to 0
                    "label endHook"
                };
                _levelcappartymembers = _hooks.CreateAsmHook(function, result.Offset + Utils.BaseAddress, AsmHookBehaviour.ExecuteFirst).Activate();
            });
        }

        private bool testingcaplevel(short level)
        {
            //int var0 = _p5rLib.FlowCaller.SCRIPT_READ(1111, 11, 12);
            //_p5rLib.FlowCaller.SCRIPT_READ_SYNC(var0);
            //_p5rLib.FlowCaller.SCRIPT_EXEC(var0, 0);
            //_p5rLib.FlowCaller.SCRIPT_FREE(var0);
            int Kamoshidapalacelevel = _configuration.setthelevelcapKamoshida;
            int Madaramepalacelevel = _configuration.setthelevelcapMadarame;
            int Kaneshiropalacelevel = _configuration.setthelevelcapKaneshiro;
            int Futabapalacelevel = _configuration.setthelevelcapFutaba;
            int Okumurapalacelevel = _configuration.setthelevelcapOkumura;
            int Saepalacelevel = _configuration.setthelevelcapSae;
            int Shidopalacelevel = _configuration.setthelevelcapShido;
            int Depthpalacelevel = _configuration.setthelevelcapDepth;
            if (_p5rLib.FlowCaller.CHK_DAYS_STARTEND(4, 11, 5, 15) == 1)
            {
                Utils.Log("Kamoshida Date");
                if (level >= Kamoshidapalacelevel)
                {
                    return true;
                }
            }
            else if (_p5rLib.FlowCaller.CHK_DAYS_STARTEND(5, 0x10, 6, 19) == 1)
            {
                Utils.Log("Madarame Date");
                if (level >= Madaramepalacelevel)
                {
                    return true;
                }
            }
            else if (_p5rLib.FlowCaller.CHK_DAYS_STARTEND(6, 20, 7, 24) == 1)
            {
                Utils.Log("Kaneshiro Date");
                if (level >= Kaneshiropalacelevel)
                {
                    return true;
                }
            }
            else if (_p5rLib.FlowCaller.CHK_DAYS_STARTEND(7, 25, 9, 14) == 1)
            {
                Utils.Log("Futaba Date");
                if (level >= Futabapalacelevel)
                {
                    return true;
                }
            }
            else if (_p5rLib.FlowCaller.CHK_DAYS_STARTEND(9, 15, 10, 10) == 1)
            {
                Utils.Log("Okumura Date");
                if (level >= Okumurapalacelevel)
                {
                    return true;
                }
            }
            else if (_p5rLib.FlowCaller.CHK_DAYS_STARTEND(10, 11, 11, 23) == 1)
            {
                Utils.Log("Nijima Date");
                if (level >= Saepalacelevel)
                {
                    return true;
                }
            }
            else if (_p5rLib.FlowCaller.CHK_DAYS_STARTEND(11, 24, 12, 14) == 1)
            {
                Utils.Log("Shido Date");
                if (level >= Shidopalacelevel)
                {
                    return true;
                }
            }
            else if (_p5rLib.FlowCaller.CHK_DAYS_STARTEND(12, 15, 12, 24) == 1)
            {
                Utils.Log("Depth of Mementos Date");
                if (level >= Depthpalacelevel)
                {
                    return true;
                }
            }
            return false;
        }

        [Function(CallingConventions.Microsoft)]
        private delegate bool testingcaplevelDelegate(short level);
        private IReverseWrapper<testingcaplevelDelegate> _testingcaplevelReverseWrapper;
        #region Standard Overrides
        public override void ConfigurationUpdated(Config configuration)
        {
            // Apply settings from configuration.
            // ... your code here.
            _configuration = configuration;
            _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
        }
        #endregion

        #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Mod() { }
#pragma warning restore CS8618
        #endregion
    }
}