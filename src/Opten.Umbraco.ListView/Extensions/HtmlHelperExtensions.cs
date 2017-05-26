using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace Opten.Umbraco.ListView.Extensions
{
	public static class HtmlHelperExtensions
	{

		public static void RenderPublishedContentPartial(this HtmlHelper htmlHelper, IPublishedContent content)
		{
			htmlHelper.RenderPartial(GetTemplate(content).VirtualPath, content);

		}
		public static MvcHtmlString PublishedContentPartial(this HtmlHelper htmlHelper, IPublishedContent content)
		{
			return htmlHelper.Partial(GetTemplate(content).VirtualPath, content);
		}

		private static ITemplate GetTemplate(IPublishedContent content)
		{
			return ApplicationContext.Current.Services.FileService.GetTemplate(content.TemplateId);
		}
	}
}
