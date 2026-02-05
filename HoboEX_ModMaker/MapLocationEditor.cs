using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using HoboEX_ModMaker.Models;
using System.Collections.Generic;

namespace HoboEX_ModMaker
{
    public class MapLocationEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                IWindowsFormsEditorService editorService = 
                    provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

                if (editorService != null)
                {
                    List<MapPointJson> locations = value as List<MapPointJson>;
                    if (locations == null)
                    {
                        locations = new List<MapPointJson>();
                    }

                    using (MapLocationPicker picker = new MapLocationPicker(locations))
                    {
                        if (picker.ShowDialog() == DialogResult.OK)
                        {
                            return picker.Locations;
                        }
                    }
                }
            }

            return value;
        }
    }
}
