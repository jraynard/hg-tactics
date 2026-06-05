#region File Description
//-----------------------------------------------------------------------------
// Skill.cs
//
// Created By:
// Justin Raynard
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace HeavyGear.GameLogic
{
    public class Skill
    {
        private int level;
        private int modifier;

        public int Level
        {
            get
            {
                return level;
            }
            set
            {
                level = value;
            }
        }
        public int Modifier
        {
            get
            {
                return modifier;
            }
        }

        public Skill(int level, int modifier)
        {
            this.level = level;
            this.modifier = modifier;
        }
    }
}
