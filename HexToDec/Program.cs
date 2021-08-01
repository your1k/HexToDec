using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;

namespace HexToDec
{
    class Table 
    {
        public string Column { get; set; }
        public List<dynamic> Value { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            // тесты на конвертацию шестнадцатеричных чисел в десятичные
            Console.WriteLine(CheckValue(HexToDec("f8"), 248).ToString());
            Console.WriteLine(CheckValue(HexToDec("3e"), 62).ToString());
            Console.WriteLine(CheckValue(HexToDec("2c"), 44).ToString());
            Console.WriteLine(CheckValue(HexToDec("null"), null).ToString());
            Console.WriteLine(CheckValue(HexToDec("1869f"), 99999).ToString());

            var data = GetData("Data");
            foreach (var item in data)
                Console.WriteLine(item.Column + " : " + string.Join(", ", item.Value.ToList()));
            Console.WriteLine(data.Select(x => x.Value.Count).FirstOrDefault().ToString());
            Console.ReadKey();
        }

        // Метод перевода из шестнадцатеричной системы счисления в десятичную
        static long? HexToDec(string hex)
        {
            long value;
            return long.TryParse(hex.ToUpper().Trim(), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out value) ? value : (long?)null;
        }

        // метод для тестирования правильности конвертации
        static bool CheckValue(long? value, long? check) => value.Equals(check);

        static List<Table> GetData(string tbName)
        {
            var data = GetData(tbName, 1);

            var table = new List<Table>();
            foreach (var column in data.Select(x => x as IDictionary<string, dynamic>).FirstOrDefault().Select(i => i.Key).Distinct().ToList())
                table.Add(new Table { Column = column, Value = new List<dynamic>() });


            foreach (ExpandoObject items in data)
                foreach (var item in items)
                    table.Where(x => x.Column.Equals((item.Key))).FirstOrDefault().Value.Add(item.Value);
            return table;
        }

        static List<dynamic> GetData(string tbName, int Id)
        {
            try 
            {
                using (var connection = new SqlConnection(@"Server=localhost\SQLEXPRESS;Database=AccountingExpensesIncomeDataBase;Trusted_Connection=True;"))
                {
                    var dt = new DataTable();
                    new SqlDataAdapter(new SqlCommand("SELECT * FROM " + tbName, connection)).Fill(dt); //+ " WHERE Id = " + Id, connection)).Fill(dt);
                    return dt.ToDynamicData();
                }
            }
            catch(Exception ex) { Console.WriteLine(ex.Message); Console.ReadKey(); return null; }
        }
    }
   

    public static class MyExtensions
    {
        public static List<dynamic> ToDynamicData(this DataTable dt)
        {
            var dynamicDt = new List<dynamic>();
            foreach (DataRow row in dt.Rows)
            {
                dynamic dyn = new ExpandoObject();
                dynamicDt.Add(dyn);
                foreach (DataColumn column in dt.Columns)
                {
                    var dic = (IDictionary<string, object>)dyn;
                    dic[column.ColumnName] = row[column];
                }
            }
            return dynamicDt;
        }
    }
}
