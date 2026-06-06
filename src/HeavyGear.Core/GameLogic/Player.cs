#region File Description
//-----------------------------------------------------------------------------
// Player.cs
//
// Created By:
// Justin Raynard
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using HeavyGear.GameScreens;
using HeavyGear.Graphics;
using HeavyGear.Helpers;
using HeavyGear.Properties;
using HeavyGear.Sounds;
using Texture = HeavyGear.Graphics.Texture;
#endregion

namespace HeavyGear.GameLogic
{
    public enum ArmyType
    {
        NLCS,
        AST
    }

    /// <summary>
    /// Player helper class, holds all the current game properties
    /// </summary>
    public class Player
    {
        #region Variables
        /// <summary>
        /// List of all units this player controls in order of turn
        /// </summary>
        //private List<Unit> army;
        private List<Squad> squads;
        private List<Unit> units;
        private int playerIndex;
        private int armyIndex = 0;
        private string name;
        private bool ready;
        private List<(int unitIndex, DamageType damage, int infDamage)> pendingDamages = new List<(int, DamageType, int)>();

        public bool Ready
        {
            get
            {
                return ready;
            }
            set
            {
                ready = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }
        public int ArmyIndex
        {
            get
            {
                return armyIndex;
            }
            set
            {
                armyIndex = value;
            }
        }
        public List<Squad> Squads
        {
            get
            {
                return squads;
            }
            set
            {
                squads = value;
                units = new List<Unit>();
                foreach (Squad squad in squads)
                {
                    foreach (Unit unit in squad.Units)
                    {
                        units.Add(unit);
                    }
                }
            }
        }

        public List<Unit> Units
        {
            get
            {
                return units;
            }
            set
            {
                units = value;
            }
        }
        public int PlayerIndex
        {
            get
            {
                return playerIndex;
            }
        }

        #endregion

        #region Constructor
        /// <summary>
        /// Create player
        /// </summary>
        /// <param name="character">Set player character</param>
        public Player(int playerIndex, string name)
        {
            this.playerIndex = playerIndex;
            this.squads = new List<Squad>();
            this.units = new List<Unit>();
            this.name = name;
        }
        #endregion

        #region Handle game logic
        /// <summary>
        /// Update game logic, called every frame.
        /// </summary>
        public void UpdateLocal()
        {
            foreach (Unit unit in units)
                unit.Update();

            if (!HeavyGearManager.GameOver && !HeavyGearManager.DeployMode)
            {
                if (HeavyGearManager.ActivePlayer.PlayerIndex == playerIndex)
                {
                    //check here to see which player's turn it is and whether they should be next
                    int unitsLeft = 0;
                    foreach (Unit unit in units)
                    {
                        if (unit.IsTurn || unit.InAnimation || unit.IsShooting || unit.IsAttacking || unit.Projectiles.Count > 0)
                        {
                            unitsLeft++;
                        }
                    }
                    //If the current player has moved all units, start the next player's turn
                    if (unitsLeft == 0)
                        HeavyGearManager.EndTurn();
                }

                foreach (var pending in pendingDamages)
                {
                    if (pending.infDamage > 0)
                        units[pending.unitIndex].ApplyDamage(pending.infDamage);
                    else
                        units[pending.unitIndex].ApplyDamage(pending.damage);
                    HeavyGearManager.SendTargetPacket(pending.unitIndex);
                }
                pendingDamages.Clear();
            }
        }
        public void UpdateRemote(int framesBetweenPackets)
        {
            foreach (Unit unit in units)
                unit.UpdateRemote(framesBetweenPackets);
        }
        public void Draw()
        {
            foreach (Unit unit in units)
                unit.Draw();
        }

        public void ReadNormalPacket(BinaryReader reader, GameTime gameTime, TimeSpan latency)
        {
            float packetSendTime = (float)reader.ReadByte();

            foreach (Unit unit in units)
                unit.ReadUnitState(reader);
        }

        public void ReadDeployPacket(BinaryReader reader, GameTime gameTime, TimeSpan latency)
        {
            ready = reader.ReadBoolean();
            float packetSendTime = (float)reader.ReadByte();

            foreach (Unit unit in units)
                unit.ReadUnitState(reader);
        }

        public void WriteNormalPacket(BinaryWriter writer, GameTime gameTime)
        {
            writer.Write((byte)PacketType.Normal);
            writer.Write(gameTime.TotalGameTime.TotalSeconds);

            foreach (Unit unit in units)
                unit.WriteUnitState(writer);
        }
        public void WriteDeployPacket(BinaryWriter writer, GameTime gameTime)
        {
            writer.Write((byte)PacketType.Deploy);
            writer.Write(ready);
            writer.Write(gameTime.TotalGameTime.TotalSeconds);

            foreach (Unit unit in units)
                unit.WriteUnitState(writer);
        }

        public void WriteAttackPacket(BinaryWriter writer, int attackingUnitIndex)
        {
            writer.Write(attackingUnitIndex);

            byte[] unitData = units[attackingUnitIndex].WriteAttackPacket();

            writer.Write(unitData);
        }

        public void WriteTargetPacket(BinaryWriter writer, int targetedUnitIndex)
        {
            writer.Write(targetedUnitIndex);

            byte[] unitData = units[targetedUnitIndex].WriteTargetPacket();

            writer.Write(unitData);
        }

        public void ApplyDamage(int targetedUnitIndex, DamageType damage)
        {
            pendingDamages.Add((targetedUnitIndex, damage, 0));
        }

        public void ApplyInfantryDamage(int targetedUnitIndex, int damage)
        {
            pendingDamages.Add((targetedUnitIndex, DamageType.None, damage));
        }
        #endregion
    }
}
