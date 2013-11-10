using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Text;

namespace Sitecore.ClientExtensions.Rules.RuleMacros
{
    public class DateOnlyMacro : DateTimeMacroEx
    {
        protected override UrlString GetDialogUrl(string value)
        {
            var url = base.GetDialogUrl(value);
            url["time"] = "false";
            return url;
        }

        protected override string GetWidth()
        {
            return "300px";
        }
    }
}