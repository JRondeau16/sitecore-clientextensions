using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Sitecore.Diagnostics;
using Sitecore.Rules.RuleMacros;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.ClientExtensions.RuleMacros
{
    public class DateTimeMacroEx : IRuleMacro
    {
        public virtual void Execute(XElement element, string name, UrlString parameters, string value)
        {
            Assert.ArgumentNotNull(element, "element");
            Assert.ArgumentNotNull(name, "name");
            Assert.ArgumentNotNull(parameters, "parameters");
            Assert.ArgumentNotNull(value, "value");

            var url = GetDialogUrl(value);
            SheerResponse.ShowModalDialog(url.ToString(), GetWidth(), GetHeight(), string.Empty, true);
        }

        protected virtual UrlString GetDialogUrl(string value)
        {
            var uri = UIUtil.GetUri("control:Sitecore.Shell.Applications.Dialogs.DateTimeSelectorEx");
            var url = new UrlString(uri);
            url["value"] = value;
            return url;
        }

        protected virtual string GetHeight()
        {
            return "300px";
        }

        protected virtual string GetWidth()
        {
            return "400px";
        }
    }
}