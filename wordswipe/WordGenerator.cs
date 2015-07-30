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
		//allWords = FetchWordsFromAsset ();
		// the stack of words, already randomly sorted
		//wordStack = new Stack<string> (allWords.OrderBy (w => random.Next ()));
		// store chunks of words at a time
		// maybe need wordClass for word data

		HttpClient client;
		Func<string, Stream> resOpener;
		static Random random = new Random();
		int [] wordReferencesList;
		string dictStorePath;

		public WordGenerator (Func<string, Stream> resOpener, string dictStorePath)
		{
			this.resOpener = resOpener;
			this.dictStorePath = dictStorePath;
			client = new HttpClient ();
			wordReferencesList = FetchWordReferencesFromAsset ();

			using (var dictAsset = resOpener ("EnglishWords.txt"))
			using (var output = File.Create (dictStorePath))
				dictAsset.CopyTo (output);
		}

		int[] FetchWordReferencesFromAsset ()
		{
			using (var binReader = new BinaryReader (resOpener ("word_indexes.bin"))) {
				// set the array size to the first value, which we set as the number of word references
				wordReferencesList = new int [binReader.ReadInt32 ()];

				// read each int from bin file set to current array index
				// we are ignoring the first int in word_indexes.bin which we had set to the number of words
				// this is because we have already read it, and will now just continue reading the file
				for (int i = 1; i < wordReferencesList.Length; i++) {
					wordReferencesList [i] = binReader.ReadInt32 ();
				}
			}

			return wordReferencesList;
		}

		// populate the next set of words to load
		// returns a dictionary of (up to) 20 randomly selected words and their definitions
		public async Task<Stack<Tuple<string, string>>> PopulateNextWordSet ()
		{
			// gets the definitions & sets up words on separate thread
			// get the random numbers (set ammount) get those references

			Stack<Tuple<string,string>> randomWords = new Stack<Tuple<string, string>>();

			var file = File.OpenRead (dictStorePath);
			using (var reader = new StreamReader (file)) {
				// FIXME: only problem is now we don't have a way to avoid repeat words
				// get 20 words from EnglishWords.txt based on their index positions
				for (int i = 0; i < 20; i++) {
					// read word at corresponding index
					var rndIndex = random.Next (wordReferencesList.Length);
					reader.BaseStream.Position = wordReferencesList [rndIndex];
					string currentWord = reader.ReadLine ();
					Console.WriteLine ("Word at the start: {0}", currentWord);

					// filter out words that start with a capital letter, contain "." or numbers
					// filter out non-root words so not to ascertain silly definitions (ends with "s")
					if (!currentWord.Contains (".") && !currentWord.Any (char.IsDigit) && !currentWord.Any (char.IsUpper) && !currentWord.EndsWith ("s")) {
						while (true) {
							try {
								var definition = await GetDefinitionForWord (reader.ReadLine ());
								if (definition != null)
									randomWords.Push (new Tuple<string,string> (currentWord, definition));
								break;
							} catch (Exception e) {
								Android.Util.Log.Error ("UpdateCurrentWord, couldn't get definition", e.ToString ());
							}
						}
					}
					reader.DiscardBufferedData ();
				}
			}

			return randomWords;
		}

		async Task<string> GetDefinitionForWord (string word)
		{
			string definition = null;

			// use word randomly selected to query for the definition
			var url = "http://api.wordnik.com:80/v4/word.json/"+ word +"/definitions?limit=1&includeRelated=false&sourceDictionaries=all&useCanonical=false&includeTags=false&api_key=a2a73e7b926c924fad7001ca3111acd55af2ffabf50eb4ae5";

			// ConfigureAwait->false to stay on the same thread while getting word definitions
			var json = await client.GetStringAsync (url).ConfigureAwait (false);
			var array = (JsonArray)JsonValue.Parse (json);
			if (array.Count == 0)
				return definition;
			var result = (JsonObject)array [0];
			Console.WriteLine ("Word at the end: {0}", (string)result ["word"]); //TODO: use this
			// "text" gives us the definition from the JSON
			if (!string.IsNullOrEmpty (result ["text"]))
				definition = result ["text"];

			// TODO: need an error message if can't connect to internet

			// definition could not be found
			return definition;

		}

		/*
		List<string> FetchWordsFromAsset ()
		{
			//var file = File.Open ("dict.txt"); var reader = new StreamReader (file); file.Position = array[2]; reader.ReadLine ();
			var groomedWordList = new List<string> (200); // set this to a number so the array doesn't have to be reset
			using (var reader = new StreamReader (resOpener ("testwords.txt"))) {
				string line = null;
				while ((line = reader.ReadLine ()) != null) {
					// filter out words that start with a capital letter, contain "." or numbers
					// filter out non-root words so not to ascertain silly definitions (ends with "s")
					if (!line.Contains (".") && !line.Any(char.IsDigit) && !line.Any(char.IsUpper) && !line.EndsWith("s"))
						groomedWordList.Add (line);
				}
			}
			return groomedWordList;
		}*/

		/*
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
						currentWord = "You've learned all the words!"; // FIXME: don't want this to actually be the word
						currentDefinition = string.Empty;
						break;
					}
				} catch (Exception e) {
					Android.Util.Log.Error ("UpdateCurrentWord", e.ToString ());
				}
			}

			return Tuple.Create (currentWord, currentDefinition);
		}
		*/
	}
}
