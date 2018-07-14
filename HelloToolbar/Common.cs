using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace PandaQRCodeReader
{
    public class Common
    {
        public static List<string> HistoryItem { get; internal set; }
        public static string DirectoryPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "stechbuzz.db3");

    }
}