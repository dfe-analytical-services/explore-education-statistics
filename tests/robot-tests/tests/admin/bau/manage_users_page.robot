*** Settings ***
Resource            ../../libs/admin-common.robot
Resource            ../../libs/common.robot

Library             ../../libs/admin_api.py

Force Tags          Admin    Local    Dev

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Variables ***
${PUBLICATION_NAME}=        Manage users %{RUN_IDENTIFIER}
${RELEASE_NAME}=            Calendar year 2000
${PUBLICATION_2_NAME}=      Manage users %{RUN_IDENTIFIER} second
${RELEASE_2_NAME}=          Academic year 2000/01


*** Test Cases ***
Navigate to manage users page as bau1
    user navigates to    %{ADMIN_URL}/administration/users
    user checks table column heading contains    1    1    Name
    user checks table column heading contains    1    2    Email
    user checks table column heading contains    1    3    Role
    user checks table column heading contains    1    4    Actions

Check correct test users are present in table
    ${row}=    user gets table row with heading    Analyst1 User1
    user checks row cell contains text    ${row}    1    ees-test.analyst1@education.gov.uk
    user checks row cell contains text    ${row}    2    Standard User
    user checks row cell contains text    ${row}    3    Manage

    ${row}=    user gets table row with heading    Analyst2 User2
    user checks row cell contains text    ${row}    1    ees-test.analyst2@education.gov.uk
    user checks row cell contains text    ${row}    2    Standard User
    user checks row cell contains text    ${row}    3    Manage

    ${row}=    user gets table row with heading    Bau1 User1
    user checks row cell contains text    ${row}    1    ees-test.bau1@education.gov.uk
    user checks row cell contains text    ${row}    2    Super User
    user checks row cell contains text    ${row}    3    Manage

    ${row}=    user gets table row with heading    Bau2 User2
    user checks row cell contains text    ${row}    1    ees-test.bau2@education.gov.uk
    user checks row cell contains text    ${row}    2    Super User
    user checks row cell contains text    ${row}    3    Manage

Assert prerelease users are present in table
    ${list}=    create list    ees-prerelease1@education.gov.uk    ees-prerelease2@education.gov.uk
    user resets user roles via api if required    ${list}
    user reloads page

    ${row1}=    user gets table row with heading    Prerelease1 User1
    ${row2}=    user gets table row with heading    Prerelease2 User2

    user checks row cell contains text    ${row1}    1    ees-prerelease1@education.gov.uk
    user checks row cell contains text    ${row1}    2    Standard User
    user checks row cell contains text    ${row1}    3    Manage
    user checks row cell contains text    ${row2}    1    ees-prerelease2@education.gov.uk
    user checks row cell contains text    ${row2}    2    Standard Use
    user checks row cell contains text    ${row2}    3    Manage
    set suite variable    ${PRE_RELEASE_ROW}    ${row2}

Select a user to manage
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    CY    2000

    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_2_NAME}
    user creates test release via api    ${PUBLICATION_ID}    AY    2000

    user clicks link    Manage    ${PRE_RELEASE_ROW}
    user waits until page contains    Manage user
    user waits until page finishes loading
    user waits until h1 is visible    Prerelease2 User2    10

Check the initial manage user page
    user checks checkbox is not checked    Super User

    user checks select contains option    name:releaseId    ${PUBLICATION_NAME} - ${RELEASE_NAME}
    user checks select contains option    name:releaseId    ${PUBLICATION_2_NAME} - ${RELEASE_2_NAME}

    user checks select contains option    name:publicationId    ${PUBLICATION_NAME}
    user checks select contains option    name:publicationId    ${PUBLICATION_2_NAME}
    user checks select contains x options    name:publicationRole    2
    user checks select contains option    name:publicationRole    Drafter
    user checks select contains option    name:publicationRole    Approver

