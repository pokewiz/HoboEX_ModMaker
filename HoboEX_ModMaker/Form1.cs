using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Windows.Forms;
using HoboEX_ModMaker.Models;

namespace HoboEX_ModMaker
{
    public partial class Form1 : Form
    {
        private string currentFilePath = null;
        private DialogueFileRoot currentRoot = null;

        public Form1()
        {
            LocalizationManager.LoadSettings();
            InitializeComponent();
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
            this.StartPosition = FormStartPosition.CenterScreen;
            ApplyLocalization();

            // Initial default for startup
            currentRoot = new DialogueFileRoot();
            currentRoot.dialogues.Add(new NpcDialogueJson { npcArchetype = "Hobo_Majsner" });
            UpdateTree();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.SuppressKeyPress = true;
                saveToolStripMenuItem_Click(sender, e);
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
            if (node == null)
            {
                e.Cancel = true;
                return;
            }

            object tag = node.Tag;
            addOptionToolStripMenuItem.Visible = tag is NpcDialogueJson || tag is DialogueReactionJson;
            addReactionToolStripMenuItem.Visible = tag is DialogueOptionJson;

            // Context menu for Actions/Conditions container nodes
            bool isActionsNode = node.Text == LocalizationManager.Get("NodeActions");
            bool isConditionsNode = node.Text == LocalizationManager.Get("NodeConditions");

            addActionToolStripMenuItem.Visible = tag is DialogueOptionJson || tag is DialogueReactionJson || isActionsNode;
            addConditionToolStripMenuItem.Visible = tag is DialogueOptionJson || isConditionsNode;
            
            bool isL10nNode = node.Text == (LocalizationManager.Get("NodeL10n") ?? "Localization");
            addL10nToolStripMenuItem.Visible = tag is DialogueOptionJson || tag is DialogueReactionJson || isL10nNode;

            deleteToolStripMenuItem.Visible = !(tag is DialogueFileRoot);

            // Translate context menu
            addReactionToolStripMenuItem.Text = LocalizationManager.Get("ContextAddReaction");
            addActionToolStripMenuItem.Text = LocalizationManager.Get("ContextAddAction");
            addConditionToolStripMenuItem.Text = LocalizationManager.Get("ContextAddCondition");
            addL10nToolStripMenuItem.Text = LocalizationManager.Get("ContextAddL10n") ?? "Add Localization";
            deleteToolStripMenuItem.Text = LocalizationManager.Get("ContextDelete");

            // Special: Allow adding NPC to root
            if (tag is DialogueFileRoot)
            {
                addOptionToolStripMenuItem.Text = LocalizationManager.Get("ContextAddNPC");
                addOptionToolStripMenuItem.Visible = true;
            }
            else
            {
                addOptionToolStripMenuItem.Text = LocalizationManager.Get("ContextAddOption");
            }
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
    }
}
