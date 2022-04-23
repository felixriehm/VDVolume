using System.Collections;
using VDVolume.Model;

namespace VDVolume.Serialization
{
    internal static class VoxelUtil
    {
        internal static void AddColorToBitMask(BitArray bitmask, byte color)
        {
            // e.g.
            // color:    ..00 0000 1010
            // _bitmask: ..01 0010 1001
            // ~clearingMask: ..00 0000 0011
            // _bitmask & ~clearingMask: ..00 0000 0001 #clear bits
            // color << 2: ..00 0010 1000
            // operator |: ..00 0010 1000 | ..00 0000 0001 => ..00 0010 1001
            //uint clearingMask = 0x3FC; // ..11 1111 1100
            //bitmask = (bitmask & ~clearingMask) | ((uint) color << 2);

            BitArray colorInBits = new BitArray(new byte[] {color});
            for (int i = 0; i < colorInBits.Length; i++)
            {
                bitmask[i + 2] = colorInBits[i];
            }
        }

        internal static void AddStateToBitMask(BitArray bitmask, VoxelState state)
        {
            //uint clearingMask = 0x3; // .... 0011
            //bitmask = (bitmask & ~clearingMask) | (uint) state;

            BitArray stateInBits = new BitArray(new int[] {(int)state});
            for (int i = 0; i < 2; i++)
            {
                bitmask[i] = stateInBits[i];
            }
        }
    }
}