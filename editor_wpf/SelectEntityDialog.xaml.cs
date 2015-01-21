using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


using Newtonsoft.Json.Linq;

using editor_wpf.ViewModel;


namespace editor_wpf
{
	/// <summary>
	/// Interaction logic for SelectEntityDialog.xaml
	/// </summary>
	public partial class SelectEntityDialog : Window
	{
		IDictionary<string, Panel> panels = new Dictionary<string, Panel>();

		public SelectEntityDialog() {
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			this.Hide();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
			this.Hide();
		}

	}
}
