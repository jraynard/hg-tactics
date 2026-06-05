#region File Description
//-----------------------------------------------------------------------------
// Unit.cs
//
// Created By:
// Justin Raynard
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using HeavyGear.Graphics;
using HeavyGear.Helpers;
using HeavyGear.Sounds;
using HeavyGear.Properties;
using HeavyGear.GameScreens;
#endregion

namespace HeavyGear.GameLogic
{
    public enum UnitType
    {
        Walker,
        Ground,
        Hover,
        Infantry
    }
    public enum DamageType
    {
        None,
        Light,
        Heavy,
        Overkill
    }

    public static class Facing
    {
        public static float North = MathHelper.ToRadians(180);
        public static float NorthEast = MathHelper.ToRadians(225);
        public static float SouthEast = MathHelper.ToRadians(315);//90;
        public static float South = MathHelper.ToRadians(0);
        public static float SouthWest = MathHelper.ToRadians(45);
        public static float NorthWest = MathHelper.ToRadians(135);//270;
    }

    public static class FacingInt
    {
        public const int North = 180;
        public const int NorthEast = 225;
        public const int SouthEast = 315;//90;
        public const int South = 0;
        public const int SouthWest = 45;
        public const int NorthWest = 135;//270;
    }

    public enum AnimationType
    {
        Move,
        Fire,
        Hit,
        Destroyed
    }

    public enum SpeedType
    {
        CombatSpeed,
        TopSpeed
    }

    public struct UnitState
    {
        /// <summary>
        /// Stores the units position on the map grid
        /// </summary>
        public Point MapPosition;
        /// <summary>
        /// If the unit is moving, determines the current position between the two hex tiles. Otherwise set to Vector2.Zero
        /// </summary>
        public Vector2 Position;
        /// <summary>
        /// Current Facing of the unit
        /// </summary>
        public float Rotation;
    }

    public class Unit
    {
        #region Constants
        private const float MoveRate = 250f;
        #endregion

        #region Variables
        // This is the latest master copy of the tank state, used by our local
        // physics computations and prediction. This state will jerk whenever
        // a new network packet is received.
        private UnitState simulationState;


        // This is a copy of the state from immediately before the last
        // network packet was received.
        private UnitState previousState;


        // This is the tank state that is drawn onto the screen. It is gradually
        // interpolated from the previousState toward the simultationState, in
        // order to smooth out any sudden jumps caused by discontinuities when
        // a network packet suddenly modifies the simultationState.
        private UnitState displayState;


        // Used to interpolate displayState from previousState toward simulationState.
        private float currentSmoothing;

        // Averaged time difference from the last 100 incoming packets, used to
        // estimate how our local clock compares to the time on the remote machine.
        private RollingAverage clockDelta = new RollingAverage(100);

        
        /// <summary>
        /// Index of this unit type in the overall army list for each side, used for menu textures
        /// </summary>
        private int unitIndex;
        private int armyIndex;
        /// <summary>
        /// the animations for this unit
        /// </summary>
        private Animation[] animation = new Animation[4];
        /// <summary>
        /// Currently playing animation for the unit
        /// </summary>
        public int animationIndex;
        private bool attacking;
        private bool shooting;
        private bool hitTarget;
        private bool moving;
        private Vector2 moveOrig;
        private Vector2 moveDest;
        private float xSpeed = 0f;
        private float ySpeed = 0f;
        
        /// <summary>
        /// represents the path of movement
        /// </summary>
        private List<HexTile> path;
        /// <summary>
        /// The current step in the path we are on
        /// </summary>
        private int pathIndex;
        /// <summary>
        /// The PlayerIndex of the player this unit belongs too
        /// </summary>
        private int playerIndex;

        private int squadIndex;
        private bool squadLeader;

        #region Unit variables
        /// <summary>
        /// Type of Unit this is
        /// </summary>
        private UnitType unitType;
        /// <summary>
        /// Name of this unit
        /// </summary>
        private string name;
        /// <summary>
        /// General indication of a unit's strength
        /// </summary>
        private int threatValue;
        
