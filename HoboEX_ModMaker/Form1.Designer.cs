namespace HoboEX_ModMaker
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            newToolStripMenuItem = new ToolStripMenuItem();
            saveToolStripMenuItem = new ToolStripMenuItem();
            saveAsToolStripMenuItem = new ToolStripMenuItem();
            languageToolStripMenuItem = new ToolStripMenuItem();
            chineseToolStripMenuItem = new ToolStripMenuItem();
            englishToolStripMenuItem = new ToolStripMenuItem();
            toolsToolStripMenuItem = new ToolStripMenuItem();
            previewToolStripMenuItem = new ToolStripMenuItem();
            syncL10nToolStripMenuItem = new ToolStripMenuItem();
            aiL10nToolStripMenuItem = new ToolStripMenuItem();
            aiSettingsToolStripMenuItem = new ToolStripMenuItem();
            openModToolStripMenuItem = new ToolStripMenuItem();
            splitContainerMain = new SplitContainer();
            splitContainer1 = new SplitContainer();
            treeView1 = new TreeView();
            contextMenuStrip1 = new ContextMenuStrip(components);
            addFileToolStripMenuItem = new ToolStripMenuItem();
            addOptionToolStripMenuItem = new ToolStripMenuItem();
            addReactionToolStripMenuItem = new ToolStripMenuItem();
            addActionToolStripMenuItem = new ToolStripMenuItem();
            addConditionToolStripMenuItem = new ToolStripMenuItem();
            addL10nToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            copyToolStripMenuItem = new ToolStripMenuItem();
            cutToolStripMenuItem = new ToolStripMenuItem();
            pasteToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            deleteToolStripMenuItem = new ToolStripMenuItem();
            propertyGrid1 = new PropertyGrid();
            txtConsole = new RichTextBox();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, languageToolStripMenuItem, toolsToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1008, 25);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newToolStripMenuItem, openModToolStripMenuItem, saveToolStripMenuItem, saveAsToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(39, 21);
            fileToolStripMenuItem.Text = "File";
            // 
            // newToolStripMenuItem
            // 
            newToolStripMenuItem.Name = "newToolStripMenuItem";
            newToolStripMenuItem.Size = new Size(121, 22);
            newToolStripMenuItem.Text = "New";
            newToolStripMenuItem.Click += newToolStripMenuItem_Click;
            // 
            // openModToolStripMenuItem
            // 
            openModToolStripMenuItem.Name = "openModToolStripMenuItem";
            openModToolStripMenuItem.Size = new Size(130, 22);
            openModToolStripMenuItem.Text = "Open Mod";
            openModToolStripMenuItem.Click += openModToolStripMenuItem_Click;
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new Size(121, 22);
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
            // 
            // saveAsToolStripMenuItem
            // 
            saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            saveAsToolStripMenuItem.Size = new Size(121, 22);
            saveAsToolStripMenuItem.Text = "Save As";
            saveAsToolStripMenuItem.Click += saveAsToolStripMenuItem_Click;
            // 
            // languageToolStripMenuItem
            // 
            languageToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { chineseToolStripMenuItem, englishToolStripMenuItem });
            languageToolStripMenuItem.Name = "languageToolStripMenuItem";
            languageToolStripMenuItem.Size = new Size(77, 21);
            languageToolStripMenuItem.Text = "Language";
            // 
            // chineseToolStripMenuItem
            // 
            chineseToolStripMenuItem.Name = "chineseToolStripMenuItem";
            chineseToolStripMenuItem.Size = new Size(124, 22);
            chineseToolStripMenuItem.Text = "简体中文";
            chineseToolStripMenuItem.Click += chineseToolStripMenuItem_Click;
            // 
            // englishToolStripMenuItem
            // 
            englishToolStripMenuItem.Name = "englishToolStripMenuItem";
            englishToolStripMenuItem.Size = new Size(124, 22);
            englishToolStripMenuItem.Text = "English";
            englishToolStripMenuItem.Click += englishToolStripMenuItem_Click;
            // 
            // toolsToolStripMenuItem
            // 
            toolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { previewToolStripMenuItem, syncL10nToolStripMenuItem, aiL10nToolStripMenuItem, aiSettingsToolStripMenuItem });
            toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            toolsToolStripMenuItem.Size = new Size(52, 21);
            toolsToolStripMenuItem.Text = "Tools";
            // 
            // previewToolStripMenuItem
            // 
            previewToolStripMenuItem.Name = "previewToolStripMenuItem";
            previewToolStripMenuItem.Size = new Size(177, 22);
            previewToolStripMenuItem.Text = "Preview Dialog";
            previewToolStripMenuItem.Click += previewToolStripMenuItem_Click;
            // 
            // syncL10nToolStripMenuItem
            // 
            syncL10nToolStripMenuItem.Name = "syncL10nToolStripMenuItem";
            syncL10nToolStripMenuItem.Size = new Size(177, 22);
            syncL10nToolStripMenuItem.Text = "Sync Text to L10n";
            syncL10nToolStripMenuItem.Click += syncL10nToolStripMenuItem_Click;
            // 
            // aiL10nToolStripMenuItem
            // 
            aiL10nToolStripMenuItem.Name = "aiL10nToolStripMenuItem";
            aiL10nToolStripMenuItem.Size = new Size(177, 22);
            aiL10nToolStripMenuItem.Text = "AI Translate (ZH)";
            aiL10nToolStripMenuItem.Click += aiL10nToolStripMenuItem_Click;
            // 
            // aiSettingsToolStripMenuItem
            // 
            aiSettingsToolStripMenuItem.Name = "aiSettingsToolStripMenuItem";
            aiSettingsToolStripMenuItem.Size = new Size(177, 22);
            aiSettingsToolStripMenuItem.Text = "AI Settings";
            aiSettingsToolStripMenuItem.Click += aiSettingsToolStripMenuItem_Click;
            // 
            // splitContainerMain
            // 
            splitContainerMain.Dock = DockStyle.Fill;
            splitContainerMain.Location = new Point(0, 25);
            splitContainerMain.Name = "splitContainerMain";
            splitContainerMain.Orientation = Orientation.Horizontal;
            // 
            // splitContainerMain.Panel1
            // 
            splitContainerMain.Panel1.Controls.Add(splitContainer1);
            // 
            // splitContainerMain.Panel2
            // 
            splitContainerMain.Panel2.Controls.Add(txtConsole);
            splitContainerMain.Size = new Size(1008, 804);
            splitContainerMain.SplitterDistance = 666;
            splitContainerMain.TabIndex = 3;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(treeView1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(propertyGrid1);
            splitContainer1.Size = new Size(1008, 666);
            splitContainer1.SplitterDistance = 333;
            splitContainer1.TabIndex = 1;
            // 
            // treeView1
            // 
            treeView1.ContextMenuStrip = contextMenuStrip1;
            treeView1.Dock = DockStyle.Fill;
            treeView1.Location = new Point(0, 0);
            treeView1.Name = "treeView1";
            treeView1.Size = new Size(333, 666);
            treeView1.TabIndex = 0;
            treeView1.AfterSelect += treeView1_AfterSelect;
            treeView1.NodeMouseClick += treeView1_NodeMouseClick;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { addFileToolStripMenuItem, addOptionToolStripMenuItem, addReactionToolStripMenuItem, addActionToolStripMenuItem, addConditionToolStripMenuItem, addL10nToolStripMenuItem, toolStripSeparator1, copyToolStripMenuItem, cutToolStripMenuItem, pasteToolStripMenuItem, toolStripSeparator2, deleteToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(173, 214);
            contextMenuStrip1.Opening += contextMenuStrip1_Opening;
            // 
            // addFileToolStripMenuItem
            // 
            addFileToolStripMenuItem.Name = "addFileToolStripMenuItem";
            addFileToolStripMenuItem.Size = new Size(172, 22);
            addFileToolStripMenuItem.Text = "Add File";
            addFileToolStripMenuItem.Click += addFileToolStripMenuItem_Click;
            // 
            // addOptionToolStripMenuItem
            // 
            addOptionToolStripMenuItem.Name = "addOptionToolStripMenuItem";
            addOptionToolStripMenuItem.Size = new Size(172, 22);
            addOptionToolStripMenuItem.Text = "Add Option";
            addOptionToolStripMenuItem.Click += addOptionToolStripMenuItem_Click;
            // 
            // addReactionToolStripMenuItem
            // 
            addReactionToolStripMenuItem.Name = "addReactionToolStripMenuItem";
            addReactionToolStripMenuItem.Size = new Size(172, 22);
            addReactionToolStripMenuItem.Text = "Add Reaction";
            addReactionToolStripMenuItem.Click += addReactionToolStripMenuItem_Click;
            // 
            // addActionToolStripMenuItem
            // 
            addActionToolStripMenuItem.Name = "addActionToolStripMenuItem";
            addActionToolStripMenuItem.Size = new Size(172, 22);
            addActionToolStripMenuItem.Text = "Add Action";
            addActionToolStripMenuItem.Click += addActionToolStripMenuItem_Click;
            // 
            // addConditionToolStripMenuItem
            // 
            addConditionToolStripMenuItem.Name = "addConditionToolStripMenuItem";
            addConditionToolStripMenuItem.Size = new Size(172, 22);
            addConditionToolStripMenuItem.Text = "Add Condition";
            addConditionToolStripMenuItem.Click += addConditionToolStripMenuItem_Click;
            // 
            // addL10nToolStripMenuItem
            // 
            addL10nToolStripMenuItem.Name = "addL10nToolStripMenuItem";
            addL10nToolStripMenuItem.Size = new Size(172, 22);
            addL10nToolStripMenuItem.Text = "Add Localization";
            addL10nToolStripMenuItem.Click += addL10nToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(169, 6);
            // 
            // copyToolStripMenuItem
            // 
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            copyToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.C;
            copyToolStripMenuItem.Size = new Size(172, 22);
            copyToolStripMenuItem.Text = "Copy";
            copyToolStripMenuItem.Click += copyToolStripMenuItem_Click;
            // 
            // cutToolStripMenuItem
            // 
            cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            cutToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.X;
            cutToolStripMenuItem.Size = new Size(172, 22);
            cutToolStripMenuItem.Text = "Cut";
            cutToolStripMenuItem.Click += cutToolStripMenuItem_Click;
            // 
            // pasteToolStripMenuItem
            // 
            pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            pasteToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.V;
            pasteToolStripMenuItem.Size = new Size(172, 22);
            pasteToolStripMenuItem.Text = "Paste";
            pasteToolStripMenuItem.Click += pasteToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(169, 6);
            // 
            // deleteToolStripMenuItem
            // 
            deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            deleteToolStripMenuItem.Size = new Size(172, 22);
            deleteToolStripMenuItem.Text = "Delete";
            deleteToolStripMenuItem.Click += deleteToolStripMenuItem_Click;
            // 
            // propertyGrid1
            // 
            propertyGrid1.Dock = DockStyle.Fill;
            propertyGrid1.Location = new Point(0, 0);
            propertyGrid1.Name = "propertyGrid1";
            propertyGrid1.Size = new Size(671, 666);
            propertyGrid1.TabIndex = 0;
            propertyGrid1.PropertyValueChanged += propertyGrid1_PropertyValueChanged;
            // 
            // txtConsole
            // 
            txtConsole.BackColor = Color.Black;
            txtConsole.Dock = DockStyle.Fill;
            txtConsole.ForeColor = Color.Lime;
            txtConsole.Location = new Point(0, 0);
            txtConsole.Name = "txtConsole";
            txtConsole.ReadOnly = true;
            txtConsole.Size = new Size(1008, 134);
            txtConsole.TabIndex = 0;
            txtConsole.Text = "";
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            statusStrip1.Location = new Point(0, 829);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1008, 22);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(44, 17);
            toolStripStatusLabel1.Text = "Ready";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1008, 851);
            Controls.Add(splitContainerMain);
            Controls.Add(menuStrip1);
            Controls.Add(statusStrip1);
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            Text = "HoboEX Dialogue Editor";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            contextMenuStrip1.ResumeLayout(false);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem newToolStripMenuItem;
        private ToolStripMenuItem openModToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem saveAsToolStripMenuItem;
        private ToolStripMenuItem languageToolStripMenuItem;
        private ToolStripMenuItem chineseToolStripMenuItem;
        private ToolStripMenuItem englishToolStripMenuItem;
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem previewToolStripMenuItem;
        private ToolStripMenuItem aiL10nToolStripMenuItem;
        private ToolStripMenuItem aiSettingsToolStripMenuItem;
        private ToolStripMenuItem syncL10nToolStripMenuItem;
        private SplitContainer splitContainer1;
        private SplitContainer splitContainerMain;
        private RichTextBox txtConsole;
        private TreeView treeView1;
        private PropertyGrid propertyGrid1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem addFileToolStripMenuItem;
        private ToolStripMenuItem addOptionToolStripMenuItem;
        private ToolStripMenuItem addReactionToolStripMenuItem;
        private ToolStripMenuItem addActionToolStripMenuItem;
        private ToolStripMenuItem addConditionToolStripMenuItem;
        private ToolStripMenuItem addL10nToolStripMenuItem; // New
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem copyToolStripMenuItem;
        private ToolStripMenuItem cutToolStripMenuItem;
        private ToolStripMenuItem pasteToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem deleteToolStripMenuItem;
    }
}
