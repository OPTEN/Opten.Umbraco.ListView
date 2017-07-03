using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Services;

namespace Opten.Umbraco.ListView.Events
{
	public class DataTypeServiceEventHandler : ApplicationEventHandler
	{
		protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
		{
			DataTypeService.Saved += DataTypeService_Saved;
		}

		private void DataTypeService_Saved(IDataTypeService sender, global::Umbraco.Core.Events.SaveEventArgs<global::Umbraco.Core.Models.IDataTypeDefinition> e)
		{
			ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch("OPTEN.FindGridListViewContentTypeAliases");
		}
	}
}
