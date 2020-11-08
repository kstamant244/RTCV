namespace RTCV.CorruptCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using RTCV.NetCore;
    using ScriptCore.Attributes;

    /// <summary>
    /// UI Side
    /// </summary>
    static class ScriptContext
    {
        //TODO: read as setting
        const int MAX_SAVESTATES = 20;

        //const string savestateDir = workingDir
        static Dictionary<string, StashKey> savestates = new Dictionary<string, StashKey>();
        public static void Reset()
        {
            foreach (var savestate in savestates.Values)
            {
                //Delete savestate file ? idk
            }
            savestates.Clear();
        }

        [LuaCallback("CreateSavestate")]
        public static void CreateSavestate(string name)
        {
            bool UseSavestates = (bool)AllSpec.VanguardSpec[VSPEC.SUPPORTS_SAVESTATES];
            if (UseSavestates)
            {
                StashKey sk = null;
                void a()
                {
                    sk = StockpileManagerEmuSide.SaveStateNET(null);
                }
                SyncObjectSingleton.EmuThreadExecute(a, false);
                if (savestates.ContainsKey(name) || savestates.Count < MAX_SAVESTATES)
                {
                    savestates[name] = sk;
                }
                else
                {
                    throw new Exception($"Cannot save more than {MAX_SAVESTATES} savestates");
                }
            }
            else
            {
                //Throw error
            }
        }

        [LuaCallback("LoadSavestate")]
        public static void LoadSavestate(string name)
        {
            if (savestates.TryGetValue(name, out StashKey psk))
            {
                LocalNetCoreRouter.Route(NetCore.Endpoints.CorruptCore, NetCore.Commands.Remote.LoadState, new object[] { psk, true, true }, true);
            }
            else
            {
                throw new Exception("savestate name does not exist");
            }
        }

    }
}