        /// <summary>
        /// List of weapons this unit is carrying
        /// </summary>
        private List<Weapon> weapons;
        /// <summary>
        /// The index of the currently equipped weapon of the unit
        /// </summary>
        private int weapon;
        /// <summary>
        /// How many Hex this unit has moved in this or last turn
        /// </summary>
        private int hexesMoved;
        /// <summary>
        /// total movement points this unit has remaining
        /// </summary>
        private int mp;
        /// <summary>
        /// how many times has the unit turned after last move
        /// </summary>
        private int timesTurned;
        /// <summary>
        /// Has this unit moved this turn?
        /// </summary>
        private bool hasMoved;
        /// <summary>
        /// Has this unit attacked this turn?
        /// </summary>
        private bool hasUsedActions;
        /// <summary>
        /// Is it this unit's turn?
        /// </summary>
        private bool isTurn;
        /// <summary>
        /// How many actions this unit has performed this turn
        /// </summary>
        private int actionsUsed;
        /// <summary>
        /// the unit this unit currently has designated by it's target designator
        /// </summary>
        private Unit taggedUnit;
        /// <summary>
        /// The unit this unit is currently acting as forward observer on
        /// </summary>
        private Unit observedUnit;
        private Unit targetedUnit;
        private DamageType damageToApply;
        private int infantryDamage = 0;
        private bool destroyed = false;

        private List<Projectile> projectiles = new List<Projectile>();
        private int currentProjectile = 0;
        private float lastProjectileFired = 0.0f;

        #endregion

        #endregion

        #region Properties
        public virtual int Skill { get { return 0;  } }
        public virtual Experience Experience { get { return Experience.Rookie; } }
        public bool IsSquadLeader
        {
            get
            {
                return squadLeader;
            }
        }
        public int SquadIndex
        {
            get
            {
                return squadIndex;
            }
        }
        public float Rotation
        {
            get
            {
                return simulationState.Rotation;
            }
            set
            {
                simulationState.Rotation = value;
            }
        }
        public Point MapPosition
        {
            get
            {
                return simulationState.MapPosition;
            }
            set
            {
                simulationState.MapPosition = value;
            }
        }
        public bool InAnimation
        {
            get
            {
                if (animationIndex >= 0)
                    return true;
                else
                    return false;
            }
        }
        public int InfantryDamage
        {
            set
            {
                infantryDamage = value;
            }
        }
        public DamageType DamageToApply
        {
            set
            {
                damageToApply = value;
            }
        }
        public int UnitIndex
        {
            get
            {
                return unitIndex;
            }
        }
        public bool IsAlive
        {
            get
            {
                return !destroyed;
            }
        }
        public bool HitTarget
        {
            get
            {
                return hitTarget;
            }
            set
            {
                hitTarget = value;
            }
        }
        public List<Projectile> Projectiles
        {
            get
            {
                return projectiles;
            }
        }
        public bool IsShooting
        {
            get
            {
                return shooting;
            }
        }
        
