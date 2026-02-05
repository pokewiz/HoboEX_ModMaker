using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Drawing.Design;
using System.Linq;
using System.Text.Json;

namespace HoboEX_ModMaker.Models
{
    // ==================== Fixed Collection Editor ====================
    public class FixedSizeArrayEditor : System.ComponentModel.Design.ArrayEditor
    {
        public FixedSizeArrayEditor(Type type) : base(type) { }
        protected override object CreateInstance(Type itemType)
        {
            throw new NotSupportedException(LocalizationManager.Get("Msg_AddingNotAllowed") ?? "Adding items is not allowed.");
        }
        protected override bool CanRemoveInstance(object value) => false;
    }

    // ==================== 根对象 ====================
    public class RootJson
    {
        public List<ConsumableJsonItem> consumables { get; set; } = new List<ConsumableJsonItem>();
        public List<ScrapJsonItem> scraps { get; set; } = new List<ScrapJsonItem>();
        public List<ShopJsonItem> shops { get; set; } = new List<ShopJsonItem>();
        public List<ShopJsonItem> sells { get; set; } = new List<ShopJsonItem>();
        public List<PackageTableJsonItem> packageTables { get; set; } = new List<PackageTableJsonItem>();
        public List<RecipeJson> recipes { get; set; } = new List<RecipeJson>();
        public List<BagJsonItem> bags { get; set; } = new List<BagJsonItem>();
        public List<GearJsonItem> gears { get; set; } = new List<GearJsonItem>();
        public List<SalvagePatternJson> salvagePatterns { get; set; } = new List<SalvagePatternJson>();
    }

    public class NpcRootJson
    {
        public List<ArchetypeJson> archetypes { get; set; } = new List<ArchetypeJson>();
        public List<NpcConversionJson> conversions { get; set; } = new List<NpcConversionJson>();
        
        [JsonPropertyName("strings")]
        public System.Text.Json.JsonElement RawStrings { get; set; }

        [JsonIgnore]
        public DialogueL10n strings
        {
            get
            {
                var l10n = new DialogueL10n();
                if (RawStrings.ValueKind == JsonValueKind.Object)
                {
                    // New format: { "strings": { "ID": { "zh": "..." } } }
                    if (RawStrings.TryGetProperty("strings", out var sProp) && sProp.ValueKind == JsonValueKind.Object)
                    {
                        l10n.strings = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(sProp.GetRawText());
                    }
                    else
                    {
                        // Maybe the object IS the strings dictionary? Case like { "3000": {...} }
                        l10n.strings = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(RawStrings.GetRawText());
                    }
                }
                else if (RawStrings.ValueKind == JsonValueKind.Array)
                {
                    // Old format: [ { "3000": { "zh": "..." } } ]
                    var list = JsonSerializer.Deserialize<List<Dictionary<string, Dictionary<string, string>>>>(RawStrings.GetRawText());
                    if (list != null)
                    {
                        foreach (var dict in list)
                        {
                            foreach (var kvp in dict)
                                l10n.strings[kvp.Key] = kvp.Value;
                        }
                    }
                }
                return l10n;
            }
            set 
            {
                // Convert back for saving
                UpdateRawStrings(value);
            }
        }

        public void UpdateRawStrings(DialogueL10n l10n)
        {
            if (l10n == null) return;

            // 如果当前是数组格式（旧格式），保持数组格式保存
            if (RawStrings.ValueKind == JsonValueKind.Array)
            {
                var list = new List<Dictionary<string, Dictionary<string, string>>>();
                foreach (var kvp in l10n.strings)
                {
                    var dict = new Dictionary<string, Dictionary<string, string>>
                    {
                        { kvp.Key, kvp.Value }
                    };
                    list.Add(dict);
                }
                var json = JsonSerializer.Serialize(list);
                RawStrings = JsonSerializer.Deserialize<JsonElement>(json);
            }
            else
            {
                // 否则使用对象格式（新格式 / 扁平格式）
                // 如果原始 RawStrings 内部包含 "strings" 属性，则维持该结构
                if (RawStrings.ValueKind == JsonValueKind.Object && RawStrings.TryGetProperty("strings", out _))
                {
                    var json = JsonSerializer.Serialize(l10n);
                    RawStrings = JsonSerializer.Deserialize<JsonElement>(json);
                }
                else
                {
                    // 扁平对象格式直接序列化字典部分
                    var json = JsonSerializer.Serialize(l10n.strings);
                    RawStrings = JsonSerializer.Deserialize<JsonElement>(json);
                }
            }
        }
    }

