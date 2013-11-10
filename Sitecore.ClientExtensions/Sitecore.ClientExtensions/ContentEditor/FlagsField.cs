using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Web.UI.HtmlControls;

namespace Sitecore.ClientExtensions.ContentEditor
{
    public class FlagsField : Sitecore.Web.UI.HtmlControls.Input
    {
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
        protected virtual bool IsFlagSet(string flags, string flag)
        {
            var flagsValue = 0;
            var flagValue = 0;
            var didParse = int.TryParse(flags, out flagsValue);
            if (didParse)
            {
                didParse = int.TryParse(flag, out flagValue);
            }
            if (!didParse)
            {
                return false;
            }
            return ((flagsValue & flagValue) == flagValue);
        }
        protected virtual bool IsFlagValue(int value)
        {
            return (value & (value - 1)) == 0;
        }
        protected virtual int? GetFlagValue(string value)
        {
            var flagValue = 0;
            var didParse = int.TryParse(value, out flagValue);
            if (!didParse)
            {
                return null;
            }
            if (!IsFlagValue(flagValue))
            {
                return null;
            }
            return flagValue;
        }

        public string ItemID { get; set; }
        public string Source { get; set; }

        protected virtual void OnListClick()
        {
            Sitecore.Context.ClientPage.ClientResponse.SetReturnValue(true);
        }

        protected override void OnLoad(EventArgs e)
        {
            if (!Sitecore.Context.ClientPage.IsEvent)
            {
                var list = new Sitecore.Shell.Applications.ContentEditor.Checklist();
                this.Controls.Add(list);
                list.ID = GetID("list");
                list.Source = this.Source;
                list.ItemID = this.ItemID;
                list.TrackModified = this.TrackModified;
                list.Disabled = this.Disabled;
                list.Click = this.ID + ".OnListClick";  //This forces a refresh when the list is clicked. 
                                                        //Without this the component will not refresh.
                var listValue = GetSelectedFlagItemsAsString(this.Source);
                if (!string.IsNullOrEmpty(listValue))
                {
                    list.Value = listValue;
                }

                var text = new Sitecore.Shell.Applications.ContentEditor.Text();
                this.Controls.AddAt(0, text);
                text.ID = GetID("text");
                text.ReadOnly = true;
                text.Disabled = this.Disabled;

                this.Controls.Add(new LiteralControl(Sitecore.Resources.Images.GetSpacer(24, 16)));
            }
            else
            {
                var list = FindControl(GetID("list")) as Sitecore.Shell.Applications.ContentEditor.Checklist;
                if (list != null)
                {
                    var newValue = 0;
                    foreach (DataChecklistItem item in list.Items)
                    {
                        if (item.Checked)
                        {
                            var flagItem = Sitecore.Context.ContentDatabase.GetItem(new ID(item.ItemID));
                            var flagValue = GetFlagValue(flagItem["Value"]);
                            Assert.IsTrue(flagValue.HasValue, "Value field on item {0} is not a valid flag.", item.ItemID);
                            newValue += flagValue.GetValueOrDefault(0);
                        }
                    }
                    if (this.Value != newValue.ToString())
                    {
                        this.TrackModified = list.TrackModified;
                        this.SetModified();
                        this.Value = newValue.ToString();
                    }
                }
            }
            base.OnLoad(e);
        }

        protected virtual void RefreshSelectedFlagsText()
        {
            var list = FindControl(GetID("list")) as Sitecore.Shell.Applications.ContentEditor.Checklist;
            var text = FindControl(GetID("text")) as Sitecore.Shell.Applications.ContentEditor.Text;
            var selectedFlagText = new List<string>();
            foreach (ChecklistItem item in list.Items)
            {
                if (item.Checked)
                {
                    selectedFlagText.Add(item.Header);
                }
            }
            text.Value = string.Join(", ", selectedFlagText);
        }

        protected override void OnPreRender(EventArgs e)
        {
            RefreshSelectedFlagsText();
            base.OnPreRender(e);
        }

        public override void HandleMessage(Sitecore.Web.UI.Sheer.Message message)
        {
            if (message["id"] == this.ID)
            {
                var list = FindControl(GetID("list")) as Sitecore.Shell.Applications.ContentEditor.Checklist;
                var text = FindControl(GetID("text")) as Sitecore.Shell.Applications.ContentEditor.Text;
                if (list != null)
                {
                    switch (message.Name)
                    {
                        case "flags:checkall":
                            list.CheckAll();
                            break;
                        case "flags:uncheckall":
                            list.UncheckAll();
                            break;
                        case "flags:invert":
                            list.Invert();
                            break;
                    }
                }
            }
            base.HandleMessage(message);
        }
    }
}
