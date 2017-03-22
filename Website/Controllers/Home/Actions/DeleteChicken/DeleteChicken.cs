using System.Web.Mvc;
using System.Web.Security;
using ChickenHunt.Website.Controllers.Home.Actions.DeleteChicken;
using ChickenHunt.Website.DataLayer;

namespace ChickenHunt.Website.Controllers.Home
{
    public partial class HomeController
    {
        [Authorize]
        public ActionResult DeleteChicken(Route r)
        {
            var m = GetModel<Model, Route, HomeController>(r, this);
            PrepareModel(m, r);
            return View(m);
        }

        void PrepareModel(Model m, Route r)
        {

            m.Chicken = _dataStorage.GetChicken(r.ChickenID);
        }

        [Authorize]
        [HttpPost]
        public ActionResult DeleteChicken(ModelPost post, Route r, FormCollection c)
        {
            var m = GetModel<Model, Route, HomeController>(r, this);
            PrepareModel(m, r);
            m.Post = post;

            if (ModelState.IsValid)
            {
                var cc = Request.Cookies[FormsAuthentication.FormsCookieName];
                var t1 = FormsAuthentication.Decrypt(cc.Value);
                var hunter = _dataStorage.GetHunterByToken(t1.UserData);
                _dataStorage.DeleteChicken(r.ChickenID,hunter.ID);
                return RedirectToAction<Actions.Index.Route>(r);
            }
            return View(m);
        }
    }
}

namespace ChickenHunt.Website.Controllers.Home.Actions.DeleteChicken
{
    public class Route : HomeRoute
    {
        public const string ActionName = "DeleteChicken";

        public Route()
        {
            Action = ActionName;
        }

        public static Route Create(int chickenID)
        {
            return new Route(){ChickenID= chickenID };
        }

        public int ChickenID { get; set; }

        public override string GetUrl()
        {
            return base.GetUrl()+"/{ChickenID}";
        }
    }

    public class Model : HomeModel<Route, HomeController>
    {
        public ModelPost Post { get; set; }
        public RecentChickenRecord Chicken { get; set; }

        public Model()
        {
            Post = new ModelPost();
        }
    }

    public class ModelPost
    {

    }
}