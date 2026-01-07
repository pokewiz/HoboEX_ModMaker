using System;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Text.Json.Serialization;

namespace HoboEX_ModMaker.Models
{
    public class SearchableStringEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) => UITypeEditorEditStyle.DropDown;
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (editorService == null) return value;

            var lb = new ListBox { BorderStyle = BorderStyle.None, Dock = DockStyle.Fill };
            var tb = new TextBox { Dock = DockStyle.Top };
            var panel = new Panel { Width = 300, Height = 250 };
            panel.Controls.Add(lb);
            panel.Controls.Add(tb);

            string[] items = new string[0];
            if (context.PropertyDescriptor.Converter is StringConverter conv)
            {
                var vals = conv.GetStandardValues(context);
                if (vals != null)
                {
                    items = new string[vals.Count];
                    for (int i = 0; i < vals.Count; i++) items[i] = vals[i].ToString();
                }
            }

            lb.Items.AddRange(items);
            if (value != null) lb.SelectedItem = value.ToString();

            tb.TextChanged += (s, e) =>
            {
                lb.Items.Clear();
                foreach (var item in items)
                {
                    if (item.ToLower().Contains(tb.Text.ToLower()))
                        lb.Items.Add(item);
                }
            };

            lb.Click += (s, e) => { if (lb.SelectedItem != null) editorService.CloseDropDown(); };
            
            tb.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Down && lb.Items.Count > 0)
                {
                    lb.Focus();
                    if (lb.SelectedIndex < 0) lb.SelectedIndex = 0;
                }
                if (e.KeyCode == Keys.Enter) editorService.CloseDropDown();
            };

            // Use a timer to ensure focus is set AFTER the dropdown is shown
            var timer = new System.Windows.Forms.Timer { Interval = 10 };
            timer.Tick += (s, e) => {
                tb.Focus();
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();

            editorService.DropDownControl(panel);
            return lb.SelectedItem ?? value;
        }
    }

    public class HexColorEditor : UITypeEditor
    {
        private static readonly ColorDialog _colorDialog = new ColorDialog { FullOpen = true };

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) => UITypeEditorEditStyle.DropDown;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (editorService == null) return value;

            var lb = new ListBox { BorderStyle = BorderStyle.None, Dock = DockStyle.Fill, DrawMode = DrawMode.OwnerDrawFixed, ItemHeight = 22 };
            
            // Populate items: Favorites + "More..."
            var favorites = LocalizationManager.Settings.FavoriteColors ?? new List<string>();
            foreach (var col in favorites) lb.Items.Add(col);
            lb.Items.Add(LocalizationManager.Get("ContextAddItem") + "..."); // Translation for "More..."

            // OwnerDraw for color swatches
            lb.DrawItem += (s, e) =>
            {
                e.DrawBackground();
                if (e.Index < 0) return;

                string itemText = lb.Items[e.Index].ToString();
                Rectangle swatchRect = new Rectangle(e.Bounds.Left + 2, e.Bounds.Top + 2, 18, e.Bounds.Height - 4);
                
                if (e.Index < favorites.Count)
                {
                    try
                    {
                        using (var brush = new SolidBrush(ColorTranslator.FromHtml(itemText)))
                            e.Graphics.FillRectangle(brush, swatchRect);
                        e.Graphics.DrawRectangle(Pens.Gray, swatchRect);
                    }
                    catch { }
                }
                else
                {
                    // Icon for "More..."
                    e.Graphics.DrawRectangle(Pens.Gray, swatchRect);
                    e.Graphics.DrawLine(Pens.Black, swatchRect.Left + 4, swatchRect.Top + 9, swatchRect.Right - 4, swatchRect.Top + 9);
                    e.Graphics.DrawLine(Pens.Black, swatchRect.Left + 9, swatchRect.Top + 4, swatchRect.Left + 9, swatchRect.Bottom - 4);
                }

                TextRenderer.DrawText(e.Graphics, itemText, lb.Font, new Point(swatchRect.Right + 5, e.Bounds.Top + 2), e.ForeColor);
                e.DrawFocusRectangle();
            };

            lb.Click += (s, e) =>
            {
                if (lb.SelectedIndex < 0) return;

                if (lb.SelectedIndex < favorites.Count)
                {
                    value = lb.SelectedItem.ToString();
                    editorService.CloseDropDown();
                }
                else
                {
                    // Open Native Dialog
                    string currentHex = value as string;
                    if (!string.IsNullOrEmpty(currentHex))
                    {
                        try { _colorDialog.Color = ColorTranslator.FromHtml(currentHex.StartsWith("#") ? currentHex : "#" + currentHex); } catch { }
                    }

                    if (_colorDialog.ShowDialog() == DialogResult.OK)
                    {
                        string newHex = "#" + (_colorDialog.Color.ToArgb() & 0x00FFFFFF).ToString("X6");
                        
                        // Update Favorites: move to top, remove duplicates, limit to 16
                        favorites.Remove(newHex);
                        favorites.Insert(0, newHex);
                        if (favorites.Count > 16) favorites.RemoveAt(16);
                        
                        LocalizationManager.Settings.FavoriteColors = favorites;
                        LocalizationManager.SaveSettings();
                        
                        value = newHex;
                        editorService.CloseDropDown();
                    }
                }
            };

            editorService.DropDownControl(lb);
            return value;
        }
    }

    public class NpcArchetypeConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => true;
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(DialogueModels.NPC_ARCHETYPES);
        }
    }

    public class ConditionTypeConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => true;
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new string[] { "Reputation", "Flag", "GlobalBool", "Cash", "Recipe", "Item", "BT", "Courage", "Parameter", "SK", "QuestNode", "Angry", "Timer" });
        }
    }

    public class ActionTypeConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => true;
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new string[] { "GiveReputation", "SetFlag", "SetGlobalBool", "Pay", "GiveRecipe", "GiveItem", "ItemNotif", "GiveBT", "GiveCourage", "GiveParameter", "GiveBuff", "StartQuest", "QuestDone", "QuestFail", "SetAngry", "SetTimer" });
        }
    }

    public class ActionKeyConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context?.Instance is DialogueActionJson action)
            {
                if (action.type == "GiveBuff")
                    return new StandardValuesCollection(new string[] { "Healing", "Faith", "Cure", "Warming", "Confidence", "Deodor", "Raging" });
                if (action.type == "GiveParameter")
                    return new StandardValuesCollection(new string[] { "Health", "Food", "Morale", "Freshness", "Warm", "Wet", "Illness", "Toxicity", "Inebriety", "Greatneed", "Smell" });
            }
            return null;
        }
    }

    public class ActionValueConverter : Int32Converter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context?.Instance is DialogueActionJson action)
            {
                if (action.type == "GiveBuff")
                    return new StandardValuesCollection(new int[] { 1, 2, 3 });
            }
            return null;
        }

        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            if (context?.Instance is DialogueActionJson action && action.type == "GiveParameter")
            {
                if (value is int val) return val >= -100 && val <= 100;
                if (value is string s && int.TryParse(s, out int sVal)) return sVal >= -100 && sVal <= 100;
            }
            return base.IsValid(context, value);
        }
    }

    public class ConditionKeyConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context?.Instance is DialogueConditionJson cond)
            {
                if (cond.type == "SK")
                    return new StandardValuesCollection(new string[] { 
                        "DrunkerSK", "DumpRiderSK", "TacticOrdinarySK", "InsolenceSK", "TrollSK", 
                        "MuggerSK", "StreetSellerSK", "ResistanceSK", "DirtyThiefSK", "DirtyThief2SK", 
                        "CharmingSK", "Charming2SK", "Charming3SK", "KurazSK", "BulglarSK", 
                        "MechatronikSK", "IntuiceSK", "AdrenalinSK", "MistrMeceSK", "MasochistaSK", 
                        "FighterSK", "WeaponsmithSK", "PaserakSK", "Paserak2SK", "NotorikSK", 
                        "Notorik2SK", "Notorik3SK", "FetakSK", "Fetak2SK" 
                    });
            }
            return null;
        }
    }

    public static class DialogueModels
    {
        public static readonly string[] NPC_ARCHETYPES = new string[] {
            "NPC_M_ChaserWorker", "NPC_M_ChaserResident", "NPC_M_ChaserGauner", "Specific_Dory",
            "Specific_Ilona", "Specific_Hektor", "Pig_Quest", "Hobo_Furgrim", "Hobo_KolotukFranz",
            "Hobo_SmradochDan", "Hobo_Shocky", "Hobo_Dedek", "Hobo_Peta", "Hobo_Medved", "NPC_M_Easy",
            "Hobo_Valoun", "Hobo_Brekeke", "Hobo_SilenaEdit", "Hobo_PetrMelicharek", "Hobo_Cica",
            "Hobo_Polda", "Hobo_Ruda", "Specific_Anatoly", "Specific_Hanicka", "Specific_IrenaSkrabisova",
            "Specific_PorucikHajek", "Specific_MagdaMazalova", "Specific_PaterBurian", "Specific_Bruno",
            "NPC_M_Medium", "NPC_M_Asertivni", "NPC_M_Hard", "NPC_M_Balik", "NPC_M_Socka", "NPC_F_Easy",
            "NPC_F_Medium", "NPC_F_Strelenka", "NPC_F_Hard", "Inside_Franta", "Specific_Muller",
            "Specific_RabinEck", "Hobo_Smrad", "NPC_F_Pipina", "Inside_Lekarnik", "Inside_Barista",
            "NPC_M_VelkyPan", "Inside_ProdavacDrogerie", "Inside_ProdavacZelezarstvi", "Inside_Utulek",
            "NPC_M_HoboZmrd", "Inside_ProdavacElektro", "NPC_M_PolicajtUniversal", "Specific_NPC_PolicajtRukavicka",
            "Inside_ProdavacSekac", "Inside_ProdavacOutdoor", "Specific_Zbysek", "Specific_CallCentrum",
            "Inside_ProdavacObuv", "Inside_ProdavacOdevy", "Inside_ProdavacItPotreby", "Inside_ProdavacAntik",
            "Inside_ProdavacAsijskeBistro", "Inside_PanKrejci", "Inside_SlecnaMagda", "Personal_M_Normal",
            "Personal_M_Karel", "NPC_M_HuliBrk", "NPC_M_Sektar", "NPC_F_Introvert", "NPC_M_Nerd",
            "NPC_M_StudentTech", "NPC_F_StudentkaSoc", "Specific_StandaGrznar", "Specific_Emka",
            "Hobo_Horor", "Hobo_Meisner", "Hobo_Monty", "Inside_Prodavac1", "Inside_Prodavac2",
            "Inside_Trafikant1", "Inside_Hajzlbaba1", "Inside_PolicistaVeSluzbe", "Specific_BarmanShishaBar",
            "Specific_BarmanPajzl", "Specific_Master", "Inside_Security1", "NPC_M_Fanda",
            "Specific_Smelar", "Inside_Prodavac3", "Specific_MartinBach", "Specific_Kocour",
            "Specific_Chef", "Specific_LubosHollzer", "NPC_M_SocialButterfly", "Specific_LiborPetula",
            "Specific_SestraAnezka", "Specific_Loudova", "Specific_ZichVedouci", "Specific_Ivan",
            "Inside_Worker1", "Specific_Kotler", "Specific_Kadlec", "Specific_BratrMarek",
            "Specific_DrKrasny", "Inside_ZamestnanecEvropy2", "Inside_Kinometropol", "Inside_Charvat",
            "Inside_CiziMuz", "Inside_ObsluhaBenzinka", "Inside_Resident_M", "Hobo_Drax",
            "Hobo_Rejsek", "Nothing", "Hobo_Festr", "Dog_Rottweiler", "Dog_Shepherd",
            "Hobo_Majsner", "Hobo_Vanga", "Hobo_Jolanda", "Hobo_Langos", "Hobo_Kentus",
            "Hobo_Koblih", "Dog_Sheepdog", "Inside_PaniSpurna", "Inside_PanSpurny", "Dog_Quest",
            "Specific_PetrKubik", "PizzaCustomer1", "PizzaCustomer2", "PizzaCustomer3",
            "PizzaCustomer4", "PizzaCustomer5", "Specific_Baron", "Specific_Zelinar",
            "Specific_Antonin", "Hobo_MasaPetrankova", "Hobo_HonzaKonecny", "Hobo_Sergej",
            "Hobo_Struk", "Hobo_Mahoney", "Hobo_Bigas", "Hobo_Princ", "Hobo_LiborPanika",
            "Hobo_AlesSmutny", "Hobo_Homola", "Hobo_Herdyn", "Hobo_Mara", "Hobo_Veverka",
            "Hobo_Zachy", "Hobo_Kardinal", "Hobo_Jankins", "Hobo_Pepic", "Hobo_Bazooka",
            "Hobo_Rocky", "Hobo_Ferguson", "Hobo_Dezon", "Hobo_Moiser", "Hobo_Ramsy",
            "Hobo_Starky", "Hobo_Satanista", "Hobo_Crazy", "Hobo_Kemr", "Hobo_Rasken",
            "Hobo_Rita", "Specific_Anton", "Hobo_Bond", "Hobo_Britva", "Hobo_Ferenz",
            "Hobo_Rizek", "Hobo_Leos", "Hobo_Bodom", "Hobo_Miky", "Hobo_Mazal",
            "Hobo_Viktor", "Hobo_Mako", "Specific_MartinSoucek", "Hobo_Fin", "Hobo_Pajour",
            "Hobo_Kroll", "Hobo_Ghaul", "Specific_MestskaHlidka", "Specific_PolicistaKvapil",
            "NPC_Fetch", "Hobo_Chicco", "Hobo_Henry", "Hobo_Frix", "Hobo_Kashee",
            "Inside_BarmanJiskra", "Hobo_Marty", "Specific_PostaUrednice", "Specific_SlapkaLevna",
            "Specific_SlapkaFancy", "Specific_SlapkaNormal", "Hobo_Herold"
        };

        public static readonly string[] LANGUAGES = new string[] { "zh", "en", "cs", "es", "ja", "fr", "ru", "pl", "de" };
    }

    public class L10nItem
    {
        [ReadOnly(true)]
        [Category("Basic")]
        public string Language { get; set; } = "";

        [Category("Basic")]
        public string Text { get; set; } = "";

        [Browsable(false)]
        public object Parent { get; set; }

        public L10nItem(string lang, string text, object parent)
        {
            Language = lang;
            Text = text;
            Parent = parent;
        }

        public override string ToString() => $"[{Language}] {Text}";
    }

    public class DialogueActionJson
    {
        [Category("Basic")]
        [DisplayName("Type")]
        [TypeConverter(typeof(ActionTypeConverter))]
        [Editor(typeof(SearchableStringEditor), typeof(UITypeEditor))]
        [RefreshProperties(RefreshProperties.All)]
        public string type { get; set; } = "Pay";
        
        [Category("Basic")]
        [DisplayName("Key")]
        [TypeConverter(typeof(ActionKeyConverter))]
        public string key { get; set; } = "";
        
        [Category("Basic")]
        [DisplayName("Value")]
        [TypeConverter(typeof(ActionValueConverter))]
        public int value { get; set; }

        public override string ToString() => $"[Action] {type} {key} {value}";
    }

    public class DialogueConditionJson
    {
        [Category("Basic")]
        [DisplayName("Type")]
        [TypeConverter(typeof(ConditionTypeConverter))]
        [Editor(typeof(SearchableStringEditor), typeof(UITypeEditor))]
        [RefreshProperties(RefreshProperties.All)]
        public string type { get; set; } = "Cash";
        
        [Category("Basic")]
        [DisplayName("Key")]
        [TypeConverter(typeof(ConditionKeyConverter))]
        public string key { get; set; } = "";
        
        [Category("Basic")]
        [DisplayName("Value")]
        public int value { get; set; }

        public override string ToString() => $"[Condition] {type} {key} {value}";
    }

    public enum ShopType
    {
        Null = 0,
        Shop = 1,
        DeathShop = 2,
        Repair = 3
    }

    public class ShopTypeConverter : EnumConverter
    {
        public ShopTypeConverter() : base(typeof(ShopType)) { }
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => true;
    }

    public class DialogueOptionJson
    {
        [Category("Identification")]
        [DisplayName("ID")]
        public string id { get; set; } = "";
        
        [Category("Display")]
        [DisplayName("Text")]
        public string text { get; set; } = "New Option";
        
        [Category("Display")]
        [DisplayName("Color")]
        [Editor(typeof(HexColorEditor), typeof(UITypeEditor))]
        public string color { get; set; } = "";

        [Category("Service")]
        [DisplayName("Shop Type")]
        [TypeConverter(typeof(ShopTypeConverter))]
        public ShopType shopType { get; set; } = ShopType.Null;
        
        [Category("Behavior")]
        [DisplayName("Is Exit")]
        public bool isExit { get; set; }
        
        [Category("Behavior")]
        [DisplayName("Force Hide")]
        public bool forceHide { get; set; }

        [Browsable(false)]
        [JsonIgnore]
        public Dictionary<string, string> l10n { get; set; } = new Dictionary<string, string>();

        [Browsable(false)]
        public List<DialogueActionJson> actions { get; set; } = new List<DialogueActionJson>();
        
        [Browsable(false)]
        public List<DialogueConditionJson> conditions { get; set; } = new List<DialogueConditionJson>();
        
        [Browsable(false)]
        public List<DialogueReactionJson> reactions { get; set; } = new List<DialogueReactionJson>();

        public override string ToString() => $"[Option] {(l10n.ContainsKey("zh") && !string.IsNullOrEmpty(l10n["zh"]) ? id : text)}";
    }

    public class DialogueReactionJson
    {
        [Category("Identification")]
        [DisplayName("ID")]
        public string id { get; set; } = "";
        
        [Category("Display")]
        [DisplayName("Text")]
        public string text { get; set; } = "NPC Response";
        
        [Category("Behavior")]
        [DisplayName("Show Native Exit")]
        public bool showNativeExit { get; set; }

        [Browsable(false)]
        [JsonIgnore]
        public Dictionary<string, string> l10n { get; set; } = new Dictionary<string, string>();

        [Browsable(false)]
        public List<DialogueActionJson> actions { get; set; } = new List<DialogueActionJson>();
        
        [Browsable(false)]
        public List<DialogueConditionJson> conditions { get; set; } = new List<DialogueConditionJson>();
        
        [Browsable(false)]
        public List<DialogueOptionJson> options { get; set; } = new List<DialogueOptionJson>();

        public override string ToString() => $"[Reaction] {(l10n.ContainsKey("zh") && !string.IsNullOrEmpty(l10n["zh"]) ? id : text)}";
    }

    public class NpcDialogueJson
    {
        [Browsable(false)]
        [JsonIgnore]
        public string SourceFilePath { get; set; } = "";

        [Category("Identification")]
        [DisplayName("NPC Archetype")]
        [TypeConverter(typeof(NpcArchetypeConverter))]
        [Editor(typeof(SearchableStringEditor), typeof(UITypeEditor))]
        public string npcArchetype { get; set; } = "Hobo_Majsner";
        
        [Browsable(false)]
        public List<DialogueOptionJson> entryOptions { get; set; } = new List<DialogueOptionJson>();

        public override string ToString() => $"NPC: {npcArchetype}";
    }

    public class DialogueFileRoot
    {
        public List<NpcDialogueJson> dialogues { get; set; } = new List<NpcDialogueJson>();
    }

    public class DialogueL10n
    {
        public Dictionary<string, Dictionary<string, string>> strings { get; set; } = new Dictionary<string, Dictionary<string, string>>();
    }
}
