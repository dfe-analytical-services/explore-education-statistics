*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Resource            ../../libs/public-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${PUBLICATION_NAME}=                Publishing organisations %{RUN_IDENTIFIER}
${RELEASE_NAME}=                    Financial year 3000-01
${DEFAULT_ORGANISATION_TEXT}=       Department for Education
${UPDATED_ORGANISATION_TEXT}=       Department for Education and Skills England


*** Test Cases ***
Create new publication and release via API
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    FY    3000

Verify default "Published by" shows as Department for Education
    user checks published by in admin    ${DEFAULT_ORGANISATION_TEXT}

Go back to "Release summary" page
    user navigates to draft release page from dashboard
    ...    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}

Set "Published by" options for release
    user waits until page contains link    Edit release summary
    user clicks link    Edit release summary
    user waits until page finishes loading
    user waits until h2 is visible    Edit release summary
    user checks checkbox is not checked    Department for Education
    user checks checkbox is not checked    Skills England
    user clicks checkbox    Department for Education
    user clicks checkbox    Skills England
    user clicks button    Update release summary
    user waits until h2 is visible    Release summary

Verify "Published by" shows updated organisations
    user checks published by in admin    ${UPDATED_ORGANISATION_TEXT}

Navigate to 'Content' page
    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}
    user waits until page contains button    Add a summary text block    %{WAIT_SMALL}

Add free text key stat
    user adds free text key stat    Free text key stat title    9001%    Trend    Guidance title    Guidance text

    user checks element count is x    testid:keyStat    1
    user checks key stat contents    1    Free text key stat title    9001%    Trend
    user checks key stat guidance    1    Guidance title    Guidance text

Approve release
    user approves original release for immediate publication

Get public release link
    ${PUBLIC_RELEASE_LINK}=    user gets url public release will be accessible at
    Set Suite Variable    ${PUBLIC_RELEASE_LINK}

Check public release page lists organisations correctly
    user navigates to public release page    ${PUBLIC_RELEASE_LINK}    ${PUBLICATION_NAME}    ${RELEASE_NAME}
    user checks published by on public release page
    ...    ${UPDATED_ORGANISATION_TEXT}

Return to Admin and create amendment
    user navigates to admin dashboard    Bau1
    user creates amendment for release    ${PUBLICATION_NAME}    ${RELEASE_NAME}

Verify amendment inherited the same organisations
    user checks published by in admin    ${UPDATED_ORGANISATION_TEXT}

Go back to "Release summary" page
    user navigates to draft release page from dashboard
    ...    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}

Verify the Organisation checkboxes are preselected
    user waits until page contains link    Edit release summary
    user clicks link    Edit release summary
    user waits until page finishes loading
    user waits until h2 is visible    Edit release summary
    user checks checkbox is checked    Department for Education
    user checks checkbox is checked    Skills England

Verify the user can unselect organisations for amendment
    user clicks checkbox    Department for Education
    user clicks checkbox    Skills England
    user clicks button    Update release summary
    user waits until h2 is visible    Release summary

Verify "Published by" shows default organisation
    user checks published by in admin    ${DEFAULT_ORGANISATION_TEXT}

Publish amendment
    user clicks button    Add note
    user enters text into element    id:create-release-note-form-reason    Test release note one
    user clicks button    Save note
    ${date}=    get london date
    user waits until element contains    css:#release-notes li:nth-of-type(1) time    ${date}
    user waits until element contains    css:#release-notes li:nth-of-type(1) p    Test release note one

Verify default organisation is showing on public page again
    user navigates to public release page    ${PUBLIC_RELEASE_LINK}    ${PUBLICATION_NAME}    ${RELEASE_NAME}
    user checks published by on public release page
    ...    ${DEFAULT_ORGANISATION_TEXT}


*** Keywords ***
user checks published by in admin
    [Arguments]    ${published_by}
    user navigates to draft release page from dashboard
    ...    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}
    user verifies release summary    Financial year
    ...    3000-01
    ...    Accredited official statistics
    ...
    ...    ${published_by}
    user clicks link    Content
    user checks testid element contains    Produced by-value    ${published_by}

user checks published by on public release page
    [Arguments]    ${published_by}
    user checks testid element contains    Produced by-value    ${published_by}
