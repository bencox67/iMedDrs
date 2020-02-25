using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views.InputMethods;
using Android.Widget;

namespace iMedDrs.Droid
{
    [Activity(Name = "com.imeddrs.imeddrs.SendActivity", Label = "iMedDrs - Send Report", Icon = "@drawable/Icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@android:style/Theme.Holo.Light")]
    public class SendActivity : Activity
    {
        string baseurl;
        string userid;
        string username;
        string questionnaire;
        string data;
        string email;
        int level;
        string[] message;
        string[] result;
        EditText locations;
        Button myemail;
        Button send;
        Button returns;
        AlertDialog alert;
        AlertDialog progress;
        MServer ms;
        InputMethodManager imm;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Get intent variables
            baseurl = Intent.GetStringExtra("baseurl");
            userid = Intent.GetStringExtra("userid");
            username = Intent.GetStringExtra("username");
            questionnaire = Intent.GetStringExtra("questionnaire");
            data = Intent.GetStringExtra("data");
            email = Intent.GetStringExtra("email");
            level = Intent.GetIntExtra("level", 0);

            // Set title
            this.ActionBar.Subtitle = username + " - " + questionnaire;

            // Set our view from the "send" layout resource
            SetContentView(Resource.Layout.Send);

            // Initialize variables
            locations = FindViewById<EditText>(Resource.Id.locations);
            myemail = FindViewById<Button>(Resource.Id.myemail);
            send = FindViewById<Button>(Resource.Id.send);
            returns = FindViewById<Button>(Resource.Id.returns);
            imm = (InputMethodManager)GetSystemService(Context.InputMethodService);

            // Initialize events
            myemail.Click += Myemail_Click;
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

            if (email == "")
                myemail.Visibility = Android.Views.ViewStates.Gone;
            else
            {
                if (locations.Text != "" && !locations.Text.EndsWith("\n"))
                    locations.Text += "\n";
                locations.Text += email + "\n";
            }
        }

        public override void OnBackPressed()
        {
            return;
        }

        private void Myemail_Click(object sender, EventArgs e)
        {
            if (!locations.Text.Contains(email))
            {
                if (locations.Text != "" && !locations.Text.EndsWith("\n"))
                    locations.Text += "\n";
                locations.Text += email + "\n";
            }
        }

        private async void Send_Click(object sender, EventArgs e)
        {
            imm.HideSoftInputFromWindow(locations.WindowToken, 0);
            string location = "~";
            if (locations.Text != "")
            {
                location = locations.Text.Replace("\n", "|");
                location = location.Replace("||", "|");
                if (location.EndsWith("|"))
                    location = location.Substring(0, location.Length - 1);
            }
            progress.Show();
            if (level < 2)
                message = new string[] { "report", "send2", userid, questionnaire, location, data };
            else
                message = new string[] { "report", "send", userid, questionnaire, location };
            await Task.Run(() => result = ms.ProcessMessage(message, "GET"));
            progress.Dismiss();
            if (result[2] != "" && result[2] != "~")
                AlertMessage(result[2]);
        }

        private void Returns_Click(object sender, EventArgs e)
        {
            Finish();
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