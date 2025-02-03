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
        string projectRoot = GetProjectRoot(Directory.GetCurrentDirectory());

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
    
    public static string GetProjectRoot(string startDirectory)
    {
        var currentDir = new DirectoryInfo(startDirectory);

        // Loop até encontrar a raiz do projeto (onde o arquivo .csproj ou outro marcador esteja)
        while (currentDir != null && !File.Exists(Path.Combine(currentDir.FullName, "config.json")))
        {
            currentDir = currentDir.Parent;
        }

        return currentDir?.FullName; // Retorna o caminho da raiz do projeto ou null se não encontrado
    }
}
