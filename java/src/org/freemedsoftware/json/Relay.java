/*
 * $Id: Relay.java 114 2009-05-05 14:48:03Z jeff $
 *
 * Authors:
 *      Adam Buchbinder <adam.buchbinder@gmail.com>
 *      Jeff Buchbinder <jeff@freemedsoftware.org>
 *
 * FreeMED Electronic Medical Record and Practice Management System
 * Copyright (C) 1999-2009 FreeMED Software Foundation
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
 */

package org.freemedsoftware.json;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.PrintStream;
import java.io.UnsupportedEncodingException;
import java.net.CookieHandler;
import java.net.HttpURLConnection;
import java.net.URI;
import java.net.URL;
import java.net.URLEncoder;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Properties;
import java.util.logging.Logger;

import javax.net.ssl.HttpsURLConnection;

import net.sf.sojo.interchange.Serializer;
import net.sf.sojo.interchange.json.JsonSerializer;

import org.apache.log4j.PropertyConfigurator;

/**
 * Relay class for communicating with FreeMED 0.9.x+.
 */
public class Relay {
	/**
	 * Java 1.5 has the CookieHandler API, but not the default CookieManager
	 * implementation. *Retarded*. Copied from
	 * http://java.sun.com/j2se/1.5.0/docs/guide/net/http-cookie.html
	 */
	class MyCookieHandler extends CookieHandler {
		/**
		 * Internal cookiejar to hold cookies.
		 */
		private List<String> cookieJar = new ArrayList<String>();

		public Map<String, List<String>> get(URI uri,
				Map<String, List<String>> rqstHdrs) throws IOException {
			// the cookies will be included in request
			Map<String, List<String>> map = new HashMap<String, List<String>>();
			List<String> l = cookieJar;
			map.put("Cookie", l);
			return Collections.unmodifiableMap(map);
		}

		/**
		 * Add all response headers to the internal cookiejar.
		 */
		@SuppressWarnings("unchecked")
		public void put(URI uri, Map responseHeaders) throws IOException {
			cookieJar.addAll((List) responseHeaders.get("Set-Cookie"));
		}
	}

	protected static Logger log = Logger.getLogger("Relay");

	private boolean DEBUG = true;

	private boolean loggedIn = false;

	private String host = "", path = "", user = "", pass = "", protocol = "";

	private int port = 80;

	public boolean isLoggedIn() {
		return loggedIn;
	}

	public String getHost() {
		return host;
	}

	public void setHost(String host) {
		this.host = host;
		loggedIn = false;
	}

	public String getPath() {
		return path;
	}

	public void setPath(String path) {
		this.path = path;
		loggedIn = false;
	}

	public String getUser() {
		return user;
	}

	public void setUser(String user) {
		this.user = user;
		loggedIn = false;
	}

	private String getPass() {
		return pass;
	}

	public void setPass(String pass) {
		this.pass = pass;
		loggedIn = false;
	}

	public int getPort() {
		return port;
	}

	public void setPort(int port) {
		this.port = port;
		loggedIn = false;
	}

	public String getProtocol() {
		return protocol;
	}

	public void setProtocol(String protocol) {
		this.protocol = protocol;
		loggedIn = false;
	}

	public Relay() {
		loggedIn = false;

		// Initialize logger if it
		try {
			String log4jfile = System.getProperty("log4j.properties");
			// System.out.println("log4j-properties: " + log4jfile);
			if (log4jfile != null) {
				String propertiesFilename = log4jfile;
				PropertyConfigurator.configure(propertiesFilename);
				log.info("Logger initialized.");
			} else {
				// System.out.println("Error setting up logger.");
			}
		} catch (Exception ex) {
			System.out.println("Couldn't set up log4j : " + ex);
		}

		// TODO make sure we really need to do this
		CookieHandler.setDefault(new MyCookieHandler());
	}

