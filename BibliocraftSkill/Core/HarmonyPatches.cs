using System;
using System.Collections.Generic;
using HarmonyLib;
using Microsoft.Xna.Framework;
using MoonShared;
using SpaceCore;
using StardewValley;
using StardewValley.Extensions;
using Object = StardewValley.Object;
using System.Linq;
using StardewValley.Buffs;
using BirbCore.Attributes;
using StardewValley.TerrainFeatures;
using Log = BirbCore.Attributes.Log;
using StardewValley.GameData.WildTrees;
using xTile.Dimensions;
using xTile.Tiles;
using StardewValley.Monsters;
using System.Numerics;
using System.Threading;
using System.Runtime.Intrinsics;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using StardewValley.Objects.Trinkets;
using StardewValley.GameData.GarbageCans;
using StardewValley.Internal;
using StardewValley.Tools;

namespace BibliocraftSkill
{

    [HarmonyPatch(typeof(Object), "readBook")]
    class ReadBookPostfix_patch
    {


        public static KeyedProfession Prof_BookWorm => Book_Skill.Book5a;
        public static KeyedProfession Prof_PageFinder => Book_Skill.Book5b;
        public static KeyedProfession Prof_PageMaster => Book_Skill.Book10a1;
        public static KeyedProfession Prof_SeasonedReader => Book_Skill.Book10a2;
        public static KeyedProfession Prof_TypeSetter => Book_Skill.Book10b1;
        public static KeyedProfession Prof_BookSeller => Book_Skill.Book10b2;



