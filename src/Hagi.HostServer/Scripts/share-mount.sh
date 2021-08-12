#!/bin/bash
(
  echo "$*"
  set | grep -i hagi

 )   >> /tmp/sm

set -x

IFS=';' read -r SERVER DOMAIN <<< "$hagi_guest_MachineName"
URL="smb://${SERVER}/${hagi_guest_MountShare}"


show_path() {
  MOUNT_PATH=$(gio info "$URL" | sed -n -r 's,^local path:\s*(.*)$,\1,p')
  echo "MOUNT_PATH=$MOUNT_PATH"
}

case "$1" in
  --mount)

    echo -e "${hagi_ShareUser}\n${DOMAIN}\n${hagi_SharePass}\n" | gio mount "$URL"
    RESULT=$?
    show_path
    ;;

  --umount|--unmount)

    gio umount "$URL"
    RESULT=$?
    ;;

  --get-path)
    show_path

    ;;

  --auth)

    IFS='|' read -r user pass < <(zenity --password --username --title="Mount guest '${hagi_guest_GuestId}'" )
    echo "MOUNT_USER=$user"
    echo "MOUNT_PASS=$pass"
    exit

    ;;
esac


exit "$RESULT"

