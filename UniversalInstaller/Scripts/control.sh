#!/bin/bash
DAEMON=mono-service
NAME=fog-service
DESC=fog-service
WORKINGDIR=/opt/fog-service/
LOCK=service.lock
FOGSERVICE=FOGService.exe
PID="`cat ${WORKINGDIR}${LOCK}`"

case "$1" in
        start)
                if [ -f ${WORKINGDIR}${LOCK} ]; then
                    myPID="$(cat ${WORKINGDIR}${LOCK})"
                    procName="$(ps -p $myPID -o comm=)"
                    toCheck="FOGService"
                    if test "${procName#*$toCheck}" != "$toCheck"
                    then
                      echo "${NAME} is running....Now Stopping"
                      kill $myPID
                    fi
                    rm ${WORKINGDIR}${LOCK}
                else
                  echo "starting ${NAME}"
                  ${DAEMON} ${WORKINGDIR}${FOGSERVICE} -d:${WORKINGDIR} -l:${WORKINGDIR}${LOCK}
                  echo "${NAME} started"
                fi
        ;;
        stop)
                if [ -f ${WORKINGDIR}${LOCK} ]; then
                    myPID="$(cat ${WORKINGDIR}${LOCK})"
                    procName="$(ps -p $myPID -o comm=)"
                    toCheck="FOGService"
                    if test "${procName#*$toCheck}" != "$toCheck"
                    then
                      echo "${NAME} is running....Now Stopping"
                      kill -15 $myPID
                    fi
                    rm ${WORKINGDIR}${LOCK}
                else
                    echo "${NAME} is not running"
                fi
        ;;
esac
exit 0
