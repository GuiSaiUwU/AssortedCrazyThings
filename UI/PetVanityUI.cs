﻿using Terraria.UI;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.UI.Chat;
using Microsoft.Xna.Framework.Graphics;
using System;
using AssortedCrazyThings.Items.PetAccessories;
using AssortedCrazyThings.Base;

namespace AssortedCrazyThings.UI
{
    class PetVanityUI : UIState
    {
        internal const int NONE = -1;

        internal const int IGNORE = -2;

        /// <summary>
        /// Circle diameter
        /// </summary>
        internal const int mainDiameter = 36;

        /// <summary>
        /// Circle radius
        /// </summary>
        internal const int mainRadius = mainDiameter / 2;

        /// <summary>
        /// Is the UI visible?
        /// </summary>
        internal static bool visible = false;

        /// <summary>
        /// Spawn position, i.e. mouse position at UI start
        /// </summary>
        internal static Vector2 spawnPosition = default(Vector2);

        /// <summary>
        /// If pet currently has something of that type in that slot
        /// </summary>
        internal static bool hasEquipped = false;

        /// <summary>
        /// Which thing is currently highlighted?
        /// </summary>
        internal static int returned = NONE;

        /// <summary>
        /// Fade in animation when opening the UI
        /// </summary>
        internal static float fadeIn = 0;

        /// <summary>
        /// Red cross for when to unequip
        /// </summary>
        internal static Texture2D redCrossTexture;

        /// <summary>
        /// Holds data about what to draw
        /// </summary>
        internal static PetAccessory petAccessory;

        /// <summary>
        /// Spawn position offset to top left corner of that to draw the icons
        /// </summary>
        private Vector2 TopLeftCorner
        {
            get
            {
                return spawnPosition - new Vector2(mainRadius, mainRadius);
            }
        }

        //Initialization
        public override void OnInitialize()
        {
            redCrossTexture = AssUtils.Instance.GetTexture("UI/UIRedCross");
        }

        //Update, unused
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        //Draw
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            Main.LocalPlayer.mouseInterface = true;

            //48
            int outerRadius = 48;
            if (petAccessory.AltTextures.Count > 5) outerRadius += 5 * (petAccessory.AltTextures.Count - 5); //increase by 5 after having more than 5 options, starts getting clumped at about 24 circles
            if (fadeIn < outerRadius) outerRadius = (int)(fadeIn += (float)outerRadius / 10);

            double angleSteps = 2.0d / petAccessory.AltTextures.Count;
            int done;
            //done --> ID of currently drawn circle
            for (done = 0; done < petAccessory.AltTextures.Count; done++)
            {
                double x = outerRadius * Math.Sin(angleSteps * done * Math.PI);
                double y = outerRadius * -Math.Cos(angleSteps * done * Math.PI);


                Rectangle bgRect = new Rectangle((int)(TopLeftCorner.X + x), (int)(TopLeftCorner.Y + y), mainDiameter, mainDiameter);
                //Check if mouse is within the circle checked
                bool isMouseWithin = CircleUI.CheckMouseWithinWheel(Main.MouseScreen, spawnPosition, mainRadius, petAccessory.AltTextures.Count, done);

                //Actually draw the bg circle
                Color drawColor = Color.White;
                if (hasEquipped && done == petAccessory.Color)
                {
                    drawColor = Color.Gray;
                }
                spriteBatch.Draw(Main.wireUITexture[isMouseWithin ? 1 : 0], bgRect, drawColor);

                //Draw sprites over the icons
                int width = petAccessory.AltTextures[done].Width;
                int height = petAccessory.AltTextures[done].Height;
                Rectangle projRect = new Rectangle((int)(spawnPosition.X + x) - (width / 2), (int)(spawnPosition.Y + y) - (height / 2), width, height);

                drawColor = Color.White;
                if (hasEquipped && done == petAccessory.Color) drawColor = Color.Gray;

                spriteBatch.Draw(petAccessory.AltTextures[done], projRect, petAccessory.AltTextures[done].Bounds, drawColor);

                if (isMouseWithin)
                {
                    //set the "returned" new type
                    returned = done;
                    //In UpdatePetVanityUI(): else if (returned == IGNORE) {nothing happens}
                    if (hasEquipped && done == petAccessory.Color) returned = IGNORE;
                }
            }

            //Draw held item bg circle
            Rectangle outputRect = new Rectangle((int)TopLeftCorner.X, (int)TopLeftCorner.Y, mainDiameter, mainDiameter);

            bool middle = CircleUI.CheckMouseWithinCircle(Main.MouseScreen, mainRadius, spawnPosition);

            spriteBatch.Draw(Main.wireUITexture[middle ? 1 : 0], outputRect, Color.White);

            //Draw held item inside circle
            if (petAccessory.Type != -1)
            {
                int finalWidth = Main.itemTexture[petAccessory.Type].Width;
                int finalHeight = Main.itemTexture[petAccessory.Type].Height;
                Rectangle outputWeaponRect = new Rectangle((int)spawnPosition.X - (finalWidth / 2), (int)spawnPosition.Y - (finalHeight / 2), finalWidth, finalHeight);
                //outputWeaponRect.Inflate(4, 4);
                spriteBatch.Draw(Main.itemTexture[petAccessory.Type], outputWeaponRect, Color.White);
            }

            if (middle)
            {
                //if hovering over the middle, reset color
                returned = NONE;
                if (hasEquipped)
                {
                    //Draw the red cross
                    int finalWidth = redCrossTexture.Width;
                    int finalHeight = redCrossTexture.Height;
                    Rectangle outputCrossRect = new Rectangle((int)spawnPosition.X - (finalWidth / 2), (int)spawnPosition.Y - (finalHeight / 2), finalWidth, finalHeight);
                    spriteBatch.Draw(redCrossTexture, outputCrossRect, Color.White);

                    //Draw the tooltip
                    Color fontColor = Color.White;
                    Vector2 mousePos = new Vector2(Main.mouseX, Main.mouseY);
                    ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, "Unequip", mousePos + new Vector2(16, 16), fontColor, 0, Vector2.Zero, Vector2.One);
                }
            }

            //extra loop so tooltips are always drawn after the circles
            for (done = 0; done < petAccessory.AltTextures.Count; done++)
            {
                bool isMouseWithin = CircleUI.CheckMouseWithinWheel(Main.MouseScreen, spawnPosition, mainRadius, petAccessory.AltTextures.Count, done);
                string tooltip = petAccessory.AltTextureSuffixes[done];

                if (isMouseWithin)
                {
                    //Draw the tooltip
                    Color fontColor = Color.White;
                    Vector2 mousePos = new Vector2(Main.mouseX, Main.mouseY);
                    ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, tooltip, mousePos + new Vector2(16, 16), fontColor, 0, Vector2.Zero, Vector2.One);
                }
            }
        }

        /// <summary>
        /// Called when the UI is about to appear
        /// </summary>
        public static void Start(PetAccessory pAccessory)
        {
            visible = true;
            spawnPosition = Main.MouseScreen;
            petAccessory = pAccessory;
            PetPlayer pPlayer = Main.LocalPlayer.GetModPlayer<PetPlayer>();
            PetAccessory equipped = pPlayer.GetAccessoryInSlot((byte)petAccessory.Slot);
            hasEquipped = equipped != null && equipped.Type == petAccessory.Type;
            fadeIn = 0;
        }
    }
}
