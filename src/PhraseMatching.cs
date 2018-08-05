using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using F23.StringSimilarity;
using LemmaSharp;
using Syn.WordNet;

namespace Puzzles
{
    class PhraseMatching
    {
        public static string[] GetMatchingPhrase(string inputPhrase, IList<string> phrases)
        {
            //var wordNet = new WordNetEngine();

            if (phrases?.Count == 0)
                return null;

            // Remove stop-words
            inputPhrase = RemoveStopWords(inputPhrase);
            var lstPhrases = new List<string>();
            foreach (var phrase in phrases)
            {
                lstPhrases.Add(RemoveStopWords(phrase));
            }

            // Lemmatize
            var lemmaListPhrases = new List<string[]>();
            var lmtz = new LemmatizerPrebuiltCompact(LanguagePrebuilt.English);
            Lemmatize(lstPhrases, lemmaListPhrases, lmtz);

            // Min distance
            var lemmaInputPhrase = string.Join(" ", LemmatizePhrase(lmtz, inputPhrase));
            var bestMatchIndex = FindMinimumDistance(lemmaInputPhrase, lemmaListPhrases);

            var paraphrasedResult = Paraphrase(phrases[bestMatchIndex]);
            return new string[] { phrases[bestMatchIndex], paraphrasedResult };
        }

        private static void Lemmatize(List<string> lstPhrases, IList<string[]> lemmaListPhrases, ILemmatizer lmtz)
        {
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
        }

        /// <summary>
        /// This function uses Google Cloud Translate to paraphrase an english sentence,
        /// first by translating it into 2 foreign languages and then back to english.
        /// Normally a seq2seq text generator should be used here.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
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

        private static void LoadWordnet(WordNetEngine wordNet)
        {
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "wordnet");
            wordNet.LoadFromDirectory(directory);
        }

        private static int FindMinimumDistance(string lemmaInputPhrase, IList<string[]> lemmaListPhrases)
        {
            var minDistance = int.MaxValue;
            int bestMatchIndex = 0;
            var lemmaInputPhraseOrdered = string.Join(" ", lemmaInputPhrase.Split(new char[] { ' ' }).OrderBy(x => x).ToList());
         
            for (var i = 0; i < lemmaListPhrases.Count; i++)
            {
                // order the list first before joining into a single string 'cos Levenshtein depends on the order or characters
                var lemmaListPhrasesOrdered = lemmaListPhrases[i].OrderBy(x => x).ToList();

                var lemmaDicPhrase = string.Join(" ", lemmaListPhrasesOrdered);
                var dist = EditDistance(lemmaInputPhraseOrdered, lemmaDicPhrase);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    bestMatchIndex = i;
                }
            }

            return bestMatchIndex;
        }

        private static string RemoveStopWords(string inputPhrase)
        {
            var stopWords = new[] { "yes", "no", "you", "are", "on", "at", "with", "from", "to", "am", "is", "my", "I", "for", "a", "of", "any" };
            var result = string.Join(" ", inputPhrase.Split(' ').Where(wrd => !stopWords.Contains(wrd)));

            return result;
        }

        private static int EditDistance(string s1, string s2)
        {
            var lev = new Levenshtein();
            return (int)lev.Distance(s1, s2);
        }

        private static string[] LemmatizePhrase(ILemmatizer lmtz, string phrase)
        {
            var words = phrase.Split(
                new char[] { ' ', ',', '.', ')', '(' }, StringSplitOptions.RemoveEmptyEntries);

            for(var i=0; i<words.Length; i++)
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
    }
}
