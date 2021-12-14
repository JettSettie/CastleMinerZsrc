using System.ComponentModel;
using System.Windows.Forms;

namespace DNA.CastleMinerZ
{
	public class OptionsForm : Form
	{
		private IContainer components;

		private Button okButton;

		private Button cancelButton;

		private CheckBox fullScreenCheckBox;

		private CheckBox facebookCheckBox;

		public bool FullScreenMode
		{
			get
			{
				return fullScreenCheckBox.Checked;
			}
			set
			{
				fullScreenCheckBox.Checked = value;
			}
		}

		public bool AskForFacebook
		{
			get
			{
				return facebookCheckBox.Checked;
			}
			set
			{
				facebookCheckBox.Checked = value;
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DNA.CastleMinerZ.OptionsForm));
			okButton = new System.Windows.Forms.Button();
			cancelButton = new System.Windows.Forms.Button();
			fullScreenCheckBox = new System.Windows.Forms.CheckBox();
			facebookCheckBox = new System.Windows.Forms.CheckBox();
			SuspendLayout();
			resources.ApplyResources(okButton, "okButton");
			okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			okButton.Name = "okButton";
			okButton.UseVisualStyleBackColor = true;
			resources.ApplyResources(cancelButton, "cancelButton");
			cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			cancelButton.Name = "cancelButton";
			cancelButton.UseVisualStyleBackColor = true;
			resources.ApplyResources(fullScreenCheckBox, "fullScreenCheckBox");
			fullScreenCheckBox.Name = "fullScreenCheckBox";
			fullScreenCheckBox.UseVisualStyleBackColor = true;
			resources.ApplyResources(facebookCheckBox, "facebookCheckBox");
			facebookCheckBox.Checked = true;
			facebookCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			facebookCheckBox.Name = "facebookCheckBox";
			facebookCheckBox.UseVisualStyleBackColor = true;
			base.AcceptButton = okButton;
			resources.ApplyResources(this, "$this");
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = cancelButton;
			base.Controls.Add(facebookCheckBox);
			base.Controls.Add(fullScreenCheckBox);
			base.Controls.Add(cancelButton);
			base.Controls.Add(okButton);
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "OptionsForm";
			base.ShowInTaskbar = false;
			ResumeLayout(false);
			PerformLayout();
		}

		public OptionsForm()
		{
			InitializeComponent();
		}
	}
}
