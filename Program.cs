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



            // Translator klient
            TextTranslationClient translatorClient = new TextTranslationClient(
                new AzureKeyCredential(translatorKey),
                new Uri(translatorEndpoint)
            );

            // BOT klient

            Uri endpoint = new Uri(qnaEndpoint);
            AzureKeyCredential credential = new AzureKeyCredential(qnaKey);


            QuestionAnsweringClient client = new QuestionAnsweringClient(endpoint, credential);
            QuestionAnsweringProject project = new QuestionAnsweringProject(projectName, deploymentName);

            Console.WriteLine("Now chatting with RoxBot!");
            Console.WriteLine("(to quit, just ask quit)");

            while (true)
            {

                Console.Write("Me: ");

                string question = Console.ReadLine();

                string translatedQuestion = await TranslateTextAsync(translatorClient, question);

                Response<AnswersResult> response = client.GetAnswers(translatedQuestion, project);

                foreach (KnowledgeBaseAnswer answer in response.Value.Answers)
                {

                    Console.WriteLine($"RoxBot: {answer.Answer}");
                }

                if (translatedQuestion.ToLower() == "quit") break;
            }
        }

        static async Task<string> TranslateTextAsync(TextTranslationClient translatorClient, string text)
        {
            string targetLanguage = "en";
            var response = await translatorClient.TranslateAsync(targetLanguage, new List<string> { text }).ConfigureAwait(false);

            // Returnera den översatta texten
            return response.Value[0].Translations[0].Text;
        }
    }
}

