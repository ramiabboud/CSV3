using CSV2.Models;
using CSV2.Services;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace CSV2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CSVController : ControllerBase
    {
        private readonly ICSVService _csvService;

        public CSVController(ICSVService csvService)
        {
            _csvService = csvService;
        }

        [HttpPost("import-csv-file")]
        public IActionResult ImportCSVFile([FromForm] IFormFileCollection file)
        {
            List<CsvRecord> csvFile = _csvService.ReadCSV<CsvRecord>(file[0].OpenReadStream()).ToList();

            foreach (var csvFileRecord in csvFile)
            {
                if (csvFileRecord != null)
                {
                    _csvService.EnhanceFile(csvFileRecord);
                }
            }

            _csvService.WriteCSV(csvFile);
           
            return Ok(csvFile);
        }

        [HttpGet("export-csv-file/{filename}")]
        public IActionResult ExportCSVFile(string filename)
        {
            if (_csvService.ExportCSV(filename) == null)
            {
                return NotFound($"File {filename} not found.");
            }

            return _csvService.ExportCSV(filename);
        }
    }
}