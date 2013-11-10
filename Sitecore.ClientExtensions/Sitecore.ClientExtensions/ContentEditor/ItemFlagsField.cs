using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Web.UI.HtmlControls;

namespace Sitecore.ClientExtensions.ContentEditor
{
    public class ItemFlagsField : FlagsField
    {
        protected override void SetSelectedItemsOnChecklist(Sitecore.Shell.Applications.ContentEditor.Checklist list)
        {
            var listValue = GetSelectedFlagItemsAsString(this.Source);
            if (!string.IsNullOrEmpty(listValue))
            {
                list.Value = listValue;
            }
        }
        protected virtual string GetSelectedFlagItemsAsString(string flagsPath)
        {
            var flagsFolderItem = Sitecore.Context.ContentDatabase.GetItem(flagsPath);
            var selectedIds = new List<ID>();
            if (flagsFolderItem != null)
            {
                foreach (Item flagItem in flagsFolderItem.Children)
                {
                    if (IsFlagSet(this.Value, flagItem["Value"]))
                    {
                        selectedIds.Add(flagItem.ID);
                    }
                }
            }
            if (selectedIds.Count == 0)
            {
                return null;
            }
            return selectedIds.Select(g => g.ToString()).Aggregate((a, b) => a + "|" + b);
        }

        protected override void SetChecklistSource(Sitecore.Shell.Applications.ContentEditor.Checklist list)
        {
            list.Source = this.Source;
        }
        protected override int GetValueFromChecklistItem(ChecklistItem item)
        {
            var item2 = item as DataChecklistItem;
            Assert.IsNotNull(item2, "Checklist item {0} is not bound to a Sitecore item.", item.Header);
            var flagItem = Sitecore.Context.ContentDatabase.GetItem(new ID(item2.ItemID));
            var flagValue = GetFlagValue(flagItem["Value"]);
            Assert.IsTrue(flagValue.HasValue, "Value field on item {0} is not a valid flag.", item2.ItemID);
            return flagValue.GetValueOrDefault(0);
        }
        protected override int GetValueFromChecklist(Sitecore.Shell.Applications.ContentEditor.Checklist list)
        {
            var value = 0;
            foreach (DataChecklistItem item in list.Items)
            {
                if (item.Checked)
                {
                    var flagItem = Sitecore.Context.ContentDatabase.GetItem(new ID(item.ItemID));
                    var flagValue = GetFlagValue(flagItem["Value"]);
                    Assert.IsTrue(flagValue.HasValue, "Value field on item {0} is not a valid flag.", item.ItemID);
                    value += flagValue.GetValueOrDefault(0);
                }
            }
            return value;
        }

    }
}
