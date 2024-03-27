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
${RELEASE_NAME}=                        Academic year Q1 2022/23
${PUBLICATION_NAME}=                    UI-tests-legacy-releases %{RUN_IDENTIFIER}
${PUBLIC_PUBLICATION_URL_ENDING}=       /find-statistics/${PUBLICATION_NAME}
${DESCRIPTION}=                         legacy release description
${UPDATED_DESCRIPTION}=                 updated legacy release description


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
    ${PUBLIC_RELEASE_LINK}=    get value    xpath://*[@data-testid="public-release-url"]
    check that variable is not empty    PUBLIC_RELEASE_LINK    ${PUBLIC_RELEASE_LINK}
    set suite variable    ${PUBLIC_RELEASE_LINK}

Check legacy release appears on public frontend
    user navigates to public frontend    ${PUBLIC_RELEASE_LINK}
    user opens details dropdown    View releases (2)

    ${other_releases}=    user gets details content element    View releases (2)

    user checks list has x items    css:ul    2    ${other_releases}

    ${other_release_1}=    user gets list item element    css:ul    2    ${other_releases}
    ${other_release_1_link}=    get child element    ${other_release_1}    link:${DESCRIPTION}
    user checks element attribute value should be    ${other_release_1_link}    href    http://test.com/

Navigate to publication to update legacy releases
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Legacy releases
    user waits until h2 is visible    Legacy releases

Update legacy release
    user clicks element    xpath://tr[2]//*[text()="Edit"]
    ${modal}=    user waits until modal is visible    Edit legacy release
    user clicks button    OK    ${modal}

    user waits until page contains element    id:releaseSeriesLegacyLinkForm-description
    user enters text into element    id:releaseSeriesLegacyLinkForm-description    ${UPDATED_DESCRIPTION}
    user enters text into element    id:releaseSeriesLegacyLinkForm-url    http://test2.com
    user clicks button    Save legacy release

Validate the updated legacy release
    user waits until h2 is visible    Legacy releases
    user checks element count is x    css:tbody tr    3
    user checks table cell contains    2    1    ${UPDATED_DESCRIPTION}
    user checks table cell contains    2    2    http://test2.com
    user checks table cell contains    2    3    Legacy release

Reorder the legacy releases
    user clicks button    Reorder releases
    user waits until modal is visible    Reorder releases
    user clicks button    OK
    user waits until modal is not visible    Reorder legacy releases
    user waits until page contains button    Confirm order
    user sets focus to element    css:tbody tr:first-child
    user presses keys    ${SPACE}
    user presses keys    ARROW_DOWN
    user presses keys    ARROW_DOWN
    user presses keys    ${SPACE}
    user clicks button    Confirm order
    sleep    2

Validate reordered legacy releases
    user waits until page contains button    Reorder releases
    user checks element count is x    css:tbody tr    3

    user checks table cell contains    1    1    ${UPDATED_DESCRIPTION}
    user checks table cell contains    1    2    http://test2.com
    user checks table cell contains    1    3    Legacy release

    user checks table cell contains    2    1    ${DESCRIPTION}
    user checks table cell contains    2    2    http://test.com
    user checks table cell contains    2    3    Legacy release

    user checks table cell contains    3    1    Academic year 2020/21
    user checks table cell contains    3    2    ${PUBLIC_RELEASE_LINK}
    user checks table cell contains    3    3    Latest release

Create a second draft release via api
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user creates release from publication page    ${PUBLICATION_NAME}    Academic year Q1    2022

Add headline text block to content page(2nd release)
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Approve 2nd release
    user clicks link    Sign off
    user approves original release for immediate publication

Navigate to publication to verify the legacy releases
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Legacy releases
    user waits until h2 is visible    Legacy releases

Return to Admin and create first amendment
    user navigates to admin dashboard    Bau1
    user creates amendment for release    ${PUBLICATION_NAME}    ${RELEASE_NAME}

Change the Release type and Academic year
    user waits until page contains link    Edit release summary
    user clicks link    Edit release summary
    user waits until page finishes loading
    user waits until h2 is visible    Edit release summary
    user checks page contains radio    Official statistics in development
    user enters text into element    id:releaseSummaryForm-timePeriodCoverageStartYear    2024
    user clicks radio    Official statistics in development
    user clicks button    Update release summary
    user checks page contains element    xpath://li/a[text()="Summary" and contains(@aria-current, 'page')]
    user verifies release summary    Academic year Q1
    ...    2024/25
    ...    Official statistics in development

Navigate to 'Content' page for amendment
    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}
    user waits until page contains button    Add a summary text block

Add release note to first amendment
    user clicks button    Add note
    user enters text into element    id:create-release-note-form-reason    Test release note one
    user clicks button    Save note
    sleep    2

Navigate to "Sign off" page
    user clicks link    Sign off
    user waits until h3 is visible    Release status history

Approve release amendment
    user approves amended release for immediate publication
    user waits until page contains element    testid:public-release-url
    ${PUBLIC_AMENDED_RELEASE_LINK}=    get value    xpath://*[@data-testid="public-release-url"]
    check that variable is not empty    PUBLIC_RELEASE_LINK    ${PUBLIC_AMENDED_RELEASE_LINK}
    set suite variable    ${PUBLIC_AMENDED_RELEASE_LINK}

Navigate to publication page and verify the amended release
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Legacy releases
    user waits until h2 is visible    Legacy releases

Validate amended legacy releases
    user waits until page contains button    Reorder releases

    user checks table cell contains    1    1    Academic year Q1 2024/25
    user checks table cell contains    1    2    ${PUBLIC_AMENDED_RELEASE_LINK}
    user checks table cell contains    1    3    Latest release

    user checks table cell contains    2    1    ${UPDATED_DESCRIPTION}
    user checks table cell contains    2    2    http://test2.com
    user checks table cell contains    2    3    Legacy release

    user checks table cell contains    3    1    ${DESCRIPTION}
    user checks table cell contains    3    2    http://test.com
    user checks table cell contains    3    3    Legacy release

    user checks table cell contains    4    1    Academic year 2020/21
    user checks table cell contains    4    2    ${PUBLIC_RELEASE_LINK}
    user checks table cell contains    4    3
