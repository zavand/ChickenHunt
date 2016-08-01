using ChickenHunt.Website.Common;
using ChickenHunt.Website.DataLayer;
using log4net;

namespace ChickenHunt.Website.Controllers.Home
{
    public partial class HomeController : BaseWebsiteController
    {
        public const string Name = "Home";

        public HomeController(ILog log, IDataStorage dataStorage) : base(log, dataStorage)
        {
        }
    }

    public class HomeRoute : BaseWebsiteRoute
    {
        public HomeRoute()
        {
            Controller = HomeController.Name;
        }
    }

    public class HomeModel<TRoute, TController> : BaseWebsiteModel<TRoute, TController>
        where TRoute : BaseWebsiteRoute
        where TController : BaseWebsiteController
    {
    }
}