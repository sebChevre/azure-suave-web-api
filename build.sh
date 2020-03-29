#!/usr/bin/env bash

PREFIX="./"

if test "$OS" = "Windows_NT"
then
  EXE=""
  FLAGS=""
else
  EXE="mono"
  FLAGS="-d:MONO"
fi

if [ ! -f packages/FAKE/tools/FAKE.exe ]; then
    ${EXE} ${PREFIX}/.paket/paket.bootstrapper.exe
    exit_code=$?
    if [ $exit_code -ne 0 ]; then
        exit $exit_code
    fi
fi
${EXE} ${PREFIX}/.paket/paket.exe restore
exit_code=$?
if [ $exit_code -ne 0 ]; then
	exit $exit_code
fi

${EXE} ${PREFIX}/packages/FAKE/tools/FAKE.exe $@ --fsiargs ${FLAGS} build.fsx 
