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
    user create test release via api    ${PUBLICATION_ID}    AY    2046

Publish release
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    Academic Year 2046/47 (not Live)
    user approves release for immediate publication

Navigate to publication on admin dashboard
    ${accordion}=    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Manage publication    ${accordion}
    user waits until page contains title    Manage publication

Update publication details
    user enters text into element    label:Publication title    ${PUBLICATION_NAME_UPDATED}
    user enters text into element    label:Team name    Team name updated
    user enters text into element    label:Team email address    email_updated@test.com
    user enters text into element    label:Contact name    Contact name updated
    user enters text into element    label:Contact telephone number    4321 4321

    user clicks button    Save publication
    user clicks button    Confirm
    user waits until page does not contain button    Confirm

Check publication is updated on dashboard
    ${accordion}=    user navigates to publication page from dashboard    ${PUBLICATION_NAME_UPDATED}
    user waits until page contains element    testid:Team name for ${PUBLICATION_NAME_UPDATED}
    user checks element contains child element    ${accordion}
    ...    xpath://*[@data-testid="Team name for ${PUBLICATION_NAME_UPDATED}" and text()="Team name updated"]
    user checks element contains child element    ${accordion}
    ...    xpath://*[@data-testid="Team email for ${PUBLICATION_NAME_UPDATED}" and text()="email_updated@test.com"]
    user checks element contains child element    ${accordion}
    ...    xpath://*[@data-testid="Contact name for ${PUBLICATION_NAME_UPDATED}" and text()="Contact name updated"]
    user checks element contains child element    ${accordion}
    ...    xpath://*[@data-testid="Contact phone number for ${PUBLICATION_NAME_UPDATED}" and text()="4321 4321"]

Go to public release page
    user checks publication is on find statistics page    ${PUBLICATION_NAME_UPDATED}
    user clicks element    testid:View stats link for ${PUBLICATION_NAME_UPDATED}
    user waits until h1 is visible    ${PUBLICATION_NAME_UPDATED}    %{WAIT_MEDIUM}

Validate publication details are updated on public page
    ${section}=    user opens accordion section    Contact us    css:#help-and-support
    user checks element contains    ${section}    Team name updated
    user checks element contains    ${section}    email_updated@test.com
    user checks element contains    ${section}    Contact name updated
    user checks element contains    ${section}    4321 4321
