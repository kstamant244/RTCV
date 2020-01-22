using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTCV.CorruptCore
{
    /// <summary>
    /// Main use is to help refactoring. foreach is for chumps
    /// </summary>
    public class BlastSet
    {
        public List<BlastLayer> BlastLayers = new List<BlastLayer>();
        public List<BlastUnit> BlastUnitOverflow = new List<BlastUnit>();//Or a blast layer I guess


        public bool IsBaked { get; private set; } = false;
        public BlastUnit[] BakedList { get; private set; } = null;

        public BlastUnit this[int index]
        {
            get
            {
                if (IsBaked) return BakedList[index];

                int ct = BlastLayers.Count;
                for (int j = 0; j < ct; j++)
                {
                    if (index >= BlastLayers[j].Layer.Count)
                    {
                        index -= BlastLayers[j].Layer.Count;
                    }
                    else
                    {
                        return BlastLayers[j].Layer[index];
                    }
                }
                //else
                return BlastUnitOverflow[index];
            }

            set
            {
                if (IsBaked) BakedList[index] = value;

                int ct = BlastLayers.Count;
                for (int j = 0; j < ct; j++)
                {
                    if (index >= BlastLayers[j].Layer.Count)
                    {
                        index -= BlastLayers[j].Layer.Count;
                    }
                    else
                    {
                        BlastLayers[j].Layer[index] = value;
                        return;
                    }
                }
                //else
                BlastUnitOverflow[index] = value;
            }
        }

        /// <summary>
        /// Recommend caching instead of using in a loop condition
        /// </summary>
        public int Count
        {
            get
            {
                if (IsBaked) return BakedList.Length;

                int res = 0;
                for (int j = 0; j < BlastLayers.Count; j++)
                {
                    res += BlastLayers[j].Layer.Count;
                }
                res += BlastUnitOverflow.Count;
                return res;
            }
        }

        public int LayerCount { get => BlastLayers.Count; }

        /// <summary>
        /// Puts all the BlastUnits from all layers and the overflow into one array;
        /// </summary>
        public void Bake()
        {
            int ct = Count;
            BakedList = new BlastUnit[ct];
            CopyTo(BakedList, 0);
            IsBaked = true;
        }

        public void Unbake()
        {
            if (IsBaked)
            {
                BakedList = null;
                IsBaked = false;
            }
        }

        /// <summary>
        /// Adds a list to the cluster
        /// </summary>
        /// <param name="list"></param>
        public void Add(BlastLayer blastLayer)
        {
            BlastLayers.Add(blastLayer);
            Unbake();
        }

        public void Add(BlastUnit unit)
        {
            BlastUnitOverflow.Add(unit);
            Unbake();
        }
        public void ClearLayers()
        {
            BlastLayers.Clear();
            Unbake();
        }

        public void Clear()
        {
            int ct = BlastLayers.Count;
            for (int j = 0; j < ct; j++)
            {
                BlastLayers[j].Layer.Clear();
            }
            BlastUnitOverflow.Clear();
            Unbake();
        }

        public bool Contains(BlastUnit item)
        {
            int ct = BlastLayers.Count;
            for (int j = 0; j < ct; j++)
            {
                if (BlastLayers[j].Layer.Contains(item)) { return true; }
            }
            if (BlastUnitOverflow.Contains(item)) { return true; }

            //else
            return false;
        }

        public void Insert(int index, BlastUnit item)
        {
            int ct = BlastLayers.Count;
            for (int j = 0; j < ct; j++)
            {
                if (index >= BlastLayers[j].Layer.Count)
                {
                    index -= BlastLayers[j].Layer.Count;
                }
                else
                {
                    BlastLayers[j].Layer.Insert(index, item);
                    Unbake();
                    return;
                }
            }
            //else
            BlastUnitOverflow.Insert(index, item);
            Unbake();
        }

        public bool Remove(BlastUnit item)
        {
            int ct = BlastLayers.Count;
            for (int j = 0; j < ct; j++)
            {
                if (BlastLayers[j].Layer.Remove(item))
                {
                    //exit
                    Unbake();
                    return true;
                }
            }

            //else
            if (BlastUnitOverflow.Remove(item))
            {
                Unbake();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes element at an index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            int ct = BlastLayers.Count;
            for (int j = 0; j < ct; j++)
            {
                if (index >= BlastLayers[j].Layer.Count)
                {
                    index -= BlastLayers[j].Layer.Count;
                }
                else
                {
                    BlastLayers[j].Layer.RemoveAt(index);
                    Unbake();
                    return;
                }
            }
            //else
            BlastUnitOverflow.RemoveAt(index);
            Unbake();
        }

        public override string ToString()
        {
            string ret = "";
            for (int j = 0; j < BlastLayers.Count; j++)
            {
                ret += BlastLayers[j].ToString() + "\r\n";
            }
            ret += "Overflow:\r\n";
            for (int j = 0; j < BlastUnitOverflow.Count; j++)
            {
                ret += BlastUnitOverflow[j];
                if (j != BlastUnitOverflow.Count - 1)
                {
                    ret += ", ";
                }
            }
            ret += "\r\n";

            return ret;
        }

        /// <summary>
        /// Copies to the array starting at the specified index
        /// </summary>
        /// <param name="array">the array to copy to</param>
        /// <param name="arrayIndex">the starting index</param>
        public void CopyTo(BlastUnit[] array, int arrayIndex = 0)
        {
            if (arrayIndex < 0) { throw new IndexOutOfRangeException("You used a negative value"); }
            int ct = Count;
            for (int j = 0; j < ct; j++)
            {
                array[arrayIndex++] = this[j];
            }
        }

        /// <summary>
        /// Returns index of a blast unit, probably very slow
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(BlastUnit item)
        {
            int overallInd = 0;
            int ct = BlastLayers.Count;
            for (int j = 0; j < ct; j++)
            {
                int layerInd = BlastLayers[j].Layer.IndexOf(item);
                if (layerInd > -1)
                {
                    return overallInd + layerInd;
                }
                overallInd += BlastLayers[j].Layer.Count;
            }
            //else

            int overflowInd = BlastUnitOverflow.IndexOf(item);
            if (overflowInd > -1)
            {
                return overallInd + overflowInd;
            }

            //else
            return -1;
        }
    }
}
