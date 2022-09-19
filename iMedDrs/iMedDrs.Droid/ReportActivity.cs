using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Webkit;
using Android.Widget;
using Newtonsoft.Json;

namespace iMedDrs.Droid
{
    [Activity(Name = "com.imeddrs.imeddrs.ReportActivity", Label = "iMedDrs - Report", Icon = "@drawable/Icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Sensor, ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize, Theme = "@android:style/Theme.Holo.Light")]
    public class ReportActivity : Activity
    {
        string baseurl;
        string userid;
        string username;
        string questionnaire;
        string reportjson;
        string data;
        string email;
        string role;
        string language;
        int last;
        int number;
        List<Report> reports;
        WebView report;
        Button next;
        Button previous;
        Button send;
        Button returns;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Get intent variables
            baseurl = Intent.GetStringExtra("baseurl");
            userid = Intent.GetStringExtra("userid");
            username = Intent.GetStringExtra("username");
            questionnaire = Intent.GetStringExtra("questionnaire");
            last = Intent.GetIntExtra("last", 0);
            number = Intent.GetIntExtra("number", 0);
            reportjson = Intent.GetStringExtra("reportjson");
            data = Intent.GetStringExtra("data");
            email = Intent.GetStringExtra("email");
            role = Intent.GetStringExtra("role");
            language = Intent.GetStringExtra("language");

            // Set title
            this.ActionBar.Subtitle = username + " - " + questionnaire;

            // Set our view from the "report" layout resource
            SetContentView(Resource.Layout.Report);

            // Initialize variables
            report = FindViewById<WebView>(Resource.Id.report);
            ReportModel model = JsonConvert.DeserializeObject<ReportModel>(reportjson);
            reports = model.Reports;
            report.LoadDataWithBaseURL(null, reports[number].Text, "text/html", "utf-8", null);
            next = FindViewById<Button>(Resource.Id.next);
            previous = FindViewById<Button>(Resource.Id.previous);
            send = FindViewById<Button>(Resource.Id.send);
            returns = FindViewById<Button>(Resource.Id.returns);

            // Initialize events
            next.Click += Next_Click;
            previous.Click += Previous_Click;
            send.Click += Send_Click;
            returns.Click += Returns_Click;
        }

        public override void OnBackPressed()
        {
            return;
        }

        private void Next_Click(object sender, EventArgs e)
        {
            number++;
            if (number > last)
                number = 0;
            report.LoadDataWithBaseURL(null, reports[number].Text, "text/html", "utf-8", null);
        }

        private void Previous_Click(object sender, EventArgs e)
        {
            number--;
            if (number < 0)
                number = last;
            report.LoadDataWithBaseURL(null, reports[number].Text, "text/html", "utf-8", null);
        }

        private void Send_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this.ApplicationContext, typeof(SendActivity));
            intent.PutExtra("baseurl", baseurl);
            intent.PutExtra("userid", userid);
            intent.PutExtra("username", username);
            intent.PutExtra("questionnaire", questionnaire);
            intent.PutExtra("data", data);
            intent.PutExtra("email", email);
            intent.PutExtra("role", role);
            intent.PutExtra("language", language);
            StartActivity(intent);
        }

        private void Returns_Click(object sender, EventArgs e)
        {
            Finish();
        }
    }
}