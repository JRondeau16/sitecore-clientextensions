using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.SecurityModel;
using Sitecore.Web;

namespace Sitecore.ClientExtensions.Pipelines.Renderings
{
    public class Initialize
    {
        public virtual void Process(RenderingPropertiesFieldEditorArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.ArgumentNotNull(args.SelectedIndex, "args.SelectedIndex");
            Assert.ArgumentNotNull(args.ClientParameters, "args.ClientParameters");

            if (string.IsNullOrEmpty(args.HandleName))
            {
                args.HandleName = args.ClientParameters["handleName"];
            }
            if (string.IsNullOrEmpty(args.DeviceId))
            {
                args.DeviceId = args.ClientParameters["deviceId"];
            }

            if (args.LayoutDefinition == null)
            {
                args.LayoutDefinition = GetLayoutDefinition(args);
            }
            args.RenderingDefinition = GetRenderingDefinition(args);
            args.StandardValuesItem = GetStandardValuesItem(args);
        }
        protected virtual RenderingDefinition GetRenderingDefinition(RenderingPropertiesFieldEditorArgs args)
        {
            var deviceDef = args.LayoutDefinition.GetDevice(args.DeviceId);
            var renderings = deviceDef.Renderings;
            if (renderings == null)
            {
                return null;
            }
            return renderings[Sitecore.MainUtil.GetInt(args.SelectedIndex, 0)] as RenderingDefinition;
        }
        protected virtual LayoutDefinition GetLayoutDefinition(RenderingPropertiesFieldEditorArgs args)
        {
            var sessionValue = WebUtil.GetSessionString(args.HandleName);
            Assert.IsNotNull(sessionValue, "sessionValue");
            return LayoutDefinition.Parse(sessionValue);
        }

        protected virtual Item GetStandardValuesItem(RenderingPropertiesFieldEditorArgs args)
        {
            using (new SecurityDisabler())
            {
                var itemId = args.RenderingDefinition.ItemID;
                if (string.IsNullOrEmpty(itemId))
                {
                    return null;
                }
                var item = Sitecore.Client.ContentDatabase.GetItem(itemId);
                if (item == null)
                {
                    return null;
                }
                return RenderingItem.GetStandardValuesItemFromParametersTemplate(item);
            }
        }
    }
}