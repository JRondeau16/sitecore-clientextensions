using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Sitecore.Data.Items;
using Sitecore.Layouts;
using Sitecore.Pipelines;

namespace Sitecore.ClientExtensions.Pipelines.Renderings
{
    public class RenderingPropertiesFieldEditorArgs : PipelineArgs
    {
        public string HandleName { get; set; }
        public string DeviceId { get; set; }
        public int SelectedIndex { get; set; }
        public string Handle { get; set; }
        public LayoutDefinition LayoutDefinition { get; set; }
        public RenderingDefinition RenderingDefinition { get; set; }
        public Item StandardValuesItem { get; set; }
        public NameValueCollection ClientParameters { get; set; }
    }
}