    public class NpcConversionJson
    {
        public string path { get; set; }
        public string archetype { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [Browsable(false)]
        public Dictionary<string, string> l10n { get; set; } = new Dictionary<string, string>();

        public override string ToString()
        {
            string displayText = archetype ?? "Unknown";

            // 尝试从本地化中获取显示文本
            string currentLang = LocalizationManager.CurrentLanguage;
            if (l10n != null && l10n.Count > 0)
            {
                string lang = currentLang.StartsWith("zh") ? "zh" : "en";
                if (l10n.ContainsKey(lang) && !string.IsNullOrEmpty(l10n[lang]))
                {
                    displayText = l10n[lang];
                }
            }

            return $"{archetype}: {displayText}";
        }
    }

    public class ModInfo
    {
        [Category("Display")]
        public string name { get; set; } = "New Mod";
        [Category("Display")]
        public string author { get; set; } = "";
        [Category("Display")]
        public string version { get; set; } = "1.0.0";
        public override string ToString() => name ?? "New Mod";
    }

    public enum ESoundType { General, ScrapGeneral, ScrapMetal, ScrapWood, ScrapSoft, ScrapGlass, Coin, Bag, DrinkGlass, DrinkGeneral, Cigarette, ConsumGeneral, ConsumFood, CondumFoodFresh, Scratch, Gear, GearZip, DrinkBubble }
    public enum ETypeAddiction { NOTHING, Alcohol, Drugs, Cigarettes }
    public enum EParameterType { NOTHING, HealingBuff, FaithBuff, ConfidenceBuff, CureBuff, DeodorBuff, WarmingBuff, RagingBuff, Health, Food, Morale, Freshness, Warm, Wet, Illness, Toxicity, Inebriety, Greatneed, Smell, Capacity, Stamina, SmellResistance, WetResistance, WarmResistance, ToxicityResistance, Immunity, Attack, Defense, Charism, Courage, CourageMax, Grit, GritMax }
    public enum ETypeChanges { NOTHING, Buff, Primary, Secondary, Normal }
    public enum ERecipeType { Structure, Cook, Item, Improvisation, Weapon, ForRepair, ForUpgrade }
    public enum EBenchType { Nothing, Normal, Kitchen, DrugLab }
    public enum ESkillKey { DrunkerSK, DumpRiderSK, TacticOrdinarySK, InsolenceSK, TrollSK, MuggerSK, StreetSellerSK, ResistanceSK, DirtyThiefSK, DirtyThief2SK, CharmingSK, Charming2SK, Charming3SK, KurazSK, BulglarSK, MechatronikSK, IntuiceSK, AdrenalinSK, MistrMeceSK, MasochistaSK, FighterSK, WeaponsmithSK, PaserakSK, Paserak2SK, NotorikSK, Notorik2SK, Notorik3SK, FetakSK, Fetak2SK, NOTHING }
    public enum EGearParameterType { Health, Food, Morale, Freshness, Warm, Wet, Illness, Toxicity, Alcohol, Greatneed, Smell, SmellResistance, WetResistance, WarmResistance, ToxicityResistance, Immunity, Attack, Defense, Charism, Capacity, Stamina, GearSmell, Grit, GritMax, Courage, CourageMax }
    public enum EGearCategory { Hat, Jacket, Trousers, Shoes }
    public enum EQuestType { Nothing, Private, Shared }
    public enum VoiceType { NoVoice, SpecificVoice, MaleHobo, MaleFetch, MaleOfficial, MaleSemiOfficial,  FemaleHobo, FemaleOfficial, FemaleOld, StreetFemaleEasy, StreetFemaleMedium, StreetMaleEasy, StreetMaleMedium, StreetMaleHard, MaleOld }

