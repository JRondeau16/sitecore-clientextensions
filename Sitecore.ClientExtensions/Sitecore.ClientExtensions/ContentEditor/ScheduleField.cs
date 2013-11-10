using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Tasks;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.ClientExtensions.ContentEditor
{
    public class ScheduleField : Input
    {
        protected Panel DescriptionPanel;
        public string Source { get; set; }

        protected override void OnInit(EventArgs e)
        {
            this.DescriptionPanel = new Panel();
            FormatPanel(this.DescriptionPanel);
            this.Controls.Add(this.DescriptionPanel);
            this.DescriptionPanel.ID = GetID("DescriptionPanel");
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            RefreshDescriptionPanel(); 
            base.OnLoad(e);
        }

        protected virtual void FormatPanel(Panel panel)
        {
            panel.Background = "white";
            panel.Border = "1px solid #cccccc";
            panel.Padding = "8px 4px 8px 4px";
        }

        protected virtual void RefreshDescriptionPanel()
        {
            var builder = new StringBuilder("");
            builder.Append("<div style=\"height:80px;\">");
            if (string.IsNullOrEmpty(this.Value))
            {
                builder.Append("No schedule has been configured.");
            }
            else
            {
                var rec = new Recurrence(this.Value);
                builder.Append("<table>");
                AddRow("Start", rec.StartDate.ToString("G", Sitecore.Context.Culture), builder);
                AddRow("End", rec.EndDate.ToString("G", Sitecore.Context.Culture), builder);

                var dayNamesArray = rec.Days.ToString().Split(',').Select(s => Translate.Text(s.Trim())).ToArray();
                var dayNames = string.Join(", ", dayNamesArray);
                AddRow("Days", dayNames, builder);

                AddRow("Interval", rec.Interval.ToString("g", Sitecore.Context.Culture), builder);

                builder.Append("</table>");
            }
            builder.Append("</div>");
            this.DescriptionPanel.InnerHtml = builder.ToString();
        }

        protected virtual void AddRow(string description, string value, StringBuilder builder)
        {
            builder.AppendFormat("<tr><td style=\"text-align:right; font-weight:bold;\">{0}:</td><td>{1}</td></tr>", Translate.Text(description), Translate.Text(value));
        }

        public override void HandleMessage(Message message)
        {
            if (message["id"] == this.ID)
            {
                switch (message.Name)
                {
                    case "schedule:edit":
                        Sitecore.Context.ClientPage.Start(this, "ShowEditSchedule");
                        break;
                }
            }
            base.HandleMessage(message);
        }

        protected virtual void ShowEditSchedule(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (args.IsPostBack)
            {
                if (args.HasResult)
                {
                    if (! string.Equals(args.Result, this.Value, StringComparison.OrdinalIgnoreCase))
                    {
                        this.Value = args.Result;
                        this.SetModified();
                    }
                }
            }
            else
            {
                var urlString = new UrlString(Sitecore.UIUtil.GetUri("control:ScheduleEditor"));
                var handle = new UrlHandle();
                handle["value"] = this.Value;
                handle.Add(urlString);
                SheerResponse.ShowModalDialog(urlString.ToString(), "420px", "500px", string.Empty, true);
                args.WaitForPostBack();
            }
        }

        protected override void SetModified()
        {
            base.SetModified();
            if (base.TrackModified)
            {
                Sitecore.Context.ClientPage.Modified = true;
                RefreshDescriptionPanel(); 
            }
        }

    }
}
