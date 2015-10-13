#!/bin/bash

#####################################
####   Variables
####################################
url="REPLACE";
depends=("unzip" "curl" "pkill" "pgrep" "wall");
toInstall=()
strSuggestedOS=""
needsMono=false


getOSVersion(){
	if [ -f "/etc/os-release" ]; then
		linuxReleaseName=`sed -n 's/^NAME=\(.*\)/\1/p' /etc/os-release | tr -d '"'`;
    	OSVersion=`sed -n 's/^VERSION_ID=\([^.]*\).*/\1/p' /etc/os-release | tr -d '"'`;
	else
		linuxReleaseName=`cat /etc/*release* 2>/devnull | head -n1 | awk '{print $1}'`;
	fi
	strSuggestedOS="";
	if [ "`echo $linuxReleaseName | grep -Ei "Fedora|Redhat|CentOS|Mageia"`" != "" ]; then
		strSuggestedOS="1";
    echo "  Staring Redhat / CentOS Installation."
	fi
	if [ "`echo $linuxReleaseName | grep -Ei "Ubuntu|Debian"`" != "" ]; then
		strSuggestedOS="2";
    echo "  Starting Debian / Ubuntu / Kubuntu / Edubuntu Installtion."
	fi
	if [ "`echo $linuxReleaseName | grep -Ei "Arch|Antergos"`" != "" ]; then
		strSuggestedOS="3";
    echo "  Starting Arch Installation.";
	fi
  if [ "$strSuggestedOS" == "" ]; then
    echo "Sorry your OS Version not supported...."
    echo "The installer will now exit";
    exit 1;
  fi

}

checkDeps(){
  didPass=true
  echo "Checking that System meets requirements...."
  for i in "${depends[@]}"
  do
    	echo -n "Checking for $i...."
      didFind="$(which $i)"
      if ["$didFind" == "" ]; then
          echo "Failed!!"
          toInstall+="$i"
          didPass=false
      else
          echo "Passed!!"
      fi
  done
  echo -n "Checking for the Mono Framework...."
  hasMono="$(which mono)"
  if [ "$hasMono" == "" ]; then
      needsMono=true
      echo "Failed!!"
  else
      echo "Passed!!"
  fi
  echo -n "Checking for the Mono Service...."
  hasMono="$(which mono-service)"
  if [ "$hasMono" == "" ]; then
      needsMono=true
      echo "Failed!!"
  else
      echo "Passed!!"
  fi
  echo ""
  if [ "$didPass" = false ] ; then
    if [ "$strSuggestedOS" == "OSX" ] ; then
      installMacDeps
    else
      installLinuxDeps
    fi
  fi
}

displayBanner() {
    echo
    echo "       ..#######:.    ..,#,..     .::##::.   ";
    echo "  .:######          .:;####:......;#;..      ";
    echo "  ...##...        ...##;,;##::::.##...       ";
    echo "     ,#          ...##.....##:::##     ..::  ";
    echo "     ##    .::###,,##.   . ##.::#.:######::. ";
    echo "  ...##:::###::....#. ..  .#...#. #...#:::.  ";
    echo "  ..:####:..    ..##......##::##  ..  #      ";
    echo "      #  .      ...##:,;##;:::#: ... ##..    ";
    echo "     .#  .       .:;####;::::.##:::;#:..     ";
    echo "      #                     ..:;###..        ";
    echo
    echo "  ###########################################";
    echo "  #     FOG                                 #";
    echo "  #     Free Computer Imaging Solution      #";
    echo "  #                                         #";
    echo "  #     http://www.fogproject.org/          #";
    echo "  #                                         #";
    echo "  #     Credits:                            #";
    echo "  #     http://fogproject.org/Credits       #";
    echo "  #     GNU GPL Version 3                   #";
    echo "  ###########################################";
    echo "  #           FOG SERVICE INSTALLER         #";
    echo "  #                 LINUX/OS X              #";
		echo ""
		echo ""
}

