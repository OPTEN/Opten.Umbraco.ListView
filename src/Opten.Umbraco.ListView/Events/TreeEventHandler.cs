using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Web.Trees;

namespace ClassLibrary1
{
	public class TreeEventHandler : ApplicationEventHandler
	{
		private const string ListViewShowAllKey = "all";
		private const string contentTypeAliasesPreValueKey = "contentTypeAliases";
		private const string ListViewEditorAlias = "OPTEN.ListView";

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
				var gridListViews = sender.Services.DataTypeService.GetDataTypeDefinitionByPropertyEditorAlias(ListViewEditorAlias);

				if (gridListViews != null && gridListViews.Any())
				{
					var content = sender.Services.ContentService.GetById(int.Parse(e.QueryStrings["id"]));

					var listViewProperty = content.Properties.FirstOrDefault(p =>
					{
						return gridListViews.Any(glv => glv.Id == p.PropertyType.DataTypeDefinitionId);
					});

					if (listViewProperty != null)
					{
						var listViewPropertyType = gridListViews.First(glv => glv.Id == listViewProperty.PropertyType.DataTypeDefinitionId);

						var preValues = sender.Services.DataTypeService.GetPreValuesCollectionByDataTypeId(listViewPropertyType.Id);

						var contentTypeAliases = preValues.PreValuesAsDictionary[contentTypeAliasesPreValueKey].Value.Split(',');

						e.Nodes.RemoveAll(treeNode => treeNode.AdditionalData.ContainsKey("contentType") &&
							(contentTypeAliases.Contains(ListViewShowAllKey, StringComparer.OrdinalIgnoreCase) ||
								contentTypeAliases.Contains(treeNode.AdditionalData["contentType"].ToString(), StringComparer.OrdinalIgnoreCase)));
					}
				}
			}
		}
	}
}
