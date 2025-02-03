using System;
using System.IO;
using System.Text.Json;

public class AppConfig
{
    public int MaxItems { get; set; } = 10; // Valor padrão caso falhe a leitura
    public string CaptchaKey { get; set; } = string.Empty;
    public string EmailLogin { get; set; } = string.Empty;
    public string PasswordLogin { get; set; } = string.Empty;

    private const string ConfigFilePath = "config.json";

    public static AppConfig Load()
    {
        string projectRoot = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName).FullName).FullName;

        // Caminho completo para o arquivo de configuração
        string configFilePath = Path.Combine(projectRoot, "config.json");
        // Caminho completo para o arquivo de configuração
        if (!File.Exists(configFilePath))
        {
            throw new FileNotFoundException($"Arquivo de configuração '{configFilePath}' não encontrado.");
        }

        string json = File.ReadAllText(configFilePath);
        return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
    }
}
