using System.Web.Mvc;
using ChickenHunt.Website.Controllers.Home.Actions.Index;
using ChickenHunt.Website.DataLayer;

namespace ChickenHunt.Website.Controllers.Home
{
    public partial class HomeController
    {
        public ActionResult Index(Route r)
        {
            var m = GetModel<Model, Route, HomeController>(r, this);
            PrepareModel(m, r);
            return View(m);
        }

        void PrepareModel(Model m, Route r)
        {
            m.Report = _dataStorage.GetReport();
            m.Recent = _dataStorage.GetRecentChickens();
        }
    }
}

namespace ChickenHunt.Website.Controllers.Home.Actions.Index
{
    public class Route : HomeRoute
    {
        public const string ActionName = "Index";

        public Route()
        {
            Action = ActionName;
        }

        public static Route Create(int? recentChickens = null)
        {
            return new Route { RecentChickens =recentChickens};
        }

        public int? RecentChickens { get; set; }
    }

    public class Model : HomeModel<Route, HomeController>
    {
        public ChickenHuntReport[] Report { get; set; }
        public RecentChickenRecord[] Recent { get; set; }

        public override void SetupModel(HomeController controller, Route route)
        {
            base.SetupModel(controller, route);

            RecentChickens = route.RecentChickens ?? 10;
        }

        public int RecentChickens { get; set; }
    }
}