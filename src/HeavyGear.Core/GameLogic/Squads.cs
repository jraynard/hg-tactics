#region File Description
//-----------------------------------------------------------------------------
// Squads.cs
//
// Created By:
// Justin Raynard
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using HeavyGear.Graphics;
using HeavyGear.Sounds;
#endregion

namespace HeavyGear.GameLogic
{
    public static class Squads
    {
        #region Northern Squads

        public static Squad NorthStandardGear(int playerIndex, int squadIndex)
        {
            List<Unit> units = new List<Unit>();
            units.Add(Units.HeadHunter(playerIndex, squadIndex, Experience.Qualified, true));
            units.Add(Units.AssaultHunter(playerIndex, squadIndex, Experience.Qualified, false));
            units.Add(Units.Hunter(playerIndex, squadIndex, Experience.Qualified, false));
            units.Add(Units.Hunter(playerIndex, squadIndex, Experience.Qualified, false));

            Squad squad = new Squad(units);
            return squad;
        }

        public static Squad NorthSupportGear(int playerIndex, int squadIndex)
        {
            List<Unit> units = new List<Unit>();
            units.Add(Units.Cheetah(playerIndex, squadIndex, Experience.Veteran, true));
            units.Add(Units.Grizzly(playerIndex, squadIndex, Experience.Qualified, false));
            units.Add(Units.Grizzly(playerIndex, squadIndex, Experience.Qualified, false));

            Squad squad = new Squad(units);
            return squad;
        }

        public static Squad NorthSpecialOpsGear(int playerIndex, int squadIndex)
        {
            List<Unit> units = new List<Unit>();
            units.Add(Units.StrikeJaguar(playerIndex, squadIndex, Experience.Veteran, true));
            units.Add(Units.StrikeCheetah(playerIndex, squadIndex, Experience.Veteran, false));
            units.Add(Units.FireJaguar(playerIndex, squadIndex, Experience.Veteran, false));

            Squad squad = new Squad(units);
            return squad;
        }

        public static Squad NorthStrider(int playerIndex, int squadIndex)
        {
            List<Unit> units = new List<Unit>();
            units.Add(Units.Mammoth(playerIndex, squadIndex, Experience.Qualified, true));
            units.Add(Units.Mammoth(playerIndex, squadIndex, Experience.Qualified, false));

            Squad squad = new Squad(units);
            return squad;
        }

        public static Squad NorthInfantry(int playerIndex, int squadIndex)
        {
            List<Unit> units = new List<Unit>();
            units.Add(Units.NorthRifleSquad(playerIndex, squadIndex, Experience.Qualified, true));
            units.Add(Units.NorthAntiHGSquad(playerIndex, squadIndex, Experience.Qualified, false));
            units.Add(Units.NorthRifleSquad(playerIndex, squadIndex, Experience.Qualified, false));

            Squad squad = new Squad(units);
            return squad;
        }

        #endregion

        #region Southern Squads

        public static Squad SouthStandardGear(int playerIndex, int squadIndex)
        {
            List<Unit> units = new List<Unit>();
            units.Add(Units.JagerCommand(playerIndex, squadIndex, Experience.Qualified, true));
            units.Add(Units.BlitzJager(playerIndex, squadIndex, Experience.Qualified, false));
            units.Add(Units.Jager(playerIndex, squadIndex, Experience.Qualified, false));
            units.Add(Units.Jager(playerIndex, squadIndex, Experience.Qualified, false));

            Squad squad = new Squad(units);
            return squad;
        }

        public static Squad SouthSupportGear(int playerIndex, int squadIndex)
        {
            List<Unit> units = new List<Unit>();
            units.Add(Units.Iguana(playerIndex, squadIndex, Experience.Veteran, true));
            units.Add(Units.SpittingCobra(playerIndex, squadIndex, Experience.Qualified, false));
            units.Add(Units.SpittingCobra(playerIndex, squadIndex, Experience.Qualified, false));

            Squad squad = new Squad(units);
            return squad;
        }

        public static Squad SouthSpecialOpsGear(int playerIndex, int squadIndex)
        {
            List<Unit> units = new List<Unit>();
            units.Add(Units.RazorFangBlackMamba(playerIndex, squadIndex, Experience.Veteran, true));
            units.Add(Units.BlitzIguana(playerIndex, squadIndex, Experience.Veteran, false));
            units.Add(Units.LongFangBlackMamba(playerIndex, squadIndex, Experience.Veteran, false));

            Squad squad = new Squad(units);
            return squad;
        }

        public static Squad SouthStrider(int playerIndex, int squadIndex)
        {
            List<Unit> units = new List<Unit>();
            units.Add(Units.Naga(playerIndex, squadIndex, Experience.Qualified, true));
            units.Add(Units.Naga(playerIndex, squadIndex, Experience.Qualified, false));

            Squad squad = new Squad(units);
            return squad;
        }

        public static Squad SouthInfantry(int playerIndex, int squadIndex)
        {
            List<Unit> units = new List<Unit>();
            units.Add(Units.SouthRifleSquad(playerIndex, squadIndex, Experience.Qualified, true));
            units.Add(Units.SouthAntiHGSquad(playerIndex, squadIndex, Experience.Qualified, false));
            units.Add(Units.SouthRifleSquad(playerIndex, squadIndex, Experience.Qualified, false));

            Squad squad = new Squad(units);
            return squad;
        }

        #endregion
    }
}
