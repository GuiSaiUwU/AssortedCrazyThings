﻿using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;







//TODO use that akhult jump over ledge thing for the movement






namespace AssortedCrazyThings.NPCs.DungeonBird
{
    public abstract class BaseHarvester : ModNPC
    {
        public const short EatTimeConst = 120; //shouldnt be equal to idleTime - 5
        public const short IdleTimeConst = 180; //shouldnt be equal to idleTime

        protected void Print(string msg)
        {
            if (Main.netMode == NetmodeID.Server)
            {
                Console.WriteLine(msg);
            }

            if (Main.netMode == NetmodeID.MultiplayerClient || Main.netMode == NetmodeID.SinglePlayer)
            {
                Main.NewText(msg);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (npc.life <= 0)
            {
            }
        }

        public static string name = "aaaHarvester";
        protected static int AI_State_Slot = 0;
        protected static int AI_X_Timer_Slot = 1;
        protected static int AI_Y_Slot = 2;
        protected static int AI_Timer_Slot = 3;
        protected static bool Target_Player = false;
        protected static bool Target_Soul = true;

        protected static float State_Distribute = 0f;
        protected static float State_Approach = 1f;
        protected static float State_Noclip = 2f;
        //protected static float State_IdleMove = 3f;
        //protected static float State_Recalculate = 4f;
        protected static float State_Stop = 5f;
        protected static float State_Transform = 6f;

        protected float maxVeloScale; //2f default //
        protected float maxAccScale; //0.07f default
        protected byte stuckTime; //*30 for ticks, *0.5 for seconds
        protected byte afterEatTime;
        protected short eatTime;
        protected short idleTime;
        protected short hungerTime; //AI_Timer
        protected byte maxSoulsEaten;
        protected short jumpRange; //also noclip detect range //100 for restricted v
        public bool restrictedSoulSearch;
        //those above are all "static", cant make em static in 

        public byte soulsEaten;
        public short stopTime;
        public bool aiTargetType;
        public short target;
        public byte stuckTimer;
        public byte rndJump;
        public bool transformServer;
        public int transformTo;

        public float AI_State
        {
            get
            {
                return npc.ai[AI_State_Slot];
            }
            set
            {
                npc.ai[AI_State_Slot] = value;
            }
        }

        public float AI_X_Timer
        {
            get
            {
                return npc.ai[AI_X_Timer_Slot];
            }
            set
            {
                npc.ai[AI_X_Timer_Slot] = value;
            }
        }

        public float AI_Y
        {
            get
            {
                return npc.ai[AI_Y_Slot];
            }
            set
            {
                npc.ai[AI_Y_Slot] = value;
            }
        }

        public float AI_Timer
        {
            get
            {
                return npc.ai[AI_Timer_Slot];
            }
            set
            {
                npc.ai[AI_Timer_Slot] = value;
            }
        }

        public float AI_Local_Timer
        {
            get
            {
                return npc.localAI[0];
            }
            set
            {
                npc.localAI[0] = value;
            }
        }

        public float AI_Init
        {
            get
            {
                return npc.localAI[1];
            }
            set
            {
                npc.localAI[1] = value;
            }
        }

        protected int SelectTarget(bool restricted = false)
        {
            if (aiTargetType == Target_Soul)
            {
                target = SoulTargetClosest(restricted);
                if (target == 200)
                {
                    AI_State = State_Stop;
                }
            }
            else if (aiTargetType == Target_Player) //Target_Player
            {
                npc.TargetClosest();
                target = (short)npc.target;
            }
            return target;
        }

        protected short SoulTargetClosest(bool restrictedvar = false)
        {
            short closest = 200;
            Vector2 soulPos = Vector2.Zero;
            float oldDistance = 1000000000f;
            float newDistance = oldDistance;
            //return index of closest soul
            for (short j = 0; j < 200; j++)
            {
                if (Main.npc[j].active && Main.npc[j].type == mod.NPCType(AssWorld.soulName))
                {
                    soulPos = Main.npc[j].Center - npc.Center;
                    newDistance = soulPos.Length();
                    if (newDistance < oldDistance && ((restrictedvar? (soulPos.Y > -jumpRange) : true) || Collision.CanHit(npc.Center, 1, 1, Main.npc[j].Center, 1, 1)))
                    {
                        oldDistance = newDistance;
                        closest = j;
                    }
                }
            }
            //NEED TO CATCH "==200" WHEN CALLING THIS 
            if(closest != 200) Main.NewText("res " + restrictedvar + " soulpos " + soulPos.Y + " rang " + (-jumpRange) + " " + Main.npc[closest].TypeName);
            return closest; //to self
        }

        protected Entity GetTarget()
        {
            if (aiTargetType == Target_Soul)
            {
                return Main.npc[target];
            }
            else //Target_Player
            {
                return Main.player[target];
            }
        }

        protected bool IsTargetActive()
        {
            return GetTarget().active;
        }

        protected void SetTimeLeft()
        {
            //only for Soul
            if (aiTargetType == Target_Soul)
            {
                NPC tar = (NPC)GetTarget();
                if (tar.active && tar.type == mod.NPCType(AssWorld.soulName) && Math.Abs(tar.Center.X - npc.Center.X) < 5f) //type check since souls might despawn and index changes
                {
                    tar.timeLeft = (int)EatTimeConst;
                    tar.netUpdate = true;
                }
            }
        }

        protected void PassCoordinates(Entity ent)
        {
            AI_X_Timer = ent.Center.X;
            AI_Y = ent.Center.Y - 8f; //buffer up
        }

        protected void KillInstantly(NPC npc)
        {
            npc.life = 0;
            npc.active = false;
        }

        protected void UpdateStuck(bool closeToSoulvar, bool allowNoclipvar)
        {
            Vector2 between = new Vector2(0f, GetTarget().Center.Y - npc.Center.Y);
            //collideY isnt proper when its on ledges/halfbricks
            if (Main.time % 30 == 0 &&
                (npc.collideX || (npc.collideY || (npc.velocity.Y == 0 || npc.velocity.Y < 2f && npc.velocity.Y > 0f))) &&
                !closeToSoulvar &&
                (!Collision.CanHit(npc.Center, 1, 1, GetTarget().Center, 1, 1) || between.Y > 0f || between.Y <= -jumpRange))
            {

                //Main.NewText("TICK TOCK " + npc.collideX + " " + npc.collideY);
                between = new Vector2(Math.Abs(npc.Center.X - AI_X_Timer), Math.Abs(npc.Center.Y - AI_Y));
                //twice a second, diff is max 39f
                if (between.Y > 100f || between.X > 35f)
                {
                    npc.netUpdate = true;
                    Print("NOT stuck actually");
                    stuckTimer = 0;
                }
                else if (between.Y <= 100f)
                {
                    if (between.X <= 35f)
                    {
                        Print("stucktimer++");
                        stuckTimer++;
                        npc.netUpdate = true;
                    }
                }
                if (stuckTimer >= stuckTime)
                {
                    if (allowNoclipvar)
                    {
                        npc.netUpdate = true;
                        Print("noclipping");
                        //Main.NewText("DOOR STUCK");
                        PassCoordinates(GetTarget());
                        AI_State = State_Noclip; //pass targets X/Y to noclip
                    }
                    else
                    {
                        KillInstantly((NPC)GetTarget());
                    }
                    stuckTimer = 0;
                    return;
                }
            }
            if (Main.time % 30 == 0) //do these always
            {
                AI_X_Timer = npc.Center.X;
                AI_Y = npc.Center.Y;
            }
        }

        protected bool UpdateVelocity()
        {
            Vector2 between = new Vector2(Math.Abs(GetTarget().Center.X - npc.Center.X), GetTarget().Center.Y - npc.Center.Y);
            bool lockedX = false;
            if (between.X < 2f && Collision.CanHit(npc.Center - new Vector2(4f, 4f), 8, 8, GetTarget().Center - new Vector2(4f, 4f), 8, 8) && between.Y <= 0f && between.Y > -jumpRange)
            {
                //actually only locked when direct LOS and not too high
                //npc.position.X = GetTarget().Center.X - GetTarget().width; //centered on the center of the soul
                Print("set lockedX");
                lockedX = true;
            }

            float veloScale = maxVeloScale; //2f default
            float accScale = maxAccScale; //0.07f default

            //VELOCITY CALCULATIONS HERE
            if (!lockedX)
            {

                if (between.X < 50f && Math.Abs(between.Y) < 24f)
                {
                    veloScale = maxVeloScale * 0.4f; //when literally near the soul
                }

                if (npc.velocity.X < -veloScale || npc.velocity.X > veloScale)
                {
                    if (npc.velocity.Y == 0f)
                    {
                        npc.velocity *= 0.7f;
                    }
                }
                else if (npc.velocity.X < veloScale && npc.direction == 1)
                {
                    npc.velocity.X += accScale;
                    if (npc.velocity.X > veloScale)
                    {
                        npc.velocity.X = veloScale;
                    }
                }
                else if (npc.velocity.X > -veloScale && npc.direction == -1)
                {
                    npc.velocity.X -= accScale;
                    if (npc.velocity.X < -veloScale)
                    {
                        npc.velocity.X = -veloScale;
                    }
                }
            }
            else
            {
                npc.velocity.X = Vector2.Zero.X;
                //  if on ground || if on downward slope
                if ((npc.velocity.Y == 0 || npc.velocity.Y < 2f && npc.velocity.Y > 0f) && between.Y < -32f) //jump when below two tiles
                {
                    //Main.NewText("jump to get to soul");
                    npc.velocity.Y = (float)(Math.Sqrt((double)-between.Y) * -0.84f);
                    npc.netUpdate = true;
                }
                npc.direction = -1;
                //go to eat mode
                stopTime = eatTime;
                AI_State = State_Distribute;
            }
            return lockedX;
        }

        protected void UpdateOtherMovement(bool flag3var)
        {
            bool flag22 = false;
            //might scrap it idk
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (Main.time % 60 == 1 && (npc.velocity.X == 0f || (npc.velocity.Y > -3f && npc.velocity.Y < 3f)))
                {
                    rndJump = (byte)Main.rand.Next(5, 8);
                    //if (rndJump >= 7)
                    //{
                    //    rndJump = 0;
                    //}
                    npc.netUpdate = true;
                }
            }
            //



            if (npc.velocity.Y == 0f)
            {
                int num178 = (int)(npc.position.Y + (float)npc.height + 7f) / 16;
                int num179 = (int)npc.position.X / 16;
                int num180 = (int)(npc.position.X + (float)npc.width) / 16;
                int num28;
                for (int num181 = num179; num181 <= num180; num181 = num28 + 1)
                {
                    if (Main.tile[num181, num178] == null)
                    {
                        return;
                    }
                    if (Main.tile[num181, num178].nactive() && Main.tileSolid[Main.tile[num181, num178].type])
                    {
                        flag22 = true;
                        break;
                    }
                    num28 = num181;
                }
            }
            if (npc.velocity.Y >= 0f)
            {
                int num182 = 0;
                if (npc.velocity.X < 0f)
                {
                    num182 = -1;
                }
                if (npc.velocity.X > 0f)
                {
                    num182 = 1;
                }
                Vector2 position2 = npc.position;
                position2.X += npc.velocity.X;
                int num183 = (int)((position2.X + (float)(npc.width / 2) + (float)((npc.width / 2 + 1) * num182)) / 16f);
                int num184 = (int)((position2.Y + (float)npc.height - 1f) / 16f);
                if (Main.tile[num183, num184] == null)
                {
                    Tile[,] tile3 = Main.tile;
                    int num185 = num183;
                    int num186 = num184;
                    Tile tile4 = new Tile();
                    tile3[num185, num186] = tile4;
                }
                if (Main.tile[num183, num184 - 1] == null)
                {
                    Tile[,] tile5 = Main.tile;
                    int num187 = num183;
                    int num188 = num184 - 1;
                    Tile tile6 = new Tile();
                    tile5[num187, num188] = tile6;
                }
                if (Main.tile[num183, num184 - 2] == null)
                {
                    Tile[,] tile7 = Main.tile;
                    int num189 = num183;
                    int num190 = num184 - 2;
                    Tile tile8 = new Tile();
                    tile7[num189, num190] = tile8;
                }
                if (Main.tile[num183, num184 - 3] == null)
                {
                    Tile[,] tile9 = Main.tile;
                    int num191 = num183;
                    int num192 = num184 - 3;
                    Tile tile10 = new Tile();
                    tile9[num191, num192] = tile10;
                }
                if (Main.tile[num183, num184 + 1] == null)
                {
                    Tile[,] tile11 = Main.tile;
                    int num193 = num183;
                    int num194 = num184 + 1;
                    Tile tile12 = new Tile();
                    tile11[num193, num194] = tile12;
                }
                if (Main.tile[num183 - num182, num184 - 3] == null)
                {
                    Tile[,] tile13 = Main.tile;
                    int num195 = num183 - num182;
                    int num196 = num184 - 3;
                    Tile tile14 = new Tile();
                    tile13[num195, num196] = tile14;
                }
                if ((float)(num183 * 16) < position2.X + (float)npc.width &&
                    (float)(num183 * 16 + 16) > position2.X &&
                    ((Main.tile[num183, num184].nactive() &&
                    !Main.tile[num183, num184].topSlope() &&
                    !Main.tile[num183, num184 - 1].topSlope() &&
                    Main.tileSolid[Main.tile[num183, num184].type] &&
                    !Main.tileSolidTop[Main.tile[num183, num184].type]) ||
                    (Main.tile[num183, num184 - 1].halfBrick() &&
                    Main.tile[num183, num184 - 1].nactive())) &&

                    (!Main.tile[num183, num184 - 1].nactive() ||
                    !Main.tileSolid[Main.tile[num183, num184 - 1].type] ||
                    Main.tileSolidTop[Main.tile[num183, num184 - 1].type] ||
                    (Main.tile[num183, num184 - 1].halfBrick() &&
                    (!Main.tile[num183, num184 - 4].nactive() ||
                    !Main.tileSolid[Main.tile[num183, num184 - 4].type] ||
                    Main.tileSolidTop[Main.tile[num183, num184 - 4].type]))) &&

                    (!Main.tile[num183, num184 - 2].nactive() ||
                    !Main.tileSolid[Main.tile[num183, num184 - 2].type] ||
                    Main.tileSolidTop[Main.tile[num183, num184 - 2].type]) &&

                    (!Main.tile[num183, num184 - 3].nactive() ||
                    !Main.tileSolid[Main.tile[num183, num184 - 3].type] ||
                    Main.tileSolidTop[Main.tile[num183, num184 - 3].type]) &&

                    (!Main.tile[num183 - num182, num184 - 3].nactive() ||
                    !Main.tileSolid[Main.tile[num183 - num182, num184 - 3].type]))
                {
                    float num197 = (float)(num184 * 16);
                    if (Main.tile[num183, num184].halfBrick())
                    {
                        num197 += 8f;
                    }
                    if (Main.tile[num183, num184 - 1].halfBrick())
                    {
                        num197 -= 8f;
                    }
                    if (num197 < position2.Y + (float)npc.height)
                    {
                        float num198 = position2.Y + (float)npc.height - num197;
                        float num199 = 16.1f;
                        if (num198 <= num199)
                        {

                            //go up slopes/halfbricks
                            npc.gfxOffY += npc.position.Y + (float)npc.height - num197;
                            npc.position.Y = num197 - (float)npc.height;
                            if (num198 < 9f)
                            {
                                npc.stepSpeed = 1f;
                            }
                            else
                            {
                                npc.stepSpeed = 2f;
                            }
                        }
                    }
                }
            }
            if (flag22)
            {
                int num200 = 0;
                int num201 = 0;
                if (1 == 1)
                {
                    num200 = (int)((npc.position.X + (float)(npc.width / 2) + (float)(15 * npc.direction)) / 16f);
                    num201 = (int)((npc.position.Y + (float)npc.height - 15f) / 16f);
                    if (Main.tile[num200, num201] == null)
                    {
                        Tile[,] tile15 = Main.tile;
                        int num202 = num200;
                        int num203 = num201;
                        Tile tile16 = new Tile();
                        tile15[num202, num203] = tile16;
                    }
                    if (Main.tile[num200, num201 - 1] == null)
                    {
                        Tile[,] tile17 = Main.tile;
                        int num204 = num200;
                        int num205 = num201 - 1;
                        Tile tile18 = new Tile();
                        tile17[num204, num205] = tile18;
                    }
                    if (Main.tile[num200, num201 - 2] == null)
                    {
                        Tile[,] tile19 = Main.tile;
                        int num206 = num200;
                        int num207 = num201 - 2;
                        Tile tile20 = new Tile();
                        tile19[num206, num207] = tile20;
                    }
                    if (Main.tile[num200, num201 - 3] == null)
                    {
                        Tile[,] tile21 = Main.tile;
                        int num208 = num200;
                        int num209 = num201 - 3;
                        Tile tile22 = new Tile();
                        tile21[num208, num209] = tile22;
                    }
                    if (Main.tile[num200, num201 + 1] == null)
                    {
                        Tile[,] tile23 = Main.tile;
                        int num210 = num200;
                        int num211 = num201 + 1;
                        Tile tile24 = new Tile();
                        tile23[num210, num211] = tile24;
                    }
                    if (Main.tile[num200 + npc.direction, num201 - 1] == null)
                    {
                        Tile[,] tile25 = Main.tile;
                        int num212 = num200 + npc.direction;
                        int num213 = num201 - 1;
                        Tile tile26 = new Tile();
                        tile25[num212, num213] = tile26;
                    }
                    if (Main.tile[num200 + npc.direction, num201 + 1] == null)
                    {
                        Tile[,] tile27 = Main.tile;
                        int num214 = num200 + npc.direction;
                        int num215 = num201 + 1;
                        Tile tile28 = new Tile();
                        tile27[num214, num215] = tile28;
                    }
                    if (Main.tile[num200 - npc.direction, num201 + 1] == null)
                    {
                        Tile[,] tile29 = Main.tile;
                        int num216 = num200 - npc.direction;
                        int num217 = num201 + 1;
                        Tile tile30 = new Tile();
                        tile29[num216, num217] = tile30;
                    }
                    Main.tile[num200, num201 + 1].halfBrick();
                }

                //adjusted here

                //int num200 = (int)((npc.position.X + (float)(npc.width / 2) + (float)(15 * npc.direction)) / 16f);
                //int num201 = (int)((npc.position.Y + (float)npc.height - 15f) / 16f);
                if (!(Main.tile[num200, num201 - 1].nactive() && (TileLoader.IsClosedDoor(Main.tile[num200, num201 - 1]) || Main.tile[num200, num201 - 1].type == 388)))
                {
                    //Main.NewText("" + num200 + " " + num201);
                    if ((npc.velocity.X < 0f && npc.spriteDirection == -1) || (npc.velocity.X > 0f && npc.spriteDirection == 1))
                    {
                        if (1 == 2)
                        {
                            //if (npc.height >= 32 && Main.tile[num200, num201 - 2].nactive() && Main.tileSolid[Main.tile[num200, num201 - 2].type])
                            //{
                            //    if (Main.tile[num200, num201 - 3].nactive() && Main.tileSolid[Main.tile[num200, num201 - 3].type])
                            //    {
                            //        Main.NewText("1111");
                            //        npc.velocity.Y = -8f;
                            //        npc.netUpdate = true;
                            //    }
                            //    else
                            //    {
                            //        Main.NewText("2222");
                            //        npc.velocity.Y = -7f;
                            //        npc.netUpdate = true;
                            //    }
                            //}
                            //else if (Main.tile[num200, num201 - 1].nactive() && Main.tileSolid[Main.tile[num200, num201 - 1].type])
                            //{
                            //    Main.NewText("3333");
                            //    npc.velocity.Y = -6f;
                            //    npc.netUpdate = true;
                            //}
                            //else if (npc.position.Y + (float)npc.height - (float)(num201 * 16) > 20f && Main.tile[num200, num201].nactive() && !Main.tile[num200, num201].topSlope() && Main.tileSolid[Main.tile[num200, num201].type])
                            //{
                            //    Main.NewText("4444");
                            //    npc.velocity.Y = -5f;
                            //    npc.netUpdate = true;
                            //}
                            //else if (npc.directionY < 0 && (!Main.tile[num200, num201 + 1].nactive() || !Main.tileSolid[Main.tile[num200, num201 + 1].type]) && (!Main.tile[num200 + npc.direction, num201 + 1].nactive() || !Main.tileSolid[Main.tile[num200 + npc.direction, num201 + 1].type]))
                            //{
                            //    //this is for when player stands on an elevation and it just jumped aswell
                            //    Main.NewText("5555");
                            //    npc.velocity.Y = -8f;
                            //    npc.velocity.X *= 1.5f;
                            //    npc.netUpdate = true;
                            //}
                            //if (npc.velocity.Y == 0f && flag3 && false/* && aiFighter == 1f*/)
                            //{
                            //    Main.NewText("6666");
                            //    npc.velocity.Y = -5f;
                            //}
                        }


                        //heck this ima do this MY way

                        if (/*npc.velocity.Y == 0f && */flag3var)
                        {
                            if (Main.time % 60 == 35)
                            {
                                npc.velocity.Y = -(float)rndJump - 0.5f;
                            }
                        }
                    }

                    //this bit is for "frantically jump when close to the player
                    //if (npc.velocity.Y == 0f && Math.Abs(npc.position.X + (float)(npc.width / 2) - (Main.player[npc.target].position.X + (float)(Main.player[npc.target].width / 2))) < 100f && Math.Abs(npc.position.Y + (float)(npc.height / 2) - (Main.player[npc.target].position.Y + (float)(Main.player[npc.target].height / 2))) < 50f && ((npc.direction > 0 && npc.velocity.X >= 1f) || (npc.direction < 0 && npc.velocity.X <= -1f)))
                    //{
                    //    npc.velocity.X *= 2f;
                    //    if (npc.velocity.X > 3f)
                    //    {
                    //        npc.velocity.X = 3f;
                    //    }
                    //    if (npc.velocity.X < -3f)
                    //    {
                    //        npc.velocity.X = -3f;
                    //    }
                    //    npc.velocity.Y = -4f;
                    //    npc.netUpdate = true;
                    //}
                }
            }
        }

