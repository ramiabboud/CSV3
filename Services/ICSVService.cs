using CSV2.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections;

namespace CSV2.Services
{
    public interface ICSVService
    {
        public IEnumerable<T> readCSV<T>(Stream file);
        void WriteCSV<T>(List<T> records);
        public FileStreamResult? exportCSV(string filename);
        public void EnhanceFile(CsvRecord enreachedFile);
    }
}
