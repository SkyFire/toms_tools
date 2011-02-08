using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CharacterConverter {
	public interface IConverterView {
		string Host { get; set; }
		string Port { get; set; }
		string Base { get; set; }
		string User { get; set; }
		string Pass { get; set; }

		void SetPresenter(ConverterPresenter presenter);
		void AddLogLine(string value);
	}
}
