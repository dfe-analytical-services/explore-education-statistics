*** Settings ***
Library     public-utilities.py
Resource    ./common.robot

*** Keywords ***
user checks headline summary contains
    [Arguments]    ${text}
    user waits until element is visible    xpath://*[@id="releaseHeadlines-summary"]//li[text()="${text}"]

user checks number of release updates
    [Arguments]    ${number}
    user waits until element is visible    id:releaseLastUpdates
    user waits until page contains element    css:#releaseLastUpdates li    limit=${number}

user checks release update
    [Arguments]    ${number}    ${date}    ${text}
    user waits until element contains    css:#releaseLastUpdates li:nth-of-type(${number}) time    ${date}
    user waits until element contains    css:#releaseLastUpdates li:nth-of-type(${number}) p    ${text}

user waits until details dropdown contains publication
    [Arguments]    ${details_heading}    ${publication_name}    ${wait}=3
    user waits until details contains element    ${details_heading}    xpath:.//*[text()="${publication_name}"]
    ...    wait=${wait}

user goes to release page via breadcrumb
    [Arguments]    ${publication}    ${release}
    user clicks link    ${publication}

    user checks breadcrumb count should be    3
    user checks nth breadcrumb contains    1    Home
    user checks nth breadcrumb contains    2    Find statistics and data
    user checks nth breadcrumb contains    3    ${publication}

    user waits until h1 is visible    ${publication}
    user waits until page contains title caption    ${release}
