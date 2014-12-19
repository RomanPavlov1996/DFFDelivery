using System;
using System.Collections.Generic;
using System.Net;
using Java.IO;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.Security.Cryptography;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Webkit;

namespace DFFDelivery
{
	[Activity( Label = "DFFDelivery", Theme = "@android:style/Theme.Holo.Light" )]
	public class MainActivity : Activity
	{
		LinearLayout lltOrders;
		List<Order> ordersList;
		string UserId;

		protected override void OnCreate ( Bundle bundle )
		{
			base.OnCreate( bundle );

			SetContentView( Resource.Layout.Main );
			lltOrders = ( LinearLayout ) FindViewById( Resource.Id.lltOrders );
			ordersList = new List<Order>( );
			UserId = Intent.GetStringExtra( "UserId" );

			Refresh( );
		}

		public override bool OnCreateOptionsMenu ( IMenu menu )
		{
			MenuInflater inflater = MenuInflater;
			inflater.Inflate( Resource.Layout.Menu, menu );
			return true;
		}

		public static bool deleteDir ( File dir )
		{
			if ( dir != null && dir.IsDirectory )
			{
				String [ ] children = dir.List( );
				for ( int i = 0; i < children.Length; i++ )
				{
					bool success = deleteDir( new File( dir, children [ i ] ) );
					if ( !success )
					{
						return false;
					}
				}
			}

			return dir.Delete( );
		}

		public override bool OnOptionsItemSelected ( IMenuItem item )
		{
			// Handle item selection
			switch ( item.ItemId )
			{
				case Resource.Id.btnRefresh:
					Refresh( );
					return true;
				case Resource.Id.btnLogout:
					UserId = "";
					ISharedPreferences settings = GetSharedPreferences( "AppData", FileCreationMode.Private );
					ISharedPreferencesEditor editor = settings.Edit( );
					editor.Remove( "UserId" );
					editor.Commit( );

					File cache = Application.CacheDir;
        File appDir = new File(cache.Parent);
        if (appDir.Exists()) {
			String [ ] children = appDir.List( );
            foreach (String s in children) {
                if (!s.Equals("lib")) {
					
                    deleteDir(new File(appDir, s));
                }
            }
        }

					this.Finish( );
					return true;
				case Resource.Id.btnUpdate:
					WebClient webClient = new WebClient( );
					webClient.DownloadDataCompleted += ( s, e ) =>
					{
						var bytes = e.Result; // get the downloaded data
						string documentsPath = System.Environment.GetFolderPath( System.Environment.SpecialFolder.Personal );
						string localFilename = "DFFDelivery.apk";
						string localPath = System.IO.Path.Combine( documentsPath, localFilename );
						System.IO.File.WriteAllBytes( localPath, bytes ); // writes to local storage

						Intent intent = new Intent( Intent.ActionView );
						intent.SetDataAndType( Android.Net.Uri.FromFile( new Java.IO.File( System.Environment.GetFolderPath( System.Environment.SpecialFolder.Personal ), "DFFDelivery.apk" ) ), "application/vnd.android.package-archive" );
						StartActivityForResult( intent, 5000 );
					};
					webClient.DownloadDataAsync( new Uri( "http://delivery.dutyfreeflowers.ru/application_updates/DFFDelivery.apk" ) );
					return true;
				default:
					return base.OnOptionsItemSelected( item );
			}
		}

		public override bool OnKeyDown ( Android.Views.Keycode keyCode, Android.Views.KeyEvent e )
		{
			if ( keyCode == Keycode.Back )
			{
				this.Finish( );
				return true;
			}

			return base.OnKeyDown( keyCode, e );
		}

