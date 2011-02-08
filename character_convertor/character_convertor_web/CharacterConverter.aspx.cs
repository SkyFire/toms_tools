using System;
using System.Web.UI;

namespace CharacterConverter.Web {
	public partial class CharacterConverterView : Page, IConverterView {
		private ConverterPresenter _presenter;
		public CharacterConverterView() {
			_presenter = new ConverterPresenter(this);
		}

		protected void FormInit(object sender, EventArgs e) {
			_presenter.ShowLogo();
		}

		public string Host {
			get { return _tbHost.Text; }
			set { _tbHost.Text = value; }
		}

		public string Port {
			get { return _tbPort.Text; }
			set { _tbPort.Text = value; }
		}

		public string Base {
			get { return _tbBase.Text; }
			set { _tbBase.Text = value; }
		}

		public new string User {
			get { return _tbUser.Text; }
			set { _tbUser.Text = value; }
		}

		public string Pass {
			get { return _tbPass.Text; }
			set { _tbPass.Text = value; }
		}

		public void SetPresenter(ConverterPresenter presenter) {
			_presenter = presenter;
		}

		public void AddLogLine(string value) {
			_lbInfo.Text += value + "<br/>";
		}

		private void Convert() {
			_presenter.Convert();
		}

		protected void BtnConvert_Click(object sender, EventArgs e) {
			_btnConvert.Enabled = false;
			Convert();
		}
	}
}
