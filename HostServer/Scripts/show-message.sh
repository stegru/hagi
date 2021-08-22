#!/bin/bash

OPTION_NAMES="dialog"
OPTION_NAMES_VALUE="message title"

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

got_command() {
  command -v "$1" >/dev/null
}


if got_command zenity; then
  ARGS=
  if [ ${PARAMS[dialog]+1} ]; then

  fi
  zenity
fi

${PARAMS[MESSAGE]}