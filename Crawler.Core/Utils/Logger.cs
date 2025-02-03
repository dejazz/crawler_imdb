namespace Crawler_Data_Lawer.Crawler.Core.Utils
{
    public static class Logger
    {
        // Caminho do arquivo de log
      

        // Método para registrar erros com uma mensagem adicional
        public static void LogError(Exception ex, string message)
        {
            // Formatação da mensagem de log com detalhes da exceção e a mensagem adicional
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERRO: {ex.Message}\nStackTrace: {ex.StackTrace}\nMensagem adicional: {message}\n";
            WriteLog(logMessage);
        }

        // Método para registrar mensagens informativas
        public static void LogMessage(string message)
        {
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO: {message}\n";
            WriteLog(logMessage);
        }

        // Método privado que escreve a mensagem no arquivo de log
        private static void WriteLog(string message)
        {
            string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            string LogFilePath = Path.Combine(projectRoot, "logs.txt");
            try
            {
                File.AppendAllText(LogFilePath, message); // Adiciona a mensagem ao arquivo
            }
            catch (Exception ex)
            {
                // Caso ocorra erro ao escrever no log, mostra uma mensagem no console
                Console.WriteLine($"Erro ao escrever no log: {ex.Message}");
            }
        }
    }
}