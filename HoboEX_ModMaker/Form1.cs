using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Windows.Forms;
using System.ComponentModel;
using HoboEX_ModMaker.Models;

namespace HoboEX_ModMaker
{
    public partial class Form1 : Form
    {
        private string currentFilePath = null;
        private DialogueFileRoot currentRoot = null;
        private RootJson currentItems = null;
        private ReputationRootJson currentReputations = null;
        private ModInfo currentModInfo = null;
        private object clipboardData = null;

        private ToolStripMenuItem addItemsJsonToolStripMenuItem;
        private ToolStripMenuItem addReputationsJsonToolStripMenuItem;
        private ToolStripMenuItem addOptionsDirToolStripMenuItem;

        public Form1()
        {
            LocalizationManager.LoadSettings();
            InitializeComponent();
            
            // Register localized type providers
            LocalizedTypeDescriptorProvider.Register(typeof(DialogueOptionJson));
            LocalizedTypeDescriptorProvider.Register(typeof(DialogueReactionJson));
            LocalizedTypeDescriptorProvider.Register(typeof(DialogueActionJson));
            LocalizedTypeDescriptorProvider.Register(typeof(DialogueConditionJson));
            LocalizedTypeDescriptorProvider.Register(typeof(NpcDialogueJson));
            LocalizedTypeDescriptorProvider.Register(typeof(NpcDialogueJson));
            LocalizedTypeDescriptorProvider.Register(typeof(L10nItem));

            // Register Items.json types
            LocalizedTypeDescriptorProvider.Register(typeof(ConsumableJsonItem));
            LocalizedTypeDescriptorProvider.Register(typeof(ScrapJsonItem));
            LocalizedTypeDescriptorProvider.Register(typeof(BagJsonItem));
            LocalizedTypeDescriptorProvider.Register(typeof(GearJsonItem));
            LocalizedTypeDescriptorProvider.Register(typeof(RecipeJson));
            LocalizedTypeDescriptorProvider.Register(typeof(ShopJsonItem));
            LocalizedTypeDescriptorProvider.Register(typeof(PackageTableJsonItem));
            LocalizedTypeDescriptorProvider.Register(typeof(SalvagePatternJson));
            LocalizedTypeDescriptorProvider.Register(typeof(ArchetypeJson));
            LocalizedTypeDescriptorProvider.Register(typeof(ReputationJson));
            LocalizedTypeDescriptorProvider.Register(typeof(ModInfo));

            // Initialize extra menu items
            addItemsJsonToolStripMenuItem = new ToolStripMenuItem();
            addItemsJsonToolStripMenuItem.Click += (s, e) => { currentItems = new RootJson(); UpdateTree(); };
            
            addReputationsJsonToolStripMenuItem = new ToolStripMenuItem();
            addReputationsJsonToolStripMenuItem.Click += (s, e) => { currentReputations = new ReputationRootJson(); UpdateTree(); };
            
            addOptionsDirToolStripMenuItem = new ToolStripMenuItem();
            addOptionsDirToolStripMenuItem.Click += (s, e) => { currentRoot = new DialogueFileRoot(); UpdateTree(); };

            contextMenuStrip1.Items.Insert(0, addItemsJsonToolStripMenuItem);
            contextMenuStrip1.Items.Insert(1, addReputationsJsonToolStripMenuItem);
            contextMenuStrip1.Items.Insert(2, addOptionsDirToolStripMenuItem);

            // Resize PropertyGrid Description Area
            foreach (Control c in propertyGrid1.Controls)
            {
                if (c.GetType().Name == "DocComment")
                {
                    c.Height = 120; // Increase height
                    break;
                }
            }

            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
            this.StartPosition = FormStartPosition.CenterScreen;
            ApplyLocalization();

            
            treeView1.AllowDrop = true;
            treeView1.ItemDrag += treeView1_ItemDrag;
            treeView1.DragEnter += treeView1_DragEnter;
            treeView1.DragDrop += treeView1_DragDrop;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // Global shortcuts
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.SuppressKeyPress = true;
                saveToolStripMenuItem_Click(sender, e);
                return;
            }
            if (e.KeyCode == Keys.F5)
            {
                e.SuppressKeyPress = true;
                previewToolStripMenuItem_Click(sender, e);
                return;
            }

            // TreeView shortcuts - only intercept when TreeView is active
            if (treeView1.Focused)
            {
                if (e.Control && e.KeyCode == Keys.C)
                {
                    e.SuppressKeyPress = true;
                    copyToolStripMenuItem_Click(sender, e);
                }
                else if (e.Control && e.KeyCode == Keys.X)
                {
                    e.SuppressKeyPress = true;
                    cutToolStripMenuItem_Click(sender, e);
                }
                else if (e.Control && e.KeyCode == Keys.V)
                {
                    e.SuppressKeyPress = true;
                    pasteToolStripMenuItem_Click(sender, e);
                }
            }
        }

        private void ApplyLocalization()
        {
            // Menu
            fileToolStripMenuItem.Text = LocalizationManager.Get("MenuFile");
            newToolStripMenuItem.Text = LocalizationManager.Get("MenuNew");
            saveToolStripMenuItem.Text = LocalizationManager.Get("MenuSave");
            saveAsToolStripMenuItem.Text = LocalizationManager.Get("MenuSaveAs");
            languageToolStripMenuItem.Text = LocalizationManager.Get("MenuLanguage");
            toolsToolStripMenuItem.Text = LocalizationManager.Get("MenuTools");
            previewToolStripMenuItem.Text = LocalizationManager.Get("MenuPreview");
            aiL10nToolStripMenuItem.Text = LocalizationManager.Get("MenuAiL10n");
            syncL10nToolStripMenuItem.Text = LocalizationManager.Get("MenuSyncL10n");
            aiSettingsToolStripMenuItem.Text = LocalizationManager.Get("AiSettingsTitle");
            openModToolStripMenuItem.Text = LocalizationManager.Get("MenuOpenMod") ?? "Open Mod";

            // Status
            toolStripStatusLabel1.Text = LocalizationManager.Get("Ready");

            // Context Menu (titles updated in Opening event usually, but set defaults)
            deleteToolStripMenuItem.Text = LocalizationManager.Get("ContextDelete");

            this.Text = "HoboEX Mod Editor";

            if (currentRoot != null) UpdateTree();
        }

        private void NewMod()
        {
            currentFilePath = null;
            currentRoot = null; 
            currentItems = null;
            currentReputations = null;
            currentModInfo = new ModInfo { name = "New Mod" };
            UpdateTree();
        }

        private bool IsModFolderMode()
        {
            return !string.IsNullOrEmpty(currentFilePath) && Directory.Exists(currentFilePath);
        }

        private void UpdateTree()
        {
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();
            
            bool isModFolder = IsModFolderMode();
            if (currentModInfo == null && currentRoot == null)
            {
                treeView1.EndUpdate();
                return;
            }

            if (currentModInfo != null)
            {
                // Root is the Mod Folder (Parent of options)
                string modName = currentModInfo.name;
                if (string.IsNullOrEmpty(modName))
                {
                    string optionsPath = currentFilePath;
                    string modPath = (!string.IsNullOrEmpty(optionsPath) && Directory.Exists(optionsPath)) ? (Directory.GetParent(optionsPath)?.FullName ?? optionsPath) : "";
                    modName = !string.IsNullOrEmpty(modPath) ? Path.GetFileName(modPath) : "New Mod";
                }

                var modRootNode = new TreeNode(modName) { Tag = currentModInfo, ImageKey = "folder", SelectedImageKey = "folder" }; 
                treeView1.Nodes.Add(modRootNode);

                // 1. Items.json Node
                if (currentItems != null)
                {
                    var itemsNode = new TreeNode("items.json") { Tag = currentItems, ImageKey = "file", SelectedImageKey = "file" };
                    modRootNode.Nodes.Add(itemsNode);

                    void AddCategory<T>(string key, IEnumerable<T> items)
                    {
                        if (items == null || !items.Any()) return;
                        string title = LocalizationManager.Get(key) ?? key.Replace("Node", "");
                        var catNode = new TreeNode(title) { Tag = items, ImageKey = "folder", SelectedImageKey = "folder" };
                        itemsNode.Nodes.Add(catNode);
                        foreach (var item in items)
                        {
                            catNode.Nodes.Add(new TreeNode(item.ToString()) { Tag = item });
                        }
                    }

                    AddCategory("NodeConsumables", currentItems.consumables);
                    AddCategory("NodeScraps", currentItems.scraps);
                    AddCategory("NodeBags", currentItems.bags);
                    AddCategory("NodeGears", currentItems.gears);
                    AddCategory("NodeRecipes", currentItems.recipes);
                    AddCategory("NodeShops", currentItems.shops);
                    AddCategory("NodeSells", currentItems.sells);
                    AddCategory("NodePackages", currentItems.packageTables);
                    AddCategory("NodeSalvage", currentItems.salvagePatterns);
                    AddCategory("NodeArchetypes", currentItems.archetypes);
                }

                // 2. Reputations.json Node
                if (currentReputations != null)
                {
                    string title = LocalizationManager.Get("NodeReputations") ?? "Reputations";
                    var repFileNode = new TreeNode("reputations.json") { Tag = currentReputations, ImageKey = "file", SelectedImageKey = "file" };
                    modRootNode.Nodes.Add(repFileNode);

                    var catNode = new TreeNode(title) { Tag = currentReputations.reputations, ImageKey = "folder", SelectedImageKey = "folder" };
                    repFileNode.Nodes.Add(catNode);
                    foreach (var rep in currentReputations.reputations)
                    {
                        catNode.Nodes.Add(new TreeNode(rep.ToString()) { Tag = rep });
                    }
                }

                // 3. Options Folder Node
                if (currentRoot != null)
                {
                    string optionsName = "options";
                    var optionsNode = new TreeNode(optionsName) { Tag = currentRoot, ImageKey = "folder", SelectedImageKey = "folder" };
                    modRootNode.Nodes.Add(optionsNode);

                    // Fill Options Content
                    AddOptionsContent(optionsNode);
                }
                
                modRootNode.Expand();
            }
            else
            {
                // Old-style or empty Dialogue Root
                string rootText = string.IsNullOrEmpty(currentFilePath) 
                    ? LocalizationManager.Get("RootNode") 
                    : Path.GetFileName(currentFilePath);
                var rootNode = new TreeNode(rootText) { Tag = currentRoot };
                treeView1.Nodes.Add(rootNode);

                AddOptionsContent(rootNode);
            }

            treeView1.EndUpdate();
        }

