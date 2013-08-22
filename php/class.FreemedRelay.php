<?php
 // $Id: class.FreemedRelay.php 60 2007-08-06 17:03:27Z jeff $
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

class FreemedRelay {

	protected $uri;
	protected $username;
	protected $password;
	protected $cookiefile;

	public function __construct ( $uri ) {
		$this->uri = $uri . "/relay.php/json/";
		$this->cookiefile = tempnam( '/tmp', 'freemedrelay' );
		$this->loggedin = false;
	} // end constructor

	public function SetCredentials ( $username, $password ) {
		$this->username = $username;
		$this->password = $password;
	} // end method SetCredentials

	// Method: Login
	//
	//	Login to FreeMED 0.9.x+ data relay.
	//
	public function Login ( ) {
		$res = $this->curl_call( $this->uri.'org.freemedsoftware.public.Login.Validate', array ( $this->username, $this->password ) );
		$res = json_decode( $res );
		if ( $res ) {
			$this->loggedin = true;
		}
		return $res;
	} // end method Login

	// Method: Call
	//
	//	Call a remote method through the FreeMED 0.9.x+ data relay.
	//
	// Parameters:
	//
	//	$namespace - Relay namespace. (example: 'org.freemedsoftware.public.Login.Validate')
	//
	//	$params - (optional) Array of parameters
	//
	//	$files - (optional) Hash of files for multipart attachments
	//
	// Returns:
	//
	//	PHP object or other encoding.
	//
	public function Call ( $namespace, $params = NULL, $files = NULL ) {
		$url = $this->uri . $namespace;
		$res = json_decode( $this->curl_call( $url, $params, $files ) );
		return $res;
	} // end method Call

	// Method: curl_call
	//
	//	Low-level cURL call.
	//
	// Parameters:
	//
	//	$url - Fully qualified URL
	//
	//	$params - (optional) Array of parameters
	//
	//	$files - (optional) Hash of file variables for multipart attachment
	//
	// Returns:
	//
	//	JSON-encoded body.
	//
	protected function curl_call ( $url, $params = NULL, $files = NULL ) {
		//print "Using $url\n";
		if ( ( strpos( $url, '.public.') === false ) and ! $this->loggedin ) {
			syslog( LOG_ERR, "Need to be logged in first" );
			return false;
		}

		$c = curl_init( );
		curl_setopt( $c, CURL_TIMEOUT, 120 );
		curl_setopt( $c, CURLOPT_URL, $url ); 
		curl_setopt( $c, CURLOPT_HEADER, 0 ); 
		curl_setopt( $c, CURLOPT_RETURNTRANSFER, 1 ); 
		curl_setopt( $c, CURLOPT_POST, 1 );
		$p = array ( );
		if ( $params != NULL ) {
			$p = array_merge( $p, $this->encode_parameters( $params ) ); 
		}
		if ( $files != NULL ) {
			foreach ( $files AS $v => $f ) {
				$m[ $v ] = '@' . $f; 
			}
			$p = array_merge( $p, $m );
		}
		curl_setopt( $c, CURLOPT_POSTFIELDS, $p ); 
		curl_setopt( $c, CURLOPT_COOKIEFILE, $this->cookiefile ); 
		curl_setopt( $c, CURLOPT_COOKIEJAR, $this->cookiefile ); 
		$data = curl_exec( $c );
		curl_close( $c );
		//print "[ DEBUG : $data ]\n";
		return $data;
	} // end method curl_call

	// Method: encode_parameters
	//
	//	Prepare parameters for the data relay.
	//
	// Parameters:
	//
	//	$params - Array of parameters to be passed.
	//
	// Returns:
	//
	//	Hash of parameters for POST-ing to the data relay.
	//
	protected function encode_parameters ( $params ) {
		// Form params
		if (is_array( $params )) {
			for ( $i=0; $i<count($params); $i++ ) {
				$f['param'.( $i + 0 )] = json_encode( $params[$i] );
			}
		} else {
			$f = '';
		}
		//print_r($f);
		return $f;
	} // end method encode_parameters

} // end class FreemedRelay

?>
