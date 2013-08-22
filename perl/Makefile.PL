#!/usr/bin/perl
#
# $Id: Makefile.PL 108 2008-08-07 17:35:00Z jeff $
#
# Authors:
#      Jeff Buchbinder <jeff@freemedsoftware.org>
#
# FreeMED Electronic Medical Record and Practice Management System
# Copyright (C) 1999-2008 FreeMED Software Foundation
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

use strict;
use warnings;
use ExtUtils::MakeMaker;

WriteMakefile(
    NAME                => 'FreeMED::Relay',
    AUTHOR              => 'Jeff Buchbinder <RTFIREFLY@cpan.org>',
    VERSION_FROM        => 'lib/FreeMED/Relay.pm',
    ABSTRACT_FROM       => 'lib/FreeMED/Relay.pm',
    PL_FILES            => {},
    PREREQ_PM => {
	'JSON' => '1.14',
	'LWP::UserAgent' => '2.033',
        'Test::More' => 0,
    },
    dist                => { COMPRESS => 'gzip -9f', SUFFIX => 'gz', },
    clean               => { FILES => 'FreeMED-Relay-*' },
);

