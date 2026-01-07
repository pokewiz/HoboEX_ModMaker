using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Drawing.Design;
using System.Linq;

namespace HoboEX_ModMaker.Models
{
    // ==================== Fixed Collection Editor ====================
    /// <summary>
    /// Custom editor to prevent adding or removing items from fixed-size arrays.
    /// </summary>
    public class FixedSizeArrayEditor : System.ComponentModel.Design.ArrayEditor
    {
        public FixedSizeArrayEditor(Type type) : base(type) { }

        protected override object CreateInstance(Type itemType)
        {
            throw new NotSupportedException(LocalizationManager.Get("Msg_AddingNotAllowed") ?? "Adding items is not allowed.");
        }

        protected override bool CanRemoveInstance(object value)
        {
            return false;
        }
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
        public List<ArchetypeJson> archetypes { get; set; } = new List<ArchetypeJson>();
        public List<ReputationJson> reputations { get; set; } = new List<ReputationJson>();
    }

    // ==================== 信任度初始化 ====================
    public class ReputationJson
    {
        [Category("CategoryBasic"), DisplayName("Prop_NPC_NpcArchetype")]
        public string archetype { get; set; } // NPC 标识符，如 "Hobo_Furgrim"
        
        [Category("CategoryBasic"), DisplayName("Prop_Reputation_Initial")]
        public int initialValue { get; set; } = 0;

        public override string ToString() => $"Reputation: {archetype} ({initialValue})";
    }

    // ==================== 消耗品 ====================
    public class ConsumableJsonItem
    {
        [Category("CategoryIdentification"), DisplayName("Prop_Common_ID")]
        public int id { get; set; }

        [Category("CategoryDisplay"), DisplayName("Prop_Item_CustomTitle")]
        public string customTitle { get; set; }

        [Category("CategoryDisplay"), DisplayName("Prop_Item_CustomDescription")]
        public string customDescription { get; set; }

        [Category("CategoryDisplay"), DisplayName("Prop_Item_IconPath")]
        public string iconPath { get; set; }

        [Category("CategoryBasic"), DisplayName("Prop_Item_Price")]
        public int? price { get; set; }

        [Category("CategoryBasic"), DisplayName("Prop_Item_Weight")]
        public float? weight { get; set; }

        [Category("CategoryBehavior"), DisplayName("Prop_Item_TypeAddiction")]
        public int? typeAddiction { get; set; }

        [Category("CategoryBasic"), DisplayName("Prop_Item_Index")]
        public int? index { get; set; }

        [Category("CategoryBasic"), DisplayName("Prop_Item_IsStockable")]
        public bool? isStockable { get; set; }

        [Category("CategoryDisplay"), DisplayName("Prop_Item_RareColor")]
        public int? rareColor { get; set; }

        [Category("CategoryBasic"), DisplayName("Prop_Item_Sellable")]
        public bool? sellable { get; set; }

        [Category("CategoryDisplay"), DisplayName("Prop_Item_SoundType")]
        public int? soundType { get; set; }

        [Category("CategoryBehavior"), DisplayName("Prop_Item_NotForFire")]
        public bool? notForFire { get; set; }

        [Category("CategoryBehavior"), DisplayName("Prop_Item_ReferenceItemId")]
        public int referenceItemId { get; set; }

        [Category("CategoryBehavior"), DisplayName("Prop_Item_PackageTable")]
        public string packageTable { get; set; } = "";

        [Category("CategoryBehavior"), DisplayName("Prop_Item_BuffetGame")]
        public bool? buffetGame { get; set; }

        [Category("CategoryBehavior"), DisplayName("Prop_Item_BuffetDifficulty")]
        public int? buffetDifficulty { get; set; }

        [Category("CategoryBasic"), DisplayName("Prop_Item_Firerate")]
        public int? firerate { get; set; }

        [Category("CategoryBehavior"), DisplayName("Prop_Item_IsForHotSlot")]
        public bool? isForHotSlot { get; set; }

        [Category("CategoryBasic"), DisplayName("Prop_Item_IsModify")]
        public bool isModify { get; set; } = false;

