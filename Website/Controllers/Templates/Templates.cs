using ChickenHunt.Website.Common;
using ChickenHunt.Website.DataLayer;
using log4net;

namespace ChickenHunt.Website.Controllers.Templates
{
    public partial class TemplatesController : BaseWebsiteController
    {
        public const string Name = "Templates";

        public TemplatesController(ILog log, IDataStorage dataStorage) : base(log, dataStorage)
        {
        }
    }

    public class TemplatesRoute : BaseWebsiteRoute
    {
        public TemplatesRoute()
        {
            Controller = TemplatesController.Name;
        }
    }

    public class TemplatesModel<TRoute, TController> : BaseWebsiteModel<TRoute, TController>
        where TRoute : BaseWebsiteRoute
        where TController : BaseWebsiteController
    {
    }
}