using ChickenHunt.Website.Common;
using ChickenHunt.Website.DataLayer;
using log4net;

namespace ChickenHunt.Website.Controllers.Hunter
{
    public partial class HunterController : BaseWebsiteController
    {
        public const string Name = "Hunter";

        public HunterController(ILog log, IDataStorage dataStorage) : base(log, dataStorage)
        {
        }
    }

    public class HunterRoute : BaseWebsiteRoute
    {
        public HunterRoute()
        {
            Controller = HunterController.Name;
        }

        public int ID { get; set; }
    }

    public class HunterModel<TRoute, TController> : BaseWebsiteModel<TRoute, TController>
        where TRoute : BaseWebsiteRoute
        where TController : BaseWebsiteController
    {
    }
}