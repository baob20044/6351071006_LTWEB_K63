using Ganss.Xss;
using Microsoft.Win32.SafeHandles;
using MvcMotorbikeStore.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ganss.Xss;
namespace MvcMotorbikeStore.Controllers
{
    public class AdminController : Controller
    {
        dbQLBanxeganmayDataContextDataContext data;
        // GET: Admin
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["QLBANXEGANMAYConnectionString"].ConnectionString;
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Login(FormCollection collection)
        {
            // Kết nối với database
            data = new dbQLBanxeganmayDataContextDataContext(connectionString);

            // Lấy thông tin từ Form
            var tendn = collection["username"];
            var matkhau = collection["password"];

            // Kiểm tra dữ liệu nhập vào
            if (String.IsNullOrEmpty(tendn))
            {
                ViewData["Loi1"] = "Phải nhập tên đăng nhập";
            }
            else if (String.IsNullOrEmpty(matkhau))
            {
                ViewData["Loi2"] = "Phải nhập mật khẩu";
            }
            else
            {
                // Tìm kiếm Admin theo tên đăng nhập và mật khẩu
                Admin ad = data.Admins.SingleOrDefault(n => n.UserAdmin == tendn && n.PassAdmin == matkhau);

                if (ad != null)
                {
                    // Lưu thông tin đăng nhập vào Session
                    Session["Taikhoanadmin"] = ad;
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    ViewBag.Thongbao = "Tên đăng nhập hoặc mật khẩu không đúng";
                }
            }

            return View();
        }
        public ActionResult Xe(int ?page)
        {
            data = new dbQLBanxeganmayDataContextDataContext(connectionString);
            int pageNumber = (page ?? 1);
            int pageSize = 7;
            return View(data.XEGANMAYs.ToList().OrderBy(n=>n.MaXe).ToPagedList(pageNumber,pageSize));
        }
        [HttpGet]
        public ActionResult Themmoixe()
        {
            var data = new dbQLBanxeganmayDataContextDataContext(connectionString); // Khởi tạo đối tượng dữ liệu

            // Kiểm tra dữ liệu có null không
            if (data.LOAIXEs == null || data.NHAPHANPHOIs == null)
            {
                return View("Error"); // Trả về trang lỗi nếu không có dữ liệu
            }

            ViewBag.MaLX = new SelectList(data.LOAIXEs.ToList().OrderBy(n => n.TenLoaiXe), "MaLX", "TenLoaiXe");
            ViewBag.MaNPP = new SelectList(data.NHAPHANPHOIs.ToList().OrderBy(n => n.TenNPP), "MaNPP", "TenNPP");

            return View();
        }
        [HttpPost]
        [ValidateInput(false)]  // Disables request validation for this action
        public ActionResult Themmoixe(XEGANMAY xe, HttpPostedFileBase fileupload)
        {
            // Khởi tạo đối tượng dữ liệu
            var data = new dbQLBanxeganmayDataContextDataContext(connectionString);

            // Lấy danh sách cho dropdown
            ViewBag.MaLX = new SelectList(data.LOAIXEs.ToList().OrderBy(n => n.TenLoaiXe), "MaLX", "TenLoaiXe");
            ViewBag.MaNPP = new SelectList(data.NHAPHANPHOIs.ToList().OrderBy(n => n.TenNPP), "MaNPP", "TenNPP");

            // Sanitize the Mota input to remove potentially dangerous HTML
            var sanitizer = new HtmlSanitizer();
            xe.Mota = sanitizer.Sanitize(xe.Mota);

            // Kiểm tra xem người dùng đã chọn ảnh hay chưa
            if (fileupload == null || fileupload.ContentLength == 0)
            {
                ModelState.AddModelError("fileupload", "Vui lòng chọn ảnh bìa");
            }

            // Kiểm tra tính hợp lệ của model
            if (ModelState.IsValid)
            {
                // Kiểm tra và xử lý ảnh
                if (fileupload != null && fileupload.ContentLength > 0)
                {
                    // Validate the file type
                    string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                    string fileExtension = Path.GetExtension(fileupload.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("fileupload", "Chỉ hỗ trợ các định dạng ảnh: .jpg, .jpeg, .png, .gif");
                        return View(xe); // Return the view if the file type is not valid
                    }

                    // Tạo tên file duy nhất
                    string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;

                    // Đường dẫn lưu ảnh
                    var path = Path.Combine(Server.MapPath("~/Images"), uniqueFileName);

                    try
                    {
                        // Lưu ảnh vào thư mục
                        fileupload.SaveAs(path);

                        // Cập nhật thông tin ảnh vào đối tượng xe
                        xe.Anhbia = uniqueFileName; // Save the unique filename to the model
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("fileupload", "Có lỗi xảy ra khi tải lên ảnh: " + ex.Message);
                        return View(xe); // Return the view with the error message
                    }
                }

                // Cập nhật thông tin xe vào cơ sở dữ liệu
                data.XEGANMAYs.InsertOnSubmit(xe);
                data.SubmitChanges();

                // Chuyển hướng về trang danh sách xe
                return RedirectToAction("Xe");
            }

            // Trả lại view nếu model không hợp lệ
            return View(xe);
        }
        public ActionResult Chitietxe(int id)
        {
            var data = new dbQLBanxeganmayDataContextDataContext(connectionString);
            XEGANMAY xe = data.XEGANMAYs.SingleOrDefault(n => n.MaXe == id);
            ViewBag.MaXe = xe.MaXe;
            if (xe == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(xe);
        }
        [HttpGet]
        public ActionResult Xoaxe(int id)
        {
            var data = new dbQLBanxeganmayDataContextDataContext(connectionString);
            XEGANMAY xe = data.XEGANMAYs.SingleOrDefault(n=>n.MaXe == id);
            ViewBag.MaXe = xe.MaXe;
            if (xe == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(xe);
        }
        [HttpPost,ActionName("Xoaxe")]
        public ActionResult Xacnhanxoa(int id)
        {
            var data = new dbQLBanxeganmayDataContextDataContext(connectionString);
            XEGANMAY xe = data.XEGANMAYs.SingleOrDefault(n => n.MaXe == id);
            ViewBag.MaXe = xe.MaXe;
            if(xe == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            data.XEGANMAYs.DeleteOnSubmit(xe);
            data.SubmitChanges();
            return RedirectToAction("Xe");
        }
        [HttpGet]
        public ActionResult Suaxe(int id)
        {
            var data = new dbQLBanxeganmayDataContextDataContext(connectionString);

            XEGANMAY xe = data.XEGANMAYs.SingleOrDefault(n => n.MaXe == id);
            if (xe == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            // Populate dropdown lists with current values
            ViewBag.MaLX = new SelectList(data.LOAIXEs.ToList().OrderBy(n => n.TenLoaiXe), "MaLX", "TenLoaiXe", xe.MaLX);
            ViewBag.MaNPP = new SelectList(data.NHAPHANPHOIs.ToList().OrderBy(n => n.TenNPP), "MaNPP", "TenNPP", xe.MaNPP);

            return View(xe);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Suaxe(XEGANMAY xe, HttpPostedFileBase fileupload)
        {
            var data = new dbQLBanxeganmayDataContextDataContext(connectionString);

            // Repopulate dropdown lists for the form
            ViewBag.MaLX = new SelectList(data.LOAIXEs.ToList().OrderBy(n => n.TenLoaiXe), "MaLX", "TenLoaiXe", xe.MaLX);
            ViewBag.MaNPP = new SelectList(data.NHAPHANPHOIs.ToList().OrderBy(n => n.TenNPP), "MaNPP", "TenNPP", xe.MaNPP);

            // Kiểm tra tính hợp lệ của model
            if (ModelState.IsValid)
            {
                // Kiểm tra và xử lý ảnh
                if (fileupload != null && fileupload.ContentLength > 0)
                {
                    // Validate the file type
                    string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                    string fileExtension = Path.GetExtension(fileupload.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("fileupload", "Chỉ hỗ trợ các định dạng ảnh: .jpg, .jpeg, .png, .gif");
                        return View(xe); // Return the view if the file type is not valid
                    }

                    // Tạo tên file duy nhất
                    string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;

                    // Đường dẫn lưu ảnh
                    var path = Path.Combine(Server.MapPath("~/Images"), uniqueFileName);

                    try
                    {
                        // Lưu ảnh vào thư mục
                        fileupload.SaveAs(path);

                        // Cập nhật thông tin ảnh vào đối tượng xe
                        xe.Anhbia = uniqueFileName; // Save the unique filename to the model
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("fileupload", "Có lỗi xảy ra khi tải lên ảnh: " + ex.Message);
                        return View(xe); // Return the view with the error message
                    }
                }
                else
                {
                    // If no new file is uploaded, retain the existing image
                    var existingXe = data.XEGANMAYs.SingleOrDefault(n => n.MaXe == xe.MaXe);
                    if (existingXe != null)
                    {
                        xe.Anhbia = existingXe.Anhbia; // Retain the existing image if no new image is uploaded
                    }
                }

                // Cập nhật thông tin xe vào cơ sở dữ liệu
                var existingXeInDb = data.XEGANMAYs.SingleOrDefault(n => n.MaXe == xe.MaXe);
                if (existingXeInDb != null)
                {
                    existingXeInDb.TenXe = xe.TenXe;
                    existingXeInDb.Giaban = xe.Giaban;
                    existingXeInDb.Mota = xe.Mota;
                    existingXeInDb.Anhbia = xe.Anhbia; // Make sure Anhbia is updated
                    existingXeInDb.Ngaycapnhat = xe.Ngaycapnhat;
                    existingXeInDb.Soluongton = xe.Soluongton;
                    existingXeInDb.MaLX = xe.MaLX;
                    existingXeInDb.MaNPP = xe.MaNPP;

                    data.SubmitChanges(); // Save changes to the database
                }

                // Redirect to the list of cars after successful update
                return RedirectToAction("Xe");
            }

            // Return the view if model is not valid
            return View(xe);
        }

        public ActionResult Bieudoxe()
        {
            var data = new dbQLBanxeganmayDataContextDataContext(connectionString);

            var thongKeXe = data.XEGANMAYs
               .GroupBy(x => x.MaLX)
               .Select(g => new
               {
                   TenLoaiXe = g.FirstOrDefault().LOAIXE.TenLoaiXe,
                   SoLuongXe = g.Count()
               })
               .ToList() // Chuyển về danh sách trong bộ nhớ
               .Select(x => new ThongKeXeViewModel
               {
                   TenLoaiXe = x.TenLoaiXe ?? "Không xác định",
                   SoLuongXe = x.SoLuongXe
               })
               .ToList();

            return View(thongKeXe);
        }
    }
}