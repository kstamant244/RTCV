namespace RTCV.CorruptCore
{
    using System.Collections.Generic;
    using RTCV.NetCore;

    public static class RTC_ClusterEngine
    {

        const string rand = "Random";
        const string reverse = "Reverse";
        const string rotFW = "Rotate Forwards";
        const string rotBW = "Rotate Backwards";

        public static string[] ShuffleTypes { get; private set; } = new string[] { rand, reverse, rotFW, rotBW };

        public static string LimiterListHash
        {
            get => (string)AllSpec.CorruptCoreSpec["CLUSTER_LIMITERLISTHASH"];
            set => AllSpec.CorruptCoreSpec.Update("CLUSTER_LIMITERLISTHASH", value);
        }

        public static int ChunkSize
        {
            get => (int)AllSpec.CorruptCoreSpec["CLUSTER_SHUFFLEAMT"];
            set => AllSpec.CorruptCoreSpec.Update("CLUSTER_SHUFFLEAMT", value);
        }

        public static string ShuffleType
        {
            get => (string)AllSpec.CorruptCoreSpec["CLUSTER_SHUFFLETYPE"];
            set => AllSpec.CorruptCoreSpec.Update("CLUSTER_SHUFFLETYPE", value);
        }

        public static int Modifier
        {
            get => (int)AllSpec.CorruptCoreSpec["CLUSTER_MODIFIER"];
            set => AllSpec.CorruptCoreSpec.Update("CLUSTER_MODIFIER", value);
        }


        public static bool OutputMultipleUnits
        {
            get => (bool)AllSpec.CorruptCoreSpec["CLUSTER_MULTIOUT"];
            set => AllSpec.CorruptCoreSpec.Update("CLUSTER_MULTIOUT", value);
        }

        private static int precision = 4;

        public static PartialSpec getDefaultPartial()
        {
            var partial = new PartialSpec("RTCSpec");
            partial["CLUSTER_LIMITERLISTHASH"] = string.Empty;
            partial["CLUSTER_SHUFFLETYPE"] = rand;
            partial["CLUSTER_SHUFFLEAMT"] = 3;
            partial["CLUSTER_MODIFIER"] = 1;
            partial["CLUSTER_MULTIOUT"] = true;
            return partial;
        }


        public static BlastUnit[] GenerateUnit(string domain, long address, int alignment)
        {
            if (Filtering.Hash2LimiterDico.TryGetValue(LimiterListHash, out IListFilter list))
            {
                precision = list.GetPrecision();
            }
            else
            {
                return null;
            }

            if (domain == null)
            {
                return null;
            }

            long safeAddress = address - (address % precision) + alignment;

            MemoryInterface mi = MemoryDomains.GetInterface(domain);
            if (mi == null)
            {
                return null;
            }

            if (safeAddress > mi.Size - precision)
            {
                safeAddress = mi.Size - (precision * 2) + alignment; //If we're out of range, hit the last aligned address
            }

            //Only query dictionary once per call
            int chunkSize = ChunkSize;

            if (safeAddress + (long)(chunkSize * precision) >= mi.Size)
            {
                return null;
            }

            //do not swap endianess
            byte[] GetSegment(long address)
            {
                byte[] values = new byte[precision];

                for (long i = 0; i < precision; i++)
                {
                    values[i] = mi.PeekByte(address + i);
                }

                return values;
            }

            void ShuffleRandom(List<byte[]> list)
            {
                int n = list.Count;
                while (n > 1)
                {
                    n--;
                    int k = RtcCore.RND.Next(n + 1);
                    byte[] value = list[k];
                    list[k] = list[n];
                    list[n] = value;
                }
            }

            void RotateForward(List<byte[]> list)
            {
                var x = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);
                list.Insert(0, x);
            }

            void RotateBackward(List<byte[]> list)
            {
                var x = list[0];
                list.RemoveAt(0);
                list.Add(x);
            }


            if (Filtering.LimiterPeekBytes(safeAddress, safeAddress + precision, domain, LimiterListHash, mi))
            {
                List<byte[]> byteArr = new List<byte[]>();


                for (int j = 0; j < chunkSize; j++)
                {
                    byteArr.Add(GetSegment(safeAddress + (long)(j * precision)));
                }

                int modifier = Modifier;
                switch (ShuffleType)
                {
                    case rotFW:
                        for (int j = 0; j < modifier; j++)
                        {
                            RotateForward(byteArr);
                        }
                        break;
                    case rotBW:
                        for (int j = 0; j < modifier; j++)
                        {
                            RotateBackward(byteArr);
                        }
                        break;
                    case reverse:
                        byteArr.Reverse();
                        break;
                    case rand:
                    default:
                        ShuffleRandom(byteArr);
                        break;
                }

                if (OutputMultipleUnits)
                {
                    BlastUnit[] ret = new BlastUnit[chunkSize];

                    for (int j = 0; j < chunkSize; j++)
                    {
                        //do not swap endianess
                        ret[j] = new BlastUnit(byteArr[j], domain, safeAddress + (j * precision), precision, false, 0, 1, null, true, false, true);
                    }

                    return ret;
                }
                else
                {
                    List<byte> btsOut = new List<byte>();
                    for (int j = 0; j < chunkSize; j++)
                    {
                        btsOut.AddRange(byteArr[j]);
                    }
                    //do not swap endianess
                    return new BlastUnit[] { new BlastUnit(btsOut.ToArray(), domain, safeAddress, (precision * chunkSize), false, 0, 1, null, true, false, true) };
                }
            }
            return null;
        }
    }
}
