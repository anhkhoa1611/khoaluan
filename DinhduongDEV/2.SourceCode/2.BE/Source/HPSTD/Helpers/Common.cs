using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using HPSTD.Models;

namespace HPSTD.Helpers
{
    public static class Common
    {
        #region Cắt chuỗi
        static public string Cat_Chuoi(int sl, string Chuoi)
        {
            if (Chuoi.Trim().Length <= sl)
                return Chuoi;
            else
            {
                for (int i = sl; i < Chuoi.Length; i++)
                    if (Chuoi[i].ToString() == " ")
                        return Chuoi.Substring(0, i) + "...";
            }
            return Chuoi;
        }
        #endregion

        #region Chuyển từ có dấu thành không dấu

        private static readonly string[] VietnameseSigns = new string[]
       {

            "aAeEoOuUiIdDyY",

            "áàạảãâấầậẩẫăắằặẳẵ",

            "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",

            "éèẹẻẽêếềệểễ",

            "ÉÈẸẺẼÊẾỀỆỂỄ",

            "óòọỏõôốồộổỗơớờợởỡ",

            "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",

            "úùụủũưứừựửữ",

            "ÚÙỤỦŨƯỨỪỰỬỮ",

            "íìịỉĩ",

            "ÍÌỊỈĨ",

            "đ",

            "Đ",

            "ýỳỵỷỹ",

            "ÝỲỴỶỸ"

       };

        public static string locDau(string str)
        {
            str = string.IsNullOrEmpty(str) ? "" : str;
            for (int i = 1; i < VietnameseSigns.Length; i++)
            {
                for (int j = 0; j < VietnameseSigns[i].Length; j++)

                    str = str.Replace(VietnameseSigns[i][j], VietnameseSigns[0][i - 1]);
            }
            return str;
        }
        static public string ConvertToUnSign(string s)
        {
            s = s.ToLower();
            string stFormD = s.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                System.Globalization.UnicodeCategory uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }
            sb = sb.Replace('Đ', 'D');
            sb = sb.Replace('đ', 'd');
            return (sb.ToString().Normalize(NormalizationForm.FormD));
        }
        public static string RejectMarkst(string text)
        {
            string[] pattern = new string[7];
            pattern[0] = "a|(á|ả|à|ạ|ã|ă|ắ|ẳ|ằ|ặ|ẵ|â|ấ|ẩ|ầ|ậ|ẫ)";
            pattern[1] = "o|(ó|ỏ|ò|ọ|õ|ô|ố|ổ|ồ|ộ|ỗ|ơ|ớ|ở|ờ|ợ|ỡ)";
            pattern[2] = "e|(é|è|ẻ|ẹ|ẽ|ê|ế|ề|ể|ệ|ễ)";
            pattern[3] = "u|(ú|ù|ủ|ụ|ũ|ư|ứ|ừ|ử|ự|ữ)";
            pattern[4] = "i|(í|ì|ỉ|ị|ĩ)";
            pattern[5] = "y|(ý|ỳ|ỷ|ỵ|ỹ)";
            pattern[6] = "d|đ";
            for (int i = 0; i < pattern.Length; i++)
            {
                // kí tự sẽ thay thế
                char replaceChar = pattern[i][0];
                MatchCollection matchs = Regex.Matches(text, pattern[i]);
                foreach (Match m in matchs)
                {
                    text = text.Replace(m.Value[0], replaceChar);
                }
            }
            return text;
        }

        public static string RejectMarksh(string text)
        {

            string[] pattern = new string[7];
            pattern[0] = "A|(Á|Ả|À|Ạ|Ã|Ă|Ắ|Ẳ|Ằ|Ặ|Ẵ|Â|Ấ|Ẩ|Ầ|Ậ|Ẫ)";
            pattern[1] = "O|(Ó|Ỏ|Ò|Ọ|Õ|Ô|Ố|Ổ|Ồ|Ộ|Ỗ|Ơ|Ớ|Ở|Ờ|Ợ|Ỡ)";
            pattern[2] = "E|(É|È|Ẻ|Ẹ|Ẽ|Ê|Ế|Ề|Ể|Ệ|Ễ)";
            pattern[3] = "U|(Ú|Ù|Ủ|Ụ|Ũ|Ư|Ứ|Ừ|Ử|Ự|Ữ)";
            pattern[4] = "I|(Í|Ì|Ỉ|Ị|Ĩ)";
            pattern[5] = "Y|(Ý|Ỳ|Ỷ|Ỵ|Ỹ)";
            pattern[6] = "D|Đ";
            for (int i = 0; i < pattern.Length; i++)
            {
                // kí tự sẽ thay thế
                char replaceChar = pattern[i][0];
                MatchCollection matchs = Regex.Matches(text, pattern[i]);
                foreach (Match m in matchs)
                {
                    text = text.Replace(m.Value[0], replaceChar);
                }
            }
            return text;
        }
        static public string RejectMarks(string text)
        {
            text = RejectMarkst(text.ToLower());
            string result = "";
            foreach (char i in text)
            {
                if ((i >= 'a' && i <= 'z') || (i >= 'A' && i <= 'Z') || (i >= '0' && i <= '9') || i == ' ')
                {
                    result += i;
                }
            }
            return result;
        }
        static public string Filename(string Chuoi)
        {
            if (Chuoi.Trim().Length > 0)
            {
                if (Chuoi.Trim().Length > 80)
                    Chuoi = Cat_Chuoi(80, Chuoi.Trim());
                Chuoi = RejectMarks(Chuoi.Trim());

                return Chuoi.Replace(" ", "-") + "-";
            }
            return "";
        }
        static public string Title(string Chuoi)
        {
            if (Chuoi.Trim().Length > 0)
            {

                Chuoi = RejectMarks(Chuoi.Trim());
                return Chuoi.Trim().Replace(" ", "-");
            }
            return "";
        }

