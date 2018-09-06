using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;

namespace NEWAB.BackUpTool
{
    public class BackUpActivator : ESRI.ArcGIS.Desktop.AddIns.Tool
    {
        public static LayerSelectPanel panel;
        private IMap FocusMap;
        public BackUpActivator()
        {
            panel = new LayerSelectPanel();
            FocusMap = ArcMap.Document.FocusMap;
        }

        protected override void OnUpdate()
        {

        }

        protected override void OnActivate()
        {
            base.OnActivate();
            if (GetArcMapLayerCount() == 0)
            {
                MessageBox.Show("当前没有可备份图层！");
            }
            else
            {
                panel.FillList(ArcMap.Document.FocusMap.Layers);
                panel.Show();
            }

        }

        private static int GetArcMapLayerCount() {
            return ArcMap.Document.FocusMap.LayerCount;
        }
    }

}
