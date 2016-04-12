#!/bin/sh
##############################################################################
####  TO DO: recreate this script. It is not in any way linux compatable  ####
##############################################################################

compName=`scutil --get HostName`;
adName="$1";
adOU="$2";
adUser="$3";
adPass="$4";

domainNow=`/usr/sbin/dsconfigad -show | /usr/bin/grep -i "Active Directory Domain" | /usr/bin/sed -n 's/[^.]*= //p'`
 
if [ "$domainNow" = "$adName" ]
    then
    exit 1
fi

####  Check if required variables hav ben populated
if [ "$doAD" == "1" ] && [ "$adUser" != "" ] && [ "$adName" != "" ] && [ "$adPass" != "" ]; then
	echo "Binding to directory";
	computerid="$compName";
 
	# Standard parameters
	domain="$adName"			# fully qualified DNS name of Active Directory Domain
	udn="$adUser"				# username of a privileged network user
	password="$adPass"			# password of a privileged network user
	ou="$adOU"					# Distinguished name of container for the computer
	 
	# Advanced options
	alldomains="enable"			# 'enable' or 'disable' automatic multi-domain authentication
	localhome="enable"			# 'enable' or 'disable' force home directory to local drive
	protocol="smb"				# 'afp' or 'smb' change how home is mounted from server
	mobile="enable"				# 'enable' or 'disable' mobile account support for offline logon
	mobileconfirm="disable"		# 'enable' or 'disable' warn the user that a mobile acct will be created
	useuncpath="disable"		# 'enable' or 'disable' use AD SMBHome attribute to determine the home dir
	user_shell="/bin/bash"		# e.g., /bin/bash or "none"
	preferred="-nopreferred"	# Use the specified server for all Directory lookups and authentication
								# (e.g. "-nopreferred" or "-preferred ad.server.edu")
	admingroups="$adName\domain admins"	# These comma-separated AD groups may administer the machine (e.g. "" or "APPLE\mac admins")
	 
	
	### End of configuration
	 
	# Activate the AD plugin
	defaults write /Library/Preferences/DirectoryService/DirectoryService "Active Directory" "Active"
	plutil -convert xml1 /Library/Preferences/DirectoryService/DirectoryService.plist
	sleep 5
	 
	# Bind to AD
	dsconfigad -f -a $computerid -domain $domain -u $udn -p "$password" -ou "$ou"
	 
	# Configure advanced AD plugin options
	if [ "$admingroups" = "" ]; then
		dsconfigad -nogroups
	else
		dsconfigad -groups "$admingroups"
	fi
	 
	dsconfigad -alldomains $alldomains -localhome $localhome -protocol $protocol \
		-mobile $mobile -mobileconfirm $mobileconfirm -useuncpath $useuncpath \
		-shell $user_shell $preferred
	 
	# Restart DirectoryService (necessary to reload AD plugin activation settings)
	killall DirectoryService
	 
	# Add the AD node to the search path
	if [ "$alldomains" = "enable" ]; then
		csp="/Active Directory/All Domains"
	else
		csp="/Active Directory/$domain"
	fi
	 
	dscl /Search -create / SearchPolicy CSPSearchPath
	dscl /Search -append / CSPSearchPath "/Active Directory/All Domains"
	dscl /Search/Contacts -create / SearchPolicy CSPSearchPath
	dscl /Search/Contacts -append / CSPSearchPath "/Active Directory/All Domains"
	 
	# This works in a pinch if the above code does not
	defaults write /Library/Preferences/DirectoryService/SearchNodeConfig "Search Node Custom Path Array" -array "/Active Directory/All Domains"
	defaults write /Library/Preferences/DirectoryService/SearchNodeConfig "Search Policy" -int 3
	defaults write /Library/Preferences/DirectoryService/ContactsNodeConfig "Search Node Custom Path Array" -array "/Active Directory/All Domains"
	defaults write /Library/Preferences/DirectoryService/ContactsNodeConfig "Search Policy" -int 3
	 
	plutil -convert xml1 /Library/Preferences/DirectoryService/SearchNodeConfig.plist
	
else
	echo "Not Binding to directory";
fi

