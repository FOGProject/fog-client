#!/bin/bash
die () {
    echo >&2 "$@"
    exit 1
}
[ "$#" -eq 1 ] || die "1 argument required, $# provided"

protocol=`echo $1 | awk -F: '{print $1;}'`
domain=`echo $1 | awk -F/ '{print $3;}'`
webroot=`echo $1`

echo "Protocol: $protocol"
echo "Domain:   $domain"
echo "WebRoot:  $webroot"