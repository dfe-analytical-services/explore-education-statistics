*** Settings ***
Resource            ../../libs/public-common.robot
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Library             ../../libs/admin_api.py

Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Variables ***
${PUBLICATION_NAME}=        UI tests - legacy releases %{RUN_IDENTIFIER}
${DESCRIPTION}=             legacy release description
${UPDATED_DESCRIPTION}=     updated legacy release description


*** Test Cases ***
Create new publication via api
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    set suite variable    ${PUBLICATION_ID}

Validate that legacy releases do not exist
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Legacy releases
    user waits until h2 is visible    Legacy releases
    user checks page does not contain element    css:tbody[data-rfd-droppable-id="droppable"]

Create legacy release
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Legacy releases
    user waits until h2 is visible    Legacy releases
    user creates legacy release    ${DESCRIPTION}    http://test.com

Create new release via api
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    AY    2020

Create 2nd legacy release
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Legacy releases
    user waits until h2 is visible    Legacy releases
    user creates legacy release    ${DESCRIPTION}    http://test.com

Validate that two legacy releases exist in the page
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Legacy releases
    user waits until h2 is visible    Legacy releases
    sleep    10

Navigate to release in admin
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    Academic year 2020/21

Add headline text block to Content page
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Approve release
    user clicks link    Sign off
    user approves original release for immediate publication
    user waits until page contains element    testid:public-release-url
    ${PUBLIC_RELEASE_LINK}=    Get Value    xpath://*[@data-testid="public-release-url"]
    check that variable is not empty    PUBLIC_RELEASE_LINK    ${PUBLIC_RELEASE_LINK}
    Set Suite Variable    ${PUBLIC_RELEASE_LINK}

Check legacy release appears on public frontend
    user navigates to public frontend    ${PUBLIC_RELEASE_LINK}
    user opens details dropdown    View releases (2)

    ${other_releases}=    user gets details content element    View releases (2)

    user checks list has x items    css:ul    2    ${other_releases}

    ${other_release_1}=    user gets list item element    css:ul    2    ${other_releases}
    ${other_release_1_link}=    get child element    ${other_release_1}    link:${DESCRIPTION}
    user checks element attribute value should be    ${other_release_1_link}    href    http://test.com/
