using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace wordswipe
{
	// also no one is using wordlash
	[Activity (Label = "wordswipe", MainLauncher = true, Icon = "@drawable/icon", Theme = "@style/Theme.Custom")]
	public class MainActivity : Activity
	{
		int count = 1;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
//			Button button = FindViewById<Button> (Resource.Id.myButton);
//			
//			button.Click += delegate {
//				button.Text = string.Format ("{0} clicks!", count++);
//			};



			// on page refresh, set the word & definition
			var random = new Random();
			var allWords = new List<string>{"hello", "world", "misanthrope", "sanctimonious", "yellow"};
			var wordStack = new Stack<string>(allWords.OrderBy(w => random.Next()));

			TextView currentWord = (TextView)FindViewById (Resource.Id.currentWord);
			currentWord.Text = wordStack.Pop();

			// get definition
			TextView currentDefinition = (TextView)FindViewById (Resource.Id.definition);
			currentDefinition.Text = GetCurrentDefinition (currentWord.Text);

			// swiping actions for left & right need to be set


			// TODO: menu for list of newly learned words
		}

		string GetCurrentDefinition (string currentWord)
		{
			string currentDefinition = "no definition";

			// use word randomly selected to query for the definition
			var url = "http://api.wordnik.com:80/v4/word.json/"+ currentWord +"/definitions?limit=1&includeRelated=false&sourceDictionaries=all&useCanonical=false&includeTags=false&api_key=a2a73e7b926c924fad7001ca3111acd55af2ffabf50eb4ae5";

			dynamic jResults = JsonConvert.DeserializeObject (WebRequester.getInstance().doWebRequest(url));

			// want "text" for definition
			if (jResults != null && jResults[0] != null && jResults[0]["text"] != null && !string.IsNullOrEmpty(jResults[0]["text"].Value)) {
				currentDefinition = jResults[0]["text"].Value;
			}

			// definition could not be found
			return currentDefinition;
		}
	}
}


