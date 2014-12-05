using System;
using System.Windows;
using System.Windows.Controls;
using Sharomank.RegexTester.Common;

namespace Sharomank.RegexTester
{
    public class TreeListViewItem : TreeViewItem
    {
        private bool _initialized;
        /// <summary>
        /// Item's hierarchy in the tree
        /// </summary>
        public int Level
        {
            get
            {
                if (_level == -1)
                {
                    var parent = ItemsControlFromItemContainer(this) as TreeListViewItem;
                    if (parent != null)
                    {
                        _level = parent.Level + 1;
                    }
                    else
                    {
                        _level = 0;
                    }
                }
                return _level;
            }
        }


        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeListViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeListViewItem;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            if (_initialized || this is CollectionTreeListViewItem) return;
            _initialized = true;
            var group = DataContext as GroupViewModel;
            var match = DataContext as MatchViewModel;
            if (group != null)
            {
                var treeListViewItem = new CollectionTreeListViewItem
                {
                    Header = string.Format("Captures {{Count={0}}}", group.Captures.Count)
                };
                treeListViewItem.SetBinding(ItemsSourceProperty, "Captures");
                Items.Add(treeListViewItem);
            }
            if (match != null)
            {
                var treeListViewItem = new CollectionTreeListViewItem
                {
                    Header = string.Format("Groups {{Count={0}}}", match.Groups.Count)
                };
                treeListViewItem.SetBinding(ItemsSourceProperty, "Groups");
                Items.Add(treeListViewItem);
            }
        }

        private int _level = -1;
    }
}