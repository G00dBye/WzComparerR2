﻿using System;
using System.Collections.Generic;
using System.Text;

using WzComparerR2.WzLib;

namespace WzComparerR2
{
    public class Item : ItemBase
    {
        public Item()
        {
            this.Props = new Dictionary<ItemPropType, int>();
            this.Specs = new Dictionary<ItemSpecType, int>();
        }

        public Dictionary<ItemPropType, int> Props { get; private set; }
        public Dictionary<ItemSpecType, int> Specs { get; private set; }

        public bool Cash
        {
            get { return GetBooleanValue(ItemPropType.cash); }
        }

        public bool TimeLimited
        {
            get { return GetBooleanValue(ItemPropType.timeLimited); }
        }

        public bool GetBooleanValue(ItemPropType type)
        {
            int value;
            return this.Props.TryGetValue(type, out value) && value != 0;
        }

        public static Item CreateFromNode(Wz_Node node)
        {
            Item item = new Item();
            int value;
            if (node == null
                || !Int32.TryParse(node.Text, out value)
                && !((value = node.Text.IndexOf(".img")) > -1 && Int32.TryParse(node.Text.Substring(0, value), out value)))
            {
                return null;
            }
            item.ItemID = value;

            //propType缓存
            Dictionary<string, ItemPropType> itemPropTypeDict = new Dictionary<string, ItemPropType>();
            foreach (ItemPropType type in Enum.GetValues(typeof(ItemPropType)))
                itemPropTypeDict[type.ToString()] = type;

            Wz_Node infoNode = node.FindNodeByPath("info");
            if (infoNode != null)
            {
                Wz_Node pngNode;
                foreach (Wz_Node subNode in infoNode.Nodes)
                {
                    switch (subNode.Text)
                    {
                        case "icon":
                            pngNode = subNode;
                            if (pngNode.Value is Wz_Uol)
                            {
                                Wz_Uol uol = pngNode.Value as Wz_Uol;
                                Wz_Node uolNode = uol.HandleUol(subNode);
                                if (uolNode != null)
                                {
                                    pngNode = uolNode;
                                }
                            }
                            if (pngNode.Value is Wz_Png)
                            {
                                item.Icon = new BitmapOrigin(pngNode);
                            }
                            break;
                        case "iconRaw":
                            pngNode = subNode;
                            if (pngNode.Value is Wz_Uol)
                            {
                                Wz_Uol uol = pngNode.Value as Wz_Uol;
                                Wz_Node uolNode = uol.HandleUol(subNode);
                                if (uolNode != null)
                                {
                                    pngNode = uolNode;
                                }
                            }
                            if (pngNode.Value is Wz_Png)
                            {
                                item.IconRaw = new BitmapOrigin(pngNode);
                            }
                            break;
                        default:
                            ItemPropType type;
                            if (itemPropTypeDict.TryGetValue(subNode.Text, out type))
                            {
                                try
                                {
                                    item.Props.Add(type, Convert.ToInt32(subNode.Value));
                                }
                                finally
                                {
                                }
                            }
                            break;
                    }
                }
            }

            //propType缓存
            Dictionary<string, ItemSpecType> itemSpecTypeDict = new Dictionary<string, ItemSpecType>();
            foreach (ItemSpecType type in Enum.GetValues(typeof(ItemSpecType)))
                itemSpecTypeDict[type.ToString()] = type;

            Wz_Node specNode = node.FindNodeByPath("spec");
            if (specNode != null)
            {
                foreach (Wz_Node subNode in specNode.Nodes)
                {
                    ItemSpecType type;
                    if (itemSpecTypeDict.TryGetValue(subNode.Text, out type))
                    {
                        try
                        {
                            item.Specs.Add(type, Convert.ToInt32(subNode.Value));
                        }
                        finally
                        {
                        }
                    }
                }
            }
            return item;
        }
    }
}
