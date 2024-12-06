using MvcMotorbikeStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;

using Antlr.Runtime.Tree;
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
        public ActionResult Index(int ? page)
        {
            int pageSize = 5;
            int pageNum = (page ?? 1);
            var xemoi = Layxemoi(15); // Get the 5 newest motorbikes
            return View(xemoi.ToPagedList(pageNum,pageSize)); // Pass the list to the view
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
        public ActionResult SPTheoloaixe(int id, int? page)
        {
            // Số lượng mục mỗi trang
            int pageSize = 10;

            // Trang hiện tại, mặc định là trang 1 nếu không được truyền vào
            int pageNumber = (page ?? 1);

            // Lấy danh sách xe theo MaLX
            var xe = data.XEGANMAYs.Where(s => s.MaLX == id).ToList();

            // Chuyển đổi danh sách thành PagedList
            var pagedXe = xe.ToPagedList(pageNumber, pageSize);

            return View(pagedXe); // Truyền PagedList vào view
        }
        public ActionResult SPTheoNPP(int id, int? page)
        {
            // Số lượng mục mỗi trang
            int pageSize = 10;

            // Trang hiện tại, mặc định là trang 1 nếu không có tham số page
            int pageNumber = (page ?? 1);

            // Lấy danh sách xe theo MaNPP
            var xe = data.XEGANMAYs.Where(s => s.MaNPP == id).ToList();

            // Chuyển danh sách thành PagedList
            var pagedXe = xe.ToPagedList(pageNumber, pageSize);

            return View(pagedXe); // Truyền PagedList vào view
        }
        public ActionResult Details(int id)
        {
            var xe = from s in data.XEGANMAYs
                     where s.MaXe == id
                     select s;
            return View(xe.Single());
        }
    }
}
