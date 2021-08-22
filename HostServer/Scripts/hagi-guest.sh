#!/bin/bash
# 


show_help() {
  ACTION=$1
  THIS=$(basename "$BASH_SOURCE")

  case "$ACTION" in


      filemap)
        sed "s/%THIS%/$THIS/g" << 'end_filemap'
%THIS% filemap

  

usage: %THIS% filemap [--path <path>] [--guest <guest>]

  --path <path>
  --guest <guest>   The guest ID.

end_filemap
      ;;
    
      join)
        sed "s/%THIS%/$THIS/g" << 'end_join'
%THIS% join

  Join the host.

usage: %THIS% join [--secret <secret>] [--guest <guest>]

  --secret <secret>
  --guest <guest>     The guest ID.

end_join
      ;;
    
      open)
        sed "s/%THIS%/$THIS/g" << 'end_open'
%THIS% open

  Opens a file on the host.

usage: %THIS% open <path> [--type <type>] [--guest <guest>]

  <path>            A url or a path on the guest.
  --type <type>
  --guest <guest>   The guest ID.

end_open
      ;;
        *)
      sed "s/%THIS%/$THIS/g" << '_help'
usage: %THIS% <action> [options]

Actions:
   filemap
   join          Join the host.
   open <path>   Opens a file on the host.

For information on each action: %THIS% <action> --help

_help

    ;;
  esac

  cat << '_help_options'
General options:

--help           Show this information.
--config <file>  Show this information.

_help_options
}



REQUEST_OPTION_NAMES=''
REQUEST_OPTION_NAMES_VALUE='path guest secret type'

OPTION_NAMES="help $REQUEST_OPTION_NAMES"
OPTION_NAMES_VALUE="$REQUEST_OPTION_NAMES_VALUE"

OPTION_NAMES_ALL="$OPTION_NAMES $OPTION_NAMES_VALUE"

## Parse the command-line options

unset PARAMS
declare -A PARAMS
EXTRA=()

while [[ $# -gt 0 ]]; do

  CURRENT="$1"
  shift

  if [[ "$CURRENT" == --* ]]; then
    # It's an option
    NAME=${CURRENT#--}

    if [[ $NAME =~ = ]]; then
      # --option=value
      VALUE="${NAME#*=}"
      NAME="${NAME%=*}"

      if ! [[ " $OPTION_NAMES_ALL " =~ ' '$NAME' ' ]]; then
        echo "error: $CURRENT is unknown"
        exit
      fi

    elif [[ " $OPTION_NAMES_VALUE " =~ ' '$NAME' ' ]]; then
      # --option value
      VALUE="$1"
      shift

      if [[ "$VALUE" == --* ]] || [ -z "$VALUE" ]; then
        echo "error: $CURRENT has no value"
        exit
      fi
    elif [[ " $OPTION_NAMES " =~ ' '$NAME' ' ]]; then
      # --option
      VALUE=1
    else
      echo "error: $CURRENT is unknown"
      exit
      VALUE=
    fi

    declare "OPTION_${NAME}=$VALUE"
    PARAMS[$NAME]="$VALUE"

  else
    # Not an option
    EXTRA+=("$CURRENT")
  fi

done

# Re-apply the non-options to the command line
set -- "${EXTRA[@]}"

COMMAND=$1
shift

if [ -n "${PARAMS[help]}" ]; then
  show_help $COMMAND
  exit
fi

FIELDS=()

add_field() {
  NAME=$1
  # types: str, bool, int
  TYPE=$2
  REQUIRED=$3
  ANON=$4

  if ! [ ${PARAMS[$NAME]+1} ]; then

    if [ "$ANON" == 1 ] && [ "$1" != "" ]; then
      PARAMS[$NAME]=$1
      shift
    else
      if [ "$REQUIRED" == 1 ]; then
        echo "$NAME is required"
        exit 1
      fi
      return
    fi
  fi

  VALUE="${PARAMS[$NAME]}"
  case $TYPE in
    bool)
      if [[ "$VALUE" = "false" ]] || [[ "$VALUE" = "0" ]] || [[ -z "$VALUE" ]]; then
        VALUE=false
      else
        VALUE=true
      fi
      ;;
    int)
      if echo -n "$VALUE" | grep -q '[^0-9]'; then
        echo "$NAME: '$VALUE' is not a number"
        exit
      fi

      if [ -z "$VALUE" ]; then
        VALUE=0
      fi

      ;;
    str)
      VALUE="\"$VALUE\""
      ;;
  esac

  FIELDS+=("\"$NAME\": $VALUE")
}

case "$COMMAND" in


    filemap)
     URL_PATH='/hagi/map'
      add_field path str 0 0
      add_field guest str 0 0
    ;;
  
    join)
     URL_PATH='/hagi/auth/join'
      add_field secret str 0 0
      add_field guest str 0 0
    ;;
  
    open)
     URL_PATH='/hagi/open'
      add_field path str 1 1
      add_field type str 0 0
      add_field guest str 0 0
    ;;
  esac

[ -z "$URL_PATH" ] && exit

DATA="{$(IFS=, ; echo "${FIELDS[*]}")}"

SCHEMA="http://"
HOST="127.0.0.1:5580"
URL="${SCHEMA}${HOST}${URL_PATH}"

RESPONSE_FILE=$(mktemp)
RESPONSE_CODE=$(
  curl --request POST "$URL" \
    --header 'Content-Type: application/json' \
    --data-raw "$DATA" \
    --silent --show-error \
    --write-out "%{response_code}" \
    --output "$RESPONSE_FILE"
)

RESULT=$?

RESPONSE=$(cat "$RESPONSE_FILE")
rm "$RESPONSE_FILE"

SUCCESS=0
if [ "$RESULT" != "0" ]; then
  echo curl failure
  exit
elif [ "$RESPONSE_CODE" != 200 ]; then
  echo "http response code $RESPONSE_CODE"
  echo "$RESPONSE"

else
  echo ok
  SUCCESS=1
fi


if [ "$SUCCESS" == 0 ]; then
  exit 1
fi

