using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore;
using Sitecore.Diagnostics;
using Sitecore.Tasks;
using Sitecore.Web;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.ClientExtensions.Applications.Dialogs.Schedule
{
    public class ScheduleEditorForm : DialogForm
    {
        protected DateTimePicker StartDate;
        protected DateTimePicker EndDate;
        protected Checklist Days;
        protected Edit IntervalDays;
        protected Edit IntervalHours;
        protected Edit IntervalMinutes;
        protected Edit IntervalSeconds;
        protected Edit IntervalMilliseconds;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (! Sitecore.Context.ClientPage.IsEvent)
            {
                foreach (var day in Enum.GetValues(typeof (DaysOfWeek)).Cast<DaysOfWeek>())
                {
                    if (day != DaysOfWeek.None)
                    {
                        var item = new ChecklistItem();
                        item.Header = day.ToString();
                        item.Value = day.ToString();
                        this.Days.Controls.Add(item);
                    }
                }
                var handle = UrlHandle.Get();
                SetControlValues(handle["value"]);
            }
        }

        protected virtual void SetControlValues(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                this.StartDate.Value = string.Empty;
                this.EndDate.Value = string.Empty;
                this.Days.UncheckAll();
                this.IntervalDays.Value = string.Empty;
                this.IntervalHours.Value = string.Empty;
                this.IntervalMinutes.Value = string.Empty;
                this.IntervalSeconds.Value = string.Empty;
                this.IntervalMilliseconds.Value = string.Empty;
            }
            else
            {
                var rec = new Recurrence(value);
                this.StartDate.Value = DateUtil.ToIsoDate(rec.StartDate);
                this.EndDate.Value = DateUtil.ToIsoDate(rec.EndDate);
                SelectDays(rec.Days);
                this.IntervalDays.Value = rec.Interval.Days.ToString();
                this.IntervalHours.Value = rec.Interval.Hours.ToString();
                this.IntervalMinutes.Value = rec.Interval.Minutes.ToString();
                this.IntervalSeconds.Value = rec.Interval.Seconds.ToString();
                this.IntervalMilliseconds.Value = rec.Interval.Milliseconds.ToString();
            }
        }

        protected virtual void SelectDays(DaysOfWeek days)
        {
            if (days == DaysOfWeek.None)
            {
                this.Days.UncheckAll();
                return;
            }
            foreach (var item in this.Days.Items)
            {
                var isSelected = false;
                if (Enum.IsDefined(typeof (DaysOfWeek), item.Value))
                {
                    var itemValue = (DaysOfWeek)Enum.Parse(typeof (DaysOfWeek), item.Value);
                    isSelected = ((days & itemValue) == itemValue);
                }
                item.Checked = isSelected;
            }
        }

        protected virtual string GetControlValues()
        {
            var values = new string[4];
            values[0] = this.StartDate.Value;
            values[1] = this.EndDate.Value;
            values[2] = ((int)GetDaysOfWeekFromControls()).ToString();
            values[3] = GetTimeSpanFromControls().ToString("c");
            return string.Join("|", values);
        }

        protected virtual string GetControlValue(Control control, string defaultValue)
        {
            if (control != null &&  ! string.IsNullOrEmpty(control.Value))
            {
                return control.Value;
            }
            return defaultValue;
        }
        protected virtual DaysOfWeek GetDaysOfWeekFromControls()
        {
            var days = DaysOfWeek.None;
            if (this.Days != null && this.Days.Items != null)
            {
                foreach (var item in this.Days.Items)
                {
                    if (item.Checked)
                    {
                        if (Enum.IsDefined(typeof (DaysOfWeek), item.Value))
                        {
                            days = (days | (DaysOfWeek)Enum.Parse(typeof (DaysOfWeek), item.Value));
                        }
                    }
                }
            }
            return days;
        }

        protected virtual TimeSpan GetTimeSpanFromControls()
        {
            var days = GetControlValueAsInt(this.IntervalDays, 0);
            var hours = GetControlValueAsInt(this.IntervalHours, 0);
            var minutes = GetControlValueAsInt(this.IntervalMinutes, 0);
            var seconds = GetControlValueAsInt(this.IntervalSeconds, 0);
            var ms = GetControlValueAsInt(this.IntervalMilliseconds, 0);
            return new TimeSpan(days, hours, minutes, seconds, ms);
        }

        protected virtual int GetControlValueAsInt(Control control, int defaultValue)
        {
            if (control != null)
            {
                int.TryParse(control.Value, out defaultValue);
            }
            return defaultValue;
        }

        protected override void OnOK(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            var value = GetControlValues();
            SheerResponse.SetDialogValue(value);
            base.OnOK(sender, args);
        }

        protected virtual void OnClear()
        {
            var args = Sitecore.Context.ClientPage.CurrentPipelineArgs as ClientPipelineArgs;
            Assert.IsNotNull(args, typeof(ClientPipelineArgs));
            if (args.IsPostBack)
            {
                if (args.Result == "yes")
                {
                    SetControlValues(null);
                }
            }
            else
            {
                SheerResponse.Confirm("Are you sure you want to clear all of the settings?");
                args.WaitForPostBack();
            }
        }
    }
}
