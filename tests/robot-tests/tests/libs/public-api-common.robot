*** Settings ***
Resource    ./common.robot
Library     admin-utilities.py


*** Keywords ***
user edits release status
    user clicks link    Sign off
    user waits until h2 is visible    Sign off    %{WAIT_SMALL}

    user clicks button    Edit release status
    user waits until h2 is visible    Edit release status    %{WAIT_SMALL}

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
    ${status}=    Run Keyword And Return Status    Wait Until Keyword Succeeds    10s    1s    Check Either Link Exists    ${text1}    ${text2}

    Run Keyword If    ${status} == False    Fail    Neither of the expected links (${text1}, ${text2}) was found. Failing fast as required.
    Log    One of the expected links (${text1}, ${text2}) is present.

check either link exists
    [Arguments]    ${text1}    ${text2}
    ${condition1}=    Run Keyword And Return Status    user waits until parent contains element without retries    testid:releaseChecklist-errors    link:${text1}    timeout=5s
    Run Keyword If    ${condition1}    Set Test Variable    ${link_found}    True

    ${condition2}=    Run Keyword And Return Status    user waits until parent contains element without retries    testid:releaseChecklist-errors    link:${text2}    timeout=5s
    Run Keyword If    ${condition1} == False    Set Test Variable    ${link_found}    ${condition2}

    ${link_found}=    Evaluate    ${condition1} or ${condition2}
    Run Keyword If    ${link_found} == False    Log    Neither link '${text1}' nor '${text2}' was found after checking both. Continuing to check...
    [Return]    ${link_found}

verify status of API Datasets
    [Arguments]    ${expected_status}
    user waits for caches to expire
    ${status_value}=    get text    xpath:(//div[@data-testid="Status"]//dd[@data-testid="Status-value"]//strong)[2]
    should be equal as strings    ${status_value}    ${expected_status}

user checks status in Draft version table
    [Arguments]    ${text}    ${expected_status}
    user waits for caches to expire
    ${status_value}=    get text    xpath:(//div[@data-testid="Status"]//dd[@data-testid="${text}"]//strong)[2]
    should be equal as strings    ${status_value}    ${expected_status}

user checks row headings within the api data set section
    [Arguments]    ${text}        ${parent}=#dataSetDetails
    user waits until page contains element    css:${PARENT} [data-testid="${text}"] > dt

user gets accordion header button element
    [Arguments]    ${heading_text}    ${parent}=css:#dataSetDetails
    ${button}=    get child element    ${parent}    css:.[data-testid="Release"] > dt
    [Return]    ${button}

user checks contents inside the cell value
    [Arguments]      ${expected_text}     ${locator}
     ${status_value}=    get text    ${locator}
    should be equal as strings    ${status_value}    ${expected_text}

user checks table headings for Draft version details table
    ${elements}=  create list  Version  Status  Release  Data set file  Geographic levels  Time periods  Indicators  Filters  Actions

    FOR  ${element}  IN  @{elements}
        ${xpath}=  set variable  //dl[@data-testid="draft-version-summary"]//div[@data-testid="${element}"]/dt
        element should be visible  xpath=${xpath}
    END
