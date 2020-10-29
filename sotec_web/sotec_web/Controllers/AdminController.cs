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
        public ActionResult ComingSoon()
        {
            return RedirectToAction("Index", "Admin");
        }

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
                        var path = Path.Combine("~/admin_src/images/sliders/orjinal", result);
                        img.Save(path);
                        img.Resize(2000, 2000, true, false).Crop(1, 1, 1, 1);
                        path = Path.Combine("~/admin_src/images/sliders/buyuk", result);
                        img.Save(path);
                        img.Resize(1000, 1000, true, false).Crop(1, 1, 1, 1);
                        path = Path.Combine("~/admin_src/images/sliders/kucuk", result);
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
                        var path = Path.Combine("~/admin_src/images/sliders/orjinal", result);
                        img.Save(path);
                        img.Resize(2000, 2000, true, false).Crop(1, 1, 1, 1);
                        path = Path.Combine("~/admin_src/images/sliders/buyuk", result);
                        img.Save(path);
                        img.Resize(1000, 1000, true, false).Crop(1, 1, 1, 1);
                        path = Path.Combine("~/admin_src/images/sliders/kucuk", result);
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
                SQL.set("UPDATE slider SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), silindi = 1 WHERE dil_id = " + dil_id + " AND slider_id = " + dt_resimler.Rows[i]["slider_id"]);
            }

            return RedirectToAction("Slider", new { id = dil_id, tepki = 1 });
        }
        #endregion

        #region KARO
        public ActionResult Karo(int id)
        {
            if (Session["kullanici_id"] == null)
                return RedirectToAction("Login");

            ViewBag.dil_id = id;

            return View();
        }

        [HttpPost]
        public ActionResult karoGuncelle(int[] karo_id, IEnumerable<HttpPostedFileBase> resim, string[] buton, string[] link, int dil_id)
        {
            string result = "";
            DataTable dt_resimler = SQL.get("SELECT * FROM karolar WHERE silindi = 0");

            for (int i = 0; i < karo_id.Length; i++)
            {
                if (karo_id[i] == 0)
                {
                    result = "";
                    if (resim.ElementAt(i) != null && resim.ElementAt(i).ContentLength > 0)
                    {
                        result = string.Format(@"{0}", Guid.NewGuid());
                        WebImage img = new WebImage(resim.ElementAt(i).InputStream);
                        var path = Path.Combine("~/admin_src/images/karo/orjinal", result);
                        img.Save(path);
                        img.Resize(2000, 2000, true, false).Crop(1, 1, 1, 1);
                        path = Path.Combine("~/admin_src/images/karo/buyuk", result);
                        img.Save(path);
                        img.Resize(1000, 1000, true, false).Crop(1, 1, 1, 1);
                        path = Path.Combine("~/admin_src/images/karo/kucuk", result);
                        img.Save(path);
                        result += Path.GetExtension(resim.ElementAt(i).FileName);
                        result = result.Replace("jpg", "jpeg");
                    }
                    SQL.set("INSERT INTO karolar (kaydeden_kullanici_id, resim, buton, link, dil_id) VALUES (" + Session["kullanici_id"] + ", '" + result + "', '" + buton[i] + "', '" + link[i] + "', " + dil_id + ")");
                }
                else
                {
                    result = "";
                    if (resim.ElementAt(i) != null && resim.ElementAt(i).ContentLength > 0)
                    {
                        result = string.Format(@"{0}", Guid.NewGuid());
                        WebImage img = new WebImage(resim.ElementAt(i).InputStream);
                        var path = Path.Combine("~/admin_src/images/karo/orjinal", result);
                        img.Save(path);
                        img.Resize(2000, 2000, true, false).Crop(1, 1, 1, 1);
                        path = Path.Combine("~/admin_src/images/karo/buyuk", result);
                        img.Save(path);
                        img.Resize(1000, 1000, true, false).Crop(1, 1, 1, 1);
                        path = Path.Combine("~/admin_src/images/karo/kucuk", result);
                        img.Save(path);
                        result += Path.GetExtension(resim.ElementAt(i).FileName);
                        result = result.Replace("jpg", "jpeg");
                    }
                    SQL.set("UPDATE karolar SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), resim = " + (result.Length <= 0 ? "resim" : "'" + result + "'") + ", buton = '" + buton[i] + "', link = '" + link[i] + "', dil_id = " + dil_id + " WHERE karo_id = " + karo_id[i]);
                }
                for (int j = 0; j < dt_resimler.Rows.Count; j++)
                {
                    if (Convert.ToInt32(dt_resimler.Rows[j]["karo_id"]) == karo_id[i])
                    {
                        dt_resimler.Rows.RemoveAt(j);
                        dt_resimler.AcceptChanges();
                    }
                }
            }

            for (int i = 0; i < dt_resimler.Rows.Count; i++)
            {
                SQL.set("UPDATE karolar SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), silindi = 1 WHERE karo_id = " + dt_resimler.Rows[i]["karo_id"]);
            }

            return RedirectToAction("Karo", new { id = dil_id, tepki = 1 });
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
            dt = SQL.get("SELECT * FROM projeler WHERE silindi = 0 AND kategori_id = " + kategori_id);
            cnt += dt.Rows.Count;
            dt = SQL.get("SELECT * FROM urun_kategorileri WHERE silindi = 0 AND kategori_id = " + kategori_id);
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
                if (dt_varmi.Rows.Count > 0)
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
                var path = Path.Combine("~/admin_src/images/blog/orjinal", result);
                img.Save(path);
                img.Resize(1600, 1600, true, false).Crop(1, 1, 1, 1);
                path = Path.Combine("~/admin_src/images/blog/buyuk", result);
                img.Save(path);
                img.Resize(400, 400, true, false).Crop(1, 1, 1, 1);
                path = Path.Combine("~/admin_src/images/blog/kucuk", result);
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
                var path = Path.Combine("~/admin_src/images/blog/orjinal", result);
                img.Save(path);
                img.Resize(1600, 1600, true, false).Crop(1, 1, 1, 1);
                path = Path.Combine("~/admin_src/images/blog/buyuk", result);
                img.Save(path);
                img.Resize(400, 400, true, false).Crop(1, 1, 1, 1);
                path = Path.Combine("~/admin_src/images/blog/kucuk", result);
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

        #region PROJE
        public ActionResult Proje()
        {
            if (Session["kullanici_id"] == null)
                return RedirectToAction("Login");

            return View();
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult projeEkle(HttpPostedFileBase gorsel, int proje_kategori_id, string baslik, string yazi)
        {
            if (baslik.Length <= 0)
                return RedirectToAction("Proje", new { hata = "Eksik bilgi girdiniz!" });

            string result = "";
            if (gorsel != null && gorsel.ContentLength > 0)
            {
                result = string.Format(@"{0}", Guid.NewGuid());
                WebImage img = new WebImage(gorsel.InputStream);
                var path = Path.Combine("~/admin_src/images/proje/orjinal", result);
                img.Save(path);
                img.Resize(1600, 1600, true, false).Crop(1, 1, 1, 1);
                path = Path.Combine("~/admin_src/images/proje/buyuk", result);
                img.Save(path);
                img.Resize(400, 400, true, false).Crop(1, 1, 1, 1);
                path = Path.Combine("~/admin_src/images/proje/kucuk", result);
                img.Save(path);
                result += Path.GetExtension(gorsel.FileName);
                result = result.Replace("jpg", "jpeg");
            }

            SQL.set("INSERT INTO projeler (kaydeden_kullanici_id, kategori_id, baslik, gorsel, yazi) VALUES (" + Session["kullanici_id"] + ", " + proje_kategori_id + ", '" + baslik + "', '" + result + "', '" + yazi + "')");

            return RedirectToAction("Proje", new { tepki = 1 });
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult projeDuzenle(int proje_id, HttpPostedFileBase gorsel, int proje_kategori_id, string baslik, string yazi)
        {
            if (baslik.Length <= 0)
                return RedirectToAction("Proje", new { hata = "Eksik bilgi girdiniz!" });

            string result = "";
            if (gorsel != null && gorsel.ContentLength > 0)
            {
                result = string.Format(@"{0}", Guid.NewGuid());
                WebImage img = new WebImage(gorsel.InputStream);
                var path = Path.Combine("~/admin_src/images/proje/orjinal", result);
                img.Save(path);
                img.Resize(1600, 1600, true, false).Crop(1, 1, 1, 1);
                path = Path.Combine("~/admin_src/images/proje/buyuk", result);
                img.Save(path);
                img.Resize(400, 400, true, false).Crop(1, 1, 1, 1);
                path = Path.Combine("~/admin_src/images/proje/kucuk", result);
                img.Save(path);
                result += Path.GetExtension(gorsel.FileName);
                result = result.Replace("jpg", "jpeg");
            }

            SQL.set("UPDATE projeler SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), kategori_id = " + proje_kategori_id + ", baslik = '" + baslik + "', yazi = '" + yazi + "' " + (result.Length <= 0 ? "" : ", gorsel = '" + result + "' ") + " WHERE proje_id = " + proje_id);

            return RedirectToAction("Proje", new { tepki = 1 });
        }

        public ActionResult projeSil(int proje_id)
        {
            SQL.set("UPDATE projeler SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), silindi = 1 WHERE proje_id = " + proje_id);
            return RedirectToAction("Proje", new { tepki = 3 });
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult projeDil(int[] dil_id, int proje_id, string[] baslik, string[] yazi)
        {
            DataTable dt_varmi;
            for (int i = 0; i < dil_id.Length; i++)
            {
                dt_varmi = SQL.get("SELECT * FROM dil_projeler WHERE silindi = 0 AND dil_id = " + dil_id[i] + " AND proje_id = " + proje_id);
                if (dt_varmi.Rows.Count > 0)
                {
                    SQL.set("UPDATE dil_projeler SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), baslik = '" + baslik[i] + "', yazi = '" + yazi[i] + "' WHERE dil_id = " + dil_id[i] + " AND proje_id = " + proje_id);
                }
                else
                {
                    SQL.set("INSERT INTO dil_projeler (kaydeden_kullanici_id, dil_id, proje_id, baslik, yazi) VALUES (" + Session["kullanici_id"] + ", " + dil_id[i] + ", " + proje_id + ", '" + baslik[i] + "', '" + yazi[i] + "')");
                }
            }

            return RedirectToAction("Proje", new { tepki = 1 });
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
                var path = Path.Combine("~/admin_src/images/haber/orjinal", result);
                img.Save(path);
                img.Resize(1600, 1600, true, false).Crop(1, 1, 1, 1);
                path = Path.Combine("~/admin_src/images/haber/buyuk", result);
                img.Save(path);
                img.Resize(400, 400, true, false).Crop(1, 1, 1, 1);
                path = Path.Combine("~/admin_src/images/haber/kucuk", result);
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
                var path = Path.Combine("~/admin_src/images/haber/orjinal", result);
                img.Save(path);
                img.Resize(1600, 1600, true, false).Crop(1, 1, 1, 1);
                path = Path.Combine("~/admin_src/images/haber/buyuk", result);
                img.Save(path);
                img.Resize(400, 400, true, false).Crop(1, 1, 1, 1);
                path = Path.Combine("~/admin_src/images/haber/kucuk", result);
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
                var path = Path.Combine("~/admin_src/images/hizmet/orjinal", result);
                img.Save(path);
                img.Resize(1600, 1600, true, false).Crop(1, 1, 1, 1);
                path = Path.Combine("~/admin_src/images/hizmet/buyuk", result);
                img.Save(path);
                img.Resize(400, 400, true, false).Crop(1, 1, 1, 1);
                path = Path.Combine("~/admin_src/images/hizmet/kucuk", result);
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
                var path = Path.Combine("~/admin_src/images/hizmet/orjinal", result);
                img.Save(path);
                img.Resize(1600, 1600, true, false).Crop(1, 1, 1, 1);
                path = Path.Combine("~/admin_src/images/hizmet/buyuk", result);
                img.Save(path);
                img.Resize(400, 400, true, false).Crop(1, 1, 1, 1);
                path = Path.Combine("~/admin_src/images/hizmet/kucuk", result);
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

        #region REFERANS
        public ActionResult Referans()
        {
            if (Session["kullanici_id"] == null)
                return RedirectToAction("Login");

            return View();
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult referansEkle(HttpPostedFileBase gorsel, string baslik, string yazi)
        {
            if (baslik.Length <= 0)
                return RedirectToAction("Referans", new { hata = "Eksik bilgi girdiniz!" });

            string result = "";
            if (gorsel != null && gorsel.ContentLength > 0)
            {
                result = string.Format(@"{0}", Guid.NewGuid());
                WebImage img = new WebImage(gorsel.InputStream);
                var path = Path.Combine("~/admin_src/images/referans/orjinal", result);
                img.Save(path);
                img.Resize(1600, 1600, true, false).Crop(1, 1, 1, 1);
                path = Path.Combine("~/admin_src/images/referans/buyuk", result);
                img.Save(path);
                img.Resize(400, 400, true, false).Crop(1, 1, 1, 1);
                path = Path.Combine("~/admin_src/images/referans/kucuk", result);
                img.Save(path);
                result += Path.GetExtension(gorsel.FileName);
                result = result.Replace("jpg", "jpeg");
            }

            SQL.set("INSERT INTO referanslar (kaydeden_kullanici_id, baslik, gorsel, yazi) VALUES (" + Session["kullanici_id"] + ", '" + baslik + "', '" + result + "', '" + yazi + "')");

            return RedirectToAction("Referans", new { tepki = 1 });
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult referansDuzenle(int referans_id, HttpPostedFileBase gorsel, string baslik, string yazi)
        {
            if (baslik.Length <= 0)
                return RedirectToAction("Referans", new { hata = "Eksik bilgi girdiniz!" });

            string result = "";
            if (gorsel != null && gorsel.ContentLength > 0)
            {
                result = string.Format(@"{0}", Guid.NewGuid());
                WebImage img = new WebImage(gorsel.InputStream);
                var path = Path.Combine("~/admin_src/images/referans/orjinal", result);
                img.Save(path);
                img.Resize(1600, 1600, true, false).Crop(1, 1, 1, 1);
                path = Path.Combine("~/admin_src/images/referans/buyuk", result);
                img.Save(path);
                img.Resize(400, 400, true, false).Crop(1, 1, 1, 1);
                path = Path.Combine("~/admin_src/images/referans/kucuk", result);
                img.Save(path);
                result += Path.GetExtension(gorsel.FileName);
                result = result.Replace("jpg", "jpeg");
            }

            SQL.set("UPDATE referanslar SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), baslik = '" + baslik + "', yazi = '" + yazi + "' " + (result.Length <= 0 ? "" : ", gorsel = '" + result + "' ") + " WHERE referans_id = " + referans_id);

            return RedirectToAction("Referans", new { tepki = 1 });
        }

        public ActionResult referansSil(int referans_id)
        {
            SQL.set("UPDATE referanslar SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), silindi = 1 WHERE referans_id = " + referans_id);
            return RedirectToAction("Referans", new { tepki = 3 });
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult referansDil(int[] dil_id, int referans_id, string[] baslik, string[] yazi)
        {
            DataTable dt_varmi;
            for (int i = 0; i < dil_id.Length; i++)
            {
                dt_varmi = SQL.get("SELECT * FROM dil_referanslar WHERE silindi = 0 AND dil_id = " + dil_id[i] + " AND referans_id = " + referans_id);
                if (dt_varmi.Rows.Count > 0)
                {
                    SQL.set("UPDATE dil_referanslar SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), baslik = '" + baslik[i] + "', yazi = '" + yazi[i] + "' WHERE dil_id = " + dil_id[i] + " AND referans_id = " + referans_id);
                }
                else
                {
                    SQL.set("INSERT INTO dil_referanslar (kaydeden_kullanici_id, dil_id, referans_id, baslik, yazi) VALUES (" + Session["kullanici_id"] + ", " + dil_id[i] + ", " + referans_id + ", '" + baslik[i] + "', '" + yazi[i] + "')");
                }
            }

            return RedirectToAction("Referans", new { tepki = 1 });
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
        public ActionResult siteBilgiGuncelle(HttpPostedFileBase logo, string site_adi, string slogan, string hakkimizda, string adres, string telefon, string gsm, string fax, string email, string calisma_saatleri, string facebook, string twitter, string instagram, string youtube, string linkedin, string whatsapp, string seo_aciklama, string seo_anahtar_kelimeler, string uyelik_sozlesmesi, string kullanim_sartlari, string mesafeli_satis_sozlesmesi, string gizlilik_politikasi, string sik_sorulan_sorular, int dil_id, string iban)
        {
            string result = "";
            if (logo != null && logo.ContentLength > 0)
            {
                result = string.Format(@"{0}", Guid.NewGuid());
                WebImage img = new WebImage(logo.InputStream);
                var path = Path.Combine("~/admin_src/images/site_logo/orjinal", result);
                img.Save(path);
                img.Resize(150, 75, true, false).Crop(1, 1, 1, 1);
                path = Path.Combine("~/admin_src/images/site_logo/", result);
                img.Save(path);
                result += Path.GetExtension(logo.FileName);
                result = result.Replace("jpg", "jpeg");
            }
            string sql = "UPDATE site_bilgileri SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), logo = " + (result.Length <= 0 ? "logo" : "'" + result + "'") + ", site_adi = '" + site_adi + "', slogan = '" + slogan + "', " +
                " hakkimizda = '" + hakkimizda + "', adres = '" + adres + "', telefon = '" + telefon + "', gsm = '" + gsm + "', fax = '" + fax + "', whatsapp = '" + whatsapp + "', email = '" + email + "', calisma_saatleri = '" + calisma_saatleri + "', facebook = '" + facebook + "', " +
                " instagram = '" + instagram + "', twitter = '" + twitter + "', youtube = '" + youtube + "', linkedin = '" + linkedin + "', seo_aciklama = '" + seo_aciklama + "', seo_anahtar_kelimeler = '" + seo_anahtar_kelimeler + "', uyelik_sozlesmesi = '" + uyelik_sozlesmesi + "', " +
                " kullanim_sartlari = '" + kullanim_sartlari + "', mesafeli_satis_sozlesmesi = '" + mesafeli_satis_sozlesmesi + "', gizlilik_politikasi = '" + gizlilik_politikasi + "', sik_sorulan_sorular = '" + sik_sorulan_sorular + "', iban = '" + iban + "' WHERE dil_id = " + dil_id;

            SQL.set(sql);

            return RedirectToAction("SiteBilgileri", new { id = dil_id, tepki = 1 });
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
                var path = Path.Combine("~/admin_src/images/dil/orjinal", result);
                img.Save(path);
                img.Resize(250, 250, true, false).Crop(1, 1, 1, 1);
                path = Path.Combine("~/admin_src/images/dil/buyuk", result);
                img.Save(path);
                img.Resize(40, 40, true, false).Crop(1, 1, 1, 1);
                path = Path.Combine("~/admin_src/images/dil/kucuk", result);
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
                var path = Path.Combine("~/admin_src/images/dil/orjinal", result);
                img.Save(path);
                img.Resize(250, 250, true, false).Crop(1, 1, 1, 1);
                path = Path.Combine("~/admin_src/images/dil/buyuk", result);
                img.Save(path);
                img.Resize(40, 40, true, false).Crop(1, 1, 1, 1);
                path = Path.Combine("~/admin_src/images/dil/kucuk", result);
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
        public ActionResult urunEkle(
            int[] urun_kategori_id, string barkod, int[] ozellik_id, int[] varyasyon_id, string[] varyasyon_fiyat, string[] varyasyon_stok, string[] resim_path, int[] resim_id, int[] resim_sira,
            string urun_adi = "", string aciklama = "", string stok_kodu = "", string stok = "0", string fiyat = "0", int one_cikan = 0)
        {
            string result = "";
            int yeni_urun_id;
            yeni_urun_id = Convert.ToInt32(SQL.get("INSERT INTO urunler (kaydeden_kullanici_id, urun_adi, aciklama, stok_kodu, barkod, stok, fiyat, one_cikan) VALUES (" + Session["kullanici_id"] + ", '" + urun_adi + "', '" + aciklama + "', '" + stok_kodu + "', '" + barkod + "', " + stok.ToString().Replace(',', '.') + ", " + fiyat.ToString().Replace(',', '.') + ", " + one_cikan + "); SELECT SCOPE_IDENTITY();").Rows[0][0]);

            if (urun_kategori_id != null)
            {
                for (int i = 0; i < urun_kategori_id.Length; i++)
                {
                    SQL.set("INSERT INTO urun_kategorileri (kaydeden_kullanici_id, urun_id, kategori_id) VALUES (" + Session["kullanici_id"] + ", " + yeni_urun_id + ", " + urun_kategori_id[i] + ")");
                }
            }
            if (ozellik_id != null)
            {
                for (int i = 0; i < ozellik_id.Length; i++)
                {
                    SQL.set("INSERT INTO urun_ozellik (kaydeden_kullanici_id, urun_id, deger_id) VALUES (" + Session["kullanici_id"] + ", " + yeni_urun_id + ", " + ozellik_id[i] + ")");
                }
            }
            if (varyasyon_id != null)
            {
                for (int i = 0; i < varyasyon_id.Length; i++)
                {
                    SQL.set("INSERT INTO urun_varyasyon (kaydeden_kullanici_id, urun_id, deger_id, tutar, stok) VALUES (" + Session["kullanici_id"] + ", " + yeni_urun_id + ", " + varyasyon_id[i] + ", " + varyasyon_fiyat[i].ToString().Replace(',', '.') + ", " + varyasyon_stok[i].ToString().Replace(',', '.') + ")");
                }
            }
            if (resim_id != null)
            {
                for (int i = 0; i < resim_id.Length; i++)
                {
                    if (resim_path[i] != null && resim_path[i].Length > 0)
                    {
                        result = string.Format(@"{0}", Guid.NewGuid());
                        byte[] imageBytes = Convert.FromBase64String(resim_path[i].Replace("data:image/jpeg;base64,", ""));
                        WebImage img = new WebImage(imageBytes);
                        var path = Path.Combine("~/admin_src/images/urun/orjinal", result);
                        img.Save(path);
                        img.Resize(1600, 1600, true, false).Crop(1, 1, 1, 1);
                        path = Path.Combine("~/admin_src/images/urun/buyuk", result);
                        img.Save(path);
                        img.Resize(400, 400, true, false).Crop(1, 1, 1, 1);
                        path = Path.Combine("~/admin_src/images/urun/kucuk", result);
                        img.Save(path);
                        result += Path.GetExtension(img.FileName);
                        result = result.Replace("jpg", "jpeg");
                        SQL.set("INSERT INTO urun_resimleri (kaydeden_kullanici_id, urun_id, resim, sira) VALUES (" + Session["kullanici_id"] + ", " + yeni_urun_id + ", '" + result + "', " + resim_sira[i] + ")");
                    }

                }
            }

            return RedirectToAction("Urun", new { tepki = 1 });
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult urunDuzenle(
            int urun_id, int[] urun_kategori_id, string barkod, int[] ozellik_id, int[] varyasyon_id, string[] varyasyon_fiyat, string[] varyasyon_stok, string[] resim_path, int[] resim_id, int[] resim_sira,
            string urun_adi = "", string aciklama = "", string stok_kodu = "", string stok = "", string fiyat = "0", int one_cikan = 0)
        {
            string result = "";
            string resim_ids = "";
            SQL.set("UPDATE urunler SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", urun_adi = '" + urun_adi + "', aciklama = '" + aciklama + "', stok_kodu = '" + stok_kodu + "', barkod = '" + barkod + "', stok = " + stok.ToString().Replace(',', '.') + ", fiyat = " + fiyat.ToString().Replace(',', '.') + ", one_cikan = " + one_cikan + " WHERE urun_id = " + urun_id);

            SQL.set("UPDATE urun_kategorileri SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), silindi = 1 WHERE urun_id = " + urun_id);
            SQL.set("UPDATE urun_ozellik SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), silindi = 1 WHERE urun_id = " + urun_id);
            SQL.set("UPDATE urun_varyasyon SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), silindi = 1 WHERE urun_id = " + urun_id);

            if (urun_kategori_id != null)
            {
                for (int i = 0; i < urun_kategori_id.Length; i++)
                {
                    SQL.set("INSERT INTO urun_kategorileri (kaydeden_kullanici_id, urun_id, kategori_id) VALUES (" + Session["kullanici_id"] + ", " + urun_id + ", " + urun_kategori_id[i] + ")");
                }
            }
            if (ozellik_id != null)
            {
                for (int i = 0; i < ozellik_id.Length; i++)
                {
                    SQL.set("INSERT INTO urun_ozellik (kaydeden_kullanici_id, urun_id, deger_id) VALUES (" + Session["kullanici_id"] + ", " + urun_id + ", " + ozellik_id[i] + ")");
                }
            }
            if (varyasyon_id != null)
            {
                for (int i = 0; i < varyasyon_id.Length; i++)
                {
                    SQL.set("INSERT INTO urun_varyasyon (kaydeden_kullanici_id, urun_id, deger_id, tutar, stok) VALUES (" + Session["kullanici_id"] + ", " + urun_id + ", " + varyasyon_id[i] + ", " + varyasyon_fiyat[i].ToString().Replace(',', '.') + ", " + varyasyon_stok[i].ToString().Replace(',', '.') + ")");
                }
            }
            if (resim_id != null)
            {
                resim_ids = "";
                for (int i = 0; i < resim_id.Length; i++)
                {
                    SQL.set("UPDATE urun_resimleri SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), sira = " + resim_sira[i] + " WHERE urun_resim_id = " + resim_id[i]);
                    resim_ids += resim_id[i] + ",";
                }
                SQL.set("UPDATE urun_resimleri SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), silindi = 1 WHERE urun_id = " + urun_id + " AND urun_resim_id NOT IN (" + resim_ids + "0" + ")");
                for (int i = 0; i < resim_id.Length; i++)
                {
                    if (resim_id[i] == 0)
                    {
                        if (resim_path[i] != null && resim_path[i].Length > 0)
                        {
                            result = string.Format(@"{0}", Guid.NewGuid());
                            byte[] imageBytes = Convert.FromBase64String(resim_path[i].Replace("data:image/jpeg;base64,", ""));
                            WebImage img = new WebImage(imageBytes);
                            var path = Path.Combine("~/admin_src/images/urun/orjinal", result);
                            img.Save(path);
                            img.Resize(1600, 1600, true, false).Crop(1, 1, 1, 1);
                            path = Path.Combine("~/admin_src/images/urun/buyuk", result);
                            img.Save(path);
                            img.Resize(400, 400, true, false).Crop(1, 1, 1, 1);
                            path = Path.Combine("~/admin_src/images/urun/kucuk", result);
                            img.Save(path);
                            result += Path.GetExtension(img.FileName);
                            result = result.Replace("jpg", "jpeg");
                            SQL.set("INSERT INTO urun_resimleri (kaydeden_kullanici_id, urun_id, resim, sira) VALUES (" + Session["kullanici_id"] + ", " + urun_id + ", '" + result + "', " + resim_sira[i] + ")");
                        }
                    }
                }
            }
            else
            {
                SQL.set("UPDATE urun_resimleri SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), silindi = 1 WHERE urun_id = " + urun_id);
            }

            return RedirectToAction("Urun", new { tepki = 2 });
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

        public ActionResult UrunDetay(int id = 0)
        {
            if (Session["kullanici_id"] == null)
                return RedirectToAction("Login");

            ViewBag.urun_id = id;

            return View();
        }
        #endregion

        #region ÜRÜN VARYASYON
        public ActionResult UrunVaryasyon()
        {
            if (Session["kullanici_id"] == null)
                return RedirectToAction("Login");

            return View();
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult varyasyonEkle(string varyasyon)
        {
            if (varyasyon.Length <= 0)
                return RedirectToAction("UrunVaryasyon", new { hata = "Eksik bilgi girdiniz!" });

            SQL.set("INSERT INTO urun_varyasyonlari (kaydeden_kullanici_id, varyasyon) VALUES (" + Session["kullanici_id"] + ", '" + varyasyon + "')");

            return RedirectToAction("UrunVaryasyon", new { tepki = 1 });
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult varyasyonDuzenle(int varyasyon_id, string varyasyon)
        {
            if (varyasyon.Length <= 0)
                return RedirectToAction("UrunVaryasyon", new { hata = "Eksik bilgi girdiniz!" });

            SQL.set("UPDATE urun_varyasyonlari SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), varyasyon = '" + varyasyon + "' WHERE urun_varyasyon_id = " + varyasyon_id);

            return RedirectToAction("UrunVaryasyon", new { tepki = 1 });
        }

        public ActionResult varyasyonSil(int varyasyon_id)
        {
            SQL.set("UPDATE urun_varyasyonlari SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), silindi = 1 WHERE urun_varyasyon_id = " + varyasyon_id);
            return RedirectToAction("UrunVaryasyon", new { tepki = 3 });
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult varyasyonDil(int[] dil_id, int varyasyon_id, string[] varyasyon)
        {
            DataTable dt_varmi;
            for (int i = 0; i < dil_id.Length; i++)
            {
                dt_varmi = SQL.get("SELECT * FROM dil_urun_varyasyonlari WHERE silindi = 0 AND dil_id = " + dil_id[i] + " AND urun_varyasyon_id = " + varyasyon_id);
                if (dt_varmi.Rows.Count > 0)
                {
                    SQL.set("UPDATE dil_urun_varyasyonlari SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), varyasyon = '" + varyasyon[i] + "' WHERE dil_id = " + dil_id[i] + " AND urun_varyasyon_id = " + varyasyon_id);
                }
                else
                {
                    SQL.set("INSERT INTO dil_urun_varyasyonlari (kaydeden_kullanici_id, dil_id, urun_varyasyon_id, varyasyon) VALUES (" + Session["kullanici_id"] + ", " + dil_id[i] + ", " + varyasyon_id + ", '" + varyasyon[i] + "')");
                }
            }

            return RedirectToAction("UrunVaryasyon", new { tepki = 1 });
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult varyasyonDegerEkle(int varyasyon_id, string deger)
        {
            if (deger.Length <= 0)
                return RedirectToAction("UrunVaryasyon", new { hata = "Eksik bilgi girdiniz!" });

            SQL.set("INSERT INTO urun_varyasyon_degerleri (kaydeden_kullanici_id, deger, varyasyon_id) VALUES (" + Session["kullanici_id"] + ", '" + deger + "', " + varyasyon_id + ")");

            return RedirectToAction("UrunVaryasyon", new { tepki = 1 });
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult varyasyonDegerDuzenle(int varyasyon_deger_id, string deger)
        {
            if (deger.Length <= 0)
                return RedirectToAction("UrunVaryasyon", new { hata = "Eksik bilgi girdiniz!" });

            SQL.set("UPDATE urun_varyasyon_degerleri SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), deger = '" + deger + "' WHERE urun_varyasyon_deger_id = " + varyasyon_deger_id);

            return RedirectToAction("UrunVaryasyon", new { tepki = 1 });
        }

        public ActionResult varyasyonDegerSil(int varyasyon_deger_id)
        {
            SQL.set("UPDATE urun_varyasyon_degerleri SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), silindi = 1 WHERE urun_varyasyon_deger_id = " + varyasyon_deger_id);
            return RedirectToAction("UrunVaryasyon", new { tepki = 3 });
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult varyasyonDegerDil(int[] dil_id, int varyasyon_deger_id, string[] deger)
        {
            DataTable dt_varmi;
            for (int i = 0; i < dil_id.Length; i++)
            {
                dt_varmi = SQL.get("SELECT * FROM dil_urun_varyasyon_degerleri WHERE silindi = 0 AND dil_id = " + dil_id[i] + " AND urun_varyasyon_deger_id = " + varyasyon_deger_id);
                if (dt_varmi.Rows.Count > 0)
                {
                    SQL.set("UPDATE dil_urun_varyasyon_degerleri SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), deger = '" + deger[i] + "' WHERE dil_id = " + dil_id[i] + " AND urun_varyasyon_deger_id = " + varyasyon_deger_id);
                }
                else
                {
                    SQL.set("INSERT INTO dil_urun_varyasyon_degerleri (kaydeden_kullanici_id, dil_id, urun_varyasyon_deger_id, deger) VALUES (" + Session["kullanici_id"] + ", " + dil_id[i] + ", " + varyasyon_deger_id + ", '" + deger[i] + "')");
                }
            }

            return RedirectToAction("UrunVaryasyon", new { tepki = 1 });
        }

        [HttpPost]
        public ActionResult varyasyonDegerGetir(int varyasyon_id)
        {
            string ret_data = "";
            DataTable dt = SQL.get("SELECT * FROM urun_varyasyon_degerleri WHERE silindi = 0 AND varyasyon_id = " + varyasyon_id);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                ret_data += "<option value='" + dt.Rows[i]["urun_varyasyon_deger_id"] + "'>" + dt.Rows[i]["deger"] + "</option>";
            }

            return Json(new { result = varyasyon_id, message = ret_data });
        }
        #endregion

        #region ÜRÜN ÖZELLİK
        public ActionResult UrunOzellik()
        {
            if (Session["kullanici_id"] == null)
                return RedirectToAction("Login");

            return View();
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult ozellikEkle(string ozellik)
        {
            if (ozellik.Length <= 0)
                return RedirectToAction("UrunOzellik", new { hata = "Eksik bilgi girdiniz!" });

            SQL.set("INSERT INTO urun_ozellikleri (kaydeden_kullanici_id, ozellik) VALUES (" + Session["kullanici_id"] + ", '" + ozellik + "')");

            return RedirectToAction("UrunOzellik", new { tepki = 1 });
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult ozellikDuzenle(int ozellik_id, string ozellik, int ana_ozellik = 0)
        {
            if (ozellik.Length <= 0)
                return RedirectToAction("UrunOzellik", new { hata = "Eksik bilgi girdiniz!" });

            SQL.set("UPDATE urun_ozellikleri SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), ozellik = '" + ozellik + "' WHERE urun_ozellik_id = " + ozellik_id);

            if(ana_ozellik == 1)
            {
                SQL.set("UPDATE urun_ozellikleri SET ana_ozellik = 0");
                SQL.set("UPDATE urun_ozellikleri SET ana_ozellik = 1 WHERE urun_ozellik_id = " + ozellik_id);
            }

            return RedirectToAction("UrunOzellik", new { tepki = 1 });
        }

        public ActionResult ozellikSil(int ozellik_id)
        {
            SQL.set("UPDATE urun_ozellikleri SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), silindi = 1 WHERE urun_ozellik_id = " + ozellik_id);
            return RedirectToAction("UrunOzellik", new { tepki = 3 });
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult ozellikDil(int[] dil_id, int ozellik_id, string[] ozellik)
        {
            DataTable dt_varmi;
            for (int i = 0; i < dil_id.Length; i++)
            {
                dt_varmi = SQL.get("SELECT * FROM dil_urun_ozellikleri WHERE silindi = 0 AND dil_id = " + dil_id[i] + " AND urun_ozellik_id = " + ozellik_id);
                if (dt_varmi.Rows.Count > 0)
                {
                    SQL.set("UPDATE dil_urun_ozellikleri SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), ozellik = '" + ozellik[i] + "' WHERE dil_id = " + dil_id[i] + " AND urun_ozellik_id = " + ozellik_id);
                }
                else
                {
                    SQL.set("INSERT INTO dil_urun_ozellikleri (kaydeden_kullanici_id, dil_id, urun_ozellik_id, ozellik) VALUES (" + Session["kullanici_id"] + ", " + dil_id[i] + ", " + ozellik_id + ", '" + ozellik[i] + "')");
                }
            }

            return RedirectToAction("UrunOzellik", new { tepki = 1 });
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult ozellikDegerEkle(int ozellik_id, string deger)
        {
            if (deger.Length <= 0)
                return RedirectToAction("UrunOzellik", new { hata = "Eksik bilgi girdiniz!" });

            SQL.set("INSERT INTO urun_ozellik_degerleri (kaydeden_kullanici_id, deger, ozellik_id) VALUES (" + Session["kullanici_id"] + ", '" + deger + "', " + ozellik_id + ")");

            return RedirectToAction("UrunOzellik", new { tepki = 1 });
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult ozellikDegerDuzenle(int ozellik_deger_id, string deger)
        {
            if (deger.Length <= 0)
                return RedirectToAction("UrunOzellik", new { hata = "Eksik bilgi girdiniz!" });

            SQL.set("UPDATE urun_ozellik_degerleri SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), deger = '" + deger + "' WHERE urun_ozellik_deger_id = " + ozellik_deger_id);

            return RedirectToAction("UrunOzellik", new { tepki = 1 });
        }

        public ActionResult ozellikDegerSil(int ozellik_deger_id)
        {
            SQL.set("UPDATE urun_ozellik_degerleri SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), silindi = 1 WHERE urun_ozellik_deger_id = " + ozellik_deger_id);
            return RedirectToAction("UrunOzellik", new { tepki = 3 });
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult ozellikDegerDil(int[] dil_id, int ozellik_deger_id, string[] deger)
        {
            DataTable dt_varmi;
            for (int i = 0; i < dil_id.Length; i++)
            {
                dt_varmi = SQL.get("SELECT * FROM dil_urun_ozellik_degerleri WHERE silindi = 0 AND dil_id = " + dil_id[i] + " AND urun_ozellik_deger_id = " + ozellik_deger_id);
                if (dt_varmi.Rows.Count > 0)
                {
                    SQL.set("UPDATE dil_urun_ozellik_degerleri SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), deger = '" + deger[i] + "' WHERE dil_id = " + dil_id[i] + " AND urun_ozellik_deger_id = " + ozellik_deger_id);
                }
                else
                {
                    SQL.set("INSERT INTO dil_urun_ozellik_degerleri (kaydeden_kullanici_id, dil_id, urun_ozellik_deger_id, deger) VALUES (" + Session["kullanici_id"] + ", " + dil_id[i] + ", " + ozellik_deger_id + ", '" + deger[i] + "')");
                }
            }

            return RedirectToAction("UrunOzellik", new { tepki = 1 });
        }

        [HttpPost]
        public ActionResult ozellikDegerGetir(int ozellik_id)
        {
            string ret_data = "";
            DataTable dt = SQL.get("SELECT * FROM urun_ozellik_degerleri WHERE silindi = 0 AND ozellik_id = " + ozellik_id);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                ret_data += "<option value='" + dt.Rows[i]["urun_ozellik_deger_id"] + "'>" + dt.Rows[i]["deger"] + "</option>";
            }

            return Json(new { result = ozellik_id, message = ret_data });
        }
        #endregion

        #region E-TİCARET
        public ActionResult Siparisler(string ad_soyad = "", int siparis_durumu_parametre_id = 0, int odeme_tipi_parametre_id = 0, int siparis_id = 0)
        {
            if (Session["kullanici_id"] == null)
                return RedirectToAction("Login");

            ViewBag.ad_soyad = ad_soyad;
            ViewBag.siparis_durumu_parametre_id = siparis_durumu_parametre_id;
            ViewBag.odeme_tipi_parametre_id = odeme_tipi_parametre_id;
            ViewBag.siparis_id = siparis_id;

            return View();
        }

        public ActionResult Siparis(int id = 0)
        {
            if (Session["kullanici_id"] == null)
                return RedirectToAction("Login");

            ViewBag.siparis_id = id;

            DataTable dt_siparis = SQL.get("SELECT * FROM siparisler WHERE siparis_id = " + id);
            if(dt_siparis.Rows.Count <= 0)
                return RedirectToAction("Index");

            return View();
        }

        public ActionResult SiparisDuzenle(int siparis_id, int siparis_durumu_parametre_id, string kargo_no)
        {
            if (Session["kullanici_id"] == null)
                return RedirectToAction("Login");

            SQL.set("UPDATE siparisler SET guncelleyen_kullanici_id = " + Session["kullanici_id"] + ", guncelleme_tarihi = GETDATE(), siparis_durumu_parametre_id = " + siparis_durumu_parametre_id + ", kargo_no = '" + kargo_no + "' WHERE siparis_id = " + siparis_id);

            return RedirectToAction("Siparis", new { id = siparis_id });
        }

        public ActionResult N11()
        {
            if (Session["kullanici_id"] == null)
                return RedirectToAction("Login");

            return View();
        }
        #endregion
    }
}
