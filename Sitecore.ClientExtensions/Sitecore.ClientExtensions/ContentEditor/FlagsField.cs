using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.ClientExtensions.ContentEditor
{
    public class FlagsField : Sitecore.Web.UI.HtmlControls.Input
    {
        protected virtual void SetSelectedItemsOnChecklist(Sitecore.Shell.Applications.ContentEditor.Checklist list)
        {
            foreach (var item in list.Items)
            {
                item.Checked = IsFlagSet(this.Value, item.Value);
            }
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

        protected virtual void SetChecklistSource(Sitecore.Shell.Applications.ContentEditor.Checklist list)
        {
            var days = this.Source.Split(',').Select(value => value.Split('='));
            if (days.Any(s => s.Length != 2))
            {
                return;
            }
            foreach (var day in days)
            {
                var item = new Sitecore.Web.UI.HtmlControls.ChecklistItem();
                item.ID = (list.ID + day[1]);
                item.Header = day[0];
                item.Value = day[1];
                list.Controls.Add(item);
            }
        }

        protected virtual int GetValueFromChecklistItem(Sitecore.Web.UI.HtmlControls.ChecklistItem item)
        {
            var value = GetFlagValue(item.Value);
            Sitecore.Diagnostics.Assert.IsTrue(value.HasValue, "Item {0} has a value {1}, which is not a valid flag.", item.Header, item.Value);
            return value.GetValueOrDefault(0);
        }
        protected virtual int GetValueFromChecklist(Sitecore.Shell.Applications.ContentEditor.Checklist list)
        {
            var value = 0;
            if (list.Items.Length == 0)
            {
                int.TryParse(this.Value, out value);
            }
            else
            {
                foreach (var item in list.Items)
                {
                    if (item.Checked)
                    {
                        value += GetValueFromChecklistItem(item);
                    }
                }
            }
            return value;
        }
        protected override void OnLoad(EventArgs e)
        {
            if (!Sitecore.Context.ClientPage.IsEvent)
            {
                var list = new Sitecore.Shell.Applications.ContentEditor.Checklist();
                this.Controls.Add(list);
                list.ID = GetID("list");
                list.ItemID = this.ItemID;
                SetChecklistSource(list);
                SetSelectedItemsOnChecklist(list);
                list.TrackModified = this.TrackModified;
                list.Disabled = this.Disabled;
                //
                //Add an event handler for when the list is clicked.
                //This forces a server-side event. If the event
                //handler is not specified, no server-side event
                //will be triggered and the component will not
                //know it needs to be refreshed.
                list.Click = this.ID + ".OnListClick";  
                //
                //For usability display the selected flag values as a
                //comma-separated string above the Checklist control
                var text = new Sitecore.Shell.Applications.ContentEditor.Text();
                this.Controls.AddAt(0, text);
                text.ID = GetID("text");
                text.ReadOnly = true;
                text.Disabled = this.Disabled;
                //
                //Add a spacer image so the controls are positioned
                //in a more visually pleasing way
                //way
                this.Controls.Add(new System.Web.UI.LiteralControl(Sitecore.Resources.Images.GetSpacer(24, 16)));
            }
            else
            {
                //
                //If the value has changed then update the 
                //value on the control
                var list = FindControl(GetID("list")) as Sitecore.Shell.Applications.ContentEditor.Checklist;
                if (list != null)
                {
                    var newValue = GetValueFromChecklist(list);
                    var currentValue = 0;
                    int.TryParse(this.Value, out currentValue);
                    if (currentValue != newValue)
                    {
                        this.TrackModified = list.TrackModified;
                        this.SetModified();
                        this.Value = newValue.ToString();
                    }
                }
            }
            base.OnLoad(e);
        }
        protected override void SetModified()
        {
            base.SetModified();
            if (base.TrackModified)
            {
                Sitecore.Context.ClientPage.Modified = true;
            }
        }

        protected virtual void RefreshSelectedFlagsText()
        {
            var list = FindControl(GetID("list")) as Sitecore.Shell.Applications.ContentEditor.Checklist;
            var text = FindControl(GetID("text")) as Sitecore.Shell.Applications.ContentEditor.Text;
            var selectedFlagText = new List<string>();
            foreach (var item in list.Items)
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