    public class ReputationRootJson { public List<ReputationJson> reputations { get; set; } = new List<ReputationJson>(); }
    public class ReputationJson
    {
        [Category("Basic"), TypeConverter(typeof(NpcArchetypeConverter)), Editor(typeof(SearchableStringEditor), typeof(UITypeEditor))]
        public string archetype { get; set; }
        [Category("Basic")]
        public int initialValue { get; set; } = 0;
        public override string ToString() => $"Reputation: {archetype} ({initialValue})";
    }

    public class ConsumableJsonItem
    {
        [Category("Identification")] public int id { get; set; }
        [Category("Display")] public string customTitle { get; set; }
        [Category("Display")] public string customDescription { get; set; }
        [Category("Display")] public string iconPath { get; set; }
        [Category("Basic")] public int? price { get; set; }
        [Category("Basic")] public float? weight { get; set; }
        [Category("Behavior")] public ETypeAddiction? typeAddiction { get; set; }
        [Category("Basic")] public int? index { get; set; }
        [Category("Basic")] public bool? isStockable { get; set; }
        [Category("Display")] public int? rareColor { get; set; }
        [Category("Basic")] public bool? sellable { get; set; }
        [Category("Display")] public ESoundType? soundType { get; set; }
        [Category("Behavior")] public bool? notForFire { get; set; }
        [Category("Behavior")] public int? referenceItemId { get; set; }
        [Category("Behavior")] public string? packageTable { get; set; }
        [Category("Behavior")] public bool? buffetGame { get; set; }
        [Category("Behavior")] public int? buffetDifficulty { get; set; }
        [Category("Basic")] public int? firerate { get; set; }
        [Category("Behavior")] public bool? isForHotSlot { get; set; }
        [Category("Basic")] public bool isModify { get; set; } = false;
        [Category("Behavior"), Editor(typeof(FixedSizeArrayEditor), typeof(UITypeEditor)), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ChangeEntry[] changes { get; set; }
        [Browsable(false)] public int[] improRecipes { get; set; }
        [Browsable(false)] public int? titleKey { get; set; }
        [Browsable(false)] public int? descriptionKey { get; set; }
        [Category("Behavior")] public int? salvageTableID { get; set; }
        [Category("Behavior")] public bool? isPermanent { get; set; }
        [Category("Behavior")] public EQuestType? questType { get; set; }
        public override string ToString() => $"Consumable [{id}] {customTitle}";
    }

    public class ChangeEntry
    {
        [Category("Behavior")] public EParameterType influence { get; set; } = EParameterType.NOTHING;
        [Category("Behavior")] public ETypeChanges typeChange { get; set; } = ETypeChanges.NOTHING;
        [Category("Behavior")] public int normalValue { get; set; } = 0;
        [Category("Behavior")] public int isAddictedValue { get; set; } = 0;
        public override string ToString() => $"Change [{influence}] T:{typeChange} V:{normalValue}";
    }

    public class ScrapJsonItem
    {
        [Category("Identification")] public int id { get; set; }
        [Category("Display")] public string customTitle { get; set; }
        [Category("Display")] public string customDescription { get; set; }
        [Category("Display")] public string iconPath { get; set; }
        [Category("Basic")] public int? price { get; set; }
        [Category("Basic")] public float? weight { get; set; }
        [Category("Basic")] public int? index { get; set; }
        [Category("Display")] public int? rareColor { get; set; }
        [Category("Basic")] public bool? sellable { get; set; }
        [Category("Display")] public ESoundType? soundType { get; set; }
        [Category("Behavior")] public bool? notForFire { get; set; }
        [Category("Behavior")] public int? referenceItemId { get; set; }
        [Category("Basic")] public int? firerate { get; set; }
        [Category("Basic")] public bool isModify { get; set; } = false;
        [Browsable(false)] public int? titleKey { get; set; }
        [Browsable(false)] public int? descriptionKey { get; set; }
        [Browsable(false)] public int[] improRecipes { get; set; }
        [Category("Behavior")] public int? salvageTableID { get; set; }
        [Category("Behavior")] public bool? isPermanent { get; set; }
        [Category("Behavior")] public EQuestType? questType { get; set; }
        public override string ToString() => $"Scrap [{id}] {customTitle}";
    }

