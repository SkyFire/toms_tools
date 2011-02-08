using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CharacterConverter {
	public class ConverterPresenter {
		private Converter _converter;
		private IConverterView _view;
		public ConverterPresenter(IConverterView view) {
			_view = view;
			_view.SetPresenter(this);
		}

		public void ShowLogo() {
			_view.AddLogLine("Characters converter for MaNGoS");
			_view.AddLogLine("Client version 2.4.2->2.4.3");
			_view.AddLogLine("© 2008 TOM_RUS, Alex Dereka, Hazzik\r\n");
			_view.Host = "localhost";
			_view.Port = "3306";
		}

		public void Convert() {
			try {
				var cs = string.Format("Server={0};Port={1};Database={2};Uid={3};Pwd={4}",
					_view.Host, _view.Port, _view.Base, _view.User, _view.Pass);

				_view.AddLogLine("Converting...");
				_converter = new Converter(cs);
				_converter.Convert();
				_view.AddLogLine("Done...");
			}
			catch(Exception e) {
				_view.AddLogLine(e.Message);
			}
		}
	}
}
