using CSV2.Models;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Globalization;

namespace CSV2.Services
{
    public class CSVService : ICSVService
    {
        private CsvHelper.Configuration.CsvConfiguration cfg { get; set; }

        private readonly HttpClient _httpClient;
        public CSVService(HttpClient httpClient)
        {
            cfg = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
            };
            _httpClient = httpClient;
        }

        public IEnumerable<T> readCSV<T>(Stream file)
        {
            var reader = new StreamReader(file);
            var csv = new CsvReader(reader, cfg);

            var records = csv.GetRecords<T>();
            return records;
        }

        public void WriteCSV<T>(List<T> csvFile)
        {
            string dateTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            using (var writer = new StreamWriter($"{Statics.filePath}\\file_{dateTime}.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(csvFile);
            }
        }

        public FileStreamResult? exportCSV(string filename)
        {
            var path = $"{Statics.filePath}\\{filename}";

            if (!File.Exists(path))
            {
               return null;
            }

            var stream = File.OpenRead(path);

            return new FileStreamResult(stream, "text/csv")
            {
                FileDownloadName = filename
            };
        }

        public void EnhanceFile(CsvRecord enreachedFile)
        {
            try
            {
                HttpResponseMessage response = _httpClient.GetAsync($"https://api.gleif.org/api/v1/lei-records?filter[lei]={enreachedFile.lei}").Result;

                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    var gleif = (JObject)JsonConvert.DeserializeObject(content);

                    enreachedFile.legalName = ExtractProperty(gleif, "legalName", "name");
                    enreachedFile.bic = ExtractArrayProperty(gleif, "bic");

                    var country = ExtractProperty(gleif, "legalAddress", "country");
                    if (!string.IsNullOrEmpty(country))
                    {
                        enreachedFile.transaction_costs = secondEnhancement(country, enreachedFile);
                    }
                }
                else
                {
                    throw new HttpRequestException($"Failed to get LEI record: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // Log the exception details here
                throw ex;
            }
        }

        private string ExtractProperty(JObject gleif, string propertyName, string subPropertyName = null)
        {
            var property = gleif.Descendants().OfType<JProperty>().FirstOrDefault(x => x.Name == propertyName)?.Value;
            if (property is JObject propertyObject)
            {
                return subPropertyName != null ? propertyObject[subPropertyName].ToString() : propertyObject.ToString();
            }

            return null;
        }

        private string ExtractArrayProperty(JObject gleif, string propertyName)
        {
            var property = gleif.Descendants().OfType<JProperty>().FirstOrDefault(x => x.Name == propertyName);
            if (property != null && property.Value is JArray propertyArray && propertyArray.Count > 0)
            {
                return propertyArray[0].ToString();
            }

            return null;
        }

        private double secondEnhancement(string country, CsvRecord enreachedRecord)
        {
            if (country.Equals("GB"))
            {
                return enreachedRecord.notional * enreachedRecord.rate;
            }
            else if (country.Equals("NL"))
            {
                return Math.Abs(enreachedRecord.notional * (1 / enreachedRecord.rate) - enreachedRecord.notional);
            }
            else
                return 0;
        }
    }
}
