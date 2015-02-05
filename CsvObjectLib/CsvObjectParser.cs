using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CsvToObjectLib
{
    public interface ICsvPropertyMapper
    {
        bool Map(string propName, string csvValue, out object val);
    }

    public class CsvObjectParser
    {
        public string Separator { get; set; }
        public ICsvPropertyMapper Mapper { get; set; }

        public CsvObjectParser()
        {
            Separator = ",";
        }

        public async Task<IEnumerable<T>> GetObjects<T>(IEnumerable<string> csvFiles) where T : class
        {
            var ret = default(List<T>);

            var properties = typeof(T).GetProperties();

            foreach (var csv in csvFiles)
            {
                int ind = 0;
                var csvHeader = default(IEnumerable<string>);

                using (var reader = File.OpenRead(csv))
                using (var tr = new StreamReader(reader))
                {
                    while (!tr.EndOfStream)
                    {
                        var values = (await tr.ReadLineAsync()).Split(Separator.ToCharArray());
                        if (ind++ == 0) // header
                        {
                            csvHeader = values.Select(it => Regex.Replace(it, @"\W", string.Empty));
                            continue;
                        }

                        if (values.Count() != csvHeader.Count()) continue;

                        ret = ret ?? new List<T>();

                        var newObj = Activator.CreateInstance<T>();

                        for (int i = 0; i < csvHeader.Count(); i++)
                        {
                            var pi = properties.Where(it => it.Name == csvHeader.ElementAt(i)).FirstOrDefault();
                            if (pi == null) continue;

                            if (Mapper != null)
                            {
                                object mappedValue = null;
                                var mapped = Mapper.Map(pi.Name, values[i], out mappedValue);

                                if (mapped)
                                {
                                    pi.SetValue(newObj, mappedValue);
                                    continue;
                                }
                            }

                            if (pi.PropertyType != typeof(string))
                            {
                                var convertedValue = Activator.CreateInstance(pi.PropertyType);
                                var parseMethod = pi.PropertyType.GetMethod("Parse", new Type[] { typeof(string) });
                                try
                                {
                                    convertedValue = parseMethod.Invoke(pi.PropertyType, new object[] { values[i] });
                                    pi.SetValue(newObj, convertedValue);
                                }
                                catch { }
                            }
                            else
                            {
                                pi.SetValue(newObj, values[i]);
                            }
                        }

                        ret.Add(newObj);
                    }
                }
            }

            return ret;
        }
    }
}
