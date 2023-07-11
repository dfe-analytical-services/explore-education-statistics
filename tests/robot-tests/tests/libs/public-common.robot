*** Settings ***
Resource    ./common.robot
Library     public-utilities.py


*** Keywords ***
user checks headline summary contains
    [Arguments]    ${text}
    user waits until element is visible    xpath://*[@id="releaseHeadlines-summary"]//li[text()="${text}"]

user checks number of release updates
    [Arguments]    ${count}
    user waits until page contains element    id:releaseLastUpdates
    user waits until page contains element    css:#releaseLastUpdates li    limit=${count}

user checks release update
    [Arguments]    ${number}    ${date}    ${text}
    user waits until element contains    css:#releaseLastUpdates li:nth-of-type(${number}) time    ${date}
    user waits until element contains    css:#releaseLastUpdates li:nth-of-type(${number}) p    ${text}

user checks publication is on find statistics page
    [Arguments]    ${publication_name}
    user navigates to find statistics page on public frontend
    user waits until page contains link    ${publication_name}

user waits until details dropdown contains publication
    [Arguments]    ${details_heading}    ${publication_name}    ${publication_type}=National and official statistics
    ${details}=    user gets details content element    ${details_heading}
    user checks publication appears under correct publication type heading
    ...    ${details}
    ...    ${publication_type}
    ...    ${publication_name}

user checks publication appears under correct publication type heading
    [Arguments]    ${parent}    ${publication_type}    ${publication_name}
    ${publication_type}=    get child element
    ...    ${parent}
    ...    xpath:.//div[@data-testid="publication-type" and descendant::h3[text()="${publication_type}"]]
    ${publications}=    get child element    ${publication_type}    css:ul
    user checks element should contain    ${publications}    ${publication_name}

user goes to release page via breadcrumb
    [Arguments]    ${publication}    ${release}
    user clicks link    ${publication}

    user checks breadcrumb count should be    3
    user checks nth breadcrumb contains    1    Home
    user checks nth breadcrumb contains    2    Find statistics and data
    user checks nth breadcrumb contains    3    ${publication}

    user waits until h1 is visible    ${publication}
    user waits until page contains title caption    ${release}

user navigates to public methodologies page
    environment variable should be set    PUBLIC_URL
    user navigates to public frontend    %{PUBLIC_URL}/methodology
    user waits until h1 is visible    Methodologies

user checks methodology note
    [Arguments]    ${number}    ${displayDate}    ${content}
    user waits until element contains    css:#methodologyNotes li:nth-of-type(${number}) time    ${displayDate}
    user waits until element contains    css:#methodologyNotes li:nth-of-type(${number}) p    ${content}
