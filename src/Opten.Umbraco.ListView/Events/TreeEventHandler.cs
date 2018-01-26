using AutoMapper;
using Opten.Umbraco.ListView.Extensions;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Trees;

namespace Opten.Umbraco.ListView
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

				if (!destTyped.IsChildOfListView)
				{
					var parent = srcTyped.Parent();
					if (parent != null)
					{
						destTyped.IsChildOfListView = parent.IsInListView(srcTyped.ContentType.Alias);
					}
				}
				
			});
		}

		private void ContentTreeController_TreeNodesRendering(TreeControllerBase sender, TreeNodesRenderingEventArgs e)
		{
			if (e.QueryStrings.Contains(new System.Collections.Generic.KeyValuePair<string, string>("application", "content")) &&
				e.QueryStrings["isDialog"].Equals("false") &&
				string.IsNullOrWhiteSpace(e.QueryStrings["id"]) == false &&
				e.QueryStrings["id"] != "-1")
			{
				var content = sender.Services.ContentService.GetById(int.Parse(e.QueryStrings["id"]));

				if (content != null)
				{
					for (var i = e.Nodes.Count - 1; i > -1; i--)
					{
						var treeNode = e.Nodes[i];

						if (treeNode.AdditionalData.ContainsKey("contentType"))
						{
							if (content.IsInListView(treeNode.AdditionalData["contentType"].ToString()))
							{
								e.Nodes.RemoveAt(i);
								continue;
							}

							if (treeNode.HasChildren)
							{
								var node = sender.Services.ContentService.GetById(int.Parse(treeNode.Id.ToString()));
								treeNode.HasChildren = node.TreeChildren().Any();
							}
						}
					}
				}
			}
		}
	}
}
