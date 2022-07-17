#!/bin/sh

echo Shell scipt: Locating binary
_basedir=$(dirname $0)
[ ! -e $_basedir/Mate ] && echo Binary called \"Mate\" not found in $_basedir && exit 1
while true; do
    if [ -e Mate_new ]; then
        echo Shell script: Updating binary
        mv Mate_new Mate
    fi
    echo Shell scipt: Starting bot
    ./Mate --helper-script $@
    [ $? -eq 0 ] && exit 0
    echo Press CTRL+C within five seconds to cancel reboot
    sleep 5s
done