using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using F23.StringSimilarity;
using LemmaSharp;
using Syn.WordNet;

namespace Puzzles_NetCore
{
    public class PhraseMatching
    {
        /// <summary>
        ///  Given a list of phrases, return the best matching phrase based on an input phrase. 
        /// </summary>
        /// <returns>List of 2 best matches</returns>
        public static List<string> GetMatchingPhrase(string inputPhrase, List<string> phrases)
        {
            if (phrases?.Count == 0)
                return null;

            // Lemmatize and remove stop-words from phrases
            var lmtz = new LemmatizerPrebuiltCompact(LanguagePrebuilt.English);
            var lemmaListPhrases = Lemmatize(phrases, lmtz);
            for (var i = 0; i < lemmaListPhrases.Count; i++)
            {
                var phrase = lemmaListPhrases[i];
                lemmaListPhrases[i] = RemoveStopWords(phrase);
            }

            // Lemmatize and remove stop-words from inputPhrase
            var lemmaInputPhrase = string.Join(" ", LemmatizePhrase(lmtz, inputPhrase));
            lemmaInputPhrase = RemoveStopWords(lemmaInputPhrase);
            lemmaInputPhrase = SubstituteWords(lemmaInputPhrase);   // "your" => "my"

            // find the best match
            var matches = BestSetMatch(lemmaInputPhrase, lemmaListPhrases);
            var matchedPhrases = matches.Count == 0 ? new List<string> { "" } : matches.Take(2).Select(idx => phrases[idx]).ToList();

            // paraphrase
            //var paraphrasedResult = Paraphrase(matchedPhrases[0]);

            return matchedPhrases;
        }

        /// <summary>
        /// Since this algorithm will be used to find best match of a response for a given query, 
        /// the response string should contain certain transposed matching words, for eg, "my" for "your" 
        /// </summary>
        private static string SubstituteWords(string inputPhrase)
        {
            var pattern = @"\byour\b";
            var regex = new Regex(pattern);
            inputPhrase = regex.Replace(inputPhrase, "my");

            pattern = @"\byou\b";
            regex = new Regex(pattern);
            inputPhrase = regex.Replace(inputPhrase, "i");

            return inputPhrase;
        }

        /// <summary>
        /// Lemmatize the phrases.
        /// </summary>
        private static List<string> Lemmatize(List<string> lstPhrases, ILemmatizer lmtz)
        {
            var lemmaListPhrases = new List<string[]>();

            foreach (var phrase in lstPhrases)
            {
                var words = phrase.Split(
                            new char[] { ' ', ',', '.', ')', '(' }, StringSplitOptions.RemoveEmptyEntries);
                lemmaListPhrases.Add(words);
            }

            foreach (var arrOfWords in lemmaListPhrases)
            {
                for (var i = 0; i < arrOfWords.Length; i++)
                {
                    arrOfWords[i] = LemmatizeWord(lmtz, arrOfWords[i]);
                }
            }

            return lemmaListPhrases.Select(arr => string.Join(" ", arr)).ToList();
        }

        /// <summary>
        /// Finds the best match by using set difference of words between inputPhrase and
        /// each of the listedPhrases.
        /// </summary>
        private static List<int> BestSetMatch(string lemmaInputPhrase, IList<string> lemmaListPhrases)
        {
            var matches = new SortedDictionary<int, int>();

            var inputSet = lemmaInputPhrase.Split(" ").ToHashSet();

            for (var i = 0; i < lemmaListPhrases.Count; i++)
            {
                var vocabSet = lemmaListPhrases[i].Split(" ").ToHashSet();
                var difference = inputSet.Except(vocabSet);
                var count = difference.Count();

                // we need to make sure at least 2 words from the inputPhrase matches with any test phrase
                if (count <= (inputSet.Count - 2))
                    matches[count] = i;
            }

            return matches.Values.ToList();
        }

        /// <summary>
        /// Remove stop-words such as "what", "from", "to", etc to increase the probability of matches.
        /// </summary>
        private static string RemoveStopWords(string inputPhrase)
        {
            var stopWords = new[] { "yes", "no", "are", "on", "at", "with", "from", "to", "am", "is", "for", "a", "of", "any", "it", "what", "when", "who", "where", "be", "the", ",", ".", "?" };
            var result = string.Join(" ", inputPhrase.Split(' ').Where(wrd => !stopWords.Contains(wrd)));

            return result;
        }

        private static string[] LemmatizePhrase(ILemmatizer lmtz, string phrase)
        {
            var words = phrase.Split(
                new char[] { ' ', ',', '.', ')', '(' }, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < words.Length; i++)
            {
                words[i] = LemmatizeWord(lmtz, words[i]);
            }

            return words;
        }

        private static string LemmatizeWord(ILemmatizer lmtz, string word)
        {
            string wordLower = word.ToLower();
            string lemma = lmtz.Lemmatize(wordLower);
            return lemma;
        }

        private static void LoadWordnet(WordNetEngine wordNet)
        {
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "wordnet");
            wordNet.LoadFromDirectory(directory);
        }

        /// <summary>
        /// This function uses Google Cloud Translate to paraphrase an english sentence,
        /// first by translating it into 2 foreign languages and then back to english.
        /// Normally a seq2seq text generator should be used here.
        /// </summary>
        private static string Paraphrase(string input)
        {
            var rnd = new Random();
            var languages = new List<string> { "fr", "de", "es" };
            var index = rnd.Next(0, 3);

            // 1st translation to a foreign language
            var str = Translation.Translate(input, languages[index]);

            // fisher-yates
            var lastPicked = languages[index];
            languages[index] = "es";
            languages[2] = lastPicked;

            // 2nd translation to another foreign language
            index = rnd.Next(0, 2);
            str = Translation.Translate(str, languages[index]);

            // final translation back to english
            str = Translation.Translate(str, "en");

            return str;
        }
    }
}
