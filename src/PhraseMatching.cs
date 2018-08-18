using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using F23.StringSimilarity;
using LemmaSharp;

namespace DayforceAssistant.Infrastructure.NLP
{
    public class PhraseMatcher
    {
        /// <summary>
        ///  Given a list of phrases, return the best matching phrase based on an input phrase. 
        /// </summary>
        /// <param name="inputPhrase"></param>
        /// <param name="phrases"></param>
        /// <returns></returns>
        public string[] GetMatchingPhrase(string inputPhrase, List<string> phrases)
        {
            if (phrases?.Count == 0)
                return null;

            // Lemmatize and remove stop-words from phrases
            var lmtz = new LemmatizerPrebuiltCompact(LanguagePrebuilt.English);
            var lemmaListPhrases = Lemmatize(phrases, lmtz);
            for(var i=0; i< lemmaListPhrases.Count; i++)
            {
                var phrase = lemmaListPhrases[i];
                lemmaListPhrases[i] = RemoveStopWords(phrase);
            }

            // Lemmatize and remove stop-words from inputPhrase
            var lemmaInputPhrase = string.Join(" ", LemmatizePhrase(lmtz, inputPhrase));
            lemmaInputPhrase = RemoveStopWords(lemmaInputPhrase);
            lemmaInputPhrase = SubstituteWords(lemmaInputPhrase);   // "your" => "my"

            // LCS distance
            var bestMatchIndex = LongestCommonSubsequence(lemmaInputPhrase, lemmaListPhrases);

            return new string[] { phrases[bestMatchIndex] };
        }

        /// <summary>
        /// Since this algorithm will be used to find best match of a response for a given query, 
        /// the response string should contain certain transposed matching words, for eg, "my" for "your" 
        /// </summary>
        /// <param name="inputPhrase"></param>
        /// <returns></returns>
        private string SubstituteWords(string inputPhrase)
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
        /// <param name="lstPhrases"></param>
        /// <param name="lmtz"></param>
        /// <returns></returns>
        private List<string> Lemmatize(List<string> lstPhrases, ILemmatizer lmtz)
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
        /// Finds the maximum LCS of an input string against a list of strings.
        /// </summary>
        /// <param name="lemmaInputPhrase"></param>
        /// <param name="lemmaListPhrases"></param>
        /// <returns></returns>
        private int LongestCommonSubsequence(string lemmaInputPhrase, IList<string> lemmaListPhrases)
        {
            var maxLCS = 0.0;
            int bestMatchIndex = 0;
            var lemmaInputPhraseOrdered = string.Join(" ", lemmaInputPhrase.Split(new char[] { ' ' }).OrderBy(x => x).ToList());

            for (var i = 0; i < lemmaListPhrases.Count; i++)
            {
                // order the list first before joining into a single string 'cos LCS depends on the order of characters
                var lemmaListPhrasesOrdered = lemmaListPhrases[i].Split(" ").OrderBy(x => x).ToList();

                var lemmaDicPhrase = string.Join(" ", lemmaListPhrasesOrdered);
                var currentLCS = LCS(lemmaInputPhraseOrdered, lemmaDicPhrase);
                if (currentLCS > maxLCS)
                {
                    maxLCS = currentLCS;
                    bestMatchIndex = i;
                }
            }

            return bestMatchIndex;
        }

        /// <summary>
        /// Remove stop-words such as "what", "from", "to", etc to increase the probability of matches.
        /// </summary>
        /// <param name="inputPhrase"></param>
        /// <returns></returns>
        private string RemoveStopWords(string inputPhrase)
        {
            var stopWords = new[] { "yes", "no", "are", "on", "at", "with", "from", "to", "am", "is", "for", "a", "of", "any", "it", "what", "when", "who", "where", "be" };
            var result = string.Join(" ", inputPhrase.Split(' ').Where(wrd => !stopWords.Contains(wrd)));

            return result;
        }

        /// <summary>
        /// Returns the length of the Longest Common Subsequence between two strings
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        private double LCS(string s1, string s2)
        {
            //var lev = new Levenshtein();
            //var jw = new JaroWinkler();
            var lcs = new LongestCommonSubsequence();
            var len = lcs.Length(s1, s2);

            //return lcs.Distance(s1, s2);
            return len;
        }

        private string[] LemmatizePhrase(ILemmatizer lmtz, string phrase)
        {
            var words = phrase.Split(
                new char[] { ' ', ',', '.', ')', '(' }, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < words.Length; i++)
            {
                words[i] = LemmatizeWord(lmtz, words[i]);
            }

            return words;
        }

        private string LemmatizeWord(ILemmatizer lmtz, string word)
        {
            string wordLower = word.ToLower();
            string lemma = lmtz.Lemmatize(wordLower);
            return lemma;
        }
    }
}