        [HarmonyLib.HarmonyPostfix]
        public static void Postfix(Object __instance, ref GameLocation location)
        {
            //Get the player
            var who = Game1.GetPlayer(Game1.player.UniqueMultiplayerID);
            int playerBookLevel = Utilities.GetLevel(who);
            string itemID = __instance.ItemId;

            Utilities.AddEXP(who, 50);


            // If the player went down the bookworm profession path,
            if (who.HasCustomProfession(Prof_BookWorm))
            {

                // Define constants for attribute types and max values
                const int NumAttributes = 10;
                const int MaxAttributeValue = 5;
                const int MaxStaminaMultiplier = 16;
                const int BuffDurationMultiplier = (6000 * 10);

                // Create the buff
                Buff buff = new(
                    id: "Bibliocraft:profession:bookworm_buff",
                    displayName: ModEntry.Instance.I18N.Get("moonslime.Bibliocraft.Profession10b2.buff"),
                    description: null,
                    iconTexture: ModEntry.Assets.Bookworm_buff,
                    iconSheetIndex: 0,
                    duration: BuffDurationMultiplier * Utilities.GetLevel(who), //Buff duration based on player Cooking level, to reward them for eating cooking foods
                    effects: null
                );

                //Chekck to see if the player has the buff
                if (!who.hasBuff(buff.id))
                {
                    // Generate random attribute and level
                    int attributeBuff = Game1.random.Next(1, NumAttributes + 1);
                    Log.Trace("Bibliocraft: random attibute is: " + attributeBuff.ToString());
                    int attributeLevel = Game1.random.Next(1, MaxAttributeValue + 1);
                    Log.Trace("Bibliocraft: random level is: " + attributeLevel.ToString());

                    // Create a BuffEffects instance
                    BuffEffects randomEffect = new()
                    {
                        FarmingLevel = { 0 },
                        FishingLevel = { 0 },
                        MiningLevel = { 0 },
                        LuckLevel = { 0 },
                        ForagingLevel = { 0 },
                        MaxStamina = { 0 },
                        MagneticRadius = { 0 },
                        Defense = { 0 },
                        Attack = { 0 },
                        Speed = { 0 }
                    };


                    // Apply the random effect based on the randomly generated attribute
                    switch (attributeBuff)
                    {
                        case 1: randomEffect.FarmingLevel.Value = attributeLevel; break;
                        case 2: randomEffect.FishingLevel.Value = attributeLevel; break;
                        case 3: randomEffect.MiningLevel.Value = attributeLevel; break;
                        case 4: randomEffect.LuckLevel.Value = attributeLevel; break;
                        case 5: randomEffect.ForagingLevel.Value = attributeLevel; break;
                        case 6: randomEffect.MaxStamina.Value = attributeLevel * MaxStaminaMultiplier; break;
                        case 7: randomEffect.MagneticRadius.Value = attributeLevel * MaxStaminaMultiplier; break;
                        case 8: randomEffect.Defense.Value = attributeLevel; break;
                        case 9: randomEffect.Attack.Value = attributeLevel; break;
                        case 10: randomEffect.Speed.Value = attributeLevel; break;
                    }

                    //Apply the effects to the buff
                    buff.effects.Add(randomEffect);

                    //Apply the new buff
                    who.applyBuff(buff);
                }

                //Profession page master
                if (who.HasCustomProfession(Prof_PageMaster))
                {
                    // use the explode feature!
                    location.explode(who.Tile, //location
                        (int)(playerBookLevel * 0.5), // radius
                        who, // who did the explosion
                        false, // it does not damage players
                        Game1.random.Choose(26, 27, 28, 29, 30, 31, 32, 33, 34), // the damage it deals
                        true); // It does not destroy objects

                    location.playSound("wind");
                }
                //Profession seasoned reader
                if (who.HasCustomProfession(Prof_SeasonedReader))
                {
                    //get a list of the tiles affected
                    List<Vector2> list = TilesAffected(who.Tile, (int)(playerBookLevel * 0.3), who);

                    // check each tile for the crops
                    foreach (var entry in location.terrainFeatures.Pairs.ToList())
                    {
                        var tf = entry.Value;
                        // If the object in hoedirt and is on the list
                        if (tf is HoeDirt dirt && list.Contains(entry.Key))
                        {
                            // continue if there is no crop or if the crop is fully grown
                            if (dirt.crop == null || dirt.crop.fullyGrown.Value)
                                continue;
                            // If it does contain a a crop, advance the crop for one day
                            dirt.crop.newDay(1);
                            location.updateMap();
                        }
                    }
                }
            }

            #region EXP increase when reading books

            //Increase the EXP that the player gaineded from exp books (Vanilla books)
            if (itemID.StartsWith("SkillBook_"))
            {
                int count = who.newLevels.Count;
                int EXP = (int)Math.Floor(250 * playerBookLevel * 0.05);
                who.gainExperience(Convert.ToInt32(itemID.Last().ToString() ?? ""), EXP);
                if (who.newLevels.Count == count || (who.newLevels.Count > 1 && count >= 1))
                {
                    DelayedAction.functionAfterDelay(delegate
                    {
                        Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:SkillBookMessage", Game1.content.LoadString("Strings\\1_6_Strings:SkillName_" + itemID.Last()).ToLower()));
                    }, 1000);
                }
                return;
            }

            if (itemID == "PurpleBook")
            {
                //Vanilla skills
                int EXP = (int)Math.Floor(250 * playerBookLevel * 0.05);
                Game1.player.gainExperience(0, EXP);
                Game1.player.gainExperience(1, EXP);
                Game1.player.gainExperience(2, EXP);
                Game1.player.gainExperience(3, EXP);
                Game1.player.gainExperience(4, EXP);

                //Modded skills
                string[] VisibleSkills = Skills.GetSkillList().Where(s => Skills.GetSkill(s).ShouldShowOnSkillsPage).ToArray();
                foreach (string skill in VisibleSkills)
                {
                    Skills.AddExperience(Game1.player, skill, EXP);
                }
                return;
            }

            //For vanilla books the player has read
            if (Game1.player.stats.Get(__instance.itemId.Value) != 0 && itemID != "Book_PriceCatalogue" && itemID != "Book_AnimalCatalogue")
            {
                bool flag = false;
                foreach (string contextTag in __instance.GetContextTags())
                {
                    if (contextTag.StartsWithIgnoreCase("book_xp_"))
                    {
                        flag = true;
                        string text = contextTag.Split('_')[2];
                        int EXP = (int)Math.Floor(100 * playerBookLevel * 0.05);
                        Game1.player.gainExperience(Farmer.getSkillNumberFromName(text), EXP);
                        break;
                    }
                }

                if (!flag)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        int EXP = (int)Math.Floor(20 * playerBookLevel * 0.05);
                        Game1.player.gainExperience(i, EXP);
                    }
                }
                return;
            }

