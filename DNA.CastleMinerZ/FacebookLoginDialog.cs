using Facebook;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Dynamic;
using System.Windows.Forms;

namespace DNA.CastleMinerZ
{
	public class FacebookLoginDialog : Form
	{
		private readonly Uri _loginUrl;

		protected readonly FacebookClient _fb;

		private IContainer components;

		private WebBrowser webBrowser;

		public FacebookOAuthResult FacebookOAuthResult
		{
			get;
			private set;
		}

		public FacebookLoginDialog(string appId, string extendedPermissions)
			: this((FacebookClient)(object)new FacebookClient(), appId, extendedPermissions)
		{
		}//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown


		public FacebookLoginDialog(FacebookClient fb, string appId, string extendedPermissions)
		{
			if (fb == null)
			{
				throw new ArgumentNullException("fb");
			}
			if (string.IsNullOrWhiteSpace(appId))
			{
				throw new ArgumentNullException("appId");
			}
			_fb = fb;
			_loginUrl = GenerateLoginUrl(appId, extendedPermissions);
			InitializeComponent();
		}

		private Uri GenerateLoginUrl(string appId, string extendedPermissions)
		{
			dynamic val = new ExpandoObject();
			val.client_id = appId;
			val.redirect_uri = "https://www.facebook.com/connect/login_success.html";
			val.response_type = "token";
			val.display = "popup";
			if (!string.IsNullOrWhiteSpace(extendedPermissions))
			{
				val.scope = extendedPermissions;
			}
			return _fb.GetLoginUrl(val);
		}

		private void FacebookLoginDialog_Load(object sender, EventArgs e)
		{
			webBrowser.Navigate(_loginUrl.AbsoluteUri);
		}

		private void webBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
		{
			FacebookOAuthResult facebookOAuthResult = default(FacebookOAuthResult);
			if (_fb.TryParseOAuthCallbackUrl(e.Url, ref facebookOAuthResult))
			{
				FacebookOAuthResult = facebookOAuthResult;
				base.DialogResult = (FacebookOAuthResult.get_IsSuccess() ? DialogResult.OK : DialogResult.No);
			}
			else
			{
				FacebookOAuthResult = null;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DNA.CastleMinerZ.FacebookLoginDialog));
			webBrowser = new System.Windows.Forms.WebBrowser();
			SuspendLayout();
			resources.ApplyResources(webBrowser, "webBrowser");
			webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
			webBrowser.Name = "webBrowser";
			webBrowser.ScrollBarsEnabled = false;
			webBrowser.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(webBrowser_Navigated);
			resources.ApplyResources(this, "$this");
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.Controls.Add(webBrowser);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FacebookLoginDialog";
			base.ShowInTaskbar = false;
			base.Load += new System.EventHandler(FacebookLoginDialog_Load);
			ResumeLayout(false);
		}
	}
}
