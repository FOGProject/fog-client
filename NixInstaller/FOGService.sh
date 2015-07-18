#!/bin/bash

#####################################
####   Variables
####################################
depends="unzip curl pkill pgrep wall";
mono="mono-complete"
systemctl="no"
fogServer="$1"
useHTTPS="$2"
fogTray="$3"

installFOGService()
{
	curl -o /tmp/core.sh $fogServer/client/core.sh
	chmod +x /tmp/core.sh 
	/tmp/./core.sh $fogServer $fogTray $useHTTPS
	rm /tmp/vore.sh
	installInitScript
}


installInitScript()
{
	initScript="#!/bin/bash

### BEGIN INIT INFO
# Provides:          fog-service.sh
# Required-Start:    \$local_fs \$syslog $remote_fs
# Required-Stop:     \$local_fs \$syslog $remote_fs
# Default-Start:     2 3 4 5
# Default-Stop:      0 1 6
# Short-Description: Start fog service
### END INIT INFO

PATH=/usr/local/sbin:/usr/local/bin:/sbin:/bin:/usr/sbin:/usr/bin
DAEMON=mono-service
NAME=fog-service
DESC=fog-service

WORKINGDIR=/opt/fog-service/
LOCK=service.lock
FOGSERVICE=FOGService.exe
PID=\"`cat \${WORKINGDIR}${LOCK}`\"

case \"\$1\" in
        start)
                if [ -z \"\${PID}\" ]; then
                        echo \"starting \${NAME}\"
                        \${DAEMON} \${WORKINGDIR}\${FOGSERVICE} -d:\${WORKINGDIR} -l:\${WORKINGDIR}${LOCK}
                        echo \"\${NAME} started\"
                else
                        echo \"\${NAME} is running\"
                fi
        ;;
        stop)
                if [ -n \"\${PID}\" ]; then
                        \${DAEMON} kill ${PID}
                        echo \"\${NAME} stopped\"
                else
                        echo \"\${NAME} is not running\"
                fi
        ;;
esac

exit 0";
echo $initScript > $initdPath/FOGService
chmod 755 $initdPath/FOGService
if [ "$systemctl" == "yes" ]; then
	systemctl enable FOGService >/dev/null 2>&1;
else
	sysv-rc-conf FOGService on >/dev/null 2>&1;
fi


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

getOSVersion()
{
	if [ -f "/etc/os-release" ]; then
		linuxReleaseName=`sed -n 's/^NAME=\(.*\)/\1/p' /etc/os-release | tr -d '"'`;
    	OSVersion=`sed -n 's/^VERSION_ID=\([^.]*\).*/\1/p' /etc/os-release | tr -d '"'`;
	else
		linuxReleaseName=`cat /etc/*release* 2>/devnull | head -n1 | awk '{print $1}'`;
	fi
	if [ -z "$OSVersion" ]; then
    	if [[ "$linuxReleaseName" == +(*'Debian'*|*'Ubuntu'*) ]]; then
        	apt-get install lsb_release >/dev/null 2>&1;
        	OSVersion=`lsb_release -r| awk -F'[^0-9]*' /^[Rr]elease\([^.]*\).*/'{print $2}'`;
    	elif [[ "$linuxReleaseName" == +(*'CentOS'*|*'Redhat'*|*'Fedora'*) ]]; then
        OSVersion=`awk -F'[^0-9]*' /[Rr]elease*\([^.]*\).*/'{print $2}' /etc/*release* | head -n1`
    	fi
	fi
	if [[ "$OSVersion" -ge 7 && "$linuxReleaseName" == +(*'CentOS'*|*'Redhat'*) ]] || [[ "$OSVersion" -ge 15 && "$linuxReleaseName" == +(*'Fedora'*|*'Ubuntu'*) ]] || [[ "$OSVersion" -ge 8 && "$linuxReleaseName" == +(*'Debian'*) ]]; then
    	systemctl="yes";
	fi
	if [ "$OSVersion" -ge 15 -a "$linuxReleaseName" == "Ubuntu" ] || [ "$OSVER" -ge 8 -a "$linuxReleaseName" == "Debian" ]; then
		initdpath="/lib/systemd/system";
	else
		initdpath="/etc/init.d";
	fi
	strSuggestedOS="";
	if [ "`echo $linuxReleaseName | grep -Ei "Fedora|Redhat|CentOS|Mageia"`" != "" ]; then
		strSuggestedOS="1";
	fi
	if [ "`echo $linuxReleaseName | grep -Ei "Ubuntu|Debian"`" != "" ]; then
		strSuggestedOS="2";
	fi
	if [ "`echo $linuxReleaseName | grep -Ei "Arch"`" != "" ]; then
		strSuggestedOS="3";
	fi


}

getUserInput()
{
	echo -n Enter in your FOG Server address:
	read fogServer
	echo -n Do you want to use HTTPS for communication?[y/n]:
	read useHTTPS
	echo -n Do you want to enable the FOGTray?[y/n]:
	read fogTray
	echo -n Are you sure you want to install the FOGService?[y/n]:
	read doInstall
	if [ "$doInstall" == "y" ]; then
		echo "Starting installation"
	else
		exit 1;
	fi
}

installDeps()
{
	case "$strSuggestedOS" in
        "1")
            echo "  Staring Redhat / CentOS Installation."
            yum install -y $depends >/dev/null 2>&1;
            rpm --import "http://keyserver.ubuntu.com/pks/lookup?op=get&search=0x3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF" >/dev/null 2>&1;
				yum-config-manager --add-repo http://download.mono-project.com/repo/centos/ >/dev/null 2>&1;
				yum install -y $mono >/dev/null 2>&1;
        ;;
        "2")
            echo "  Starting Debian / Ubuntu / Kubuntu / Edubuntu Installtion."
            apt-get install -y $depends >/dev/null 2>&1;
            apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF >/dev/null 2>&1;
				echo "deb http://download.mono-project.com/repo/debian wheezy main" | tee /etc/apt/sources.list.d/mono-xamarin.list >/dev/null 2>&1;
				apt-get update >/dev/null 2>&1;
				apt-get install -y $mono >/dev/null 2>&1;
        ;;
        "3")
            echo "  Starting Arch Installation.";
           	pacman -Syu --noconfirm >/dev/null 2>&1;
				sleep 1;
				pacman -Q $depends >/dev/null 2>&1;
				packman -Q mono >/dev/null 2>&1;
        ;;
        *)
            echo "  Sorry OS Version not supported."
            echo "";
            exit 1;
        ;;
    esac

}


#####################################
## Main Entry Point
#####################################

warnRoot
if [ "$fpgServer" == "" ] || [ "$useHTTPS" == "" ] || [ "$fogTray" == "" ]; then
	getUserInput
fi
getOSVersion
installDeps
installFOGService

exit 1;

