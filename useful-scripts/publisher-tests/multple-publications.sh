#!/bin/bash

echo "How many publications do you want to release ? "
read -r choice 
echo "publishing ${choice} publications to EES"
npm run build

for ((i = 0; i < choice; i++)); do 
  npm run start 
done
