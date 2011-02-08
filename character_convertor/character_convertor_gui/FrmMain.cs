using System;
using System.Threading;
using System.Windows.Forms;

namespace CharacterConverter.Gui {
	public partial class FrmMain : Form, IConverterView {
		delegate void SetInfoDelegate(string value);
		private ConverterPresenter _presenter;
		public FrmMain() {
			InitializeComponent();
			_presenter = new ConverterPresenter(this);
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

		public string User {
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
			if(_lbInfo.InvokeRequired) {
				_lbInfo.Invoke(new SetInfoDelegate(AddLogLine), value);
			}
			else {
				_lbInfo.Text += value + "\r\n";
			}
		}

		private void Convert() {
			new Thread(_presenter.Convert).Start();
		}

		private void BtnConvert_Click(object sender, EventArgs e) {
			_btnConvert.Enabled = false;
			Convert();
		}
	}
}
