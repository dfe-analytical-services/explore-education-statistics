#!/bin/bash

pip3 install -r requirements.txt
python3 get_azureadcookie.py $1 $2 $3
