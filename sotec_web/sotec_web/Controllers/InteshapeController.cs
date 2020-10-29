using sotec_firma.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace sotec_web.Controllers
{
    public class InteshapeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ComingSoon()
        {
            return View();
        }

        [Route("Contact")]
        public ActionResult Contact()
        {
            return View();
        }

        [Route("About")]
        public ActionResult About()
        {
            return View();
        }

        [Route("Projects")]
        [Route("Projects/{id}")]
        [Route("Projects/{id}/{id2}")]
        public ActionResult Projects(int id = 0, int id2 = 0)
        {
            ViewBag.sayfa_no = id;
            ViewBag.kategori_id = id2;

            return View();
        }

        [Route("Project/{id}")]
        public ActionResult Project(int id = 0)
        {
            ViewBag.proje_id = id;

            if (SQL.get("SELECT * FROM projeler WHERE silindi = 0 AND proje_id = " + id).Rows.Count <= 0)
            {
                return RedirectToAction("Projects", new { id = 0, id2 = 0 });
            }

            return View();
        }

        [Route("Services")]
        public ActionResult Services()
        {
            return View();
        }

        [Route("Service/{id}")]
        public ActionResult Service(int id = 0)
        {
            ViewBag.hizmet_id = id;

            if (SQL.get("SELECT * FROM hizmetler WHERE silindi = 0 AND hizmet_id = " + id).Rows.Count <= 0)
            {
                return RedirectToAction("Services", new { id = 0, id2 = 0 });
            }

            return View();
        }

        [Route("Referances")]
        public ActionResult Referances()
        {
            return View();
        }

        [Route("Referance/{id}")]
        public ActionResult Referance(int id = 0)
        {
            ViewBag.referans_id = id;

            if (SQL.get("SELECT * FROM referanslar WHERE silindi = 0 AND referans_id = " + id).Rows.Count <= 0)
            {
                return RedirectToAction("Referances", new { id = 0, id2 = 0 });
            }

            return View();
        }

        [Route("Blogs")]
        [Route("Blogs/{id}")]
        [Route("Blogs/{id}/{id2}")]
        public ActionResult Blogs(int id = 0, int id2 = 0)
        {
            ViewBag.sayfa_no = id;
            ViewBag.kategori_id = id2;

            return View();
        }

        [Route("Blog/{id}")]
        public ActionResult Blog(int id = 0)
        {
            ViewBag.blog_id = id;

            if (SQL.get("SELECT * FROM bloglar WHERE silindi = 0 AND blog_id = " + id).Rows.Count <= 0)
            {
                return RedirectToAction("Blogs", new { id = 0, id2 = 0 });
            }

            return View();
        }

        [Route("Products")]
        [Route("Products/{id}")]
        [Route("Products/{id}/{id2}")]
        public ActionResult Products(int id = 0, int id2 = 0)
        {
            ViewBag.sayfa_no = id;
            ViewBag.kategori_id = id2;

            return View();
        }

        [Route("Product/{id}")]
        public ActionResult Product(int id = 0)
        {
            ViewBag.urun_id = id;

            if (SQL.get("SELECT * FROM urunler WHERE silindi = 0 AND urun_id = " + id).Rows.Count <= 0)
            {
                return RedirectToAction("Products", new { id = 0, id2 = 0 });
            }

            return View();
        }

        public ActionResult ChangeLanguage(string language, int id = 0)
        {
            if (id != 0)
            {
                ViewBag.dil_id = id;
            }

            HttpCookie cookie;
            cookie = new HttpCookie("MultiLanguageExample");
            Thread.CurrentThread.CurrentCulture = new CultureInfo(language);
            cookie.Value = language;
            Response.SetCookie(cookie);
            if (Request.UrlReferrer != null) return Redirect(Request.UrlReferrer.LocalPath);
            return Redirect("/Inteshape/Index");
        }

        [ValidateInput(false)]
        [HttpPost]
        public string MesajGonder(string ad_soyad, string email, string mesaj, string telefon, int kullanici_id = 0, string kaynak = "", string on_bilgi = "", string on_bilgi1 = "", string on_bilgi2 = "")
        {
            if (on_bilgi.Length > 0)
                mesaj = "<strong>" + on_bilgi + "</br>" + on_bilgi1 + " Adet</br>" + on_bilgi2 + "</strong></br></br>" + mesaj;

            if (String.IsNullOrEmpty(ad_soyad) || String.IsNullOrEmpty(email) || String.IsNullOrEmpty(mesaj))
            {
                return sotec_web.Resources.Dil.Ekisk_bilgi_girdiniz_lğtfen_tüm_alanları_doldurun;
            }
            else
            {
                SQL.set("INSERT INTO mesajlar (ad_soyad, email, telefon, mesaj, hedef_kullanici_id, kaynak) VALUES ('" + ad_soyad + "', '" + email + "', '" + telefon + "', '" + mesaj + "', " + kullanici_id + ", '" + kaynak + "')");
                return Resources.Dil.Mesajınız_alındı__en_kısa_sürede_size_dönüş_yapacağız_;
            }
        }
    }
}