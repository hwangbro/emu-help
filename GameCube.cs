﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiveSplit.ComponentUtil;
using LiveSplit.EMUHELP;
using System.Linq;

public partial class GCN
{
    // Stuff that need to be defined in the ASL
    public string[] Gamecodes { get; set; }
    public Func<IntPtr, MemoryWatcherList> Load { get; set; }

    // Other stuff
    private ProcessHook GameProcess { get; }
    private Func<bool> KeepAlive { get; set; }
    private Process game => GameProcess.Game;
    public MemoryWatcherList Watchers { get; private set; }
    public List<dynamic> LittleEndianWatchers { get; private set; }
    public bool IsBigEndian { get; set; }
    private IntPtr MEM1 { get; set; }


    public GCN()
    {
        var processNames = new string[]
        {
            "Dolphin",
        };

        Debug.WriteLine($@"Thank you for using emu-help, created by Jujstme! For more information, see https://github.com/Jujstme/emu-help.

Currently supported GCN emulators: Dolphin

If you have any questions, please tag @Jujstme#6860 in the #auto-splitters channel
of the Speedrun Tool Development Discord server: https://discord.gg/cpYsxz7");

        GameProcess = new ProcessHook(processNames);
    }

    public bool Update()
    {
        if (Gamecodes == null || Load == null)
            return false;

        if (!Init())
            return false;

        if (!KeepAlive())
        {
            GameProcess.InitStatus = GameInitStatus.NotStarted;
            return false;
        }

        Watchers.UpdateAll(game);
        foreach (var entry in LittleEndianWatchers) entry.Update();

        if (!Gamecodes.Contains(game.ReadString(MEM1, 6, " ")))
            return false;

        return true;
    }

    private bool Init()
    {
        // This "init" function checks if the autosplitter has connected to the game
        // (if it has not, there's no point in going further) and starts a Task to
        // get the needed memory addresses for the other methods.
        if (!GameProcess.IsGameHooked)
            return false;

        // The purpose of this task is to limit the update cycle to 1 every 1.5 seconds
        // (instead of the usual one every 16 msec) in order to avoid wasting resources
        if (GameProcess.InitStatus == GameInitStatus.NotStarted)
            Task.Run(() =>
            {
                GameProcess.InitStatus = GameInitStatus.InProgress;
                try
                {
                    var Init = GetWRAM();
                    MEM1 = Init.Item1;
                    KeepAlive = Init.Item2;
                    Watchers = Load(MEM1);
                    LittleEndianWatchers = SetFakeWatchers(Watchers).ToList();
                    GameProcess.InitStatus = GameInitStatus.Completed;
                }
                catch
                {
                    Task.Delay(2000).Wait();
                    GameProcess.InitStatus = GameInitStatus.NotStarted;
                }
                // I'm running this manually because the signature scanner, especially
                // if it runs several times, can take A LOT of memory, to the point of
                // filling your RAM with several GB of useless data that doesn't get
                // collected for some reason.
                GC.Collect();
            });

        // At this point, if init has not been completed yet, return
        // false to avoid running the rest of the splitting logic.
        return GameProcess.InitStatus == GameInitStatus.Completed;
    }

    public void Dispose()
    {
        GameProcess.Dispose();
    }

    public dynamic this[string index] => IsBigEndian ? LittleEndianWatchers.First(w => w.Name == index) : Watchers[index];

    private Tuple<IntPtr, Func<bool>> GetWRAM()
    {
        switch (game.ProcessName)
        {
            case "Dolphin": return Dolphin();
        }

        Debugs.Info("  => Unrecognized emulator. Autosplitter will be disabled");
        return new Tuple<IntPtr, Func<bool>>(IntPtr.Zero, () => true);
    }

    private IEnumerable<dynamic> SetFakeWatchers(MemoryWatcherList Watchers)
    {
        foreach (var entry in Watchers)
        {
            var type = entry.GetType();

            if (type == typeof(MemoryWatcher<byte>))
                yield return new FakeMemoryWatcher<byte>(() => (byte)entry.Current) { Name = entry.Name };
            else if (type == typeof(FakeMemoryWatcher<sbyte>))
                yield return new FakeMemoryWatcher<sbyte>(() => (sbyte)entry.Current) { Name = entry.Name };
            else if (type == typeof(MemoryWatcher<short>))
                yield return new FakeMemoryWatcher<short>(() => ToLittleEndian.Short((short)entry.Current)) { Name = entry.Name };
            else if (type == typeof(MemoryWatcher<ushort>))
                yield return new FakeMemoryWatcher<ushort>(() => ToLittleEndian.UShort((ushort)entry.Current)) { Name = entry.Name };
            else if (type == typeof(MemoryWatcher<int>))
                yield return new FakeMemoryWatcher<int>(() => ToLittleEndian.Int((int)entry.Current)) { Name = entry.Name };
            else if (type == typeof(MemoryWatcher<uint>))
                yield return new FakeMemoryWatcher<uint>(() => ToLittleEndian.UInt((uint)entry.Current)) { Name = entry.Name };
            else if (type == typeof(MemoryWatcher<long>))
                yield return new FakeMemoryWatcher<long>(() => ToLittleEndian.Long((long)entry.Current)) { Name = entry.Name };
            else if (type == typeof(MemoryWatcher<ulong>))
                yield return new FakeMemoryWatcher<ulong>(() => ToLittleEndian.ULong((ulong)entry.Current)) { Name = entry.Name };
            else if (type == typeof(MemoryWatcher<float>))
                yield return new FakeMemoryWatcher<float>(() => ToLittleEndian.Float((float)entry.Current)) { Name = entry.Name };
            else if (type == typeof(StringWatcher))
                yield return new FakeMemoryWatcher<string>(() => (string)entry.Current) { Name = entry.Name };
        }
    }
}