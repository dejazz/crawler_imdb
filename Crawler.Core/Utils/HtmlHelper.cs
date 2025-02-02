using HtmlAgilityPack;
using Newtonsoft.Json.Linq;


namespace Crawler_Data_Lawer.Crawler.Core.Utils
{
    public class HTMLHelper
    {
        // Método genérico para extrair o conteúdo JSON da tag <script type="application/ld+json">
        public static JObject ExtractJsonFromScript(string htmlContent, string paramForSearch)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            // Encontra a tag script que contém o JSON
            var jsonScript = doc.DocumentNode.Descendants("script")
                                             .FirstOrDefault(node => node.GetAttributeValue("type", "") == paramForSearch );


            if (jsonScript != null)
            {
                // Converte o conteúdo da tag script em JSON (JObject)
                var jsonString = jsonScript.InnerText.Trim();
                return JObject.Parse(jsonString);
            }

            return [];
        }
    }
}
