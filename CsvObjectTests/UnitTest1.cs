using System;
using System.IO;
using System.Text.RegularExpressions;
using CsvToObjectLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvToObjectTests
{
    public class Car
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public DateTime ManufactureDate { get; set; }
    }

    public class CarPropertyMapper : ICsvPropertyMapper
    {
        public bool Map(string propName, string csvValue, out object value)
        {
            value = null;
            if (propName != "ManufactureDate") return false;

            csvValue = Regex.Replace(csvValue, @"^(\w+).(\d+)$", @"$1.01-$2");

            value = DateTime.Parse(csvValue);

            return true;
        }
    }

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var fn = "testFile.csv";

            var lines = new string[]
            {
                "Brand,Model,Manufacture Date",
                "Open,Astra,Dec.15",
                "BMW,116,Jun.14"
            };

            File.WriteAllLines(fn, lines);

            var parser = new CsvObjectParser()
            {
                Separator = ",",
                Mapper = new CarPropertyMapper()
            };

            var task = parser.GetObjects<Car>(new string[] { fn });
            task.Wait();

            File.Delete(fn);
        }
    }
}
