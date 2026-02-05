using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using HoboEX_ModMaker.Models;

namespace HoboEX_ModMaker
{
    public partial class MapLocationPicker : Form
    {
        private PictureBox pictureBox;
        private Panel mapPanel;
        private ListBox locationListBox;
        private Button addButton;
        private Button removeButton;
        private Button okButton;
        private Button cancelButton;
        private ComboBox typeComboBox;
        private Label typeLabel;
        private Image mapImage;
        
        // Game coordinate bounds (actual game coordinates)
        // Top-left corner of map: (25, 475)
        // Bottom-right corner of map: (-1075, -650)
        private const float GAME_MIN_X = -1075f;  // Bottom of map
        private const float GAME_MAX_X = 25f;     // Top of map
        private const float GAME_MIN_Y = -650f;   // Right of map
        private const float GAME_MAX_Y = 475f;    // Left of map
        
        // Image dimensions (visible map area)
        private const float IMAGE_WIDTH = 1102f;
        private const float IMAGE_HEIGHT = 1080f;  // Visible height from offset
        
        public List<MapPointJson> Locations { get; set; }
        
        public MapLocationPicker(List<MapPointJson> locations)
        {
            Locations = new List<MapPointJson>(locations ?? new List<MapPointJson>());
            InitializeComponent();
            LoadMapImage();
            RefreshLocationList();
        }
        
