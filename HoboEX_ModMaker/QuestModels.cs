using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text.Json.Serialization;

namespace HoboEX_ModMaker.Models
{
    // ==================== 任务系统 (FMQ) ====================
    public class QuestRootJson
    {
        public List<QuestJson> quests { get; set; } = new List<QuestJson>();
        
        [Browsable(false)]
        [JsonIgnore]
        public string SourceFilePath { get; set; } = "";
    }

    public class QuestJson
    {
        [Category("Basic")]
        [DisplayName("Quest ID")]
        public string questID { get; set; } = "";

        [Category("Display")]
        [DisplayName("Quest Title")]
        public string questTitle { get; set; } = "";

        [Category("Basic")]
        [DisplayName("Is Private")]
        public bool isPrivate { get; set; }

        [Category("Basic")]
        [DisplayName("Is Auto Start")]
        public bool isAutoStart { get; set; }

        [Category("Basic")]
        [DisplayName("Is Permanent")]
        public bool isPermanent { get; set; }

        [Category("Behavior")]
        [DisplayName("First Node ID")]
        public string firstNodeID { get; set; } = "";

        [Browsable(false)]
        public List<QuestNodeJson> nodes { get; set; } = new List<QuestNodeJson>();

        [Browsable(false)]
        [JsonIgnore]
        public Dictionary<string, string> l10n { get; set; } = new Dictionary<string, string>();

        [Browsable(false)]
        [JsonIgnore]
        public string SourceFilePath { get; set; } = "";

        public override string ToString() => $"[Quest] {questTitle} ({questID})";
    }

    public class QuestNodeJson
    {
        [Category("Basic")]
        [DisplayName("ID")]
        public string id { get; set; } = "";

        [Category("Basic")]
        [DisplayName("Type")]
        [TypeConverter(typeof(QuestNodeTypeConverter))]
        public string typeAction { get; set; } = "ActionNow"; // "ActionNow", "ActionWait", "ActionConv"

        [Category("Behavior")]
        [DisplayName("Next Node IDs")]
        public string[] nextNodeIDs { get; set; } = new string[0];

        // ActionNow (立即执行)
        [Category("Action")]
        [DisplayName("Action Now")]
        public QuestActionNowJson actionNow { get; set; }

        // ActionWait (等待动作)
        [Category("Action")]
        [DisplayName("Action Wait")]
        public QuestActionWaitJson actionWait { get; set; }

        // ActionConv (对话相关)
        [Category("Action")]
        [DisplayName("Action Conv")]
        public QuestActionConvJson actionConv { get; set; }

        public override string ToString() => $"[Node] {id} ({typeAction})";
    }

    public class QuestActionNowJson
    {
        [Category("Basic")]
        [DisplayName("Type of Action")]
        [TypeConverter(typeof(QuestActionNowTypeConverter))]
        public string typeOfAction { get; set; } = "QuestDone"; // e.g., "GiveItem", "SetFlag", "FinishQuest", "RemoveQuest"

        [Category("Basic")]
        [DisplayName("Key")]
        public string key { get; set; } = "";

        [Category("Basic")]
        [DisplayName("Value")]
        public int value { get; set; }

        [Category("Diary")]
        [DisplayName("Is Diary Note")]
        public bool isDiaryNote { get; set; }

        [Category("Diary")]
        [DisplayName("Note")]
        public string note { get; set; } = "";

        [Browsable(false)]
        [JsonIgnore]
        public Dictionary<string, string> l10n { get; set; } = new Dictionary<string, string>();

        public override string ToString() => $"[ActionNow] {typeOfAction} {key} {value}";
    }

    public class QuestActionWaitJson
    {
        [Category("Basic")]
        [DisplayName("Type of Action")]
        [TypeConverter(typeof(QuestActionWaitTypeConverter))]
        public string typeOfAction { get; set; } = "Item"; // e.g., "Dig", "Collect", "Item"

        [Category("Basic")]
        [DisplayName("Key")]
        public string key { get; set; } = "";

