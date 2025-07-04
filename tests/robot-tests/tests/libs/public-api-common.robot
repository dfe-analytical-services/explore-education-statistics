*** Settings ***
Resource    ./common.robot
Library     admin-utilities.py


*** Keywords ***
user waits until draft API data set status contains
    [Arguments]    ${expected_status}    ${retries}=10x    ${interval}=%{WAIT_SMALL}s
    wait until keyword succeeds    ${retries}    ${interval}
    ...    user checks summary list contains    Status    ${expected_status}    testid:draft-version-summary
