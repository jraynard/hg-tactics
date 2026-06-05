#region File Description
//-----------------------------------------------------------------------------
// Dice.cs
//
// Created By:
// Justin Raynard
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using HeavyGear.GameLogic;

namespace HeavyGear.Helpers
{
    public static class Dice
    {
        private static Random generator;

        /// <summary>
        /// Rolls a number of dice for a given Skill and returns the result
        /// </summary>
        /// <param name="skill">The skill being rolled for</param>
        /// <returns>The highest dice rolled + 1 for every extra 6 rolled, or 0
        /// if all dice roll 1 (fumble)</returns>
        public static int Roll(Skill skill)
        {
            generator = new Random(DateTime.Now.Millisecond);
            int fumbleCount = 0;
            int result = 0;
            if (skill.Level <= 0)
                return 0;
            for (int i = 0; i < skill.Level; i++)
            {
                int roll = generator.Next(1, 6);
                if (roll == 1)
                {
                    fumbleCount++;
                }
                else if (roll == 6 && result >= 6)
                {
                    result++;
                }
                else if (roll > result)
                {
                    result = roll;
                }
            }

            generator = null;

            result += skill.Modifier;

            if (fumbleCount == skill.Level)
                return 0;
            else
                return result;
        }
        /// <summary>
        /// Rolls a number of dice and returns the result
        /// </summary>
        /// <param name="numberOfDice">The number of dice to roll</param>
        /// <returns>The highest dice rolled + 1 for every extra 6 rolled, 
        /// or 0 if all dice roll 1 (fumble)</returns>
        public static int Roll(int numberOfDice)
        {
            generator = new Random(DateTime.Now.Millisecond);
            int fumbleCount = 0;
            int result = 0;
            if (numberOfDice <= 0)
                return 0;
            for (int i = 0; i < numberOfDice; i++)
            {
                int roll = generator.Next(1, 6);
                if (roll == 1)
                {
                    fumbleCount++;
                }
                else if (roll == 6 && result >= 6)
                {
                    result++;
                }
                else if (roll > result)
                {
                    result = roll;
                }
            }

            generator = null;

            if (fumbleCount == numberOfDice)
                return 0;
            else
                return result;
        }
        /// <summary>
        /// Rolls 1 dice and returns result
        /// </summary>
        /// <returns>Returns the result of the roll, 1-6</returns>
        public static int Roll()
        {
            generator = new Random(DateTime.Now.Millisecond);
            int result = generator.Next(1, 6);
            generator = null;
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int Roll(int min, int max)
        {
            generator = new Random(DateTime.Now.Millisecond);
            int result = generator.Next(min, max);
            generator = null;
            return result;
        }
    }
}