		void Refresh ( )
		{
			ordersList = new List<Order>( );
			lltOrders.RemoveAllViews( );

			try
			{
				string url = @"http://delivery.dutyfreeflowers.ru/android_api.php";
				//string Hash = GetMD5 ("nordwestdevelopmentgroup" + GetMD5 (GetMD5 (UserId + DateTime.Now.ToUniversalTime ().ToShortDateString())));
				string Hash = GetMD5( "nordwestdevelopmentgroup" );
				string Data = "uid=" + UserId + "&hash=" + Hash;
				string Response = GET( url, Data );
				XElement ResponseXml = XDocument.Parse( Response ).Root;

				foreach ( XElement currentElement in ResponseXml.Elements( ) )
				{
					ordersList.Add( new Order(
						int.Parse( currentElement.Element( "id" ).Value ),
						currentElement.Element( "address" ).Value,
						DateTime.Parse( currentElement.Element( "time" ).Value ),
						currentElement.Element( "fioSender" ).Value,
						currentElement.Element( "phoneSender" ).Value,
						currentElement.Element( "fioRecipient" ).Value,
						currentElement.Element( "phoneRecipient" ).Value,
						currentElement.Element( "phoneExt" ).Value,
						int.Parse( currentElement.Element( "sum" ).Value ),
						int.Parse( currentElement.Element( "prepaid" ).Value ),
						currentElement.Element( "note" ).Value ) );
				}

				if ( ordersList.Count != 0 )
				{
					foreach ( Order currentOrder in ordersList )
					{
						TextView txtOutput = new TextView( this )
						{
							Text = "Адрес: " + currentOrder.Address + "\nВремя: " + currentOrder.Time.ToShortTimeString( ) + "\nЗаказчик: " + currentOrder.SenderName + "\nТелефон: " + currentOrder.SenderPhone + "\nПолучатель: " + currentOrder.RecipientName + "\nТелефон: " + currentOrder.RecipientPhone + "\nДоп. телефон: " + currentOrder.ExtraPhone + "\nСумма: " + currentOrder.Sum.ToString( ) + "р.\nДоплата: " + currentOrder.Prepaid.ToString( ) + "р.\nЗабрать: " + currentOrder.Take.ToString( ) + "р.\nЗаметки: " + currentOrder.Note
						};

						Button btnDelete = new Button( this )
						{
							Text = "Отправить СМС",
							Tag = currentOrder.ID
						};

						btnDelete.Click += btnDelete_Click;
						lltOrders.AddView( txtOutput );
						lltOrders.AddView( btnDelete );
					}
				}
				else
				{
					TextView txtOutput = new TextView( this )
						{
							Text = "Нет заказов"
						};
					lltOrders.AddView( txtOutput );
				}
			}
			catch { Toast.MakeText( this, "Ошибка", ToastLength.Long ); }
		}

		void btnDelete_Click ( object sender, EventArgs e )
		{
			Button btnSender = ( Button ) sender;

			try
			{
				string Response = GET( @"http://delivery.dutyfreeflowers.ru/script_order_completed.php", "order_id=" + btnSender.Tag.ToString( ) );

				if ( Response == "1" )
				{
					Toast.MakeText( this, "СМС отправлено", ToastLength.Long ).Show( );
				}
				else
				{
					Toast.MakeText( this, "Ошибка при отправке СМС", ToastLength.Long ).Show( );
				}
			}
			catch { Toast.MakeText( this, "Ошибка", ToastLength.Long ); }

			Refresh( );
		}

		private static string GET ( string Url, string Data )
		{
			System.Net.WebRequest req = System.Net.WebRequest.Create( Url + "?" + Data );
			System.Net.WebResponse resp = req.GetResponse( );
			System.IO.Stream stream = resp.GetResponseStream( );
			System.IO.StreamReader sr = new System.IO.StreamReader( stream );
			string Out = sr.ReadToEnd( );
			sr.Close( );
			return Out;
		}

		private static string POST ( string Url, string Data )
		{
			System.Net.WebRequest req = System.Net.WebRequest.Create( Url );
			req.Method = "POST";
			req.Timeout = 100000;
			req.ContentType = "application/x-www-form-urlencoded";
			byte [ ] sentData = Encoding.GetEncoding( 1251 ).GetBytes( Data );
			req.ContentLength = sentData.Length;
			System.IO.Stream sendStream = req.GetRequestStream( );
			sendStream.Write( sentData, 0, sentData.Length );
			sendStream.Close( );
			System.Net.WebResponse res = req.GetResponse( );
			System.IO.Stream ReceiveStream = res.GetResponseStream( );
			System.IO.StreamReader sr = new System.IO.StreamReader( ReceiveStream, Encoding.UTF8 );
			//Кодировка указывается в зависимости от кодировки ответа сервера
			Char [ ] read = new Char [ 256 ];
			int count = sr.Read( read, 0, 256 );
			string Out = String.Empty;
			while ( count > 0 )
			{
				String str = new String( read, 0, count );
				Out += str;
				count = sr.Read( read, 0, 256 );
			}
			return Out;
		}

		public static string GetMD5 ( string input )
		{
			MD5 MD5Hasher = MD5.Create( );
			byte [ ] data = MD5Hasher.ComputeHash( Encoding.Default.GetBytes( input ) );
			StringBuilder sBuilder = new StringBuilder( );
			for ( int i = 0; i < data.Length; i++ )
			{
				sBuilder.Append( data [ i ].ToString( "x2" ) );
			}
			return sBuilder.ToString( );
		}

		static public string EncodeTo64 ( string toEncode )
		{
			byte [ ] toEncodeAsBytes
				= System.Text.ASCIIEncoding.ASCII.GetBytes( toEncode );
			string returnValue
				= System.Convert.ToBase64String( toEncodeAsBytes );
			return returnValue;
		}
	}
}