Give the user prerelease access to a release
    user chooses select option    name:releaseId    ${PUBLICATION_NAME} - ${RELEASE_NAME}
    user clicks button    Add pre-release access

    user checks table body has x rows    1    testid:preReleaseAccessTable
    user checks table column heading contains    1    1    Publication    testid:preReleaseAccessTable
    user checks table column heading contains    1    2    Release    testid:preReleaseAccessTable
    user checks table column heading contains    1    3    Actions    testid:preReleaseAccessTable

    user checks table cell contains    1    1    ${PUBLICATION_NAME}    testid:preReleaseAccessTable
    user checks table cell contains    1    2    ${RELEASE_NAME}    testid:preReleaseAccessTable
    user checks table cell contains    1    3    Remove    testid:preReleaseAccessTable

Remove prerelease access for release from user
    user clicks button in table cell    1    3    Remove    testid:preReleaseAccessTable
    user checks table body has x rows    0    testid:preReleaseAccessTable

Give the user drafter access to some publications
    user chooses select option    name:publicationId    ${PUBLICATION_NAME}
    user chooses select option    name:publicationRole    Drafter
    user clicks button    Add publication access
    user checks table body has x rows    1    testid:publicationAccessTable
    user checks table cell contains    1    1    ${PUBLICATION_NAME}    testid:publicationAccessTable
    user checks table cell contains    1    2    Drafter    testid:publicationAccessTable
    user checks table cell contains    1    3    Remove    testid:publicationAccessTable

    user chooses select option    name:publicationId    ${PUBLICATION_2_NAME}
    user chooses select option    name:publicationRole    Drafter
    user clicks button    Add publication access
    user checks table body has x rows    2    testid:publicationAccessTable
    user checks table cell contains    1    1    ${PUBLICATION_NAME}    testid:publicationAccessTable
    user checks table cell contains    1    2    Drafter    testid:publicationAccessTable
    user checks table cell contains    1    3    Remove    testid:publicationAccessTable
    user checks table cell contains    2    1    ${PUBLICATION_2_NAME}    testid:publicationAccessTable
    user checks table cell contains    2    2    Drafter    testid:publicationAccessTable
    user checks table cell contains    2    3    Remove    testid:publicationAccessTable

Give the user the Super User role
    user clicks checkbox    Super User
    user checks checkbox is checked    Super User
    user clicks button    Update access
    user waits until page finishes loading
    user checks checkbox is checked    Super User

Remove publication drafter access for one of the publications from user while they are Super User
    user clicks button in table cell    1    3    Remove    testid:publicationAccessTable
    user checks table body has x rows    1    testid:publicationAccessTable
    user checks table cell contains    1    1    ${PUBLICATION_2_NAME}    testid:publicationAccessTable
    user checks table cell contains    1    2    Drafter    testid:publicationAccessTable
    user checks table cell contains    1    3    Remove    testid:publicationAccessTable

Remove publication drafter access for the final publication from user while they are Super User
    user clicks button in table cell    1    3    Remove    testid:publicationAccessTable
    user checks table body has x rows    0    testid:publicationAccessTable

Give the user approver access to a publication while they are Super User and manually set their global role to Standard User
    user chooses select option    name:publicationId    ${PUBLICATION_NAME}
    user chooses select option    name:publicationRole    Approver
    user clicks button    Add publication access
    user checks table body has x rows    1    testid:publicationAccessTable
    user checks table cell contains    1    1    ${PUBLICATION_NAME}    testid:publicationAccessTable
    user checks table cell contains    1    2    Approver    testid:publicationAccessTable
    user checks table cell contains    1    3    Remove    testid:publicationAccessTable

    user clicks checkbox    Super User
    user checks checkbox is not checked    Super User
    user clicks button    Update access
    user waits until page finishes loading
    user checks checkbox is not checked    Super User

Remove publication approver access from user after they have manually been set to Standard User
    user clicks button in table cell    1    3    Remove    testid:publicationAccessTable
    user checks table body has x rows    0    testid:publicationAccessTable
