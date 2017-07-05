using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace Opten.Umbraco.ListView.Extensions
{
	public static class IPublishedContentExtensions
	{

		public static IEnumerable<IPublishedContent> ListViewChildren(this IPublishedContent content)
		{
			return content.Children(c => c.IsInListView());
		}

		public static IEnumerable<IPublishedContent> TreeChildren(this IPublishedContent content)
		{
			return content.Children(c => c.IsInListView() == false);
		}

		public static IEnumerable<IContent> TreeChildren(this IContent content)
		{
			return content.Children().Where(c => c.IsInListView(content) == false);
		}

		public static IEnumerable<IPublishedContent> ListViewChildren(this IPublishedContent content, string alias)
		{
			return content.Children(c => c.IsInListView(alias));
		}

		public static bool IsInListView(this IPublishedContent content)
		{
			var contentTypeAliases = content.Parent.FindGridListViewContentTypeAliases();

			return contentTypeAliases.IsInListView(content.DocumentTypeAlias);
		}

		public static bool IsInListView(this IContent content, IContent parent)
		{
			var contentTypeAliases = parent.FindGridListViewContentTypeAliases();

			return contentTypeAliases.IsInListView(content.ContentType.Alias);
		}

		public static bool IsInListView(this IPublishedContent content, string alias)
		{
			var contentTypeAliases = content.Parent.FindGridListViewContentTypeAliases(alias);

			return contentTypeAliases.IsInListView(content.DocumentTypeAlias);
		}

		public static bool IsInListView(this IContent content, string contentTypeAlias)
		{
			var contentTypeAliases = content.FindGridListViewContentTypeAliases();

			return contentTypeAliases.IsInListView(contentTypeAlias);
		}

		public static bool IsInListView(this IEnumerable<string> contentTypeAliases, string contentTypeAlias)
		{
			return contentTypeAliases.Contains(Constants.ListViewShowAllKey, StringComparer.OrdinalIgnoreCase) ||
					contentTypeAliases.Contains(contentTypeAlias, StringComparer.OrdinalIgnoreCase);
		}

		public static IEnumerable<string> FindGridListViewContentTypeAliases(this IPublishedContent content)
		{
			return FindGridListViewContentTypeAliases(gridListView =>
				content.Properties.Any(property => IsPropertyDataTypeDefinition(property, gridListView)),
				"FindGridListViewContentTypeAliasesIPublishedContent" + content.Id
			);
		}

		public static IEnumerable<string> FindGridListViewContentTypeAliases(this IContent content)
		{
			return FindGridListViewContentTypeAliases(gridListView =>
				content.Properties.Any(property => IsPropertyDataTypeDefinition(property, gridListView)),
				"FindGridListViewContentTypeAliasesIContent" + content.Id
			);
		}


		public static IEnumerable<string> FindGridListViewContentTypeAliases(this IPublishedContent content, string alias)
		{
			return FindGridListViewContentTypeAliases(gridListView =>
				IsPropertyDataTypeDefinition(content.Properties.FirstOrDefault(property => property.PropertyTypeAlias.Equals(alias)), gridListView),
				"FindGridListViewContentTypeAliasesIPublishedContent" + content.Id + alias
			);
		}

		public static IEnumerable<string> FindGridListViewContentTypeAliases(this IContent content, string alias)
		{
			return FindGridListViewContentTypeAliases(gridListView =>
				IsPropertyDataTypeDefinition(content.Properties.FirstOrDefault(propertyType => propertyType.Alias.Equals(alias)), gridListView),
				"FindGridListViewContentTypeAliasesIContent" + content.Id + alias
			);
		}

		private static bool IsPropertyDataTypeDefinition(Property property, IDataTypeDefinition dataTypeDefinition)
		{
			return property.PropertyType.DataTypeDefinitionId.Equals(dataTypeDefinition.Id);
		}

		private static bool IsPropertyDataTypeDefinition(IPublishedProperty property, IDataTypeDefinition dataTypeDefinition)
		{
			return property.PropertyTypeAlias.Equals(dataTypeDefinition.Name); // Really dont know if this is correct!
		}

		public static ITemplate GetTemplate(this IPublishedContent content)
		{
			return ApplicationContext.Current.Services.FileService.GetTemplate(content.TemplateId);
		}

		private static IEnumerable<string> FindGridListViewContentTypeAliases(Func<IDataTypeDefinition, bool> predicate, string key)
		{
			return ApplicationContext.Current.ApplicationCache.RuntimeCache
				.GetCacheItem<IEnumerable<string>>("OPTEN.FindGridListViewContentTypeAliases." + key,
				() =>
				{
					var dataTypeService = ApplicationContext.Current.Services.DataTypeService;

					var contentTypeAliases = new List<string>();
					foreach (var gridListView in dataTypeService.GetDataTypeDefinitionByPropertyEditorAlias(Constants.ListViewEditorAlias).Where(predicate))
					{
						foreach (var contentTypeAlias in dataTypeService.GetPreValuesCollectionByDataTypeId(gridListView.Id).PreValuesAsDictionary[Constants.ContentTypeAliasesPreValueKey].Value.Split(','))
						{
							contentTypeAliases.Add(contentTypeAlias);
						}
					}

					return contentTypeAliases.Distinct();
				}, TimeSpan.FromMinutes(10));

		}
	}
}
