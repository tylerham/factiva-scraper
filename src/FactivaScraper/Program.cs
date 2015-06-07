using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsQuery;
using CsQuery.ExtensionMethods.Internal;
using CsvHelper;

namespace FactivaScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Environment.CurrentDirectory);
            CQ f = File.ReadAllText("factiva.htm");
//            CQ article = f["div.article .enArticle"];
            CQ paragraphs = f["p.enarticleParagraph"];

            IList<string[]> rows = new List<string[]>();
            var speakers = new List<string>();
            string speaker = String.Empty;

            for(int i = 0; i<paragraphs.Length; i++)
            {
                var paragraph = paragraphs[i];
                var newSpeaker = Helpers.FindSpeaker(paragraph.InnerText);
                if (!newSpeaker.IsNullOrEmpty())
                {
                    speaker = newSpeaker;

                    if (!speakers.Contains(speaker))
                        speakers.Add(speaker);
                }
                string speech = Helpers.GetSpeech(paragraphs[i].InnerText, newSpeaker);
                string[] row = new string[4];
                rows.Add(new string[4]);
                row[0] = "TK";
                row[1] = "TK-Date";
                row[2] = speaker;
                row[3] = speech;
                rows.Add(row);
            }
            WriteCsv(rows);
            Console.WriteLine("press any key to continue");
            Console.Read();
        }

        public static void WriteCsv(IList<string[]> rows)
        {
            using (var writer = new StreamWriter("output.csv", false, Encoding.ASCII))
            {

                using (var csv = new CsvWriter(writer))
                {

                    foreach (var row in rows)
                    {
                        foreach (var column in row)
                        {
                            csv.WriteField(column);
                        }
                        csv.NextRecord();
                    }
                }
            }
        }

    }

    public static class Helpers
    {
        public static string FindSpeaker(string text)
        {
            var indexOfColon = IndexOfColon(text);
            // if the colon is not in the text there is no new speaker
            if (indexOfColon == -1)
                return string.Empty;
            var speaker = text.Substring(0, indexOfColon);

            // need to determine if paragrah as a colon elsewhere but no new speaker
            // the previous charachter immediately before the colon MUST BE upper case. Otherwise it's just in the sentence and isn't a new speaker
            // there's probably other cases: for example, acronyms then a colon in a regular sentence.
            if(char.IsUpper(text[indexOfColon - 1]))
                return speaker;
            return string.Empty;
        }

        private static int IndexOfColon(string text)
        {
            var colon = new char[1];
            colon[0] = ':';
            var positionOfColon = text.IndexOfAny(colon);
            return positionOfColon;
        }

        public static string GetSpeech(string paragraph, string speeker)
        {
            if (speeker.IsNullOrEmpty())
                return paragraph;
            var indexOfColon = IndexOfColon(paragraph);
            // we don't want the colon or the space after it, so go two ahead. May not be necessary with .Trim()
            return paragraph.Substring(indexOfColon + 2).Trim();
        }
        public static bool IsLower(this string value)
        {
            // Consider string to be lowercase if it has no uppercase letters.
            for (int i = 0; i < value.Length; i++)
            {
                if (char.IsUpper(value[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
