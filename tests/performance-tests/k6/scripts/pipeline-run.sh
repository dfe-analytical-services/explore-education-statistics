#!/bin/bash 

sudo apt-get install wget -y 
wget https://github.com/k6io/k6/releases/download/v0.32.0/k6-v0.32.0-linux-amd64.deb
sudo dpkg -i k6-*.deb
