using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;

namespace iMedDrs.Droid
{
    [Activity(Name = "com.imeddrs.imeddrs.LoginActivity", Label = "iMedDrs - Login", Icon = "@drawable/Icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@android:style/Theme.Holo.Light")]
    public class LoginActivity : Activity
    {
        string baseurl;
        string datapath;
        string languages;
        string[] message;
        string[] result;
        EditText userid;
        EditText password;
        CheckBox rememberme;
        TextView reset;
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
            languages = Intent.GetStringExtra("languages");

            // Set our view from the "login" layout resource
            SetContentView(Resource.Layout.Login);

            // Initialize variables
            userid = FindViewById<EditText>(Resource.Id.userid);
            password = FindViewById<EditText>(Resource.Id.password);
            rememberme = FindViewById<CheckBox>(Resource.Id.rememberme);
            login = FindViewById<Button>(Resource.Id.login);
            register = FindViewById<Button>(Resource.Id.register);
            returns = FindViewById<Button>(Resource.Id.returns);
            reset = FindViewById<TextView>(Resource.Id.reset);

            // Initialize events
            login.Click += Login_Click;
            register.Click += Register_Click;
            returns.Click += Returns_Click;
            reset.Click += Reset_Click;

            // Alert dialog for messages
            AlertDialog.Builder alertbuilder1 = new AlertDialog.Builder(this);
            alertbuilder1.SetPositiveButton("Ok", (EventHandler<DialogClickEventArgs>)null);
            alert = alertbuilder1.Create();
            alertbuilder1.Dispose();

            // Progress dialog for messaging
            AlertDialog.Builder alertbuilder2 = new AlertDialog.Builder(this);
            alertbuilder2.SetView(LayoutInflater.Inflate(Resource.Layout.Progress, null));
            progress = alertbuilder2.Create();
            progress.SetCancelable(false);
            alertbuilder2.Dispose();

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
                message = new string[] { "users", userid.Text, password.Text };
                await Task.Run(() => result = ms.ProcessMessage(message, "GET", ""));
                progress.Dismiss();
                if (result[0] == "ack")
                {
                    UserModel user = JsonConvert.DeserializeObject<UserModel>(result[1]);
                    string questionnaires = "";
                    foreach (var item in user.QuestionnaireList)
                    {
                        questionnaires += questionnaires != "" ? "," + item.Name : item.Name;
                    }
                    ps.RememberMe(datapath, user.Email, rememberme.Checked);
                    Intent intent = new Intent(this.ApplicationContext, typeof(ProcessActivity));
                    intent.PutExtra("baseurl", baseurl);
                    intent.PutExtra("userid", user.Id.Value.ToString());
                    intent.PutExtra("username", user.Name);
                    intent.PutExtra("questionnaires", questionnaires);
                    intent.PutExtra("languages", user.LanguageList);
                    intent.PutExtra("gender", user.Gender);
                    intent.PutExtra("birthdate", user.Birthdate.ToShortDateString());
                    intent.PutExtra("language", user.Language);
                    intent.PutExtra("languages", languages);
                    intent.PutExtra("email", user.Email);
                    intent.PutExtra("role", user.Role);
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
            intent.PutExtra("languages", languages);
            StartActivity(intent);
        }

        private void Returns_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this.ApplicationContext, typeof(MainActivity));
            StartActivity(intent);
            Finish();
        }

        private async void Reset_Click(object sender, EventArgs e)
        {
            if (userid.Text != "")
            {
                progress.Show();
                message = new string[] { "users", "reset", userid.Text };
                await Task.Run(() => result = ms.ProcessMessage(message, "GET", ""));
                progress.Dismiss();
                AlertMessage(result[1]);
            }
            else
                AlertMessage("Enter your email address first!");
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