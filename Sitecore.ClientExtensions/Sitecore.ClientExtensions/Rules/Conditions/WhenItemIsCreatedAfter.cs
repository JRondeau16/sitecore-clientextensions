using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data.Items;
using Sitecore.Rules;

namespace Sitecore.ClientExtensions.Rules.Conditions
{
    public class WhenItemIsCreatedAfter<T> : WhenItemDate<T> where T : RuleContext
    {
        protected override bool DoCompare(Item item)
        {
            var date = DateUtil.IsoDateToDateTime(this.IsoDate);
            return (DateTime.Compare(date, item.Statistics.Created) > 0);
        }
    }
}
