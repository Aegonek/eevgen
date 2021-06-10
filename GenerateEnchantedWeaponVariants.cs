using eevgen.SkyrimData;
using eevgen.Utilities;
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
    class GenerateEnchantedWeaponVariants : IWorker
    {
        ISkyrimMod mod;
        Logger logger;
        Arguments args;

        public GenerateEnchantedWeaponVariants(ISkyrimMod mod, Logger logger, Arguments args) => (this.mod, this.logger, this.args) = (mod, logger, args);

        public void Work()
        {
            List<IWeaponGetter> baseWeapons = GetBaseWeapons(args.DataFolderPath, logger);
            List<ObjectEffect> newEnchantments = GetWeaponEnchantments(mod);
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
                            RetryAddWeaponWithDifferentKey(generatedWeapon);
                            break;
                        }
                        catch (Exception) { }
                    }
                }

            logger.Info("Almost finished!");
            mod.WriteToBinaryParallel(args.ModPath);
            logger.Info("Finished work!");
        }



        private List<Weapon> GenerateEnchantedVariants(List<IWeaponGetter> baseWeapons, List<ObjectEffect> newEnchantments, ModKey modKey, Logger logger)
        {
            IEnumerable<(ObjectEffect, IWeaponGetter)>? product = (from enchantment in newEnchantments
                                                                   from baseWeapon in baseWeapons
                                                                   select (enchantment, baseWeapon));

            IEnumerable<Weapon> variants = product.Choose<(ObjectEffect, IWeaponGetter), Weapon>(prod =>
                TryGenerateEnchantedVariant(prod.Item2, prod.Item1, modKey, logger)!);

            return variants.ToList();
        }

        private (bool, Weapon?) TryGenerateEnchantedVariant(IWeaponGetter weaponTemplate, IObjectEffectGetter enchantment, ModKey modKey, Logger logger)
        {
            try
            {
                uint id = RandomUtilities.GetRandomUint();
                FormKey key = new(modKey, id);

                Material material = Reading.ReadWeaponMaterial(weaponTemplate);
                int[] tiers = Common.GetAvailableTiers(material);
                int enchantmentTier = Reading.ReadEnchantmentPowerLevel(enchantment);
                if (!tiers.Contains(enchantmentTier))
                {
                    logger.Info($"Chose to not generate weapon for: {weaponTemplate.Name} & {enchantment.Name}");
                    return (false, null);
                }

                Weapon copied = weaponTemplate.Duplicate(key);
                IFormLinkNullable<IObjectEffectGetter> copyable = enchantment.AsNullableLink();
                copied.EditorID = $"MAG_{copied.EditorID}_{enchantment.EditorID!.Replace("MAG_", "")}";
                copied.Name = GenerateEnchantedItemName(weaponTemplate, enchantment);
                copied.ObjectEffect = copyable;
                copied.EnchantmentAmount = (ushort)CalculateEnchantmentAmount(enchantment);
                logger.Info($"Generated weapon: {copied.Name}!");
                return (true, copied);
            }
            catch (Exception ex)
            {
                logger.Error($"Issue when trying to generate weapon for: {weaponTemplate.EditorID} & {enchantment.EditorID}!" + ex.Message);
                return (false, null);
            }
        }

        private List<IWeaponGetter> GetBaseWeapons(string dataPath, Logger logger)
        {
            List<string> baseWeaponNames = (from material in EnumExtensions.GetValuesAsStrings<Material>()
                                            from weaponType in EnumExtensions.GetDescriptions<WeaponType>()
                                            select material + " " + weaponType).ToList();

            IEnumerable<ISkyrimModGetter> vanillaFiles = Common.VanillaFileNames.Select(fileName =>
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

        private List<ObjectEffect> GetWeaponEnchantments(ISkyrimMod mod)
        {
            return mod.ObjectEffects
                .Where(x => x.CastType == CastType.FireAndForget)
                .Also(x => x.ForEach(_x => logger.Info($"Found enchantment: {_x.Name}")))
                .ToList();
        }

        private int CalculateEnchantmentAmount(IObjectEffectGetter enchantment)
        {
            int powerLevel = Reading.ReadEnchantmentPowerLevel(enchantment);
            return powerLevel * 500;
        }

        private string GenerateEnchantedItemName(IWeaponGetter weapon, IObjectEffectGetter enchantment)
        {
            int powerLevel = Reading.ReadEnchantmentPowerLevel(enchantment);
            string rawEnchantment = Reading.ReadRawEnchantmentFromEditorID(enchantment);
            string enchantmentSuffix = Common.GenerateEnchantmentSuffix(rawEnchantment, powerLevel);
                
            return $"{weapon.Name} {enchantmentSuffix}";

        }

        private void RetryAddWeaponWithDifferentKey(Weapon generatedWeapon)
        {
            uint id = RandomUtilities.GetRandomUint();
            FormKey key = new(mod.ModKey, id);
            Weapon? withDifferentId = generatedWeapon.Duplicate(key);
            mod.Weapons.Add(withDifferentId);
        }
    }
}
