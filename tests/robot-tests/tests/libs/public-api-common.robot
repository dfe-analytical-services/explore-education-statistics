*** Settings ***
Resource    ./common.robot
Library     admin-utilities.py


*** Keywords ***
user checks checklist errors contains
    [Arguments]    ${text}
    user waits until page contains testid    releaseChecklist-errors
    user waits until element contains    testid:releaseChecklist-errors    ${text}

user checks checklist errors contains link
    [Arguments]    ${text}
    user waits until page contains testid    releaseChecklist-errors
    user waits until parent contains element    testid:releaseChecklist-errors    link:${text}

user checks checklist errors contains either link
    [Arguments]    ${text1}    ${text2}
    user waits until page contains testid    releaseChecklist-errors
    ${status}=    Run Keyword And Return Status    Wait Until Keyword Succeeds    10s    1s    Check Either Link Exists
    ...    ${text1}    ${text2}

    IF    ${status} == False
        Fail    Neither of the expected links (${text1}, ${text2}) was found. Failing fast as required.
    END
    Log    One of the expected links (${text1}, ${text2}) is present.

check either link exists
    [Arguments]    ${text1}    ${text2}
    ${condition1}=    Run Keyword And Return Status    user waits until parent contains element without retries
    ...    testid:releaseChecklist-errors    link:${text1}    timeout=5s
    IF    ${condition1}
        Set Test Variable    ${link_found}    True
    END

    ${condition2}=    Run Keyword And Return Status    user waits until parent contains element without retries
    ...    testid:releaseChecklist-errors    link:${text2}    timeout=5s
    IF    ${condition1} == False
        Set Test Variable    ${link_found}    ${condition2}
    END

    ${link_found}=    Evaluate    ${condition1} or ${condition2}
    IF    ${link_found} == False
        Log    Neither link '${text1}' nor '${text2}' was found after checking both. Continuing to check...
    END
    [Return]    ${link_found}

user waits until draft API data set status contains
    [Arguments]    ${expected_status}    ${retries}=10x    ${interval}=%{WAIT_SMALL}s
    wait until keyword succeeds    ${retries}    ${interval}
    ...    user checks summary list contains    Status    ${expected_status}    testid:draft-version-summary
