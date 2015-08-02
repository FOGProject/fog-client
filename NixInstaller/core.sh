#!/bin/bash

########################
# Variables
########################
# $1 fogURL $2 useTray $3 https(not required)
$version="0.9.9"
########################
# Perform Installation
########################

echo "Checking for existing installations...."
if [ -f /opt/fog-service/FOGService.exe ]; then
    echo "Existing installation found, now performing uninstallation...."
    find /opt/fog-service -maxdepth 1 -type f -exec rm {} \;
    echo "Done!"
    
else
	if [ -d /opt/fog-service ]; then
		echo "Creating Required Directories...."
		mkdir -p /opt/fog-service
		echo "Done!"
	fi
fi
echo "Downloading files..."
curl -o /opt/FOGService.zip $1client/FOGService.zip
echo "Extracting files..."
unzip -o /opt/FOGService.zip /opt/fog-service
echo "Adjusting permissions..."
touch /opt/fog-service/fog.log
chmod 775 /opt/fog-service/fog.log

#sent variables fogURL useTray version company rootlog https

mono /opt/fog-service/SetupHelper.exe $1 $2 $version "FOG" "0" $3

########################
# Clean Up
########################

rm /opt/FOGService.zip
rm /opt/fog-service/SetupHelper.exe
