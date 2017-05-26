using Opten.Umbraco.ListView.Extensions;
using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Web.Trees;

namespace ClassLibrary1
{
	public class TreeEventHandler : ApplicationEventHandler
	{
		

		protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
		{
			ContentTreeController.TreeNodesRendering += ContentTreeController_TreeNodesRendering;
		}

		private void ContentTreeController_TreeNodesRendering(TreeControllerBase sender, TreeNodesRenderingEventArgs e)
		{
			if (e.QueryStrings["application"].Equals("content") &&
				e.QueryStrings["isDialog"].Equals("false") &&
				string.IsNullOrWhiteSpace(e.QueryStrings["id"]) == false)
			{
				var content = sender.Services.ContentService.GetById(int.Parse(e.QueryStrings["id"]));

				e.Nodes.RemoveAll(treeNode => treeNode.AdditionalData.ContainsKey("contentType") && content.IsInListView(treeNode.AdditionalData["contentType"].ToString()));
			}
		}
	}
}
