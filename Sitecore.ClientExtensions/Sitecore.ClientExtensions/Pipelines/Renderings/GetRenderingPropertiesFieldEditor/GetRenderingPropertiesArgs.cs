using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data;

namespace Sitecore.ClientExtensions.Pipelines.Renderings.GetRenderingPropertiesFieldEditor
{
    public class GetRenderingPropertiesArgs : RenderingPropertiesFieldEditorArgs
    {
        public List<FieldDescriptor> FieldDescriptors { get; set; }
        public Dictionary<string, string> RenderingParameters { get; set; }
    }
}