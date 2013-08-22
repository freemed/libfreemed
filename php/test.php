#!/usr/bin/php
<?php
 // $Id: test.php 60 2007-08-06 17:03:27Z jeff $
 //
 // Authors:
 //      Jeff Buchbinder <jeff@freemedsoftware.org>
 //
 // FreeMED Electronic Medical Record and Practice Management System
 // Copyright (C) 1999-2013 FreeMED Software Foundation
 //
 // This program is free software; you can redistribute it and/or modify
 // it under the terms of the GNU General Public License as published by
 // the Free Software Foundation; either version 2 of the License, or
 // (at your option) any later version.
 //
 // This program is distributed in the hope that it will be useful,
 // but WITHOUT ANY WARRANTY; without even the implied warranty of
 // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 // GNU General Public License for more details.
 //
 // You should have received a copy of the GNU General Public License
 // along with this program; if not, write to the Free Software
 // Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

include_once(dirname(__FILE__).'/class.FreemedRelay.php');
openlog( 'libfreemed', LOG_PID | LOG_PERROR, LOG_LOCAL0 );

if ( $argc != 4 ) {
	print "syntax: $argv[0] url username password\n";
	print " example: \n";
	print "  $argv[0] http://localhost/freemed user myPassword\n";
	exit(0);
}

$r = new FreemedRelay( $argv[1] );
$r->SetCredentials( $argv[2], $argv[3] );
if ( $r->Login() ) {
	print "Connected to data relay!\n";
} else {
	die( "Failed to connect\n" );
}

// Now, quick test of the data relay
print "org.freemedsoftware.api.SystemConfig.GetAll : \n";
print_r( $r->Call( 'org.freemedsoftware.api.SystemConfig.GetAll' ) );
print "-------------------------------------------------------------------------- \n\n";
print "org.freemedsoftware.api.Scheduler.GetDailyAppointments : \n";
print_r( $r->Call( 'org.freemedsoftware.api.Scheduler.GetDailyAppointments', array ( '2007-08-01' ) ) );
print "-------------------------------------------------------------------------- \n\n";

?>
