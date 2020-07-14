using sotec_firma.Helpers;
using sotec_web.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace sotec_web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult ComingSoon()
        {
            return View();
        }
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
        public ActionResult LostPass()
        {
            if (Session["kullanici_id"] != null)
                return RedirectToAction("Index");
            return View();
        }
        public ActionResult SignIn()
        {
            if (Session["kullanici_id"] != null)
                return RedirectToAction("Index");
            return View();
        }
        public ActionResult SignOut()
        {
            Session["kullanici_id"] = null;
            return RedirectToAction("Index");
        }
        public ActionResult Register()
        {
            if (Session["kullanici_id"] != null)
                return RedirectToAction("Index");
            return View();
        }
        public ActionResult Acount()
        {
            if (Session["kullanici_id"] == null)
                return RedirectToAction("Index");
            return View();
        }
        public ActionResult Adress()
        {
            if (Session["kullanici_id"] == null)
                return RedirectToAction("Index");
            return View();
        }
        public ActionResult AddAdress(int id = 0)
        {
            ViewBag.adres_id = id;

            return View();
        }

        public ActionResult Wishlist()
        {
            if (Session["kullanici_id"] == null)
                return RedirectToAction("Index");
            return View();
        }

        public ActionResult Cart()
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

        public ActionResult kullaniciGiris(string email, string sifre)
        {
            if (email.Length <= 0 || sifre.Length <= 0)
                return RedirectToAction("SignIn", new { hata = "E-mail ve Şifre giriniz!" });

            DataTable dt_kullanici = SQL.get("SELECT * FROM kullanicilar WHERE silindi = 0 AND email = '" + email + "' AND sifre = '" + sifre + "'");
            if (dt_kullanici.Rows.Count > 0)
            {
                Session["kullanici_id"] = dt_kullanici.Rows[0]["kullanici_id"];
                return RedirectToAction("Acount");
            }
            else
                return RedirectToAction("SignIn", new { hata = "E-mail veya şifre yanlış..." });
        }

        public ActionResult kullaniciEkle(string ad, string soyad, string telefon, string email, string sifre, int tip_parametre_id = 2)
        {
            if (ad.Length <= 0)
                return RedirectToAction("Register", new { hata = "Ad giriniz!" });
            if (soyad.Length <= 0)
                return RedirectToAction("Register", new { hata = "Soyad giriniz!" });
            if (email.Length <= 0)
                return RedirectToAction("Register", new { hata = "EMail giriniz!" });
            if (sifre.Length <= 0)
                return RedirectToAction("Register", new { hata = "Şifre giriniz!" });

            DataTable dt = SQL.get("SELECT * FROM kullanicilar WHERE silindi = 0 AND email = '" + email + "'");
            if (dt.Rows.Count > 0)
                return RedirectToAction("Register", new { hata = "Girdiğiniz E-Mail kullanılmaktadır!" });

            dt = SQL.get("INSERT INTO kullanicilar (kaydeden_kullanici_id, email, sifre, ad, soyad, kullanici_tipi_parametre_id, telefon) VALUES (0, '" + email + "', '" + sifre + "', '" + ad + "', '" + soyad + "', " + tip_parametre_id + ", '" + telefon + "'); SELECT SCOPE_IDENTITY();");
            Session["kullanici_id"] = dt.Rows[0][0];
            return RedirectToAction("Acount");
        }

        [HttpPost]
        public ActionResult kullaniciDuzenle(int kullanici_id, string ad, string soyad, string telefon, string email, string eski_sifre, string yeni_sifre, int tip_parametre_id = 2)
        {
            if (ad.Length <= 0)
                return RedirectToAction("Acount", new { hata = "Ad giriniz!" });
            if (soyad.Length <= 0)
                return RedirectToAction("Acount", new { hata = "Soyad giriniz!" });
            if (email.Length <= 0)
                return RedirectToAction("Acount", new { hata = "E-mail giriniz!" });

            DataTable dt = SQL.get("SELECT * FROM kullanicilar WHERE silindi = 0 AND kullanici_id != " + kullanici_id + " AND email = '" + email + "'");
            if (dt.Rows.Count > 0)
                return RedirectToAction("Kullanicilar", new { hata = "Girdiğiniz E-Mail kullanılmaktadır!" });

            SQL.set("UPDATE kullanicilar SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), email = '" + email + "', ad = '" + ad + "', soyad = '" + soyad + "', kullanici_tipi_parametre_id = " + tip_parametre_id + ", telefon = '" + telefon + "' WHERE kullanici_id = " + kullanici_id);


            dt = SQL.get("SELECT * FROM kullanicilar WHERE silindi = 0 AND kullanici_id != " + kullanici_id + " AND sifre = '" + eski_sifre + "'");
            if (dt.Rows.Count > 0 && yeni_sifre.Length > 5)
                SQL.set("UPDATE kullanicilar SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), sifre = '" + yeni_sifre + "' WHERE kullanici_id = " + kullanici_id);

            return RedirectToAction("Acount", new { tepki = 2 });
        }

        public ActionResult DeleteAdress(int id)
        {
            SQL.set("UPDATE adresler SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), silindi = 1 WHERE adres_id = " + id);
            return RedirectToAction("Adress", new { tepki = 3 });
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult adresEkle(int adres_id, string ad, string alici, string telefon, string ulke, string sehir, string semt, string adres, string posta_kodu, int kullanici_id)
        {
            if (ad.Length <= 0 || alici.Length <= 0 || adres.Length <= 0 || ulke.Length <= 0 || sehir.Length <= 0 || semt.Length <= 0)
            {
                return RedirectToAction("AddAdress", new { hata = @sotec_web.Resources.Dil.Ekisk_bilgi_girdiniz_lğtfen_tüm_alanları_doldurun });
            }
            else
            {
                if (adres_id == 0)
                    SQL.set("INSERT INTO adresler (kaydeden_kullanici_id, ad, alici, telefon, ulke, sehir, semt, adres, posta_kodu, kullanici_id) VALUES (" + Session["kullanici_id"] + ", '" + ad + "', '" + alici + "', '" + telefon + "', '" + ulke + "', '" + sehir + "', '" + semt + "', '" + adres + "', '" + posta_kodu + "', " + kullanici_id + ")");
                else
                    SQL.set("UPDATE adresler SET ad = '" + ad + "', alici = '" + alici + "', telefon = '" + telefon + "', ulke = '" + ulke + "', sehir = '" + sehir + "', semt = '" + semt + "', adres = '" + adres + "', posta_kodu = '" + posta_kodu + "' WHERE adres_id = " + adres_id);
                return RedirectToAction("Adress");
            }
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult sepeteEkle(int urun_id, int miktar, int[] deger_id)
        {
            List<SepetKalem> sepetKalem = new List<SepetKalem>();

            if (Session["sepetKalem"] != null)
            {
                sepetKalem = (List<SepetKalem>)Session["sepetKalem"];
            }

            SepetKalem sk = new SepetKalem();
            int maxID = sk.findMaxID(sepetKalem);
            sk.sepet_id = maxID + 1;
            sk.urun_id = urun_id;
            sk.miktar = Convert.ToDecimal(miktar);
            sk.deger_id = deger_id;

            sepetKalem.Add(sk);
            Session["sepetKalem"] = sepetKalem;

            return PartialView("_sepetDropDown");
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult sepetSil(int sepet_id)
        {
            List<SepetKalem> sepetKalem = new List<SepetKalem>();

            if (Session["sepetKalem"] != null)
            {
                sepetKalem = (List<SepetKalem>)Session["sepetKalem"];
            }

            sepetKalem.RemoveAll(r => r.sepet_id == sepet_id);
            Session["sepetKalem"] = sepetKalem;

            return PartialView("_sepetDropDown");
        }
        public ActionResult sepetSil2(int id)
        {
            List<SepetKalem> sepetKalem = new List<SepetKalem>();

            if (Session["sepetKalem"] != null)
            {
                sepetKalem = (List<SepetKalem>)Session["sepetKalem"];
            }

            sepetKalem.RemoveAll(r => r.sepet_id == id);
            Session["sepetKalem"] = sepetKalem;

            return RedirectToAction("Cart", new { tepki = 3 });
        }
        public ActionResult sepetTemizle()
        {
            List<SepetKalem> sepetKalem = new List<SepetKalem>();

            if (Session["sepetKalem"] != null)
            {
                sepetKalem = (List<SepetKalem>)Session["sepetKalem"];
            }

            sepetKalem.Clear();
            Session["sepetKalem"] = sepetKalem;

            return RedirectToAction("Cart", new { tepki = 3 });
        }
    }
}