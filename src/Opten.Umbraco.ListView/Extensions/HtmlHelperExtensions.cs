using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Umbraco.Core.Models;

namespace Opten.Umbraco.ListView.Extensions
{
	public static class HtmlHelperExtensions
	{
		public static void RenderTemplate(this HtmlHelper htmlHelper, IPublishedContent content)
		{
			htmlHelper.RenderPartial(content.GetTemplate().VirtualPath, content);
		}

		public static MvcHtmlString Template(this HtmlHelper htmlHelper, IPublishedContent content)
		{
			return htmlHelper.Partial(content.GetTemplate().VirtualPath, content);
		}
	}
}
