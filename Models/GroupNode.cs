﻿using Pdoxcl2Sharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPP.Models
{
    public class GroupNode : Node
    {
        public List<Node> Nodes { get; private set; } = new();

        public override void TokenCallback(ParadoxParser parser, string token)
        {
            try
            {
                if (parser.NextIsBracketed())
                {
                    Nodes.Add(parser.Parse(new GroupNode() { Name = token, Parent = this }));
                }

                else
                {
                    Nodes.Add(new ValueNode(token, parser.ReadString(), this));
                }
            }
            catch (Exception e)
            {
                StringBuilder nodePath = new StringBuilder(token);
                GroupNode parent = Parent;
                while (parent != null)
                {
                    nodePath.Append(" <= " + parent.Name);
                    parent = parent.Parent;
                }

                throw new Exception($"Token exception, token: {nodePath.ToString()} \n{e.ToString()}");
            }
        }

        public string GetText(int indentLevel)
        {
            StringBuilder text = new StringBuilder();
            string baseTabulation = indentLevel >= 0 ? new string('\t', indentLevel) : "";
            string childrenTabulation = indentLevel >= 0 ? baseTabulation + "\t" : "";

            if (indentLevel >= 0)
            {
                text.AppendLine(baseTabulation + Name);
            }

            for (int i = 0; i < Nodes.Count; i++)
            {
                var node = Nodes[i];
                if (node is ValueNode)
                {
                    text.AppendLine(childrenTabulation + node.ToString());
                }
                else
                {
                    GroupNode groupNode = (GroupNode)node;
                    text.Append(groupNode.GetText(indentLevel + 1));
                }
            }

            return text.ToString();
        }
    }
}