            #endregion

        }

        public static List<Vector2> TilesAffected(Vector2 tileLocation, int power, Farmer who)
        {
            List<Vector2> list = new List<Vector2>();
            list.Add(tileLocation);
            for (int i = (int)tileLocation.X - power; (float)i <= tileLocation.X + power; i++)
            {
                for (int j = (int)tileLocation.Y - power; (float)j <= tileLocation.Y + power; j++)
                {
                    list.Add(new Vector2(i, j));
                }
            }
            return list;
        }
    }


    //Goal of the patch is to increase wood dropping at a low chance if you have woody's secret
    [HarmonyPatch(typeof(Tree), "tickUpdate")]
    class WoodySecret_patch
    {
        [HarmonyLib.HarmonyPostfix]
        public static void Postfix(Tree __instance)
        {
            if (__instance.falling.Value)
            {
                WildTreeData data = __instance.GetData();
                Farmer farmer = Game1.GetPlayer(__instance.lastPlayerToHit.Value) ?? Game1.MasterPlayer;
                if (data.DropWoodOnChop &&
                    Utilities.GetLevel(farmer, true) >= 4 &&
                    farmer.stats.Get("Book_Woodcutting") != 0 &&
                    Game1.random.NextDouble() < 0.05)
                {
                    Vector2 tile = __instance.Tile;
                    GameLocation location = __instance.Location;
                    int num2 = (int)((farmer.professions.Contains(12) ? 1.25 : 1.0) * (double)(12 + ExtraWoodCalculator(__instance, tile)));

                    Game1.createRadialDebris(location, 12, (int)tile.X + (__instance.shakeLeft.Value ? (-4) : 4), (int)tile.Y, num2, resource: true);
                    Game1.createRadialDebris(location, 12, (int)tile.X + (__instance.shakeLeft.Value ? (-4) : 4), (int)tile.Y, (int)((farmer.professions.Contains(12) ? 1.25 : 1.0) * (double)(12 + ExtraWoodCalculator(__instance, tile))), resource: false);
                }
            }
        }

        private static int ExtraWoodCalculator(Tree tree, Vector2 tileLocation)
        {
            Random random = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, (double)tileLocation.X * 7.0, (double)tileLocation.Y * 11.0);
            int num = 0;
            if (random.NextDouble() < Game1.player.DailyLuck)
            {
                num++;
            }

            if (random.NextDouble() < (double)Game1.player.ForagingLevel / 12.5)
            {
                num++;
            }

            if (random.NextDouble() < (double)Game1.player.ForagingLevel / 12.5)
            {
                num++;
            }

            if (random.NextDouble() < (double)Game1.player.LuckLevel / 25.0)
            {
                num++;
            }

            if (tree.treeType.Value == "3")
            {
                num++;
            }

            return num;
        }
    }


    //Goal of the patch is to increase monster drops at a low chance if you have the monster compendium
    [HarmonyPatch(typeof(GameLocation), "monsterDrop")]
    class MonsterCompedium_patch
    {
        [HarmonyLib.HarmonyPostfix]
        public static void Postfix(GameLocation __instance, ref Monster monster, ref int x, ref int y, ref Farmer who)
        {
            if (who.stats.Get("Book_Void") != 0 && Utilities.GetLevel(who, true) >= 7 && Game1.random.NextDouble() < 0.03 )
            {
                IList<string> objectsToDrop = monster.objectsToDrop;
                Vector2 vector = Utility.PointToVector2(who.StandingPixel);
                List<Item> extraDropItems = monster.getExtraDropItems();
                if (who.isWearingRing("526") && DataLoader.Monsters(Game1.content).TryGetValue(monster.Name, out string value))
                {
                    string[] array = ArgUtility.SplitBySpace(value.Split('/')[6]);
                    for (int i = 0; i < array.Length; i += 2)
                    {
                        if (Game1.random.NextDouble() < Convert.ToDouble(array[i + 1]))
                        {
                            objectsToDrop.Add(array[i]);
                        }
                    }
                }
                List<Debris> list = new List<Debris>();
                for (int j = 0; j < objectsToDrop.Count; j++)
                {
                    string text = objectsToDrop[j];
                    if (text != null && text.StartsWith('-') && int.TryParse(text, out int result))
                    {
                        list.Add(monster.ModifyMonsterLoot(new Debris(Math.Abs(result), Game1.random.Next(1, 4), new Vector2(x, y), vector)));
                    }
                    else
                    {
                        list.Add(monster.ModifyMonsterLoot(new Debris(text, new Vector2(x, y), vector)));
                    }
                }

                for (int k = 0; k < extraDropItems.Count; k++)
                {
                    list.Add(monster.ModifyMonsterLoot(new Debris(extraDropItems[k], new Vector2(x, y), vector)));
                }
                if (who.isWearingRing("526"))
                {
                    extraDropItems = monster.getExtraDropItems();
                    for (int l = 0; l < extraDropItems.Count; l++)
                    {
                        Item one = extraDropItems[l].getOne();
                        one.Stack = extraDropItems[l].Stack;
                        one.HasBeenInInventory = false;
                        list.Add(monster.ModifyMonsterLoot(new Debris(one, new Vector2(x, y), vector)));
                    }
                }
                foreach (Debris item2 in list)
                {
                    __instance.debris.Add(item2);
                }

            }
        }
    }

    //Goal is to prevent slowdown when running through grass
    [HarmonyPatch(typeof(Grass), "doCollisionAction")]
    class Slitherlegs_patch
    {
        [HarmonyLib.HarmonyPostfix]
        public static void Postfix(Grass __instance, ref Character who)
        {
            if (who is Farmer farmer)
            {
                if (farmer.stats.Get("Book_Grass") != 0 && Utilities.GetLevel(farmer, true) >= 4)
                {
                    farmer.temporarySpeedBuff = 1f;
                }
            }
        }
    }

    //diamond hunter
    [HarmonyPatch(typeof(Pickaxe), "DoFunction")]
    class DiamondHunter_patch
    {
        [HarmonyLib.HarmonyPostfix]
        public static void Postfix(GameLocation location, int x, int y, int power, Farmer who)
        {
            if (who != null && who.stats.Get("Book_Diamonds") != 0 && Game1.random.NextDouble() < 0.0066 && Utilities.GetLevel(who, true) >= 8)
            {
                Utility.clampToTile(new Vector2(x, y));
                int num = x / 64;
                int num2 = y / 64;
                Vector2 vector = new Vector2(num, num2);
                location.Objects.TryGetValue(vector, out var value);
                if (value != null)
                {
                    if (value.IsBreakableStone() && value.MinutesUntilReady <= 0)
                    {
                        Game1.createObjectDebris("(O)72", num, num2, who.UniqueMultiplayerID, location);
                        if (who.professions.Contains(19) && Game1.random.NextBool())
                        {
                            Game1.createObjectDebris("(O)72", num, num2, who.UniqueMultiplayerID, location);
                        }
                    }
                }
            }
        }
    }

    //Goal of the patch is to increase monster drops at a low chance if you have the monster compendium
    [HarmonyPatch(typeof(GameLocation), "monsterDrop")]
    class Blank_patch
    {
        [HarmonyLib.HarmonyPostfix]
        public static void Postfix()
        {

        }
    }

}
