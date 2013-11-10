using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Text;
using Sitecore.Shell.Applications.WebEdit;

namespace Sitecore.ClientExtensions.Pipelines.Renderings.SaveRenderingPropertiesFieldEditor
{
    public class SetValues
    {
        public virtual void Process(RenderingPropertiesFieldEditorArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.ArgumentNotNull(args.StandardValuesItem, "args.StandardValuesItem");
            Assert.ArgumentNotNullOrEmpty(args.Handle, "args.Handle");
            Assert.ArgumentNotNull(args.RenderingDefinition, "args.RenderingDefinition");

            var renderingDef = args.RenderingDefinition;
            var fieldCol = args.StandardValuesItem.Fields;

            var parameters = new UrlString();
            var options = RenderingParametersFieldEditorOptions.Parse(args.Handle);
            
            //
            //save the parameters that have already been set
            if (! string.IsNullOrEmpty(args.RenderingDefinition.Parameters))
            {
                var currentParams = new UrlString(args.RenderingDefinition.Parameters);
                foreach (var key in currentParams.Parameters.AllKeys)
                {
                    var name = key;
                    var value = currentParams[key];
                    SetValue(renderingDef, parameters, name, value);
                }
            }


            foreach (var field in options.Fields)
            {
                SetValue(renderingDef, parameters, fieldCol[field.FieldID].Name, field.Value);
            }
            args.RenderingDefinition.Parameters = parameters.ToString();
        }

        protected virtual void SetValue(RenderingDefinition renderingDef, UrlString parameters, string fieldName, string value)
        {
            Assert.ArgumentNotNull(renderingDef, "renderingDef");
            Assert.ArgumentNotNull(fieldName, "fieldName");
            Assert.ArgumentNotNull(value, "value");
            Assert.ArgumentNotNull(parameters, "parameters");

            var name = fieldName.ToLowerInvariant();
            switch (name)
            {
                case "placeholder":
                    renderingDef.Placeholder = value;
                    return;
                case "data source":
                    renderingDef.Datasource = value;
                    return;
                case "caching":
                    SetCaching(renderingDef, value);
                    return;
                case "personalization":
                    renderingDef.Conditions = value;
                    return;
                case "tests":
                    if (string.IsNullOrEmpty(value))
                    {
                        renderingDef.MultiVariateTest = string.Empty;
                    }

                    var item = Sitecore.Client.ContentDatabase.GetItem(value);
                    if (item != null)
                    {
                        renderingDef.MultiVariateTest = item.ID.ToString();
                        return;
                    }

                    renderingDef.MultiVariateTest = value;
                    return;
            }

            if (name == "additional parameters")
            {
                var additionalParameters = new UrlString(value);
                parameters.Append(additionalParameters);
                return;
            }

            parameters[fieldName] = value;
        }
        protected virtual void SetCaching(RenderingDefinition renderingDef, string value)
        {
            Assert.ArgumentNotNull(renderingDef, "renderingDef");
            Assert.ArgumentNotNull(value, "value");

            if (string.IsNullOrEmpty(value))
            {
                value = "0|0|0|0|0|0|0|0";
            }

            var parts = value.Split('|');
            Assert.IsTrue(parts.Length == 8, "Invalid caching value format");

            renderingDef.Cachable = parts[0] == "1" ? "1" : null;
            renderingDef.ClearOnIndexUpdate = parts[1] == "1" ? "1" : null;
            renderingDef.VaryByData = parts[2] == "1" ? "1" : null;
            renderingDef.VaryByDevice = parts[3] == "1" ? "1" : null;
            renderingDef.VaryByLogin = parts[4] == "1" ? "1" : null;
            renderingDef.VaryByParameters = parts[5] == "1" ? "1" : null;
            renderingDef.VaryByQueryString = parts[6] == "1" ? "1" : null;
            renderingDef.VaryByUser = parts[7] == "1" ? "1" : null;
        }
    }
}