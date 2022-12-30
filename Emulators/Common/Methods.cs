﻿using System;
using System.Linq;
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
            CheckPtr(tempAddr);
            return tempAddr;
        }

        /// <summary>
        /// Checks whether a provided IntPtr is equal to IntPtr.Zero. If it is, an Exception will be thrown.
        /// </summary>
        /// <param name="ptr"></param>
        /// <exception cref="SigscanFailedException"></exception>
        public static void CheckPtr(IntPtr ptr)
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
        public static short Short(this short value) => BitConverter.ToInt16(BitConverter.GetBytes(value).Reverse().ToArray(), 0);
        public static ushort UShort(this ushort value) => BitConverter.ToUInt16(BitConverter.GetBytes(value).Reverse().ToArray(), 0);
        public static int Int(this int value) => BitConverter.ToInt32(BitConverter.GetBytes(value).Reverse().ToArray(), 0);
        public static uint UInt(this uint value) => BitConverter.ToUInt32(BitConverter.GetBytes(value).Reverse().ToArray(), 0);
        public static long Long(this long value) => BitConverter.ToInt64(BitConverter.GetBytes(value).Reverse().ToArray(), 0);
        public static ulong ULong(this ulong value) => BitConverter.ToUInt64(BitConverter.GetBytes(value).Reverse().ToArray(), 0);
        public static float Float(this float value) => BitConverter.ToSingle(BitConverter.GetBytes(value).Reverse().ToArray(), 0);
        public static double Double(this double value) => BitConverter.ToDouble(BitConverter.GetBytes(value).Reverse().ToArray(), 0);

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
                var type = entry.GetType();

                if (type == typeof(MemoryWatcher<bool>))
                    list.Add(new FakeMemoryWatcher<bool>(() => (bool)entry.Current) { Name = entry.Name });
                else if (type == typeof(MemoryWatcher<byte>))
                    list.Add(new FakeMemoryWatcher<byte>(() => (byte)entry.Current) { Name = entry.Name });
                else if (type == typeof(FakeMemoryWatcher<sbyte>))
                    list.Add(new FakeMemoryWatcher<sbyte>(() => (sbyte)entry.Current) { Name = entry.Name });
                else if (type == typeof(MemoryWatcher<short>))
                    list.Add(new FakeMemoryWatcher<short>(() => Short((short)entry.Current)) { Name = entry.Name });
                else if (type == typeof(MemoryWatcher<ushort>))
                    list.Add(new FakeMemoryWatcher<ushort>(() => UShort((ushort)entry.Current)) { Name = entry.Name });
                else if (type == typeof(MemoryWatcher<int>))
                    list.Add(new FakeMemoryWatcher<int>(() => Int((int)entry.Current)) { Name = entry.Name });
                else if (type == typeof(MemoryWatcher<uint>))
                    list.Add(new FakeMemoryWatcher<uint>(() => UInt((uint)entry.Current)) { Name = entry.Name });
                else if (type == typeof(MemoryWatcher<long>))
                    list.Add(new FakeMemoryWatcher<long>(() => Long((long)entry.Current)) { Name = entry.Name });
                else if (type == typeof(MemoryWatcher<ulong>))
                    list.Add(new FakeMemoryWatcher<ulong>(() => ULong((ulong)entry.Current)) { Name = entry.Name });
                else if (type == typeof(MemoryWatcher<float>))
                    list.Add(new FakeMemoryWatcher<float>(() => Float((float)entry.Current)) { Name = entry.Name });
                else if (type == typeof(StringWatcher))
                    list.Add(new FakeMemoryWatcher<string>(() => (string)entry.Current) { Name = entry.Name });
            }

            return list;
        }
    }
}