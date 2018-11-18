using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Widget;

namespace iMedDrs.Droid
{
    [Activity(Name = "com.imeddrs.imeddrs.ProcessActivity", Label = "iMedDrs - Questionnaires/Reports", Icon = "@drawable/Icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@android:style/Theme.Holo.Light")]
    public class ProcessActivity : Activity
    {
        string baseurl;
        string userid;
        string password;
        string username;
        string gender;
        string birthdate;
        string language;
        string email;
        string datapath;
        string languages;
        int level;
        string[] questionnaires;
        string[] message;
        string[] result;
        Spinner questionnaire;
        CheckBox handsfree;
        Button start;
        Button view;
        Button update;
        Button maintain;
        Button logout;
        AlertDialog alert;
        AlertDialog progress;
        MServer ms;
        PServer ps;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Get intent variables
            baseurl = Intent.GetStringExtra("baseurl");
            userid = Intent.GetStringExtra("userid");
            password = Intent.GetStringExtra("password");
            username = Intent.GetStringExtra("username");
            questionnaires = Intent.GetStringExtra("questionnaires").Split(',');
            gender = Intent.GetStringExtra("gender");
            birthdate = Intent.GetStringExtra("birthdate");
            language = Intent.GetStringExtra("language");
            email = Intent.GetStringExtra("email");
            level = Intent.GetIntExtra("level", 0);
            datapath = Intent.GetStringExtra("datapath");

            // Set title
            this.ActionBar.Subtitle = username;

            // Set our view from the "process" layout resource
            SetContentView(Resource.Layout.Process);

            // Initialize variables
            questionnaire = FindViewById<Spinner>(Resource.Id.questionnaire);
            questionnaire.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, questionnaires);
            handsfree = FindViewById<CheckBox>(Resource.Id.handsfree);
            start = FindViewById<Button>(Resource.Id.start);
            view = FindViewById<Button>(Resource.Id.view);
            update = FindViewById<Button>(Resource.Id.update);
            maintain = FindViewById<Button>(Resource.Id.maintain);
            logout = FindViewById<Button>(Resource.Id.logout);

            // Initialize events
            start.Click += Start_Click;
            view.Click += View_Click;
            update.Click += Update_Click;
            maintain.Click += Maintain_Click;
            logout.Click += Logout_Click;

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
            ps = new PServer();

            if (level == 0)
                logout.Text = "Return to Start";
            if (level < 2)
                update.Visibility = Android.Views.ViewStates.Gone;
            if (level < 8)
                maintain.Visibility = Android.Views.ViewStates.Gone;
        }

        protected override void OnResume()
        {
            base.OnResume();
            GetLanguages();
        }

        public override void OnBackPressed()
        {
            return;
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == 1 && resultCode == Result.Ok)
            {
                username = data.GetStringExtra("username");
                gender = data.GetStringExtra("gender");
                birthdate = data.GetStringExtra("birthdate");
                language = data.GetStringExtra("language");
                email = data.GetStringExtra("email");
                password = data.GetStringExtra("password");
                this.ActionBar.Subtitle = username;
            }
        }

        private async void Start_Click(object sender, EventArgs e)
        {
            progress.Show();
            message = new string[] { "questionnaire", "start", userid, questionnaire.SelectedItem.ToString() };
            await Task.Run(() => result = ms.ProcessMessage(message, "GET"));
            progress.Dismiss();
            if (result[1] == "ack")
            {
                Intent intent = new Intent(this.ApplicationContext, typeof(QuestionsActivity));
                intent.PutExtra("baseurl", baseurl);
                intent.PutExtra("userid", userid);
                intent.PutExtra("username", username);
                intent.PutExtra("questionnaire", questionnaire.SelectedItem.ToString());
                intent.PutExtra("number", result[2]);
                intent.PutExtra("name", result[3]);
                intent.PutExtra("text", result[4]);
                intent.PutExtra("response", result[5]);
                intent.PutExtra("last", result[6]);
                intent.PutExtra("responses", result[7]);
                intent.PutExtra("branches", result[8]);
                intent.PutExtra("required", result[9]);
                intent.PutExtra("type", result[10]);
                intent.PutExtra("answer", result[11]);
                intent.PutExtra("eresponse", result[12]);
                intent.PutExtra("extension", result[13]);
                intent.PutExtra("instruction", result[14]);
                intent.PutExtra("level", level);
                intent.PutExtra("datapath", datapath);
                intent.PutExtra("language", language);
                intent.PutExtra("email", email);
                intent.PutExtra("handsfree", handsfree.Checked.ToString());
                StartActivity(intent);
            }
            else
                AlertMessage(result[2]);
        }

        private async void View_Click(object sender, EventArgs e)
        {
            string data = "";
            message = null;
            if (level < 2)
            {
                data = ps.ReadFromFile(Path.Combine(datapath, questionnaire.SelectedItem.ToString().Replace(" ", "_") + ".txt"));
                if (data != "")
                    message = new string[] { "report", "load2", userid, questionnaire.SelectedItem.ToString(), "0", data };
                else
                    AlertMessage("No responses saved");
            }
            else
                message = new string[] { "report", "load", userid, questionnaire.SelectedItem.ToString(), "0" };
            if (message != null)
            {
                progress.Show();
                await Task.Run(() => result = ms.ProcessMessage(message, "GET"));
                progress.Dismiss();
                if (result[1] == "ack")
                {
                    Intent intent = new Intent(this.ApplicationContext, typeof(ReportActivity));
                    intent.PutExtra("baseurl", baseurl);
                    intent.PutExtra("userid", userid);
                    intent.PutExtra("username", username);
                    intent.PutExtra("questionnaire", questionnaire.SelectedItem.ToString());
                    intent.PutExtra("last", Convert.ToInt32(result[2]));
                    intent.PutExtra("number", Convert.ToInt32(result[3]));
                    intent.PutExtra("text", result[4]);
                    intent.PutExtra("data", data);
                    intent.PutExtra("email", email);
                    intent.PutExtra("level", level);
                    StartActivity(intent);
                }
                else
                    AlertMessage(result[2]);
            }
        }

        private void Update_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this.ApplicationContext, typeof(UserActivity));
            intent.PutExtra("baseurl", baseurl);
            intent.PutExtra("userid", userid);
            intent.PutExtra("password", password);
            intent.PutExtra("username", username);
            intent.PutExtra("gender", gender);
            intent.PutExtra("birthdate", birthdate);
            intent.PutExtra("language", language);
            intent.PutExtra("languages", languages);
            intent.PutExtra("email", email);
            StartActivityForResult(intent, 1);
        }

        private void Maintain_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this.ApplicationContext, typeof(MaintainActivity));
            intent.PutExtra("baseurl", baseurl);
            intent.PutExtra("userid", userid);
            intent.PutExtra("questionnaires", Intent.GetStringExtra("questionnaires"));
            intent.PutExtra("level", level);
            intent.PutExtra("languages", languages);
            intent.PutExtra("datapath", datapath);
            StartActivity(intent);
        }

        private void Logout_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this.ApplicationContext, typeof(MainActivity));
            StartActivity(intent);
            Finish();
        }

        private async void GetLanguages()
        {
            progress.Show();
            message = new string[] { "user", "languages" };
            await Task.Run(() => result = ms.ProcessMessage(message, "GET"));
            progress.Dismiss();
            if (result[1] == "ack")
            {
                languages = result[2];
                for (int i = 3; i < result.Length; i++)
                    languages = languages + "," + result[i];
            }
            else
                languages = "English";
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