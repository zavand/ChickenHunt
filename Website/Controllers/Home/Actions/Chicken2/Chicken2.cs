using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using ChickenHunt.Website.Controllers.Home.Actions.Chicken2;
using ChickenHunt.Website.DataLayer;

namespace ChickenHunt.Website.Controllers.Home
{
    public partial class HomeController
    {
        [Authorize]
        public ActionResult Chicken2(Route r)
        {
            var m = GetModel<Model, Route, HomeController>(r, this);
            PrepareModel(m, r);
            return View(m);
        }

        void PrepareModel(Model m, Route r)
        {
            m.Recipients = _dataStorage.GetHunters();
        }

        [Authorize]
        [HttpPost]
        public ActionResult Chicken2(ModelPost post, Route r, FormCollection c)
        {
            var m = GetModel<Model, Route, HomeController>(r, this);
            PrepareModel(m, r);
            m.Post = post;

            int[] recipients = null;
            int[] makers = null;
            try
            {
                recipients = post.Recipients.Trim('[', ']').Split(',').Select(int.Parse).ToArray();
                makers = post.Makers.Trim('[', ']').Split(',').Select(int.Parse).ToArray();
            }
            catch (Exception)
            {
                ModelState.AddModelError("","Invalid input");
            }

            if (recipients == null || recipients.Length != 2 || makers == null || makers.Length != 2)
            {
                ModelState.AddModelError("","Invalid input");
            }
            if (ModelState.IsValid)
            {
                if (post.Date == DateTime.Today.Date)
                {
                    post.Date = DateTime.Now;
                }
                else
                {
                    post.Date = post.Date.Date.AddDays(1).AddMinutes(-1);
                }
                _dataStorage.InsertChickenRecord(DateTime.Now, post.Date, recipients[0], recipients[1], makers[0], makers[1], m.CurrentHunter.ID);
                var d = post.Date.Date;
                d = d.AddDays(-d.Day + 1);
                _dataStorage.UpdateCude(recipients[0], d);
                _dataStorage.UpdateCude(recipients[1], d);

                return RedirectToAction<Actions.Index.Route>(r);
            }
            return View(m);
        }
    }
}

namespace ChickenHunt.Website.Controllers.Home.Actions.Chicken2
{
    public class Route : HomeRoute
    {
        public const string ActionName = "Chicken2";

        public Route()
        {
            Action = ActionName;
        }

        public static Route Create()
        {
            return new Route();
        }
    }

    public class Model : HomeModel<Route, HomeController>
    {
        public ModelPost Post { get; set; }

        public ChickenCandidate[] Recipients { get; set; }
        public Model()
        {
            Post = new ModelPost();
        }
    }

    public class ModelPost
    {
        public string Recipients { get; set; }
        public string Makers { get; set; }
        public DateTime Date { get; set; }
    }
}