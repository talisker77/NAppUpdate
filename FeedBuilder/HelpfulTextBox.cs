using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FeedBuilder
{
	public partial class HelpfulTextBox : TextBox

	{
		public HelpfulTextBox()
		{
			InitializeComponent();
		}

		public HelpfulTextBox(IContainer container)
		{
			container.Add(this);

			InitializeComponent();
		}

		#region "Win32 DLL Imports"

	  #endregion

		private string _helpfulText;

		[Browsable(true)]
		[Category("Appearance")]
		[DefaultValue("")]
		[Description("The grayed text that appears in the box when the box contains no text and is not focused.")]
		[RefreshProperties(RefreshProperties.None)]
		public string HelpfulText
		{
			get { return _helpfulText; }
			set
			{
				_helpfulText = value;
				SetCue();
			}
		}

		/// <summary>
		///   Actually, the system cue only works for editable (i.e. not read-only) text boxes.
		/// </summary>
		/// <remarks>
		/// </remarks>
		private void SetCue()
		{
			SendMessage(Handle, EM_SETCUEBANNER, 0, HelpfulText);
		}
	}
}