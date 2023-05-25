*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/tables-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${PUBLICATION_NAME}=    UI tests - invite new users %{RUN_IDENTIFIER}
${RELEASE1_NAME}=       ${PUBLICATION_NAME} - Academic year 2000/01
${RELEASE2_NAME}=       ${PUBLICATION_NAME} - Academic year 2001/02
${RELEASE3_NAME}=       ${PUBLICATION_NAME} - Academic year 2002/03
${EMAIL}=               ees-ui-test-%{RUN_IDENTIFIER}@education.gov.uk


*** Test Cases ***
Create Publication as bau1
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    AY    2000
    user creates test release via api    ${PUBLICATION_ID}    AY    2001
    user creates test release via api    ${PUBLICATION_ID}    AY    2002

Navigate to Platform administration, Invite new users page
    user clicks link    Platform administration
    user waits until h1 is visible    Platform administration

    user clicks link    Invite new users
    user waits until h1 is visible    Pending invites

Invite a new user without any additional roles
    user clicks link    Invite a new user
    user waits until h1 is visible    Invite user

    user enters text into element    name:userEmail    ${EMAIL}
    user clicks button    Send invite
    user waits until h1 is visible    Pending invites

Validate newly invited user appears on Pending invites page
    ${ROW}=    user gets table row    ${EMAIL}
    set suite variable    ${ROW}
    user checks element contains    ${ROW}    Analyst
    user checks element contains    ${ROW}    No user release roles
    user checks element contains    ${ROW}    No user publication roles

Cancel invite
    user clicks button    Cancel invite    ${ROW}
    user waits until page does not contain    ${EMAIL}
    user waits until h1 is visible    Pending invites

Invite a new user with release and publication roles
    user clicks link    Invite a new user
    user waits until h1 is visible    Invite user

    user enters text into element    name:userEmail    ${EMAIL}

    user chooses select option    name:releaseId    ${RELEASE1_NAME}
    user chooses select option    name:releaseRole    Approver
    user clicks button    Add release role

    user checks table body has x rows    1    testid:release-role-table
    ${ROW}=    user gets table row    ${RELEASE1_NAME}    testid:release-role-table
    user checks element contains    ${ROW}    Approver

    user chooses select option    name:releaseId    ${RELEASE2_NAME}
    user chooses select option    name:releaseRole    Contributor
    user clicks button    Add release role

    user checks table body has x rows    2    testid:release-role-table
    ${ROW}=    user gets table row    ${RELEASE1_NAME}    testid:release-role-table
    user checks element contains    ${ROW}    Approver
    ${ROW}=    user gets table row    ${RELEASE2_NAME}    testid:release-role-table
    user checks element contains    ${ROW}    Contributor

    user chooses select option    name:releaseId    ${RELEASE3_NAME}
    user chooses select option    name:releaseRole    PrereleaseViewer
    user clicks button    Add release role

    user checks table body has x rows    3    testid:release-role-table
    ${ROW}=    user gets table row    ${RELEASE3_NAME}    testid:release-role-table
    user checks element contains    ${ROW}    PrereleaseViewer

    user clicks button    Remove    ${ROW}
    user checks table body has x rows    2    testid:release-role-table
    user checks element does not contain    testid:release-role-table    ${RELEASE3_NAME}

    user chooses select option    name:publicationId    ${PUBLICATION_NAME}
    user chooses select option    name:publicationRole    Approver
    user clicks button    Add publication role

    user checks table body has x rows    1    testid:publication-role-table
    ${ROW}=    user gets table row    ${PUBLICATION_NAME}    testid:publication-role-table
    user checks element contains    ${ROW}    Approver

    user clicks button    Send invite
    user waits until h1 is visible    Pending invites

Validate newly invited user with roles appears on Pending invites page
    ${ROW}=    user gets table row    ${EMAIL}
    set suite variable    ${ROW}
    user checks element contains    ${ROW}    Analyst
    user checks element contains    ${ROW}    ${PUBLICATION_NAME}

    user checks element contains    ${ROW}    Academic year 2000/01
    user checks element contains    ${ROW}    Approver
    user checks element contains    ${ROW}    Academic year 2001/02
    user checks element contains    ${ROW}    Contributor
    user checks element does not contain    ${ROW}    Academic year 2002/03
    user checks element does not contain    ${ROW}    PrereleaseViewer

    user checks element contains    ${ROW}    ${PUBLICATION_NAME} - Approver

Cancel invite with roles
    user clicks button    Cancel invite    ${ROW}
    user waits until page does not contain    ${EMAIL}
    user waits until h1 is visible    Pending invites
