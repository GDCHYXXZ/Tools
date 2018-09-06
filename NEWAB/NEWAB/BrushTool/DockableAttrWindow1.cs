using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace NEWAB
{
    
    /// <summary>
    /// Designer class of the dockable window add-in. It contains user interfaces that
    /// make up the dockable window.
    /// </summary>
    public partial class DockableAttrWindow1 : UserControl
    {
        public class ARow
        {
            public ARow() { }
            public CheckBox Cbox;
            public TextBox Tbox1;
            public TextBox Tbox2;
        }

        private static DockableAttrWindow1 m_DocWin;
        private IFeature SelectedFeature;
        public List<string> CheckedItems;
        private bool LockFeature = false;
        private List<CheckBox> cboxRef;
        List<CheckBox> b_cBox;
        List<string> b_CheckedItems;
        string CurrentWSPath;
        List<ARow> ControlContainer;
        public DockableAttrWindow1(object hook)
        {
            InitializeComponent();
            this.Hook = hook;
            this.Load += DockableAttrWindow1_Load;
        }

        void DockableAttrWindow1_Load(object sender, EventArgs e)
        {
            m_DocWin = this;
            CheckedItems = new List<string>();
            m_DocWin.b_CheckedItems = new List<string>();
            m_DocWin.b_cBox = new List<CheckBox>();
            cboxRef = new List<CheckBox>();
            m_DocWin.toolStripButton1.Image = NEWAB.Resource1.unlock;
            if (ArcMap.Editor.EditState != ESRI.ArcGIS.Editor.esriEditState.esriStateEditing)
                m_DocWin.toolStripButton1.Enabled = false;
            ControlContainer = new List<ARow>();
        }

        /// <summary>
        /// Host object of the dockable window
        /// </summary>
        private object Hook
        {
            get;
            set;
        }

        /// <summary>
        /// Implementation class of the dockable window add-in. It is responsible for 
        /// creating and disposing the user interface class of the dockable window.
        /// </summary>
        public class AddinImpl : ESRI.ArcGIS.Desktop.AddIns.DockableWindow
        {
            private DockableAttrWindow1 m_windowUI;

            public AddinImpl()
            {
            }

            protected override IntPtr OnCreateChild()
            {
                m_windowUI = new DockableAttrWindow1(this.Hook);
                return m_windowUI.Handle;
            }

            protected override void Dispose(bool disposing)
            {
                if (m_windowUI != null)
                    m_windowUI.Dispose(disposing);

                base.Dispose(disposing);
            }

        }

        public static void FillTree(IFeature feature)
        {
            if (!m_DocWin.LockFeature)
            {
                IDataset ds = feature.Class as IDataset;
                string newPath = ds.Workspace.PathName + "\\" + ds.BrowseName;
                if (m_DocWin.SelectedFeature != null && newPath == m_DocWin.CurrentWSPath) {
                    m_DocWin.RemoveAllItems();
                    m_DocWin.SelectedFeature = feature;
                    
                    foreach (string cb in m_DocWin.CheckedItems)
                    {
                        m_DocWin.b_CheckedItems.Add(cb);
                    }
                    if (m_DocWin != null && feature != null)
                    {
                        for (int i = 0; i < m_DocWin.ControlContainer.Count; i++)
                        {
                            if (i > 0)
                            {
                                string Text = feature.get_Value(feature.Fields.FindField(m_DocWin.ControlContainer[i].Cbox.Text.ToString())).ToString();
                                m_DocWin.ControlContainer[i].Tbox2.Text = Text;
                            }
                            if (m_DocWin.b_CheckedItems.Contains(m_DocWin.ControlContainer[i].Cbox.Text))
                                m_DocWin.ControlContainer[i].Cbox.Checked = true;
                            else
                                m_DocWin.ControlContainer[i].Cbox.Checked = false;
                            m_DocWin.tableLayoutPanel1.Controls.Add(m_DocWin.ControlContainer[i].Cbox, 0,i);
                            m_DocWin.tableLayoutPanel1.Controls.Add(m_DocWin.ControlContainer[i].Tbox1, 1, i);
                            m_DocWin.tableLayoutPanel1.Controls.Add(m_DocWin.ControlContainer[i].Tbox2, 2, i);
                        }
                    }
                }
                else
                {
                    m_DocWin.CurrentWSPath = ds.Workspace.PathName + "\\" + ds.BrowseName;
                    m_DocWin.b_cBox.Clear();
                    m_DocWin.b_CheckedItems.Clear(); 
                    m_DocWin.ControlContainer.Clear();
                    foreach (CheckBox cb in m_DocWin.cboxRef)
                    {
                        m_DocWin.b_cBox.Add(cb);
                    }
                    foreach (string cb in m_DocWin.CheckedItems)
                    {
                        m_DocWin.b_CheckedItems.Add(cb);
                    }
                    m_DocWin.cboxRef.Clear();
                    m_DocWin.tableLayoutPanel1.Controls.Clear();
                    m_DocWin.SelectedFeature = feature;
                    m_DocWin.toolStripButton1.Enabled = true;
                    m_DocWin.CheckedItems.Clear();

                    if (m_DocWin != null && feature != null)
                    {
                        m_DocWin.AddRow("全选", "-", "-");
                        for (int i = 0; i < feature.Fields.FieldCount; i++)
                        {
                            IField mField = feature.Fields.get_Field(i);
                            if (mField.Name.ToLower() != "ruleid" && mField.Name.ToLower() != "override" && mField.Name.ToLower() != "fid" && mField.Name.ToLower() != "shape" && mField.Name.ToLower() != "objectid" && mField.Name.ToLower() != "shape_length" && mField.Name.ToLower() != "shape_area")
                            {
                                m_DocWin.AddRow(mField.Name, mField.AliasName, feature.get_Value(i).ToString());
                            }
                        }
                    }
                    if (m_DocWin.b_cBox.Count == m_DocWin.cboxRef.Count)
                    {
                        for (int i = 0; i < m_DocWin.b_CheckedItems.Count; i++)
                        {
                            for (int j = 0; j < m_DocWin.cboxRef.Count; j++)
                            {
                                if (m_DocWin.b_CheckedItems[i] == m_DocWin.cboxRef[j].Text)
                                {
                                    m_DocWin.cboxRef[j].Checked = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        m_DocWin.b_cBox.Clear();
                        m_DocWin.b_CheckedItems.Clear();
                    }
                }
            }
        }

        private void RemoveAllItems() {
            for (int i = 0; i < m_DocWin.tableLayoutPanel1.Controls.Count; i++)
                m_DocWin.tableLayoutPanel1.Controls.RemoveAt(i);
        }


        private void InsertRow(string t1, string t2, string t3, int RowIndex)
        {
            CheckBox cbox = new CheckBox() { Text = t1 };
            TextBox tBox1 = new TextBox() { Text = t2 };
            TextBox tBox2 = new TextBox() { Text = t3 };
            tBox1.Enabled = tBox2.Enabled = false;
            cbox.CheckedChanged += cbox_CheckedChanged;
            //cbox.Checked = true;
            m_DocWin.tableLayoutPanel1.Controls.Add(cbox, 0, RowIndex);
            m_DocWin.tableLayoutPanel1.Controls.Add(tBox1, 1, RowIndex);
            m_DocWin.tableLayoutPanel1.Controls.Add(tBox2, 2, RowIndex);
            cboxRef.Insert(RowIndex,cbox);
        }

        private void AddRow(string t1, string t2, string t3)
        {
            int rowIndex = AddTableRow();
            CheckBox cbox = new CheckBox() { Text = t1 };
            TextBox tbox1 = new TextBox() { Text = t2 };
            TextBox tbox2 = new TextBox() { Text = t3 };
            tbox1.Enabled = tbox2.Enabled = false;
            cbox.CheckedChanged += cbox_CheckedChanged;
            //cbox.Checked = true;
            m_DocWin.tableLayoutPanel1.Controls.Add(cbox, 0, rowIndex);
            m_DocWin.tableLayoutPanel1.Controls.Add(tbox1, 1, rowIndex);
            m_DocWin.tableLayoutPanel1.Controls.Add(tbox2, 2, rowIndex);
            m_DocWin.cboxRef.Add(cbox);
            m_DocWin.ControlContainer.Add(new ARow() { Cbox = cbox, Tbox1 = tbox1, Tbox2 = tbox2 });
        }

        void cbox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cbox = sender as CheckBox;
            if (cbox.Text == "全选")
            {
                if (cbox.Checked)
                {
                    for (int i = 1; i < m_DocWin.cboxRef.Count - 1; i++)
                    {
                        m_DocWin.cboxRef[i].Checked = true;
                    }
                }
                else
                {
                    for (int i = 1; i < m_DocWin.cboxRef.Count - 1; i++)
                    {
                        m_DocWin.cboxRef[i].Checked = false;
                    }
                }
            }
            if (cbox.Checked && cbox.Text != "全选")
            {
                m_DocWin.CheckedItems.Add(cbox.Text);
            }
            else
            {
                m_DocWin.CheckedItems.Remove(cbox.Text);
            }
        }

        private int AddTableRow()
        {
            int index = m_DocWin.tableLayoutPanel1.RowCount++;
            RowStyle style = new RowStyle(SizeType.AutoSize);
            m_DocWin.tableLayoutPanel1.RowStyles.Add(style);
            return index;
        }

        public static void ClearCheckBoxListItem()
        {
            //m_DocWin.checkedListBox1.Items.Clear();
        }

        public static void FillSecondSelection(IFeatureCursor features)
        {
            IFeature feature;
            try
            {
                if (m_DocWin.CheckedItems.Count > 0)
                {
                    while ((feature = features.NextFeature()) != null)
                    {
                        foreach (string key in m_DocWin.CheckedItems)
                        {
                            int index = feature.Fields.FindField(key);
                            if (index > -1)
                            {
                                feature.set_Value(index, m_DocWin.SelectedFeature.get_Value(m_DocWin.SelectedFeature.Fields.FindField(key)));
                                feature.Store();
                            }
                        }
                       // features.UpdateFeature(feature);
                    }
                }
            }
            catch (COMException comExc)
            {
                MessageBox.Show("发生错误：" + comExc.Message.ToString());
            }

            feature = null;
            Marshal.ReleaseComObject(features);
        }
        public static void FillSecondSelection(IFeature feature)
        {
            try
            {
                if (m_DocWin.CheckedItems.Count > 0)
                {
                    if(feature  != null)
                    {
                        foreach (string key in m_DocWin.CheckedItems)
                        {
                            int index = feature.Fields.FindField(key);
                            if (index > -1)
                            {
                                feature.set_Value(index, m_DocWin.SelectedFeature.get_Value(m_DocWin.SelectedFeature.Fields.FindField(key)));
                                feature.Store();
                            }
                        }
                        // features.UpdateFeature(feature);
                    }
                }
            }
            catch (COMException comExc)
            {
                MessageBox.Show("发生错误：" + comExc.Message.ToString());
            }
            feature = null;
        }
        private void CheckBoxChanged(object sender, ItemCheckEventArgs e)
        {

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            m_DocWin.LockFeature = !m_DocWin.LockFeature;
            if (m_DocWin.LockFeature)
                m_DocWin.toolStripButton1.Image = NEWAB.Resource1.locked;
            else
                m_DocWin.toolStripButton1.Image = NEWAB.Resource1.unlock;
        }

        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            m_DocWin.LockFeature = !m_DocWin.LockFeature;
            if (m_DocWin.LockFeature)
                m_DocWin.toolStripButton1.Image = NEWAB.Resource1.locked;
            else
                m_DocWin.toolStripButton1.Image = NEWAB.Resource1.unlock;
        }
    }
}
