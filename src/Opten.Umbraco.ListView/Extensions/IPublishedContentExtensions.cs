using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace Opten.Umbraco.ListView.Extensions
{
	public static class IPublishedContentExtensions
	{

		public static IEnumerable<IPublishedContent> ListViewChildren(this IPublishedContent content)
		{
			return content.Children(c => c.IsInListView());
		}

		public static IEnumerable<IPublishedContent> NoListViewChildren(this IPublishedContent content)
		{
			return content.Children(c => c.IsInListView() == false);
		}

		public static bool IsInListView(this IPublishedContent content)
		{
			var contentTypeAliases = content.Parent.FindGridListViewContentTypeAliases();

			return contentTypeAliases.IsInListView(content.DocumentTypeAlias);
		}


		public static bool IsInListView(this IPublishedContent content, string contentTypeAlias)
		{
			var contentTypeAliases = content.FindGridListViewContentTypeAliases();

			return contentTypeAliases.IsInListView(contentTypeAlias);
		}

		public static bool IsInListView(this IContent content, string contentTypeAlias)
		{
			var contentTypeAliases = content.FindGridListViewContentTypeAliases();

			return contentTypeAliases.IsInListView(contentTypeAlias);
		}

		public static bool IsInListView(this string[] contentTypeAliases, string contentTypeAlias)
		{
			return contentTypeAliases.Contains(Constants.ListViewShowAllKey, StringComparer.OrdinalIgnoreCase) ||
					contentTypeAliases.Contains(contentTypeAlias, StringComparer.OrdinalIgnoreCase);
		}

		public static string[] FindGridListViewContentTypeAliases(this IPublishedContent content)
		{
			return FindGridListViewContentTypeAliases(gridListView =>
				content.ContentType.PropertyTypes.Any(propertyType => propertyType.PropertyEditorAlias.Equals(gridListView.PropertyEditorAlias))
			);
		}

		public static string[] FindGridListViewContentTypeAliases(this IContent content)
		{
			return FindGridListViewContentTypeAliases(gridListView =>
				content.ContentType.PropertyTypes.Any(propertyType => propertyType.PropertyEditorAlias.Equals(gridListView.PropertyEditorAlias))
			);
		}

		private static string[] FindGridListViewContentTypeAliases(Func<IDataTypeDefinition, bool> predicate)
		{
			var gridListViews = GetListViewEditorDataTypeDefinitions();

			var usedGridListView = gridListViews.FirstOrDefault(predicate);

			if (usedGridListView != null)
			{
				return GetPreValuesByDataTypeId(usedGridListView.Id);
			}

			return new string[0];
		}

		private static IEnumerable<IDataTypeDefinition> GetListViewEditorDataTypeDefinitions()
		{
			return ApplicationContext.Current.ApplicationCache.RuntimeCache
				.GetCacheItem<IEnumerable<IDataTypeDefinition>>("OPTEN.GetListViewEditorDataTypeDefinitions",
				() => ApplicationContext.Current.Services.DataTypeService.GetDataTypeDefinitionByPropertyEditorAlias(Constants.ListViewEditorAlias),
				TimeSpan.FromMinutes(10));

		}

		private static string[] GetPreValuesByDataTypeId(int dataTypeId)
		{
			return ApplicationContext.Current.ApplicationCache.RuntimeCache
				.GetCacheItem<string[]>("OPTEN.GetPreValuesByDataTypeId." + dataTypeId,
				() => ApplicationContext.Current.Services.DataTypeService.GetPreValuesCollectionByDataTypeId(dataTypeId)
				.PreValuesAsDictionary[Constants.ContentTypeAliasesPreValueKey].Value.Split(','),
				TimeSpan.FromMinutes(10));
		}
	}
}