        [Category("Basic")]
        [DisplayName("Value")]
        public int value { get; set; }

        [Category("Diary")]
        [DisplayName("Is Diary Note")]
        public bool isDiaryNote { get; set; }

        [Category("Diary")]
        [DisplayName("Note")]
        public string note { get; set; } = "";

        [Category("Location")]
        [DisplayName("Want Location")]
        public bool wantLocation { get; set; }

        [Category("Behavior")]
        [DisplayName("Done After Success")]
        public bool doneAfterSuccess { get; set; }

        [Category("Location")]
        [DisplayName("Locations")]
        [Editor(typeof(MapLocationEditor), typeof(UITypeEditor))]
        public List<MapPointJson> locations { get; set; } = new List<MapPointJson>();

        [Category("Requirements")]
        [DisplayName("Items")]
        public List<QuestItemRequirementJson> items { get; set; } = new List<QuestItemRequirementJson>();

        [Category("Requirements")]
        [DisplayName("Global Bools")]
        public List<QuestBoolRequirementJson> globalBools { get; set; } = new List<QuestBoolRequirementJson>();

        [Category("Requirements")]
        [DisplayName("Global Nums")]
        public List<QuestNumRequirementJson> globalNums { get; set; } = new List<QuestNumRequirementJson>();

        [Browsable(false)]
        [JsonIgnore]
        public Dictionary<string, string> l10n { get; set; } = new Dictionary<string, string>();

        public override string ToString() => $"[ActionWait] {typeOfAction}";
    }

    public class QuestBoolRequirementJson
    {
        public string key { get; set; } = "";
        public bool value { get; set; }
        public override string ToString() => $"[Bool] {key} = {value}";
    }

    public class QuestNumRequirementJson
    {
        public string key { get; set; } = "";
        public int value { get; set; }
        public override string ToString() => $"[Num] {key} = {value}";
    }

    public class QuestItemRequirementJson
    {
        [DisplayName("Item ID")]
        public uint id { get; set; }
        [DisplayName("Count")]
        public int count { get; set; }
        public override string ToString() => $"[Item] ID:{id} x{count}";
    }

    public class QuestActionConvJson
    {
        [Category("Basic")]
        [DisplayName("NPC Archetype")]
        [TypeConverter(typeof(NpcArchetypeConverter))]
        [Editor(typeof(SearchableStringEditor), typeof(UITypeEditor))]
        public string npcArchetype { get; set; } = "";

        [Category("Behavior")]
        [DisplayName("Done Now")]
        public bool doneNow { get; set; }

        [Category("Diary")]
        [DisplayName("Is Diary Note")]
        public bool isDiaryNote { get; set; }

        [Category("Diary")]
        [DisplayName("Note")]
        public string note { get; set; } = "";

        [Category("Location")]
        [DisplayName("Want Location")]
        public bool wantLocation { get; set; }

        [Category("Location")]
        [DisplayName("Locations")]
        [Editor(typeof(MapLocationEditor), typeof(UITypeEditor))]
        public List<MapPointJson> locations { get; set; } = new List<MapPointJson>();

        [Browsable(false)]
        [JsonIgnore]
        public Dictionary<string, string> l10n { get; set; } = new Dictionary<string, string>();

        public override string ToString() => $"[ActionConv] {npcArchetype}";
    }

    public class MapPointJson
    {
        public float x { get; set; }
        public float y { get; set; }
        public int type { get; set; } // 0: Dot, 1: Circle etc.
        public override string ToString() => $"({x}, {y}) Type:{type}";
    }

    // ============= Converters =============

    public class QuestNodeTypeConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => true;
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new string[] { "ActionNow", "ActionWait", "ActionConv" });
        }
    }

    public class QuestActionNowTypeConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => true;
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new string[] { "GiveItem", "SetFlag", "QuestDone", "QuestFail", "RemoveQuest" });
        }
    }

    public class QuestActionWaitTypeConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => true;
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new string[] { "Dig", "Collect", "Item", "WaitTime" });
        }
    }
}
