using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YuzuModDownloader
{
    public partial class frmSelect : Form
    {
        private List<string> selectedTitles = new List<string> { };
        public frmSelect()
        {
            InitializeComponent();
        }
        public frmSelect(Dictionary<string, string> titles)
        {
            init_titles(titles);
            InitializeComponent();
        }

        private void frmSelect_Load(object sender, EventArgs e)
        {
            treeView1.CheckBoxes = true;

            TreeNode mainNode = treeView1.Nodes.Add("Available Games");

            // Add nodes to treeView1.

            foreach (var title in this.titles.Keys)
            {
                TreeNode gameNode = mainNode.Nodes.Add(title);
                // Not Yet Implemented
                /*foreach (var mod in this.title_mods[titles])
                {
                    gameNode.Nodes.Add(mod);
                }*/
                
            }

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void init_titles(Dictionary<string, string> titles)
        {
            this.titles = titles;
        }

        public List<string> getSelectedTitles()
        {
            return selectedTitles;
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Action != TreeViewAction.ByKeyboard && e.Action != TreeViewAction.ByMouse)
                return;
            TreeNode mainNode = treeView1.Nodes[0];
            if (e.Node == mainNode)
            {
                foreach (TreeNode node in mainNode.Nodes)
                {
                    node.Checked = mainNode.Checked;
                }
            }
            else
            {
                int sum = 0;
                foreach (TreeNode node in mainNode.Nodes)
                {
                    sum += node.Checked ? 1 : 0;
                }

                if (sum == 0)
                {
                    mainNode.Checked = false;
                }
                else if (sum == mainNode.Nodes.Count)
                {
                    mainNode.Checked = true;
                }
                else
                {
                    mainNode.Checked = false;
                }
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach(TreeNode node in treeView1.Nodes[0].Nodes) // [0] -> All Games
            {
                if (!node.Checked)
                    continue;
                selectedTitles.Add(node.Text);
            }
            this.Close();
        }
    }
}