        static public string Keywords(string Chuoi)
        {
            if (Chuoi.Trim().Length > 0)
            {

                Chuoi = RejectMarks(Chuoi.Trim());
                return Chuoi.Trim().Replace(" ", " ");
            }
            return "";
        }

        public static string phoneFormat(object p)
        {
            string phone_ = p.ToString().Trim().Replace(" ", "");
            if (phone_.Count() == 0)
                return "";
            if (phone_.Length == 10)
                phone_ = phone_.Insert(4, " ").Insert(8, " ");
            if (phone_.Length == 11)
                phone_ = phone_.Insert(5, " ").Insert(9, " ");

            return phone_.ToString();
        }

        public static string ToPrice(object price_)
        {
            if (price_ == null || price_.ToString().Trim() == "") return "0";
            int price = int.Parse(price_.ToString());
            if (price == 0)
                return "0";

            if (price < 1000)
                return price.ToString();

            string GiaKq = price.ToString("#,#0,00");
            GiaKq = GiaKq.Replace(",", ".");
            return GiaKq;
        }

        public static string VietHoaChuCaiDauTien(string s)
        {
            if (String.IsNullOrEmpty(s))
                return s;

            string result = "";

            //lấy danh sách các từ  

            string[] words = s.Split(' ');

            foreach (string word in words)
            {
                // từ nào là các khoảng trắng thừa thì bỏ  
                if (word.Trim() != "")
                {
                    if (word.Length > 1)
                        result += word.Substring(0, 1).ToUpper() + word.Substring(1).ToLower() + " ";
                    else
                        result += word.ToUpper() + " ";
                }

            }
            return result.Trim();
        }
        #endregion

        #region Converts datatable to list<T> dynamically
        /// <summary>
        /// Converts datatable to list<T> dynamically
        /// </summary>
        /// <typeparam name="T">Class name</typeparam>
        /// <param name="dataTable">data table to convert</param>
        /// <returns>List<T></returns>
        public static List<T> ToListObject<T>(this DataTable dataTable) where T : new()
        {
            var dataList = new List<T>();

            //Define what attributes to be read from the class
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

            //Read Attribute Names and Types
            var objFieldNames = typeof(T).GetProperties(flags).Cast<PropertyInfo>().
                Select(item => new
                {
                    Name = item.Name,
                    Type = Nullable.GetUnderlyingType(item.PropertyType) ?? item.PropertyType
                }).ToList();

            //Read Datatable column names and types
            var dtlFieldNames = dataTable.Columns.Cast<DataColumn>().
                Select(item => new {
                    Name = item.ColumnName,
                    Type = item.DataType
                }).ToList();

            foreach (DataRow dataRow in dataTable.AsEnumerable().ToList())
            {
                var classObj = new T();

                foreach (var dtField in dtlFieldNames)
                {
                    PropertyInfo propertyInfos = classObj.GetType().GetProperty(dtField.Name);

                    var field = objFieldNames.Find(x => x.Name == dtField.Name);

                    if (field != null)
                    {

                        if (propertyInfos.PropertyType == typeof(DateTime))
                        {
                            propertyInfos.SetValue
                            (classObj, convertToDateTime(dataRow[dtField.Name]), null);
                        }
                        else if (propertyInfos.PropertyType == typeof(int))
                        {
                            propertyInfos.SetValue
                            (classObj, ConvertToInt(dataRow[dtField.Name]), null);
                        }
                        else if (propertyInfos.PropertyType == typeof(long))
                        {
                            propertyInfos.SetValue
                            (classObj, ConvertToLong(dataRow[dtField.Name]), null);
                        }
                        else if (propertyInfos.PropertyType == typeof(float))
                        {
                            propertyInfos.SetValue
                            (classObj, ConvertToFloat(dataRow[dtField.Name]), null);
                        }
                        else if (propertyInfos.PropertyType == typeof(decimal))
                        {
                            propertyInfos.SetValue
                            (classObj, ConvertToDecimal(dataRow[dtField.Name]), null);
                        }
                        else if (propertyInfos.PropertyType == typeof(String))
                        {
                            if (dataRow[dtField.Name].GetType() == typeof(DateTime))
                            {
                                propertyInfos.SetValue
                                (classObj, ConvertToDateString(dataRow[dtField.Name]), null);
                            }
                            else
                            {
                                propertyInfos.SetValue
                                (classObj, ConvertToString(dataRow[dtField.Name]), null);
                            }
                        }
                    }
                }
                dataList.Add(classObj);
            }
            return dataList;
        }

