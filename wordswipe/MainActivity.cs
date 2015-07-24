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
			base.OnCreate (bundle);

			Window.SetBackgroundDrawable (Resources.GetDrawable (Resource.Color.wordswipe_background));

			// Set our view from the "main" layout resource
			//SetContentView (Resource.Layout.Main);
			SetContentView (Resource.Layout.ViewManager);
			// set the splash screen to visible first

			SetActionBar (FindViewById<Toolbar> (Resource.Id.toolbar));

			generator = new WordGenerator (Assets.Open, GetFileStreamPath ("dict.txt").AbsolutePath);

			// get the TextViews from the UI
			currentWordView = FindViewById<TextView> (Resource.Id.currentWord);
			currentDefinitionView = FindViewById<TextView> (Resource.Id.definition);
			swipeYesView = FindViewById<TextView> (Resource.Id.swipeYes);
			swipeNoView = FindViewById<TextView> (Resource.Id.swipeNo);
			viewWordsButton = FindViewById<Button> (Resource.Id.viewWordsButton);

			InitializeWordSet ();

			//FIXME: need to store this so it doesn't reset each time the user restarts the app
			List<string> learnedWords = new List<string> ();

			// swiping actions for left & right need to be set
			swipeYesView.Click += delegate {
				UpdateCurrentWord ();
			};

			swipeNoView.Click += delegate {
				UpdateCurrentWord ();
				learnedWords.Add (currentWord);
			};

			viewWordsButton.Click += (sender, e) =>
			{
				var intent = new Intent(this, typeof(WordsLearnedActivity));
				intent.PutStringArrayListExtra("learned_words", learnedWords);
				StartActivity(intent);
			};

			// TODO: menu for list of newly learned words
		}

		void UpdateCurrentWord ()
		{
			currentWordView.Visibility = ViewStates.Invisible;
			currentDefinitionView.Visibility = ViewStates.Invisible;

			if (!currentWordSet.Any ()) {
				//fetch more words
				currentWordSet = nextWordSet.Result;
				nextWordSet = generator.PopulateNextWordSet ();
			}

			Tuple<string,string> currentSet = currentWordSet.Pop();
			currentWord = currentSet.Item1;
			currentWordView.Text = currentSet.Item1;
			currentDefinitionView.Text = currentSet.Item2;

			currentWordView.Visibility = ViewStates.Visible;
			currentDefinitionView.Visibility = ViewStates.Visible;
			/*
			currentWord = entry.Item1;
			currentWordView.Text = entry.Item1;
			currentDefinitionView.Text = entry.Item2;
			*/
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