    public class BagJsonItem
    {
        [Category("Identification")] public int id { get; set; }
        [Category("Display")] public string customTitle { get; set; }
        [Category("Display")] public string customDescription { get; set; }
        [Category("Display")] public string iconPath { get; set; }
        [Category("Basic")] public int? price { get; set; }
        [Category("Basic")] public float? weight { get; set; }
        [Category("Basic")] public int? index { get; set; }
        [Category("Basic")] public bool? sellable { get; set; }
        [Category("Display")] public int? rareColor { get; set; }
        [Category("Display")] public ESoundType? soundType { get; set; }
        [Category("Behavior")] public bool? notForFire { get; set; }
        [Category("Behavior")] public int? referenceItemId { get; set; }
        [Category("Basic")] public int? capacity { get; set; }
        [Category("Basic")] public int? firerate { get; set; }
        [Category("Basic")] public bool? isStockable { get; set; }
        [Category("Basic")] public bool isModify { get; set; } = false;
        [Browsable(false)] public int? titleKey { get; set; }
        [Browsable(false)] public int? descriptionKey { get; set; }
        [Browsable(false)] public int[] improRecipes { get; set; }
        [Category("Behavior")] public int? salvageTableID { get; set; }
        [Category("Behavior")] public bool? isPermanent { get; set; }
        [Category("Behavior")] public EQuestType? questType { get; set; }
        public override string ToString() => $"Bag [{id}] {customTitle}";
    }

    public class GearJsonItem
    {
        [Category("Identification")] public int id { get; set; }
        [Category("Display")] public string customTitle { get; set; }
        [Category("Display")] public string customDescription { get; set; }
        [Category("Display")] public string iconPath { get; set; }
        [Category("Basic")] public int? price { get; set; }
        [Category("Basic")] public float? weight { get; set; }
        [Category("Basic")] public int? firerate { get; set; }
        [Category("Display")] public int? rareColor { get; set; }
        [Category("Display")] public ESoundType? soundType { get; set; }
        [Category("Behavior")] public bool? notForFire { get; set; }
        [Category("Behavior")] public EGearCategory? category { get; set; }
        [Category("Behavior")] public int? durabilityResistance { get; set; }
        [Category("Behavior")] public int? warmResistance { get; set; }
        [Category("Behavior")] public int? wetResistance { get; set; }
        [Category("Behavior")] public uint? repairRecipe { get; set; }
        [Category("Behavior")] public int? repairCash { get; set; }
        [Category("Behavior"), Editor(typeof(FixedSizeArrayEditor), typeof(UITypeEditor))] public ParameterChangeJson[] parameterChanges { get; set; }
        [Category("Basic")] public bool isModify { get; set; } = false;
        [Category("Behavior")] public int? referenceItemId { get; set; }
        [Browsable(false)] public int? titleKey { get; set; }
        [Browsable(false)] public int? descriptionKey { get; set; }
        [Browsable(false)] public int[] improRecipes { get; set; }
        [Category("Behavior")] public int? salvageTableID { get; set; }
        [Category("Behavior")] public bool? isPermanent { get; set; }
        [Category("Behavior")] public EQuestType? questType { get; set; }
        public override string ToString() => $"Gear [{id}] {customTitle}";
    }

    public class ParameterChangeJson
    {
        [Category("Behavior")] public EGearParameterType influencedParameterType { get; set; }
        [Category("Behavior")] public int value { get; set; }
        public override string ToString() => $"Param: {influencedParameterType} ({value})";
    }

    public class ShopJsonItem
    {
        [Category("Identification")] public string title { get; set; }
        [Category("Behavior")] public ShopItemEntry[] items { get; set; }
        public override string ToString() => $"Shop: {title}";
    }

