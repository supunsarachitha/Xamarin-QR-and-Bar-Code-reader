using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using Android;
using QRCodeReader;
using SQLite;

namespace BuiltInViews {
    [Activity(Label = "History", Icon = "@drawable/icon" , Theme = "@android:style/Theme.Material.Light")]
    public class HomeScreen : ListActivity {
        
        List<TableItem> tableItems = new List<TableItem>();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            var conn = new SQLiteConnection(Common.DirectoryPath);
            conn.CreateTable<ConfigDetails>();
            var historyList = conn.Table<ConfigDetails>();

            foreach (var item in historyList)
            {
                tableItems.Add(new TableItem(){ Heading= item.HistryItem, SubHeading = item.date});
            }

            // Select multiple rows for activated ListViews:
            // ListView.ChoiceMode = ChoiceMode.Multiple;
            ListView.ChoiceMode = ChoiceMode.Single;
            ListView.FastScrollEnabled = true;
            ListAdapter = new HomeScreenAdapter(this, tableItems);
            


        }
        protected override void OnListItemClick(ListView l, View v, int position, long id)
        {
            var t = tableItems[position];
            string item = t.Heading;

            if (item.StartsWith("https://") || item.StartsWith("http://") || item.StartsWith("www."))
            {
                Android.Net.Uri uri = Android.Net.Uri.Parse(item);
                var Browser = new Intent(Intent.ActionView, uri);
                Browser.AddFlags(ActivityFlags.ExcludeFromRecents);
                Browser.SetFlags(ActivityFlags.NoHistory);
                StartActivity(Browser);
            }
            else
            {
                Intent sendIntent = new Intent();
                sendIntent.SetAction(Intent.ActionSend);
                sendIntent.PutExtra(Intent.ExtraText, item);
                sendIntent.SetType("text/plain");
                StartActivity(sendIntent);
            }
        }

        public override void OnBackPressed()
        {
            this.Finish();
            base.OnBackPressed();
            
        }




    }
}

