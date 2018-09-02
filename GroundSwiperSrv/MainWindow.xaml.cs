using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GroundSwiperSrv
{
	using static Keys;
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			DataContext = this;
		}
		
		public bool Enabled
		{
			get { return (bool)GetValue(ServerButtonIsTogglableProperty); }
			set { SetValue(ServerButtonIsTogglableProperty, value); }
		}
		public static readonly DependencyProperty ServerButtonIsTogglableProperty =
			DependencyProperty.Register(nameof(Enabled), typeof(bool), typeof(MainWindow), new PropertyMetadata(true));

		public string Port
		{
			get { return (string)GetValue(PortProperty); }
			set { SetValue(PortProperty, value); }
		}	
		public static readonly DependencyProperty PortProperty =
			DependencyProperty.Register(nameof(Port), typeof(string), typeof(MainWindow), new PropertyMetadata("8123"));

		public string ServerButtonText
		{
			get { return (string)GetValue(ServerButtonTextProperty); }
			set { SetValue(ServerButtonTextProperty, value); }
		}		
		public static readonly DependencyProperty ServerButtonTextProperty =
			DependencyProperty.Register("ServerButtonText", typeof(string), typeof(MainWindow), new PropertyMetadata("起動"));

		public string Log
		{
			get { return (string)GetValue(LogProperty); }
			set { SetValue(LogProperty, value); }
		}
		public static readonly DependencyProperty LogProperty =
			DependencyProperty.Register("Log", typeof(string), typeof(MainWindow), new PropertyMetadata(""));

		bool isConnecting = false;
		
		Keys[] keys = { A, Z, S, X, D, C, F, V, G, B, H, N, J, M, K, Oemcomma };

		void LogWrite(string text)
		{
			Log += text += Environment.NewLine;
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			Enabled = false;

			isConnecting = !isConnecting;

			if (isConnecting)
			{
#pragma warning disable CS4014 // この呼び出しを待たないため、現在のメソッドの実行は、呼び出しが完了する前に続行します
				Task.Run(async () =>
				{
					var i = 0;
					while (isConnecting)
					{
						foreach (var k in keys) KeySender.KeyDown(k);
						await Task.Delay(50);
						foreach (var k in keys) KeySender.KeyUp(k);
						await Task.Delay(10);
						i++;
						if (keys.Length <= i)
							i = 0;
					};
				});
#pragma warning restore CS4014 // この呼び出しを待たないため、現在のメソッドの実行は、呼び出しが完了する前に続行します
			}

			LogWrite("サーバーを" +  (isConnecting ? "起動" : "終了") + "しました");

			ServerButtonText = isConnecting ? "終了" : "起動";


			Enabled = true;

		}

	}

	public static class KeySender {
		static byte up = 0;
		static byte down = 2;

		public static void KeyDown(Keys key)
		{
			keybd_event((byte)key, 0, up, (UIntPtr)0);
		}

		public static void KeyUp(Keys key)
		{
			keybd_event((byte)key, 0, down, (UIntPtr)0);
		}

		[DllImport("user32.dll")]
		private static extern uint keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
	}


}
