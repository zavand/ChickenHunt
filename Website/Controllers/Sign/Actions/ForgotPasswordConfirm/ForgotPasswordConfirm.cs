using System.Web.Mvc;
using ChickenHunt.Website.Controllers.Sign.Actions.ForgotPasswordConfirm;

namespace ChickenHunt.Website.Controllers.Sign
{
    public partial class SignController
    {
        public ActionResult ForgotPasswordConfirm(Route r)
        {
            var m = GetModel<Model, Route, SignController>(r, this);
            return View(m);
        }
    }
}

namespace ChickenHunt.Website.Controllers.Sign.Actions.ForgotPasswordConfirm
{
    public class Route : SignRoute
    {
        public const string ActionName = "ForgotPasswordConfirm";

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