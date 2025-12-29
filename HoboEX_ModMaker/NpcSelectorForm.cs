using System;
using System.Windows.Forms;

namespace HoboEX_ModMaker
{
    public partial class NpcSelectorForm : Form
    {
        public string SelectedArchetype { get; private set; }

        private ComboBox comboBox1;
        private Button btnOk;
        private Button btnCancel;

        public NpcSelectorForm(string[] archetypes)
        {
            this.Text = "Select NPC Archetype";
            this.Size = new System.Drawing.Size(300, 150);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            comboBox1 = new ComboBox { Left = 20, Top = 20, Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            comboBox1.Items.AddRange(archetypes);
            if (comboBox1.Items.Count > 0) comboBox1.SelectedIndex = 0;

            btnOk = new Button { Text = "OK", Left = 100, Top = 65, DialogResult = DialogResult.OK };
            btnCancel = new Button { Text = "Cancel", Left = 185, Top = 65, DialogResult = DialogResult.Cancel };

            this.Controls.Add(comboBox1);
            this.Controls.Add(btnOk);
            this.Controls.Add(btnCancel);

            btnOk.Click += (s, e) => { SelectedArchetype = comboBox1.SelectedItem?.ToString(); this.Close(); };
            btnCancel.Click += (s, e) => { this.Close(); };
            
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }
    }
}
