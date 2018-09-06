using ESRI.ArcGIS.Carto;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NEWAB.BackUpTool
{
    public partial class LayerSelectPanel : Form
    {
        private static LayerSelectPanel m_LyrListWin; 
        private List<CheckBox> cboxRef;

        private List<string> CheckedItems;

        public LayerSelectPanel()
        {
            InitializeComponent();
            this.Load += LayerSelectPanel_Load;
        }

        void LayerSelectPanel_Load(object sender, EventArgs e)
        {
            cboxRef = new List<CheckBox>();
            CheckedItems = new List<string>();
        }


        public void FillList(object data)
        {
            IEnumLayer layers = data as IEnumLayer;
            ILayer lyr = layers.Next();
            while (lyr != null)
            {
                AddRow(lyr.Name, lyr);
            }
            this.Show();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.FormClosing += LayerSelectPanel_FormClosing;
            this.Hide();
        }

        void LayerSelectPanel_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            this.Hide();
        }

        private void AddRow(string t1, string t2)
        {
            int rowIndex = AddTableRow();
            CheckBox cbox = new CheckBox() { Text = t1 };
            TextBox tbox1 = new TextBox() { Text = t2 };
            tbox1.Enabled = false;
            cbox.CheckedChanged += cbox_CheckedChanged;
            this.tableLayoutPanel1.Controls.Add(cbox, 0, rowIndex);
            this.tableLayoutPanel1.Controls.Add(tbox1, 1, rowIndex);
            this.cboxRef.Add(cbox);
        }
        private void AddRow(string t1, ILayer t2)
        {
            int rowIndex = AddTableRow();
            CheckBox cbox = new CheckBox() { Text = t1 };
            cbox.Tag = t2;
            cbox.CheckedChanged += cbox_CheckedChanged;
            this.tableLayoutPanel1.Controls.Add(cbox, 0, rowIndex);
            this.cboxRef.Add(cbox);
        }

        void cbox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cbox = sender as CheckBox;
            if (cbox.Text == "全选")
            {
                if (cbox.Checked)
                {
                    for (int i = 1; i < this.cboxRef.Count - 1; i++)
                    {
                        this.cboxRef[i].Checked = true;
                    }
                }
                else
                {
                    for (int i = 1; i < this.cboxRef.Count - 1; i++)
                    {
                        this.cboxRef[i].Checked = false;
                    }
                }
            }
            if (cbox.Checked && cbox.Text != "全选")
            {
                this.CheckedItems.Add(cbox.Text);
            }
            else
            {
                this.CheckedItems.Remove(cbox.Text);
            }
        }
        private int AddTableRow()
        {
            int index = this.tableLayoutPanel1.RowCount++;
            RowStyle style = new RowStyle(SizeType.AutoSize);
            this.tableLayoutPanel1.RowStyles.Add(style);
            return index;
        }
    }
}
