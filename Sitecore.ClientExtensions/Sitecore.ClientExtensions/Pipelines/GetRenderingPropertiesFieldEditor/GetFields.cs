using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Sitecore.Collections;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Shell;
using Sitecore.Text;
using Sitecore.Data;
using Sitecore.Layouts;

namespace Sitecore.ClientExtensions.Pipelines.GetRenderingPropertiesFieldEditor
{
    public class GetFields
    {
        public virtual void Process(GetRenderingPropertiesArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.ArgumentNotNull(args.RenderingDefinition, "args.RenderingDefinition");
            Assert.ArgumentNotNull(args.StandardValuesItem, "args.StandardValuesItem");
            args.RenderingParameters = GetRenderingParameters(args);
            args.FieldDescriptors = GetFieldDescriptors(args);
        }

        protected virtual Dictionary<string, string> GetRenderingParameters(GetRenderingPropertiesArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.ArgumentNotNull(args.RenderingDefinition, "args.RenderingDefinition");
            var parameters = new Dictionary<string, string>();
            var parameterCol = Sitecore.Web.WebUtil.ParseUrlParameters(args.RenderingDefinition.Parameters ?? string.Empty);
            foreach (string key in parameterCol.Keys)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    parameters[key] = parameterCol[key];
                }
            }
            return parameters;
        }

        protected virtual List<FieldDescriptor> GetFieldDescriptors(GetRenderingPropertiesArgs args)
        {
            var fieldDescriptors = new List<FieldDescriptor>();
            var additionalParameters = new Dictionary<string, string>(args.RenderingParameters);

            var fieldCol = args.StandardValuesItem.Fields;
            fieldCol.ReadAll();
            fieldCol.Sort();

            var includedFields = GetIncludedFields(fieldCol, args);
            if (includedFields == null || includedFields.Count == 0)
            {
                return fieldDescriptors;
            }

            foreach (Field field in fieldCol)
            {
                if (includedFields.Contains(field))
                {
                    var fieldDescriptor = GetFieldDescriptor(field, args);
                    if (fieldDescriptor != null)
                    {
                        fieldDescriptors.Add(fieldDescriptor);
                    }
                }
                additionalParameters.Remove(field.Name);
            }
            var additionalParametersField = includedFields.FirstOrDefault(f => f.Name.Equals("Additional Parameters", StringComparison.OrdinalIgnoreCase));
            if (additionalParametersField != null)
            {
                var additionalParametersDescriptor = GetAdditionalParameters(args.StandardValuesItem, additionalParameters);
                if (additionalParametersDescriptor != null)
                {
                    fieldDescriptors.Add(additionalParametersDescriptor);
                }
            }
            return fieldDescriptors;
        }
        
        protected virtual List<Field> GetIncludedFields(FieldCollection fieldCol, GetRenderingPropertiesArgs args)
        {
            var includedFields = new List<Field>();
            var parameters = args.ClientParameters;
            if (parameters == null || parameters.Count == 0)
            {
                return includedFields;
            }

            AddFieldsIncludedByName(fieldCol, includedFields, args);
            AddFieldsIncludedBySection(fieldCol, includedFields, args);
            return includedFields;
        }
        protected virtual void AddFieldsIncludedByName(FieldCollection fieldCol, List<Field> includedFields, GetRenderingPropertiesArgs args)
        {
            var fieldsParam = args.ClientParameters["fields"];
            if (string.IsNullOrEmpty(fieldsParam))
            {
                return;
            }

            var fieldNames = fieldsParam.Split(new[] { '|' }).Where(name => !string.IsNullOrWhiteSpace(name)).ToList();
            var fields = fieldCol.Where(f => fieldNames.Contains(f.Name));
            includedFields.AddRange(fields);
        }
        protected virtual void AddFieldsIncludedBySection(FieldCollection fieldCol, List<Field> includedFields, GetRenderingPropertiesArgs args)
        {
            var sectionsParam = args.ClientParameters["sections"];
            if (string.IsNullOrEmpty(sectionsParam))
            {
                return;
            }

            var sectionNames = sectionsParam.Split(new[] { '|' }).Where(name => !string.IsNullOrWhiteSpace(name)).ToList();
            var fields = fieldCol.Where(f => sectionNames.Contains(f.Section));
            includedFields.AddRange(fields);
        }

        protected virtual FieldDescriptor GetFieldDescriptor(Field field, GetRenderingPropertiesArgs args)
        {
            if (field.Name == "Additional Parameters")
            {
                return null;
            }
            if (field.Name == "Personalization" && !UserOptions.View.ShowPersonalizationSection)
            {
                return null;
            }
            if (field.Name == "Tests" && !UserOptions.View.ShowTestLabSection)
            {
                return null;
            }
            if (!RenderingItem.IsAvalableNotBlobNotSystemField(field))
            {
                return null;
            }

            var value = GetValue(field.Name, args);

            var fieldDescriptor = new FieldDescriptor(args.StandardValuesItem, field.Name)
            {
                Value = value ?? field.Value,
                ContainsStandardValue = (value == null) ? true : false,
            };

            return fieldDescriptor;
        }

        protected virtual string GetCaching(RenderingDefinition renderingDefinition)
        {
            Assert.ArgumentNotNull(renderingDefinition, "renderingDefinition");

            return (renderingDefinition.Cachable == "1" ? "1" : "0") + "|" +
                   (renderingDefinition.ClearOnIndexUpdate == "1" ? "1" : "0") + "|" +
                   (renderingDefinition.VaryByData == "1" ? "1" : "0") + "|" +
                   (renderingDefinition.VaryByDevice == "1" ? "1" : "0") + "|" +
                   (renderingDefinition.VaryByLogin == "1" ? "1" : "0") + "|" +
                   (renderingDefinition.VaryByParameters == "1" ? "1" : "0") + "|" +
                   (renderingDefinition.VaryByQueryString == "1" ? "1" : "0") + "|" +
                   (renderingDefinition.VaryByUser == "1" ? "1" : "0");
        }

        protected virtual string GetValue(string fieldName, GetRenderingPropertiesArgs args)
        {
            Assert.ArgumentNotNull(fieldName, "fieldName");
            Assert.ArgumentNotNull(args, "args");
            Assert.ArgumentNotNull(args.RenderingDefinition, "args.RenderingDefinition");
            Assert.ArgumentNotNull(args.RenderingParameters, "args.RenderingParameters");
            
            var renderingDef = args.RenderingDefinition;
            switch (fieldName.ToLowerInvariant())
            {
                case "placeholder":
                    return renderingDef.Placeholder ?? string.Empty;
                case "data source":
                    return renderingDef.Datasource ?? string.Empty;
                case "caching":
                    return GetCaching(renderingDef);
                case "personalization":
                    return renderingDef.Conditions ?? string.Empty;
                case "tests":
                    return renderingDef.MultiVariateTest ?? string.Empty;
            }
            string value;
            args.RenderingParameters.TryGetValue(fieldName, out value);
            return value;
        }
        protected virtual FieldDescriptor GetAdditionalParameters(Item standardValues, Dictionary<string, string> additionalParameters)
        {
            Assert.ArgumentNotNull(standardValues, "standardValues");
            Assert.ArgumentNotNull(additionalParameters, "additionalParameters");

            var value = new UrlString();
            foreach (var key in additionalParameters.Keys)
            {
                value[key] = HttpUtility.UrlDecode(additionalParameters[key]);
            }

            var descriptor = new FieldDescriptor(standardValues, "Additional Parameters") { Value = value.ToString() };
            return descriptor;
        }

    }
}