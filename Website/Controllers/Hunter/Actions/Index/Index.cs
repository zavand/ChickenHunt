using System.Web.Mvc;
using ChickenHunt.Website.Controllers.Hunter.Actions.Index;
using ChickenHunt.Website.DataLayer;

namespace ChickenHunt.Website.Controllers.Hunter
{
    public partial class HunterController
    {
        public ActionResult Index(Route r)
        {
            var m = GetModel<Model, Route, HunterController>(r, this);
            PrepareModel(m, r);
            return View(m);
        }

        void PrepareModel(Model m, Route r)
        {
            m.Games = _dataStorage.GetHunterGames(r.ID);
        }

        [HttpPost]
        public ActionResult Index(ModelPost post, Route r, FormCollection c)
        {
            var m = GetModel<Model, Route, HunterController>(r, this);
            PrepareModel(m, r);
            m.Post = post;

            if (ModelState.IsValid)
            {
                return RedirectToAction<Actions.Index.Route>(r);
            }
            return View(m);
        }
    }
}

namespace ChickenHunt.Website.Controllers.Hunter.Actions.Index
{
    public class Route : HunterRoute
    {
        public const string ActionName = "Index";

        public Route()
        {
            Action = ActionName;
        }

        public static Route Create(int hunterID)
        {
            return new Route() { ID = hunterID };
        }
    }

    public class Model : HunterModel<Route, HunterController>
    {
        public ModelPost Post { get; set; }
        public RecentChickenRecord[] Games { get; set; }

        public Model()
        {
            Post = new ModelPost();
        }
    }

    public class ModelPost
    {

    }
}