using DNA.CastleMinerZ.Globalization;
using DNA.Drawing.UI;
using DNA.Drawing.UI.Controls;
using Microsoft.Xna.Framework;
using System;

namespace DNA.CastleMinerZ.UI
{
	public class ReleaseNotesScreen : CreditsScreen
	{
		public ReleaseNotesScreen(CastleMinerZGame game, string versionString)
			: base(game._largeFont, game._medLargeFont, game._medFont, false)
		{
			ImageButtonControl imageButtonControl = new ImageButtonControl();
			imageButtonControl.Image = game._uiSprites["BackArrow"];
			imageButtonControl.Font = game._medFont;
			imageButtonControl.LocalPosition = new Point(32, 32);
			imageButtonControl.Pressed += _backButton_Pressed;
			imageButtonControl.Text = " " + Strings.Back;
			imageButtonControl.ImageDefaultColor = new Color(CMZColors.MenuGreen.ToVector4() * 0.8f);
			base.Controls.Add(imageButtonControl);
			AddCreditsItem("Castleminer Z " + versionString + " Release Notes", ItemTypes.Title);
			AddCreditsItem(" ");
			AddCreditsItem("1.9.6", ItemTypes.Header);
			AddCreditsItem("Added Lootblocks! These glowing blocks come in two versions, common (blue) and rare (gold).");
			AddCreditsItem("Gold loot blocks require a gold pickaxe or higher to open.");
			AddCreditsItem("New Scavenger game mode! Scavenger mode follows Survival mode rules, but with plentiful loot blocks for rewarding exploration.");
			AddCreditsItem("Survival mode has fewer loot blocks while Endurance modes have none.");
			AddCreditsItem("Fixed spawn beacons to work under ground.");
			AddCreditsItem("Increased block stack size to 999.");
			AddCreditsItem(" ");
			AddCreditsItem("1.9.5", ItemTypes.Header);
			AddCreditsItem("Stamina adjustments from beta feedback.");
			AddCreditsItem("You cannot use stamina in either Endurance mode.");
			AddCreditsItem("Whenever you use stamina, there is a cooldown before it regenerates.");
			AddCreditsItem("Reduced Sprint speed (previously 2.5x, now 2x).");
			AddCreditsItem("Removed the cost for single jumps and raised double jump cost from 15% to 25%.");
			AddCreditsItem(" ");
			AddCreditsItem("1.9.4", ItemTypes.Header);
			AddCreditsItem("New Stamina system!");
			AddCreditsItem("Added Sprint by holding Left Shift.");
			AddCreditsItem("Added Double jump by pressing space twice.");
			AddCreditsItem("Increased the distance you can safely fall before taking damage.");
			AddCreditsItem(" ");
			AddCreditsItem("1.9.3", ItemTypes.Header);
			AddCreditsItem("Fixed Exploration mode to work with hosting online.");
			AddCreditsItem("Using Jump while Flying will now move you directly upward.");
			AddCreditsItem(" ");
			AddCreditsItem("1.9.2", ItemTypes.Header);
			AddCreditsItem("Creative mode has been upgraded! Three major changes to this mode:");
			AddCreditsItem(" - You can craft anything without needing resources. Build whatever you want!");
			AddCreditsItem(" - Dying no longer drops anything from your inventory.");
			AddCreditsItem(" - Press F to toggle Fly mode.");
			AddCreditsItem("Added a second Lantern model for some additional variety.");
			AddCreditsItem("Added a unique model for the Laser Drill.");
			AddCreditsItem("Fixed a bug that destroyed all stacks of Recall Beacons when placing one.");
			AddCreditsItem(" ");
			AddCreditsItem("1.8.8", ItemTypes.Header);
			AddCreditsItem("Added a Laser Drill! This modified Laser rifle will cut through dirt and stone but leave valuable resources unharmed and ready to collect.");
			AddCreditsItem(" ");
			AddCreditsItem("1.8.7", ItemTypes.Header);
			AddCreditsItem("Fixed glass and respawn beacons to be diggable. This will not change your respawn location.");
			AddCreditsItem("Fixed inventory art for explosives and rocket launchers.");
			AddCreditsItem(" ");
			AddCreditsItem("1.8.5", ItemTypes.Header);
			AddCreditsItem("Added a Respawn Beacon. Place this to set where you spawn after dying!");
			AddCreditsItem("Added two sub menus to the crafting menu: Containers and Spawn Points.");
			AddCreditsItem("Added a variety of new crates to help organize your base.");
			AddCreditsItem("Added window recipes the Other Structures menu.");
			AddCreditsItem("Reduced the dust when digging and the smoke on explosions for better visibility.");
			AddCreditsItem("Lowered the cost of crafting Stone pickaxes for a smoother starting experience.");
			AddCreditsItem(" ");
			AddCreditsItem("1.8.1", ItemTypes.Header);
			AddCreditsItem("Fix to make sure sticky grenades don't stick to the owner's hand. That hurt!");
			AddCreditsItem(" ");
			AddCreditsItem("1.8.0", ItemTypes.Header);
			AddCreditsItem("Added Sticky Grenades! Great for precision mining or making sure a zombie don't get away.");
			AddCreditsItem("Moved Grenades, TNT and C4 recipes to a new Explosives menu.");
			AddCreditsItem("Spooky! Grenades have transformed into Jack-o-lanterns for Halloween!");
			AddCreditsItem("Made tooltips show up faster.");
			AddCreditsItem(" ");
			AddCreditsItem("1.6.1", ItemTypes.Header);
			AddCreditsItem("Fixed laser sword crafting bug");
			AddCreditsItem("Made LMG stronger");
			AddCreditsItem("Zombies can't dig through hardest materials");
			AddCreditsItem(" ");
			AddCreditsItem("1.6", ItemTypes.Header);
			AddCreditsItem("Added PVP");
			AddCreditsItem("Added new gun: LMG");
			AddCreditsItem("Added laser sword");
			AddCreditsItem("C4 and TNT can be removed with shovels");
			AddCreditsItem("Zombies dig through blocks");
			AddCreditsItem(" ");
			AddCreditsItem("1.5.4", ItemTypes.Header);
			AddCreditsItem("Fixed Auto Climb bugs");
			AddCreditsItem("Added Auto Climb Option To Settings");
			AddCreditsItem(" ");
			AddCreditsItem("1.5.3", ItemTypes.Header);
			AddCreditsItem("Players can walk up blocks without jumping");
			AddCreditsItem("Inventory is no longer lost when disconnected");
			AddCreditsItem(" ");
			AddCreditsItem("1.5.2", ItemTypes.Header);
			AddCreditsItem("Fixed laser weapons being the wrong colors.");
			AddCreditsItem("Fixed issues with the game freezing in the menus periodically.");
			AddCreditsItem("Fixed the flashing blue line that appeared on SDTVs.");
			AddCreditsItem("Huge performance enhancements.");
			AddCreditsItem(" ");
			AddCreditsItem("1.5.1", ItemTypes.Header);
			AddCreditsItem("Fixed performance issue when close to many torches.");
			AddCreditsItem("Fixed exploit that allows you to access crates from anywhere.");
			AddCreditsItem("Fixed lighting issue caused by large TNT explosions.");
			AddCreditsItem(" ");
			AddCreditsItem("1.5", ItemTypes.Header);
			AddCreditsItem("Added metor crash biome, they start appearing randomly after about distance 300.");
			AddCreditsItem("Added new blocks Space Rock and Space Goo, used to make sci-fi tools and weapons");
			AddCreditsItem("Added new alien enemy which will defend Space Goo.");
			AddCreditsItem("Added new laser weapons, laser weapons remove some blocks when shoot.");
			AddCreditsItem("Added explosives TNT,C4 can be set off by hitting or pressing B, also shooting.");
			AddCreditsItem("Added grenades, grenades can be cooked by holding right trigger.");
			AddCreditsItem("Added RPG launcher.");
			AddCreditsItem("Added dragon killing AA rocket which locks on dragons.");
			AddCreditsItem("Added Explosive Powder needed to make explosive items, dropped by dragons.");
			AddCreditsItem("Dragons are now MUCH harder to kill");
			AddCreditsItem("Demons are much harder to kill and will now start to spawn above ground, if you have been to hell.");
			AddCreditsItem("Completely reworked sound engine, sounds are now more realistic outdoors and in caves.");
			AddCreditsItem("Changed spark effect");
			AddCreditsItem(" ");
			AddCreditsItem("1.4.6", ItemTypes.Header);
			AddCreditsItem("Added Release Notes");
			AddCreditsItem("Minor Bug Fixes");
			AddCreditsItem(" ");
			AddCreditsItem("1.4.5", ItemTypes.Header);
			AddCreditsItem("Max players has been increased from 4 to 8");
			AddCreditsItem("Framerate has been nearly doubled");
			AddCreditsItem("Load Times have been cut in half");
			AddCreditsItem("Added some detailed descriptions to the menus");
			AddCreditsItem("Added a code for Avatar Laser Wars 2, CMZ users will get a full weapon unlock in Avatar Laser Wars 2");
			AddCreditsItem("Fixed the problem with Invisible Avatars.");
			AddCreditsItem(" ");
			AddCreditsItem("1.4.3", ItemTypes.Header);
			AddCreditsItem("Fixed problem where people would start with no inventory");
			AddCreditsItem("Fixed problem where all music would play after you spawn/teleport");
			AddCreditsItem("Fixed incorrect Menu descriptions on Mode Menu");
			AddCreditsItem(" ");
			AddCreditsItem("1.4.2", ItemTypes.Header);
			AddCreditsItem("Brightness Setting");
			AddCreditsItem("Sound Settings");
			AddCreditsItem("Controller Sensitivity Setting");
			AddCreditsItem("Zombies make much less noise");
			AddCreditsItem("Music is not as loud");
			AddCreditsItem("Axe melee damage has been decreased, and is slightly more expensive to make");
			AddCreditsItem("Better Error Handling");
			AddCreditsItem("Lava is breakable");
			AddCreditsItem("'Free Build' has been removed, instead you can play survival with 'No Enemies'");
			AddCreditsItem("'Hard' difficulty has been renamed to 'Normal'");
			AddCreditsItem("Default difficulty is now 'Normal'");
			AddCreditsItem("Original Creator of a world now shows up in browser, even if it is copied");
			AddCreditsItem("Made Promo Codes More User friendly");
			AddCreditsItem("In Hardcore mode, you no longer start with anything (get ready to punch trees)");
			AddCreditsItem("Option to delete storage in game");
			AddCreditsItem("Overall difficulty is reduced, especially in the distance");
			AddCreditsItem(" ");
			AddCreditsItem("1.4.1", ItemTypes.Header);
			AddCreditsItem("Fixed Promo codes not working in network games");
			AddCreditsItem("Creative mode can now be played online");
			AddCreditsItem("Teleporters don't teleport you to your death if they are not set");
			AddCreditsItem("Fixed some duplication bugs");
			AddCreditsItem("Removed the original CastleMiner code from the menu ");
			AddCreditsItem(" ");
			AddCreditsItem("1.4.0", ItemTypes.Header);
			AddCreditsItem("Creative Mode -  Infinite ammo and blocks");
			AddCreditsItem("4 New Biome Music Tracks");
			AddCreditsItem("Other players worlds are automatically deleted when the game starts");
			AddCreditsItem("Animation blending");
			AddCreditsItem("Nav Aids: A settable compass  or ???Locator???, and a Teleporter ");
			AddCreditsItem("New Hardcore Mode");
			AddCreditsItem("Dragons now give more resources when killed");
			AddCreditsItem("New Enemy: Demon");
			AddCreditsItem("More duping Fixes");
			AddCreditsItem("Infinite Ammo Fix");
			AddCreditsItem(" ");
			AddCreditsItem("1.3.1", ItemTypes.Header);
			AddCreditsItem("Fixed dragon spawn problems.");
			AddCreditsItem("Joining a game with the wrong version no longer crashes it");
			AddCreditsItem("Can no longer attach things to doors. i.e. torches");
			AddCreditsItem("Days now tally correctly for acheivements");
			AddCreditsItem("Undead Dragon much easier to find");
			AddCreditsItem("Dragons spawn much faster in Dragon Endurance");
			AddCreditsItem("Fixed another duplication glitch");
			AddCreditsItem("Improved Network performance slightly");
			AddCreditsItem("Added wait screen to the delete world, so people don't think their Xbox locked up");
			AddCreditsItem("Fixed Stat issues, gave all old players free Dragon Endurance unlock");
			AddCreditsItem("Fixed a bug with recipes not showing up in the crafting menu");
			AddCreditsItem(" ");
			AddCreditsItem("1.3.0", ItemTypes.Header);
			AddCreditsItem("Doors... you got em");
			AddCreditsItem("Dragon Endurance Mode");
			AddCreditsItem("Made shotguns spread less");
			AddCreditsItem("Rebalanced the knifes, most are slower and far less durable");
			AddCreditsItem("Removed Crosshairs on Shouldered Weapons");
			AddCreditsItem("Build Cursor doesn't show up if you can't build. i.e. you are too close to the spot");
			AddCreditsItem("Decreased mining particles");
			AddCreditsItem("Fixed more Duping bugs");
			AddCreditsItem("Fixed crash when bringing up crafting screen with empty inventory");
			AddCreditsItem("Fixed nullReference crash when joining a network game");
			AddCreditsItem("Fixed Incorrect Dragon Kill Messages");
			AddCreditsItem("More Skeletons underground");
			AddCreditsItem(" ");
			AddCreditsItem("1.2.1", ItemTypes.Header);
			AddCreditsItem("'The Descent' Biome as been restored");
			AddCreditsItem("Dragon spawn distances have been adjusted");
			AddCreditsItem("Fixed shotgun no spread bug");
			AddCreditsItem("Fixed recipes don't show up bug");
			AddCreditsItem("Show game difficuly on session browser");
			AddCreditsItem("Made all ores slightly richer");
			AddCreditsItem("Crate Duplication Bug Fixed");
			AddCreditsItem("Xbox Notifications have be relocated on the Screen");
			AddCreditsItem("Shotgun quick Reload fix");
			AddCreditsItem("Players now notifed of dragon kills");
			AddCreditsItem("'Y' will quick transfer between crates/inventory");
			AddCreditsItem(" ");
			AddCreditsItem("1.2.0", ItemTypes.Header);
			AddCreditsItem("Dragons!");
			AddCreditsItem("Fixed Torch Bug");
			AddCreditsItem("Reworked Ore Distributions");
			AddCreditsItem("Adjusted some recipies slightly");
			AddCreditsItem("Made Zombie Drops more generous");
			AddCreditsItem("Added Difficulty Setting for Survival");
			AddCreditsItem("Fixed problems with splitting items in crates (crashes 'Chainsaw' items, auto heals, etc)");
			AddCreditsItem("Fixed zoom problems when switching guns in zoomed states");
			AddCreditsItem(" ");
			AddCreditsItem("1.1.0", ItemTypes.Header);
			AddCreditsItem("Storage Crates");
			AddCreditsItem("Monsters now have Shadows");
			AddCreditsItem("You can split stacks with the Right stick in the inventory screen (up down or press for half)");
			AddCreditsItem("Compass now works right if you look at it in someone else's hand");
			AddCreditsItem("You can now mine snow");
			AddCreditsItem("Survival Mode is now hard again");
			AddCreditsItem("Iron is less common. But still more than 1.0");
			AddCreditsItem("Fixed the AR code. (Sorry Guys)");
			AddCreditsItem("Player Inventory now saves in online games too");
			AddCreditsItem("Transition Music, day counters now appear in all modes");
			AddCreditsItem("Decreased Drop rates on Zombies( killing shouldn't replace mining.)");
			AddCreditsItem("Achievement and stats are only counted in Endurance");
			AddCreditsItem("You can no longer join games of different versions through invites");
			AddCreditsItem("Days now reset when your party dies in Endurance");
			AddCreditsItem("B Button now 'Activates' items, such as crates");
			AddCreditsItem("Invincible Grey Zombies can now be killed");
			AddCreditsItem("Removed annoying purple flash when the game starts");
			AddCreditsItem("Improved rendering performance");
			AddCreditsItem("Improved Monster Lighting and detail");
			AddCreditsItem("You can now Mine Ice");
			AddCreditsItem("Hell is Much harder");
			AddCreditsItem("Changed Drops for Zombies, zombies now drop wood.");
			AddCreditsItem("Item duping exploit fixed");
			AddCreditsItem("Pick sounds can no longer be heard everywhere");
			AddCreditsItem(" ");
			AddCreditsItem("1.0.1", ItemTypes.Header);
			AddCreditsItem("On screen direction damage indicator");
			AddCreditsItem("Inventory now saves in Survival, and Free Mode");
			AddCreditsItem("Torches now show up when you reload a world");
			AddCreditsItem("Lightning only lights up the sky instead of the whole screen");
			AddCreditsItem("Menu now pauses single player games");
			AddCreditsItem("Left Trigger now drops blocks as well as right");
			AddCreditsItem("Blocks no longer get dropped if you don't have a square highlighted");
			AddCreditsItem("Fixed bug where Iron picks didn't give you diamonds");
			AddCreditsItem("Fixed bug where some weapon icons weren't showing up in crafting screen");
			AddCreditsItem("Removed 'No Fall Mode' people were just getting stuck running away from zombies");
			AddCreditsItem("Fixed bug where walls could not be removed with picks");
			AddCreditsItem("Crafting UI now shows ALL recipes needed to make something, even if there are multiple steps");
			AddCreditsItem("Crafting UI improved, now clicking on a component takes you to the recipe to make it");
			AddCreditsItem("Survival Mode is now significantly easier i.e no more daytime zombies. Endurance is still very hard");
			AddCreditsItem("Pickups stay around longer");
			AddCreditsItem("Fixed bug where you could repair items for free by passing them to a friend");
			AddCreditsItem("Achievement status screen. Shows all Acheivements");
			AddCreditsItem("Changes to weapon damage values. Pretty Much made everything stronger");
			AddCreditsItem("Knives do much more damge, now take use damage");
			AddCreditsItem("No more Zombies in Free Build");
			AddCreditsItem("Copper Gold and Iron, Ore are slightly more common");
			AddCreditsItem("Fixed bug where Guns of different types were not different colors");
		}

		private void _backButton_Pressed(object sender, EventArgs e)
		{
			PopMe();
		}
	}
}
