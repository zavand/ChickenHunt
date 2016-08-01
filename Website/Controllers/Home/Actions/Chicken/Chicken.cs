using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using ChickenHunt.Website.Controllers.Home.Actions.Chicken;
using ChickenHunt.Website.DataLayer;

namespace ChickenHunt.Website.Controllers.Home
{
    public partial class HomeController
    {
        [Authorize]
        public ActionResult Chicken(Route r)
        {
            var m = GetModel<Model, Route, HomeController>(r, this);
            PrepareModel(m, r);
            return View(m);
        }

        void PrepareModel(Model m, Route r)
        {
            m.Recipients = new[] { new ChickenCandidate { Name = "- Select Chicken Hunter -" } }.Union(_dataStorage.GetHunters()).ToArray();
        }

        [Authorize]
        [HttpPost]
        public ActionResult Chicken(ModelPost post, Route r, FormCollection c)
        {
            var m = GetModel<Model, Route, HomeController>(r, this);
            PrepareModel(m, r);
            m.Post = post;

            if (post.ChickenRecipient1.HasValue && new[] {post.ChickenRecipient2, post.ChickenMaker1, post.ChickenMaker2}.Contains(post.ChickenRecipient1))
            {
                ModelState.AddModelError($"{nameof(Model.Post)}.{nameof(ModelPost.ChickenRecipient1)}", "Duplicated hunter Chicken Recipient 1");
            }

            if (post.ChickenRecipient2.HasValue && new[] {post.ChickenRecipient1, post.ChickenMaker1, post.ChickenMaker2}.Contains(post.ChickenRecipient2))
            {
                ModelState.AddModelError($"{nameof(Model.Post)}.{nameof(ModelPost.ChickenRecipient2)}", "Duplicated hunter Chicken Recipient 2");
            }

            if (post.ChickenMaker1.HasValue && new[] {post.ChickenRecipient1, post.ChickenRecipient2, post.ChickenMaker2}.Contains(post.ChickenMaker1))
            {
                ModelState.AddModelError($"{nameof(Model.Post)}.{nameof(ModelPost.ChickenMaker1)}", "Duplicated hunter Chicken Maker 1");
            }

            if (post.ChickenMaker2.HasValue && new[] {post.ChickenRecipient1, post.ChickenRecipient2, post.ChickenMaker1}.Contains(post.ChickenMaker2))
            {
                ModelState.AddModelError($"{nameof(Model.Post)}.{nameof(ModelPost.ChickenMaker2)}", "Duplicated hunter Chicken Maker 2");
            }

            if (ModelState.IsValid)
            {
                _dataStorage.InsertChickenRecord(DateTime.Now, post.ChickenRecipient1.Value, post.ChickenRecipient2.Value, post.ChickenMaker1, post.ChickenMaker2, m.CurrentHunter.ID);
                var d=DateTime.Today;
                d = d.AddDays(-d.Day+1);
                _dataStorage.UpdateCude(post.ChickenRecipient1.Value,d);
                _dataStorage.UpdateCude(post.ChickenRecipient2.Value,d);
                
                // Send email

                return RedirectToAction<Actions.Index.Route>(r);
            }
            return View(m);
        }
    }
}

namespace ChickenHunt.Website.Controllers.Home.Actions.Chicken
{
    public class Route : HomeRoute
    {
        public const string ActionName = "Chicken";

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
        [Required]
        public int? ChickenRecipient1 { get; set; }
        [Required]
        public int? ChickenRecipient2 { get; set; }
        [Required]
        public int? ChickenMaker1 { get; set; }
        [Required]
        public int? ChickenMaker2 { get; set; }
    }
}