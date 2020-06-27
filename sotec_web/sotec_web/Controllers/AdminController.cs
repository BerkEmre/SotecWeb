using sotec_firma.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace sotec_web.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            if (Session["kullanici_id"] == null)
                return RedirectToAction("Login");

            if (SQL.get("SELECT * FROM kullanicilar WHERE kullanici_id = " + Session["kullanici_id"] + " AND kullanici_tipi_parametre_id IN (1, 5)").Rows.Count <= 0)
                return RedirectToAction("Index", "Home");

            return View();
        }

        #region LOGIN
        public ActionResult Login()
        {
            if (Session["kullanici_id"] != null)
                return RedirectToAction("Index");

            return View();
        }

        public ActionResult kullaniciGiris(string email, string sifre)
        {
            if (email.Length <= 0 || sifre.Length <= 0)
                return RedirectToAction("Login", new { hata = "EMail ve Şifre giriniz!" });

            DataTable dt_kullanici = SQL.get("SELECT * FROM kullanicilar WHERE silindi = 0 AND email = '" + email + "' AND sifre = '" + sifre + "'");
            if (dt_kullanici.Rows.Count > 0)
            {
                Session["kullanici_id"] = dt_kullanici.Rows[0]["kullanici_id"];
                return RedirectToAction("Index");
            }
            else
                return RedirectToAction("Login", new { hata = "Kullanıcı bulunamadı..." });
        }

        public ActionResult kullaniciCikis()
        {
            Session["kullanici_id"] = null;
            return RedirectToAction("Login");
        }
        #endregion

        #region KULLANICILAR
        public ActionResult Kullanici()
        {
            if (Session["kullanici_id"] == null)
                return RedirectToAction("Login");

            return View();
        }

        [HttpPost]
        public ActionResult kullaniciEkle(string ad, string soyad, string telefon, string email, string sifre, int tip_parametre_id)
        {
            if (ad.Length <= 0)
                return RedirectToAction("Kullanici", new { hata = "Ad giriniz!" });
            if (soyad.Length <= 0)
                return RedirectToAction("Kullanici", new { hata = "Soyad giriniz!" });
            if (email.Length <= 0)
                return RedirectToAction("Kullanici", new { hata = "EMail giriniz!" });
            if (sifre.Length <= 0)
                return RedirectToAction("Kullanici", new { hata = "Şifre giriniz!" });

            DataTable dt = SQL.get("SELECT * FROM kullanicilar WHERE silindi = 0 AND email = '" + email + "'");
            if (dt.Rows.Count > 0)
                return RedirectToAction("Kullanicilar", new { hata = "Girdiğiniz E-Mail kullanılmaktadır!" });

            SQL.set("INSERT INTO kullanicilar (kaydeden_kullanici_id, email, sifre, ad, soyad, kullanici_tipi_parametre_id, telefon) VALUES (" + Session["kullanici_id"] + ", '" + email + "', '" + sifre + "', '" + ad + "', '" + soyad + "', " + tip_parametre_id + ", '" + telefon + "')");

            return RedirectToAction("Kullanici", new { tepki = 1 });
        }

        [HttpPost]
        public ActionResult kullaniciDuzenle(int kullanici_id, string ad, string soyad, string telefon, string email, string sifre, int tip_parametre_id)
        {
            if (ad.Length <= 0)
                return RedirectToAction("Kullanici", new { hata = "Ad giriniz!" });
            if (soyad.Length <= 0)
                return RedirectToAction("Kullanici", new { hata = "Soyad giriniz!" });
            if (email.Length <= 0)
                return RedirectToAction("Kullanici", new { hata = "EMail giriniz!" });
            if (sifre.Length <= 0)
                return RedirectToAction("Kullanici", new { hata = "Şifre giriniz!" });

            DataTable dt = SQL.get("SELECT * FROM kullanicilar WHERE silindi = 0 AND kullanici_id != " + kullanici_id + " AND email = '" + email + "'");
            if (dt.Rows.Count > 0)
                return RedirectToAction("Kullanicilar", new { hata = "Girdiğiniz E-Mail kullanılmaktadır!" });

            SQL.set("UPDATE kullanicilar SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), email = '" + email + "', ad = '" + ad + "', soyad = '" + soyad + "', kullanici_tipi_parametre_id = " + tip_parametre_id + ", telefon = '" + telefon + "', sifre = '" + sifre + "' WHERE kullanici_id = " + kullanici_id);

            return RedirectToAction("Kullanici", new { tepki = 2 });
        }

        public ActionResult kullaniciSil(int kullanici_id)
        {
            SQL.set("UPDATE kullanicilar SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), silindi = 1 WHERE kullanici_id = " + kullanici_id);
            return RedirectToAction("Kullanici", new { tepki = 3 });
        }
        #endregion

        #region SLIDER
        public ActionResult Slider(int id)
        {
            if (Session["kullanici_id"] == null)
                return RedirectToAction("Login");

            ViewBag.dil_id = id;

            return View();
        }

        [HttpPost]
        public ActionResult sliderGuncelle(int[] slider_id, IEnumerable<HttpPostedFileBase> resim, string[] baslik, string[] yazi, string[] alt_yazi, string[] buton, string[] link, int dil_id)
        {
            string result = "";
            DataTable dt_resimler = SQL.get("SELECT * FROM slider WHERE silindi = 0");

            for (int i = 0; i < slider_id.Length; i++)
            {
                if (slider_id[i] == 0)
                {
                    result = "";
                    if (resim.ElementAt(i) != null && resim.ElementAt(i).ContentLength > 0)
                    {
                        result = string.Format(@"{0}", Guid.NewGuid());
                        WebImage img = new WebImage(resim.ElementAt(i).InputStream);
                        var path = Path.Combine(Server.MapPath("~/admin_src/images/sliders/orjinal"), result);
                        img.Save(path);
                        img.Resize(2000, 2000, true, false);
                        path = Path.Combine(Server.MapPath("~/admin_src/images/sliders/buyuk"), result);
                        img.Save(path);
                        img.Resize(1000, 1000, true, false);
                        path = Path.Combine(Server.MapPath("~/admin_src/images/sliders/kucuk"), result);
                        img.Save(path);
                        result += Path.GetExtension(resim.ElementAt(i).FileName);
                        result = result.Replace("jpg", "jpeg");
                    }
                    SQL.set("INSERT INTO slider (kaydeden_kullanici_id, resim, baslik, yazi, alt_yazi, buton, link, dil_id) VALUES (" + Session["kullanici_id"] + ", '" + result + "', '" + baslik[i] + "', '" + yazi[i] + "', '" + alt_yazi[i] + "', '" + buton[i] + "', '" + link[i] + "', " + dil_id + ")");
                }
                else
                {
                    result = "";
                    if (resim.ElementAt(i) != null && resim.ElementAt(i).ContentLength > 0)
                    {
                        result = string.Format(@"{0}", Guid.NewGuid());
                        WebImage img = new WebImage(resim.ElementAt(i).InputStream);
                        var path = Path.Combine(Server.MapPath("~/admin_src/images/sliders/orjinal"), result);
                        img.Save(path);
                        img.Resize(2000, 2000, true, false);
                        path = Path.Combine(Server.MapPath("~/admin_src/images/sliders/buyuk"), result);
                        img.Save(path);
                        img.Resize(1000, 1000, true, false);
                        path = Path.Combine(Server.MapPath("~/admin_src/images/sliders/kucuk"), result);
                        img.Save(path);
                        result += Path.GetExtension(resim.ElementAt(i).FileName);
                        result = result.Replace("jpg", "jpeg");
                    }
                    SQL.set("UPDATE slider SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), resim = " + (result.Length <= 0 ? "resim" : "'" + result + "'") + ", baslik = '" + baslik[i] + "', yazi = '" + yazi[i] + "', alt_yazi = '" + alt_yazi[i] + "', buton = '" + buton[i] + "', link = '" + link[i] + "', dil_id = " + dil_id + " WHERE slider_id = " + slider_id[i]);
                }
                for (int j = 0; j < dt_resimler.Rows.Count; j++)
                {
                    if (Convert.ToInt32(dt_resimler.Rows[j]["slider_id"]) == slider_id[i])
                    {
                        dt_resimler.Rows.RemoveAt(j);
                        dt_resimler.AcceptChanges();
                    }
                }
            }

            for (int i = 0; i < dt_resimler.Rows.Count; i++)
            {
                SQL.set("UPDATE slider SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), silindi = 1 WHERE slider_id = " + dt_resimler.Rows[i]["slider_id"]);
            }

            return RedirectToAction("Slider", new { tepki = 1 });
        }
        #endregion

        #region KATEGORİ
        public ActionResult Kategori(int id)
        {
            if (Session["kullanici_id"] == null)
                return RedirectToAction("Login");

            ViewBag.kategori_tipi_parametre_id = id;

            return View();
        }

        [HttpPost]
        public ActionResult kategoriEkle(string kategori, int kategori_tipi_parametre_id, int ust_kategori_id)
        {
            if (kategori.Length <= 0)
                return RedirectToAction("Kategori", new { hata = "Kategori adı giriniz!" });

            SQL.set("INSERT INTO kategoriler (kaydeden_kullanici_id, kategori, kategori_tipi_parametre_id, ust_kategori_id) VALUES (" + Session["kullanici_id"] + ", '" + kategori + "', " + kategori_tipi_parametre_id + ", " + ust_kategori_id + ")");

            return RedirectToAction("Kategori", new { tepki = 1, id = kategori_tipi_parametre_id });
        }

        [HttpPost]
        public ActionResult kategoriDuzenle(int kategori_id, string kategori, int kategori_tipi_parametre_id, int ust_kategori_id)
        {
            if (kategori.Length <= 0)
                return RedirectToAction("Kategori", new { hata = "Kategori adı giriniz!" });

            SQL.set("UPDATE kategoriler SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), kategori = '" + kategori + "', kategori_tipi_parametre_id = " + kategori_tipi_parametre_id + ", ust_kategori_id = " + ust_kategori_id + " WHERE kategori_id = " + kategori_id);

            return RedirectToAction("Kategori", new { tepki = 2, id = kategori_tipi_parametre_id });
        }

        public ActionResult kategoriSil(int kategori_id, int kategori_tipi_parametre_id)
        {
            int cnt = 0;

            DataTable dt = SQL.get("SELECT * FROM bloglar WHERE silindi = 0 AND kategori_id = " + kategori_id);
            cnt += dt.Rows.Count;
            dt = SQL.get("SELECT * FROM haberler WHERE silindi = 0 AND kategori_id = " + kategori_id);
            cnt += dt.Rows.Count;
            dt = SQL.get("SELECT * FROM urunler WHERE silindi = 0 AND kategori_id = " + kategori_id);
            cnt += dt.Rows.Count;

            if (dt.Rows.Count > 0)
                return RedirectToAction("Kategori", new { hata = "Silmek istediğiniz kategoriye bağlı kayıtlar vardır, bu kayıtlar mevcutken kategori silinemez!" });

            SQL.set("UPDATE kategoriler SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), silindi = 1 WHERE kategori_id = " + kategori_id);
            return RedirectToAction("Kategori", new { tepki = 3, id = kategori_tipi_parametre_id });
        }

        [HttpPost]
        public ActionResult kategoriDil(string[] kategori, int[] dil_id, int kategori_id, int kategori_tipi_parametre_id)
        {
            DataTable dt_varmi;
            for (int i = 0; i < dil_id.Length; i++)
            {
                dt_varmi = SQL.get("SELECT * FROM dil_kategoriler WHERE silindi = 0 AND dil_id = " + dil_id[i] + " AND kategori_id = " + kategori_id);
                if(dt_varmi.Rows.Count > 0)
                {
                    SQL.set("UPDATE dil_kategoriler SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), kategori = '" + kategori[i] + "' WHERE dil_id = " + dil_id[i] + " AND kategori_id = " + kategori_id);
                }
                else
                {
                    SQL.set("INSERT INTO dil_kategoriler (kaydeden_kullanici_id, dil_id, kategori_id, kategori) VALUES (" + Session["kullanici_id"] + ", " + dil_id[i] + ", " + kategori_id + ", '" + kategori[i] + "')");
                }
            }
            return RedirectToAction("Kategori", new { tepki = 1, id = kategori_tipi_parametre_id });
        }
        #endregion

        #region BLOG
        public ActionResult Blog()
        {
            if (Session["kullanici_id"] == null)
                return RedirectToAction("Login");

            return View();
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult blogEkle(HttpPostedFileBase gorsel, int blog_kategori_id, string baslik, string yazi)
        {
            if (baslik.Length <= 0)
                return RedirectToAction("Blog", new { hata = "Eksik bilgi girdiniz!" });

            string result = "";
            if (gorsel != null && gorsel.ContentLength > 0)
            {
                result = string.Format(@"{0}", Guid.NewGuid());
                WebImage img = new WebImage(gorsel.InputStream);
                var path = Path.Combine(Server.MapPath("~/admin_src/images/blog/orjinal"), result);
                img.Save(path);
                img.Resize(1600, 1600, true, false);
                path = Path.Combine(Server.MapPath("~/admin_src/images/blog/buyuk"), result);
                img.Save(path);
                img.Resize(400, 400, true, false);
                path = Path.Combine(Server.MapPath("~/admin_src/images/blog/kucuk"), result);
                img.Save(path);
                result += Path.GetExtension(gorsel.FileName);
                result = result.Replace("jpg", "jpeg");
            }

            SQL.set("INSERT INTO bloglar (kaydeden_kullanici_id, kategori_id, baslik, gorsel, yazi) VALUES (" + Session["kullanici_id"] + ", " + blog_kategori_id + ", '" + baslik + "', '" + result + "', '" + yazi + "')");

            return RedirectToAction("Blog", new { tepki = 1 });
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult blogDuzenle(int blog_id, HttpPostedFileBase gorsel, int blog_kategori_id, string baslik, string yazi)
        {
            if (baslik.Length <= 0)
                return RedirectToAction("Blog", new { hata = "Eksik bilgi girdiniz!" });

            string result = "";
            if (gorsel != null && gorsel.ContentLength > 0)
            {
                result = string.Format(@"{0}", Guid.NewGuid());
                WebImage img = new WebImage(gorsel.InputStream);
                var path = Path.Combine(Server.MapPath("~/admin_src/images/blog/orjinal"), result);
                img.Save(path);
                img.Resize(1600, 1600, true, false);
                path = Path.Combine(Server.MapPath("~/admin_src/images/blog/buyuk"), result);
                img.Save(path);
                img.Resize(400, 400, true, false);
                path = Path.Combine(Server.MapPath("~/admin_src/images/blog/kucuk"), result);
                img.Save(path);
                result += Path.GetExtension(gorsel.FileName);
                result = result.Replace("jpg", "jpeg");
            }

            SQL.set("UPDATE bloglar SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), kategori_id = " + blog_kategori_id + ", baslik = '" + baslik + "', yazi = '" + yazi + "' " + (result.Length <= 0 ? "" : ", gorsel = '" + result + "' ") + " WHERE blog_id = " + blog_id);

            return RedirectToAction("Blog", new { tepki = 1 });
        }

        public ActionResult blogSil(int blog_id)
        {
            SQL.set("UPDATE bloglar SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), silindi = 1 WHERE blog_id = " + blog_id);
            return RedirectToAction("Blog", new { tepki = 3 });
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult blogDil(int[] dil_id, int blog_id, string[] baslik, string[] yazi)
        {
            DataTable dt_varmi;
            for (int i = 0; i < dil_id.Length; i++)
            {
                dt_varmi = SQL.get("SELECT * FROM dil_bloglar WHERE silindi = 0 AND dil_id = " + dil_id[i] + " AND blog_id = " + blog_id);
                if (dt_varmi.Rows.Count > 0)
                {
                    SQL.set("UPDATE dil_bloglar SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), baslik = '" + baslik[i] + "', yazi = '" + yazi[i] + "' WHERE dil_id = " + dil_id[i] + " AND blog_id = " + blog_id);
                }
                else
                {
                    SQL.set("INSERT INTO dil_bloglar (kaydeden_kullanici_id, dil_id, blog_id, baslik, yazi) VALUES (" + Session["kullanici_id"] + ", " + dil_id[i] + ", " + blog_id + ", '" + baslik[i] + "', '" + yazi[i] + "')");
                }
            }

            return RedirectToAction("Blog", new { tepki = 1 });
        }
        #endregion

        #region HABER
        public ActionResult Haber()
        {
            if (Session["kullanici_id"] == null)
                return RedirectToAction("Login");

            return View();
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult haberEkle(HttpPostedFileBase gorsel, int haber_kategori_id, string baslik, string yazi)
        {
            if (baslik.Length <= 0)
                return RedirectToAction("Haber", new { hata = "Eksik bilgi girdiniz!" });

            string result = "";
            if (gorsel != null && gorsel.ContentLength > 0)
            {
                result = string.Format(@"{0}", Guid.NewGuid());
                WebImage img = new WebImage(gorsel.InputStream);
                var path = Path.Combine(Server.MapPath("~/admin_src/images/haber/orjinal"), result);
                img.Save(path);
                img.Resize(1600, 1600, true, false);
                path = Path.Combine(Server.MapPath("~/admin_src/images/haber/buyuk"), result);
                img.Save(path);
                img.Resize(400, 400, true, false);
                path = Path.Combine(Server.MapPath("~/admin_src/images/haber/kucuk"), result);
                img.Save(path);
                result += Path.GetExtension(gorsel.FileName);
                result = result.Replace("jpg", "jpeg");
            }

            SQL.set("INSERT INTO haberler (kaydeden_kullanici_id, kategori_id, baslik, gorsel, yazi) VALUES (" + Session["kullanici_id"] + ", " + haber_kategori_id + ", '" + baslik + "', '" + result + "', '" + yazi + "')");

            return RedirectToAction("Haber", new { tepki = 1 });
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult haberDuzenle(int haber_id, HttpPostedFileBase gorsel, int haber_kategori_id, string baslik, string yazi)
        {
            if (baslik.Length <= 0)
                return RedirectToAction("Haber", new { hata = "Eksik bilgi girdiniz!" });

            string result = "";
            if (gorsel != null && gorsel.ContentLength > 0)
            {
                result = string.Format(@"{0}", Guid.NewGuid());
                WebImage img = new WebImage(gorsel.InputStream);
                var path = Path.Combine(Server.MapPath("~/admin_src/images/haber/orjinal"), result);
                img.Save(path);
                img.Resize(1600, 1600, true, false);
                path = Path.Combine(Server.MapPath("~/admin_src/images/haber/buyuk"), result);
                img.Save(path);
                img.Resize(400, 400, true, false);
                path = Path.Combine(Server.MapPath("~/admin_src/images/haber/kucuk"), result);
                img.Save(path);
                result += Path.GetExtension(gorsel.FileName);
                result = result.Replace("jpg", "jpeg");
            }

            SQL.set("UPDATE haberler SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), kategori_id = " + haber_kategori_id + ", baslik = '" + baslik + "', yazi = '" + yazi + "' " + (result.Length <= 0 ? "" : ", gorsel = '" + result + "' ") + " WHERE haber_id = " + haber_id);

            return RedirectToAction("Haber", new { tepki = 1 });
        }

        public ActionResult haberSil(int haber_id)
        {
            SQL.set("UPDATE haberler SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), silindi = 1 WHERE haber_id = " + haber_id);
            return RedirectToAction("Haber", new { tepki = 3 });
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult haberDil(int[] dil_id, int haber_id, string[] baslik, string[] yazi)
        {
            DataTable dt_varmi;
            for (int i = 0; i < dil_id.Length; i++)
            {
                dt_varmi = SQL.get("SELECT * FROM dil_haberler WHERE silindi = 0 AND dil_id = " + dil_id[i] + " AND haber_id = " + haber_id);
                if (dt_varmi.Rows.Count > 0)
                {
                    SQL.set("UPDATE dil_haberler SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), baslik = '" + baslik[i] + "', yazi = '" + yazi[i] + "' WHERE dil_id = " + dil_id[i] + " AND haber_id = " + haber_id);
                }
                else
                {
                    SQL.set("INSERT INTO dil_haberler (kaydeden_kullanici_id, dil_id, haber_id, baslik, yazi) VALUES (" + Session["kullanici_id"] + ", " + dil_id[i] + ", " + haber_id + ", '" + baslik[i] + "', '" + yazi[i] + "')");
                }
            }

            return RedirectToAction("Haber", new { tepki = 1 });
        }
        #endregion

        #region HİZMET
        public ActionResult Hizmet()
        {
            if (Session["kullanici_id"] == null)
                return RedirectToAction("Login");

            return View();
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult hizmetEkle(HttpPostedFileBase gorsel, string baslik, string yazi)
        {
            if (baslik.Length <= 0)
                return RedirectToAction("Hizmet", new { hata = "Eksik bilgi girdiniz!" });

            string result = "";
            if (gorsel != null && gorsel.ContentLength > 0)
            {
                result = string.Format(@"{0}", Guid.NewGuid());
                WebImage img = new WebImage(gorsel.InputStream);
                var path = Path.Combine(Server.MapPath("~/admin_src/images/hizmet/orjinal"), result);
                img.Save(path);
                img.Resize(1600, 1600, true, false);
                path = Path.Combine(Server.MapPath("~/admin_src/images/hizmet/buyuk"), result);
                img.Save(path);
                img.Resize(400, 400, true, false);
                path = Path.Combine(Server.MapPath("~/admin_src/images/hizmet/kucuk"), result);
                img.Save(path);
                result += Path.GetExtension(gorsel.FileName);
                result = result.Replace("jpg", "jpeg");
            }

            SQL.set("INSERT INTO hizmetler (kaydeden_kullanici_id, baslik, gorsel, yazi) VALUES (" + Session["kullanici_id"] + ", '" + baslik + "', '" + result + "', '" + yazi + "')");

            return RedirectToAction("Hizmet", new { tepki = 1 });
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult hizmetDuzenle(int hizmet_id, HttpPostedFileBase gorsel, string baslik, string yazi)
        {
            if (baslik.Length <= 0)
                return RedirectToAction("Hizmet", new { hata = "Eksik bilgi girdiniz!" });

            string result = "";
            if (gorsel != null && gorsel.ContentLength > 0)
            {
                result = string.Format(@"{0}", Guid.NewGuid());
                WebImage img = new WebImage(gorsel.InputStream);
                var path = Path.Combine(Server.MapPath("~/admin_src/images/hizmet/orjinal"), result);
                img.Save(path);
                img.Resize(1600, 1600, true, false);
                path = Path.Combine(Server.MapPath("~/admin_src/images/hizmet/buyuk"), result);
                img.Save(path);
                img.Resize(400, 400, true, false);
                path = Path.Combine(Server.MapPath("~/admin_src/images/hizmet/kucuk"), result);
                img.Save(path);
                result += Path.GetExtension(gorsel.FileName);
                result = result.Replace("jpg", "jpeg");
            }

            SQL.set("UPDATE hizmetler SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), baslik = '" + baslik + "', yazi = '" + yazi + "' " + (result.Length <= 0 ? "" : ", gorsel = '" + result + "' ") + " WHERE hizmet_id = " + hizmet_id);

            return RedirectToAction("Hizmet", new { tepki = 1 });
        }

        public ActionResult hizmetSil(int hizmet_id)
        {
            SQL.set("UPDATE hizmetler SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), silindi = 1 WHERE hizmet_id = " + hizmet_id);
            return RedirectToAction("Hizmet", new { tepki = 3 });
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult hizmetDil(int[] dil_id, int hizmet_id, string[] baslik, string[] yazi)
        {
            DataTable dt_varmi;
            for (int i = 0; i < dil_id.Length; i++)
            {
                dt_varmi = SQL.get("SELECT * FROM dil_hizmetler WHERE silindi = 0 AND dil_id = " + dil_id[i] + " AND hizmet_id = " + hizmet_id);
                if (dt_varmi.Rows.Count > 0)
                {
                    SQL.set("UPDATE dil_hizmetler SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), baslik = '" + baslik[i] + "', yazi = '" + yazi[i] + "' WHERE dil_id = " + dil_id[i] + " AND hizmet_id = " + hizmet_id);
                }
                else
                {
                    SQL.set("INSERT INTO dil_hizmetler (kaydeden_kullanici_id, dil_id, hizmet_id, baslik, yazi) VALUES (" + Session["kullanici_id"] + ", " + dil_id[i] + ", " + hizmet_id + ", '" + baslik[i] + "', '" + yazi[i] + "')");
                }
            }

            return RedirectToAction("Hizmet", new { tepki = 1 });
        }
        #endregion

        #region MESAJ
        public ActionResult Mesaj()
        {
            if (Session["kullanici_id"] == null)
                return RedirectToAction("Login");

            return View();
        }

        public ActionResult mesajSil(int mesaj_id)
        {
            if (Session["kullanici_id"] == null)
                return RedirectToAction("Login");

            SQL.set("UPDATE mesajlar SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), silindi = 1 WHERE mesaj_id = " + mesaj_id);
            return RedirectToAction("Mesaj", new { tepki = 3 });
        }
        #endregion

        #region SİTEBİLGİLERİ
        public ActionResult SiteBilgileri(int id)
        {
            if (Session["kullanici_id"] == null)
                return RedirectToAction("Login");

            ViewBag.dil_id = id;

            return View();
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult siteBilgiGuncelle(HttpPostedFileBase logo, string site_adi, string slogan, string hakkimizda, string adres, string telefon, string gsm, string fax, string email, string calisma_saatleri, string facebook, string twitter, string instagram, string youtube, string linkedin, string whatsapp, string seo_aciklama, string seo_anahtar_kelimeler, string uyelik_sozlesmesi, string kullanim_sartlari, string mesafeli_satis_sozlesmesi, string gizlilik_politikasi, string sik_sorulan_sorular, int dil_id)
        {
            string result = "";
            if (logo != null && logo.ContentLength > 0)
            {
                result = string.Format(@"{0}", Guid.NewGuid());
                WebImage img = new WebImage(logo.InputStream);
                var path = Path.Combine(Server.MapPath("~/admin_src/images/site_logo/orjinal"), result);
                img.Save(path);
                img.Resize(100, 50, true, false);
                path = Path.Combine(Server.MapPath("~/admin_src/images/site_logo/"), result);
                img.Save(path);
                result += Path.GetExtension(logo.FileName);
                result = result.Replace("jpg", "jpeg");
            }
            string sql = "UPDATE site_bilgileri SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), logo = " + (result.Length <= 0 ? "logo" : "'" + result + "'") + ", site_adi = '" + site_adi + "', slogan = '" + slogan + "', " +
                " hakkimizda = '" + hakkimizda + "', adres = '" + adres + "', telefon = '" + telefon + "', gsm = '" + gsm + "', fax = '" + fax + "', whatsapp = '" + whatsapp + "', email = '" + email + "', calisma_saatleri = '" + calisma_saatleri + "', facebook = '" + facebook + "', " +
                " instagram = '" + instagram + "', twitter = '" + twitter + "', youtube = '" + youtube + "', linkedin = '" + linkedin + "', seo_aciklama = '" + seo_aciklama + "', seo_anahtar_kelimeler = '" + seo_anahtar_kelimeler + "', uyelik_sozlesmesi = '" + uyelik_sozlesmesi + "', " +
                " kullanim_sartlari = '" + kullanim_sartlari + "', mesafeli_satis_sozlesmesi = '" + mesafeli_satis_sozlesmesi + "', gizlilik_politikasi = '" + gizlilik_politikasi + "', sik_sorulan_sorular = '" + sik_sorulan_sorular + "' WHERE dil_id = " + dil_id;

            SQL.set(sql);

            return RedirectToAction("SiteBilgileri", new { tepki = 1 });
        }
        #endregion

        #region DİL
        public ActionResult Dil()
        {
            if (Session["kullanici_id"] == null)
                return RedirectToAction("Login");

            return View();
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult dilEkle(HttpPostedFileBase ikon, string dil, string kisa_kod, int dil_durumu_parametre_id)
        {
            if (dil.Length <= 0)
                return RedirectToAction("Dil", new { hata = "Eksik bilgi girdiniz!" });
            if (kisa_kod.Length <= 0)
                return RedirectToAction("Dil", new { hata = "Eksik bilgi girdiniz!" });

            string result = "";
            if (ikon != null && ikon.ContentLength > 0)
            {
                result = string.Format(@"{0}", Guid.NewGuid());
                WebImage img = new WebImage(ikon.InputStream);
                var path = Path.Combine(Server.MapPath("~/admin_src/images/dil/orjinal"), result);
                img.Save(path);
                img.Resize(250, 250, true, false);
                path = Path.Combine(Server.MapPath("~/admin_src/images/dil/buyuk"), result);
                img.Save(path);
                img.Resize(40, 40, true, false);
                path = Path.Combine(Server.MapPath("~/admin_src/images/dil/kucuk"), result);
                img.Save(path);
                result += Path.GetExtension(ikon.FileName);
                result = result.Replace("jpg", "jpeg");
            }

            DataTable dt_yeni_dil = SQL.get("INSERT INTO diller (kaydeden_kullanici_id, dil, icon, kisa_kod, dil_durumu_parametre_id) VALUES (" + Session["kullanici_id"] + ", '" + dil + "', '" + result + "', '" + kisa_kod + "', " + dil_durumu_parametre_id + "); SELECT SCOPE_IDENTITY();");
            SQL.set("INSERT INTO site_bilgileri ([logo],[site_adi],[slogan],[hakkimizda],[adres],[telefon],[gsm],[fax],[whatsapp],[email],[calisma_saatleri],[facebook],[instagram],[twitter],[youtube],[linkedin],[seo_aciklama],[seo_anahtar_kelimeler],[uyelik_sozlesmesi],[kullanim_sartlari],[mesafeli_satis_sozlesmesi],[gizlilik_politikasi],[sik_sorulan_sorular],[dil_id]) VALUES ('','','','','','','','','','','','','','','','','','','','','','',''," + dt_yeni_dil.Rows[0][0] + ")");

            return RedirectToAction("Dil", new { tepki = 1 });
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult dilDuzenle(int dil_id, HttpPostedFileBase ikon, string dil, string kisa_kod, int dil_durumu_parametre_id)
        {
            if (dil.Length <= 0)
                return RedirectToAction("Hizmet", new { hata = "Eksik bilgi girdiniz!" });
            if (kisa_kod.Length <= 0)
                return RedirectToAction("Hizmet", new { hata = "Eksik bilgi girdiniz!" });

            string result = "";
            if (ikon != null && ikon.ContentLength > 0)
            {
                result = string.Format(@"{0}", Guid.NewGuid());
                WebImage img = new WebImage(ikon.InputStream);
                var path = Path.Combine(Server.MapPath("~/admin_src/images/dil/orjinal"), result);
                img.Save(path);
                img.Resize(250, 250, true, false);
                path = Path.Combine(Server.MapPath("~/admin_src/images/dil/buyuk"), result);
                img.Save(path);
                img.Resize(40, 40, true, false);
                path = Path.Combine(Server.MapPath("~/admin_src/images/dil/kucuk"), result);
                img.Save(path);
                result += Path.GetExtension(ikon.FileName);
                result = result.Replace("jpg", "jpeg");
            }

            SQL.set("UPDATE diller SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), dil = '" + dil + "', kisa_kod = '" + kisa_kod + "' " + (result.Length <= 0 ? "" : ", icon = '" + result + "' ") + ", dil_durumu_parametre_id = " + dil_durumu_parametre_id + " WHERE dil_id = " + dil_id);

            return RedirectToAction("Dil", new { tepki = 1 });
        }

        public ActionResult dilSil(int dil_id)
        {
            SQL.set("UPDATE diller SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), silindi = 1 WHERE varsayilan = 0 AND dil_id = " + dil_id);
            return RedirectToAction("Dil", new { tepki = 3 });
        }
        #endregion

        #region ÜRÜN
        public ActionResult Urun()
        {
            if (Session["kullanici_id"] == null)
                return RedirectToAction("Login");

            return View();
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult urunEkle(HttpPostedFileBase gorsel, int urun_kategori_id, string urun_adi, string aciklama, decimal fiyat)
        {
            if (urun_adi.Length <= 0)
                return RedirectToAction("Ürün", new { hata = "Eksik bilgi girdiniz!" });

            string result = "";
            if (gorsel != null && gorsel.ContentLength > 0)
            {
                result = string.Format(@"{0}", Guid.NewGuid());
                WebImage img = new WebImage(gorsel.InputStream);
                var path = Path.Combine(Server.MapPath("~/admin_src/images/urun/orjinal"), result);
                img.Save(path);
                img.Resize(1600, 1600, true, false);
                path = Path.Combine(Server.MapPath("~/admin_src/images/urun/buyuk"), result);
                img.Save(path);
                img.Resize(400, 400, true, false);
                path = Path.Combine(Server.MapPath("~/admin_src/images/urun/kucuk"), result);
                img.Save(path);
                result += Path.GetExtension(gorsel.FileName);
                result = result.Replace("jpg", "jpeg");
            }

            SQL.set("INSERT INTO urunler (kaydeden_kullanici_id, kategori_id, urun_adi, gorsel, aciklama, fiyat) VALUES (" + Session["kullanici_id"] + ", " + urun_kategori_id + ", '" + urun_adi + "', '" + result + "', '" + aciklama + "', " + fiyat.ToString().Replace(',', '.') + ")");

            return RedirectToAction("Urun", new { tepki = 1 });
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult urunDuzenle(int urun_id, HttpPostedFileBase gorsel, int urun_kategori_id, string urun_adi, string aciklama, decimal fiyat)
        {
            if (urun_adi.Length <= 0)
                return RedirectToAction("Ürün", new { hata = "Eksik bilgi girdiniz!" });

            string result = "";
            if (gorsel != null && gorsel.ContentLength > 0)
            {
                result = string.Format(@"{0}", Guid.NewGuid());
                WebImage img = new WebImage(gorsel.InputStream);
                var path = Path.Combine(Server.MapPath("~/admin_src/images/urun/orjinal"), result);
                img.Save(path);
                img.Resize(1600, 1600, true, false);
                path = Path.Combine(Server.MapPath("~/admin_src/images/urun/buyuk"), result);
                img.Save(path);
                img.Resize(400, 400, true, false);
                path = Path.Combine(Server.MapPath("~/admin_src/images/urun/kucuk"), result);
                img.Save(path);
                result += Path.GetExtension(gorsel.FileName);
                result = result.Replace("jpg", "jpeg");
            }

            SQL.set("UPDATE urunler SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), kategori_id = " + urun_kategori_id + ", urun_adi = '" + urun_adi + "', aciklama = '" + aciklama + "' " + (result.Length <= 0 ? "" : ", gorsel = '" + result + "' ") + ", fiyat = " + fiyat.ToString().Replace(',', '.') + " WHERE urun_id = " + urun_id);

            return RedirectToAction("Urun", new { tepki = 1 });
        }

        public ActionResult urunSil(int urun_id)
        {
            SQL.set("UPDATE urunler SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), silindi = 1 WHERE urun_id = " + urun_id);
            return RedirectToAction("Urun", new { tepki = 3 });
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult urunDil(int[] dil_id, int urun_id, string[] urun_adi, string[] aciklama)
        {
            DataTable dt_varmi;
            for (int i = 0; i < dil_id.Length; i++)
            {
                dt_varmi = SQL.get("SELECT * FROM dil_urunler WHERE silindi = 0 AND dil_id = " + dil_id[i] + " AND urun_id = " + urun_id);
                if (dt_varmi.Rows.Count > 0)
                {
                    SQL.set("UPDATE dil_urunler SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), urun_adi = '" + urun_adi[i] + "', aciklama = '" + aciklama[i] + "' WHERE dil_id = " + dil_id[i] + " AND urun_id = " + urun_id);
                }
                else
                {
                    SQL.set("INSERT INTO dil_urunler (kaydeden_kullanici_id, dil_id, urun_id, urun_adi, aciklama) VALUES (" + Session["kullanici_id"] + ", " + dil_id[i] + ", " + urun_id + ", '" + urun_adi[i] + "', '" + aciklama[i] + "')");
                }
            }

            return RedirectToAction("Urun", new { tepki = 1 });
        }
        #endregion
    }
}
