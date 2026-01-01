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
        private object clipboardData = null;

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
            LocalizedTypeDescriptorProvider.Register(typeof(L10nItem));

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

            // Initial default for startup
            currentRoot = new DialogueFileRoot();
            currentRoot.dialogues.Add(new NpcDialogueJson { npcArchetype = "Hobo_Majsner" });
            
            treeView1.AllowDrop = true;
            treeView1.ItemDrag += treeView1_ItemDrag;
            treeView1.DragEnter += treeView1_DragEnter;
            treeView1.DragDrop += treeView1_DragDrop;

            UpdateTree();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.SuppressKeyPress = true;
                saveToolStripMenuItem_Click(sender, e);
            }
            else if (e.Control && e.KeyCode == Keys.C)
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

        private void ApplyLocalization()
        {
            // Menu
            fileToolStripMenuItem.Text = LocalizationManager.Get("MenuFile");
            newToolStripMenuItem.Text = LocalizationManager.Get("MenuNew");
            openToolStripMenuItem.Text = LocalizationManager.Get("MenuOpen");
            saveToolStripMenuItem.Text = LocalizationManager.Get("MenuSave");
            saveAsToolStripMenuItem.Text = LocalizationManager.Get("MenuSaveAs");
            languageToolStripMenuItem.Text = LocalizationManager.Get("MenuLanguage");
            toolsToolStripMenuItem.Text = LocalizationManager.Get("MenuTools");
            previewToolStripMenuItem.Text = LocalizationManager.Get("MenuPreview");
            aiL10nToolStripMenuItem.Text = LocalizationManager.Get("MenuAiL10n");
            syncL10nToolStripMenuItem.Text = LocalizationManager.Get("MenuSyncL10n");
            aiSettingsToolStripMenuItem.Text = LocalizationManager.Get("AiSettingsTitle");

            // Status
            toolStripStatusLabel1.Text = LocalizationManager.Get("Ready");

            // Context Menu (titles updated in Opening event usually, but set defaults)
            deleteToolStripMenuItem.Text = LocalizationManager.Get("ContextDelete");

            this.Text = "HoboEX Dialogue Editor";

            if (currentRoot != null) UpdateTree();
        }

        private void NewFile()
        {
            using (var selector = new NpcSelectorForm(DialogueModels.NPC_ARCHETYPES))
            {
                if (selector.ShowDialog() == DialogResult.OK)
                {
                    currentRoot = new DialogueFileRoot();
                    currentRoot.dialogues.Add(new NpcDialogueJson { npcArchetype = selector.SelectedArchetype });
                    currentFilePath = null;
                    UpdateTree();
                    toolStripStatusLabel1.Text = LocalizationManager.Get("NewFileCreated");
                }
            }
        }

        private void UpdateTree()
        {
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();

            var rootNode = new TreeNode(LocalizationManager.Get("RootNode")) { Tag = currentRoot };
            treeView1.Nodes.Add(rootNode);

            foreach (var npc in currentRoot.dialogues)
            {
                var npcNode = new TreeNode(npc.ToString()) { Tag = npc };
                rootNode.Nodes.Add(npcNode);

                foreach (var opt in npc.entryOptions)
                {
                    AddOptionNode(npcNode, opt);
                }
            }

            treeView1.ExpandAll();
            treeView1.EndUpdate();
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
                condsNode.Expand();
            }

            // Actions Group
            if (opt.actions.Count > 0)
            {
                var actsNode = new TreeNode(LocalizationManager.Get("NodeActions")) { ImageKey = "folder", SelectedImageKey = "folder" };
                node.Nodes.Add(actsNode);
                foreach (var act in opt.actions)
                    actsNode.Nodes.Add(new TreeNode(act.ToString()) { Tag = act });
                actsNode.Expand();
            }

            // L10n Group
            if (opt.l10n.Count > 0)
            {
                var l10nNode = new TreeNode(LocalizationManager.Get("NodeL10n")) { ImageKey = "folder", SelectedImageKey = "folder" };
                node.Nodes.Add(l10nNode);
                foreach (var kvp in opt.l10n)
                    l10nNode.Nodes.Add(new TreeNode(new L10nItem(kvp.Key, kvp.Value, opt).ToString()) { Tag = new L10nItem(kvp.Key, kvp.Value, opt) });
                l10nNode.Expand();
            }

            foreach (var react in opt.reactions)
                AddReactionNode(node, react);
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
                actsNode.Expand();
            }

            // L10n Group
            if (react.l10n.Count > 0)
            {
                var l10nNode = new TreeNode(LocalizationManager.Get("NodeL10n")) { ImageKey = "folder", SelectedImageKey = "folder" };
                node.Nodes.Add(l10nNode);
                foreach (var kvp in react.l10n)
                    l10nNode.Nodes.Add(new TreeNode(new L10nItem(kvp.Key, kvp.Value, react).ToString()) { Tag = new L10nItem(kvp.Key, kvp.Value, react) });
                l10nNode.Expand();
            }

            foreach (var opt in react.options)
                AddOptionNode(node, opt);
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            propertyGrid1.SelectedObject = e.Node.Tag;
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
                if (node.Tag is L10nItem item)
                {
                    if (item.Parent is DialogueOptionJson opt) opt.l10n[item.Language] = item.Text;
                    else if (item.Parent is DialogueReactionJson react) react.l10n[item.Language] = react.l10n[item.Language] = item.Text;
                }
                node.Text = node.Tag.ToString() ?? "Entity";
                
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
            NewFile();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string json = File.ReadAllText(ofd.FileName);
                        var loaded = JsonSerializer.Deserialize<DialogueFileRoot>(json);
                        if (loaded != null)
                        {
                            currentRoot = loaded;
                            currentFilePath = ofd.FileName;
                            LoadL10n(ofd.FileName);
                            UpdateTree();
                            toolStripStatusLabel1.Text = string.Format(LocalizationManager.Get("FileOpened"), Path.GetFileName(ofd.FileName));
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format(LocalizationManager.Get("LoadError"), ex.Message), LocalizationManager.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
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
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    Save(sfd.FileName);
                }
            }
        }

        private void Save(string path)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                string json = JsonSerializer.Serialize(currentRoot, options);
                File.WriteAllText(path, json);
                SaveL10n(path);
                currentFilePath = path;
                toolStripStatusLabel1.Text = string.Format(LocalizationManager.Get("FileSaved"), Path.GetFileName(path));
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(LocalizationManager.Get("SaveError"), ex.Message), LocalizationManager.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            // 1. Logic States
            bool canAddOption = tag is DialogueFileRoot || tag is NpcDialogueJson || tag is DialogueReactionJson;
            bool canAddReaction = tag is DialogueOptionJson;
            bool canAddAction = tag is DialogueOptionJson || tag is DialogueReactionJson || isActionsNode;
            bool canAddCondition = tag is DialogueOptionJson || isConditionsNode;
            bool canAddL10n = tag is DialogueOptionJson || tag is DialogueReactionJson || isL10nNode;

            bool isDataNode = tag is DialogueActionJson || tag is DialogueConditionJson || tag is DialogueOptionJson || tag is DialogueReactionJson;
            bool canCopy = isDataNode;
            bool canCut = isDataNode && !(tag is DialogueOptionJson && node.Parent?.Tag is NpcDialogueJson); 
            
            bool canPaste = false;
            if (clipboardData != null)
            {
                if (clipboardData is DialogueActionJson && (tag is DialogueOptionJson || tag is DialogueReactionJson || isActionsNode)) canPaste = true;
                else if (clipboardData is DialogueConditionJson && (tag is DialogueOptionJson || isConditionsNode)) canPaste = true;
                else if (clipboardData is DialogueOptionJson && (tag is DialogueReactionJson || tag is NpcDialogueJson)) canPaste = true;
                else if (clipboardData is DialogueReactionJson && tag is DialogueOptionJson) canPaste = true;
            }
            bool canShowPaste = tag is DialogueOptionJson || tag is DialogueReactionJson || tag is NpcDialogueJson || isActionsNode || isConditionsNode;
            bool canDelete = !(tag is DialogueFileRoot);

            // 2. Apply Visibility & Localization
            addOptionToolStripMenuItem.Visible = canAddOption;
            addOptionToolStripMenuItem.Text = tag is DialogueFileRoot ? LocalizationManager.Get("ContextAddNPC") : LocalizationManager.Get("ContextAddOption");
            
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
            bool anyAdd = canAddOption || canAddReaction || canAddAction || canAddCondition || canAddL10n;
            bool anyEdit = canCopy || canCut || canShowPaste;
            bool anyDelete = canDelete;

            toolStripSeparator1.Available = anyAdd && (anyEdit || anyDelete);
            toolStripSeparator2.Available = anyEdit && anyDelete;
        }

        private string GenerateUniqueID(TreeNode contextNode, string typeSuffix)
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

            // Get all existing IDs in the whole file
            var existingIds = new HashSet<string>();
            GatherIds(currentRoot, existingIds);

            int seq = 1;
            while (existingIds.Contains($"{basePrefix}_{seq}"))
            {
                seq++;
            }

            return $"{basePrefix}_{seq}";
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

        private void addOptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = treeView1.SelectedNode;
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
            }
            else if (node.Tag is DialogueOptionJson opt)
            {
                opt.conditions.Add(cond);
                RefreshOptionNode(node);
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
            
            // Deep copy via Json
            string json = JsonSerializer.Serialize(node.Tag);
            if (node.Tag is DialogueActionJson) clipboardData = JsonSerializer.Deserialize<DialogueActionJson>(json);
            else if (node.Tag is DialogueConditionJson) clipboardData = JsonSerializer.Deserialize<DialogueConditionJson>(json);
            else if (node.Tag is DialogueOptionJson) clipboardData = JsonSerializer.Deserialize<DialogueOptionJson>(json);
            else if (node.Tag is DialogueReactionJson) clipboardData = JsonSerializer.Deserialize<DialogueReactionJson>(json);
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = treeView1.SelectedNode;
            if (node == null || clipboardData == null) return;

            // Clone data for multiple pastes
            string json = JsonSerializer.Serialize(clipboardData);
            object clone = null;
            if (clipboardData is DialogueActionJson) clone = JsonSerializer.Deserialize<DialogueActionJson>(json);
            else if (clipboardData is DialogueConditionJson) clone = JsonSerializer.Deserialize<DialogueConditionJson>(json);

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
                else if (node.Tag is NpcDialogueJson n) { n.entryOptions.Add(newOpt); UpdateTree(); }
            }
            else if (clone is DialogueReactionJson newReact)
            {
                if (node.Tag is DialogueOptionJson o) { o.reactions.Add(newReact); RefreshOptionNode(node); }
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
                else if (targetTag is DialogueOptionJson) { MoveCondition(draggedNode, targetNode); allowed = true; }
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

            if (targetParentNode.Tag is DialogueOptionJson to) to.conditions.Add(cond);
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
                Log("Starting AI Translation...");
                toolStripStatusLabel1.Text = LocalizationManager.Get("AiTranslating");
                this.Cursor = Cursors.WaitCursor;

                // 1. Gather ONLY nodes that are missing at least one of the target languages
                var targetLangs = LocalizationManager.Settings.AiTargetLanguages;
                if (targetLangs == null || !targetLangs.Any()) targetLangs = new List<string> { "zh", "en" };

                var nodesToTranslate = new List<dynamic>();
                GatherMissingTextNodes(currentRoot, targetLangs, nodesToTranslate);

                if (nodesToTranslate.Count == 0)
                {
                    Log("All nodes are already translated for the selected languages.");
                    MessageBox.Show("No new text found to translate.");
                    return;
                }

                Log($"Gathered {nodesToTranslate.Count} nodes missing translations. Target languages scope: {string.Join(", ", targetLangs)}");

                // --- SPEED OPTIMIZATION: BATCHING & PARALLELISM ---
                int batchSize = 10;
                int maxParallelTasks = 3; // Keep it modest to avoid Rate Limits (429)
                var batches = new List<List<dynamic>>();
                for (int i = 0; i < nodesToTranslate.Count; i += batchSize)
                {
                    batches.Add(nodesToTranslate.GetRange(i, Math.Min(batchSize, nodesToTranslate.Count - i)));
                }

                Log($"Split into {batches.Count} batches. Parallel degree: {maxParallelTasks}");

                var semaphore = new System.Threading.SemaphoreSlim(maxParallelTasks);
                var tasks = batches.Select(async (batch, index) => {
                    await semaphore.WaitAsync();
                    try {
                        Log($"[Batch {index+1}/{batches.Count}] Sending request...");
                        var jsonBatch = JsonSerializer.Serialize(batch);
                        var result = await DeepSeekService.TranslateAsync($"Context: {jsonBatch}");
                        Log($"[Batch {index+1}/{batches.Count}] Received.");
                        return result;
                    } catch (Exception ex) {
                        Log($"[Batch {index+1}] Failed: {ex.Message}", true);
                        return null;
                    } finally {
                        semaphore.Release();
                    }
                });

                var batchResults = await Task.WhenAll(tasks);
                
                // --- APPLY & CHECK FOR MISSING LANGUAGES ---
                Log("Applying results and checking for missing languages...");
                
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
                    Log($"Missing translations detected in {missingTranslations.Count} nodes. Retrying specifically for these...", true);
                    // Single batch for retry usually sufficient if not too many
                    var retryJson = JsonSerializer.Serialize(missingTranslations);
                    var retryResponse = await DeepSeekService.TranslateAsync($"COMPLETION_MODE: Translate the following missing nodes into the specified languages ONLY. Return original JSON structure. Context: {retryJson}");
                    
                    if (!string.IsNullOrEmpty(retryResponse))
                    {
                        try {
                            using var retryDoc = JsonDocument.Parse(retryResponse);
                            ApplyTranslationsRecursive(currentRoot, retryDoc.RootElement);
                            Log("Retry successful. Missing translations filled.");
                        } catch (Exception ex) {
                            Log($"Retry failed to parse: {ex.Message}", true);
                            Log(retryResponse);
                        }
                    }
                }

                UpdateTree();
                Log("AI Translation process finished.");
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