        protected void HarvesterAIGround(bool allowNoclip = true)
        {
            //if(npc.velocity.Y != 0f) Main.NewText("veloy " + npc.velocity.Y); //use that in findframe to animate the wings


            bool flag3 = false;
            bool closeToSoul = false;
            //bool flag4 = false;

            if (npc.velocity.X == 0f)
            {
                flag3 = true;
            }
            if (npc.justHit)
            {
                flag3 = false;
            }

            //if (AI_Init > 0) //1
            //{
            //    stuckTimer = 0;
            //    AI_X_Timer = npc.position.X;
            //    AI_Y = npc.position.Y;

            //    if (npc.direction == 0)
            //    {
            //        npc.direction = 1;
            //    }
            //    AI_Init++; //0
            //}
            //
            if (true/*&& AI_Timer < (float)aiFighterLimit*/)
            {
                if (Main.time % 60 == 0)
                {
                    Main.NewText("test " + restrictedSoulSearch);
                    SelectTarget(restrictedSoulSearch);
                    Print("soulseaten:" + soulsEaten);
                }
                //npc.TargetClosest();
                //Main.NewText("" + SelectTarget(restrictedSoulSearch) + " " + npc.target); //if p, automatically in npc.target
                //GetTarget();
            }

            if (!IsTargetActive())
            {
                stopTime = idleTime;
                AI_State = State_Stop;
                //npc.velocity = Vector2.Zero;
                return;
            }
            else
            {
                Vector2 between = GetTarget().Center - npc.Center;
                if (between.Length() < 20f)
                {
                    AI_Timer = 0f; //when literally near the soul
                    closeToSoul = true; //used to prevent the stuck timer to run
                }
                npc.direction = (between.X <= 0f) ? -1 : 1;
            }

            //if (npc.velocity.Y == 0f && ((npc.velocity.X > 0f && npc.direction < 0) || (npc.velocity.X < 0f && npc.direction > 0)))
            //{
            //    flag4 = true;
            //}
            //if ((npc.position.X == npc.oldPosition.X || AI_Timer >= (float)hungerTimeLimit) | flag4)
            //{
            //    AI_Timer += 1f;
            //}
            ////else if (/*(double)Math.Abs(npc.velocity.X) > (veloScale*0.4f) && */AI_Timer > 0f) //0.9
            ////{
            ////    //near 
            ////    //AI_Timer -= 1f;
            ////}
            //if (AI_Timer > (float)(hungerTimeLimit * 2))
            //{
            //    AI_Timer = 0f;
            //}

            //hungerTimer
            AI_Timer++;
            if (npc.justHit)
            {
                AI_Timer = 0f;
            }
            if (AI_Timer >= hungerTime)
            {
                AI_Timer = 0f;
                //goto noclip 
                PassCoordinates(GetTarget());
                AI_State = State_Noclip; //this is needed in order for the harvester to keep progressing
                npc.netUpdate = true;
            }

            UpdateStuck(closeToSoul, allowNoclip);

            //if not locked, do othermovement
            if (!UpdateVelocity()) UpdateOtherMovement(flag3);

        }

