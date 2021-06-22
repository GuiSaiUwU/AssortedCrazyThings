﻿using System.ComponentModel;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace AssortedCrazyThings
{
    /// <summary>
    /// The class name is what is shown in the filename as AssortedCrazyThings_Config
    /// </summary>
    public class Config : ServerConfigBase
	{
		public static Config Instance => ModContent.GetInstance<Config>();

		/// <summary>
		/// Affects the way Cute Slimes spawn and how the Jellied Ale works
		/// </summary>
		[DefaultValue(true)]
        [Label("Cute Slimes Potion Only")]
        [Tooltip("Affects the way Cute Slimes spawn and how the Jellied Ale works")]
        public bool CuteSlimesPotionOnly;

        /// <summary>
        /// Enable/Disable Walking Tombstones spawning
        /// </summary>
        [DefaultValue(true)]
        [Label("Walking Tombstones")]
        [Tooltip("Enable/Disable Walking Tombstone spawning")]
        public bool WalkingTombstones;
	}

	public class AConfigurationConfig : ServerConfigBase
	{
		public static AConfigurationConfig Instance => ModContent.GetInstance<AConfigurationConfig>();

		internal ContentType FilterFlags { private set; get; }

		[Header("NPCs")]

		[ReloadRequired]
		[DefaultValue(true)]
		[Tooltip("This will also disable all their relevant content")]
		[Label("Bosses")]
		public bool Bosses { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		[Tooltip("This will also disable all their relevant content")]
		[Label("Hostile NPCs")]
		public bool HostileNPCs { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		[Tooltip("This will also disable all their relevant content")]
		[Label("Friendly NPCs")]
		public bool FriendlyNPCs { get; set; }

		[Header("Items")]

		[ReloadRequired]
		[DefaultValue(true)]
		[Tooltip("Disable pet items that drop from various NPCs")]
		[Label("Dropped Pets")]
		public bool DroppedPets { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		[Tooltip("Disable items that drop for failing to defeat a vanilla boss within 5 attempts")]
		[Label("Boss Consolation")]
		public bool BossConsolation { get; set; }

        public override void OnChanged()
        {
			//Inverted, sets a flag if toggle is false
			FilterFlags = ContentType.Always;

			if (!Bosses)
			{
				FilterFlags |= ContentType.Bosses;
			}
			if (!HostileNPCs)
			{
				FilterFlags |= ContentType.HostileNPCs;
			}
			if (!FriendlyNPCs)
			{
				FilterFlags |= ContentType.FriendlyNPCs;
			}
			if (!DroppedPets)
			{
				FilterFlags |= ContentType.DroppedPets;
			}
			if (!BossConsolation)
			{
				FilterFlags |= ContentType.BossConsolation;
			}
		}
    }

	public abstract class ServerConfigBase : ModConfig
    {
		public override ConfigScope Mode => ConfigScope.ServerSide;

		public static bool IsPlayerLocalServerOwner(int whoAmI)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return Netplay.Connection.Socket.GetRemoteAddress().IsLocalHost();
			}

			for (int i = 0; i < Main.maxPlayers; i++)
			{
				RemoteClient client = Netplay.Clients[i];
				if (client.State == 10 && i == whoAmI && client.Socket.GetRemoteAddress().IsLocalHost())
				{
					return true;
				}
			}
			return false;
		}

		public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) return true;
			else if (!IsPlayerLocalServerOwner(whoAmI))
			{
				message = "You are not the server owner so you can not change this config";
				return false;
			}
			return base.AcceptClientChanges(pendingConfig, whoAmI, ref message);
		}
	}
}
