﻿using System;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using LiveSplit.ComponentUtil;

namespace LiveSplit.EMUHELP
{
    /// <summary>
    /// Custom extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Perform a signature scan, similarly to how it would achieve with SignatureScanner.Scan()
        /// </summary>
        /// <returns>Address of the signature, if found. Otherwise, an Exception will be thrown.</returns>
        public static IntPtr ScanOrThrow(this SignatureScanner scanner, SigScanTarget target, int align = 1)
        {
            IntPtr tempAddr = scanner.Scan(target, align);
            tempAddr.ThrowIfZero();
            return tempAddr;
        }

        /// <summary>
        /// Checks whether a provided IntPtr is equal to IntPtr.Zero. If it is, an Exception will be thrown.
        /// </summary>
        /// <param name="ptr"></param>
        /// <exception cref="SigscanFailedException"></exception>
        public static void ThrowIfZero(this IntPtr ptr)
        {
            if (ptr.IsZero())
                throw new SigscanFailedException();
        }

        /// <summary>
        /// Checks is a specific bit inside a byte value is set or not.
        /// </summary>
        /// <param name="value">The byte value in which to perform the check</param>
        /// <param name="bitPos">The bit position. Can range from 0 to 7: any value outside this range will make the function automatically return false.</param>
        /// <returns></returns>
        public static bool BitCheck(this byte value, byte bitPos)
        {
            return bitPos >= 0 && bitPos <= 7 && (value & (1 << bitPos)) != 0;
        }

        /// <summary>
        /// Checks if a provided IntPtr value is equal to IntPtr.Zero
        /// </summary>
        /// <param name="value"></param>
        /// <returns>True is the value is IntPtr.Zero, false otherwise.</returns>
        public static bool IsZero(this IntPtr value)
        {
            return value == IntPtr.Zero;
        }
    }

    public class SigscanFailedException : Exception
    {
        public SigscanFailedException() { }
        public SigscanFailedException(string message) : base(message) { }
    }

    /// <summary>
    /// Tools for dealing with Endianess
    /// </summary>
    public static class ToLittleEndian
    {
        public static short SwapEndianess(this short value) => BitConverter.ToInt16(BitConverter.GetBytes(value).Reverse().ToArray(), 0);
        public static ushort SwapEndianess(this ushort value) => BitConverter.ToUInt16(BitConverter.GetBytes(value).Reverse().ToArray(), 0);
        public static int SwapEndianess(this int value) => BitConverter.ToInt32(BitConverter.GetBytes(value).Reverse().ToArray(), 0);
        public static uint SwapEndianess(this uint value) => BitConverter.ToUInt32(BitConverter.GetBytes(value).Reverse().ToArray(), 0);
        public static long SwapEndianess(this long value) => BitConverter.ToInt64(BitConverter.GetBytes(value).Reverse().ToArray(), 0);
        public static ulong SwapEndianess(this ulong value) => BitConverter.ToUInt64(BitConverter.GetBytes(value).Reverse().ToArray(), 0);
        public static IntPtr SwapEndianess(this IntPtr value) => (IntPtr)BitConverter.ToInt64(BitConverter.GetBytes((long)value).Reverse().ToArray(), 0);
        public static UIntPtr SwapEndianess(this UIntPtr value) => (UIntPtr)BitConverter.ToUInt64(BitConverter.GetBytes((ulong)value).Reverse().ToArray(), 0);
        public static float SwapEndianess(this float value) => BitConverter.ToSingle(BitConverter.GetBytes(value).Reverse().ToArray(), 0);
        public static double SwapEndianess(this double value) => BitConverter.ToDouble(BitConverter.GetBytes(value).Reverse().ToArray(), 0);


        /// <summary>
        /// Creates a new FakeMemoryWatcherList from an existing MemoryWatcherList, with each element having it's Current and Old properties with switched endianess.
        /// </summary>
        /// <param name="Watchers">A MemoryWatcherList with elements we want to convert from Big Endian to Little Endian (or vice versa).</param>
        /// <returns></returns>
        public static FakeMemoryWatcherList SetFakeWatchers(MemoryWatcherList Watchers)
        {
            var list = new FakeMemoryWatcherList();

            foreach (var entry in Watchers)
            {
                switch (entry)
                {
                    case MemoryWatcher<byte>:
                        list.Add(new FakeMemoryWatcher<byte>(() => (byte)entry.Current) { Name = entry.Name });
                        break;
                    case MemoryWatcher<sbyte>:
                        list.Add(new FakeMemoryWatcher<sbyte>(() => (sbyte)entry.Current) { Name = entry.Name });
                        break;
                    case MemoryWatcher<bool>:
                        list.Add(new FakeMemoryWatcher<bool>(() => (bool)entry.Current) { Name = entry.Name });
                        break;
                    case MemoryWatcher<short>:
                        list.Add(new FakeMemoryWatcher<short>(() => ((short)entry.Current).SwapEndianess()) { Name = entry.Name });
                        break;
                    case MemoryWatcher<ushort>:
                        list.Add(new FakeMemoryWatcher<ushort>(() => ((ushort)entry.Current).SwapEndianess()) { Name = entry.Name });
                        break;
                    case MemoryWatcher<int>:
                        list.Add(new FakeMemoryWatcher<int>(() => ((int)entry.Current).SwapEndianess()) { Name = entry.Name });
                        break;
                    case MemoryWatcher<uint>:
                        list.Add(new FakeMemoryWatcher<uint>(() => ((uint)entry.Current).SwapEndianess()) { Name = entry.Name });
                        break;
                    case MemoryWatcher<long>:
                        list.Add(new FakeMemoryWatcher<long>(() => ((long)entry.Current).SwapEndianess()) { Name = entry.Name });
                        break;
                    case MemoryWatcher<ulong>:
                        list.Add(new FakeMemoryWatcher<ulong>(() => ((ulong)entry.Current).SwapEndianess()) { Name = entry.Name });
                        break;
                    case MemoryWatcher<IntPtr>:
                        list.Add(new FakeMemoryWatcher<IntPtr>(() => ((IntPtr)entry.Current).SwapEndianess()) { Name = entry.Name });
                        break;
                    case MemoryWatcher<UIntPtr>:
                        list.Add(new FakeMemoryWatcher<UIntPtr>(() => ((UIntPtr)entry.Current).SwapEndianess()) { Name = entry.Name });
                        break;
                    case MemoryWatcher<float>:
                        list.Add(new FakeMemoryWatcher<float>(() => ((float)entry.Current).SwapEndianess()) { Name = entry.Name });
                        break;
                    case MemoryWatcher<double>:
                        list.Add(new FakeMemoryWatcher<double>(() => ((double)entry.Current).SwapEndianess()) { Name = entry.Name });
                        break;
                    case MemoryWatcher<char>:
                        list.Add(new FakeMemoryWatcher<char>(() => (char)entry.Current) { Name = entry.Name });
                        break;
                    case StringWatcher:
                        list.Add(new FakeMemoryWatcher<string>(() => (string)entry.Current) { Name = entry.Name });
                        break;
                }
            }

            return list;
        }
    }
}