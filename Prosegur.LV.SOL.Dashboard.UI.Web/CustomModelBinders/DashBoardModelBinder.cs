using System;
using System.Web.Mvc;

namespace Prosegur.LV.SOL.Dashboard.UI.Web.ModelBinders
{
    public class DashBoardModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var prefixValue = bindingContext.ValueProvider.GetValue("__prefix");
            if (prefixValue != null)
            {
                var prefix = (String)prefixValue.ConvertTo(typeof(String));
                if (!String.IsNullOrEmpty(prefix) && !bindingContext.ModelName.StartsWith(prefix))
                {
                    if (String.IsNullOrEmpty(bindingContext.ModelName))
                    {
                        bindingContext.ModelName = prefix;
                    }
                    else
                    {
                        bindingContext.ModelName = prefix + "." + bindingContext.ModelName;

                        // fall back
                        if (bindingContext.FallbackToEmptyPrefix &&
                            !bindingContext.ValueProvider.ContainsPrefix(bindingContext.ModelName))
                        {
                            bindingContext.ModelName = prefix;
                        }
                    }
                }
            }

            return base.BindModel(controllerContext, bindingContext);
        }
    }
}