        private static string ConvertToDateString(object date)
        {
            if (date == null)
                return string.Empty;

            return HelperFunctions.ConvertDate(Convert.ToDateTime(date));
        }

        private static string ConvertToString(object value)
        {
            return Convert.ToString(HelperFunctions.ReturnEmptyIfNull(value));
        }

        private static int ConvertToInt(object value)
        {
            return Convert.ToInt32(HelperFunctions.ReturnZeroIfNull(value));
        }

        private static long ConvertToLong(object value)
        {
            return Convert.ToInt64(HelperFunctions.ReturnZeroIfNull(value));
        }

        private static float ConvertToFloat(object value)
        {
            try
            {
                return float.Parse(value.ToString());
            }
            catch
            {
                return 0;
            }
        }

        private static decimal ConvertToDecimal(object value)
        {
            return Convert.ToDecimal(HelperFunctions.ReturnZeroIfNull(value));
        }

        private static DateTime convertToDateTime(object date)
        {
            return Convert.ToDateTime(HelperFunctions.ReturnDateTimeMinIfNull(date));
        }
        #endregion

        #region Chuyển số thành chữ
        public static string join_unit(string n)
        {
            int sokytu = n.Length;
            int sodonvi = (sokytu % 3 > 0) ? (sokytu / 3 + 1) : (sokytu / 3);
            n = n.PadLeft(sodonvi * 3, '0');
            sokytu = n.Length;
            string chuoi = "";
            int i = 1;
            while (i <= sodonvi)
            {
                if (i == sodonvi) chuoi = Common.join_number((int.Parse(n.Substring(sokytu - (i * 3), 3))).ToString()) + Common.unit(i) + chuoi;
                else chuoi = Common.join_number(n.Substring(sokytu - (i * 3), 3)) + Common.unit(i) + chuoi;
                i += 1;
            }
            return chuoi;
        }


        private static string unit(int n)
        {
            string chuoi = "";
            if (n == 1) chuoi = " đồng ";
            else if (n == 2) chuoi = " nghìn ";
            else if (n == 3) chuoi = " triệu ";
            else if (n == 4) chuoi = " tỷ ";
            else if (n == 5) chuoi = " nghìn tỷ ";
            else if (n == 6) chuoi = " triệu tỷ ";
            else if (n == 7) chuoi = " tỷ tỷ ";
            return chuoi;
        }


        private static string convert_number(string n)
        {
            string chuoi = "";
            if (n == "0") chuoi = "không";
            else if (n == "1") chuoi = "một";
            else if (n == "2") chuoi = "hai";
            else if (n == "3") chuoi = "ba";
            else if (n == "4") chuoi = "bốn";
            else if (n == "5") chuoi = "năm";
            else if (n == "6") chuoi = "sáu";
            else if (n == "7") chuoi = "bảy";
            else if (n == "8") chuoi = "tám";
            else if (n == "9") chuoi = "chín";
            return chuoi;
        }


        private static string join_number(string n)
        {
            string chuoi = "";
            int i = 1, j = n.Length;
            while (i <= j)
            {
                if (i == 1) chuoi = convert_number(n.Substring(j - i, 1)) + chuoi;
                else if (i == 2) chuoi = convert_number(n.Substring(j - i, 1)) + " mươi " + chuoi;
                else if (i == 3) chuoi = convert_number(n.Substring(j - i, 1)) + " trăm " + chuoi;
                i += 1;
            }
            return chuoi;
        }


        public static string replace_special_word(string chuoi)
        {
            chuoi = chuoi.Replace("không mươi không ", "");
            chuoi = chuoi.Replace("không mươi", "lẻ");
            chuoi = chuoi.Replace("i không", "i");
            chuoi = chuoi.Replace("i năm", "i lăm");
            chuoi = chuoi.Replace("một mươi", "mười");
            chuoi = chuoi.Replace("mươi một", "mươi mốt");
            return chuoi;
        }
        #endregion

    }
}