using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Json;

namespace wordswipe
{
	public class WordGenerator
	{
		//var words = new List<string>{"hello", "world", "misanthrope", "sanctimonious", "yellow"};
		// store chunks of words at a time
		// maybe need wordClass for word data

		static Random random = new Random();
		static List<string> allWords;
		static Stack<string> wordStack;

		// to call the api to get the word definitions
		HttpClient client;
		Func<string, Stream> resOpener;

		public WordGenerator (Func<string, Stream> resOpener)
		{
			
			this.resOpener = resOpener;
			client = new HttpClient ();
			allWords = FetchWordsFromAsset ();
			// the stack of words, already randomly sorted
			wordStack = new Stack<string> (allWords.OrderBy (w => random.Next ()));
		}

		List<string> FetchWordsFromAsset ()
		{
			var groomedWordList = new List<string> (200); // set this to a number so the array doesn't have to be reset
			using (var reader = new StreamReader (resOpener ("testwords.txt"))) {
				string line = null;
				while ((line = reader.ReadLine ()) != null) {
					// filter out words that start with a capital letter, contain "."
					// TODO: filter out non-root words so not to ascertain silly definitions
					if (!line.Contains (".") && !line.Any(char.IsDigit) && !line.Any(char.IsUpper))
						groomedWordList.Add (line);
				}
			}
			return groomedWordList;
		}

		async Task<string> GetCurrentDefinition (string currentWord)
		{
			string currentDefinition = "no definition";

			// use word randomly selected to query for the definition
			var url = "http://api.wordnik.com:80/v4/word.json/"+ currentWord +"/definitions?limit=1&includeRelated=false&sourceDictionaries=all&useCanonical=false&includeTags=false&api_key=a2a73e7b926c924fad7001ca3111acd55af2ffabf50eb4ae5";

			// ConfigureAwait->false to stay on the same thread while getting word definitions
			var json = await client.GetStringAsync (url).ConfigureAwait (false);
			var result = (JsonObject)((JsonArray)JsonValue.Parse (json)) [0];
			if (!string.IsNullOrEmpty (result ["text"]))
				currentDefinition = result ["text"];


			// want "text" for definition
			//return result ["text"];

			// TODO: need to try a new word if we fail (optimize this, needs to go fast)


			// definition could not be found
			return currentDefinition;

		}


		public async Task<Tuple<string, string>> GetNextWordEntry ()
		{
			string currentWord, currentDefinition;

			while (true) {
				try {
					if (wordStack.Any ()) {
						// on update, set the word & definition
						currentWord = wordStack.Pop ();
						currentDefinition = await GetCurrentDefinition (currentWord).ConfigureAwait (false);
						break;
					} else {
						// eventually need to circle through the list of unknown words
						// need to tell them they've learned all the words
						currentWord = "You've learned all the words!";
						currentDefinition = string.Empty;
						break;
					}
				} catch (Exception e) {
					Android.Util.Log.Error ("UpdateCurrentWord", e.ToString ());
				}
			}

			return Tuple.Create (currentWord, currentDefinition);
		}
	}
}
