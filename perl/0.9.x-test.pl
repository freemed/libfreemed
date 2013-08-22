#!/usr/bin/perl
#
# $Id: 0.9.x-test.pl 108 2008-08-07 17:35:00Z jeff $
#
# Authors:
#      Jeff Buchbinder <jeff@freemedsoftware.org>
#
# FreeMED Electronic Medical Record and Practice Management System
# Copyright (C) 1999-2013 FreeMED Software Foundation
#
# This program is free software; you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation; either version 2 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program; if not, write to the Free Software
# Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

use lib 'lib';

use FreeMED::Relay;
use Data::Dumper;
use Config::Tiny;

my $config = Config::Tiny->read("0.9.x-config");
my $f = FreeMED::Relay->new( debug => 1 );
$f->set_credentials(
	$config->{'installation'}->{'uri'},
	$config->{'installation'}->{'username'},
	$config->{'installation'}->{'password'}
);
$f->login();

print "org.freemedsoftware.api.Scheduler.GetDailyAppointments()\n";
	print Dumper($f->call('org.freemedsoftware.api.Scheduler.GetDailyAppointments', '2007-01-12'));
	print "-"x50 . "\n";

print "org.freemedsoftware.api.PatientInterface.picklist('doe')\n";
	print Dumper($f->call('org.freemedsoftware.api.PatientInterface.picklist', 'doe'));
	print "-"x50 . "\n";

