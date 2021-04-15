#!/bin/bash

# Usage: 
# ./multiple-import-publication.sh -n <num_of_imports> <importer or publisher>
# -n = number of subjects to upload 
# last option = test the importer or the publisher

# examples: 
# ./multiple-importer-publication.sh -n 5 importer 
# ./multiple-importer-publication.sh -n 2 publisher 

numOfSubjects=1
while [[ $# -gt 1 ]]; do
  key="$1"
  case "$key" in
    -n|--num)
    shift
    numOfSubject="$1"
    ;;
    *)
    echo "Unknown option '$key'"
    ;;
  esac
  shift
done
toRun="$1"
if [ "$toRun" = "importer" ]; then
    npm run build
    echo "Uploading ${numOfSubjects} imports to EES"
    for ((i = 0; i < ${numOfSubjects}; i++)); do 
      npm run test:importer 
    done
elif [ "$toRun" = "publisher" ]; then
    npm run build
    echo "Uploading ${numOfSubjects} publications to EES"
    for ((i = 0; i < ${numOfSubjects}; i++)); do 
      npm run test:publisher
    done 
else
echo "Invalid option. Must be 'importer' or 'publisher'"
fi
