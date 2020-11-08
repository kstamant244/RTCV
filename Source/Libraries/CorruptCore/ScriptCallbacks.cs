namespace RTCV.CorruptCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ScriptCore.Attributes;

    static class ScriptCallbacks
    {
        [LuaCallback("PeekByte")]
        public static byte PeekByte(string domain, long address)
        {
            return MemoryDomains.GetInterface(domain).PeekByte(address);
        }

        [LuaCallback("PokeByte")]
        public static void PokeByte(string domain, long address, byte val)
        {
            MemoryDomains.GetInterface(domain).PokeByte(address,val);
        }

        [LuaCallback("PeekBytes")]
        public static byte[] PeekBytes(string domain, long address, int amt)
        {
            return MemoryDomains.GetInterface(domain).PeekBytes(address, address + amt, true);
        }

        [LuaCallback("PokeBytes")]
        public static void PokeBytes(string domain, long address, byte val)
        {
            MemoryDomains.GetInterface(domain).PokeByte(address, val);
        }


        [LuaCallback("Peek32")]
        public static ulong Peek32(string domain, long address, bool raw = true)
        {
            return BitConverter.ToUInt64(MemoryDomains.GetInterface(domain).PeekBytes(address,address + 4L, raw), 0);
        }

        //TODO: sort out endianess shenanigans
        [LuaCallback("Poke32")]
        public static void Poke32(string domain, long address, uint data, bool raw = true)
        {
            MemoryDomains.GetInterface(domain).PokeBytes(address, BitConverter.GetBytes(data), (BitConverter.IsLittleEndian ? !raw : raw)); //Reverse endian integers on windows
        }

        [LuaCallback("PeekFloat")]
        public static float PeekFloat(string domain, long address, bool raw = true)
        {
            return BitConverter.ToSingle(MemoryDomains.GetInterface(domain).PeekBytes(address, address + 4L, raw), 0);
        }

        [LuaCallback("PokeFloat")]
        public static void PokeFloat(string domain, long address, float data, bool raw = true)
        {
            MemoryDomains.GetInterface(domain).PokeBytes(address, BitConverter.GetBytes(data), raw);
        }
    }
}
