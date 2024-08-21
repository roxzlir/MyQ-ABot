using Azure.AI.Language.QuestionAnswering;
using Azure.AI.Translation.Text;
using Azure;
using Microsoft.Extensions.Configuration;

namespace MyQ_ABot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Hämtar alla nycklar och det jag behöver för att köra mot Azure services

            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();

            string translatorEndpoint = configuration["TranslatorEndpoint"];
            string translatorKey = configuration["TranslatorKey"];
            string qnaEndpoint = configuration["QnAEndpoint"];
            string qnaKey = configuration["QnAKey"];
            string projectName = configuration["QnAProjectName"];
            string deploymentName = configuration["QnADeploymentName"];



            // Skapar en Translator klient

            TextTranslationClient translatorClient = new TextTranslationClient(
                new AzureKeyCredential(translatorKey),
                new Uri(translatorEndpoint)
            );

            // Samt en BOT klient

            Uri endpoint = new Uri(qnaEndpoint);
            AzureKeyCredential credential = new AzureKeyCredential(qnaKey);

            QuestionAnsweringClient client = new QuestionAnsweringClient(endpoint, credential);
            QuestionAnsweringProject project = new QuestionAnsweringProject(projectName, deploymentName);

            Console.WriteLine("Now chatting with RoxBot!");
            Console.WriteLine("(to quit, just ask quit)");

            while (true)
            {

                Console.Write("Me: ");
                // Tar emot en fråga på valfritt språk
                string question = Console.ReadLine();
                // Översätter frågan till engelska med metoden TranslateTextAsync
                string translatedQuestion = await TranslateTextAsync(translatorClient, question);
                //Tar emot ett svar en det kommer som ett paket
                Response<AnswersResult> response = client.GetAnswers(translatedQuestion, project);
                // Så måste loopa då det ligger som en generic lista,då vill jag få ut VÄRDET och ANSWERS
                foreach (KnowledgeBaseAnswer answer in response.Value.Answers)
                {
                    Console.WriteLine($"RoxBot: {answer.Answer}");
                }

                if (translatedQuestion.ToLower() == "quit") break;
            }
        }

        // Gör en metod som tar emot en tranlator klient samt den text jag vill få översatt till engelska åt min bot
        static async Task<string> TranslateTextAsync(TextTranslationClient translatorClient, string text)
        {
            string targetLanguage = "en";
            var response = await translatorClient.TranslateAsync(targetLanguage, new List<string> { text }).ConfigureAwait(false);

            // Returnera den översatta texten
            return response.Value[0].Translations[0].Text;
        }
    }
}

