#!/bin/bash

show_help() {
  ACTION="$1"
  THIS=$(basename "$BASH_SOURCE")

  show_action_help "$ACTION"

  if [ -z "$ACTION" ]; then
    cat << '_help_options'
General options:

  --help             Show basic command help.
  --help <action>    Show help on a specific action.
  --help-all         Show full command help.
  --config <file>    Specify the config file (may be written to).

_help_options
  fi
}

show_action_help() {
  ACTION="$1"
  THIS=$(basename "$BASH_SOURCE")

  case "$ACTION" in
#@include:Help
  esac
}

fail() {
  echo "$*"
  exit 1
}

unset CONFIG
declare -A CONFIG

load_config() {
  if [ -f "$CONFIG_FILE" ]; then
    while read -r NAME VALUE ; do
      CONFIG[$NAME]="$VALUE"
    done < <(sed -n -r -e 's,^\s*([a-z0-9_]+)\s*=\s*(.*)$,\1 \2,pi' "$CONFIG_FILE")
  fi
}

put_config() {
  NAME=$1
  VALUE=$2
  CONFIG[$NAME]="$VALUE"
  if grep -q "^$NAME=" "$CONFIG_FILE"; then
    sed -i -r "s,^($NAME=).*,\1$VALUE," "$CONFIG_FILE"
  else
    echo "$NAME=$VALUE" >> "$CONFIG_FILE"
  fi
}


load_config

#@include:OptionNames

OPTION_NAMES="help help-all $REQUEST_OPTION_NAMES"
OPTION_NAMES_VALUE="config $REQUEST_OPTION_NAMES_VALUE"

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
        fail "error: $CURRENT is unknown"
      fi

    elif [[ " $OPTION_NAMES_VALUE " =~ ' '$NAME' ' ]]; then
      # --option value
      VALUE="$1"
      shift

      if [[ "$VALUE" == --* ]] || [ -z "$VALUE" ]; then
        fail "error: $CURRENT has no value"
      fi
    elif [[ " $OPTION_NAMES " =~ ' '$NAME' ' ]]; then
      # --option
      VALUE=1
    else
      fail "error: $CURRENT is unknown"
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

ACTION=$1
shift

if [ -n "${PARAMS[help-all]}" ]; then
  show_help $ACTION
  exit
elif [ -n "${PARAMS[help]}" ]; then
  show_help $ACTION
  exit
fi

if [ ${PARAMS[config]+1} ]; then
  CONFIG_FILE="${PARAMS[config]}"
else
  CONFIG_FILE=~/.hagi-guest
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
        fail "$NAME is required"
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
        fail "$NAME: '$VALUE' is not a number"
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

case "$ACTION" in

#@include:BuildRequest

  *)
    fail "Unknown action '$ACTION'"
    ;;

esac

[ -z "$URL_PATH" ] && exit

GUEST="${CONFIG[GUEST]}"
if [ -z "$GUEST" ] && [ "$ACTION" != join ]; then

  GUEST="$(uname -s -n | sed 's,\s,,g' )-$RANDOM"
  put_config GUEST $GUEST

  

fi

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


if [ "$RESULT" != "0" ]; then
  fail curl failure
elif [ "$RESPONSE_CODE" != 200 ]; then
  echo "http response code $RESPONSE_CODE"
  fail "$RESPONSE"
else
  echo ok
  SUCCESS=1
fi

