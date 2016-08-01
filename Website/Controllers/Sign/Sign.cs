using ChickenHunt.Website.Common;
using ChickenHunt.Website.DataLayer;
using log4net;

namespace ChickenHunt.Website.Controllers.Sign
{
    public partial class SignController : BaseWebsiteController
    {
        public const string Name = "Sign";

        public SignController(ILog log, IDataStorage dataStorage) : base(log, dataStorage)
        {
        }
    }

    public class SignRoute : BaseWebsiteRoute
    {
        public SignRoute()
        {
            Controller = SignController.Name;
        }
    }

    public class SignModel<TRoute, TController> : BaseWebsiteModel<TRoute, TController>
        where TRoute : BaseWebsiteRoute
        where TController : BaseWebsiteController
    {
    }
}