        public bool IsAttacking
        {
            get
            {
                return attacking;
            }
        }
        public bool IsMoving
        {
            get
            {
                return moving;
            }
        }
        public int ActionsUsed
        {
            get
            {
                return actionsUsed;
            }
            set
            {
                actionsUsed = value;
            }
        }
        public int PlayerIndex
        {
            get
            {
                return this.playerIndex;
            }
            set
            {
                this.playerIndex = value;
            }
        }
        public int TimesTurned
        {
            get
            {
                return timesTurned;
            }
            set
            {
                timesTurned = value;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public List<Weapon> Weapons
        {
            get
            {
                return this.weapons;
            }
        }

        public int Weapon
        {
            get
            {
                return this.weapon;
            }
            set
            {
                this.weapon = value;
            }
        }

        public UnitType UnitType
        {
            get
            {
                return this.unitType;
            }
        }
        public int ThreatValue
        {
            get
            {
                return this.threatValue;
            }
        }

        public int HexesMoved
        {
            get
            {
                return this.hexesMoved;
            }
        }

        public bool HasMoved
        {
            get
            {
                return this.hasMoved;
            }
            set
            {
                this.hasMoved = value;
            }
        }

        public bool HasUsedActions
        {
            get
            {
                return this.hasUsedActions;
            }
            set
            {
                this.hasUsedActions = value;
            }
        }

        public bool IsTurn
        {
            get
            {
                return this.isTurn;
            }
            set
            {
                this.isTurn = value;
            }
        }

        public int MP
        {
            get
            {
                return mp;
            }
            set
            {
                mp = value;
            }
        }

        public Unit TaggedUnit
        {
            get
            {
                return taggedUnit;
            }
            set
            {
                taggedUnit = value;
            }
        }
        public Unit ObservedUnit
        {
            get
            {
                return observedUnit;
            }
            set
            {
                observedUnit = value;
            }
        }
        public Unit TargetedUnit
        {
            get
            {
                return targetedUnit;
            }
            set
            {
                targetedUnit = value;
            }
        }

        #endregion

        #region Constructor
        /// <summary>
        /// Create car physics controller
        /// </summary>
        /// <param name="setCarPosition">Set car position</param>
        public Unit(UnitType unitType, string name, int threatValue, List<Weapon> weapons, int playerIndex, int armyIndex, int squadIndex, 
            int unitIndex, bool squadLeader)
        {
            this.unitType = unitType;
            this.name = name;
            this.threatValue = threatValue;
            this.weapons = weapons;
            this.playerIndex = playerIndex;
            this.unitIndex = unitIndex;
            this.squadIndex = squadIndex;
            this.armyIndex = armyIndex;
            this.squadLeader = squadLeader;
            this.simulationState.MapPosition = new Point(-1, -1);
            this.simulationState.Position = Vector2.Zero;
            //this.origin = new Vector2(UIRenderer.UnitIdleGfxRect.Width / 2, UIRenderer.UnitIdleGfxRect.Height / 2);
            this.animation[0] = new Animation(UIRenderer.UnitMoveGfxRect, 4, 120f);
            this.animation[1] = new Animation(UIRenderer.UnitFireGfxRect, 4, 120f);
            this.animation[2] = new Animation(UIRenderer.UnitHitGfxRect, 4, 120f);
            this.animation[3] = new Animation(UIRenderer.UnitDestroyGfxRect, 4, 120f);
            this.weapon = 0;
            mp = 0;
            hexesMoved = 0;
            actionsUsed = 0;
        }
        #endregion

        #region Reset
        /// <summary>
        /// Reset all player entries for restarting a game, just resets the
        /// car speed here.
        /// </summary>
        public void Reset()
        {
            mp = 0;
            hexesMoved = 0;
            actionsUsed = 0;
            hasUsedActions = false;
            hasMoved = false;
            isTurn = false;
            moving = false;
            attacking = false;
            //destroyed = false;
        }
       #endregion

        #region Draw
        public void Draw()
        {
            if (HeavyGearManager.InGame)
            {
                //Avoid drawing unit if it hasn't been placed
                if (simulationState.MapPosition.X < 0 || simulationState.MapPosition.Y < 0)
                    return;

                //Get the unit's position from the grid
                //in this section we're using reference methods
                //for the high frequency math functions
                Vector2 origin = HeavyGearManager.Map.Origin;
                float zoomValue = HeavyGearManager.Map.ZoomValue;
                Vector2 position;

                // convert from hex map position to pixel position
                if (simulationState.Position == Vector2.Zero)
                    BaseGame.ConvertMapToPixel(ref simulationState.MapPosition, out position);
                else
                    position = displayState.Position;

                //Now, we get the camera position relative to the unit's map position
                Vector2.Subtract(ref position, ref origin, out position);

                Vector2 unitOffset = new Vector2();
                unitOffset.X = BaseGame.UnitOffset.X / zoomValue;
                unitOffset.Y = BaseGame.UnitOffset.Y / zoomValue;

                //center the unit
                Vector2.Add(ref position, ref unitOffset, out position);

                Rectangle renderRect = new Rectangle((int)position.X, (int)position.Y, 
                    (int)(BaseGame.TileWidth / zoomValue), (int)(BaseGame.TileWidth / zoomValue));

                if (animationIndex >= 0)
                    Units.DrawAnimation(armyIndex, unitIndex, renderRect, animation[animationIndex].AnimationRect, displayState.Rotation, animation[animationIndex].FrameIndex);
                else if (IsAlive)
                    Units.Draw(armyIndex, unitIndex, renderRect, UIRenderer.UnitIdleGfxRect, displayState.Rotation);
                else
                    Units.Draw(armyIndex, unitIndex, renderRect, UIRenderer.UnitDestroyedGfxRect, displayState.Rotation);
                foreach (Projectile projectile in projectiles)
                {
                    if (!projectile.Done)
                        projectile.Draw();
                }
            }
        }
        #endregion

        #region Update
        public virtual void Update()
        {
            #region Movement
            if (simulationState.Position != Vector2.Zero)
            {
                if (Math.Abs(simulationState.Position.X - moveDest.X )< 5.0 && Math.Abs(simulationState.Position.Y - moveDest.Y) < 5.0)
                    simulationState.Position = Vector2.Zero;
                else
                {
                    simulationState.Position.X = simulationState.Position.X + MoveRate * BaseGame.MoveFactorPerSecond * xSpeed;
                    simulationState.Position.Y = simulationState.Position.Y + MoveRate * BaseGame.MoveFactorPerSecond * ySpeed;
                }
            }
            #endregion

            #region Projectiles
            if (projectiles.Count > 0)
            {
                float projectileInterval = GetProjectileInterval(weapons[weapon].WeaponType);

                if (currentProjectile < projectiles.Count)
                {
                    if (lastProjectileFired > projectileInterval)
                    {
                        currentProjectile++;
                        if (animationIndex < 0 && currentProjectile < projectiles.Count)
                            StartAnimation(AnimationType.Fire);
                        lastProjectileFired = 0.0f;
                    }
                    else
                    {
                        lastProjectileFired += BaseGame.ElapsedTimeThisFrameInMilliseconds;
                    }
                }

                int projectilesDone = 0;
                for (int i = 0; i < currentProjectile; i++)
                {
                    projectiles[i].Update();
                    if (projectiles[i].Done)
                        projectilesDone++;
                }

                if (projectilesDone == projectiles.Count)
                {
                    projectiles.Clear();
                    currentProjectile = 0;
                }
            }
            #endregion

            #region Animation
            if (animationIndex >= 0)
            {
                if (animation[animationIndex].Done)
                    animation[animationIndex].Start();
                else if (!animation[animationIndex].Update())
                    animationIndex = -1;


            }
            #endregion

            #region Logic
            else
            {
                if (attacking)
                {
                    int count = 1;
                    Weapon weaponInUse = weapons[weapon];
                    if (weaponInUse.RateOfFire > 0)
                    {
                        count = weaponInUse.RateOfFire * 2;
                        /*
                        if (weaponInUse.WeaponType == WeaponType.Missile || weaponInUse.WeaponType == WeaponType.Rocket)
                        {
                            for (int i = 0; i < weaponInUse.RateOfFire; i++)
                            {
                                count *= 2;
                            }
                        }
                        else
                        {
                            count = weaponInUse.RateOfFire * 10;
                        }*/
                    }

                    AddProjectiles(weaponInUse.WeaponType, count);
                    int thisUnit = HeavyGearManager.Player(playerIndex).Units.IndexOf(this);
                    if (HeavyGearManager.Transport.IsConnected)
                        HeavyGearManager.SendAttackPacket(thisUnit);
                    attacking = false;
                    shooting = true;
                }
                else if (shooting)
                {
                    if (projectiles.Count == 0)
                    {
                        shooting = false;
                        if (hitTarget && !HeavyGearManager.Transport.IsConnected)
                        {
                            if (targetedUnit.UnitType == UnitType.Infantry)
                                targetedUnit.ApplyDamage(infantryDamage);
                            else
                                targetedUnit.ApplyDamage(damageToApply);
                        }
                        else
                            targetedUnit = null;

                        hitTarget = false;
                        targetedUnit = null;
                        damageToApply = DamageType.None;
                        infantryDamage = 0;
                    }
                }
                if (moving)
                {
                    //check to see if we ran out of mp, just in case
                    if (mp <= 0)
                        moving = false;
                    else
                    {
                        if (pathIndex == path.Count)
                        {
                            moving = false;
                            simulationState.Position = Vector2.Zero;
                        }
                        else
                        {
                            if (Move(path[pathIndex].MapPosition))
                                pathIndex++;
                            else
                            {
                                moving = false;
                                simulationState.Position = Vector2.Zero;
                            }
                        }
                    }
                }
            }
            #endregion

            // Locally controlled tanks have no prediction or smoothing, so we
            // just copy the simulation state directly into the display state.
            displayState = simulationState;
        }
        #endregion

        #region UpdateRemote
        public void UpdateRemote(int framesBetweenPackets)
        {
            // Update the smoothing amount, which interpolates from the previous
            // state toward the current simultation state. The speed of this decay
            // depends on the number of frames between packets: we want to finish
            // our smoothing interpolation at the same time the next packet is due.
            float smoothingDecay = 1.0f / framesBetweenPackets;

            currentSmoothing -= smoothingDecay;

            if (currentSmoothing < 0)
                currentSmoothing = 0;

            if (currentSmoothing > 0)
                // Interpolate the display state gradually from the
                // previous state to the current simultation state.
                ApplySmoothing();
            else
            // Copy the simulation state directly into the display state.
                displayState = simulationState;

            if (projectiles.Count > 0)
            {
                bool done = false;
                foreach (Projectile projectile in projectiles)
                {
                    projectile.Update();
                    if (projectile.Done)
                        done = true;
                }

                if (done)
                    projectiles.Clear();
            }
        }
        #endregion
        
        #region ApplySmoothing
        /// <summary>
        /// Applies smoothing by interpolating the display state somewhere
        /// in between the previous state and current simulation state.
        /// </summary>
        public void ApplySmoothing()
        {
            if (simulationState.Position != Vector2.Zero)
                displayState.Position = Vector2.Lerp(simulationState.Position,
                                                 previousState.Position,
                                                 currentSmoothing);

            displayState.Rotation = MathHelper.Lerp(simulationState.Rotation,
                                                          previousState.Rotation,
                                                          currentSmoothing);
        }
        #endregion

        #region WriteUnitState
        /// <summary>
        /// Writes our local tank state into a byte[] for network transmission
        /// </summary>
        public void WriteUnitState(BinaryWriter packetWriter)
        {
            // Send the current state of the tank.
            packetWriter.Write(simulationState.MapPosition.X);
            packetWriter.Write(simulationState.MapPosition.Y);
            packetWriter.Write(simulationState.Position.X);
            packetWriter.Write(simulationState.Position.Y);
            packetWriter.Write(simulationState.Rotation);
        }
        #endregion

        #region ReadUnitState
        /// <summary>
        /// Reads the state of a remotely controlled player from a network packet.
        /// </summary>
        public void ReadUnitState(BinaryReader packetReader)
        {
            previousState = displayState;
            currentSmoothing = 1;

            simulationState.MapPosition.X = packetReader.ReadInt32();
            simulationState.MapPosition.Y = packetReader.ReadInt32();
            simulationState.Position = new Vector2(packetReader.ReadSingle(), packetReader.ReadSingle());
            simulationState.Rotation = packetReader.ReadSingle();

            if (simulationState.Position != Vector2.Zero && previousState.Position == Vector2.Zero)
                StartAnimation(AnimationType.Move);
        }
        #endregion

        #region Write Attack/Target Packet
        public byte[] WriteAttackPacket()
        {
            byte[] data = new byte[5];

            int unitIndex = HeavyGearManager.Player(targetedUnit.PlayerIndex).Units.IndexOf(targetedUnit);

            data[0] = (byte)targetedUnit.UnitType;
            data[1] = (byte)targetedUnit.PlayerIndex;
            data[2] = (byte)unitIndex;
            if (targetedUnit.UnitType == UnitType.Infantry)
                data[3] = (byte)infantryDamage;
            else
                data[3] = (byte)damageToApply;
            data[4] = (byte)weapons[weapon].WeaponType;

            return data;
        }

        public byte[] WriteTargetPacket()
        {
            byte[] data = new byte[1];

            data[0] = Convert.ToByte(destroyed);

            return data;
        }
        #endregion

        #region ApplyPrediction
        /*
        void ApplyPrediction(GameTime gameTime, TimeSpan latency, float packetSendTime)
        {
            // Work out the difference between our current local time
            // and the remote time at which this packet was sent.
            float localTime = (float)gameTime.TotalGameTime.TotalSeconds;

            float timeDelta = localTime - packetSendTime;

            // Maintain a rolling average of time deltas from the last 100 packets.
            clockDelta.AddValue(timeDelta);

            // The caller passed in an estimate of the average network latency, which
            // is provided by the XNA Framework networking layer. But not all packets
            // will take exactly that average amount of time to arrive! To handle
            // varying latencies per packet, we include the send time as part of our
            // packet data. By comparing this with a rolling average of the last 100
            // send times, we can detect packets that are later or earlier than usual,
            // even without having synchronized clocks between the two machines. We
            // then adjust our average latency estimate by this per-packet deviation.

            float timeDeviation = timeDelta - clockDelta.AverageValue;

            latency += TimeSpan.FromSeconds(timeDeviation);

            TimeSpan oneFrame = TimeSpan.FromSeconds(1.0 / 60.0);

            // Apply prediction by updating our simulation state however
            // many times is necessary to catch up to the current time.
            while (latency >= oneFrame)
            {
                UpdateState(ref simulationState);

                latency -= oneFrame;
            }
        }*/
        #endregion

        #region AddProjectiles
        public void AddProjectiles(WeaponType weaponType, int count)
        {
            Point origin = simulationState.MapPosition;
            Point target = targetedUnit.MapPosition;

            Vector2 origV, targV;
            BaseGame.ConvertMapToPixel(ref origin, out origV);
            BaseGame.ConvertMapToPixel(ref target, out targV);
            for (int i = 0; i < count; i++)
            {
                switch (weaponType)
                {
                    case WeaponType.AntiHGRifle:
                    case WeaponType.AssaultRifle:
                    case WeaponType.AutoCannon:
                    case WeaponType.FieldGun:
                    case WeaponType.MachineGun:
                    case WeaponType.RailGun:
                        projectiles.Add(Projectile.Bullet(origV, targV));
                        break;
                    case WeaponType.Missile:
                        projectiles.Add(Projectile.Missile(origV, targV));
                        break;
                    case WeaponType.Rocket:
                    case WeaponType.Mortar:
                        projectiles.Add(Projectile.Rocket(origV, targV));
                        break;
                    case WeaponType.Grenade:
                    case WeaponType.GrenadeLauncher:
                        projectiles.Add(Projectile.Grenade(origV, targV));
                        break;
                }
            }

        }
        #endregion

        #region GetProjectileInterval
        public float GetProjectileInterval(WeaponType weaponType)
        {
            switch (weaponType)
            {
                case WeaponType.AssaultRifle:
                case WeaponType.AutoCannon:
                    return 75f;
                case WeaponType.MachineGun:
                    return 50f;
                case WeaponType.AntiHGRifle:
                case WeaponType.FieldGun:
                    return 250f;
                case WeaponType.Grenade:
                case WeaponType.GrenadeLauncher:
                    return 150f;
                case WeaponType.Missile:
                case WeaponType.Rocket:
                    return 50f;
                case WeaponType.Mortar:
                case WeaponType.RailGun:
                    return 300f;
                default:
                    return 250f;
            }
        }
        #endregion

        #region ObserveUnit
        public virtual bool ObserveUnit(Unit target)
        {
            if (HeavyGearManager.Map.CheckLOS(this, target))
            {
                HeavyGearManager.MessageLogAdd(name + " is observing " + target.Name);
                observedUnit = target;
                actionsUsed++;
                return true;
            }
            else
            {
                HeavyGearManager.MessageLogAdd(target.Name + " is outside " + name + "'s line of sight.");
                return false;
            }
        }
        #endregion

        #region Start Turn
        public virtual void StartTurn()
        {
            this.isTurn = true;
            this.hasMoved = false;
            this.hasUsedActions = false;
            timesTurned = 0;
            hexesMoved = 0;
            //mp = 0;
            actionsUsed = 0;
            foreach (Weapon weapon in weapons)
                weapon.Reset();
            observedUnit = null;
            taggedUnit = null;
            targetedUnit = null;
        }
        #endregion

        #region MoveUnit
        public void MoveUnit(List<HexTile> newPath)
        {
            if (newPath == null)
                return;
            path = newPath;
            //Start on 1 since the origin is the first in the list
            pathIndex = 1;
            moving = true;
            //set initial rotation
            Rotation = MathHelper.ToRadians(DetermineAngle(MapPosition, path[1].MapPosition));

        }
        /// <summary>
        /// Attempt to move unit to the destination tile
        /// </summary>
        /// <returns>True if the unit moved, false if not</returns>
        private bool Move(Point destination)
        {
            #region Set Destination and Current tiles

            HexTile currentTile = HeavyGearManager.Map.GetTile(simulationState.MapPosition.X, simulationState.MapPosition.Y);
            HexTile destinationTile = HeavyGearManager.Map.GetTile(destination.X, destination.Y);

            #endregion

            #region Collistion check
            //Check if destination is already occupied by another unit
            bool occupied = false;
            for (int i = 0; i < HeavyGearManager.NumberOfPlayers; i++)
            {
                Player player = HeavyGearManager.Player(i);
                foreach (Unit unit in player.Units)
                {
                    if (unit.simulationState.MapPosition == destination)
                        occupied = true;
                }
            }
            if (occupied)
                return false;

            //Check to see if the unit is trying to go through a cliff
            if (destinationTile.Elevation - currentTile.Elevation > 1)
                return false;

            #endregion

            #region Movement costs
            
            int tileCost = GetTileCost(currentTile, destinationTile);

            if (unitType != UnitType.Infantry && tileCost > 1)
            {
                Vehicle vehicle = (Vehicle)this;
                if (vehicle.ImprovedOffRoad)
                    tileCost--;
            }

            if (unitType == UnitType.Infantry && tileCost > 2)
                tileCost = 2;
            
            #endregion

            #region Unit Movement
            mp -= tileCost;
            hexesMoved++;
            simulationState.MapPosition = destination;
            timesTurned = 0;

            //Rotate the unit to face the correct way
            Rotation = MathHelper.ToRadians(DetermineAngle(currentTile.MapPosition, destinationTile.MapPosition));

            StartAnimation(AnimationType.Move);

            //Initialize origin and dest
            BaseGame.ConvertMapToPixel(currentTile.MapPosition.X, currentTile.MapPosition.Y, out moveOrig);
            simulationState.Position = moveOrig;
            BaseGame.ConvertMapToPixel(destination.X, destination.Y, out moveDest);

            float distance = (float)Math.Sqrt(Math.Pow(moveDest.X - moveOrig.X, 2) + Math.Pow(moveDest.Y - moveOrig.Y, 2));
            xSpeed = (moveDest.X - moveOrig.X) / distance;
            ySpeed = (moveDest.Y - moveOrig.Y) / distance;

            return true;
            #endregion
        }
        #endregion

        #region GetTileCost
        public int GetTileCost(HexTile currentTile, HexTile destinationTile)
        {
            int tileCost = 0;
            if (unitType == UnitType.Walker)
            {
                switch (destinationTile.HexType)
                {
                    case HexType.Clear:
                    case HexType.Rough:
                    case HexType.Woodland:
                        tileCost = 1;
                        break;
                    case HexType.Sand:
                    case HexType.Jungle:
                    case HexType.Water:
                        tileCost = 2;
                        break;
                    case HexType.Swamp:
                        tileCost = 3;
                        break;
                }
                if (destinationTile.Elevation - currentTile.Elevation == 1)
                    tileCost += 2;
                else if (currentTile.Elevation - destinationTile.Elevation == 1)
                    tileCost += 1;
            }
            
            if (unitType == UnitType.Ground || unitType == UnitType.Infantry)
            {
                switch (destinationTile.HexType)
                {
                    case HexType.Clear:
                        tileCost = 1;
                        break;
                    case HexType.Rough:
                    case HexType.Woodland:
                    case HexType.Sand:
                        tileCost = 2;
                        break;
                    case HexType.Jungle:
                    case HexType.Water:
                        tileCost = 3;
                        break;
                    case HexType.Swamp:
                        tileCost = 4;
                        break;
                }
                if (destinationTile.Elevation - currentTile.Elevation == 1)
                    tileCost += 2;
            }
            
            if (unitType == UnitType.Hover)
            {
                switch (destinationTile.HexType)
                {
                    case HexType.Clear:
                    case HexType.Rough:
                    case HexType.Sand:
                    case HexType.Swamp:
                    case HexType.Water:
                        tileCost = 1;
                        break;
                    case HexType.Jungle:
                    case HexType.Woodland:
                        tileCost = 2;
                        break;
                }
                if (destinationTile.Elevation - currentTile.Elevation == 1)
                    tileCost += 4;
            }

            return tileCost;
        }
        #endregion

        #region Attack
        public virtual void Attack(Unit target) 
        {
            attacking = true;
            StartAnimation(AnimationType.Fire);
            targetedUnit = target;
        }
        #endregion

        #region CheckDefenseArc
        /// <summary>
        /// Checks to see what quadrant the attacker is in compared to the defender
        /// </summary>
        /// <param name="attackerFacing">Facing of the attacker</param>
        /// <param name="targetFacing">Facing of the target</param>
        /// <param name="attackerPosition">Position of the attacker</param>
        /// <param name="targetPosition">Position of the target</param>
        /// <returns>0 for front, -1 for rear flank, -2 for rear</returns>
        public int CheckDefenseArc(float facing, Point attackerPosition, Point targetPosition)
        {
            int targetFacing = (int)Math.Round(MathHelper.ToDegrees(facing));

            int dY = targetPosition.Y - attackerPosition.Y;
            int dX = targetPosition.X - attackerPosition.X;

            if (targetPosition.X == attackerPosition.X)
            {
                //in a vertical line with target
                if (targetPosition.Y < attackerPosition.Y)
                {
                    if (targetFacing == FacingInt.North)
                        return -2;

                    if (targetFacing == FacingInt.NorthEast || targetFacing == FacingInt.NorthWest)
                        return -1;
                }
                else if (targetPosition.Y > attackerPosition.Y)
                {
                    if (targetFacing == FacingInt.South)
                        return -2;

                    if (targetFacing == FacingInt.SouthEast || targetFacing == FacingInt.SouthWest)
                        return -1;
                }
            }

            if (targetPosition.X - attackerPosition.X > 0)
            {
                //target is to the right of attacker
                if (targetPosition.Y < attackerPosition.Y)
                {
                    //target is to the right and above
                    if (targetFacing == FacingInt.NorthEast)
                        return -2;

                    if (targetFacing == FacingInt.North)
                        return -1;
                }
                else if (targetPosition.Y > attackerPosition.Y)
                {
                    //target is to the right and below
                    if (targetFacing == FacingInt.SouthEast)
                        return -2;

                    if (targetFacing == FacingInt.South)
                        return -1;
                }
            }

            if (attackerPosition.X  - targetPosition.X > 0)
            {
                //target is to the left of attacker
                if (targetPosition.Y < attackerPosition.Y)
                {
                    //target is to the left and above
                    if (targetFacing == FacingInt.NorthWest)
                        return -2;

                    if (targetFacing == FacingInt.North)
                        return -1;
                }
                else if (targetPosition.Y > attackerPosition.Y)
                {
                    //target is to the left and below
                    if (targetFacing == FacingInt.SouthWest)
                        return -2;

                    if (targetFacing == FacingInt.South)
                        return -1;
                }
            }

            return 0;
        }
        #endregion

        #region StartAnimation
        public void StartAnimation(AnimationType animationType)
        {
            animationIndex = (int)animationType;
            animation[(int)animationType].Start();
        }
        #endregion

        #region ApplyDamage
        public virtual void ApplyDamage(DamageType damage)
        {
            if ((int)damage < 0)
                return;

            if (damage == DamageType.Overkill)
                Destroy();
            else
                StartAnimation(AnimationType.Hit);
        }
        public virtual void ApplyDamage(int damage)
        {
            return;
        }
        #endregion

        #region Destroy
        public void Destroy()
        {
            StartAnimation(AnimationType.Destroyed);
            destroyed = true;

            HeavyGearManager.Player(playerIndex).Squads[squadIndex].MoraleCheck(true);
        }
        #endregion

        #region Determine Angle
        private int DetermineAngle(Point origin, Point target)
        {
            int angle = 0;
            int dX = target.X - origin.X;
            int dY = origin.Y - target.Y;
            if (dX == 0)
            {
                // if dx = 0 and dy = 1, move up and left
                if (dY == 1)
                {
                    if (origin.Y % 2 > 0)
                        angle = FacingInt.NorthWest;
                    else
                        angle = FacingInt.NorthEast;
                }
                else if (dY >= 2)
                    angle = FacingInt.North;
                else if (dY == -1)
                {
                    if (origin.Y % 2 > 0)
                        angle = FacingInt.SouthWest;
                    else
                        angle = FacingInt.SouthEast;
                }
                else
                    angle = FacingInt.South;
            }
            else if (dX > 0)
            {
                // Right movement
                if (dY > 0)
                    angle = FacingInt.NorthEast;
                else if (dY == 0)
                {
                    if (origin.Y == 0)
                        angle = FacingInt.SouthEast;
                    else
                        angle = FacingInt.NorthEast;
                }
                else
                    angle = FacingInt.SouthEast;
            }
            else
            {
                //Left movement
                if (dY > 0)
                    angle = FacingInt.NorthWest;
                else if (dY == 0)
                {
                    if (origin.Y == 0)
                        angle = FacingInt.SouthWest;
                    else
                        angle = FacingInt.NorthWest;
                }
                else
                    angle = FacingInt.SouthWest;
            }

            return angle;
        }
        
        #endregion
    }
}
