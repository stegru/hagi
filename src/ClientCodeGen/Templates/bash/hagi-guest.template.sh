#!/bin/bash

CONFIG_FILE=~/.config/hagi-guest/guest.conf

# Output general help text
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

_help_options
  fi
}

# Output help for an action
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

status() {
  printf "%s\n" "$*" >&2
}

unset CONFIG
declare -A CONFIG

# Loads the config file into $CONFIG
load_config() {
  if [ -f "$CONFIG_FILE" ]; then
    while read -r NAME VALUE ; do
      NAME=$(echo "$NAME" | tr '[:upper:]' '[:lower:]')
      CONFIG[$NAME]="$VALUE"
    done < <(sed -n -r -e 's,^\s*([a-z0-9_]+)\s*=\s*(.*)$,\1 \2,pi' "$CONFIG_FILE")
  fi
}

# Writes to the config file
put_config() {
  NAME=$1
  VALUE=$2
  CONFIG[$NAME]="$VALUE"

  mkdir -p "$(dirname "$CONFIG_FILE")"

  if grep -q "^$NAME=" "$CONFIG_FILE"; then
    sed -i -r "s,^($NAME=).*,\1$VALUE," "$CONFIG_FILE"
  else
    echo "$NAME=$VALUE" >> "$CONFIG_FILE"
  fi
}

#@include:OptionNames

# Options
OPTION_NAMES="help help-all $REQUEST_OPTION_NAMES"
# Options with values
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

# Show some help
if [ -n "${PARAMS[help-all]}" ]; then
  show_help ALL
  exit
elif [ -z "$ACTION" ] || [ "$ACTION" == "help" ] || [ -n "${PARAMS[help]}" ]; then
  show_help "$ACTION"
  exit
fi


load_config


FIELDS=()
# Add a field for the request
add_field() {
  NAME=$1
  # types: str, bool, int
  TYPE=$2
  # 1 if the field is required
  REQUIRED=$3
  # 1 if value can be pulled from $1
  ANON=$4

  # Check if the value is defined
  if ! [ ${PARAMS[$NAME]+1} ]; then

    if [ "$ANON" == 1 ] && [ "$1" != "" ]; then
      # Take it from the next argument
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

  # "JSON.stringify" the value
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

# Build the request payload
case "$ACTION" in

#@include:BuildRequest

  *)
    fail "Unknown action '$ACTION'"
    ;;

esac

[ -z "$URL_PATH" ] && exit

# Gets a JSON field value
get_json() {
  JSON=$1
  FIELD=$2
  echo "$JSON" | python3 -c "import sys, json; print(json.load(sys.stdin)['$FIELD'])"
}

# Join a host
join_host() {
  if [ -z "$GUEST" ]; then
    GUEST="$(uname -s -n | sed -r 's,(\S+)\s+(\S+),\2-\1,g' )-$RANDOM"
    put_config guest $GUEST
  fi

  $0 join --guest="$GUEST" --config="$CONFIG_FILE" || fail "Unable to join"

  load_config
}

jq=$(command -v jq || command -v cat)

RETRY=1

# Perform the request
while true; do

  # Make the json data
  DATA="{$(IFS=, ; echo "${FIELDS[*]}")}"

  GUEST="${CONFIG[guest]}"
  SECRET="${CONFIG[secret]}"

  SCHEMA="http://"
  HOST="127.0.0.1:5580"
  URL="${SCHEMA}${HOST}${URL_PATH}"

  RESPONSE_FILE=$(mktemp)
  RESPONSE_CODE=$(
    curl --request POST "$URL" \
      --header "X-Guest: $GUEST" \
      --header "X-Secret: $SECRET" \
      --header 'Content-Type: application/json' \
      --data-raw "$DATA" \
      --silent --show-error \
      --write-out "%{response_code}" \
      --output "$RESPONSE_FILE"
  )

  RESULT=$?

  RESPONSE=$(cat "$RESPONSE_FILE")
  rm "$RESPONSE_FILE"

  echo "$RESPONSE" | $jq

  if [ "$RESULT" != "0" ]; then
    # curl did not work
    fail curl failure

  elif [ "$RESPONSE_CODE" == 401 ] && [ "$ACTION" != "join" ] && [ "$RETRY" = 1 ]; then
    # Unauthorized - try to join the host.
    RETRY=0
    status Unauthorized. Trying to join the host...
    join_host

    status Retrying original request...
    continue

  elif [ "$RESPONSE_CODE" != 200 ]; then
    # Not a good response
    status "http response code $RESPONSE_CODE"
    fail "$RESPONSE"

  elif [ "$ACTION" == "join" ]; then
    # For joining, store the secret in the config
    SECRET=$(get_json "$RESPONSE" "guestSecret")
    put_config secret "$SECRET"
  fi

  break
done

status ok