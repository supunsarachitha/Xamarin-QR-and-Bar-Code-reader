using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using ZXing.Mobile;
using ZXing;
using Android.Content.PM;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Java.IO;
using System.Linq;
using System.Collections;
using SQLite;
using BuiltInViews;
using Android.Hardware.Camera2;

[assembly: UsesPermission(Android.Manifest.Permission.Flashlight)]
namespace PandaQRCodeReader
{
	[Activity (Label = "QR & Barcode Reader", Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
        Button buttonScanDefaultView;
        MobileBarcodeScanner scanner;
        string value = "";
        bool torch = false;
        public View zxingOverlay;
        public Button flashButton;

        protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
            MobileBarcodeScanner.Initialize(Application);
  
            SetContentView (Resource.Layout.Main);
            Window.SetBackgroundDrawableResource(Resource.Drawable.background);

            scanner = new MobileBarcodeScanner();
            scanner.AutoFocus();
            scanner.FlashButtonText = "flash";
            
            var toolbar = FindViewById<Toolbar> (Resource.Id.toolbar);

			//Toolbar will now take on default actionbar characteristics
			SetActionBar (toolbar);
            ActionBar.SetIcon(Resource.Drawable.iconsamll);

			ActionBar.Title = "QR & Bar Code Reader";
            ActionBar.Title.TrimEnd();

			var btnSingle = FindViewById<Button> (Resource.Id.btnSingle);
            btnSingle.Click += BtnSingle_Click;

            var btnMulty = FindViewById<Button>(Resource.Id.btnMulty);
            btnMulty.Click += BtnMulty_Click;


            Common.HistoryItem = new List<string>();
            readXml();




            var toolbarBottom = FindViewById<Toolbar>(Resource.Id.toolbar_bottom);
            //toolbarBottom.Title = "Settings";
            toolbarBottom.InflateMenu(Resource.Menu.photo_edit);
            toolbarBottom.MenuItemClick += (sender, e) =>
            {
                if (e.Item.TitleFormatted.ToString() == "Delete")
                {

                    Android.App.AlertDialog.Builder dialog = new AlertDialog.Builder(this);
                    AlertDialog alert = dialog.Create();
                    
                    alert.SetMessage("Clear History?");
                    alert.SetButton("OK", (c, ev) =>
                    {
                        var conn = new SQLiteConnection(Common.DirectoryPath);
                        conn.Execute("Delete FROM ConfigDetails");
                        Toast.MakeText(this, "Complete", ToastLength.Short).Show();
                    });
                    alert.SetButton2("CANCEL", (c, ev) => { });
                    alert.Show();                   
                }                
            };
        }


        private async void BtnSingle_Click(object sender, EventArgs e)
        {
            //Tell our scanner we want to use a custom overlay instead of the default
            scanner.UseCustomOverlay = true;

            //Inflate our custom overlay from a resource layout
            zxingOverlay = LayoutInflater.FromContext(this).Inflate(Resource.Layout.ZxingOverlay, null);

            //Find the button from our resource layout and wire up the click event
            flashButton = zxingOverlay.FindViewById<Button>(Resource.Id.buttonZxingFlash);
            flashButton.Click += FlashButton_Click;

            //Set our custom overlay
            scanner.CustomOverlay = zxingOverlay;

            //Start scanning!
            var result = await scanner.Scan(new MobileBarcodeScanningOptions { AutoRotate = true });

            HandleScanResult(result);
        }

        private void BtnMulty_Click(object sender, EventArgs e)
        {

            //Tell our scanner we want to use a custom overlay instead of the default
            scanner.UseCustomOverlay = true;

            //Inflate our custom overlay from a resource layout
            zxingOverlay = LayoutInflater.FromContext(this).Inflate(Resource.Layout.ZxingOverlay, null);

            //Find the button from our resource layout and wire up the click event
            flashButton = zxingOverlay.FindViewById<Button>(Resource.Id.buttonZxingFlash);
            flashButton.Click += FlashButton_Click;

            //Set our custom overlay
            scanner.CustomOverlay = zxingOverlay;

            var opt = new MobileBarcodeScanningOptions();
            opt.DelayBetweenContinuousScans = 3000;

            //Start scanning
            scanner.ScanContinuously(opt, HandleScanResult);
        }

        private void FlashButton_Click(object sender, EventArgs e)
        {
            scanner.ToggleTorch();

            if (scanner.IsTorchOn)
            {

                flashButton.SetBackgroundResource(Resource.Drawable.flash);
            }
            else
            {
                flashButton.SetBackgroundResource(Resource.Drawable.flashoff);
            }
        }


        /// <Docs>The options menu in which you place your items.</Docs>
        /// <returns>To be added.</returns>
        /// <summary>
        /// This is the menu for the Toolbar/Action Bar to use
        /// </summary>
        /// <param name="menu">Menu.</param>
        public override bool OnCreateOptionsMenu (IMenu menu)
		{
			MenuInflater.Inflate (Resource.Menu.home, menu);
			return base.OnCreateOptionsMenu (menu);
		}


        void HandleScanResult(ZXing.Result result)
        {
            string msg = "";

            if (result != null && !string.IsNullOrEmpty(result.Text))
            {
                msg = "Found Barcode: " + result.Text;
                updateValue(result.Text, result.BarcodeFormat.ToString());
                WriteXml(result.Text);
            }


            else
            {
                msg = "Scanning Canceled!";
            }
               

            this.RunOnUiThread(() => Toast.MakeText(this, msg, ToastLength.Short).Show());
        }



        public override bool OnOptionsItemSelected (IMenuItem item)
		{

            if (item.ToString()== "Share")
            {
                if (value != "" || value != null)
                {
                    //var ValueField = FindViewById<TextView>(Resource.Id.Result);

                    Intent sendIntent = new Intent();
                    sendIntent.SetAction(Intent.ActionSend);
                    sendIntent.PutExtra(Intent.ExtraText, "https://play.google.com/store/apps/details?id=lk.stechbuzz.qrreader");
                    sendIntent.SetType("text/plain");
                    StartActivity(sendIntent);
                }
            }
            else if( item.ToString() == "Rate this App ")
            {
                this.RunOnUiThread(() => Toast.MakeText(this, "Thank you!", ToastLength.Short).Show());
                Android.Net.Uri uri = Android.Net.Uri.Parse("market://details?id=" + "lk.stechbuzz.qrreader");
                var openmarket = new Intent(Intent.ActionView, uri);
                openmarket.AddFlags(ActivityFlags.ExcludeFromRecents);
                openmarket.SetFlags(ActivityFlags.NoHistory);
                StartActivity(openmarket);
            }
            else if (item.ToString() == "History")
            {
                var openmarket = new Intent(this, typeof(HomeScreen));
                StartActivity(openmarket);
            }
            else if (item.ToString() == "Our apps")
            {
                Android.Net.Uri uri = Android.Net.Uri.Parse("market://details?id=" + "lk.stechbuzz.electronic");
                var openmarket = new Intent(Intent.ActionView, uri);
                openmarket.AddFlags(ActivityFlags.ExcludeFromRecents);
                openmarket.SetFlags(ActivityFlags.NoHistory);
                StartActivity(openmarket);
            }


            return base.OnOptionsItemSelected (item);
		}

        protected override void OnResume()
        {
            base.OnResume();

            if (ZXing.Net.Mobile.Android.PermissionsHandler.NeedsPermissionRequest(this))
                ZXing.Net.Mobile.Android.PermissionsHandler.RequestPermissionsAsync(this);

            
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            global::ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        [Java.Interop.Export("UITestBackdoorScan")]
        public Java.Lang.String UITestBackdoorScan(string param)
        {
            var expectedFormat = BarcodeFormat.QR_CODE;
            Enum.TryParse(param, out expectedFormat);
            var opts = new MobileBarcodeScanningOptions
            {
                PossibleFormats = new List<BarcodeFormat> { expectedFormat }
            };
            var barcodeScanner = new MobileBarcodeScanner();

            System.Console.WriteLine("Scanning " + expectedFormat);

            //Start scanning
            barcodeScanner.Scan(opts).ContinueWith(t => {

                var result = t.Result;

                var format = result?.BarcodeFormat.ToString() ?? string.Empty;
                value = result?.Text ?? string.Empty;

                RunOnUiThread(() => {

                    AlertDialog dialog = null;
                    dialog = new AlertDialog.Builder(this)
                                    .SetTitle("Barcode Result")
                                    .SetMessage(format + "|" + value)
                                    .SetNeutralButton("OK", (sender, e) => {
                                        dialog.Cancel();
                                    }).Create();
                    dialog.Show();

                    
                });
            });

            return new Java.Lang.String();
        }

        private void updateValue(string val, string format)
        {
            var ValueField = FindViewById<TextView>(Resource.Id.Result);
            ValueField.Text = val;

            var btnBrowser = FindViewById<Button>(Resource.Id.btnBrowse);
            var txtBrowser = FindViewById<TextView>(Resource.Id.browserTxt);

            if (val.StartsWith("https://") || val.StartsWith("http://"))
            {
                btnBrowser.Visibility = ViewStates.Visible;
                txtBrowser.Visibility = ViewStates.Visible;
                btnBrowser.SetBackgroundResource(Resource.Drawable.browser);
                txtBrowser.Text = "Open in web browser";
                btnBrowser.Click += delegate
                {
                    Android.Net.Uri uri = Android.Net.Uri.Parse(ValueField.Text);
                    var Browser = new Intent(Intent.ActionView, uri);
                    Browser.AddFlags(ActivityFlags.ExcludeFromRecents);
                    Browser.SetFlags(ActivityFlags.NoHistory);
                    StartActivity(Browser);
                };
            }
            else
            {
                btnBrowser.SetBackgroundResource(Resource.Drawable.share);
                txtBrowser.Text = "Share";
                btnBrowser.Click += delegate
                {
                    Intent sendIntent = new Intent();
                    sendIntent.SetAction(Intent.ActionSend);
                    sendIntent.PutExtra(Intent.ExtraText, ValueField.Text);
                    sendIntent.SetType("text/plain");
                    StartActivity(sendIntent);
                };
                btnBrowser.Visibility = ViewStates.Visible;
                txtBrowser.Visibility = ViewStates.Visible;
            }


        }


        //private void BtnBrowser_Click(object sender, EventArgs e)
        //{
        //    var ValueField = FindViewById<TextView>(Resource.Id.Result);

        //    Android.Net.Uri uri = Android.Net.Uri.Parse(ValueField.Text);
        //    var Browser = new Intent(Intent.ActionView, uri);
        //    Browser.AddFlags(ActivityFlags.ExcludeFromRecents);
        //    Browser.SetFlags(ActivityFlags.NoHistory);
        //    StartActivity(Browser);
        //}


        public void WriteXml(string result)
        { 
            var conn = new SQLiteConnection(Common.DirectoryPath);
            var table = new ConfigDetails();
            table.HistryItem = result;
            table.date = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
            conn.Insert(table);            
        }

        private void readXml()
        { 
            var conn = new SQLiteConnection(Common.DirectoryPath);
            conn.CreateTable<ConfigDetails>();
            var historyList = conn.Table<ConfigDetails>();

            foreach (var item in historyList)
            {
                Common.HistoryItem.Add(item.HistryItem);
            }

        }


        public override void OnBackPressed()
        {
            
            Android.App.AlertDialog.Builder dialog = new AlertDialog.Builder(this);
            AlertDialog alert = dialog.Create();
            alert.SetTitle("Exit");
            alert.SetMessage("Do You want to exit?");
            alert.SetIcon(Resource.Drawable.alert);
            alert.SetButton("OK", (c, ev) =>
            {
                base.OnBackPressed(); 
            });
            alert.SetButton2("CANCEL", (c, ev) => { });
            alert.Show();

            

        }
    }



    public class ConfigDetails
    {
        [PrimaryKey, AutoIncrement, Column("_Id")]

        public int id { get; set; }

        public string HistryItem { get; set; }

        public string date { get; set; }
    }
}


