*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/tables-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${PUBLICATION1_NAME}=               Invite new users 1 %{RUN_IDENTIFIER}
${PUBLICATION2_NAME}=               Invite new users 2 %{RUN_IDENTIFIER}
${PUBLICATION1_RELEASE1_NAME}=      ${PUBLICATION1_NAME} - Academic year 2000/01
${PUBLICATION1_RELEASE2_NAME}=      ${PUBLICATION1_NAME} - Academic year 2001/02
${PUBLICATION2_RELEASE1_NAME}=      ${PUBLICATION2_NAME} - Academic year 2000/01
${EMAIL}=                           ees-ui-test-%{RUN_IDENTIFIER}@education.gov.uk


*** Test Cases ***
Create Publication as bau1
    ${publication1_id}=    user creates test publication via api    ${PUBLICATION1_NAME}
    ${publication2_id}=    user creates test publication via api    ${PUBLICATION2_NAME}
    user creates test release via api    ${publication1_id}    AY    2000
    user creates test release via api    ${publication1_id}    AY    2001
    user creates test release via api    ${publication2_id}    AY    2000

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
    user checks element contains    ${ROW}    No user pre-release roles
    user checks element contains    ${ROW}    No user publication roles

Cancel invite
    user clicks button    Cancel invite    ${ROW}
    user waits until page does not contain    ${EMAIL}
    user waits until h1 is visible    Pending invites

Invite a new user with pre-release and publication roles within the same publication
    user clicks link    Invite a new user
    user waits until h1 is visible    Invite user

    user enters text into element    name:userEmail    ${EMAIL}

    user chooses select option    name:releaseId    ${PUBLICATION1_RELEASE1_NAME}
    user clicks button    Add pre-release role

    user checks table body has x rows    1    testid:pre-release-role-table
    user gets table row    ${PUBLICATION1_RELEASE1_NAME}    testid:pre-release-role-table

    user chooses select option    name:releaseId    ${PUBLICATION1_RELEASE2_NAME}
    user clicks button    Add pre-release role

    user checks table body has x rows    2    testid:pre-release-role-table
    user gets table row    ${PUBLICATION1_RELEASE1_NAME}    testid:pre-release-role-table
    ${ROW}=    user gets table row    ${PUBLICATION1_RELEASE2_NAME}    testid:pre-release-role-table

    user clicks button    Remove    ${ROW}
    user checks table body has x rows    1    testid:pre-release-role-table
    user checks element does not contain    testid:pre-release-role-table    ${PUBLICATION1_RELEASE2_NAME}

    # Because we're adding a more powerful publication-level role to the same publication as the pre-release
    # role above, the pre-release role will be rendered redundant and should therefore be silently
    # ignored on the BE and not added to the user's list of pre-release roles (current implementation).
    # Therefore, we would not expect for this pre-release role to appear on the Pending Invites page
    # after we send the invite.
    user chooses select option    name:publicationId    ${PUBLICATION1_NAME}
    user chooses select option    name:publicationRole    Approver
    user clicks button    Add publication role

    user checks table body has x rows    1    testid:publication-role-table
    ${ROW}=    user gets table row    ${PUBLICATION1_NAME}    testid:publication-role-table
    user checks element contains    ${ROW}    Approver

    user clicks button    Send invite
    user waits until h1 is visible    Pending invites

Validate newly invited user with roles appears on Pending invites page but the redundant pre-release role was not added
    ${ROW}=    user gets table row    ${EMAIL}
    set suite variable    ${ROW}
    user checks element contains    ${ROW}    Analyst
    user checks element does not contain    ${ROW}    Academic year 2000/01
    user checks element does not contain    ${ROW}    Academic year 2001/02
    user checks element contains    ${ROW}    No user pre-release roles
    user checks element contains    ${ROW}    ${PUBLICATION1_NAME} - Approver

Cancel invite with roles
    user clicks button    Cancel invite    ${ROW}
    user waits until page does not contain    ${EMAIL}
    user waits until h1 is visible    Pending invites

Invite a new user with pre-release and publication roles within different publications
    user clicks link    Invite a new user
    user waits until h1 is visible    Invite user

    user enters text into element    name:userEmail    ${EMAIL}

    user chooses select option    name:releaseId    ${PUBLICATION1_RELEASE1_NAME}
    user clicks button    Add pre-release role

    user checks table body has x rows    1    testid:pre-release-role-table
    user gets table row    ${PUBLICATION1_RELEASE1_NAME}    testid:pre-release-role-table

    user chooses select option    name:releaseId    ${PUBLICATION1_RELEASE2_NAME}
    user clicks button    Add pre-release role

    user checks table body has x rows    2    testid:pre-release-role-table
    user gets table row    ${PUBLICATION1_RELEASE1_NAME}    testid:pre-release-role-table
    ${ROW}=    user gets table row    ${PUBLICATION1_RELEASE2_NAME}    testid:pre-release-role-table

    user clicks button    Remove    ${ROW}
    user checks table body has x rows    1    testid:pre-release-role-table
    user checks element does not contain    testid:pre-release-role-table    ${PUBLICATION1_RELEASE2_NAME}

    # In this case, because we're adding a publication-level role to a DIFFERENT publication to the pre-release
    # role above, the pre-release role will still be useful, and therefore should not be silently
    # ignored on the BE and will be added to the user's list of pre-release roles.
    # Therefore, we WOULD expect for this pre-release role to appear on the Pending Invites page
    # after we send the invite.
    user chooses select option    name:publicationId    ${PUBLICATION2_NAME}
    user chooses select option    name:publicationRole    Approver
    user clicks button    Add publication role

    user checks table body has x rows    1    testid:publication-role-table
    ${ROW}=    user gets table row    ${PUBLICATION2_NAME}    testid:publication-role-table
    user checks element contains    ${ROW}    Approver

    user clicks button    Send invite
    user waits until h1 is visible    Pending invites

Validate newly invited user with roles appears on Pending invites page and the pre-release role appears this time
    ${ROW}=    user gets table row    ${EMAIL}
    set suite variable    ${ROW}
    user checks element contains    ${ROW}    Analyst
    user checks element contains    ${ROW}    Academic year 2000/01
    user checks element does not contain    ${ROW}    Academic year 2001/02
    user checks element contains    ${ROW}    ${PUBLICATION2_NAME} - Approver

Cancel invite with roles
    user clicks button    Cancel invite    ${ROW}
    user waits until page does not contain    ${EMAIL}
    user waits until h1 is visible    Pending invites
