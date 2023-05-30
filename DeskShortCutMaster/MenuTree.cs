using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace DeskShortCutMaster
{
    
    public class MenuTree
    {
        public uint id;
        public UInt16 DisplayPosition;
        public string DisplayName;
        public string NodeType;
        public string NodeCommand;
        public string NodeData;
        public MenuTree Parent;
        public List<MenuTree> children;
        public MenuTree(uint id, UInt16 pos, String display_name, String type, String command, String data, MenuTree parent = null)
        {
            children = new List<MenuTree>();
            this.id = id;
            this.DisplayPosition = pos;
            this.DisplayName = display_name;
            this.NodeType = type;
            this.NodeCommand = command;
            this.NodeData = data;
            this.Parent = parent;
        }

        public string Save()
        {

            return "";
        }

        public override string ToString()
        {
            String line = "";
            line += id.ToString() + " ";
            line += DisplayPosition.ToString() + " ";
            line += DisplayName + " ";
            line += NodeType + " ";
            line += NodeCommand + " ";
            line += NodeData + " ";
            if (Parent != null)
            {
                line += Parent.id.ToString() + " ";
            }
            else
            {
                line += "NULL ";
            }
            return line;
        }
    }

    public class Menu
    {
        public List<MenuTree> MenuList;
        public MenuTree ROOTNode;
        public Menu()
        {
            MenuList = new List<MenuTree>();
            ROOTNode = new MenuTree(0, 0, "", "ROOT", "", "");
            MenuList.Add(ROOTNode);
        }
        private void ProcessNodeString(MenuTree Node, string str, int paraPos)
        {
            if (paraPos == 0)
            {
                Node.id = (uint)int.Parse(str);
            }
            else if (paraPos == 1)
            {
                Node.DisplayPosition = (UInt16)int.Parse(str);
            }
            else if (paraPos == 2)
            {
                Node.DisplayName = str;
            }
            else if (paraPos == 3)
            {
                Node.NodeType = str;
            }
            else if (paraPos == 4)
            {
                Node.NodeCommand = str;
            }
            else if (paraPos == 5)
            {
                Node.NodeData = str;
            }
            else if (paraPos == 6)
            {
                foreach (MenuTree MenuNode in MenuList)
                {
                    if (MenuNode.id.ToString() == str)
                    {
                        
                        MenuNode.children.Add(Node);
                        Node.Parent = MenuNode;
                    }
                }
            }
        }
        public void GenerateTree(string str)
        {
            string[] lines = str.Split('\n');
            foreach (string line in lines)
            {
                string iline = line.Trim();
                if (iline != "")
                {
                    MenuList.Add(GenerateNode(iline));
                }
            }
        }

        private MenuTree GenerateNode(string line)
        {
            MenuTree Node = new MenuTree(0, 0, "", "", "", "");
            string[] paras = line.Split('/');
            int paraPos = 0;
            foreach (string para in paras)
            {
                //Console.WriteLine(paraPos.ToString()+" "+para);
                ProcessNodeString(Node, para, paraPos);
                paraPos++;
            }
            return Node;
        }
        private string StoreResult = "";
        private void GetTreeString(MenuTree Menu)
        {

            foreach(MenuTree item in Menu.children)
            {
                String line = "";
                MenuTree p = item;
                line += p.id.ToString() + "/";
                line += p.DisplayPosition.ToString() + "/";
                line += p.DisplayName + "/";
                line += p.NodeType + "/";
                line += p.NodeCommand + "/";
                line += p.NodeData + "/";
                line += p.Parent.id.ToString() + "/";
                line += "\r\n";
                StoreResult += line;
                GetTreeString(item);
            }
        }

        public string GetSaveString()
        {
            GetTreeString(ROOTNode);
            return StoreResult;
        }
    }
}
