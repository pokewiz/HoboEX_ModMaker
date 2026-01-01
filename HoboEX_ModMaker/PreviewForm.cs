using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HoboEX_ModMaker.Models;

namespace HoboEX_ModMaker
{
    public partial class PreviewForm : Form
    {
        private object _startNode;
        private object _currentNode;
        private List<DialogueOptionJson> _currentOptions = new List<DialogueOptionJson>();
        private string _npcName = "NPC";

        public PreviewForm(object startNode, string npcName = "NPC")
        {
            InitializeComponent();
            _startNode = startNode;
            _currentNode = startNode;
            
            // Fix: npcName should come from node if it's NpcDialogueJson
            if (startNode is NpcDialogueJson npc) _npcName = npc.npcArchetype;
            else _npcName = npcName;
            
            this.Text = $"Dialogue Preview - {_npcName}";
            
            // Set dark theme
            this.BackColor = Color.FromArgb(30, 30, 30);
            lblNpcName.ForeColor = Color.Goldenrod;
            lblNpcText.ForeColor = Color.White;
            flowOptions.FlowDirection = FlowDirection.TopDown;
            flowOptions.WrapContents = false;

            ShowNode(_currentNode);
        }

        private void ShowNode(object node)
        {
            flowOptions.Controls.Clear();
            _currentNode = node;

            if (node is NpcDialogueJson npc)
            {
                lblNpcName.Text = _npcName;
                lblNpcText.Text = "..."; // Entry point usually just shows options
                _currentOptions = npc.entryOptions;
            }
            else if (node is DialogueReactionJson react)
            {
                lblNpcName.Text = _npcName;
                lblNpcText.Text = GetL10n(react.l10n, react.text);
                _currentOptions = react.options;
            }
            else if (node is DialogueOptionJson opt)
            {
                // If we land on an option, move to the first reaction
                if (opt.reactions.Any())
                {
                    ShowNode(opt.reactions.First());
                    return;
                }
                else
                {
                    lblNpcText.Text = "[End of Dialogue]";
                    _currentOptions = new List<DialogueOptionJson>();
                }
            }

            foreach (var opt in _currentOptions)
            {
                AddOptionButton(opt);
            }

            // Handle showNativeExit: If current reaction wants it, add a Back button
            if (node is DialogueReactionJson r && r.showNativeExit)
            {
                string backText = LocalizationManager.CurrentLanguage.StartsWith("zh") ? "[返回]" : "[Back]";
                AddSpecialButton(backText, Color.Gray, () => ShowNode(_startNode));
            }
        }

        private void AddOptionButton(DialogueOptionJson opt)
        {
            Label btn = new Label
            {
                Text = "> " + GetL10n(opt.l10n, opt.text),
                ForeColor = Color.LightSkyBlue,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                AutoSize = false,
                Width = flowOptions.Width - 40,
                Height = 35,
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand,
                Padding = new Padding(10, 0, 0, 0),
                Margin = new Padding(0, 2, 0, 2)
            };

            btn.MouseEnter += (s, e) => { btn.BackColor = Color.FromArgb(60, 60, 60); btn.ForeColor = Color.White; };
            btn.MouseLeave += (s, e) => { btn.BackColor = Color.Transparent; btn.ForeColor = Color.LightSkyBlue; };
            btn.Click += (s, e) => {
                if (opt.isExit)
                {
                    // USER requested: return to dialogue start
                    ShowNode(_startNode);
                }
                else if (opt.reactions.Any())
                {
                    ShowNode(opt.reactions.First());
                }
                else if (opt.shopType != ShopType.Null)
                {
                    string msg = LocalizationManager.CurrentLanguage.StartsWith("zh") 
                        ? $"已打开商店: {opt.shopType}" 
                        : $"Opened shop: {opt.shopType}";
                    MessageBox.Show(msg);
                }
                else
                {
                    MessageBox.Show("No reactions linked to this option (and IsExit is false).");
                }
            };

            flowOptions.Controls.Add(btn);
        }

        private void AddSpecialButton(string text, Color color, Action onClick)
        {
            Label btn = new Label
            {
                Text = "> " + text,
                ForeColor = color,
                Font = new Font("Segoe UI", 11, FontStyle.Italic),
                AutoSize = false,
                Width = flowOptions.Width - 40,
                Height = 35,
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand,
                Padding = new Padding(10, 0, 0, 0),
                Margin = new Padding(0, 2, 0, 2)
            };

            btn.MouseEnter += (s, e) => { btn.BackColor = Color.FromArgb(60, 60, 60); btn.ForeColor = Color.White; };
            btn.MouseLeave += (s, e) => { btn.BackColor = Color.Transparent; btn.ForeColor = color; };
            btn.Click += (s, e) => onClick();

            flowOptions.Controls.Add(btn);
        }

        private string GetL10n(Dictionary<string, string> l10n, string @default)
        {
            if (l10n != null)
            {
                string langShort = LocalizationManager.CurrentLanguage.Split('-')[0];
                if (l10n.TryGetValue(langShort, out var val) && !string.IsNullOrEmpty(val))
                    return val;
            }
            return @default;
        }
    }
}
