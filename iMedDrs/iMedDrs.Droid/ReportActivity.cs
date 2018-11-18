using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Webkit;
using Android.Widget;

namespace iMedDrs.Droid
{
    [Activity(Name = "com.imeddrs.imeddrs.ReportActivity", Label = "iMedDrs - Report", Icon = "@drawable/Icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Sensor, ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize, Theme = "@android:style/Theme.Holo.Light")]
    public class ReportActivity : Activity
    {
        string baseurl;
        string userid;
        string username;
        string questionnaire;
        string text;
        string data;
        string email;
        string[] message;
        string[] result;
        int level;
        int last;
        int number;
        WebView report;
        Button next;
        Button previous;
        Button send;
        Button returns;
        AlertDialog alert;
        AlertDialog progress;
        MServer ms;

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
            text = Intent.GetStringExtra("text");
            data = Intent.GetStringExtra("data");
            email = Intent.GetStringExtra("email");
            level = Intent.GetIntExtra("level", 0);

            // Set title
            this.ActionBar.Subtitle = username + " - " + questionnaire;

            // Set our view from the "report" layout resource
            SetContentView(Resource.Layout.Report);

            // Initialize variables
            report = FindViewById<WebView>(Resource.Id.report);
            //report.Settings.DefaultTextEncodingName = "utf-8";
            report.LoadDataWithBaseURL(null, text, "text/html", "utf-8", null);
            next = FindViewById<Button>(Resource.Id.next);
            previous = FindViewById<Button>(Resource.Id.previous);
            send = FindViewById<Button>(Resource.Id.send);
            returns = FindViewById<Button>(Resource.Id.returns);

            // Initialize events
            next.Click += Next_Click;
            previous.Click += Previous_Click;
            send.Click += Send_Click;
            returns.Click += Returns_Click;

            // Alert dialog for messages
            AlertDialog.Builder alertbuilder1 = new AlertDialog.Builder(this);
            alertbuilder1.SetPositiveButton("Ok", (EventHandler<DialogClickEventArgs>)null);
            alert = alertbuilder1.Create();

            // Progress dialog for messaging
            AlertDialog.Builder alertbuilder2 = new AlertDialog.Builder(this);
            alertbuilder2.SetView(LayoutInflater.Inflate(Resource.Layout.Progress, null));
            progress = alertbuilder2.Create();
            progress.SetCancelable(false);

            // Initialize messaging
            ms = new MServer(baseurl);
        }

        public override void OnBackPressed()
        {
            return;
        }

        private void Next_Click(object sender, EventArgs e)
        {
            number++;
            if (number == last)
                number = 0;
            ShowReport();
        }

        private void Previous_Click(object sender, EventArgs e)
        {
            number--;
            if (number < 0)
                number = last - 1;
            ShowReport();
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
            intent.PutExtra("level", level);
            StartActivity(intent);
        }

        private void Returns_Click(object sender, EventArgs e)
        {
            Finish();
        }

        private async void ShowReport()
        {
            progress.Show();
            if (level < 2)
                message = new string[] { "report", "load2", userid, questionnaire, number.ToString(), data };
            else
                message = new string[] { "report", "load", userid, questionnaire, number.ToString() };
            await Task.Run(() => result = ms.ProcessMessage(message, "GET"));
            progress.Dismiss();
            if (result[1] == "ack")
            {
                Intent intent = new Intent(this.ApplicationContext, typeof(ReportActivity));
                last = Convert.ToInt32(result[2]);
                number = Convert.ToInt32(result[3]);
                report.LoadDataWithBaseURL(null, result[4], "text/html", "utf-8", null);
            }
            else
                AlertMessage(result[2]);
        }

        private void AlertMessage(string messagetext)
        {
            if (messagetext != "")
            {
                alert.SetMessage(messagetext);
                alert.Show();
            }
        }
    }
}