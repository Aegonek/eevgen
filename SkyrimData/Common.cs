using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace eevgen.SkyrimData
{
    enum WeaponType
    {
        [Description("Dagger")]
        Dagger,
        [Description("Sword")]
        Sword,
        [Description("Greatsword")]
        Greatsword,
        [Description("War Axe")]
        WarAxe,
        [Description("Battleaxe")]
        Battleaxe,
        [Description("Mace")]
        Mace,
        [Description("Warhammer")]
        Warhammer,
        [Description("Bow")]
        Bow
    }

    enum Material { Iron, Steel, Imperial, Dwarven, Orcish, Elven, Glass, Nordic, Deadric, Ebony, Stalhrim }

    static class Common
    {

        internal readonly static string[] VanillaFileNames = new string[] { "Skyrim.esm", "Update.esm", "Dawnguard.esm", "Dragonborn.esm", "HearthFires.esm" };

        internal static int[] GetAvailableTiers(Material material)
        {
            return material switch
            {
                var x when x <= Material.Imperial => new[] { 1, 2, 3 },
                var x when x <= Material.Orcish => new[] { 2, 3, 4 },
                var x when x <= Material.Nordic => new[] { 3, 4, 5 },
                _ => new[] { 4, 5, 6 }
            };
        }

        internal static string GenerateEnchantmentSuffix(string rawEnchantment, int powerLevel)
        {
            powerLevel = powerLevel - 1;
            return rawEnchantment switch
            {
                var ench when Regex.IsMatch("DamageArmor", ench) => new[] { "of Cracking", "of Sundering", "of Corrosion", "of Annihilation", "of Armor Eating" }[powerLevel],
                var ench when Regex.IsMatch("DamageWeapon", ench) => new[] { "of Fracture", "of Rust", "of Disintegration", "of Shattering", "of Demolition", "of Disarming" }[powerLevel],
                var ench when Regex.IsMatch("SunDamage", ench) => new[] { "of Shimmer", "of Glare", "of Sun", "of Radiance", "of Brilliance", "of Incandescence" }[powerLevel],
                var ench when Regex.IsMatch("PoisonDamage", ench) => new[] { "of Infection", "of Affliction", "of Poison", "of Plague", "of Venom", "of Scourge" }[powerLevel],
                var ench when Regex.IsMatch("Frenzy", ench) => new[] { "of Fury", "of Rage", "of Frenzy", "of Wrath", "of Mayhem", "of Madness" }[powerLevel],
                var ench when Regex.IsMatch("Silence", ench) => new[] { "of Murmur", "of Hush", "of Silence", "of Quiet", "of Mute", "of Tongue Tying" }[powerLevel],
                var ench => "of " + new[] { "Minor ", "", "Major ", "Eminent ", "Extreme ", "Peerless " }[powerLevel] + Regex.Replace(ench, @"(?<!^)\p{Lu}", x => " " + x.Value)
                    .Then(x => x.Replace("To", "to"))
            };
        }
    }
}