	/**
	 * Load FreeMED server connection information from a Java properties file.
	 * 
	 * @param fileName
	 *            Fully qualified path to properties file.
	 * @throws IOException
	 */
	public void loadProperties(String fileName) throws IOException {
		Properties props = new Properties();
		InputStream inputStream = ClassLoader
				.getSystemResourceAsStream(fileName);
		if (inputStream == null)
			throw new IOException("Couldn't open " + fileName + ".");
		props.load(inputStream);
		inputStream.close();
		protocol = props.getProperty("freemed.protocol");
		host = props.getProperty("freemed.host");
		path = props.getProperty("freemed.path");
		port = Integer.parseInt(props.getProperty("freemed.port"));
		user = props.getProperty("freemed.user");
		pass = props.getProperty("freemed.pass");
	}

	/**
	 * Log in.
	 * 
	 * @returns true if the user is logged in; false if it failed.
	 */
	public boolean logIn() {
		if (isLoggedIn())
			return true;
		try {
			if (call("org.freemedsoftware.public.Login.Validate",
					new Object[] { getUser(), getPass() }).equals("true")) {
				return true;
			} else {
				return false;
			}
		} catch (UnsupportedEncodingException uee) {
			log.warning("Unsupported encoding");
			return false;
		}
	}

	/**
	 * Call a named JSON method on the FreeMED server.
	 * 
	 * @param method
	 *            Fully qualified method name (example:
	 *            "org.freemedsoftware.public.Login.Validate")
	 * @param params
	 *            Array of objects to serialize into the JSON call as
	 *            parameters.
	 * @return
	 * @throws UnsupportedEncodingException
	 *             If JSON encoding isn't supported, throws this exception.
	 */
	public String call(String method, Object[] params)
			throws UnsupportedEncodingException {
		if (!isLoggedIn() && !method.matches(".*public.*")) {
			if (DEBUG) {
				System.out.println("Must be logged in first.");
			}
			log.warning("Must be logged in first.");
			return null;
		}

		HttpURLConnection conn = null;
		try {
			URL url = new URL(protocol, host, port, path + method);
			if (protocol.compareToIgnoreCase("http") == 0) {
				conn = (HttpURLConnection) url.openConnection();
			} else if (protocol.compareToIgnoreCase("https") == 0) {
				conn = (HttpsURLConnection) url.openConnection();
			} else {
				throw new Exception("Invalid protocol " + protocol
						+ " specified");
			}
			conn.setRequestMethod("POST");
		} catch (IOException e) {
			System.out.println("Failed to open connection: " + e);
			return null;
		} catch (Exception e) {
			System.out.println("Failed to create connection: " + e);
		}
		conn.setRequestProperty("Content-type",
				"application/x-www-form-urlencoded");
		conn.setUseCaches(false);
		conn.setDoOutput(true);
		conn.setDoInput(true);
		// construct array from params
		StringBuffer sb = new StringBuffer();
		int count = 0;
		for (Object p : params) {
			if (count > 0)
				sb.append("&");
			Serializer serializer = new JsonSerializer();
			String json = serializer.serialize(p).toString();
			sb.append("param" + count + "=" + URLEncoder.encode(json, "UTF-8"));
			count++;
		}

		StringBuffer result = new StringBuffer();
		try {
			PrintStream os = new PrintStream(conn.getOutputStream());
			os.print(sb.toString());
			os.flush();
			os.close();

			BufferedReader reader = new BufferedReader(new InputStreamReader(
					conn.getInputStream()));
			String line;
			while ((line = reader.readLine()) != null) {
				result.append(line);
			}
			reader.close();

			conn.disconnect();
		} catch (IOException e) {
			log.warning("IO failure " + e.toString());
			System.out.println("IO failure: " + e);
			e.printStackTrace();
			return null;
		}

		// TODO JSON-decode?
		return result.toString();
	}

	public static void main(String[] argv) {
		Relay r = new Relay();
		try {
			r.loadProperties("freemed.properties");
		} catch (IOException e) {
			System.out
					.println("Couldn't find freemed.properties in classpath.");
			System.exit(1);
		}
		if (r.logIn()) {
			System.out.println("Login successful.");
		} else {
			System.out.println("Login failed.");
		}
		/*
		 * String result = r.call("org.freemedsoftware.public.Login.Validate",
		 * new Object[] { r.getUser(), r.getPass() });
		 * System.out.println("Result = "+result);
		 */
	}
}