        [Category("CategoryBehavior"), DisplayName("Prop_Item_Changes")]
        [Description("Fixed size array of 8 changes. You cannot add or remove items.")]
        [Editor(typeof(FixedSizeArrayEditor), typeof(UITypeEditor))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ChangeEntry[] changes { get; set; }

        [Browsable(false)]
        public int[] improRecipes { get; set; }

        [Browsable(false)]
        public int? titleKey { get; set; }
        [Browsable(false)]
        public int? descriptionKey { get; set; }

        [Category("CategoryBehavior"), DisplayName("Prop_Item_SalvageTableID")]
        public int? salvageTableID { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Item_IsPermanent")]
        public bool? isPermanent { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Item_QuestType")]
        public int? questType { get; set; }

        public override string ToString() => $"Consumable [{id}] {customTitle}";
    }

    public class ChangeEntry
    {
        [Category("CategoryBehavior"), DisplayName("Prop_Change_Influence")]
        public int influence { get; set; } = 0;
        
        [Category("CategoryBehavior"), DisplayName("Prop_Change_TypeChange")]
        public int typeChange { get; set; } = 0;
        
        [Category("CategoryBehavior"), DisplayName("Prop_Change_NormalValue")]
        public int normalValue { get; set; } = 0;
        
        [Category("CategoryBehavior"), DisplayName("Prop_Change_IsAddictedValue")]
        public int isAddictedValue { get; set; } = 0;

        public override string ToString() => $"Change [{influence}] T:{typeChange} V:{normalValue}";
    }

    // ==================== 静态物品（Scraps）====================
    public class ScrapJsonItem
    {
        [Category("CategoryIdentification"), DisplayName("Prop_Common_ID")]
        public int id { get; set; }
        [Category("CategoryDisplay"), DisplayName("Prop_Item_CustomTitle")]
        public string customTitle { get; set; }
        [Category("CategoryDisplay"), DisplayName("Prop_Item_CustomDescription")]
        public string customDescription { get; set; }
        [Category("CategoryDisplay"), DisplayName("Prop_Item_IconPath")]
        public string iconPath { get; set; }
        [Category("CategoryBasic"), DisplayName("Prop_Item_Price")]
        public int? price { get; set; }
        [Category("CategoryBasic"), DisplayName("Prop_Item_Weight")]
        public float? weight { get; set; }
        [Category("CategoryBasic"), DisplayName("Prop_Item_Index")]
        public int? index { get; set; }
        [Category("CategoryDisplay"), DisplayName("Prop_Item_RareColor")]
        public int? rareColor { get; set; }
        [Category("CategoryBasic"), DisplayName("Prop_Item_Sellable")]
        public bool? sellable { get; set; }
        [Category("CategoryDisplay"), DisplayName("Prop_Item_SoundType")]
        public int? soundType { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Item_NotForFire")]
        public bool? notForFire { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Item_ReferenceItemId")]
        public int referenceItemId { get; set; }
        [Category("CategoryBasic"), DisplayName("Prop_Item_Firerate")]
        public int? firerate { get; set; }
        [Category("CategoryBasic"), DisplayName("Prop_Item_IsModify")]
        public bool isModify { get; set; } = false;

        [Browsable(false)] public int? titleKey { get; set; }
        [Browsable(false)] public int? descriptionKey { get; set; }
        [Browsable(false)] public int[] improRecipes { get; set; }

        [Category("CategoryBehavior"), DisplayName("Prop_Item_SalvageTableID")]
        public int? salvageTableID { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Item_IsPermanent")]
        public bool? isPermanent { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Item_QuestType")]
        public int? questType { get; set; }

        public override string ToString() => $"Scrap [{id}] {customTitle}";
    }

    // ==================== 背包（Bags）====================
    public class BagJsonItem
    {
        [Category("CategoryIdentification"), DisplayName("Prop_Common_ID")]
        public int id { get; set; }
        [Category("CategoryDisplay"), DisplayName("Prop_Item_CustomTitle")]
        public string customTitle { get; set; }
        [Category("CategoryDisplay"), DisplayName("Prop_Item_CustomDescription")]
        public string customDescription { get; set; }
        [Category("CategoryDisplay"), DisplayName("Prop_Item_IconPath")]
        public string iconPath { get; set; }
        [Category("CategoryBasic"), DisplayName("Prop_Item_Price")]
        public int? price { get; set; }
        [Category("CategoryBasic"), DisplayName("Prop_Item_Weight")]
        public float? weight { get; set; }
        [Category("CategoryBasic"), DisplayName("Prop_Item_Index")]
        public int? index { get; set; }
        [Category("CategoryBasic"), DisplayName("Prop_Item_Sellable")]
        public bool? sellable { get; set; }
        [Category("CategoryDisplay"), DisplayName("Prop_Item_RareColor")]
        public int? rareColor { get; set; }
        [Category("CategoryDisplay"), DisplayName("Prop_Item_SoundType")]
        public int? soundType { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Item_NotForFire")]
        public bool? notForFire { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Item_ReferenceItemId")]
        public int referenceItemId { get; set; }
        [Category("CategoryBasic"), DisplayName("Prop_Item_Capacity")]
        public int? capacity { get; set; }
        [Category("CategoryBasic"), DisplayName("Prop_Item_Firerate")]
        public int? firerate { get; set; }
        [Category("CategoryBasic"), DisplayName("Prop_Item_IsStockable")]
        public bool? isStockable { get; set; }
        [Category("CategoryBasic"), DisplayName("Prop_Item_IsModify")]
        public bool isModify { get; set; } = false;

        [Browsable(false)] public int? titleKey { get; set; }
        [Browsable(false)] public int? descriptionKey { get; set; }
        [Browsable(false)] public int[] improRecipes { get; set; }

        [Category("CategoryBehavior"), DisplayName("Prop_Item_SalvageTableID")]
        public int? salvageTableID { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Item_IsPermanent")]
        public bool? isPermanent { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Item_QuestType")]
        public int? questType { get; set; }

        public override string ToString() => $"Bag [{id}] {customTitle}";
    }

    public class GearJsonItem
    {
        [Category("CategoryIdentification"), DisplayName("Prop_Common_ID")]
        public int id { get; set; }
        [Category("CategoryDisplay"), DisplayName("Prop_Item_CustomTitle")]
        public string customTitle { get; set; }
        [Category("CategoryDisplay"), DisplayName("Prop_Item_CustomDescription")]
        public string customDescription { get; set; }
        [Category("CategoryDisplay"), DisplayName("Prop_Item_IconPath")]
        public string iconPath { get; set; }
        [Category("CategoryBasic"), DisplayName("Prop_Item_Price")]
        public int? price { get; set; }
        [Category("CategoryBasic"), DisplayName("Prop_Item_Weight")]
        public float? weight { get; set; }
        [Category("CategoryBasic"), DisplayName("Prop_Item_Firerate")]
        public int? firerate { get; set; }
        [Category("CategoryDisplay"), DisplayName("Prop_Item_RareColor")]
        public int? rareColor { get; set; }
        [Category("CategoryDisplay"), DisplayName("Prop_Item_SoundType")]
        public int? soundType { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Item_NotForFire")]
        public bool? notForFire { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Item_Category")]
        public int? category { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Item_DurabilityResistance")]
        public int? durabilityResistance { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Item_WarmResistance")]
        public int? warmResistance { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Item_WetResistance")]
        public int? wetResistance { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Item_RepairRecipe")]
        public uint? repairRecipe { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Item_RepairCash")]
        public int? repairCash { get; set; }
        
        [Category("CategoryBehavior"), DisplayName("Prop_Item_ParameterChanges")]
        public ParameterChangeJson[] parameterChanges { get; set; }
        [Category("CategoryBasic"), DisplayName("Prop_Item_IsModify")]
        public bool isModify { get; set; } = false;
        [Category("CategoryBehavior"), DisplayName("Prop_Item_ReferenceItemId")]
        public int referenceItemId { get; set; }

        [Browsable(false)] public int? titleKey { get; set; }
        [Browsable(false)] public int? descriptionKey { get; set; }
        [Browsable(false)] public int[] improRecipes { get; set; }
        
        [Category("CategoryBehavior"), DisplayName("Prop_Item_SalvageTableID")]
        public int? salvageTableID { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Item_IsPermanent")]
        public bool? isPermanent { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Item_QuestType")]
        public int? questType { get; set; }

        public override string ToString() => $"Gear [{id}] {customTitle}";
    }

    public class ParameterChangeJson
    {
        [Category("CategoryBehavior"), DisplayName("Prop_Param_Type")]
        public int influencedParameterType { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Common_Value")]
        public int value { get; set; }
    }

    // ==================== 商店补丁 ====================
    public class ShopJsonItem
    {
        [Category("CategoryIdentification"), DisplayName("Prop_Common_Title")]
        public string title { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Shop_Items")]
        public ShopItemEntry[] items { get; set; }
        public override string ToString() => $"Shop: {title}";
    }

    public class ShopItemEntry
    {
        [Category("CategoryBehavior"), DisplayName("Prop_Shop_IsStealable")]
        public bool isStealable { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Shop_CountItems")]
        public int countItems { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Common_ID")]
        public int itemID { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Shop_PricePercent")]
        public float pricePercent { get; set; }
        public override string ToString() => $"Item {itemID} (x{countItems})";
    }

    // ==================== 包裹表 ====================
    public class PackageTableJsonItem
    {
        [Category("CategoryIdentification"), DisplayName("Prop_Common_Title")]
        public string title { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Shop_Items")]
        public PackageTableItemEntry[] items { get; set; }
        public override string ToString() => $"Package: {title}";
    }

    public class PackageTableItemEntry
    {
        [Category("CategoryBehavior"), DisplayName("Prop_Shop_IsStealable")]
        public bool isStealable { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Shop_CountItems")]
        public int countItems { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Common_ID")]
        public int itemID { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Shop_PricePercent")]
        public float pricePercent { get; set; }
        public override string ToString() => $"Item {itemID} (x{countItems})";
    }

    // ==================== 蓝图 ====================
    public class RecipeJson
    {
        [Category("CategoryIdentification"), DisplayName("Prop_Common_ID")]
        public int id { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Common_Type")]
        public int type { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Recipe_NotActive")]
        public bool notActive { get; set; }
        [Category("CategoryBasic"), DisplayName("Prop_Item_Index")]
        public int index { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Recipe_RequireSkillLvl")]
        public int requireSkillLvl { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Recipe_RequireSK")]
        public int requireSK { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Recipe_ResultItemId")]
        public int resultItemId { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Recipe_MyBench")]
        public int myBench { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Recipe_MyAsociatedBench")]
        public int myAsociatedBench { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Recipe_CraftingDifficulty")]
        public int craftingDifficulty { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Recipe_RequireItemsPrimary")]
        public RequireItemJson[] requireItemsPrimary { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Recipe_RequireItemsSecondary")]
        public RequireItemJson[] requireItemsSecondary { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Recipe_AutoUnlock")]
        public bool autoUnlockByItem { get; set; } = true;

        public override string ToString() => $"Recipe [{id}] Result: {resultItemId}";
    }

    public class RequireItemJson
    {
        [Category("CategoryBehavior"), DisplayName("Prop_Common_ID")]
        public int itemID { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Shop_CountItems")]
        public int itemCount { get; set; }
    }

    // ==================== 拆解模式 ====================
    public class SalvagePatternJson
    {
        [Category("CategoryIdentification"), DisplayName("Prop_Common_ID")]
        public int id { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Salvage_Rewards")]
        public SalvageRewardJson[] rewards { get; set; }
        public override string ToString() => $"Salvage Pattern [{id}]";
    }

    public class SalvageRewardJson
    {
        [Category("CategoryBehavior"), DisplayName("Prop_Recipe_ResultItemId")]
        public uint resultItemID { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Shop_CountItems")]
        public int maxCount { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Shop_PricePercent")]
        public int percent { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Recipe_RequireSkillLvl")]
        public int minSkillLvl { get; set; }
    }

    // ==================== NPC 原型 (Archetypes) ====================
    public class ArchetypeJson
    {
        [Category("CategoryIdentification"), DisplayName("Prop_Common_Key")]
        public string key { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Arch_VoiceType")]
        public int? voiceType { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Arch_VoiceIndex")]
        public int? voiceIndex { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Arch_DefaultBT")]
        public int? defaultBT { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Arch_ShopTable")]
        public string shopTable { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Arch_SellTable")]
        public string sellTable { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Arch_BeggingPrimTable")]
        public string beggingPrimTable { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Arch_BeggingSecondTable")]
        public string beggingSecondTable { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Arch_PricePercentSell")]
        public float? pricePercentSell { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Arch_PricePercentShop")]
        public float? pricePercentShop { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Arch_IsRandom")]
        public bool? isRandom { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Arch_RandomMin")]
        public int? randomMin { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Arch_RandomMax")]
        public int? randomMax { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Arch_DifferentMinMax")]
        public bool? differentMinMax { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Arch_Defense")]
        public int? defense { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Arch_Strength")]
        public int? strength { get; set; }
        [Category("CategoryBehavior"), DisplayName("Prop_Arch_CriticalChance")]
        public int? criticalChance { get; set; }
        
        public override string ToString() => $"Archetype: {key}";
    }
}
