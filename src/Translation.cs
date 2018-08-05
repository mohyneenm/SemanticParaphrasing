using Google.Apis.Services;
using Google.Apis.Translate.v2;

namespace Puzzles
{
    public class Translation
    {
        private static string key = "GOOGLE_TRANSLATE_API_KEY";

        public static string Translate(string textToTranslate, string targetLanguage)
        {
            // Create the service.
            var service = new TranslateService(new BaseClientService.Initializer()
            {
                ApiKey = key,
                ApplicationName = "SoftwareCraftsmanship"
            });

            string[] srcText = new[] { textToTranslate };
            var response = service.Translations.List(srcText, targetLanguage).ExecuteAsync().Result;
            return response.Translations[0].TranslatedText;
        }
    }
}
