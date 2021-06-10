using eevgen.Utilities;
using Mutagen.Bethesda.Skyrim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace eevgen.SkyrimData
{
    static class Reading
    {
        public static Material ReadWeaponMaterial(IWeaponGetter weapon)
        {
            string? name = weapon.Name?.String;
            string[] materials = EnumExtensions.GetValuesAsStrings<Material>();
            string? material = materials.FirstOrDefault(material => name?.Contains(material) ?? false);
            if (material is null)
                throw new ArgumentException($"Couldn't read weapon material for {weapon.EditorID}");
            return (Material) Enum.Parse(typeof(Material), material);
        }

        public static WeaponType ReadWeaponType(IWeaponGetter weapon)
        {
            string? name = weapon.Name?.String;
            string[] weaponTypes = EnumExtensions.GetValuesAsStrings<WeaponType>();
            string? weaponType = weaponTypes.FirstOrDefault(wpnType => name?.Contains(wpnType) ?? false);
            if (weaponType is null)
                throw new ArgumentException($"Couldn't read weapon type for {weapon.EditorID}");

            return (WeaponType) Enum.Parse(typeof(WeaponType), weaponType);
        }

        public static int ReadEnchantmentPowerLevel(IObjectEffectGetter enchantment)
        {
            Match mEnchantmentStrength = Regex.Match(enchantment.EditorID!, @"\d$");
            if (!mEnchantmentStrength.Success)
                throw new ArgumentException($"Couldn't evaluate strength of: {enchantment.EditorID}");
            return int.Parse(mEnchantmentStrength.Value);
        }

        public static string ReadRawEnchantmentFromEditorID(IObjectEffectGetter enchantment)
        {
            Match mEnchantmentName = Regex.Match(enchantment.EditorID!, @"Ench(Weapon)?(?<name>.+?)\d");
            if (!mEnchantmentName.Success)
                throw new ArgumentException($"Couldn't evaluate strength of: {enchantment.EditorID}");
            return mEnchantmentName.Groups["name"].Value;
        }
    }
}
