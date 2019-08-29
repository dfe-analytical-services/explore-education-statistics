#!/bin/bash

pip install -r requirements.txt
python get_azureadcookie.py $1 $2 $3
