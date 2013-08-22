/*
 * $Id: RelayTest.cs 88 2007-09-06 01:44:08Z jeff $
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
using System.Collections;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;

using Newtonsoft.Json;
using org.freemedsoftware.FreeMED;

namespace org.freemedsoftware.FreeMED {

class RelayTest {
	public class ApptRecord {
		public ApptRecord ( ) { }

		public string date_of;
		public string date_of_mdy;
		public string hour;
		public string minute;
		public string appointment_time;
		public string provider;
		public int provider_id;
		public string patient;
		public int patient_id;
		public string note;
		public string status;
		public string status_color;
		public int scheduler_id;
		public int duration;
		public string resource_type;
	}

	public static int Main ( string[] argv ) {
		Console.WriteLine("org.freemedsoftware.FreeMED.Relay Test Suite $Revision: 88 $");
		if ( argv.Length != 3 ) {
			Console.WriteLine("syntax : program URL username password");
			return 1;
		}
		Console.WriteLine(" * Attempting login");
		Console.WriteLine(" - Creating Relay instance");
		Relay r = new Relay();
		Console.WriteLine(" - Debug = true");
		r.Debug = true;
		Console.WriteLine(" - SetParameters ( {0}, {1}, {2} )", argv[0], argv[1], argv[2]);
		r.SetParameters( argv[0], argv[1], argv[2] );
		Console.WriteLine(" - Login( )");
		r.Login();
		Console.WriteLine(" - Call org.freemedsoftware.api.Scheduler.GetDailyAppointments");
		string val = r.CallRaw( "org.freemedsoftware.api.Scheduler.GetDailyAppointments", new object[] { "2006-01-12" }, null );
		ApptRecord[] x = (ApptRecord []) JavaScriptConvert.DeserializeObject( val, typeof(ApptRecord[]) );
		Console.WriteLine( "ApptResults.Length = " + x.Length );
		for (int i=0; i<x.Length; i++) {
			Console.WriteLine(" - Patient : {0} ({1}), Date : {2}", x[i].patient, x[i].patient_id, x[i].date_of );
		}
		return 0;
	}
}

} // end namespace

