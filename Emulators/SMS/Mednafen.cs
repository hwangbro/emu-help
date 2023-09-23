﻿using LiveSplit.ComponentUtil;
using System;

namespace LiveSplit.EMUHELP.SMS
{
    internal class Mednafen : SMSBase
    {
        public Mednafen(HelperBase helper) : base(helper)
        {
            ram_base = Helper.game.SafeSigScanOrThrow(Helper.game.Is64Bit()
                ? new SigScanTarget(7, "25 FF 1F 00 00 88 90") { OnFound = (p, s, addr) => (IntPtr)p.ReadValue<int>(addr) }
                : new SigScanTarget(8, "25 FF 1F 00 00 0F B6 80") { OnFound = (p, s, addr) => (IntPtr)p.ReadValue<int>(addr) });

            Debugs.Info("  => Hooked to emulator: Mednafen");
            Debugs.Info($"  => RAM address found at 0x{ram_base.ToString("X")}");
        }

        public override bool KeepAlive()
        {
            return true;
        }
    }
}