        private void AddOptionsContent(TreeNode rootNode)
        {
            if (IsModFolderMode())
            {
                var files = Directory.GetFiles(currentFilePath, "*.json", SearchOption.TopDirectoryOnly)
                    .Where(f => !f.EndsWith("_l10n.json"))
                    .OrderBy(f => f)
                    .ToList();

                foreach (var file in files)
                {
                    string fileName = Path.GetFileName(file);
                    var fileNode = new TreeNode(fileName) { Tag = file };
                    rootNode.Nodes.Add(fileNode);

                    var npcs = currentRoot.dialogues.Where(d => d.SourceFilePath == file);
                    foreach (var npc in npcs)
                    {
                        var npcNode = new TreeNode(npc.ToString()) { Tag = npc };
                        fileNode.Nodes.Add(npcNode);
                        foreach (var opt in npc.entryOptions) AddOptionNode(npcNode, opt);
                    }
                }
            }
            else
            {
                foreach (var npc in currentRoot.dialogues)
                {
                    var npcNode = new TreeNode(npc.ToString()) { Tag = npc };
                    rootNode.Nodes.Add(npcNode);
                    foreach (var opt in npc.entryOptions) AddOptionNode(npcNode, opt);
                }
            }
        }

        private void AddOptionNode(TreeNode parent, DialogueOptionJson opt)
        {
            var node = new TreeNode(opt.ToString()) { Tag = opt };
            parent.Nodes.Add(node);
            RefreshOptionNode(node);
        }

        private void RefreshOptionNode(TreeNode node)
        {
            if (node.Tag is not DialogueOptionJson opt) return;

            node.Nodes.Clear();

            // Conditions Group
            if (opt.conditions.Count > 0)
            {
                var condsNode = new TreeNode(LocalizationManager.Get("NodeConditions")) { ImageKey = "folder", SelectedImageKey = "folder" };
                node.Nodes.Add(condsNode);
                foreach (var cond in opt.conditions)
                    condsNode.Nodes.Add(new TreeNode(cond.ToString()) { Tag = cond });
            }

            // Actions Group
            if (opt.actions.Count > 0)
            {
                var actsNode = new TreeNode(LocalizationManager.Get("NodeActions")) { ImageKey = "folder", SelectedImageKey = "folder" };
                node.Nodes.Add(actsNode);
                foreach (var act in opt.actions)
                    actsNode.Nodes.Add(new TreeNode(act.ToString()) { Tag = act });
            }

            // L10n Group
            if (opt.l10n.Count > 0)
            {
                var l10nNode = new TreeNode(LocalizationManager.Get("NodeL10n")) { ImageKey = "folder", SelectedImageKey = "folder" };
                node.Nodes.Add(l10nNode);
                foreach (var kvp in opt.l10n)
                    l10nNode.Nodes.Add(new TreeNode(new L10nItem(kvp.Key, kvp.Value, opt).ToString()) { Tag = new L10nItem(kvp.Key, kvp.Value, opt) });
                //l10nNode.Expand();
            }

            foreach (var react in opt.reactions)
                AddReactionNode(node, react);
        }

        private void RecalculateNodeText(TreeNode node)
        {
             if (node.Tag != null) node.Text = node.Tag.ToString();
        }

        private void AddReactionNode(TreeNode parent, DialogueReactionJson react)
        {
            var node = new TreeNode(react.ToString()) { Tag = react };
            parent.Nodes.Add(node);
            RefreshReactionNode(node);
        }