        protected void HarvesterAI(bool allowNoclip = true)
        {
            //Collision.CanHit == is there direct line of sight from a to b
            npc.noGravity = false;
            npc.noTileCollide = false;
            npc.dontTakeDamage = false;

            if (AI_Init == 0)
            {
                //initialize it to go for souls first
                aiTargetType = Target_Soul;
                stopTime = idleTime;
                SelectTarget(restrictedSoulSearch);
                AI_Init = 1;
                AI_Local_Timer = 0f;
            }

            if (AI_Local_Timer < afterEatTime)
            {
                AI_Local_Timer++;
                return;
            }

            if (transformServer)
            {
                Print("allowed to transform");
                //go to transform state and return
                transformServer = false;
            }

            if (!(AI_State == State_Noclip))
            {
                if(!IsTargetActive())
                {
                    if (aiTargetType == Target_Soul)
                    {
                        stopTime = idleTime;
                    }
                    else //if target is player, its eating anyways (to prevent it from resetting because of target switch)
                    {
                        stopTime = eatTime;
                    }
                    //AI_X_Timer = 0f;
                    AI_State = State_Stop;
                }
                
                AI_Timer++;
                if (npc.justHit)
                {
                    AI_Timer = 0f;
                }
                if (AI_Timer >= hungerTime)
                {
                    AI_Timer = 0f;
                    //goto noclip 
                    PassCoordinates(GetTarget());
                    AI_State = State_Noclip; //this is needed in order for the harvester to keep progressing
                    npc.netUpdate = true;
                }
            }

            if (AI_State == State_Distribute/*0*/)
            {
                SelectTarget(restrictedSoulSearch);

                if (stopTime == eatTime &&
                    (npc.velocity.Y == 0 || (npc.velocity.Y < 2f && npc.velocity.Y > 0f)) && //still (or on slope)
                    (GetTarget().velocity.Y == 0 || (GetTarget().velocity.Y < 2f && GetTarget().velocity.Y > 0f))//still (or on slope)
                                                                                                                 /*&& !Collision.SolidCollision(npc.position, npc.width, npc.height)*/)
                {
                    //sitting on soul, go into stop/eat mode
                    if (npc.velocity.Y != 0)
                    {
                        int num200 = (int)((npc.position.X + (float)(npc.width / 2) + (float)(15 * npc.direction)) / 16f);
                        int num201 = (int)((npc.position.Y + (float)npc.height - 15f) / 16f);
                        if (Main.tile[num200 + npc.direction, num201 + 1] == null)
                        {
                            Tile[,] tile27 = Main.tile;
                            int num214 = num200 + npc.direction;
                            int num215 = num201 + 1;
                            Tile tile28 = new Tile();
                            tile27[num214, num215] = tile28;
                        }
                        if (Main.tile[num200 - npc.direction, num201 + 1] == null)
                        {
                            Tile[,] tile29 = Main.tile;
                            int num216 = num200 - npc.direction;
                            int num217 = num201 + 1;
                            Tile tile30 = new Tile();
                            tile29[num216, num217] = tile30;
                        }
                        Main.tile[num200, num201 + 1].halfBrick();
                    }

                    npc.netUpdate = true;

                    Print("distribute to stop");
                    SetTimeLeft();
                    aiTargetType = Target_Player;
                    SelectTarget(restrictedSoulSearch); //now player

                    AI_X_Timer = 0f;
                    AI_State = State_Stop; //start to eat
                }
                else if (stopTime != eatTime)
                {
                    npc.netUpdate = true;

                    //go into regular mode
                    PassCoordinates(npc);
                    stuckTimer = 0;
                    if (npc.direction == 0)
                    {
                        npc.direction = 1;
                    }
                    AI_State = State_Approach;
                }
                else//keep state
                {
                    AI_State = State_Distribute;
                }
            }
            else if (AI_State == State_Approach/*1*/)
            {
                HarvesterAIGround(allowNoclip);
            }
            else if (AI_State == State_Noclip/*2*/)
            {
                npc.noGravity = true;
                npc.noTileCollide = true;
                Vector2 between = new Vector2(AI_X_Timer - npc.Center.X, AI_Y - npc.Center.Y); //use latest known position from UpdateStuck of target
                float distance = between.Length();
                float factor = 3f; //2f
                int acc = 40; //4
                between.Normalize();
                between *= factor;
                npc.velocity = (npc.velocity * (acc - 1) + between) / acc;
                //concider only the bottom half of the hitbox (plus a small bit below)
                if (distance < 16f /*600f*/ && !Collision.SolidCollision(npc.position + new Vector2(0f, npc.height / 2), npc.width, npc.height / 2 + 4))
                {
                    npc.netUpdate = true;
                    AI_State = State_Distribute;
                }
            }
            else if (3 == 4 /*idlemove, recalculate*/)
            {
                //else if (AI_State == State_IdleMove/*3*/)
                //{
                //    npc.noGravity = false;
                //    //player unreachable now but distance less than 800f, gets parameters AI_X_Timer, AI_Y passed by State_Distribute and State_Recalculate
                //    //Main.NewText("unreachable");
                //    Vector2 value42 = new Vector2(AI_X_Timer, AI_Y);
                //    Vector2 between = value42 - npc.Center;
                //    float distance = between.Length();
                //    //distance == distance from the X axis of the player
                //    float factor = 1f;
                //    float acc = 3f;
                //    between.Normalize();
                //    between *= factor;
                //    npc.velocity = (npc.velocity * (acc - 1f) + between) / acc;
                //    if (npc.collideX || npc.collideY)
                //    {
                //        //just collided
                //        AI_State = State_Recalculate;
                //        AI_X_Timer = 0f;
                //    }
                //    if (distance < factor || distance > 800f || Collision.CanHit(npc.Center, 1, 1, GetTarget().Center, 1, 1))
                //    {
                //        AI_State = State_Distribute;
                //    }
                //}
                //else if (AI_State == State_Recalculate/*4*/)
                //{
                //    npc.noGravity = false;
                //    //just collided
                //    //Main.NewText("just collided");
                //    Vector2 between = GetTarget().Center - npc.Center;
                //    npc.direction = (between.X < 0f) ? -1 : 1;
                //    if (npc.collideX)
                //    {
                //        if (npc.velocity.X < 0.3f && npc.direction == -1)
                //        {
                //            npc.velocity.X = -0.3f;
                //        }
                //        else if (npc.velocity.X > -0.3f && npc.direction == 1)
                //        {
                //            npc.velocity.X = 0.3f;
                //        }
                //        npc.velocity.X = npc.velocity.X * -0.8f;
                //    }
                //    if (npc.collideY)
                //    {
                //        npc.velocity.Y = npc.velocity.Y * -0.8f;
                //    }
                //    Vector2 vel;
                //    if (npc.velocity.X == 0f && npc.velocity.Y == 0f)
                //    {
                //        vel = GetTarget().Center - npc.Center;
                //        vel.Y -= (float)(GetTarget().height / 4);
                //        vel.Normalize();
                //        npc.velocity = vel * 0.1f;
                //    }
                //    float scaleFactor21 = 1.5f; //1.5f
                //    float acc = 20f; //20f
                //    vel = npc.velocity;
                //    vel.Normalize();
                //    vel *= scaleFactor21;
                //    npc.velocity = (npc.velocity * (acc - 1f) + vel) / acc;
                //    AI_X_Timer += 1f;
                //    if (AI_X_Timer > 180f)
                //    {
                //        AI_State = State_Distribute;
                //        AI_X_Timer = 0f;
                //    }
                //    if (Collision.CanHit(npc.Center, 1, 1, GetTarget().Center, 1, 1))
                //    {
                //        AI_State = State_Distribute;
                //    }
                //    AI_Local_Timer += 1f;
                //    //Adjust these values in the if() depending on your NPCs dimensions (Granite elemental is 20x30)
                //    if (AI_Local_Timer >= 5f && !Collision.SolidCollision(npc.position, npc.width, npc.height))
                //    {
                //        AI_Local_Timer = 0f;
                //        Vector2 centerAlignedX = npc.Center;
                //        centerAlignedX.X = GetTarget().Center.X;
                //        if (Collision.CanHit(npc.Center, 1, 1, centerAlignedX, 1, 1) && Collision.CanHit(GetTarget().Center, 1, 1, centerAlignedX, 1, 1))
                //        {
                //            //player unreachable now
                //            AI_State = State_IdleMove;
                //            AI_X_Timer = centerAlignedX.X;
                //            AI_Y = centerAlignedX.Y;
                //        }
                //        else
                //        {
                //            Vector2 centerAlignedY = npc.Center;
                //            centerAlignedY.Y = GetTarget().Center.Y;
                //            if (Collision.CanHit(npc.Center, 1, 1, centerAlignedY, 1, 1) && Collision.CanHit(GetTarget().Center, 1, 1, centerAlignedY, 1, 1))
                //            {
                //                //player unreachable now
                //                AI_State = State_IdleMove;
                //                AI_X_Timer = centerAlignedY.X;
                //                AI_Y = centerAlignedY.Y;
                //            }
                //        }
                //    }
                //}
            }
            else if (AI_State == State_Stop/*5*/)
            {

                if (AI_X_Timer == 0f && stopTime == eatTime)
                {
                    Main.NewText("started eating");
                    npc.netUpdate = true;
                }
                npc.noGravity = false;

                npc.velocity.X = 0f;
                AI_X_Timer += 1f;
                if (AI_X_Timer > stopTime)
                {
                    npc.netUpdate = true;
                    AI_State = State_Distribute;
                    AI_X_Timer = 0f;

                    if (stopTime == idleTime)
                    {
                        SelectTarget(restrictedSoulSearch); //to retarget for the IsActive check (otherwise it gets stuck in this state)
                    }
                    else if (stopTime == eatTime)
                    {
                        //soul eaten and soul still there: initialize transformation
                        Print("finished eating");
                        AI_Init = 0; //reinitialize
                        soulsEaten++;

                        if (soulsEaten >= maxSoulsEaten)
                        {
                            Print("souls eaten max reached");
                            soulsEaten = 0;
                            //AI_State = State_Transform;
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                transformServer = true;
                            }
                        }
                        //else{AI_State = State_Stop;}
                        AI_State = State_Stop;
                    }
                }

                if (Main.netMode != NetmodeID.MultiplayerClient && AI_X_Timer % 60 == 0 && aiTargetType == Target_Player && soulsEaten <= maxSoulsEaten)
                {
                    Rectangle rect = new Rectangle((int)npc.Center.X - 200, (int)npc.Center.Y - 200, npc.width + 200 * 2, npc.height + 200 * 2);
                    if (rect.Intersects(new Rectangle((int)GetTarget().position.X, (int)GetTarget().position.Y, GetTarget().width, GetTarget().height)) &&
                        (Collision.CanHit(npc.Center, 1, 1, GetTarget().Center, 1, 1) || (GetTarget().Center.Y - npc.Center.Y) <= 0f))
                    {
                        Vector2 between = GetTarget().Center - npc.Center;
                        float factor = 7f;
                        between.Normalize();
                        between *= factor;
                        Projectile.NewProjectile(npc.Center + new Vector2(0f, -npc.height / 4), between, ProjectileID.SkeletonBone, 5, 0f, Main.myPlayer);
                    }
                }
            }
            else if (AI_State == State_Transform/*6*/)
            {
                //if (AI_X_Timer == 0f && stopTime == eatTime)
                //{
                //    resting = true;
                //}
                //else if (stopTime == idleTime) resting = false;
                //if (resting) npc.noGravity = false;

                //npc.velocity = Vector2.Zero;
                //AI_X_Timer += 1f;
                //if (AI_X_Timer > stopTime)
                //{
                //    AI_State = State_Distribute;
                //    AI_X_Timer = 0f;

                //    if (stopTime == idleTime)
                //    {
                //        SelectTarget(restrictedSoulSearch);
                //    }
                //    else if (stopTime == eatTime)
                //    {
                //        //soul eaten and soul still there: initialize transformation
                //        Main.NewText("finished eating");
                //        //if the "catch souls to deny" idea works out then add a check during timer increment if souls is alive, and increment a saturation counter
                //        stopTime = idleTime;
                //        resting = false;
                //    }
                //}
            }
        }

        public virtual void Transform(int to)
        {
            KillInstantly(npc);
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int type = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, to);
                if (Main.netMode == NetmodeID.Server && type < 200)
                {
                    NetMessage.SendData(23, -1, -1, null, type);
                }
            }
        }

        public override void AI()
        {
            //HarvesterAI(allowNoclip: true);
            //if (transformServer) Transform(transformTo);
        }
    }
}
