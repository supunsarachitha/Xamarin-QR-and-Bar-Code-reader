using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace PandaQRCodeReader
{
    [Activity(Label = "History")]
    public class History : ListActivity
    {
        

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetActionBar(toolbar);

             ListAdapter = new ArrayAdapter<string>(this, Resource.Layout.HistoryList, Common.HistoryItem.ToArray());

            ListView.TextFilterEnabled = true;
            ListView.FastScrollEnabled = true;

            ListView.ItemClick += ListView_ItemClick;

        }

        private void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            string item = ListAdapter.GetItem(e.Position).ToString();

            if (item.StartsWith("https://") || item.StartsWith("http://"))
            {
                Android.Net.Uri uri = Android.Net.Uri.Parse(item);
                var Browser = new Intent(Intent.ActionView, uri);
                Browser.AddFlags(ActivityFlags.ExcludeFromRecents);
                Browser.SetFlags(ActivityFlags.NoHistory);
                StartActivity(Browser);
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            this.Finish();
        }








    }
}