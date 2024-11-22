using MvcMotorbikeStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcMotorbikeStore.Controllers
{
    public class MotorbikeStoreController : Controller
    {
        dbQLBanxeganmayDataContextDataContext data;

        public MotorbikeStoreController()
        {
            // Get the connection string from the Web.config file
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["QLBANXEGANMAYConnectionString"].ConnectionString;

            // Initialize the DataContext with the connection string
            data = new dbQLBanxeganmayDataContextDataContext(connectionString);
        }

        private List<XEGANMAY> Layxemoi(int count)
        {
            // Fetch the newest motorbikes based on "Ngaycapnhat"
            return data.XEGANMAYs.OrderByDescending(a => a.Ngaycapnhat).Take(count).ToList();
        }

        // GET: MotorbikeStore
        public ActionResult Index()
        {
            var xemoi = Layxemoi(5); // Get the 5 newest motorbikes
            return View(xemoi); // Pass the list to the view
        }
        public ActionResult Loaixe() 
        {
            var loaixe = from cd in data.LOAIXEs select cd;
            return PartialView(loaixe);
        }
        public ActionResult Nhaphanphoi()
        {
            var nhaphanphoi = from cd in data.NHAPHANPHOIs select cd;
            return PartialView(nhaphanphoi);
        }
        public ActionResult SPTheoloaixe(int id)
        {
            // Get the motorbike(s) based on the category ID
            var xe = data.XEGANMAYs.Where(s => s.MaLX == id).ToList(); // Use ToList() to ensure it's a collection
            return View(xe); // Pass a collection of XEGANMAY objects
        }
        public ActionResult SPTheoNPP(int id)
        {
            // Get the motorbike(s) based on the category ID
            var xe = data.XEGANMAYs.Where(s => s.MaNPP == id).ToList(); // Use ToList() to ensure it's a collection
            return View(xe); // Pass a collection of XEGANMAY objects
        }
        public ActionResult Details(int id)
        {
            var xe = from s in data.XEGANMAYs
                     where s.MaLX == id
                     select s;
            return View(xe.Single());
        }
    }
}
