using AutoMapper;
using Opten.Umbraco.ListView.Extensions;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Trees;

namespace ClassLibrary1
{
	public class TreeEventHandler : ApplicationEventHandler
	{
		

		protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
		{
			ContentTreeController.TreeNodesRendering += ContentTreeController_TreeNodesRendering;

			var typeMaps = Mapper.GetAllTypeMaps();

			var contentMapper = typeMaps.First(map => map.DestinationType.Equals(typeof(ContentItemDisplay)) && map.SourceType.Equals(typeof(IContent)));

			contentMapper.AddAfterMapAction((src, dest) =>
			{
				var srcTyped = src as IContent;
				var destTyped = dest as ContentItemDisplay; 

				destTyped.IsContainer = destTyped.IsContainer || srcTyped.FindGridListViewContentTypeAliases().Any();

				var parent = srcTyped.Parent();
				if (parent != null)
				{
					destTyped.IsChildOfListView = destTyped.IsChildOfListView || parent.IsInListView(srcTyped.ContentType.Alias);
				}
				
			});
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