    public class ShopItemEntry
    {
        [Category("Behavior")] public bool isStealable { get; set; }
        [Category("Behavior")] public int countItems { get; set; }
        [Category("Behavior")] public int itemID { get; set; }
        [Category("Behavior")] public float pricePercent { get; set; }
        public override string ToString() => $"Item {itemID} (x{countItems})";
    }

    public class PackageTableJsonItem
    {
        [Category("Identification")] public string title { get; set; }
        [Category("Behavior")] public PackageTableItemEntry[] items { get; set; }
        public override string ToString() => $"Package: {title}";
    }

    public class PackageTableItemEntry
    {
        [Category("Behavior")] public bool isStealable { get; set; }
        [Category("Behavior")] public int countItems { get; set; }
        [Category("Behavior")] public int itemID { get; set; }
        [Category("Behavior")] public float pricePercent { get; set; }
        public override string ToString() => $"Item {itemID} (x{countItems})";
    }

    public class RecipeJson
    {
        [Category("Identification")] public int id { get; set; }
        [Category("Behavior")] public ERecipeType type { get; set; }
        [Category("Behavior")] public bool notActive { get; set; }
        [Category("Basic")] public int index { get; set; }
        [Category("Behavior")] public int requireSkillLvl { get; set; }
        [Category("Behavior")] public ESkillKey requireSK { get; set; }
        [Category("Behavior")] public int resultItemId { get; set; }
        [Category("Behavior")] public EBenchType myBench { get; set; }
        [Category("Behavior")] public EBenchType myAsociatedBench { get; set; }
        [Category("Behavior")] public int craftingDifficulty { get; set; }
        [Category("Behavior")] public RequireItemJson[] requireItemsPrimary { get; set; }
        [Category("Behavior")] public RequireItemJson[] requireItemsSecondary { get; set; }
        [Category("Behavior")] public bool autoUnlockByItem { get; set; } = true;
        public override string ToString() => $"Recipe [{id}] Result: {resultItemId}";
    }

    public class RequireItemJson
    {
        [Category("Behavior")] public int itemID { get; set; }
        [Category("Behavior")] public int itemCount { get; set; }
        public override string ToString() => $"Item {itemID} (x{itemCount})";
    }

    public class SalvagePatternJson
    {
        [Category("Identification")] public int id { get; set; }
        [Category("Behavior")] public SalvageRewardJson[] rewards { get; set; }
        public override string ToString() => $"Salvage Pattern [{id}]";
    }

    public class SalvageRewardJson
    {
        [Category("Behavior")] public uint resultItemID { get; set; }
        [Category("Behavior")] public int maxCount { get; set; }
        [Category("Behavior")] public int percent { get; set; }
        [Category("Behavior")] public int minSkillLvl { get; set; }
    }

    public class ArchetypeJson
    {
        [Category("Identification")] public string key { get; set; }
        [Category("Behavior")] public VoiceType? voiceType { get; set; }
        [Category("Behavior")] public int? voiceIndex { get; set; }
        [Category("Behavior")] public int? defaultBT { get; set; }
        [Category("Behavior")] public string shopTable { get; set; }
        [Category("Behavior")] public string sellTable { get; set; }
        [Category("Behavior")] public string beggingPrimTable { get; set; }
        [Category("Behavior")] public string beggingSecondTable { get; set; }
        [Category("Behavior")] public float? pricePercentSell { get; set; }
        [Category("Behavior")] public float? pricePercentShop { get; set; }
        [Category("Behavior")] public bool? isRandom { get; set; }
        [Category("Behavior")] public int? randomMin { get; set; }
        [Category("Behavior")] public int? randomMax { get; set; }
        [Category("Behavior")] public bool? differentMinMax { get; set; }
        [Category("Behavior")] public int? defense { get; set; }
        [Category("Behavior")] public int? strength { get; set; }
        [Category("Behavior")] public int? criticalChance { get; set; }
        public override string ToString() => $"Archetype: {key?.ToString() ?? "New"}";
    }
}
