
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;

namespace wordswipe
{
	// could be the heading here, also need back arrow
	[Activity (Label = "wordswipe", Icon = "@drawable/icon", Theme = "@style/Theme.Custom")]	
	public class WordsLearnedActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.WordsLearned);

			// Add the list of words the user didn't know
			// nest their definitions in the ExpandableListView

			//view = context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItem1, null);
			ListView wordsListView = FindViewById<ListView> (Resource.Id.wordsListView);
			//view.FindViewById<TextView> (Android.Resource.Id.Background).SetBackgroundColor (Color.White);
			//view.FindViewById<TextView>(Android.Resource.Id.Text2).Text = item.SubHeading;

			// the list of words that were learned
			var learnedWords = Intent.Extras.GetStringArrayList("learned_words") ?? new string[0];
			var adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1, learnedWords);
			wordsListView.Adapter = (adapter);

		}
	}
}

