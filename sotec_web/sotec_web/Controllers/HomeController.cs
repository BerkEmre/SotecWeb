using sotec_firma.Helpers;
using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace sotec_web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Contact()
        {
            return View();
        }
        public ActionResult About()
        {
            return View();
        }
        public ActionResult Blogs(int id = 0, int id2 = 0)
        {
            ViewBag.sayfa_no = id;
            ViewBag.kategori_id = id2;

            return View();
        }
        public ActionResult Blog(int id = 0)
        {
            ViewBag.blog_id = id;

            if (SQL.get("SELECT * FROM bloglar WHERE silindi = 0 AND blog_id = " + id).Rows.Count <= 0)
            {
                return RedirectToAction("Blogs", new { id = 0, id2 = 0 });
            }

            return View();
        }
        public ActionResult News(int id = 0, int id2 = 0)
        {
            ViewBag.sayfa_no = id;
            ViewBag.kategori_id = id2;

            return View();
        }
        public ActionResult New(int id = 0)
        {
            ViewBag.haber_id = id;

            if (SQL.get("SELECT * FROM haberler WHERE silindi = 0 AND haber_id = " + id).Rows.Count <= 0)
            {
                return RedirectToAction("News", new { id = 0, id2 = 0 });
            }

            return View();
        }
        public ActionResult Services()
        {
            return View();
        }
        public ActionResult Service(int id = 0)
        {
            ViewBag.hizmet_id = id;

            if (SQL.get("SELECT * FROM hizmetler WHERE silindi = 0 AND hizmet_id = " + id).Rows.Count <= 0)
            {
                return RedirectToAction("Services", new { id = 0, id2 = 0 });
            }

            return View();
        }
        public ActionResult Products(int id = 0, int id2 = 0)
        {
            ViewBag.sayfa_no = id;
            ViewBag.kategori_id = id2;

            return View();
        }
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
            return Redirect("/Home/Index");
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