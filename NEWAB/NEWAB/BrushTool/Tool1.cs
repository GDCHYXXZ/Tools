using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.Editor;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoDatabaseUI;

namespace NEWAB
{
    public class SelectionFeature
    {
        public SelectionFeature() { }
        public SelectionFeature(IFeature sel_feat)
        {
            this.FeatureLayer = sel_feat.Class as IDataset;
            this.Feature = sel_feat;
            this.WorkSpacePath = this.FeatureLayer.Workspace.PathName + @"\" + this.FeatureLayer.BrowseName;
        }
        public IFeature Feature
        {
            get;
            set;
        }

        public IDataset FeatureLayer { get; set; }

        public string WorkSpacePath { get; set; }

    }

    public class Tool1 : ESRI.ArcGIS.Desktop.AddIns.Tool
    {
        IActiveView m_focusMap;
        ISelectionEnvironment env;
        IEnvelope p_Envelope;
        private IFeature m_mf;
        IRgbColor color_red;
        IRgbColor color_default;
        private bool m_onkeydown = false;
        private bool m_onmousedown = false;
        IEditor m_Editor;
        private ESRI.ArcGIS.Display.INewEnvelopeFeedback m_envelopFeedback;
        private static IDockableWindow attr_dockWin;
        System.Drawing.Icon myIcon;
        Dictionary<object, string> MultiFilterString;
        bool hasM = false;
        bool is_active = false;
        List<IFeature> tmpEnumFeats1;
        List<IFeature> tmpEnumFeats2;
        IEnumFeature EnumFeats;
        string EditLayerName = "";
        public Tool1()
        {
            IMxDocument mxDoc = ArcMap.Document;
            m_focusMap = mxDoc.FocusMap as IActiveView;
            env = new SelectionEnvironmentClass();
            color_red = new RgbColorClass();
            color_default = env.DefaultColor as IRgbColor;
            color_red.Red = 255;
            color_red.Blue = 0;
            color_red.Green = 0;
            env.DefaultColor = color_default;
            m_Editor = NEWAB.ArcMap.Editor;
            MultiFilterString = new Dictionary<object, string>();
            tmpEnumFeats1 = new List<IFeature>();
            tmpEnumFeats2 = new List<IFeature>();
        }

        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;

            if (m_Editor.EditState == esriEditState.esriStateNotEditing)
            {
                hasM = false;
            }
            if (hasM && is_active)
            {
                if (attr_dockWin != null)
                {
                    if (!attr_dockWin.IsVisible())
                    {
                        attr_dockWin.Show(true);
                    }
                }
                #region 实时更新选中项
                ITableWindow twin = IsAttributeTableShown();
                
                if (twin!=null)
                {
                    tmpEnumFeats2.Clear();
                    EnumFeats =  ArcMap.Editor.EditSelection;
                    IFeature feat = EnumFeats.Next();
                    while (feat != null)
                    {
                        if (!tmpEnumFeats2.Contains(feat))
                            tmpEnumFeats2.Add(feat);
                        else
                            break;
                        feat = EnumFeats.Next();
                    }
                    ArcMap.Editor.EditSelection.Reset();
                    if (tmpEnumFeats1.Count > 0)
                    {
                        if (!isSameSet(tmpEnumFeats1, tmpEnumFeats2))
                        {
                            UpdateSelection(tmpEnumFeats2);
                        }
                    }
                    else
                        if(tmpEnumFeats2.Count > 0 )
                            foreach(IFeature Feat in tmpEnumFeats2)
                                tmpEnumFeats1.Add(Feat);
                        
                }
                #endregion
            }

            
        }


        protected override void OnActivate()
        {
            base.OnActivate();
            is_active = true;
            if (m_Editor.EditState != esriEditState.esriStateEditing)
            {
                MessageBox.Show("未进入编辑模式");
                this.Cursor = System.Windows.Forms.Cursors.Default;
            }
            else
            {

                if (attr_dockWin == null)
                {
                    UIDClass dUID = new UIDClass();
                    dUID.Value = NEWAB.ThisAddIn.IDs.DockableAttrWindow1;
                    attr_dockWin = ArcMap.DockableWindowManager.GetDockableWindow(dUID);
                    attr_dockWin.Show(true);
                }
                //attr_dockWin.Show(true);
                if (ArcMap.Editor.SelectionCount == 1)
                {
                    m_mf = null;
                    env.DefaultColor = color_default;
                    //IEnumFeature EnumFeats = ArcMap.Document.FocusMap.FeatureSelection as IEnumFeature;
                    IEnumFeature EnumFeats1 = ArcMap.Editor.EditSelection;
                    m_mf = EnumFeats1.Next();

                    EditLayerName = m_mf.Class.AliasName;
                    if (m_mf != null)
                    {

                        DockableAttrWindow1.FillTree(m_mf);
                        if (!attr_dockWin.IsVisible())
                            attr_dockWin.Show(true);
                    }
                    hasM = true;
                    //MemoryStream ms = new MemoryStream(Resource1.paint_brush);
                    //ms.Position = 0;
                    //this.Cursor = new System.Windows.Forms.Cursor(ms);
                    this.Cursor = System.Windows.Forms.Cursors.Cross;
                }
                else
                {
                    if (!hasM)
                    {
                        this.Cursor = System.Windows.Forms.Cursors.Default;
                        MessageBox.Show("未选择模板要素");
                    }

                }
            }
        }

