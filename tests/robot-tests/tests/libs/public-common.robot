*** Settings ***
Resource    ./common.robot
Library     public-utilities.py

*** Keywords ***
user checks headline summary contains
    [Arguments]  ${text}
    user waits until element is visible  xpath://*[@id="releaseHeadlines-summary"]//li[text()="${text}"]

user checks number of release updates
    [Arguments]  ${number}
    user waits until element is visible  id:releaseNotes
    user waits until page contains element  css:#releaseNotes li  limit=${number}

user checks release update
    [Arguments]  ${number}  ${date}  ${text}
    user waits until element contains  css:#releaseNotes li:nth-of-type(${number}) time  ${date}
    user waits until element contains  css:#releaseNotes li:nth-of-type(${number}) p     ${text}
