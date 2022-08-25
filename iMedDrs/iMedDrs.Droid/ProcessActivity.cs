using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Newtonsoft.Json;

namespace iMedDrs.Droid
{
    [Activity(Name = "com.imeddrs.imeddrs.ProcessActivity", Label = "iMedDrs - Questionnaires/Reports", Icon = "@drawable/Icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@android:style/Theme.Holo.Light")]
    public class ProcessActivity : Activity
    {
        string baseurl;
        string userid;
        string username;
        string gender;
        string birthdate;
        string language;
        string languages;
        string email;
        string datapath;
        string role;
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
            username = Intent.GetStringExtra("username");
            questionnaires = Intent.GetStringExtra("questionnaires").Split(',');
            gender = Intent.GetStringExtra("gender");
            birthdate = Intent.GetStringExtra("birthdate");
            language = Intent.GetStringExtra("language");
            languages = Intent.GetStringExtra("languages");
            email = Intent.GetStringExtra("email");
            role = Intent.GetStringExtra("role");
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

            if (role == "demo")
            {
                logout.Text = "Return to Start";
                update.Visibility = Android.Views.ViewStates.Gone;
            }
            if (role != "admin")
                maintain.Visibility = Android.Views.ViewStates.Gone;
        }

        protected override void OnResume()
        {
            base.OnResume();
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
                this.ActionBar.Subtitle = username;
            }
        }

        private async void Start_Click(object sender, EventArgs e)
        {
            progress.Show();
            message = new string[] { "questionnaires", questionnaire.SelectedItem.ToString(), userid, language };
            await Task.Run(() => result = ms.ProcessMessage(message, "GET", ""));
            progress.Dismiss();
            if (result[0] == "ack")
            {
                QuestionnaireModel model = JsonConvert.DeserializeObject<QuestionnaireModel>(result[1]);
                Intent intent = new Intent(this.ApplicationContext, typeof(QuestionsActivity));
                intent.PutExtra("baseurl", baseurl);
                intent.PutExtra("userid", userid);
                intent.PutExtra("username", username);
                intent.PutExtra("questionnaireid", model.Id.ToString());
                intent.PutExtra("questionnaire", questionnaire.SelectedItem.ToString());
                intent.PutExtra("number", model.Sequence.ToString());
                intent.PutExtra("name", model.QuestionName);
                intent.PutExtra("text", model.Question);
                intent.PutExtra("response", model.ActResponses.ToArray());
                intent.PutExtra("last", model.EndSequence.ToString());
                intent.PutExtra("responses", model.Responses);
                intent.PutExtra("required", model.Required);
                intent.PutExtra("type", model.Type);
                intent.PutExtra("answer", "");
                intent.PutExtra("eresponse", model.EngResponses.ToArray());
                intent.PutExtra("extension", ".mp3");
                intent.PutExtra("instruction", model.Instructions.Replace("<br/>", "\r\n"));
                intent.PutExtra("role", role);
                intent.PutExtra("datapath", datapath);
                intent.PutExtra("language", language);
                intent.PutExtra("email", email);
                intent.PutExtra("handsfree", handsfree.Checked.ToString());
                StartActivity(intent);
            }
            else
                AlertMessage(result[1]);
        }

        private async void View_Click(object sender, EventArgs e)
        {
            string data = "";
            message = null;
            string[] responses = new string[] { "" };
            if (role == "demo")
            {
                data = ps.ReadFromFile(Path.Combine(datapath, questionnaire.SelectedItem.ToString().Replace(" ", "_") + ".txt"));
                if (data != "")
                {
                    responses = data.Split(',');
                    message = new string[] { "reports", userid, questionnaire.SelectedItem.ToString(), "English" };
                }
                else
                    AlertMessage("No responses saved");
            }
            else
                message = new string[] { "reports", userid, questionnaire.SelectedItem.ToString(), "English" };
            if (message != null)
            {
                progress.Show();
                if (role == "demo")
                {
                    await Task.Run(() => result = ms.ProcessMessage(message, "POST", JsonConvert.SerializeObject(responses)));
                }
                else
                {
                    await Task.Run(() => result = ms.ProcessMessage(message, "GET", ""));
                }
                progress.Dismiss();
                if (result[0] == "ack")
                {
                    ReportModel model = JsonConvert.DeserializeObject<ReportModel>(result[1]);
                    if (!string.IsNullOrWhiteSpace(model.Reports[0].Text))
                    {
                        Intent intent = new Intent(this.ApplicationContext, typeof(ReportActivity));
                        intent.PutExtra("baseurl", baseurl);
                        intent.PutExtra("userid", userid);
                        intent.PutExtra("username", username);
                        intent.PutExtra("questionnaire", questionnaire.SelectedItem.ToString());
                        intent.PutExtra("last", model.MaxId);
                        intent.PutExtra("number", 0);
                        intent.PutExtra("reportjson", result[1]);
                        intent.PutExtra("data", data);
                        intent.PutExtra("email", email);
                        intent.PutExtra("role", role);
                        intent.PutExtra("language", language);
                        StartActivity(intent);
                    }
                    else
                        AlertMessage("No responses saved");
                }
                else
                    AlertMessage(result[1]);
            }
        }

        private void Update_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this.ApplicationContext, typeof(UserActivity));
            intent.PutExtra("baseurl", baseurl);
            intent.PutExtra("userid", userid);
            intent.PutExtra("username", username);
            intent.PutExtra("gender", gender);
            intent.PutExtra("birthdate", birthdate);
            intent.PutExtra("language", language);
            intent.PutExtra("email", email);
            intent.PutExtra("role", role);
            intent.PutExtra("languages", languages);
            StartActivityForResult(intent, 1);
        }

        private void Maintain_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this.ApplicationContext, typeof(MaintainActivity));
            intent.PutExtra("baseurl", baseurl);
            intent.PutExtra("userid", userid);
            intent.PutExtra("questionnaires", Intent.GetStringExtra("questionnaires"));
            intent.PutExtra("languages", "English|French|Spanish");
            intent.PutExtra("role", role);
            intent.PutExtra("datapath", datapath);
            StartActivity(intent);
        }

        private void Logout_Click(object sender, EventArgs e)
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