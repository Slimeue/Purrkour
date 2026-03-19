using System.Collections.Generic;
using Platform;
using UnityEngine;

namespace Managers
{
    public class SpawnSectionContext
    {
        public PlatformInstance Platform { get; }
        public List<float> SlotXPositions { get; }
        public HashSet<int> ReservedSlots { get; }

        public SpawnSectionContext(PlatformInstance platform, List<float> slotXPositions)
        {
            Platform = platform;
            SlotXPositions = slotXPositions;
            ReservedSlots = new HashSet<int>();
        }

        public bool IsSlotFree(int index)
        {
            if (index < 0 || index >= SlotXPositions.Count)
                return false;

            return !ReservedSlots.Contains(index);
        }

        public bool CanReserveSlots(int startIndex, int slotWidth, int padding = 0)
        {
            int min = Mathf.Max(0, startIndex - padding);
            int max = Mathf.Min(SlotXPositions.Count - 1, startIndex + slotWidth - 1 + padding);

            for (int i = min; i <= max; i++)
            {
                if (ReservedSlots.Contains(i))
                    return false;
            }

            return true;
        }

        public void ReserveSlots(int startIndex, int slotWidth, int padding = 0)
        {
            int min = Mathf.Max(0, startIndex - padding);
            int max = Mathf.Min(SlotXPositions.Count - 1, startIndex + slotWidth - 1 + padding);

            for (int i = min; i <= max; i++)
            {
                ReservedSlots.Add(i);
            }
        }

        public List<int> GetFreeSlotIndices()
        {
            List<int> indices = new();

            for (int i = 0; i < SlotXPositions.Count; i++)
            {
                if (!ReservedSlots.Contains(i))
                    indices.Add(i);
            }

            return indices;
        }
    }
}
