if [ -f "~/.fog_user_service.lock" ]; then
    running_pid=`cat ~/.fog_user_service.lock`
    kill -9 $running_pid
    rm -f ~/.fog_user_service.lock
fi
