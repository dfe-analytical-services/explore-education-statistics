*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/public-common.robot
Resource            ../../libs/admin-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${PUBLICATION_NAME}=            UI tests - publish publication update %{RUN_IDENTIFIER}
${PUBLICATION_NAME_UPDATED}=    ${PUBLICATION_NAME} updated


*** Test Cases ***
Create Publication as bau1
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    AY    2046

Publish release
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    Academic year 2046/47
    user approves release for immediate publication

Update publication details
    user navigates to details on publication page    ${PUBLICATION_NAME}

    user clicks button    Edit publication details
    user waits until page contains element    label:Publication title

    user enters text into element    label:Publication title    ${PUBLICATION_NAME_UPDATED}
    user clicks button    Update publication details
    ${modal}=    user waits until modal is visible    Confirm publication changes
    user clicks button    Confirm    ${modal}
    user waits until modal is not visible    Confirm publication changes

    user checks summary list contains    Publication title    ${PUBLICATION_NAME_UPDATED}
    user checks summary list contains    Publication summary    ${PUBLICATION_NAME} summary
    user checks summary list contains    Theme    %{TEST_THEME_NAME}
    user checks summary list contains    Topic    %{TEST_TOPIC_NAME}
    user checks summary list contains    Superseding publication    This publication is not archived

Update publication contact
    user clicks link    Contact
    user waits until h2 is visible    Contact for this publication

    user clicks button    Edit contact details
    user waits until page contains element    label:Team name

    user enters text into element    label:Team name    Team name updated
    user enters text into element    label:Team email    email_updated@test.com
    user enters text into element    label:Contact name    Contact name updated
    user enters text into element    label:Contact telephone    4321 4321

    user clicks button    Update contact details
    ${modal}=    user waits until modal is visible    Confirm contact changes
    user clicks button    Confirm    ${modal}
    user waits until modal is not visible    Confirm contact changes

    user checks summary list contains    Team name    Team name updated
    user checks summary list contains    Team email    email_updated@test.com
    user checks summary list contains    Contact name    Contact name updated
    user checks summary list contains    Contact telephone    4321 4321

Check publication is updated on dashboard
    user navigates to admin dashboard if needed    %{ADMIN_URL}
    user waits until h1 is visible    Dashboard
    user selects dashboard theme and topic if possible

    user waits until page contains link    ${PUBLICATION_NAME_UPDATED}

Go to public release page
    user checks publication is on find statistics page    ${PUBLICATION_NAME_UPDATED}
    user clicks link    ${PUBLICATION_NAME_UPDATED}
    user waits until h1 is visible    ${PUBLICATION_NAME_UPDATED}    %{WAIT_MEDIUM}

Validate publication details are updated on public page
    ${section}=    user opens accordion section    Contact us    css:#help-and-support-accordion
    user checks element contains    ${section}    Team name updated
    user checks element contains    ${section}    email_updated@test.com
    user checks element contains    ${section}    Contact name updated
    user checks element contains    ${section}    4321 4321