        private void RefreshReactionNode(TreeNode node)
        {
            if (node.Tag is not DialogueReactionJson react) return;

            node.Nodes.Clear();

            // Actions Group
            if (react.actions.Count > 0)
            {
                var actsNode = new TreeNode(LocalizationManager.Get("NodeActions")) { ImageKey = "folder", SelectedImageKey = "folder" };
                node.Nodes.Add(actsNode);
                foreach (var act in react.actions)
                    actsNode.Nodes.Add(new TreeNode(act.ToString()) { Tag = act });
            }

            // Conditions Group
            if (react.conditions.Count > 0)
            {
                var condsNode = new TreeNode(LocalizationManager.Get("NodeConditions")) { ImageKey = "folder", SelectedImageKey = "folder" };
                node.Nodes.Add(condsNode);
                foreach (var cond in react.conditions)
                    condsNode.Nodes.Add(new TreeNode(cond.ToString()) { Tag = cond });
            }

            // L10n Group
            if (react.l10n.Count > 0)
            {
                var l10nNode = new TreeNode(LocalizationManager.Get("NodeL10n")) { ImageKey = "folder", SelectedImageKey = "folder" };
                node.Nodes.Add(l10nNode);
                foreach (var kvp in react.l10n)
                    l10nNode.Nodes.Add(new TreeNode(new L10nItem(kvp.Key, kvp.Value, react).ToString()) { Tag = new L10nItem(kvp.Key, kvp.Value, react) });
                //l10nNode.Expand();
            }

            foreach (var opt in react.options)
                AddOptionNode(node, opt);
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            propertyGrid1.SelectedObject = e.Node.Tag;

            // Auto expand collection arrays (changes and parameterChanges)
            if (e.Node.Tag != null)
            {
                var tagType = e.Node.Tag.GetType();
                if (tagType == typeof(ConsumableJsonItem) || tagType == typeof(GearJsonItem) || tagType == typeof(RecipeJson))
                {
                    // Expand all categories and specific arrays
                    propertyGrid1.ExpandAllGridItems();
                }
            }
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeView1.SelectedNode = e.Node;
            }
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            var node = treeView1.SelectedNode;
            if (node != null && node.Tag != null)
            {
                // ID Duplication Check
                if (e.ChangedItem.PropertyDescriptor.Name == "id" && e.OldValue != e.ChangedItem.Value)
                {
                    int newId = (int)e.ChangedItem.Value;
                    bool isDuplicate = false;
                    
                    // Shared Pool: Consumables, Scraps, Bags, Gears
                    // If the item falls into this category, check ALL lists
                    bool isSharedType = node.Tag is ConsumableJsonItem || node.Tag is ScrapJsonItem || 
                                        node.Tag is BagJsonItem || node.Tag is GearJsonItem;

                    if (isSharedType)
                    {
                        var allIds = GetAllGlobalIds();
                        // Count occurrences. If newId appears more than once (meaning it's already used elsewhere in the pool),
                        // or if it's new and already exists in pool.
                        // Since we just changed the value, 'currentItems' currently holds the object with the *new* value (property grid updates reference).
                        // Wait, property grid updates the object directly. So the object ALREADY has newId.
                        // We need to check if ANY OTHER object has this ID.
                        
                        int count = 0;
                        count += currentItems.consumables.Count(x => x.id == newId);
                        count += currentItems.scraps.Count(x => x.id == newId);
                        count += currentItems.bags.Count(x => x.id == newId);
                        count += currentItems.gears.Count(x => x.id == newId);
                        
                        if (count > 1) isDuplicate = true; // 1 occurrence is itself
                    }
                    else if (node.Tag is RecipeJson && currentItems.recipes.Any(x => x != node.Tag && x.id == newId)) isDuplicate = true;
                    else if (node.Tag is SalvagePatternJson && currentItems.salvagePatterns.Any(x => x != node.Tag && x.id == newId)) isDuplicate = true;
                    // ... other types checking ...

                    if (isDuplicate)
                    {
                        MessageBox.Show($"ID {newId} already exists in the Global Item Pool (Consumables/Scraps/Bags/Gears)! Reverting...", "Duplicate ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        e.ChangedItem.PropertyDescriptor.SetValue(node.Tag, e.OldValue);
                        return;
                    }
                }

                if (node.Tag is L10nItem item)
                {
                    if (item.Parent is DialogueOptionJson opt) opt.l10n[item.Language] = item.Text;
                    else if (item.Parent is DialogueReactionJson react) react.l10n[item.Language] = react.l10n[item.Language] = item.Text;
                }
                
                // Recalculate text
                RecalculateNodeText(node);
                
                // If it's the main text of an option/reaction, we might want to refresh the parent label
                if (node.Parent != null)
                {
                    if (node.Parent.Tag is DialogueOptionJson || node.Parent.Tag is DialogueReactionJson)
                        node.Parent.Text = node.Parent.Tag.ToString();
                    else if (node.Parent.Parent != null && (node.Parent.Parent.Tag is DialogueOptionJson || node.Parent.Parent.Tag is DialogueReactionJson))
                         node.Parent.Parent.Text = node.Parent.Parent.Tag.ToString();
                }
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewMod();
        }

        private void openModToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string path = fbd.SelectedPath;
                    string optionsPath = path;
                    if (Path.GetFileName(path).ToLower() != "options")
                    {
                        string subPath = Path.Combine(path, "options");
                        if (Directory.Exists(subPath)) optionsPath = subPath;
                        else
                        {
                            MessageBox.Show("Selected folder must be 'options' or contain an 'options' subfolder.");
                            return;
                        }
                    }

                    LoadModFolder(optionsPath);
                }
            }
        }

        private void LoadModFolder(string folderPath)
        {
            try
            {
                currentRoot = new DialogueFileRoot();
                currentFilePath = folderPath;
                var files = Directory.GetFiles(folderPath, "*.json", SearchOption.TopDirectoryOnly)
                    .Where(f => !f.EndsWith("_l10n.json"))
                    .ToList();

                foreach (var file in files)
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        var loaded = JsonSerializer.Deserialize<DialogueFileRoot>(json);
                        if (loaded != null && loaded.dialogues != null)
                        {
                            foreach (var npc in loaded.dialogues)
                            {
                                npc.SourceFilePath = file; // Store source file path
                                currentRoot.dialogues.Add(npc);
                                // Load and sync L10n for THIS specific file's dialogues
                                string l10nPath = file.Replace(".json", "_l10n.json");
                                if (File.Exists(l10nPath))
                                {
                                    string l10nJson = File.ReadAllText(l10nPath);
                                    var l10n = JsonSerializer.Deserialize<DialogueL10n>(l10nJson);
                                    if (l10n != null) SyncL10nToModels(npc, l10n);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"Failed to load {file}: {ex.Message}", true);
                    }
                }

                
                // Try to load items.json and reputations.json from parent directory
                try
                {
                    string parentDir = Directory.GetParent(folderPath)?.FullName;
                    if (!string.IsNullOrEmpty(parentDir))
                    {
                        string itemsPath = Path.Combine(parentDir, "items.json");
                        if (File.Exists(itemsPath))
                        {
                            string json = File.ReadAllText(itemsPath);
                            currentItems = JsonSerializer.Deserialize<RootJson>(json);
                        }
                        else currentItems = null;

                        string repsPath = Path.Combine(parentDir, "reputations.json");
                        if (File.Exists(repsPath))
                        {
                            string json = File.ReadAllText(repsPath);
                            currentReputations = JsonSerializer.Deserialize<ReputationRootJson>(json);
                        }
                        else currentReputations = null;

                        string infoPath = Path.Combine(parentDir, "info.json");
                        if (File.Exists(infoPath))
                        {
                            string json = File.ReadAllText(infoPath);
                            currentModInfo = JsonSerializer.Deserialize<ModInfo>(json);
                        }
                        else currentModInfo = new ModInfo { name = Path.GetFileName(parentDir) };
                    }
                }
                catch (Exception ex)
                {
                    Log($"Failed to load items/reputations: {ex.Message}", true);
                    currentItems = null;
                    currentReputations = null;
                }

                UpdateTree();
                toolStripStatusLabel1.Text = $"Loaded mod folder: {Path.GetFileName(folderPath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading mod: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Log(string message, bool isError = false)
        {
            if (txtConsole.InvokeRequired)
            {
                txtConsole.Invoke(new Action(() => Log(message, isError)));
                return;
            }
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            txtConsole.AppendText($"[{timestamp}] {message}{Environment.NewLine}");
            if (isError) {
                int start = txtConsole.TextLength - message.Length - timestamp.Length - 4;
                txtConsole.Select(start, message.Length + timestamp.Length + 4);
                txtConsole.SelectionColor = Color.Red;
                txtConsole.DeselectAll();
            }
            txtConsole.ScrollToCaret();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                saveAsToolStripMenuItem_Click(sender, e);
            }
            else
            {
                Save(currentFilePath);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string path = Path.Combine(fbd.SelectedPath, "options");
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    Save(path);
                }
            }
        }

        private void Save(string path)
        {
            try
            {
                // Check if path is actually a directory (happens when using Open Mod)
                if (Directory.Exists(path))
                {
                    // Group dialogues by their source file path
                    var groups = currentRoot.dialogues
                        .Where(d => !string.IsNullOrEmpty(d.SourceFilePath))
                        .GroupBy(d => d.SourceFilePath);

                    int savedCount = 0;
                    foreach (var group in groups)
                    {
                        string filePath = group.Key;
                        var rootForFile = new DialogueFileRoot { dialogues = group.ToList() };
                        
                        SaveFileAndL10n(filePath, rootForFile);
                        savedCount++;
                    }

                    // Handle dialogues WITHOUT source path (manually added in new mod)
                    var unsaved = currentRoot.dialogues.Where(d => string.IsNullOrEmpty(d.SourceFilePath)).ToList();
                    if (unsaved.Count > 0)
                    {
                        string defaultPath = Path.Combine(path, "mod_dialogues.json");
                        var rootForFile = new DialogueFileRoot { dialogues = unsaved };
                        SaveFileAndL10n(defaultPath, rootForFile);
                        foreach (var npc in unsaved) npc.SourceFilePath = defaultPath;
                        savedCount++;
                    }

                    // Save items.json if currentItems is active
                    var saveOptions = new JsonSerializerOptions { 
                        WriteIndented = true, 
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                    };

                    if (currentItems != null)
                    {
                        string parentDir = Directory.GetParent(path)?.FullName;
                        if (!string.IsNullOrEmpty(parentDir))
                        {
                            string itemsPath = Path.Combine(parentDir, "items.json");
                            File.WriteAllText(itemsPath, JsonSerializer.Serialize(currentItems, saveOptions));
                            Log($"Saved items.json to {itemsPath}");
                        }
                    }

                    // Save reputations.json if currentReputations is active
                    if (currentReputations != null)
                    {
                        string parentDir = Directory.GetParent(path)?.FullName;
                        if (!string.IsNullOrEmpty(parentDir))
                        {
                            string repsPath = Path.Combine(parentDir, "reputations.json");
                            File.WriteAllText(repsPath, JsonSerializer.Serialize(currentReputations, saveOptions));
                            Log($"Saved reputations.json to {repsPath}");
                        }
                    }

                    // Save info.json if currentModInfo is active
                    if (currentModInfo != null)
                    {
                        string parentDir = Directory.GetParent(path)?.FullName;
                        if (!string.IsNullOrEmpty(parentDir))
                        {
                            string infoPath = Path.Combine(parentDir, "info.json");
                            File.WriteAllText(infoPath, JsonSerializer.Serialize(currentModInfo, saveOptions));
                            Log($"Saved info.json to {infoPath}");
                        }
                    }

                    if (savedCount > 0)
                    {
                        toolStripStatusLabel1.Text = string.Format(LocalizationManager.Get("Msg_ModSavedWithItems"), savedCount);
                    }
                    else
                    {
                        // Fallback if no source paths (new NPC added in folder mode)
                        saveAsToolStripMenuItem_Click(null, null);
                    }
                    return;
                }

                SaveFileAndL10n(path, currentRoot);
                currentFilePath = path;
                toolStripStatusLabel1.Text = string.Format(LocalizationManager.Get("FileSaved"), Path.GetFileName(path));
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(LocalizationManager.Get("SaveError"), ex.Message), LocalizationManager.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveFileAndL10n(string path, DialogueFileRoot root)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            string json = JsonSerializer.Serialize(root, options);
            File.WriteAllText(path, json);
            
            // Save L10n specifically for the dialogues in this root
            var l10n = new DialogueL10n();
            GatherL10nFromModels(root, l10n);
            if (l10n.strings.Count > 0)
            {
                string l10nPath = path.Replace(".json", "_l10n.json");
                File.WriteAllText(l10nPath, JsonSerializer.Serialize(l10n, options));
            }
        }

        private void LoadL10n(string path)
        {
            string l10nPath = path.Replace(".json", "_l10n.json");
            if (File.Exists(l10nPath))
            {
                try
                {
                    string json = File.ReadAllText(l10nPath);
                    var l10n = JsonSerializer.Deserialize<DialogueL10n>(json);
                    if (l10n != null)
                    {
                        SyncL10nToModels(currentRoot, l10n);
                    }
                }
                catch { }
            }
        }

        private void SyncL10nToModels(object root, DialogueL10n l10n)
        {
            if (root is DialogueFileRoot fileRoot)
                foreach (var npc in fileRoot.dialogues) SyncL10nToModels(npc, l10n);
            else if (root is NpcDialogueJson npc)
                foreach (var opt in npc.entryOptions) SyncL10nToModels(opt, l10n);
            else if (root is DialogueOptionJson opt)
            {
                if (!string.IsNullOrEmpty(opt.id) && l10n.strings.TryGetValue(opt.id, out var entries))
                {
                    foreach (var kvp in entries) opt.l10n[kvp.Key] = kvp.Value;
                }
                foreach (var r in opt.reactions) SyncL10nToModels(r, l10n);
            }
            else if (root is DialogueReactionJson react)
            {
                if (!string.IsNullOrEmpty(react.id) && l10n.strings.TryGetValue(react.id, out var entries))
                {
                    foreach (var kvp in entries) react.l10n[kvp.Key] = kvp.Value;
                }
                foreach (var o in react.options) SyncL10nToModels(o, l10n);
            }
        }

        private void SaveL10n(string path)
        {
            var l10n = new DialogueL10n();
            GatherL10nFromModels(currentRoot, l10n);
            if (l10n.strings.Count > 0)
            {
                string l10nPath = path.Replace(".json", "_l10n.json");
                var options = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
                File.WriteAllText(l10nPath, JsonSerializer.Serialize(l10n, options));
            }
        }

        private void GatherL10nFromModels(object root, DialogueL10n l10n)
        {
            if (root is DialogueFileRoot fileRoot)
                foreach (var npc in fileRoot.dialogues) GatherL10nFromModels(npc, l10n);
            else if (root is NpcDialogueJson npc)
                foreach (var opt in npc.entryOptions) GatherL10nFromModels(opt, l10n);
            else if (root is DialogueOptionJson opt)
            {
                if (!string.IsNullOrEmpty(opt.id) && opt.l10n.Count > 0)
                {
                    l10n.strings[opt.id] = new Dictionary<string, string>(opt.l10n);
                }
                foreach (var r in opt.reactions) GatherL10nFromModels(r, l10n);
            }
            else if (root is DialogueReactionJson react)
            {
                if (!string.IsNullOrEmpty(react.id) && react.l10n.Count > 0)
                {
                    l10n.strings[react.id] = new Dictionary<string, string>(react.l10n);
                }
                foreach (var o in react.options) GatherL10nFromModels(o, l10n);
            }
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var node = treeView1.SelectedNode;
            if (node == null) { e.Cancel = true; return; }

            object tag = node.Tag;
            bool isActionsNode = node.Text == LocalizationManager.Get("NodeActions");
            bool isConditionsNode = node.Text == LocalizationManager.Get("NodeConditions");
            bool isL10nNode = node.Text == (LocalizationManager.Get("NodeL10n") ?? "Localization");

            // Sub-Item Categories
            bool isConsumables = node.Text == LocalizationManager.Get("NodeConsumables");
            bool isScraps = node.Text == LocalizationManager.Get("NodeScraps");
            bool isBags = node.Text == LocalizationManager.Get("NodeBags");
            bool isGears = node.Text == LocalizationManager.Get("NodeGears");
            bool isRecipes = node.Text == LocalizationManager.Get("NodeRecipes");
            bool isShops = node.Text == LocalizationManager.Get("NodeShops");
            bool isSells = node.Text == LocalizationManager.Get("NodeSells");
            bool isPackages = node.Text == LocalizationManager.Get("NodePackages");
            bool isSalvage = node.Text == LocalizationManager.Get("NodeSalvage");
            bool isArchetypes = node.Text == LocalizationManager.Get("NodeArchetypes");
            bool isReputations = node.Text == LocalizationManager.Get("NodeReputations");

            object parentTag = node.Tag;
            bool canAddItem = (isConsumables || isScraps || isBags || isGears || isRecipes || isShops || isSells || isPackages || isSalvage || isArchetypes) && currentItems != null;
            canAddItem |= isReputations && currentReputations != null;

            // 1. Logic States
            bool isModFolder = IsModFolderMode();
            bool isFileNode = tag is string && !string.IsNullOrEmpty((string)tag);

            bool canAddOption = (tag is DialogueFileRoot && !isModFolder) || isFileNode || tag is NpcDialogueJson || tag is DialogueReactionJson;
            bool canAddReaction = tag is DialogueOptionJson;
            bool canAddAction = tag is DialogueOptionJson || tag is DialogueReactionJson || isActionsNode;
            bool canAddCondition = tag is DialogueOptionJson || tag is DialogueReactionJson || isConditionsNode;
            bool canAddL10n = tag is DialogueOptionJson || tag is DialogueReactionJson || isL10nNode;

            bool isDataNode = tag is DialogueActionJson || tag is DialogueConditionJson || tag is DialogueOptionJson || tag is DialogueReactionJson;
            bool canCopy = isDataNode;
            bool canCut = isDataNode && !(tag is DialogueOptionJson && node.Parent?.Tag is NpcDialogueJson); 
            
            bool canPaste = false;
            if (clipboardData != null)
            {
                if (clipboardData is DialogueActionJson && (tag is DialogueOptionJson || tag is DialogueReactionJson || isActionsNode)) canPaste = true;
                else if (clipboardData is DialogueConditionJson && (tag is DialogueOptionJson || tag is DialogueReactionJson || isConditionsNode)) canPaste = true;
                else if (clipboardData is DialogueOptionJson && (tag is DialogueReactionJson || tag is NpcDialogueJson)) canPaste = true;
                else if (clipboardData is DialogueReactionJson && tag is DialogueOptionJson) canPaste = true;
            }
            bool canShowPaste = tag is DialogueOptionJson || tag is DialogueReactionJson || tag is NpcDialogueJson || isActionsNode || isConditionsNode;
            bool canDelete = !(tag is DialogueFileRoot) && !isFileNode; // Prevent deleting the file node wrapper itself in mod mode 
            bool isModRoot = tag is ModInfo;

            // 2. Apply Visibility & Localization
            addItemsJsonToolStripMenuItem.Visible = isModRoot && currentItems == null;
            addItemsJsonToolStripMenuItem.Text = LocalizationManager.Get("ContextAddItemsJson");

            addReputationsJsonToolStripMenuItem.Visible = isModRoot && currentReputations == null;
            addReputationsJsonToolStripMenuItem.Text = LocalizationManager.Get("ContextAddReputationsJson");

            addOptionsDirToolStripMenuItem.Visible = isModRoot && currentRoot == null;
            addOptionsDirToolStripMenuItem.Text = LocalizationManager.Get("ContextAddOptionsDir");

            bool canAddFile = tag is DialogueFileRoot && isModFolder;
            addFileToolStripMenuItem.Visible = canAddFile;
            addFileToolStripMenuItem.Text = LocalizationManager.Get("MenuAddFile") ?? "Add Dialogue File (.json)";

            addOptionToolStripMenuItem.Visible = canAddOption;
            addOptionToolStripMenuItem.Text = (tag is DialogueFileRoot || isFileNode) ? LocalizationManager.Get("ContextAddNPC") : LocalizationManager.Get("ContextAddOption");
            
            addReactionToolStripMenuItem.Visible = canAddReaction;
            addReactionToolStripMenuItem.Text = LocalizationManager.Get("ContextAddReaction");
            
            addActionToolStripMenuItem.Visible = canAddAction;
            addActionToolStripMenuItem.Text = LocalizationManager.Get("ContextAddAction");
            
            addConditionToolStripMenuItem.Visible = canAddCondition;
            addConditionToolStripMenuItem.Text = LocalizationManager.Get("ContextAddCondition");
            
            addL10nToolStripMenuItem.Visible = canAddL10n;
            addL10nToolStripMenuItem.Text = LocalizationManager.Get("ContextAddL10n") ?? "Add Localization";
            
            copyToolStripMenuItem.Visible = canCopy;
            copyToolStripMenuItem.Text = LocalizationManager.Get("ContextCopy");
            
            cutToolStripMenuItem.Visible = canCut;
            cutToolStripMenuItem.Text = LocalizationManager.Get("ContextCut");
            
            pasteToolStripMenuItem.Visible = canShowPaste;
            pasteToolStripMenuItem.Enabled = canPaste;
            pasteToolStripMenuItem.Text = LocalizationManager.Get("ContextPaste");
            
            deleteToolStripMenuItem.Visible = canDelete;
            deleteToolStripMenuItem.Text = LocalizationManager.Get("ContextDelete");

            // 3. Separator Management (Use Available for better Reliability)
            bool anyAdd = canAddOption || canAddReaction || canAddAction || canAddCondition || canAddL10n || canAddItem;
            bool anyEdit = canCopy || canCut || canShowPaste;
            bool anyDelete = canDelete;
            
            if (canAddItem)
            {
                addOptionToolStripMenuItem.Visible = true;
                addOptionToolStripMenuItem.Text = LocalizationManager.Get("ContextAddItem") ?? "Add New Item";
                // Redirect click event if needed or handle carefully in the main handler
                // Since we reuse the same menu item, we need to check context in the handler
            }

            toolStripSeparator1.Available = anyAdd && (anyEdit || anyDelete);
            toolStripSeparator2.Available = anyEdit && anyDelete;
        }

        private string GenerateUniqueID(TreeNode contextNode, string typeSuffix, HashSet<string>? existingIds = null)
        {
            // Find NPC archetype from ancestors
            string npcName = "UNKNOWN";
            TreeNode curr = contextNode;
            while (curr != null)
            {
                if (curr.Tag is NpcDialogueJson npc)
                {
                    npcName = npc.npcArchetype;
                    if (npcName.Contains("_")) npcName = npcName.Substring(npcName.LastIndexOf("_") + 1);
                    break;
                }
                curr = curr.Parent;
            }

            string basePrefix = $"MOD_{npcName.ToUpper()}_{typeSuffix.ToUpper()}";

            // If no set provided, gather all current IDs in the file
            if (existingIds == null)
            {
                existingIds = new HashSet<string>();
                GatherIds(currentRoot, existingIds);
            }

            int seq = 1;
            while (existingIds.Contains($"{basePrefix}_{seq}"))
            {
                seq++;
            }

            string finalId = $"{basePrefix}_{seq}";
            existingIds.Add(finalId); // Important: add to set so subsequent calls in a loop see it
            return finalId;
        }

        private void GatherIds(object root, HashSet<string> ids)
        {
            if (root is DialogueFileRoot fileRoot)
            {
                foreach (var npc in fileRoot.dialogues) GatherIds(npc, ids);
            }
            else if (root is NpcDialogueJson npc)
            {
                foreach (var opt in npc.entryOptions) GatherIds(opt, ids);
            }
            else if (root is DialogueOptionJson opt)
            {
                if (!string.IsNullOrEmpty(opt.id)) ids.Add(opt.id);
                foreach (var r in opt.reactions) GatherIds(r, ids);
            }
            else if (root is DialogueReactionJson react)
            {
                if (!string.IsNullOrEmpty(react.id)) ids.Add(react.id);
                foreach (var o in react.options) GatherIds(o, ids);
            }
        }

        private void addFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!IsModFolderMode()) return;

            using (Form prompt = new Form())
            {
                prompt.Width = 350;
                prompt.Height = 150;
                prompt.Text = LocalizationManager.Get("MenuAddFile") ?? "Add New File";
                prompt.StartPosition = FormStartPosition.CenterParent;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.MaximizeBox = false;

                Label textLabel = new Label() { Left = 20, Top = 20, Text = "Filename:", Width = 80 };
                TextBox textBox = new TextBox() { Left = 100, Top = 20, Width = 200 };
                Button confirmation = new Button() { Text = "Add", Left = 220, Width = 80, Top = 60, DialogResult = DialogResult.OK };
                
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(textLabel);
                prompt.AcceptButton = confirmation;

                if (prompt.ShowDialog() == DialogResult.OK)
                {
                    string name = textBox.Text.Trim();
                    if (string.IsNullOrEmpty(name)) return;
                    if (!name.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) name += ".json";

                    string fullPath = Path.Combine(currentFilePath, name);
                    if (File.Exists(fullPath))
                    {
                        MessageBox.Show("File already exists!");
                        return;
                    }

                    // Create physical file
                    var newRoot = new DialogueFileRoot();
                    var options = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
                    File.WriteAllText(fullPath, JsonSerializer.Serialize(newRoot, options));

                    // Add to current root to show in UI
                    // We don't necessarily need to add a dummy NPC, but the SourceFilePath needs to exist for the tree
                    // A simple way is to refresh the whole mod folder or just add a placeholder
                    LoadModFolder(currentFilePath); 
                }
            }
        }

        private void addOptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = treeView1.SelectedNode;
            if (node == null) return;

            if (node.Tag is DialogueFileRoot root)
            {
                using (var selector = new NpcSelectorForm(DialogueModels.NPC_ARCHETYPES))
                {
                    if (selector.ShowDialog() == DialogResult.OK)
                    {
                        var npc = new NpcDialogueJson { npcArchetype = selector.SelectedArchetype };
                        root.dialogues.Add(npc);
                        var npcNode = new TreeNode(npc.ToString()) { Tag = npc };
                        node.Nodes.Add(npcNode);
                        node.Expand();
                    }
                }
            }
            else if (node.Tag is string fileTag && !string.IsNullOrEmpty(fileTag))
            {
                using (var selector = new NpcSelectorForm(DialogueModels.NPC_ARCHETYPES))
                {
                    if (selector.ShowDialog() == DialogResult.OK)
                    {
                        var npc = new NpcDialogueJson { npcArchetype = selector.SelectedArchetype, SourceFilePath = fileTag };
                        currentRoot.dialogues.Add(npc);
                        var npcNode = new TreeNode(npc.ToString()) { Tag = npc };
                        node.Nodes.Add(npcNode);
                        node.Expand();
                    }
                }
            }
            else if (node.Tag is NpcDialogueJson npc)
            {
                var opt = new DialogueOptionJson { id = GenerateUniqueID(node, "OPT") };
                npc.entryOptions.Add(opt);
                AddOptionNode(node, opt);
                node.Expand();
            }
            else if (node.Tag is DialogueReactionJson react)
            {
                var opt = new DialogueOptionJson { id = GenerateUniqueID(node, "OPT") };
                react.options.Add(opt);
                AddOptionNode(node, opt);
                node.Expand();
            }
            // Handle Item Categories
            else if (currentItems != null || currentReputations != null)
            {
                string nodeText = node.Text;
                
                if (nodeText == LocalizationManager.Get("NodeConsumables") && currentItems != null)
                    AddItem(node, new ConsumableJsonItem { changes = new ChangeEntry[8] });                
                else if (nodeText == LocalizationManager.Get("NodeScraps") && currentItems != null)
                    AddItem(node, new ScrapJsonItem());
                else if (nodeText == LocalizationManager.Get("NodeBags") && currentItems != null)
                    AddItem(node, new BagJsonItem());
                else if (nodeText == LocalizationManager.Get("NodeGears") && currentItems != null)
                    AddItem(node, new GearJsonItem());
                else if (nodeText == LocalizationManager.Get("NodeRecipes") && currentItems != null)
                    AddItem(node, new RecipeJson());
                else if (nodeText == LocalizationManager.Get("NodeShops") && currentItems != null)
                    AddItem(node, new ShopJsonItem());
                else if (nodeText == LocalizationManager.Get("NodeSells") && currentItems != null)
                    AddItem(node, new ShopJsonItem()); 
                else if (nodeText == LocalizationManager.Get("NodePackages") && currentItems != null)
                    AddItem(node, new PackageTableJsonItem());
                else if (nodeText == LocalizationManager.Get("NodeSalvage") && currentItems != null)
                    AddItem(node, new SalvagePatternJson());
                else if (nodeText == LocalizationManager.Get("NodeArchetypes") && currentItems != null)
                    AddItem(node, new ArchetypeJson());
                else if (nodeText == LocalizationManager.Get("NodeReputations") && currentReputations != null)
                    AddItem(node, new ReputationJson());
            }

            // Helper for ID and adding
            void AddItem<T>(TreeNode parentNode, T newItem)
            {
                // Logic for Global ID Pool (Consumables, Scraps, Bags, Gears)
                var globalIds = GetAllGlobalIds();

                if (newItem is ConsumableJsonItem c)
                {
                    c.id = GetNextId(globalIds);
                    c.customTitle = "New Consumable";
                    c.changes = new ChangeEntry[8];
                    for(int i=0; i<8; i++) c.changes[i] = new ChangeEntry();
                    currentItems.consumables.Add(c);
                }
                else if (newItem is ScrapJsonItem s)
                {
                    s.id = GetNextId(globalIds);
                    s.customTitle = "New Scrap";
                    currentItems.scraps.Add(s);
                }
                else if (newItem is BagJsonItem b)
                {
                    b.id = GetNextId(globalIds);
                    b.customTitle = "New Bag";
                    currentItems.bags.Add(b);
                }
                else if (newItem is GearJsonItem g)
                {
                    g.id = GetNextId(globalIds);
                    g.customTitle = "New Gear";
                    g.parameterChanges = new ParameterChangeJson[3];
                    for (int i = 0; i < 3; i++) g.parameterChanges[i] = new ParameterChangeJson();
                    currentItems.gears.Add(g);
                }
                else if (newItem is RecipeJson r)
                {
                    r.id = GetNextId(currentItems.recipes.Select(x => x.id));
                    currentItems.recipes.Add(r);
                }
                else if (newItem is ShopJsonItem sh)
                {
                    sh.title = "New Shop";
                    if (parentNode.Text == LocalizationManager.Get("NodeShops")) currentItems.shops.Add(sh);
                    else currentItems.sells.Add(sh);
                }
                else if (newItem is PackageTableJsonItem p)
                {
                    p.title = "New Package";
                    currentItems.packageTables.Add(p);
                }
                else if (newItem is SalvagePatternJson sp)
                {
                    sp.id = GetNextId(currentItems.salvagePatterns.Select(x => x.id));
                    currentItems.salvagePatterns.Add(sp);
                }
                else if (newItem is ArchetypeJson a)
                {
                    a.key = "New_Archetype";
                    currentItems.archetypes.Add(a);
                }
                else if (newItem is ReputationJson rep)
                {
                    rep.archetype = "Hobo_Majsner";
                    currentReputations.reputations.Add(rep);
                }

                var newNode = new TreeNode(newItem.ToString()) { Tag = newItem };
                parentNode.Nodes.Add(newNode);
                parentNode.Expand();
                treeView1.SelectedNode = newNode;
            }
            
            int GetNextId(IEnumerable<int> existing)
            {
                if (!existing.Any()) return 10000;
                return existing.Max() + 1;
            }
        }

        private List<int> GetAllGlobalIds()
        {
            var ids = new List<int>();
            if (currentItems == null) return ids;

            ids.AddRange(currentItems.consumables.Select(x => x.id));
            ids.AddRange(currentItems.scraps.Select(x => x.id));
            ids.AddRange(currentItems.bags.Select(x => x.id));
            ids.AddRange(currentItems.gears.Select(x => x.id));
            return ids;
        }

        private void addReactionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = treeView1.SelectedNode;
            var react = new DialogueReactionJson { id = GenerateUniqueID(node, "REACT") };
            if (node.Tag is DialogueOptionJson opt)
                opt.reactions.Add(react);

            AddReactionNode(node, react);
            node.Expand();
        }

        private void addActionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var act = new DialogueActionJson();
            var node = treeView1.SelectedNode;

            // Handle clicking on container node "执行动作"
            if (node.Text == LocalizationManager.Get("NodeActions") && node.Parent != null)
            {
                var parentTag = node.Parent.Tag;
                if (parentTag is DialogueOptionJson opt) { opt.actions.Add(act); RefreshOptionNode(node.Parent); }
                else if (parentTag is DialogueReactionJson react) { react.actions.Add(act); RefreshReactionNode(node.Parent); }
            }
            else if (node.Tag is DialogueOptionJson opt)
            {
                opt.actions.Add(act);
                RefreshOptionNode(node);
            }
            else if (node.Tag is DialogueReactionJson react)
            {
                react.actions.Add(act);
                RefreshReactionNode(node);
            }
            node.Expand();
        }

        private void addConditionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var cond = new DialogueConditionJson();
            var node = treeView1.SelectedNode;

            // Handle clicking on container node "触发条件"
            if (node.Text == LocalizationManager.Get("NodeConditions") && node.Parent != null)
            {
                var parentTag = node.Parent.Tag;
                if (parentTag is DialogueOptionJson opt) { opt.conditions.Add(cond); RefreshOptionNode(node.Parent); }
                else if (parentTag is DialogueReactionJson react) { react.conditions.Add(cond); RefreshReactionNode(node.Parent); }
            }
            else if (node.Tag is DialogueOptionJson opt)
            {
                opt.conditions.Add(cond);
                RefreshOptionNode(node);
            }
            else if (node.Tag is DialogueReactionJson react)
            {
                react.conditions.Add(cond);
                RefreshReactionNode(node);
            }
            node.Expand();
        }

        private void addL10nToolStripMenuItem_Click(object sender, EventArgs e)
        {
             var node = treeView1.SelectedNode;
             object targetObj = node.Tag;
             Dictionary<string, string> l10nDict = null;
             
             if (node.Text == LocalizationManager.Get("NodeL10n") || node.Tag == null)
             {
                 targetObj = node.Parent.Tag;
             }

             if (targetObj is DialogueOptionJson opt) l10nDict = opt.l10n;
             else if (targetObj is DialogueReactionJson react) l10nDict = react.l10n;

             if (l10nDict != null)
             {
                 // Create a list of 9 languages
                 string[] allLangs = { "en", "cs", "es", "ja", "fr", "zh", "ru", "pl", "de" };
                 var availableLangs = allLangs.Where(l => !l10nDict.ContainsKey(l)).ToArray();
                 if (availableLangs.Length == 0) return;

                 using (var selector = new NpcSelectorForm(availableLangs))
                 {
                     if (selector.ShowDialog() == DialogResult.OK)
                     {
                         l10nDict[selector.SelectedArchetype] = "New Text";
                         if (targetObj is DialogueOptionJson) RefreshOptionNode(node.Tag == null ? node.Parent : node);
                         else RefreshReactionNode(node.Tag == null ? node.Parent : node);
                     }
                 }
             }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = treeView1.SelectedNode;
            if (node == null || node.Parent == null) return;

            var parentNode = node.Parent;
            object tag = node.Tag;

            // Handle virtual container deletion
            bool isActionsNode = node.Text == LocalizationManager.Get("NodeActions");
            bool isConditionsNode = node.Text == LocalizationManager.Get("NodeConditions");
            bool isL10nNode = node.Text == LocalizationManager.Get("NodeL10n");

            if (isActionsNode || isConditionsNode || isL10nNode)
            {
                object pTag = parentNode.Tag;
                if (isActionsNode)
                {
                    if (pTag is DialogueOptionJson pOpt) pOpt.actions.Clear();
                    else if (pTag is DialogueReactionJson pReact) pReact.actions.Clear();
                }
                else if (isConditionsNode)
                {
                    if (pTag is DialogueOptionJson pOpt) pOpt.conditions.Clear();
                }
                else if (isL10nNode)
                {
                    if (pTag is DialogueOptionJson pOpt) pOpt.l10n.Clear();
                    else if (pTag is DialogueReactionJson pReact) pReact.l10n.Clear();
                }

                // Refresh parent to remove the container
                if (pTag is DialogueOptionJson) RefreshOptionNode(parentNode);
                else if (pTag is DialogueReactionJson) RefreshReactionNode(parentNode);
                return;
            }

            // If tag is an item inside a folder, parentTag is grandparent
            object parentTag = parentNode.Tag;
            if (parentTag == null && parentNode.Parent != null)
            {
                parentTag = parentNode.Parent.Tag;
            }
            
            if (tag is L10nItem lItem)
            {
                 if (lItem.Parent is DialogueOptionJson o) o.l10n.Remove(lItem.Language);
                 else if (lItem.Parent is DialogueReactionJson r) r.l10n.Remove(lItem.Language);
                 
                 var realParent = parentNode.Parent;
                 if (realParent.Tag is DialogueOptionJson) RefreshOptionNode(realParent);
                 else RefreshReactionNode(realParent);
                 return;
            }

            if (tag is DialogueOptionJson opt)
            {
                if (parentTag is NpcDialogueJson npc) npc.entryOptions.Remove(opt);
                else if (parentTag is DialogueReactionJson react) react.options.Remove(opt);
            }
            else if (tag is DialogueReactionJson react)
            {
                if (parentTag is DialogueOptionJson parentOpt) parentOpt.reactions.Remove(react);
            }
            else if (tag is DialogueActionJson act)
            {
                if (parentTag is DialogueOptionJson parentOpt) parentOpt.actions.Remove(act);
                else if (parentTag is DialogueReactionJson parentReact) parentReact.actions.Remove(act);
            }
            else if (tag is DialogueConditionJson cond)
            {
                if (parentTag is DialogueOptionJson parentOpt) parentOpt.conditions.Remove(cond);
                else if (parentTag is DialogueReactionJson parentReact) parentReact.conditions.Remove(cond);
            }

            // After deleting an item, refresh the container's real parent to handle auto-disappear of empty folder
            if (parentNode.Tag == null) // We were inside a virtual folder
            {
                var realParent = parentNode.Parent;
                if (realParent != null)
                {
                    if (realParent.Tag is DialogueOptionJson) RefreshOptionNode(realParent);
                    else if (realParent.Tag is DialogueReactionJson) RefreshReactionNode(realParent);
                }
            }
            else
            {
                node.Remove();
            }
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = treeView1.SelectedNode;
            if (node == null || node.Parent == null) return;
            
            copyToolStripMenuItem_Click(sender, e);
            deleteToolStripMenuItem_Click(sender, e);
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = treeView1.SelectedNode;
            if (node?.Tag == null) return;
            
            // For now, we use internal memory for high-fidelity cloning (including JsonIgnored l10n)
            clipboardData = node.Tag;
            toolStripStatusLabel1.Text = "Copied item to internal clipboard.";
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = treeView1.SelectedNode;
            if (node == null || clipboardData == null) return;

            // Clone data for multiple pastes
            object? clone = null;
            string json = JsonSerializer.Serialize(clipboardData);
            
            if (clipboardData is DialogueActionJson) 
                clone = JsonSerializer.Deserialize<DialogueActionJson>(json);
            else if (clipboardData is DialogueConditionJson) 
                clone = JsonSerializer.Deserialize<DialogueConditionJson>(json);
            else if (clipboardData is DialogueOptionJson opt)
            {
                var c = JsonSerializer.Deserialize<DialogueOptionJson>(json);
                if (c != null)
                {
                    var existingIds = new HashSet<string>();
                    GatherIds(currentRoot, existingIds);
                    CopyL10nRecursive(opt, c);
                    RegenerateIdsRecursive(c, node, existingIds);
                    clone = c;
                }
            }
            else if (clipboardData is DialogueReactionJson react)
            {
                var r = JsonSerializer.Deserialize<DialogueReactionJson>(json);
                if (r != null)
                {
                    var existingIds = new HashSet<string>();
                    GatherIds(currentRoot, existingIds);
                    CopyL10nRecursive(react, r);
                    RegenerateIdsRecursive(r, node, existingIds);
                    clone = r;
                }
            }

            if (clone == null) return;

            // Target resolution
            bool isActionsNode = node.Text == LocalizationManager.Get("NodeActions");
            bool isConditionsNode = node.Text == LocalizationManager.Get("NodeConditions");

            if (clone is DialogueActionJson act)
            {
                if (isActionsNode)
                {
                    if (node.Parent?.Tag is DialogueOptionJson opt) opt.actions.Add(act);
                    else if (node.Parent?.Tag is DialogueReactionJson react) react.actions.Add(act);
                    if (node.Parent != null) { if (node.Parent.Tag is DialogueOptionJson) RefreshOptionNode(node.Parent); else RefreshReactionNode(node.Parent); }
                }
                else if (node.Tag is DialogueOptionJson opt) { opt.actions.Add(act); RefreshOptionNode(node); }
                else if (node.Tag is DialogueReactionJson react) { react.actions.Add(act); RefreshReactionNode(node); }
            }
            else if (clone is DialogueConditionJson cond)
            {
                if (isConditionsNode)
                {
                    if (node.Parent?.Tag is DialogueOptionJson opt) opt.conditions.Add(cond);
                    if (node.Parent != null) RefreshOptionNode(node.Parent);
                }
                else if (node.Tag is DialogueOptionJson opt) { opt.conditions.Add(cond); RefreshOptionNode(node); }
            }
            else if (clone is DialogueOptionJson newOpt)
            {
                if (node.Tag is DialogueReactionJson r) { r.options.Add(newOpt); RefreshReactionNode(node); }
                else if (node.Tag is NpcDialogueJson n) { n.entryOptions.Add(newOpt); AddOptionNode(node, newOpt); node.Expand(); }
            }
            else if (clone is DialogueReactionJson newReact)
            {
                if (node.Tag is DialogueOptionJson o) { o.reactions.Add(newReact); RefreshOptionNode(node); }
            }
        }

        private void CopyL10nRecursive(object source, object target)
        {
            if (source is DialogueOptionJson sOpt && target is DialogueOptionJson tOpt)
            {
                tOpt.l10n = new Dictionary<string, string>(sOpt.l10n);
                for (int i = 0; i < Math.Min(sOpt.reactions.Count, tOpt.reactions.Count); i++)
                    CopyL10nRecursive(sOpt.reactions[i], tOpt.reactions[i]);
            }
            else if (source is DialogueReactionJson sReact && target is DialogueReactionJson tReact)
            {
                tReact.l10n = new Dictionary<string, string>(sReact.l10n);
                for (int i = 0; i < Math.Min(sReact.options.Count, tReact.options.Count); i++)
                    CopyL10nRecursive(sReact.options[i], tReact.options[i]);
            }
        }

        private void RegenerateIdsRecursive(object obj, TreeNode contextNode, HashSet<string> existingIds)
        {
            if (obj is DialogueOptionJson opt)
            {
                opt.id = GenerateUniqueID(contextNode, "OPT", existingIds);
                foreach (var r in opt.reactions) RegenerateIdsRecursive(r, contextNode, existingIds);
            }
            else if (obj is DialogueReactionJson react)
            {
                react.id = GenerateUniqueID(contextNode, "REACT", existingIds);
                foreach (var o in react.options) RegenerateIdsRecursive(o, contextNode, existingIds);
            }
        }

        private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Item is TreeNode node && node.Tag != null && node.Parent != null)
                DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void treeView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            Point targetPoint = treeView1.PointToClient(new Point(e.X, e.Y));
            TreeNode targetNode = treeView1.GetNodeAt(targetPoint);
            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            if (draggedNode == null || targetNode == null || draggedNode == targetNode) return;

            object draggedTag = draggedNode.Tag;
            object targetTag = targetNode.Tag;
            bool isTargetActions = targetNode.Text == LocalizationManager.Get("NodeActions");
            bool isTargetConditions = targetNode.Text == LocalizationManager.Get("NodeConditions");

            // Define Rules
            bool allowed = false;

            // 1. Condition & Action Rules: Only to same-type folders or parents
            if (draggedTag is DialogueActionJson act)
            {
                if (isTargetActions) { MoveAction(draggedNode, targetNode.Parent); allowed = true; }
                else if (targetTag is DialogueOptionJson || targetTag is DialogueReactionJson) { MoveAction(draggedNode, targetNode); allowed = true; }
            }
            else if (draggedTag is DialogueConditionJson cond)
            {
                if (isTargetConditions) { MoveCondition(draggedNode, targetNode.Parent); allowed = true; }
                else if (targetTag is DialogueOptionJson || targetTag is DialogueReactionJson) { MoveCondition(draggedNode, targetNode); allowed = true; }
            }
            // 2. Option Rules: To Reaction or Root (NpcDialogue)
            else if (draggedTag is DialogueOptionJson opt)
            {
                if (targetTag is DialogueReactionJson || targetTag is NpcDialogueJson) { MoveOption(draggedNode, targetNode); allowed = true; }
            }
            // 3. Reaction Rules: Only to Option
            else if (draggedTag is DialogueReactionJson react)
            {
                if (targetTag is DialogueOptionJson) { MoveReaction(draggedNode, targetNode); allowed = true; }
            }

            if (allowed) UpdateTree(); // Full refresh ensures consistency
        }

        private void MoveAction(TreeNode itemNode, TreeNode targetParentNode)
        {
            // Remove from old
            var act = (DialogueActionJson)itemNode.Tag;
            var oldParent = itemNode.Parent.Tag == null ? itemNode.Parent.Parent.Tag : itemNode.Parent.Tag;
            if (oldParent is DialogueOptionJson o) o.actions.Remove(act);
            else if (oldParent is DialogueReactionJson r) r.actions.Remove(act);

            // Add to new
            if (targetParentNode.Tag is DialogueOptionJson to) to.actions.Add(act);
            else if (targetParentNode.Tag is DialogueReactionJson tr) tr.actions.Add(act);
        }

        private void MoveCondition(TreeNode itemNode, TreeNode targetParentNode)
        {
            var cond = (DialogueConditionJson)itemNode.Tag;
            var oldParent = itemNode.Parent.Tag == null ? itemNode.Parent.Parent.Tag : itemNode.Parent.Tag;
            if (oldParent is DialogueOptionJson o) o.conditions.Remove(cond);
            else if (oldParent is DialogueReactionJson r) r.conditions.Remove(cond);

            if (targetParentNode.Tag is DialogueOptionJson to) to.conditions.Add(cond);
            else if (targetParentNode.Tag is DialogueReactionJson tr) tr.conditions.Add(cond);
        }

        private void MoveOption(TreeNode itemNode, TreeNode targetParentNode)
        {
            var opt = (DialogueOptionJson)itemNode.Tag;
            var oldParent = itemNode.Parent.Tag; // For Option, parent is always Npc or Reaction
            if (oldParent is NpcDialogueJson n) n.entryOptions.Remove(opt);
            else if (oldParent is DialogueReactionJson r) r.options.Remove(opt);

            if (targetParentNode.Tag is NpcDialogueJson tn) tn.entryOptions.Add(opt);
            else if (targetParentNode.Tag is DialogueReactionJson tr) tr.options.Add(opt);
        }

        private void MoveReaction(TreeNode itemNode, TreeNode targetParentNode)
        {
            var react = (DialogueReactionJson)itemNode.Tag;
            var oldParent = (DialogueOptionJson)itemNode.Parent.Tag;
            oldParent.reactions.Remove(react);

            var to = (DialogueOptionJson)targetParentNode.Tag;
            to.reactions.Add(react);
        }

        private void chineseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LocalizationManager.SetLanguage("zh-CN");
            ApplyLocalization();
        }

        private void englishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LocalizationManager.SetLanguage("en-US");
            ApplyLocalization();
        }

        private void aiSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var settings = LocalizationManager.Settings;
            using (Form f = new Form())
            {
                f.Text = LocalizationManager.Get("AiSettingsTitle");
                f.Size = new Size(550, 400); // Increased size
                f.StartPosition = FormStartPosition.CenterParent;
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.MaximizeBox = false;

                Label lblBase = new Label { Text = LocalizationManager.Get("AiApiBase"), Left = 20, Top = 20, Width = 150 };
                TextBox txtBase = new TextBox { Text = settings.AiApiBase, Left = 180, Top = 20, Width = 320 };

                Label lblKey = new Label { Text = LocalizationManager.Get("AiApiKey"), Left = 20, Top = 55, Width = 150 };
                TextBox txtKey = new TextBox { Text = settings.AiApiKey, Left = 180, Top = 55, Width = 320, PasswordChar = '*' };

                Label lblModel = new Label { Text = LocalizationManager.Get("AiModel"), Left = 20, Top = 90, Width = 150 };
                TextBox txtModel = new TextBox { Text = settings.AiModel, Left = 180, Top = 90, Width = 320 };

                Label lblLangs = new Label { Text = LocalizationManager.Get("AiSelectLangs"), Left = 20, Top = 125, Width = 150 };
                FlowLayoutPanel flowLangs = new FlowLayoutPanel { Left = 180, Top = 125, Width = 320, Height = 150, AutoScroll = true };
                
                var checkBoxes = new List<CheckBox>();
                foreach (var lang in DialogueModels.LANGUAGES)
                {
                    var cb = new CheckBox { Text = lang, Width = 70, Checked = settings.AiTargetLanguages.Contains(lang) };
                    checkBoxes.Add(cb);
                    flowLangs.Controls.Add(cb);
                }

                Button btnOk = new Button { Text = "OK", Left = 300, Top = 300, Width = 80, DialogResult = DialogResult.OK };
                Button btnCancel = new Button { Text = "Cancel", Left = 400, Top = 300, Width = 80, DialogResult = DialogResult.Cancel };

                f.Controls.AddRange(new Control[] { lblBase, txtBase, lblKey, txtKey, lblModel, txtModel, lblLangs, flowLangs, btnOk, btnCancel });
                f.AcceptButton = btnOk;

                if (f.ShowDialog() == DialogResult.OK)
                {
                    settings.AiApiBase = txtBase.Text;
                    settings.AiApiKey = txtKey.Text;
                    settings.AiModel = txtModel.Text;
                    settings.AiTargetLanguages = checkBoxes.Where(c => c.Checked).Select(c => c.Text).ToList();
                    LocalizationManager.SaveSettings();
                }
            }
        }

        private async void aiL10nToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentRoot == null) return;
            
            try
            {
                Log(LocalizationManager.Get("Log_AiStart"));
                toolStripStatusLabel1.Text = LocalizationManager.Get("AiTranslating");
                this.Cursor = Cursors.WaitCursor;

                // 1. Gather ONLY nodes that are missing at least one of the target languages
                var targetLangs = LocalizationManager.Settings.AiTargetLanguages;
                if (targetLangs == null || !targetLangs.Any()) targetLangs = new List<string> { "zh", "en" };

                var nodesToTranslate = new List<dynamic>();
                GatherMissingTextNodes(currentRoot, targetLangs, nodesToTranslate);

                if (nodesToTranslate.Count == 0)
                {
                    Log(LocalizationManager.Get("Log_AiNoNewNodes"));
                    MessageBox.Show(LocalizationManager.Get("Log_AiNoNewNodes"));
                    return;
                }

                Log(string.Format(LocalizationManager.Get("Log_AiGathered"), nodesToTranslate.Count, string.Join(", ", targetLangs)));

                // --- SPEED OPTIMIZATION: BATCHING & PARALLELISM ---
                int batchSize = 10;
                int maxParallelTasks = 3; // Keep it modest to avoid Rate Limits (429)
                var batches = new List<List<dynamic>>();
                for (int i = 0; i < nodesToTranslate.Count; i += batchSize)
                {
                    batches.Add(nodesToTranslate.GetRange(i, Math.Min(batchSize, nodesToTranslate.Count - i)));
                }

                Log(string.Format(LocalizationManager.Get("Log_AiBatches"), batches.Count, maxParallelTasks));

                var semaphore = new System.Threading.SemaphoreSlim(maxParallelTasks);
                var tasks = batches.Select(async (batch, index) => {
                    await semaphore.WaitAsync();
                    try {
                        Log(string.Format(LocalizationManager.Get("Log_AiBatchSend"), index + 1, batches.Count));
                        var jsonBatch = JsonSerializer.Serialize(batch);
                        var result = await DeepSeekService.TranslateAsync($"Context: {jsonBatch}");
                        Log(string.Format(LocalizationManager.Get("Log_AiBatchReceive"), index + 1, batches.Count));
                        return result;
                    } catch (Exception ex) {
                        Log(string.Format(LocalizationManager.Get("Log_AiBatchFail"), index + 1, ex.Message), true);
                        return null;
                    } finally {
                        semaphore.Release();
                    }
                });

                var batchResults = await Task.WhenAll(tasks);
                
                // --- APPLY & CHECK FOR MISSING LANGUAGES ---
                Log(LocalizationManager.Get("Log_AiApply"));
                
                foreach (var res in batchResults)
                {
                    if (string.IsNullOrEmpty(res)) continue;
                    try {
                        using var doc = JsonDocument.Parse(res);
                        ApplyTranslationsRecursive(currentRoot, doc.RootElement);
                    } catch { /* Handled in retry logic later */ }
                }

                // --- RETRY LOGIC: Verify completeness ---
                var missingTranslations = new List<dynamic>();
                CheckCompletenessRecursive(currentRoot, targetLangs, missingTranslations);

                if (missingTranslations.Any())
                {
                    Log(string.Format(LocalizationManager.Get("Log_AiMissingRetry"), missingTranslations.Count), true);
                    // Single batch for retry usually sufficient if not too many
                    var retryJson = JsonSerializer.Serialize(missingTranslations);
                    var retryResponse = await DeepSeekService.TranslateAsync($"COMPLETION_MODE: Translate the following missing nodes into the specified languages ONLY. Return original JSON structure. Context: {retryJson}");
                    
                    if (!string.IsNullOrEmpty(retryResponse))
                    {
                        try {
                            using var retryDoc = JsonDocument.Parse(retryResponse);
                            ApplyTranslationsRecursive(currentRoot, retryDoc.RootElement);
                            Log(LocalizationManager.Get("Log_AiRetrySuccess"));
                        } catch (Exception ex) {
                            Log(string.Format(LocalizationManager.Get("Log_AiRetryFail"), ex.Message), true);
                            Log(retryResponse);
                        }
                    }
                }

                UpdateTree();
                Log(LocalizationManager.Get("Log_AiFinish"));
                MessageBox.Show(LocalizationManager.Get("AiDone"));
            }
            catch (Exception ex)
            {
                Log($"AI Error: {ex.Message}", true);
                MessageBox.Show(ex.Message, LocalizationManager.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                toolStripStatusLabel1.Text = LocalizationManager.Get("Ready");
            }
        }

        private void CheckCompletenessRecursive(object root, List<string> targetLangs, List<dynamic> missingList)
        {
            if (root is DialogueFileRoot fileRoot)
                foreach (var npc in fileRoot.dialogues) CheckCompletenessRecursive(npc, targetLangs, missingList);
            else if (root is NpcDialogueJson npc)
                foreach (var opt in npc.entryOptions) CheckCompletenessRecursive(opt, targetLangs, missingList);
            else if (root is DialogueOptionJson opt)
            {
                var missing = targetLangs.Where(l => !opt.l10n.ContainsKey(l) || string.IsNullOrEmpty(opt.l10n[l])).ToList();
                if (missing.Any() && !string.IsNullOrEmpty(opt.text))
                    missingList.Add(new { node_id = opt.id, text = opt.text, type = "Option", missing_languages = missing });
                foreach (var r in opt.reactions) CheckCompletenessRecursive(r, targetLangs, missingList);
            }
            else if (root is DialogueReactionJson react)
            {
                var missing = targetLangs.Where(l => !react.l10n.ContainsKey(l) || string.IsNullOrEmpty(react.l10n[l])).ToList();
                if (missing.Any() && !string.IsNullOrEmpty(react.text))
                    missingList.Add(new { node_id = react.id, text = react.text, type = "Reaction", missing_languages = missing });
                foreach (var o in react.options) CheckCompletenessRecursive(o, targetLangs, missingList);
            }
        }

        private void GatherMissingTextNodes(object root, List<string> targetLangs, List<dynamic> list)
        {
            if (root is DialogueFileRoot fileRoot)
                foreach (var npc in fileRoot.dialogues) GatherMissingTextNodes(npc, targetLangs, list);
            else if (root is NpcDialogueJson npc)
                foreach (var opt in npc.entryOptions) GatherMissingTextNodes(opt, targetLangs, list);
            else if (root is DialogueOptionJson opt)
            {
                var missing = targetLangs.Where(l => !opt.l10n.ContainsKey(l) || string.IsNullOrEmpty(opt.l10n[l])).ToList();
                if (missing.Any() && !string.IsNullOrEmpty(opt.text)) 
                    list.Add(new { node_id = opt.id, text = opt.text, type = "Option", target_languages = missing });
                foreach (var r in opt.reactions) GatherMissingTextNodes(r, targetLangs, list);
            }
            else if (root is DialogueReactionJson react)
            {
                var missing = targetLangs.Where(l => !react.l10n.ContainsKey(l) || string.IsNullOrEmpty(react.l10n[l])).ToList();
                if (missing.Any() && !string.IsNullOrEmpty(react.text)) 
                    list.Add(new { node_id = react.id, text = react.text, type = "Reaction", target_languages = missing });
                foreach (var o in react.options) GatherMissingTextNodes(o, targetLangs, list);
            }
        }

        private void GatherTextNodes(object root, List<dynamic> list)
        {
            if (root is DialogueFileRoot fileRoot)
                foreach (var npc in fileRoot.dialogues) GatherTextNodes(npc, list);
            else if (root is NpcDialogueJson npc)
                foreach (var opt in npc.entryOptions) GatherTextNodes(opt, list);
            else if (root is DialogueOptionJson opt)
            {
                if (!string.IsNullOrEmpty(opt.text)) list.Add(new { node_id = opt.id, text = opt.text, type = "Option" });
                foreach (var r in opt.reactions) GatherTextNodes(r, list);
            }
            else if (root is DialogueReactionJson react)
            {
                if (!string.IsNullOrEmpty(react.text)) list.Add(new { node_id = react.id, text = react.text, type = "Reaction" });
                foreach (var o in react.options) GatherTextNodes(o, list);
            }
        }

        private void ApplyTranslationsRecursive(object root, JsonElement translations)
        {
            if (root is DialogueFileRoot fileRoot)
                foreach (var npc in fileRoot.dialogues) ApplyTranslationsRecursive(npc, translations);
            else if (root is NpcDialogueJson npc)
                foreach (var opt in npc.entryOptions) ApplyTranslationsRecursive(opt, translations);
            else if (root is DialogueOptionJson opt)
            {
                if (translations.TryGetProperty(opt.id, out var nodeL10n))
                {
                    var selectedLangs = LocalizationManager.Settings.AiTargetLanguages;
                    foreach (var prop in nodeL10n.EnumerateObject())
                    {
                        if (selectedLangs.Contains(prop.Name))
                        {
                            opt.l10n[prop.Name] = prop.Value.GetString();
                        }
                    }
                }
                foreach (var r in opt.reactions) ApplyTranslationsRecursive(r, translations);
            }
            else if (root is DialogueReactionJson react)
            {
                if (translations.TryGetProperty(react.id, out var nodeL10n))
                {
                    var selectedLangs = LocalizationManager.Settings.AiTargetLanguages;
                    foreach (var prop in nodeL10n.EnumerateObject())
                    {
                        if (selectedLangs.Contains(prop.Name))
                        {
                            react.l10n[prop.Name] = prop.Value.GetString();
                        }
                    }
                }
                foreach (var o in react.options) ApplyTranslationsRecursive(o, translations);
            }
        }

        private void syncL10nToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentRoot == null) return;
            string targetLang = LocalizationManager.CurrentLanguage.StartsWith("zh") ? "zh" : "en";
            
            int count = 0;
            SyncTextRecursive(currentRoot, targetLang, ref count);
            
            Log($"Synced {count} text items to '{targetLang}' localization cache.");
            UpdateTree();
        }

        private void SyncTextRecursive(object root, string lang, ref int count)
        {
            if (root is DialogueFileRoot fileRoot)
                foreach (var npc in fileRoot.dialogues) SyncTextRecursive(npc, lang, ref count);
            else if (root is NpcDialogueJson npc)
                foreach (var opt in npc.entryOptions) SyncTextRecursive(opt, lang, ref count);
            else if (root is DialogueOptionJson opt)
            {
                if (!string.IsNullOrEmpty(opt.text)) { opt.l10n[lang] = opt.text; count++; }
                foreach (var r in opt.reactions) SyncTextRecursive(r, lang, ref count);
            }
            else if (root is DialogueReactionJson react)
            {
                if (!string.IsNullOrEmpty(react.text)) { react.l10n[lang] = react.text; count++; }
                foreach (var o in react.options) SyncTextRecursive(o, lang, ref count);
            }
        }

        private void previewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentRoot == null) return;
            
            object startNode = treeView1.SelectedNode?.Tag;
            if (startNode == null || startNode is DialogueFileRoot) startNode = currentRoot.dialogues.FirstOrDefault();
            
            if (startNode == null) return;

            string npcName = "NPC";
            // Try to find the NPC name if we are inside an NPC dialogue
            TreeNode p = treeView1.SelectedNode;
            while (p != null)
            {
                if (p.Tag is NpcDialogueJson n) { npcName = n.npcArchetype; break; }
                p = p.Parent;
            }

            using (var pf = new PreviewForm(startNode, npcName))
            {
                pf.ShowDialog();
            }
        }
    }
}
