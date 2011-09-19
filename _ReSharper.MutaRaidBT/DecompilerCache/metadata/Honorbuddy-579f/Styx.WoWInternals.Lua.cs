// Type: Styx.WoWInternals.Lua
// Assembly: Honorbuddy, Version=2.0.0.5198, Culture=neutral, PublicKeyToken=50a565ab5c01ae50
// Assembly location: D:\Users\Public\Documents\HB Stuff\cc\rogue\MutaRaidBT\bin\Debug\calc.exe

using System;
using System.Collections.Generic;

namespace Styx.WoWInternals
{
    public static class Lua
    {
        public static LuaState State { get; }
        public static LuaEvents Events { get; }
        public static string Escape(string unescaped);
        public static void ShowLuaStack(uint pState);
        public static List<string> GetReturnValues(string lua);
        public static List<string> GetReturnValues(string lua, string scriptName = "lua.lua");

        [Obsolete("Use GetReturnValues instead. They do the same.")]
        public static List<string> LuaGetReturnValue(string lua, string scriptName);

        public static T GetReturnVal<T>(string lua, uint retVal);
        public static void DoString(string lua, string luaFile, uint pState);
        public static void DoString(string szLua, string szLuaFile);
        public static void DoString(string szLua);
        public static void DoString(string format, params object[] args);

        [Obsolete("GetLocalizedText is deprecated. Please use GetReturnValues instead.")]
        public static T GetLocalizedText<T>(string szLuaVariable);

        [Obsolete("GetLocalizedText is deprecated. Please use GetReturnValues instead.")]
        public static string GetLocalizedText(string szLuaVariable, uint lpLocalPlayer);

        [Obsolete("GetLocalizedText is deprecated. Please use GetReturnValues instead.")]
        public static string GetLocalizedText(string szLuaVariable);

        [Obsolete("GetLocalizedText is deprecated. Please use GetReturnValues instead.")]
        public static int GetLocalizedInt32(string szLuaVariable, uint lpLocalPlayer);

        [Obsolete("GetLocalizedText is deprecated. Please use GetReturnValues instead.")]
        public static uint GetLocalizedUInt32(string szLuaVariable, uint lpLocalPlayer);

        [Obsolete("GetLocalizedText is deprecated. Please use GetReturnValues instead.")]
        public static long GetLocalizedInt64(string szLuaVariable, uint lpLocalPlayer);

        [Obsolete("GetLocalizedText is deprecated. Please use GetReturnValues instead.")]
        public static ulong GetLocalizedUInt64(string szLuaVariable, uint lpLocalPlayer);

        [Obsolete("GetLocalizedText is deprecated. Please use GetReturnValues instead.")]
        public static bool GetLocalizedBool(string szLuaVariable, uint lpLocalPlayer);
    }
}
