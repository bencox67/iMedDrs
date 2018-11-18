using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace iMedDrs.Droid
{
    [Activity(Name = "com.imeddrs.imeddrs.LoginActivity", Label = "iMedDrs - Login", Icon = "@drawable/Icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@android:style/Theme.Holo.Light")]
    public class LoginActivity : Activity
    {
        string baseurl;
        string datapath;
        string[] message;
        string[] result;
        EditText userid;
        EditText password;
        CheckBox rememberme;
        Button login;
        Button register;
        Button returns;
        AlertDialog alert;
        AlertDialog progress;
        MServer ms;
        PServer ps;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Get intent variables
            baseurl = Intent.GetStringExtra("baseurl");
            datapath = Intent.GetStringExtra("datapath");

            // Set our view from the "login" layout resource
            SetContentView(Resource.Layout.Login);

            // Initialize variables
            userid = FindViewById<EditText>(Resource.Id.userid);
            password = FindViewById<EditText>(Resource.Id.password);
            rememberme = FindViewById<CheckBox>(Resource.Id.rememberme);
            login = FindViewById<Button>(Resource.Id.login);
            register = FindViewById<Button>(Resource.Id.register);
            returns = FindViewById<Button>(Resource.Id.returns);

            // Initialize events
            login.Click += Login_Click;
            register.Click += Register_Click;
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

            // Initialize remember me
            ps = new PServer();
            userid.Text = ps.RememberMe(datapath, "", true);
            if (userid.Text != "")
            {
                rememberme.Checked = true;
                password.RequestFocus();
            }
        }

        public override void OnBackPressed()
        {
            return;
        }

        private async void Login_Click(object sender, EventArgs e)
        {
            if (userid.Text != "" && password.Text != "" && userid.Text.ToLower() != "demo")
            {
                progress.Show();
                message = new string[] { "user", "in", userid.Text, password.Text };
                await Task.Run(() => result = ms.ProcessMessage(message, "GET"));
                progress.Dismiss();
                if (result[1] == "ack")
                {
                    string up = userid.Text;
                    if (result[8] == "1")
                        up = up + "~" + password.Text;
                    ps.RememberMe(datapath, up, rememberme.Checked);
                    Intent intent = new Intent(this.ApplicationContext, typeof(ProcessActivity));
                    intent.PutExtra("baseurl", baseurl);
                    intent.PutExtra("userid", userid.Text);
                    intent.PutExtra("password", password.Text);
                    intent.PutExtra("username", result[2]);
                    intent.PutExtra("questionnaires", result[3]);
                    intent.PutExtra("gender", result[4]);
                    intent.PutExtra("birthdate", result[5]);
                    intent.PutExtra("language", result[6]);
                    intent.PutExtra("email", result[7]);
                    intent.PutExtra("level", Convert.ToInt32(result[8]));
                    intent.PutExtra("datapath", datapath);
                    StartActivity(intent);
                    Finish();
                }
                else
                    AlertMessage(result[2]);
            }
        }

        private void Register_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this.ApplicationContext, typeof(RegisterActivity));
            intent.PutExtra("baseurl", baseurl);
            StartActivity(intent);
        }

        private void Returns_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this.ApplicationContext, typeof(MainActivity));
            StartActivity(intent);
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