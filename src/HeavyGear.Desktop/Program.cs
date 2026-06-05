#region File Description
//-----------------------------------------------------------------------------
// Program.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;

namespace HeavyGear
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            using HeavyGearManager game = new HeavyGearManager();
            game.Run();
        }
    }
}
