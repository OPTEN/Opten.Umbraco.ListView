using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace Opten.Umbraco.ListView.Extensions
{
	public static class IPublishedContentExtensions
	{

		public static IEnumerable<IPublishedContent> ListViewChildren(this IPublishedContent content, string alias = null)
		{
			return content.ListViewOrTreeChildren(true, alias);
		}

		public static IEnumerable<IPublishedContent> TreeChildren(this IPublishedContent content)
		{
			return content.ListViewOrTreeChildren(false);
		}

		public static IEnumerable<IPublishedContent> ListViewOrTreeChildren(this IPublishedContent content, bool isListView, string alias = null)
		{
			var contentTypeAliases = content.FindListViewContentTypeAliases(alias);

			return content.Children(c => contentTypeAliases.IsInListView(c.DocumentTypeAlias) == isListView);
		}


		public static IEnumerable<IContent> ListViewChildren(this IContent content, string alias = null)
		{
			return content.ListViewOrTreeChildren(true, alias);
		}

		public static IEnumerable<IContent> TreeChildren(this IContent content)
		{
			return content.ListViewOrTreeChildren(false);
		}

		public static IEnumerable<IContent> ListViewOrTreeChildren(this IContent content, bool isListView, string alias = null)
		{
			var contentTypeAliases = content.FindGridListViewContentTypeAliases(alias);

			return content.Children().Where(c => contentTypeAliases.IsInListView(c.ContentType.Alias) == isListView);
		}

		public static bool IsInListView(this IContent content, string contentTypeAlias)
		{
			return content.FindGridListViewContentTypeAliases().IsInListView(contentTypeAlias);
		}

		public static bool IsInListView(this IEnumerable<string> contentTypeAliases, string contentTypeAlias)
		{
			return contentTypeAliases.Contains(Constants.ListViewShowAllKey, StringComparer.OrdinalIgnoreCase) ||
					contentTypeAliases.Contains(contentTypeAlias, StringComparer.OrdinalIgnoreCase);
		}


		public static IEnumerable<string> FindListViewContentTypeAliases(this IPublishedContent content, string alias = null)
		{
			return content.Properties
				.Where(property => alias == null || property.PropertyTypeAlias.Equals(alias))
				.Select(property =>
				{
					var prop = property.GetType().GetField("PropertyType");
					var type = (PublishedPropertyType)prop.GetValue(property);

					return type.DataTypeId;
				})
				.ContentTypeAliases();
		}


		public static IEnumerable<string> FindGridListViewContentTypeAliases(this IContent content, string alias = null)
		{
			return content.Properties
				.Where(property => alias == null || property.PropertyType.Alias.Equals(alias))
				.Select(property => property.PropertyType.DataTypeDefinitionId)
				.ContentTypeAliases();
		}

		public static IEnumerable<string> ContentTypeAliases(this IEnumerable<int> dataTypeDefinitionIds)
		{
			return GetListViewPropertyEditors()
				.Where(propertyEditor => dataTypeDefinitionIds.Any(dataTypeDefinitionId => dataTypeDefinitionId == propertyEditor.Key))
				.SelectMany(propertyEditor => propertyEditor.Value)
				.Distinct();
		}

		public static Dictionary<int, IEnumerable<string>> GetListViewPropertyEditors()
		{
			return ApplicationContext.Current.ApplicationCache.RuntimeCache
				.GetCacheItem<Dictionary<int, IEnumerable<string>>>(
					"OPTEN.GetListViewPropertyEditors",
					() => ApplicationContext.Current.Services.DataTypeService.GetDataTypeDefinitionByPropertyEditorAlias(Constants.ListViewEditorAlias)
							.Select(listView => new KeyValuePair<int, IEnumerable<string>>(
								listView.Id,
								ApplicationContext.Current.Services.DataTypeService.GetPreValuesCollectionByDataTypeId(listView.Id)
									.PreValuesAsDictionary[Constants.ContentTypeAliasesPreValueKey].Value
									.Split(','))
							).ToDictionary(x => x.Key, x => x.Value),
					TimeSpan.FromMinutes(10)
				);
		}

		public static ITemplate GetTemplate(this IPublishedContent content)
		{
			return ApplicationContext.Current.Services.FileService.GetTemplate(content.TemplateId);
		}
	}
}
