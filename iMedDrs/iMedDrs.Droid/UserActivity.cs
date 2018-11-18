using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

namespace iMedDrs.Droid
{
    [Activity(Name = "com.imeddrs.imeddrs.UserActivity", Label = "iMedDrs - Personal Data", Icon = "@drawable/Icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@android:style/Theme.Holo.Light")]
    public class UserActivity : Activity
    {
        string baseurl;
        string password;
        string gender;
        string languageid;
        string emailaddr;
        string newpassword;
        string[] languages;
        string[] message;
        string[] result;
        bool updated;
        EditText userid;
        EditText name;
        RadioButton male;
        RadioButton female;
        EditText birthdate;
        Spinner language;
        EditText email;
        EditText password1;
        EditText password2;
        ImageButton calendar;
        Button update;
        Button returns;
        AlertDialog alert;
        AlertDialog progress;
        MServer ms;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "login" layout resource
            SetContentView(Resource.Layout.User);

            // Initialize variables
            userid = FindViewById<EditText>(Resource.Id.userid);
            name = FindViewById<EditText>(Resource.Id.name);
            male = FindViewById<RadioButton>(Resource.Id.male);
            female = FindViewById<RadioButton>(Resource.Id.female);
            birthdate = FindViewById<EditText>(Resource.Id.birthdate);
            language = FindViewById<Spinner>(Resource.Id.language);
            languageid = Intent.GetStringExtra("language");
            languages = Intent.GetStringExtra("languages").Split(',');
            language.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, languages);
            for (int i = 0; i < languages.Length; i++)
            {
                if (languages[i] == languageid)
                    language.SetSelection(i);
            }
            email = FindViewById<EditText>(Resource.Id.email);
            password1 = FindViewById<EditText>(Resource.Id.password1);
            password2 = FindViewById<EditText>(Resource.Id.password2);
            calendar = FindViewById<ImageButton>(Resource.Id.calendar);
            update = FindViewById<Button>(Resource.Id.update);
            returns = FindViewById<Button>(Resource.Id.returns);

            // Get intent variables
            baseurl = Intent.GetStringExtra("baseurl");
            userid.Text = Intent.GetStringExtra("userid");
            password = Intent.GetStringExtra("password");
            name.Text = Intent.GetStringExtra("username");
            gender = Intent.GetStringExtra("gender");
            birthdate.Text = Intent.GetStringExtra("birthdate");
            email.Text = Intent.GetStringExtra("email");
            if (gender == "Male")
                male.Checked = true;
            else
                female.Checked = true;

            // Initialize events
            calendar.Click += Calendar_Click;
            update.Click += Update_Click;
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
            updated = false;
        }

        private void Returns_Click(object sender, EventArgs e)
        {
            if (updated)
            {
                Intent intent = new Intent(this.ApplicationContext, typeof(ProcessActivity));
                intent.PutExtra("username", name.Text);
                intent.PutExtra("gender", gender);
                intent.PutExtra("birthdate", birthdate.Text);
                intent.PutExtra("language", language.SelectedItem.ToString());
                intent.PutExtra("email", email.Text);
                if (newpassword != "~")
                    intent.PutExtra("password", newpassword);
                else
                    intent.PutExtra("password", password);
                SetResult(Result.Ok, intent);
            }
            Finish();
        }

        public override void OnBackPressed()
        {
            return;
        }

        private void Calendar_Click(object sender, EventArgs e)
        {
            DatePickerFragment frag = DatePickerFragment.NewInstance(birthdate.Text, delegate (DateTime time)
            {
                birthdate.Text = time.ToString("MM/dd/yyyy");
            });
            frag.Show(FragmentManager, DatePickerFragment.TAG);
        }

        private async void Update_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                progress.Show();
                message = new string[] { "user", "change", userid.Text, name.Text, gender, birthdate.Text.Replace("/","|"), language.SelectedItem.ToString(), emailaddr, password, newpassword };
                await Task.Run(() => result = ms.ProcessMessage(message, "GET"));
                progress.Dismiss();
                if (result[1] == "ack")
                {
                    password1.Text = "";
                    password2.Text = "";
                    updated = true;
                    AlertMessage("Update Complete");
                }
                else
                    AlertMessage(result[2]);
            }
        }

        private bool ValidateData()
        {
            bool result = true;
            string error = "";
            if (result && name.Text.Split(' ').Length < 2)
            {
                error = "Check Name";
                result = false;
            }
            gender = "";
            if (male.Checked)
                gender = "Male";
            if (female.Checked)
                gender = "Female";
            if (result && gender == "")
            {
                error = "Check Gender";
                result = false;
            }
            try { DateTime dt = Convert.ToDateTime(birthdate.Text); }
            catch
            {
                if (result)
                {
                    error = "Check Birth Date";
                    result = false;
                }
            }
            newpassword = "~";
            if (result && password1.Text != "" && (password1.Text != password2.Text || password1.Text.Length < 4))
            {
                error = "Check Password";
                result = false;
            }
            if (email.Text == "")
                emailaddr = "~";
            else
                emailaddr = email.Text;
            if (password1.Text == "")
                newpassword = "~";
            else
                newpassword = password1.Text;
            AlertMessage(error);
            return result;
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