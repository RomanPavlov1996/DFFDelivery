using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
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
using Android.Preferences;

namespace DFFDelivery
{
	[Activity( Label = "DFFDelivery", MainLauncher = true, Theme = "@android:style/Theme.Holo.Light" )]
	public class AuthActivity : Activity
	{
		public WebView AuthWebView;
		static AuthActivity a;
		public string UserId = "";

		protected override void OnCreate ( Bundle bundle )
		{
			base.OnCreate( bundle );

			try
			{
				ISharedPreferences settings = GetSharedPreferences( "AppData", FileCreationMode.Private );
				UserId = settings.GetString( "UserId", "" );
			}
			catch { }

			if ( UserId != "" )
			{
				OpenMainActivity( UserId );
			}
			else
			{
				SetContentView( Resource.Layout.Auth );

				AuthWebView = ( WebView ) FindViewById( Resource.Id.AuthWebView );
				AuthWebView.ClearHistory( );
				AuthWebView.ClearFormData( );
				AuthWebView.ClearCache( true );
				AuthWebView.LoadUrl( "https://oauth.vk.com/authorize?client_id=3445352&scope=&redirect_uri=http://oauth.vk.com/blank.html&display=touch&response_type=token" );
				AuthWebView.SetWebViewClient( new AuthWebViewClient( ) );
				a = this;
			}
		}

		private void OpenMainActivity ( string UserId )
		{
			Intent NextActivity = new Intent( this, typeof( MainActivity ) );
			NextActivity.PutExtra( "UserId", UserId );
			StartActivity( NextActivity );

			//AuthWebView.LoadUrl( "http://vk.com/" );

			UserId = "";
			AuthWebView = new WebView( this );

			this.Finish( );
		}

		private void SaveUserId ( string UserId )
		{
			ISharedPreferences settings = GetSharedPreferences( "AppData", FileCreationMode.Private );
			ISharedPreferencesEditor editor = settings.Edit( );
			editor.PutString( "UserId", UserId );
			editor.Commit( );
		}

		class AuthWebViewClient : WebViewClient
		{
			public override void OnPageStarted ( WebView view, string url, Android.Graphics.Bitmap favicon )
			{
				base.OnPageStarted( view, url, favicon );

				if ( url.Contains( "user_id=" ) )
				{
					int IdIndex = url.IndexOf( "user_id=" ) + 8;
					string UserId = url.Substring( IdIndex, url.Length - IdIndex );

					a.SaveUserId( UserId );
					a.OpenMainActivity( UserId );
				}
			}
		}
	}
}