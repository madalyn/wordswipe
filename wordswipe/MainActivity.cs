using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Json;

namespace wordswipe
{
	// also no one is using wordlash
	[Activity (Label = "wordswipe", MainLauncher = true, Icon = "@drawable/icon", Theme = "@style/Theme.Custom")]
	public class MainActivity : Activity
	{
		// UI Elements
		TextView currentWordView;
		TextView currentDefinitionView;
		TextView swipeYesView;
		TextView swipeNoView;
		Button viewWordsButton;

		WordGenerator generator;
		Stack<Tuple<string,string>> currentWordSet;
		Task<Stack<Tuple<string,string>>> nextWordSet;

		string currentWord;

		/*
		 * setting for looping through words
		 * setting for language of words (later)
		 * setting for difficultly/level of words (by grade maybe?)
		 * setting for length of words
		*/
		protected override void OnCreate (Bundle bundle)
		{
			// TODO: rotating the screen restarts the app!

			base.OnCreate (bundle);

			Window.SetBackgroundDrawable (Resources.GetDrawable (Resource.Color.wordswipe_background));

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.ViewManager);
			// set the splash screen to visible first (default)

			SetActionBar (FindViewById<Toolbar> (Resource.Id.toolbar));

			// get the TextViews from the UI
			currentWordView = FindViewById<TextView> (Resource.Id.currentWord);
			currentDefinitionView = FindViewById<TextView> (Resource.Id.definition);
			swipeYesView = FindViewById<TextView> (Resource.Id.swipeYes);
			swipeNoView = FindViewById<TextView> (Resource.Id.swipeNo);
			viewWordsButton = FindViewById<Button> (Resource.Id.viewWordsButton);

			generator = new WordGenerator (Assets.Open, GetFileStreamPath ("dict.txt").AbsolutePath);
			InitializeWordSet ();

			//FIXME: need to store this so it doesn't reset each time the user restarts the app
			List<string> learnedWords = new List<string> ();

			swipeYesView.Click += delegate {
				UpdateCurrentWord ();
			};

			swipeNoView.Click += delegate {
				learnedWords.Add (currentWord);
				UpdateCurrentWord ();
			};

			viewWordsButton.Click += (sender, e) =>
			{
				var intent = new Intent(this, typeof(WordsLearnedActivity));
				intent.PutStringArrayListExtra("learned_words", learnedWords);
				StartActivity(intent);
			};
		}

		// TODO: need safety to fix bug if user clicks too fast
		void UpdateCurrentWord ()
		{
			currentWordView.Visibility = ViewStates.Invisible;
			currentDefinitionView.Visibility = ViewStates.Invisible;

			if (!currentWordSet.Any ()) {
				//fetch more words
				if (nextWordSet != null) {
					currentWordSet = nextWordSet.Result;
					nextWordSet = generator.PopulateNextWordSet ();
				} else {
					// TODO: show spinner
					nextWordSet = generator.PopulateNextWordSet ();
					currentWordSet = nextWordSet.Result;
				}
			}

			Tuple<string,string> currentSet = currentWordSet.Pop();
			currentWord = currentSet.Item1;
			currentWordView.Text = currentSet.Item1;
			currentDefinitionView.Text = currentSet.Item2;

			currentWordView.Visibility = ViewStates.Visible;
			currentDefinitionView.Visibility = ViewStates.Visible;
		}

		async void InitializeWordSet ()
		{
			currentWordSet = await generator.PopulateNextWordSet ();
			nextWordSet = generator.PopulateNextWordSet ();
			UpdateCurrentWord ();
			// switch the splash screen to the main screen
			FindViewById (Resource.Id.launchLayout).Visibility = ViewStates.Gone;
		}
	}
	
}


