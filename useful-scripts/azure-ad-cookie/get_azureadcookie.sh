#!/bin/bash

pip install -q -r requirements.txt
python get_azureadcookie.py $1 $2 $3