        private void InitializeComponent()
        {
            this.Text = LocalizationManager.Get("MapPickerTitle") ?? "Map Location Picker";
            this.Size = new Size(1480, 1140);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // Map panel with scrollbars
            mapPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(1120, 1080),
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            
            // PictureBox for map
            pictureBox = new PictureBox
            {
                Location = new Point(0, 0),
                SizeMode = PictureBoxSizeMode.AutoSize,
                Cursor = Cursors.Cross
            };
            pictureBox.MouseClick += PictureBox_MouseClick;
            pictureBox.Paint += PictureBox_Paint;
            
            mapPanel.Controls.Add(pictureBox);
            this.Controls.Add(mapPanel);
            
            // Location list
            Label listLabel = new Label
            {
                Text = LocalizationManager.Get("MapPickerLocations") ?? "Locations:",
                Location = new Point(1145, 10),
                Size = new Size(300, 20)
            };
            this.Controls.Add(listLabel);
            
            locationListBox = new ListBox
            {
                Location = new Point(1145, 35),
                Size = new Size(300, 820)
            };
            locationListBox.SelectedIndexChanged += LocationListBox_SelectedIndexChanged;
            this.Controls.Add(locationListBox);
            
            // Type selector
            typeLabel = new Label
            {
                Text = LocalizationManager.Get("MapPickerType") ?? "Type:",
                Location = new Point(1145, 865),
                Size = new Size(100, 20)
            };
            this.Controls.Add(typeLabel);
            
            typeComboBox = new ComboBox
            {
                Location = new Point(1145, 890),
                Size = new Size(300, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            typeComboBox.Items.AddRange(new object[] { 
                "0 - Cross (十字)",
                "1 - BigCircle (大圆圈)", 
                "2 - SmallCircle (小圆圈)" 
            });
            typeComboBox.SelectedIndex = 0;
            this.Controls.Add(typeComboBox);
            
            // Buttons
            addButton = new Button
            {
                Text = LocalizationManager.Get("MapPickerAddManual") ?? "Add Manually",
                Location = new Point(1145, 925),
                Size = new Size(145, 30)
            };
            addButton.Click += AddButton_Click;
            this.Controls.Add(addButton);
            
            removeButton = new Button
            {
                Text = LocalizationManager.Get("MapPickerRemove") ?? "Remove",
                Location = new Point(1300, 925),
                Size = new Size(145, 30),
                Enabled = false
            };
            removeButton.Click += RemoveButton_Click;
            this.Controls.Add(removeButton);
            
            okButton = new Button
            {
                Text = LocalizationManager.Get("ButtonOK") ?? "OK",
                Location = new Point(1145, 1040),
                Size = new Size(145, 35),
                DialogResult = DialogResult.OK
            };
            this.Controls.Add(okButton);
            
            cancelButton = new Button
            {
                Text = LocalizationManager.Get("ButtonCancel") ?? "Cancel",
                Location = new Point(1300, 1040),
                Size = new Size(145, 35),
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(cancelButton);
            
            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
        }
        
        private void LoadMapImage()
        {
            try
            {
                string mapPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "map.png");
                if (File.Exists(mapPath))
                {
                    mapImage = Image.FromFile(mapPath);
                    pictureBox.Image = mapImage;
                }
                else
                {
                    // Create a placeholder image
                    mapImage = new Bitmap(800, 600);
                    using (Graphics g = Graphics.FromImage(mapImage))
                    {
                        g.Clear(Color.LightGray);
                        g.DrawString("Map image not found\nPlace map.png in application directory", 
                            new Font("Arial", 16), Brushes.Black, new PointF(200, 280));
                    }
                    pictureBox.Image = mapImage;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load map: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void PictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && mapImage != null)
            {
                // Get the type from combo box
                int type = typeComboBox.SelectedIndex;
                
                // Convert image coordinates to game coordinates using linear interpolation:
                // Game X: ranges from GAME_MAX_X (top) to GAME_MIN_X (bottom)
                // Game Y: ranges from GAME_MAX_Y (left) to GAME_MIN_Y (right)
                // Note: X and Y are swapped between image and game coordinates
                
                float normalizedY = e.Y / IMAGE_HEIGHT;  // 0 at top, 1 at bottom
                float normalizedX = e.X / IMAGE_WIDTH;   // 0 at left, 1 at right
                
                float gameX = GAME_MAX_X + normalizedY * (GAME_MIN_X - GAME_MAX_X);
                float gameY = GAME_MAX_Y + normalizedX * (GAME_MIN_Y - GAME_MAX_Y);
                
                var location = new MapPointJson
                {
                    x = gameX,
                    y = gameY,
                    type = type
                };
                
                Locations.Add(location);
                RefreshLocationList();
                pictureBox.Invalidate(); // Trigger repaint
            }
        }
        
        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (Locations == null || Locations.Count == 0) return;
            
            // Draw all locations
            foreach (var loc in Locations)
            {
                DrawLocation(e.Graphics, loc, false);
            }
            
            // Highlight selected location
            if (locationListBox.SelectedIndex >= 0 && locationListBox.SelectedIndex < Locations.Count)
            {
                DrawLocation(e.Graphics, Locations[locationListBox.SelectedIndex], true);
            }
        }
        
        private void DrawLocation(Graphics g, MapPointJson loc, bool selected)
        {
            // Convert game coordinates back to image coordinates using reverse interpolation:
            // Reverse the linear interpolation used in mouse click
            
            // Calculate normalized position (0 to 1)
            float normalizedY = (loc.x - GAME_MAX_X) / (GAME_MIN_X - GAME_MAX_X);
            float normalizedX = (loc.y - GAME_MAX_Y) / (GAME_MIN_Y - GAME_MAX_Y);
            
            // Convert to image coordinates
            float imageX = normalizedX * IMAGE_WIDTH;
            float imageY = normalizedY * IMAGE_HEIGHT;
            
            Pen pen = selected ? new Pen(Color.Yellow, 3) : new Pen(Color.Red, 2);
            Brush brush = selected ? Brushes.Yellow : Brushes.Red;
            
            switch (loc.type)
            {
                case 0: // Cross (打叉 X形状)
                    // Draw an X marker (diagonal cross)
                    g.DrawLine(pen, imageX - 16, imageY - 16, imageX + 16, imageY + 16); // Top-left to bottom-right
                    g.DrawLine(pen, imageX - 16, imageY + 16, imageX + 16, imageY - 16); // Bottom-left to top-right
                    break;
                    
                case 1: // BigCircle (大圆圈) - doubled size
                    g.DrawEllipse(pen, imageX - 50, imageY - 50, 100, 100);
                    break;
                    
                case 2: // SmallCircle (小圆圈) - doubled size
                    g.DrawEllipse(pen, imageX - 24, imageY - 24, 48, 48);
                    break;
            }
            
            pen.Dispose();
        }
        
        private void RefreshLocationList()
        {
            int selectedIndex = locationListBox.SelectedIndex;
            locationListBox.Items.Clear();
            
            for (int i = 0; i < Locations.Count; i++)
            {
                var loc = Locations[i];
                string typeName = loc.type switch
                {
                    0 => "Cross",
                    1 => "BigCircle",
                    2 => "SmallCircle",
                    _ => "Unknown"
                };
                locationListBox.Items.Add($"[{i}] ({loc.x:F1}, {loc.y:F1}) - {typeName}");
            }
            
            if (selectedIndex >= 0 && selectedIndex < locationListBox.Items.Count)
            {
                locationListBox.SelectedIndex = selectedIndex;
            }
        }
        
        private void LocationListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            removeButton.Enabled = locationListBox.SelectedIndex >= 0;
            pictureBox.Invalidate(); // Repaint to show selection
        }
        
        private void AddButton_Click(object sender, EventArgs e)
        {
            // Show a simple input dialog
            using (var form = new Form())
            {
                form.Text = LocalizationManager.Get("MapPickerAddManual") ?? "Add Location Manually";
                form.Size = new Size(300, 200);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                
                Label xLabel = new Label { Text = "X:", Location = new Point(10, 20), Size = new Size(50, 20) };
                NumericUpDown xInput = new NumericUpDown { Location = new Point(70, 18), Size = new Size(200, 25), DecimalPlaces = 1, Minimum = -10000, Maximum = 10000 };
                
                Label yLabel = new Label { Text = "Y:", Location = new Point(10, 55), Size = new Size(50, 20) };
                NumericUpDown yInput = new NumericUpDown { Location = new Point(70, 53), Size = new Size(200, 25), DecimalPlaces = 1, Minimum = -10000, Maximum = 10000 };
                
                Label typeLabel = new Label { Text = "Type:", Location = new Point(10, 90), Size = new Size(50, 20) };
                ComboBox typeInput = new ComboBox { Location = new Point(70, 88), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
                typeInput.Items.AddRange(new object[] { "0 - Cross", "1 - BigCircle", "2 - SmallCircle" });
                typeInput.SelectedIndex = 0;
                
                Button okBtn = new Button { Text = "OK", Location = new Point(70, 125), Size = new Size(90, 30), DialogResult = DialogResult.OK };
                Button cancelBtn = new Button { Text = "Cancel", Location = new Point(180, 125), Size = new Size(90, 30), DialogResult = DialogResult.Cancel };
                
                form.Controls.AddRange(new Control[] { xLabel, xInput, yLabel, yInput, typeLabel, typeInput, okBtn, cancelBtn });
                form.AcceptButton = okBtn;
                form.CancelButton = cancelBtn;
                
                if (form.ShowDialog() == DialogResult.OK)
                {
                    var location = new MapPointJson
                    {
                        x = (float)xInput.Value,
                        y = (float)yInput.Value,
                        type = typeInput.SelectedIndex
                    };
                    Locations.Add(location);
                    RefreshLocationList();
                    pictureBox.Invalidate();
                }
            }
        }
        
        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (locationListBox.SelectedIndex >= 0 && locationListBox.SelectedIndex < Locations.Count)
            {
                Locations.RemoveAt(locationListBox.SelectedIndex);
                RefreshLocationList();
                pictureBox.Invalidate();
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                mapImage?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