        protected override bool OnDeactivate()
        {
            if (attr_dockWin != null && attr_dockWin.IsVisible())
                attr_dockWin.Show(false);
            env.DefaultColor = color_default;
            is_active = false;
            return base.OnDeactivate();
        }

        protected override void OnMouseDown(ESRI.ArcGIS.Desktop.AddIns.Tool.MouseEventArgs arg)
        {
            base.OnMouseDown(arg);
            ArcMap.Document.FocusMap.ClearSelection();
            m_onmousedown = true;
            if (m_envelopFeedback != null)
            {
                m_envelopFeedback.Stop();
                m_envelopFeedback = null;
            }
            m_envelopFeedback = new ESRI.ArcGIS.Display.NewEnvelopeFeedback();
            m_envelopFeedback.Display = m_focusMap.ScreenDisplay;
            IPoint point = m_focusMap.ScreenDisplay.DisplayTransformation.ToMapPoint(arg.X, arg.Y) as IPoint;
            m_envelopFeedback.Start(point);
            p_Envelope = point.Envelope;
        }

        protected override void OnMouseUp(ESRI.ArcGIS.Desktop.AddIns.Tool.MouseEventArgs arg)
        {
            this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            base.OnMouseUp(arg);
            m_onmousedown = false;
            if (m_envelopFeedback!=null)
            {
                IEnvelope envelop = m_envelopFeedback.Stop();
                if (envelop.IsEmpty)
                    envelop = p_Envelope;
                if (m_Editor.EditState == esriEditState.esriStateEditing)
                {
                    if (hasM)
                    {
                        m_Editor.StartOperation();
                        MultiFilterString.Clear();
                        env.DefaultColor = color_red;
                        ArcMap.Document.FocusMap.SelectByShape(envelop, env, false);
                        EnumFeats = ArcMap.Editor.EditSelection;// ArcMap.Document.FocusMap.FeatureSelection as IEnumFeature;
                        IFeature feat = EnumFeats.Next();
                        string queryString = ""; 
                        IQueryFilter filter = new ESRI.ArcGIS.Geodatabase.QueryFilterClass();
                        if (feat != null)
                        {
                            while (feat != null)
                            {
                                IFeatureLayer ilyr = new FeatureLayer();
                                
                                ilyr.FeatureClass = feat.Table as IFeatureClass;
                                if (feat.HasOID)
                                {
                                    
                                    queryString = "\"" + feat.Table.OIDFieldName + "\" = " + feat.get_Value(feat.Fields.FindField(feat.Table.OIDFieldName)).ToString();

                                    filter.WhereClause = queryString;
                                    ESRI.ArcGIS.Geodatabase.IFeatureCursor featureCursor = ilyr.FeatureClass.Search(filter, false); //layer.FeatureClass.Search(spatialFilter, true);
                                    DockableAttrWindow1.FillSecondSelection(featureCursor);
                                    feat = EnumFeats.Next();
                                }
                            }

                        }

                        
                        //for (int i = 0; i < ArcMap.Document.FocusMap.LayerCount; i++)
                        //{
                        //    IFeatureLayer lyr = ArcMap.Document.FocusMap.get_Layer(i) as IFeatureLayer;
                        
                        //if (lyr.Selectable)
                        //{
                        //foreach (KeyValuePair<object, string> kvp in MultiFilterString)
                        //{
                        //    //if (lyr.FeatureClass.AliasName.ToString() == kvp.Key.ToString())
                        //    //{
                        //    IFeatureLayer lyr = kvp.Key as IFeatureLayer;
                        //    IQueryFilter filter = new ESRI.ArcGIS.Geodatabase.QueryFilterClass();
                        //    filter.WhereClause = kvp.Value;
                        //    ESRI.ArcGIS.Geodatabase.IFeatureCursor featureCursor = lyr.FeatureClass.Update(filter, false); //layer.FeatureClass.Search(spatialFilter, true);
                        //    DockableAttrWindow1.FillSecondSelection(featureCursor);
                        //    //}
                        //}
                        //}
                        //}
                        m_Editor.StopOperation("AttributeBrush");
                        //MemoryStream ms = new MemoryStream(Resource1.paint_brush);
                        //ms.Position = 0;
                        //this.Cursor = new System.Windows.Forms.Cursor(ms);
                        this.Cursor = System.Windows.Forms.Cursors.Cross;
                        m_focusMap.Refresh();
                    }

                }
                m_focusMap.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                m_envelopFeedback = null;
                //复制属性
                //IFeatureSelection features = ArcMap.Document.FocusMap.FeatureSelection as IFeatureSelection;
            }
        }

