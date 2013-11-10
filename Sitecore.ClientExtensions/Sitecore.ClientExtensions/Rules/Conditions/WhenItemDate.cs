using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Rules;
using Sitecore.Rules.Conditions;

namespace Sitecore.ClientExtensions.Rules.Conditions
{
    public abstract class WhenItemDate<T> : WhenCondition<T> where T : RuleContext
    {
        protected override bool Execute(T ruleContext)
        {
            Assert.ArgumentNotNull(ruleContext, "ruleContext");
            var item = ruleContext.Item;
            if (item == null)
            {
                return false;
            }
            return DoCompare(item);
        }

        protected abstract bool DoCompare(Item item);
        public string IsoDate { get; set; }
    }

}
