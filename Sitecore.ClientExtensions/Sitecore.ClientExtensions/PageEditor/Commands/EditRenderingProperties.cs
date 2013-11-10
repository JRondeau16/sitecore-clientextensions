using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Layouts;
using Sitecore.Pipelines;
using Sitecore.Shell.Applications.WebEdit;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;
using Sitecore.ClientExtensions.Pipelines.Renderings;
using Sitecore.ClientExtensions.Pipelines.Renderings.GetRenderingPropertiesFieldEditor;

namespace Sitecore.ClientExtensions.PageEditor.Commands
{
    public class EditRenderingProperties : Command
    {
        public EditRenderingProperties()
        {
            this.HandleName = "PageDesigner";
        }
        protected string HandleName { get; set; }

        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");

            var layoutAsJson = WebUtil.GetFormValue("scLayout");
            var deviceId = ShortID.Decode(WebUtil.GetFormValue("scDeviceID"));
            var uniqueId = context.Parameters["referenceId"];


            var layoutAsXml = Sitecore.Web.WebEditUtil.ConvertJSONLayoutToXML(layoutAsJson);
            Assert.IsNotNull(layoutAsXml, "layoutAsXml");
            WebUtil.SetSessionValue(this.HandleName, layoutAsXml);

            var layoutDef = LayoutDefinition.Parse(layoutAsXml);
            var deviceDefinition = layoutDef.GetDevice(deviceId);
            var index = deviceDefinition.GetIndex(uniqueId);

            var parameters = new NameValueCollection();
            parameters["handleName"] = HandleName;
            parameters["deviceId"] = deviceId;
            parameters["selectedindex"] = index.ToString();
            parameters.Add(context.Parameters);

            var args = new ClientPipelineArgs(parameters);
            Sitecore.Context.ClientPage.Start(this, "Run", args);
        }
        protected virtual void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");

            var selectedIndex = Sitecore.MainUtil.GetInt(args.Parameters["selectedindex"], -1);
            if (selectedIndex < 0)
            {
                return;
            }

            var item = Sitecore.Web.WebEditUtil.GetClientContentItem(Sitecore.Client.ContentDatabase);
            if (ShowDialog(args, selectedIndex, item))
            {
                if (args.HasResult)
                {
                    HandleResult(args);
                }
                else
                {
                    SheerResponse.SetAttribute("scLayoutDefinition", "value", string.Empty);
                }
                WebUtil.RemoveSessionValue(args.Parameters["handleName"]);
            }
        }

        protected virtual void HandleResult(ClientPipelineArgs args)
        {
            var layoutAsXml = WebUtil.GetSessionString(args.Parameters["handleName"]);
            var layoutAsJson = Sitecore.Web.WebEditUtil.ConvertXMLLayoutToJSON(layoutAsXml);
            SheerResponse.SetAttribute("scLayoutDefinition", "value", layoutAsJson);
            SheerResponse.Eval("window.parent.Sitecore.PageModes.ChromeManager.handleMessage('chrome:rendering:propertiescompleted');");
        }

        protected virtual void Save(string handleName, string deviceId, NameValueCollection clientParameters, int selectedIndex, string handle)
        {
            var args2 = new RenderingPropertiesFieldEditorArgs()
            {
                ClientParameters = clientParameters,
                HandleName = handleName,
                DeviceId = deviceId,
                SelectedIndex = selectedIndex,
                Handle = handle
            };
            CorePipeline.Run("saveRenderingPropertiesFieldEditor", args2);
            var layoutDef = args2.LayoutDefinition;
            WebUtil.SetSessionValue(args2.HandleName, layoutDef.ToXml());
        }
        protected virtual string GetDialogTitle(ClientPipelineArgs args)
        {
            var title = args.Parameters["title"];
            if (string.IsNullOrEmpty(title))
            {
                title = Sitecore.Texts.ControlProperties;
            }
            return Translate.Text(title);
        }
        protected virtual bool ShowDialog(ClientPipelineArgs args, int selectedIndex, Item item)
        {
            var handleNameParam = args.Parameters["handleName"];
            var deviceIdParam = args.Parameters["deviceId"];

            if (args.IsPostBack)
            {
                if (args.HasResult)
                {
                    Save(handleNameParam, deviceIdParam, args.Parameters, selectedIndex, args.Result);
                }
                return true;
            }

            if (selectedIndex < 0)
            {
                return true;
            }
            var args2 = new GetRenderingPropertiesArgs()
            {
                ClientParameters = args.Parameters,
                HandleName = handleNameParam,
                DeviceId = deviceIdParam,
                SelectedIndex = selectedIndex,
            };
            CorePipeline.Run("getRenderingPropertiesFieldEditor", args2);
            var fields = args2.FieldDescriptors;
            var dialogTitle = GetDialogTitle(args);

            var options = new RenderingParametersFieldEditorOptions(fields)
            {
                DialogTitle = dialogTitle,
                HandleName = handleNameParam,
                PreserveSections = true
            };

            this.SetCustomParameters(args2.RenderingDefinition, options, item);

            var url = options.ToUrlString();
            SheerResponse.ShowModalDialog(url.ToString(), "720", "480", string.Empty, true);

            args.WaitForPostBack();
            return false;
        }
        protected virtual void SetCustomParameters(RenderingDefinition renderingDefinition, RenderingParametersFieldEditorOptions options, Item item)
        {
            Assert.ArgumentNotNull(renderingDefinition, "renderingDefinition");
            Assert.ArgumentNotNull(options, "options");

            var renderingItem = renderingDefinition.ItemID != null ? Sitecore.Client.ContentDatabase.GetItem(renderingDefinition.ItemID) : null;
            if (renderingItem != null)
            {
                options.Parameters["rendering"] = renderingItem.Uri.ToString();
            }

            if (item != null)
            {
                options.Parameters["contentitem"] = item.Uri.ToString();
            }

            if (Sitecore.Web.WebEditUtil.IsRenderingPersonalized(renderingDefinition))
            {
                options.Parameters["warningtext"] = Sitecore.Texts.PersonalizationConditionsDefinedWarning;
            }

            if (!string.IsNullOrEmpty(renderingDefinition.MultiVariateTest))
            {
                options.Parameters["warningtext"] = Sitecore.Texts.Thereisamultivariatetestsetupforthiscontro;
            }
        }

    }
}
