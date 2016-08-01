using System.Web.Mvc;
using System.Web.Security;
using ChickenHunt.Website.Controllers.Sign.Actions.Out;

namespace ChickenHunt.Website.Controllers.Sign
{
    public partial class SignController
    {
        public ActionResult Out(Route r)
        {
            var m = GetModel<Model, Route, SignController>(r, this);
            FormsAuthentication.SignOut();
            return RedirectToAction<Home.Actions.Index.Route>(r);
        }
    }
}

namespace ChickenHunt.Website.Controllers.Sign.Actions.Out
{
    public class Route : SignRoute
    {
        public const string ActionName = "Out";

        public Route()
        {
            Action = ActionName;
        }

        public static Route Create()
        {
            return new Route();
        }
    }

    public class Model : SignModel<Route, SignController>
    {
    }
}