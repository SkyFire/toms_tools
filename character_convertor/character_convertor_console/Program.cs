using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CharacterConverter.Con {
	class Program : IConverterView {
		private ConverterPresenter _presenter;

		public Program() {
			_presenter = new ConverterPresenter(this);
			_presenter.ShowLogo();
		}

		private string _host;
		public string Host {
			get {
				Console.Write("Enter DB host [{0}]: ", _host);
				var tmp = Console.ReadLine().Trim();
				if(!string.IsNullOrEmpty(tmp)) {
					_host = tmp;
				}
				return _host;
			}
			set { _host = value; }
		}

		private string _port;
		public string Port {
			get {
				Console.Write("Enter DB port [{0}]: ", _port);
				var tmp = Console.ReadLine().Trim();
				if(!string.IsNullOrEmpty(tmp)) {
					_port = tmp;
				}
				return _port;
			}
			set { _port = value; }
		}

		private string _base;
		public string Base {
			get {
				Console.Write("Enter DB name [{0}]: ", _base);
				var tmp = Console.ReadLine().Trim();
				if(!string.IsNullOrEmpty(tmp)) {
					_base = tmp;
				}
				return _base;
			}
			set { _base = value; }
		}

		private string _user;
		public string User {
			get {
				Console.Write("Enter DB user name [{0}]: ", _user);
				var tmp = Console.ReadLine().Trim();
				if(!string.IsNullOrEmpty(tmp)) {
					_user = tmp;
				}
				return _user;
			}
			set { _user = value; }
		}

		private string _pass;
		public string Pass {
			get {
				Console.Write("Enter DB password: ");
				_pass = Console.ReadLine();
				return _pass;
			}
			set { _pass = value; }
		}

		public void SetPresenter(ConverterPresenter presenter) {
			_presenter = presenter;
		}

		public void AddLogLine(string value) {
			Console.WriteLine(value);
		}

		private void Convert() {
			_presenter.Convert();
		}

		static void Main(string[] args) {
			AppDomain currentDomain = AppDomain.CurrentDomain;
			currentDomain.UnhandledException += new UnhandledExceptionEventHandler(TopLevelErrorHandler);

			Console.Title = "Character Converter console";

			Program program = new Program();
			program.Convert();

			Console.ReadKey();
		}

		private static void TopLevelErrorHandler(object sender, UnhandledExceptionEventArgs args) {
			Exception e = (Exception)args.ExceptionObject;
			Console.WriteLine("Error Occured : " + e.ToString());
		}
	}
}
