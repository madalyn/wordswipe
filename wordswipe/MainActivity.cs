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
		WordGenerator generator;

		/*
		 * setting for looping through words
		 * setting for language of words (later)
		 * setting for difficultly/level of words (by grade maybe?)
		 * setting for length of words
		*/
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);
			generator = new WordGenerator (Assets.Open);

			// get the TextViews from the UI
			currentWordView = FindViewById<TextView> (Resource.Id.currentWord);
			currentDefinitionView = FindViewById<TextView> (Resource.Id.definition);
			swipeYesView = FindViewById<TextView> (Resource.Id.swipeYes);
			swipeNoView = FindViewById<TextView> (Resource.Id.swipeNo);

			UpdateCurrentWord ();
			//List<string> unknownWords = new List<string> ();

			// swiping actions for left & right need to be set
			swipeYesView.Click += delegate {
				UpdateCurrentWord ();
			};

			swipeNoView.Click += delegate {
				UpdateCurrentWord ();
				//unknownWords.Add (currentWordView.Text);
			};


			// TODO: menu for list of newly learned words
		}

		async void UpdateCurrentWord ()
		{
			currentWordView.Visibility = ViewStates.Invisible;
			currentDefinitionView.Visibility = ViewStates.Invisible;

			var entry = await generator.GetNextWordEntry ();

			currentWordView.Visibility = ViewStates.Visible;
			currentDefinitionView.Visibility = ViewStates.Visible;

			currentWordView.Text = entry.Item1;
			currentDefinitionView.Text = entry.Item2;
		}
	}
	
}


