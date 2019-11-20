#!/bin/bash

pip3 install -r requirements.txt
python3 get_auth_tokens.py $1 $2 $3
