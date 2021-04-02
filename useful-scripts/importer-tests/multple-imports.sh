#!/bin/bash

echo "How many subjects do you want to upload ? "
read -r choice 


echo "Uploading ${choice} imports to EES"
npm run build

for ((i = 0; i < choice; i++)); do 
  npm run start 
done 