warnRoot() {
    currentuser=`whoami`;
    if [ "$currentuser" != "root" ]; then
        echo
        echo "  This installation script should be run as"
        echo "  user \"root\".  You are currenly running ";
        echo "  as $currentuser.  "
        echo
        echo -n "  Do you wish to continue? [N] "
        read ignoreroot;
        if [ "$ignoreroot" = "" ]; then
            ignoreroot="N";
        else
            case "$ignoreroot" in
				[yY]*)
                    ignoreroot="Y";
                ;;
                [nN]*)
                    ignoreroot="N";
                ;;
                *)
                    ignoreroot="N";
                ;;
            esac
        fi
        if [ "$ignoreroot" = "N" ]; then
            echo " Exiting...";
            echo
            exit 1;
        fi
    fi
}

installMacDeps(){
  if [ "$needsMono" = true ]; then
    echo "You need to install the Mono Framework to continue.
    We will now open a web browser for you to download Mono"
    echo ""
    echo "When you are Finished please conitnue"
    echo ""
    echo ""
    read -n1 -r -p "Press space to continue..." key
    if [ "$key" = ' ' ]; then
        keyPress=""
    fi
  fi
}

conitnueInstall(){
	echo -n "Downloading FOG Client Universal Installer...."
  curl -o /tmp/fog.exe http://$URL/client/UniversalInstaller.exe >/dev/null 2>&1;
	echo "Done!"
  mono /tmp/fog.exe
  rm /tmp/fog.exe
}


installLinuxDeps(){
	case "$strSuggestedOS" in
        "1")
            echo -n "Installing dependecies...."
            yum install -y $toInstall >/dev/null 2>&1;
            echo "OK"
            if [ "$needsMono" = true ]; then
              echo -n "Adding Mono Keys and frameworks...."
              rpm --import "http://keyserver.ubuntu.com/pks/lookup?op=get&search=0x3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF" >/dev/null 2>&1;
				      yum-config-manager --add-repo http://download.mono-project.com/repo/centos/ >/dev/null 2>&1;
				      yum install -y $mono >/dev/null 2>&1;
              echo "OK"
            fi
        ;;
        "2")
            echo -n "Installing dependecies...."
            apt-get install -y $toInstall >/dev/null 2>&1;
            echo "OK"
            if [ "$needsMono" = true ]; then
              echo -n "Adding Mono Keys and frameworks...."
              apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF >/dev/null 2>&1;
				      echo "deb http://download.mono-project.com/repo/debian wheezy main" | tee /etc/apt/sources.list.d/mono-xamarin.list >/dev/null 2>&1;
				      apt-get update >/dev/null 2>&1;
				      apt-get install -y $mono >/dev/null 2>&1;
              echo "OK"
            fi
        ;;
        "3")
            echo -n "Installing dependecies...."
           	pacman -Syu --noconfirm >/dev/null 2>&1;
				    sleep 1;
				    pacman -Q $depends >/dev/null 2>&1;
            echo "OK"
            if [ "$needsMono" = true ]; then
              echo -n "Adding Mono Keys and frameworks...."
				      pacman -S mono --noconfirm >/dev/null 2>&1;
              echo "OK"
            fi
        ;;
        *)

        ;;
    esac
    echo "Finished Trying to Install Dependecies......"
    echo "Now Confirming that the required software is Installed"
    declare -a arr=("unzip" "curl" "pkill" "pgrep" "wall" "su" "mono" "mono-service")
    didFail="false"
    for i in "${arr[@]}"
    do
        echo -n "Checking for installation of $i....."
        didHave="$(which $i)";
        if [ "$didHave" != "" ]; then
            echo "OK!"
        else
            echo "Failed!"
            didFail="true"
        fi
    done
    if [ "$didFail" == "true" ]; then
        echo "Sorry but the required software was not found on this computer!!"
        exit 0
    fi

}

#####################################
## Main Entry Point
#####################################
displayBanner
warnRoot
isOSX="$(uname)"
if [ "$isOSX" == "Darwin" ]; then
  strSuggestedOS="OSX"
	echo "Detect Mac OS X Operating System"
else
  getOSVersion >/dev/null 2>&1
fi
checkDeps
conitnueInstall
exit 1;
