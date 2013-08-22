/*
 * $Id: Relay.cs 44 2007-02-06 13:14:22Z jeff $
 *
 * Authors:
 * 	Jeff Buchbinder <jeff@freemedsoftware.org>
 *
 * FreeMED Electronic Medical Record and Practice Management System
 * Copyright (C) 1999-2013 FreeMED Software Foundation
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 */

using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;

using Newtonsoft.Json;

namespace org.freemedsoftware.FreeMED {

public class Relay {
	private string _uri;
	private string _username;
	private string _password;
	private bool _debug;
	private CookieCollection _cookies;

	public Relay ( ) {
		_cookies = null;
		_uri = null;
		_username = null;
		_password = null;
		_debug = false;
	}

	public bool Debug {
		get { return _debug; }
		set { _debug = value; }
	}
	
	public void SetParameters ( string s_uri, string s_username, string s_password ) {
		_uri = s_uri;
		_username = s_username;
		_password = s_password;
	} // end method SetParameters

	public bool Login ( ) {
		if ( (_uri.Length < 5) || (_username.Length < 3) ) {
			Console.WriteLine("Need to call SetParameters()");
			return false;
		}
		return (bool) Call( 
			"org.freemedsoftware.public.Login.Validate",
			new object[] {
				_username,
				_password
			}
		);
	} // end method Login

	public object Call ( string method, object[] parameters ) {
		return Call ( method, parameters, null );
	}
	public object Call ( string method, object[] parameters, Type type ) {
		string outputValue = CallRaw( method, parameters, type );
		object deserializedObject;
		if ( type == null ) {
			deserializedObject = (object) JavaScriptConvert.DeserializeObject(outputValue);
		} else {
			deserializedObject = JavaScriptConvert.DeserializeObject(outputValue, type);
		}
		if ( _debug ) { Console.WriteLine( "DEBUG: deserialized = {0}", deserializedObject.ToString() ); }
		return deserializedObject;
	}
	public string CallRaw ( string method, object[] parameters, Type type ) {
		string fullUri = String.Format( "{0}/relay.php/json/{1}", _uri, method );
		if ( _debug ) { Console.WriteLine( "DEBUG: POST to {0} with {1}", fullUri, parameters.ToString() ); }
		HttpWebRequest webRequest = (HttpWebRequest) HttpWebRequest.Create( fullUri );
		webRequest.ContentType = "application/x-www-form-urlencoded";
		webRequest.Method = "POST";
		webRequest.CookieContainer = new CookieContainer();
		if (_cookies != null) {
			webRequest.CookieContainer.Add( _cookies );
		}

		// Form parameters
		string formedParameters = "";
		for (int i=0; i<parameters.Length; i++) {
			Type objectType = parameters[i].GetType();
			if ( _debug ) { Console.WriteLine("DEBUG: Processing parameter {0} type = {1}", i, objectType.ToString() ); }
			if ( formedParameters.Length > 0 ) { formedParameters += "&"; }
			switch (objectType.ToString()) {
				case "System.Int32":
				formedParameters += "param" + i.ToString() + "=" + parameters[i].ToString();
				break;

				case "System.String":
				formedParameters += "param" + i.ToString() + "=" + parameters[i];
				break;

				default:
				if ( _debug ) { Console.WriteLine( "DEBUG: serializing non-understood parameter" ); }
				// Check for ToJson() method
				try {
					MethodInfo[] mi = objectType.GetMethods();
					bool found = false;
					foreach (MethodInfo m in mi) {
						if ( m.Name == "ToJson" ) {
							if ( _debug ) { Console.WriteLine( "DEBUG: found ToJson() method" ); }
							found = true;
						}
					}
					if (found) {
						MethodInfo ToJson = objectType.GetMethod ("ToJson");
						formedParameters += "param" + i.ToString() + "=" + System.Web.HttpUtility.UrlEncode( (string) ToJson.Invoke (parameters[i], null) );
					} else {
						throw new Exception( "parameter" );
					}
				} catch (Exception) {
					formedParameters += "param" + i.ToString() + "=" + (string) JavaScriptConvert.SerializeObject( parameters[i] );
				}
				break;
			}
		}

		if ( _debug ) { Console.WriteLine( "DEBUG: formedParameters = '{0}'", formedParameters ); }
		byte[] bytes = Encoding.ASCII.GetBytes( formedParameters );
		Stream os = null;
		try {
			webRequest.ContentLength = bytes.Length;
			os = webRequest.GetRequestStream( );
			os.Write( bytes, 0, bytes.Length );
		} catch ( WebException ex ) {
			Console.WriteLine( "ERROR: Call() caught exception for {0} ({1})", fullUri, ex.Message );
			return null;
		} finally {
			if (os != null) { os.Close( ); }
		}

		try {
			HttpWebResponse webResponse = (HttpWebResponse) webRequest.GetResponse( );
			if ( webResponse == null ) { return null; }
			StreamReader sr = new StreamReader( webResponse.GetResponseStream( ) );
			string outputValue = sr.ReadToEnd( ).Trim( );
			if ( _debug ) { Console.WriteLine( "DEBUG: Returned {0}", outputValue ); }

			// Pass cookie value back
			if (webResponse.Cookies.Count > 0) {
				_cookies = webResponse.Cookies;
			}
			if ( _debug ) { Console.WriteLine( "DEBUG: Finished importing new cookies, if any" ); }

			try {
				if ( _debug ) { Console.WriteLine( "DEBUG: About to examine outputValue" ); }
				if ( ( outputValue == null ) || ( outputValue == "" ) ) {
					if ( _debug ) { Console.WriteLine( "DEBUG: NULL output value found" ); }
					return "false";
				}
				if ( _debug ) { Console.WriteLine( "DEBUG: About to return outputValue" ); }
				return outputValue;
			} catch (NullReferenceException iEx) {
				Console.WriteLine( "ERROR: Caught exception for {0}", iEx.Message );
				return "";
			}
		} catch ( WebException ex ) {
			Console.WriteLine( "ERROR: Caught exception for {0} ({1})", fullUri, ex.Message );
		} 

		return null;
	} // end Call

} // end class

} // end namespace

