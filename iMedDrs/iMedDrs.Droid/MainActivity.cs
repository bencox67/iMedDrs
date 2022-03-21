using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Newtonsoft.Json;

namespace iMedDrs.Droid
{
    [Activity(Name="com.imeddrs.imeddrs.MainActivity", Label = "iMedDrs", MainLauncher = true, Icon = "@drawable/Icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@android:style/Theme.Holo.Light")]
    public class MainActivity : Activity
    {
        string baseurl;
        string userid;
        string password;
        string datapath;
        string languages;
        string[] message;
        string[] result;
        TextView inst2;
        TextView inst3;
        Button start;
        Button login;
        AlertDialog alert;
        AlertDialog progress;
        MServer ms;
        PServer ps;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set title
            string version = this.ApplicationContext.PackageManager.GetPackageInfo(this.ApplicationContext.PackageName, 0).VersionName;
            this.ActionBar.Subtitle = "version " + version;

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Initialize variables
            baseurl = "https://imeddrs.com/api";
            datapath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            inst2 = FindViewById<TextView>(Resource.Id.textView2);
            inst3 = FindViewById<TextView>(Resource.Id.textView3);
            start = FindViewById<Button>(Resource.Id.start);
            login = FindViewById<Button>(Resource.Id.login);

            // Initialize events
            start.Click += Start_Click;
            login.Click += Login_Click;

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

            // Check remembered user
            ps = new PServer();
            userid = ps.RememberMe(datapath, "", true);
            if (userid.Contains("~"))
            {
                string[] up = userid.Split('~');
                userid = up[0];
                password = up[1];
                inst2.Visibility = Android.Views.ViewStates.Gone;
                inst3.Visibility = Android.Views.ViewStates.Gone;
                login.Visibility = Android.Views.ViewStates.Gone;
            }
            else
            {
                userid = "demo";
                password = "1234";
            }

            GetLanguages();
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (userid != "demo")
                Start();
        }

        private void Start_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void Login_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this.ApplicationContext, typeof(LoginActivity));
            intent.PutExtra("baseurl", baseurl);
            intent.PutExtra("datapath", datapath);
            intent.PutExtra("languages", languages);
            StartActivity(intent);
            Finish();
        }

        private async void Start()
        {
            progress.Show();
            message = new string[] { "users", "demo", password };
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
                Intent intent = new Intent(this.ApplicationContext, typeof(ProcessActivity));
                intent.PutExtra("baseurl", baseurl);
                intent.PutExtra("userid", user.Id.Value.ToString());
                intent.PutExtra("username", user.Name);
                intent.PutExtra("questionnaires", questionnaires);
                intent.PutExtra("languages", user.LanguageList);
                intent.PutExtra("gender", user.Gender);
                intent.PutExtra("birthdate", user.Birthdate.ToShortDateString());
                intent.PutExtra("language", user.Language);
                intent.PutExtra("email", user.Email);
                intent.PutExtra("role", user.Role);
                intent.PutExtra("datapath", datapath);
                StartActivity(intent);
                Finish();
            }
            else
                AlertMessage(result[1]);
        }

        private async void GetLanguages()
        {
            progress.Show();
            message = new string[] { "users", "demo", "1234" };
            await Task.Run(() => result = ms.ProcessMessage(message, "GET", ""));
            progress.Dismiss();
            if (result[0] == "ack")
            {
                UserModel user = JsonConvert.DeserializeObject<UserModel>(result[1]);
                languages = String.Join('|', user.LanguageList);
            }
            else
            {
                languages = "English|French|Spanish";
            }
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