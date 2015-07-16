#!/bin/bash
die () {
    echo >&2 "$@"
    exit 1
}

########################
# Check Dependencies
########################

if [ "$(id -u)" != "0" ]; then
   die "This script must be run as root"
fi
[ "$#" -eq 1 ] || die "1 argument required, $# provided"

critical_commands=(mono mono-service unzip curl pkill);
optional_commands=(xprintidle);

echo "Checking required dependencies..."

for i in ${critical_commands[@]}; do
        if [ '$(hash $i)' != '' ]; then
			printf "%-20s %-30s" $i "Failed";
			exit 1;
		else
			printf "%-20s %-30s" $i "Success";
		fi
		echo "";
done

echo ""	
echo "Checking optional dependencies..."
for i in ${optional_commands[@]}; do
        if [ '$(hash $i)' != ''  ]; then
			printf "%-20s %-30s" $i "Failed";
		else
			printf "%-20s %-30s" $i "Success";
		fi
		echo "";
done

########################
# Perform Installation
########################

echo ""
echo "Downloading files..."
curl -o /opt/FOGService.zip $1client/download.php?newclientzip
echo "Extracting files..."
unzip /opt/FOGService.zip /opt/fog-service
echo "Adjusting permissions..."
touch /opt/fog-service/fog.log
chmod 775 /opt/fog-service/fog.log

mono /opt/fog-service/SetupHelper.exe $1

########################
# Clean Up
########################

rm /opt/FOGService.zip
rm /opt/fog-service/SetupHelper.exe