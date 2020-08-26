*** Settings ***
Resource    ./common.robot
Library     public-utilities.py

*** Keywords ***
user checks key stat contents
    [Arguments]   ${tile}  ${title}  ${value}  ${summary}
    user waits until element is visible  css:[data-testid="keyStat"]:nth-of-type(${tile}) [data-testid="keyStat-title"]  90
    user checks element contains  css:[data-testid="keyStat"]:nth-of-type(${tile}) [data-testid="keyStat-title"]  ${title}
    user checks element contains  css:[data-testid="keyStat"]:nth-of-type(${tile}) [data-testid="keyStat-value"]  ${value}
    user checks element contains  css:[data-testid="keyStat"]:nth-of-type(${tile}) [data-testid="keyStat-summary"]  ${summary}

user checks headline summary contains
    [Arguments]  ${text}
    user waits until element is visible  xpath://*[@id="releaseHeadlines-summary"]//li[text()="${text}"]

user checks number of release updates
    [Arguments]  ${number}
    user waits until element is visible  id:releaseNotes
    user waits until page contains element  css:#releaseNotes li  limit=${number}

user checks release update
    [Arguments]  ${number}  ${date}  ${text}
    user checks element contains  css:#releaseNotes li:nth-of-type(${number}) time  ${date}
    user checks element contains  css:#releaseNotes li:nth-of-type(${number}) p     ${text}
