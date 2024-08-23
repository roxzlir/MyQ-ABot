using Azure.AI.Language.QuestionAnswering;
using Azure.AI.Translation.Text;
using Azure;
using Microsoft.Extensions.Configuration;
using static System.Net.Mime.MediaTypeNames;

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

            Console.WriteLine("Now chatting with RoxBot! (to quit, just ask me to quit)");
            

            while (true)
            {

                Console.Write("Me: ");
                // Tar emot en fråga på valfritt språk
                string question = Console.ReadLine();

                //Här vill jag ta reda på vilket språk som man har skrivit frågan på
                Response<IReadOnlyList<TranslatedTextItem>> resp = await translatorClient.TranslateAsync("en", question).ConfigureAwait(false);
                IReadOnlyList<TranslatedTextItem> translations = resp.Value;
                TranslatedTextItem translation = translations.FirstOrDefault();
                // Och sparar ner språk koden till language
                string language = translation?.DetectedLanguage.Language;

                // Översätter frågan till engelska med metoden TranslateTextAsync
                string translatedQuestion = await TranslateTextAsync(translatorClient, question, "en");
                //Tar emot ett svar en det kommer som ett paket
                Response<AnswersResult> response = client.GetAnswers(translatedQuestion, project);

                // Så måste loopa då det ligger som en generic lista,då vill jag få ut VÄRDET och ANSWERS
                var botAnswer = "";
                foreach (KnowledgeBaseAnswer answer in response.Value.Answers)
                {
                    // Och här där jag kommer åt botens svar, så översätter jag det till samma språk som det man ställde fråga på genom att skicka med den språk koden
                    botAnswer = await TranslateTextAsync(translatorClient, answer.Answer, language);
                    Console.WriteLine($"RoxBot: {botAnswer}");
                }


                if (translatedQuestion.ToLower() == "quit") break;
            }
        }

        // Gör en metod som tar emot en tranlator klient samt den text jag vill få översatt till engelska åt min bot
        static async Task<string> TranslateTextAsync(TextTranslationClient translatorClient, string text, string targetLang)
        {
            string targetLanguage = targetLang;
            var response = await translatorClient.TranslateAsync(targetLanguage, new List<string> { text }).ConfigureAwait(false);

            // Returnera den översatta texten
            return response.Value[0].Translations[0].Text;
        }
 

    }
}

