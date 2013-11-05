using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore;
using Sitecore.Diagnostics;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.XmlControls;

namespace Sitecore.ClientExtensions.Dialogs
{
    public class DateTimeSelectorEx : Sitecore.Web.UI.Pages.DialogForm
    {
        protected XmlControl Dialog;
        protected override void OnLoad(EventArgs e)
        {
            Debug.ArgumentNotNull(e, "e");
            base.OnLoad(e);
            if (!Context.ClientPage.IsEvent)
            {
                var showTime = true;
                if (!bool.TryParse(WebUtil.GetQueryString("time"), out showTime))
                {
                    showTime = true;
                }
                this.DateTimePicker.ShowTime = showTime;

                var value = WebUtil.GetQueryString("value");
                if (string.IsNullOrEmpty(value))
                {
                    value = DateUtil.IsoNow;
                }
                this.DateTimePicker.Value = value;
                if (!showTime)
                {
                    this.DateTimePicker.Time = "";
                    this.Dialog["Header"] = "Date";
                    this.Dialog["Text"] = "Select a date.";
                    this.Dialog["Icon"] = "Business/32x32/calendar.png";
                }
            }
        }

        protected override void OnOK(object sender, EventArgs args)
        {
            Debug.ArgumentNotNull(sender, "sender");
            Debug.ArgumentNotNull(args, "args");
            SheerResponse.SetDialogValue(this.DateTimePicker.Value);
            base.OnOK(sender, args);
        }

        protected DateTimePicker DateTimePicker { get; set; }
    }
}
