using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace IMDB_Crawler.Crawler.Core.Utils
{
    public class CsvExporter() // Construtor primário
    {

        public void SaveToCsv<T>(IEnumerable<T> data)
        {
            if (data == null || !data.Any())
            {
                Console.WriteLine("Nenhum dado para exportar.");
                return;
            }

            try
            {
                string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"); 
                string _filePath = Path.Combine(downloadsFolder, $"top20filmes_{timestamp}.csv");
                using var writer = new StreamWriter(_filePath, false, Encoding.UTF8);
                using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

                csv.WriteRecords(data);
                Console.WriteLine($"Dados salvos em: {_filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar CSV: {ex.Message}");
            }
        }
    }
}
