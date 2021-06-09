using MoreLinq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace eevgen
{
    class EnchantedVariantsGenerator : IWorker
    {
        readonly string[] VanillaFileNames = new string[] { "Skyrim.esm", "Update.esm", "Dawnguard.esm", "Dragonborn.esm", "HearthFires.esm" };
        readonly string[] WeaponTypes = new[] { "Dagger", "Sword", "Greatsword", "War Axe", "Battleaxe", "Mace", "Warhammer", "Bow" };
        readonly string[] Materials = new[] { "Iron", "Steel", "Orcish", "Dwarven", "Elven", "Glass", "Imperial", "Ebony", "Deadric", "Dragonbone", "Nord Hero", "Stalhrim", "Nordic" };
        readonly Dictionary<int, string> PowerLevelNames = new() { { 1, "Minor " }, { 2, "" }, { 3, "Major " }, { 4, "Eminent " }, { 5, "Extreme " }, { 6, "Peerless " } };

        public void Work(ISkyrimMod mod, Logger logger, Arguments args)
        {
            List<IWeaponGetter> baseWeapons = GetBaseWeapons(args.DataFolderPath, logger);
            List<ObjectEffect> newEnchantments = GetWeaponEnchantments(mod)
                .Also(x => x.ForEach(_x => logger.Info($"Found enchantment: {_x.Name}")));
            List<Weapon> generatedWeapons = GenerateEnchantedVariants(baseWeapons, newEnchantments, mod.ModKey, logger);

            foreach (Weapon generatedWeapon in generatedWeapons)
                try
                {
                    mod.Weapons.Add(generatedWeapon);
                }
                catch (ArgumentException ex)
                {
                    while (true)
                    {
                        try
                        {
                            logger.Warning("Duplicate FormKey? " + ex.Message);
                            uint id = GetRandomUint();
                            FormKey key = new(mod.ModKey, id);
                            var withDifferentId = generatedWeapon.Duplicate(key);
                            mod.Weapons.Add(withDifferentId);
                            break;
                        }
                        catch (Exception) { }
                    }    
                }

            logger.Info("Almost finished!");
            mod.WriteToBinaryParallel(args.ModPath);
            logger.Info("Finished work!");
        }

        public List<Weapon> GenerateEnchantedVariants(List<IWeaponGetter> baseWeapons, List<ObjectEffect> newEnchantments, ModKey modKey, Logger logger)
        {
            IEnumerable<(ObjectEffect, IWeaponGetter)>? product = (from enchantment in newEnchantments
                                                                                          from baseWeapon in baseWeapons
                                                                                          select (enchantment, baseWeapon));
            IEnumerable<Weapon> variants = product.Choose<(ObjectEffect, IWeaponGetter), Weapon>(prod =>
                GenerateEnchantedVariant(prod.Item2, prod.Item1, modKey, logger)!);

            return variants.ToList();
        }

        private (bool, Weapon?) GenerateEnchantedVariant(IWeaponGetter weaponTemplate, IObjectEffectGetter enchantment, ModKey modKey, Logger logger)
        {
            try
            {
                uint id = GetRandomUint();
                FormKey key = new(modKey, id);

                Weapon copied = weaponTemplate.Duplicate(key);
                IFormLinkNullable<IObjectEffectGetter> copyable = enchantment.AsNullableLink();
                copied.EditorID = $"MAG_{copied.EditorID}_{enchantment.EditorID!.Replace("MAG_", "")}";
                copied.Name = GenerateEnchantedItemName(weaponTemplate, enchantment);
                copied.ObjectEffect = copyable;
                copied.EnchantmentAmount = (ushort)CalculateEnchantmentAmount(copied, enchantment);
                logger.Info($"Generated weapon: {copied.Name}!");
                return (true, copied);
            }
            catch (Exception ex)
            {
                logger.Error($"Issue when trying to generate weapon for: {weaponTemplate.Name} & {enchantment.Name}!" + ex.Message);
                return (false, null);
            }
        }

        private uint GetRandomUint()
        {
            Random random = new Random();
            byte[] bytes = new byte[4];
            random.NextBytes(bytes);
            return BitConverter.ToUInt32(bytes);
        }

        public List<IWeaponGetter> GetBaseWeapons(string dataPath, Logger logger)
        {
            List<string> baseWeaponNames = (from material in Materials
                                            from weaponType in WeaponTypes
                                            select material + " " + weaponType).ToList();

            IEnumerable<ISkyrimModGetter> vanillaFiles = VanillaFileNames.Select(fileName =>
                SkyrimMod.CreateFromBinaryOverlay(Path.Join(dataPath, fileName), SkyrimRelease.SkyrimSE));
            IEnumerable<IWeaponGetter> vanillaWeapons = vanillaFiles.SelectMany(file => file.Weapons);

            List<IWeaponGetter> baseWeapons = new();

            foreach (IWeaponGetter? weapon in vanillaWeapons)
            {
                if (baseWeaponNames.Contains(weapon?.Name?.String!))
                {
                    baseWeaponNames.Remove(weapon!.Name!.String!);
                    baseWeapons.Add(weapon);
                }
            }
            foreach (IWeaponGetter? baseWeapon in baseWeapons)
                logger.Info($"Found base weapon: {baseWeapon.Name}");

            return baseWeapons;
        }

        public static List<ObjectEffect> GetWeaponEnchantments(ISkyrimMod mod)
        {
            return mod.ObjectEffects.Where(x => x.CastType == CastType.FireAndForget).ToList();
        }

        public int CalculateEnchantmentAmount(IWeaponGetter weapon, IObjectEffectGetter enchantment)
        {
            int powerLevel = ReadEnchantmentPowerLevel(enchantment);
            return powerLevel * 500;
        }

        public string GenerateEnchantedItemName(IWeaponGetter weapon, IObjectEffectGetter enchantment)
        {
            int powerLevel = ReadEnchantmentPowerLevel(enchantment);
            string enchantmentName = ReadEnchantmentName(enchantment);
            string powerDependent = PowerLevelNames[powerLevel];
            return $"{weapon.Name} of {powerDependent}{enchantmentName}";
        }

        public int ReadEnchantmentPowerLevel(IObjectEffectGetter enchantment)
        {
            Match mEnchantmentStrength = Regex.Match(enchantment.EditorID!, @"\d$");
            if (!mEnchantmentStrength.Success)
                throw new ArgumentException($"Couldn't evaluate strength of: {enchantment.EditorID}");
            return int.Parse(mEnchantmentStrength.Value);
        }

        public string ReadEnchantmentName(IObjectEffectGetter enchantment)
        {
            Match mEnchantmentName = Regex.Match(enchantment.EditorID!, @"Ench(Weapon)?(?<name>.+?)\d");
            if (!mEnchantmentName.Success)
                throw new ArgumentException($"Couldn't evaluate strength of: {enchantment.EditorID}");
            string rawName = mEnchantmentName.Groups["name"].Value;
            string parsedName = Regex.Replace(rawName, @"(?<!^)\p{Lu}", x => " " + x.Value)
                .Then(x => x.Replace("To", "to"));

            return parsedName;
        }
    }
}
