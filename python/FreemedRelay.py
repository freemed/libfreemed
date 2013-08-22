#!/usr/bin/python
#
# $Id: FreemedRelay.py 87 2007-09-06 01:19:23Z jeff $
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

import os.path
import sys
import simplejson
import urllib, MultipartPostHandler
from UserDict import UserDict

class FreemedRelay(UserDict):
	"FreeMED 0.9.x+ data relay binding"
	def __init__( self, url, debug=False ):
		UserDict.__init__( self )
		self['url'] = url

		self['COOKIEFILE'] = 'freemed-cookies.lwp'

		if debug:
			import logging
			logger = logging.getLogger("cookielib")
			logger.addHandler(logging.StreamHandler(sys.stdout))
			logger.setLevel(logging.DEBUG)
	
		cj = None
		ClientCookie = None
		cookielib = None
	
		try:
			import cookielib            
		except ImportError:
			pass
		else:
			import urllib2    
			urlopen = urllib2.urlopen
			cj = cookielib.LWPCookieJar()
			Request = urllib2.Request

		if not cookielib:
			try:                                            
				import ClientCookie 
			except ImportError:
				import urllib2
				urlopen = urllib2.urlopen
				Request = urllib2.Request
			else:
				urlopen = ClientCookie.urlopen
				cj = ClientCookie.LWPCookieJar()
				Request = ClientCookie.Request
		if cj != None:
			if os.path.isfile( self['COOKIEFILE'] ):
				if debug:
					print 'DEBUG: Loading cookiefile ' + self['COOKIEFILE']
				cj.load( self['COOKIEFILE'] )
			if cookielib:
				opener = urllib2.build_opener(urllib2.HTTPCookieProcessor(cj), MultipartPostHandler.MultipartPostHandler)
				urllib2.install_opener(opener)
			else:
				opener = ClientCookie.build_opener(ClientCookie.HTTPCookieProcessor(cj), MultipartPostHandler.MultipartPostHandler)
				ClientCookie.install_opener(opener)
		self['Request'] = Request
		self['urlopen'] = urlopen
		self['cj'] = cj

	def LoadURL( self, method, data=None, postFiles=None, debug=False ):
		if data is None:
			txdata = None
		else:
			txdata = { }
			count = 0
			for iter in data:
				txdata[ 'param' + str( count ) ] = simplejson.dumps( iter )
				count += 1
			txdata = urllib.urlencode( txdata )
			if debug:
				print 'DEBUG: ' + txdata

		if postFiles is not None:
			for (k, v) in postFiles:
				txdata[ k ] = open( v )
				if debug:
					print 'DEBUG (POST): ' + k + ' = ' + v
	
		txheaders =  {'User-agent' : 'Mozilla/4.0 (compatible; MSIE 5.5; Windows NT)'}
	
		try:
			fullurl = self['url'] + method
			if debug:
				print 'DEBUG: fullurl = ' + fullurl
			req = self['Request']( fullurl, txdata, txheaders )
			handle = self['urlopen']( req )
			val = handle.read()
		except IOError, e:
			print 'We failed to open "%s".' % fullurl
			if hasattr(e, 'code'):
				print 'We failed with error code - %s.' % e.code
			elif hasattr(e, 'reason'):
				print "The error object has the following 'reason' attribute :", e.reason
				print "This usually means the server doesn't exist, is down, or we don't have an internet connection."
			else:
				print 'Here are the headers of the page :'
				print handle.info()
		
		print ' '
		if self['cj'] == None:
			print "We don't have a cookie library available - sorry."
			print "I can't show you any cookies."
			sys.exit()
		else:
			if debug:
				print 'These are the cookies we have received so far :'
				for index, cookie in enumerate(self['cj']):
					print index, '  :  ', cookie
				print 'DEBUG: save cookies in ' + self['COOKIEFILE']
			self['cj'].save( self['COOKIEFILE'] )
			try:
				ret = simplejson.loads( val )
			except ValueError:
				print 'ERROR, RETURNED: ' + val
				ret = False
			return ret

#
#	Test suite goes here:::
#

if __name__ == "__main__":
	BASE = 'http://localhost/freemed'
	RELAY_PATH = '/relay.php/json/'
	
	x = FreemedRelay( BASE + RELAY_PATH )
	print 'org.freemedsoftware.public.Login.Validate: '
	print x.LoadURL( 'org.freemedsoftware.public.Login.Validate', [ 'demo', 'demo' ] )
	print "\n----------------------------------------\n"
	print 'org.freemedsoftware.api.Scheduler.GetDailyAppointments("2007-01-12"): '
	print x.LoadURL( 'org.freemedsoftware.api.Scheduler.GetDailyAppointments', ['2007-01-02'] )
	print "\n----------------------------------------\n"
	print 'org.freemedsoftware.api.Messages.view(): '
	print x.LoadURL( 'org.freemedsoftware.api.Messages.view' )
	print "\n----------------------------------------\n"