        private bool isSameSet(List<IFeature> LastFeats, List<IFeature> NextFeats)
        {
            if (LastFeats.Count != NextFeats.Count)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < LastFeats.Count; i++) {
                    if (LastFeats[i].OID != NextFeats[i].OID)
                        return false;
                }
                return true;
            }
        }
        protected override void OnKeyDown(ESRI.ArcGIS.Desktop.AddIns.Tool.KeyEventArgs arg)
        {
            //base.OnKeyDown(arg);
            if (arg.KeyCode == System.Windows.Forms.Keys.ControlKey)
                m_onkeydown = true;
            else
                m_onkeydown = false;
            if (arg.KeyCode == Keys.Enter && EnumFeats.Next() != null)
            {
               
            }
        }

        private ITableWindow IsAttributeTableShown()
        {
            ITableWindow pTableWindow = new TableWindowClass();
            pTableWindow.Application = ArcMap.Application;
            ITableWindow3 pTableWindow3 = pTableWindow as ITableWindow3;
            ISet pTableSet = new SetClass();
            pTableWindow3.FindOpenTableWindows(out pTableSet);
            if (pTableSet != null)
            {
                pTableSet.Reset();
                ITableWindow pTableWindowTemp = pTableSet.Next() as ITableWindow;
                if (pTableWindowTemp != null)
                {
                    while (pTableWindowTemp != null)
                    {
                        if (pTableWindowTemp.FeatureLayer.Name == EditLayerName)
                        {
                            if (pTableWindowTemp.IsVisible)
                                return pTableWindowTemp;
                            else
                                return null;
                        }
                        else
                        {
                            pTableWindowTemp = pTableSet.Next() as ITableWindow;
                        }
                    }
                }
                
            }
            return null;
        }
        private void UpdateSelection(List<IFeature> Twin)
        {
          
            //IFeatureLayer lyr = Twin.FeatureLayer;
            if (is_active)
            {
                if (m_Editor.EditState == esriEditState.esriStateEditing)
                {
                    if (hasM)
                    {
                        m_Editor.StartOperation();
                        MultiFilterString.Clear();
                        env.DefaultColor = color_red;
                        string queryString = "";
                        QueryFilter filter = new QueryFilter();
                        foreach (IFeature feat in Twin)
                        {
                            MessageBox.Show(feat.OID.ToString());
                            IFeatureLayer ilyr = new FeatureLayer();
                            ilyr.FeatureClass = feat.Table as IFeatureClass;
                            if (feat.HasOID)
                            {
                                queryString = "\"" + feat.Table.OIDFieldName + "\" = " + feat.get_Value(feat.Fields.FindField(feat.Table.OIDFieldName)).ToString();

                                filter.WhereClause = queryString;
                                ESRI.ArcGIS.Geodatabase.IFeatureCursor featureCursor = ilyr.FeatureClass.Search(filter, false); //layer.FeatureClass.Search(spatialFilter, true);
                                DockableAttrWindow1.FillSecondSelection(featureCursor);
                            }
                        }
                        m_Editor.StopOperation("AttributeBrush");
                        this.Cursor = System.Windows.Forms.Cursors.Cross;
                        m_focusMap.Refresh();
                    }
                }
                m_focusMap.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            }

        }
        protected override void OnKeyUp(ESRI.ArcGIS.Desktop.AddIns.Tool.KeyEventArgs arg)
        {
            //base.OnKeyUp(arg);
            m_onkeydown = false;
        }

        protected override void OnMouseMove(ESRI.ArcGIS.Desktop.AddIns.Tool.MouseEventArgs arg)
        {
            base.OnMouseMove(arg);
            IPoint point = m_focusMap.ScreenDisplay.DisplayTransformation.ToMapPoint(arg.X, arg.Y) as IPoint;
            if (m_envelopFeedback != null)
                m_envelopFeedback.MoveTo(point);
        }
    }

}
