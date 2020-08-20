*** Settings ***
Resource    ./common.robot
Library     public-utilities.py

*** Keywords ***
user checks key stat tile contents
    [Arguments]  ${tile_title}  ${tile_value}  ${summary
    }
