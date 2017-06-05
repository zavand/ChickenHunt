using System;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Routing;
using ChickenHunt.Website.Controllers.Home.Actions.Index;
using ChickenHunt.Website.DataLayer;
using Zavand.Web.Mvc.Manana.Framework;
using Route = ChickenHunt.Website.Controllers.Home.Actions.Index.Route;

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
            m.Chickens = _dataStorage.GetChickens();
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
        public SortType? SortBy { get; set; }
        public bool? SortAsc { get; set; }
        public string SortPeriod { get; set; }

        public override void MakeTheSameAs(IBaseRoute r)
        {
            base.MakeTheSameAs(r);

            var d = r as Route;
            if (d != null)
            {
                RecentChickens = d.RecentChickens;
                SortBy = d.SortBy;
                SortAsc = d.SortAsc;
                SortPeriod = d.SortPeriod;
                Months = d.Months;
            }
        }
        public int? Months { get; set; }

        //        public override object GetDefaults()
        //        {
        //            var d = new RouteValueDictionary(base.GetDefaults());
        //            d.Add(nameof(SortBy), SortType.K);
        ////            d.Add(nameof(SortAsc), false);
        ////            d.Add(nameof(PageSize), BaseWebsiteRoute.DefaultPageSize);
        ////            d.Add(nameof(OrderBy), BusinessOrderBy.CreateDate.ToString());
        ////            d.Add(nameof(Asc), AscDescType.Desc);
        //            return d;
        //        }
    }

    public class Model : HomeModel<Route, HomeController>
    {
        public SortType SortBy { get; set; }
        public bool SortAsc { get; set; }
        public DateTime SortPeriod { get; set; }
        public DateTime SortPeriodFrom { get; set; }
        public DateTime SortPeriodTo { get; set; }
        public bool SortPeriodAnnual { get; set; }

        public override void SetupModel(HomeController controller, Route route)
        {
            base.SetupModel(controller, route);

            RecentChickens = Math.Min(route.RecentChickens ?? 10, 1000);
            SortBy = route.SortBy ?? SortType.T;
            SortAsc = route.SortAsc ?? false;
            SortPeriod = DateTime.Today.AddDays(-DateTime.Today.Day + 1);
            SortPeriodFrom = DateTime.Today.AddDays(-DateTime.Today.Day + 1);
            SortPeriodTo = DateTime.Today.AddDays(-DateTime.Today.Day + 1).AddMonths(1);
            SortPeriodAnnual = false;
            Months = Math.Min(route.Months ?? 4, 12);

            if (!String.IsNullOrEmpty(route.SortPeriod))
            {
                DateTime d;
                if (DateTime.TryParseExact(route.SortPeriod, new[] { "yyyy-MM", "yyyyMM", "yyyy"  }, System.Threading.Thread.CurrentThread.CurrentCulture, DateTimeStyles.None, out d))
                {
                    SortPeriod = d;
                    SortPeriodFrom = d;
                    SortPeriodTo = route.SortPeriod.Length == 4 ? d.AddYears(1) : d.AddMonths(1);
                    SortPeriodAnnual = route.SortPeriod.Length == 4;
                }
            }
        }

        public int RecentChickens { get; set; }
        public int Months { get; set; }
        public RecentChickenRecord[] Chickens { get; set; }
    }

    public enum SortType
    {
        B,
        K,
        T,
        R,
        M
    }
}