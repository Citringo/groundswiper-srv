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
	using SuperSocket.SocketBase;
	using SuperSocket.SocketBase.Config;
	using SuperSocket.WebSocket;
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

		WebSocketSession session;

		WebSocketServer server;

		void LogWrite(string text)
		{
			Log += text += Environment.NewLine;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Enabled = false;
			var state = false;
			if (!isConnecting)
				state = StartServer();
			else
				state = StopServer();
			if (!state)
				LogWrite("失敗しました");
			else
				LogWrite("サーバーを" +  (isConnecting ? "起動" : "終了") + "しました");

			ServerButtonText = isConnecting ? "終了" : "起動";

			Enabled = true;
		}



		private bool StartServer()
		{
			if (!int.TryParse(Port, out var port))
				return false;

			var server = new WebSocketServer();
			var config = new RootConfig();
			var srvConfig = new ServerConfig
			{
				Port = port,
				Ip = "Any",
				MaxConnectionNumber = 1,
				Mode = SocketMode.Tcp,
				Name = "GroundSwiper Server"
			};
			server.NewSessionConnected += s => session = s;
			server.NewDataReceived += (s, d) =>
			{
				// d[0]: mode (0: down 1: up)
				// d[1]: keyCode
				if (d.Length != 2)
				{
					LogWrite($"Invalid Data Length {d.Length}!");
					return;
				}

				switch (d[0])
				{
					case 0:
						KeySender.KeyDown(d[1]);
						break;
					case 1:
						KeySender.KeyUp(d[1]);
						break;
					default:
						LogWrite($"Invalid Data Mode {d[0]}");
						break;
				}
			};
			server.Setup(config, srvConfig);

			server.Start();

			this.server = server;
			isConnecting = true;
			return true;
		}

		public bool StopServer()
		{
			if (server == null)
			{
				LogWrite("サーバーは既に起動していません。");
				return false;
			}
			server.Stop();
			isConnecting = false;
			return true;
		}

	}

	public static class KeySender {
		static byte up = 0;
		static byte down = 2;

		public static void KeyDown(byte key)
		{
			keybd_event(key, 0, up, (UIntPtr)0);
		}

		public static void KeyUp(byte key)
		{
			keybd_event(key, 0, down, (UIntPtr)0);
		}

		[DllImport("user32.dll")]
		private static extern uint keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
	}


}
