using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaintTextBoxClassLib
{
    public class frmTreeNode : TreeNode
    {
        /// <summary>
        /// 파라미터의 개수를 담을 공간
        /// </summary>
        public string parameters { get; set; }
        public string returnvalue { get; set; }
        public new List<frmTreeNode> Nodes = new List<frmTreeNode>();

        public frmTreeNode(string text)
        {
            base.Text = text;
        }

        public frmTreeNode()
        {
             
        }

        public override object Clone()
        {
            frmTreeNode node = base.Clone() as frmTreeNode;
            this.NodesCopy(this.Nodes, node.Nodes);
            node.Name = this.Name;
            node.Tag = this.Tag;
            node.Text = this.Text;
            node.parameters = this.parameters;
            node.returnvalue = this.returnvalue;
            return node;
        }

        //public frmTreeNode innerNodes(frmTreeNode sourceNode, frmTreeNode targetNode)
        //{
        //    foreach (frmTreeNode node in sourceNode.Nodes)
        //    {
        //        frmTreeNode tmpNode = new frmTreeNode(node.Text);
        //        tmpNode.parameters = node.parameters;
        //        tmpNode.returnvalue = node.returnvalue;
        //        if (node.Nodes.Count > 0)
        //        {
        //            innerNodes(node.Nodes
        //        }
        //        targetNode.Nodes.Add();
        //    }
        //}

        public List<frmTreeNode> NodesCopy(List<frmTreeNode> sourceNodes, List<frmTreeNode> targetNodes)
        {
            foreach (frmTreeNode sNode in sourceNodes)
            {
                frmTreeNode tmpNode = new frmTreeNode();
                tmpNode.Name = sNode.Name;
                tmpNode.Text = sNode.Text;
                tmpNode.parameters = sNode.parameters;
                tmpNode.returnvalue = sNode.returnvalue;
                tmpNode.Tag = sNode.Tag;
                NodesCopy(sNode.Nodes, tmpNode.Nodes);
                targetNodes.Add(tmpNode);
            }
            return targetNodes;
        }
    }
}