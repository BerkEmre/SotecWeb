using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace sotec_firma.Helpers
{
    public static class SQL
    {
#if DEBUG
        static string text = ConfigurationManager.ConnectionStrings["conStr_local"].ConnectionString;
#else
            static string text = ConfigurationManager.ConnectionStrings["conStr_srver"].ConnectionString;
#endif
        //ConfigurationSettings.AppSettings["connecitonstring"].ToString();//@"Server=DESKTOP-46PJDJK\SQLEXPRESS;Database=sotec_ticaret;User Id=sa;Password=1234;";
        static SqlConnection con = new SqlConnection(@text);

        public static bool baglanti_test()
        {

            try
            {
                //text = System.IO.File.ReadAllText(@"constr.txt");
                con = new SqlConnection(text);
                SQL.get("SELECT * FROM kullanicilar"); return true;
            }
            catch
            {
                con.Close();
                return false;
            }
        }

        public static DataTable get(string query)
        {
            try
            {
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataSet ds = new DataSet();
                con.Open();
                da.Fill(ds);
                DataTable dt = ds.Tables[0];
                con.Close();
                return dt;
            }
            catch
            {
                con.Close();
                return null;
            }
        }

        public static void set(string query)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                con.Open();
                cmd.Connection = con;
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception e)
            {
                con.Close();
            }
        }

        public static void setParam(string query, params object[] param)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                con.Open();
                cmd.Connection = con;
                cmd.CommandText = query;
                for (int i = 0; i < param.Length; i++)
                {
                    cmd.Parameters.AddWithValue("@value" + i, param[i]);
                }
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception e)
            {
                con.Close();
            }
        }

        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }
    }
}