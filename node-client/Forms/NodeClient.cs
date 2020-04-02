using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace node_client {
    public partial class NodeClient : Form {
        public NodeClient() {
            InitializeComponent();
            this.Icon = Properties.Resources.ctt_logo_icon;
            tabControlMain.DrawItem += new DrawItemEventHandler(TabControlMain_DrawItem);
        }
        /// <summary>
        /// Draws tabs of tabControl horizontally, applies color and text. This function assumes
        /// tabControl.Alignment {Left|Right} and tabControl.DrawMode {OwnerDrawFixed}.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControlMain_DrawItem(object sender, DrawItemEventArgs e) {

            TabControl control = sender as TabControl;
            TabPage tabPage = control.TabPages[e.Index];
            Font tabFont = new Font("Arial", 12.0f, FontStyle.Bold, GraphicsUnit.Pixel);
            Rectangle tabBounds = control.GetTabRect(e.Index);

            Brush textBrush;
            Graphics g = e.Graphics;
            if (e.State == DrawItemState.Selected) {
                textBrush = new SolidBrush(e.ForeColor);
                g.FillRectangle(Brushes.Gray, e.Bounds);
            } else {
                textBrush = new System.Drawing.SolidBrush(e.ForeColor);
                e.DrawBackground();
            }

            StringFormat stringFlags = new StringFormat();
            stringFlags.Alignment = StringAlignment.Center;
            stringFlags.LineAlignment = StringAlignment.Center;

            g.DrawString(tabPage.Text, tabFont, textBrush, tabBounds, new StringFormat(stringFlags));
        }
    }
}
