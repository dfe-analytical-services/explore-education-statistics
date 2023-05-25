*** Settings ***
Resource            ../../libs/admin-common.robot
Resource            ../../libs/common.robot

Library             ../../libs/admin_api.py

Force Tags          Admin    Local    Dev

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Variables ***
${PUBLICATION_NAME}=        UI tests - manage users %{RUN_IDENTIFIER}
${RELEASE_NAME}=            Calendar year 2000
${PUBLICATION_2_NAME}=      UI tests - manage users second %{RUN_IDENTIFIER}
${RELEASE_2_NAME}=          Academic year 2000/01


*** Test Cases ***
Navigate to manage users page as bau1
    user navigates to admin frontend    %{ADMIN_URL}/administration/users
    user checks table column heading contains    1    1    Name
    user checks table column heading contains    1    2    Email
    user checks table column heading contains    1    3    Role
    user checks table column heading contains    1    4    Actions

Check correct test users are present in table
    ${ROW}=    user gets table row with heading    Analyst1 User1
    user checks row cell contains text    ${ROW}    1    EES-test.ANALYST1@education.gov.uk
    user checks row cell contains text    ${ROW}    2    Analyst
    user checks row cell contains text    ${ROW}    3    Manage

    ${ROW}=    user gets table row with heading    Analyst2 User2
    user checks row cell contains text    ${ROW}    1    EES-test.ANALYST2@education.gov.uk
    user checks row cell contains text    ${ROW}    2    Analyst
    user checks row cell contains text    ${ROW}    3    Manage

    ${ROW}=    user gets table row with heading    Bau1 User1
    user checks row cell contains text    ${ROW}    1    EES-test.BAU1@education.gov.uk
    user checks row cell contains text    ${ROW}    2    BAU User
    user checks row cell contains text    ${ROW}    3    Manage

    ${ROW}=    user gets table row with heading    Bau2 User2
    user checks row cell contains text    ${ROW}    1    EES-test.BAU2@education.gov.uk
    user checks row cell contains text    ${ROW}    2    BAU User
    user checks row cell contains text    ${ROW}    3    Manage

Assert prerelease users are present in table
    ${list}=    create list    ees-prerelease1@education.gov.uk    ees-prerelease2@education.gov.uk
    user resets user roles via api if required    ${list}
    user reloads page

    ${ROW}=    user gets table row with heading    Prerelease2 User2

    user checks row cell contains text    ${ROW}    1    ees-prerelease2@education.gov.uk
    user checks row cell contains text    ${ROW}    2    No role
    user checks row cell contains text    ${ROW}    3    Manage
    set suite variable    ${PRE_RELEASE_ROW}    ${ROW}

Select a user to manage
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    CY    2000

    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_2_NAME}
    user creates test release via api    ${PUBLICATION_ID}    AY    2000

    user clicks link    Manage    ${PRE_RELEASE_ROW}
    user waits until page contains title    Manage user
    user waits until page does not contain loading spinner
    user waits until h1 is visible    Prerelease2 User2    10

Check the initial manage user page
    user checks select contains x options    //*[@name="selectedRoleId"]    4
    user checks select contains option    //*[@name="selectedRoleId"]    Choose role
    user checks select contains option    //*[@name="selectedRoleId"]    Analyst
    user checks select contains option    //*[@name="selectedRoleId"]    BAU User
    user checks select contains option    //*[@name="selectedRoleId"]    Prerelease User
    user checks selected option label    //*[@name="selectedRoleId"]    Choose role

    user checks select contains option    name:selectedReleaseId    ${PUBLICATION_NAME} - ${RELEASE_NAME}
    user checks select contains option    name:selectedReleaseId    ${PUBLICATION_2_NAME} - ${RELEASE_2_NAME}
    user checks select contains x options    name:selectedReleaseRole    5
    user checks select contains option    name:selectedReleaseRole    Approver
    user checks select contains option    name:selectedReleaseRole    Contributor
    user checks select contains option    name:selectedReleaseRole    Lead
    user checks select contains option    name:selectedReleaseRole    PrereleaseViewer
    user checks select contains option    name:selectedReleaseRole    Viewer
    user checks table body has x rows    0    testid:releaseAccessTable

    user checks select contains option    name:selectedPublicationId    ${PUBLICATION_NAME}
    user checks select contains option    name:selectedPublicationId    ${PUBLICATION_2_NAME}
    user checks select contains x options    name:selectedPublicationRole    2
    user checks select contains option    name:selectedPublicationRole    Owner
    user checks select contains option    name:selectedPublicationRole    Approver
    user checks table body has x rows    0    testid:publicationAccessTable

Give the user prerelease access to a release
    user chooses select option    name:selectedReleaseId    ${PUBLICATION_NAME} - ${RELEASE_NAME}
    user chooses select option    name:selectedReleaseRole    PrereleaseViewer
    user clicks button    Add release access
    user checks table body has x rows    1    testid:releaseAccessTable
    user checks table column heading contains    1    1    Publication    testid:releaseAccessTable
    user checks table column heading contains    1    2    Release    testid:releaseAccessTable
    user checks table column heading contains    1    3    Role    testid:releaseAccessTable
    user checks table column heading contains    1    4    Actions    testid:releaseAccessTable

    user checks table cell contains    1    1    ${PUBLICATION_NAME}    testid:releaseAccessTable
    user checks table cell contains    1    2    ${RELEASE_NAME}    testid:releaseAccessTable
    user checks table cell contains    1    3    PrereleaseViewer    testid:releaseAccessTable
    user checks table cell contains    1    4    Remove    testid:releaseAccessTable
    user checks selected option label    //*[@name="selectedRoleId"]    Prerelease User

Give the user approver access to a release
    user chooses select option    name:selectedReleaseId    ${PUBLICATION_NAME} - ${RELEASE_NAME}
    user chooses select option    name:selectedReleaseRole    Approver
    user clicks button    Add release access
    user checks table body has x rows    2    testid:releaseAccessTable

    user checks table cell contains    1    1    ${PUBLICATION_NAME}    testid:releaseAccessTable
    user checks table cell contains    1    2    ${RELEASE_NAME}    testid:releaseAccessTable
    user checks table cell contains    1    3    PrereleaseViewer    testid:releaseAccessTable
    user checks table cell contains    1    4    Remove    testid:releaseAccessTable
    user checks table cell contains    2    1    ${PUBLICATION_NAME}    testid:releaseAccessTable
    user checks table cell contains    2    2    ${RELEASE_NAME}    testid:releaseAccessTable
    user checks table cell contains    2    3    Approver    testid:releaseAccessTable
    user checks table cell contains    2    4    Remove    testid:releaseAccessTable
    user checks selected option label    //*[@name="selectedRoleId"]    Analyst

Remove approver access for release from user
    user clicks button in table cell    2    4    Remove    testid:releaseAccessTable
    user checks table body has x rows    1    testid:releaseAccessTable
    user checks table cell contains    1    1    ${PUBLICATION_NAME}    testid:releaseAccessTable
    user checks table cell contains    1    2    ${RELEASE_NAME}    testid:releaseAccessTable
    user checks table cell contains    1    3    PrereleaseViewer    testid:releaseAccessTable
    user checks table cell contains    1    4    Remove    testid:releaseAccessTable
    user checks selected option label    //*[@name="selectedRoleId"]    Prerelease User

Remove prerelease access for release from user
    user clicks button in table cell    1    4    Remove    testid:releaseAccessTable
    user checks table body has x rows    0    testid:releaseAccessTable
    user checks selected option label    //*[@name="selectedRoleId"]    Choose role

Give the user owner access to some publications
    user chooses select option    name:selectedPublicationId    ${PUBLICATION_NAME}
    user chooses select option    name:selectedPublicationRole    Owner
    user clicks button    Add publication access
    user checks table body has x rows    1    testid:publicationAccessTable
    user checks table cell contains    1    1    ${PUBLICATION_NAME}    testid:publicationAccessTable
    user checks table cell contains    1    2    Owner    testid:publicationAccessTable
    user checks table cell contains    1    3    Remove    testid:publicationAccessTable
    user checks selected option label    //*[@name="selectedRoleId"]    Analyst

    user chooses select option    name:selectedPublicationId    ${PUBLICATION_2_NAME}
    user chooses select option    name:selectedPublicationRole    Owner
    user clicks button    Add publication access
    user checks table body has x rows    2    testid:publicationAccessTable
    user checks table cell contains    1    1    ${PUBLICATION_NAME}    testid:publicationAccessTable
    user checks table cell contains    1    2    Owner    testid:publicationAccessTable
    user checks table cell contains    1    3    Remove    testid:publicationAccessTable
    user checks table cell contains    2    1    ${PUBLICATION_2_NAME}    testid:publicationAccessTable
    user checks table cell contains    2    2    Owner    testid:publicationAccessTable
    user checks table cell contains    2    3    Remove    testid:publicationAccessTable
    user checks selected option label    //*[@name="selectedRoleId"]    Analyst

Give the user the BAU User role
    user chooses select option    //*[@name="selectedRoleId"]    BAU User
    user clicks button    Update role
    user waits for page to finish loading
    user checks selected option label    //*[@name="selectedRoleId"]    BAU User

Remove publication owner access for one of the publications from user while they are BAU
    user clicks button in table cell    1    3    Remove    testid:publicationAccessTable
    user checks table body has x rows    1    testid:publicationAccessTable
    user checks table cell contains    1    1    ${PUBLICATION_2_NAME}    testid:publicationAccessTable
    user checks table cell contains    1    2    Owner    testid:publicationAccessTable
    user checks table cell contains    1    3    Remove    testid:publicationAccessTable
    user checks selected option label    //*[@name="selectedRoleId"]    BAU User

Give the user approver access to a release while they are BAU
    user chooses select option    name:selectedReleaseId    ${PUBLICATION_NAME} - ${RELEASE_NAME}
    user chooses select option    name:selectedReleaseRole    Approver
    user clicks button    Add release access
    user checks table body has x rows    1    testid:releaseAccessTable
    user checks table cell contains    1    1    ${PUBLICATION_NAME}    testid:releaseAccessTable
    user checks table cell contains    1    2    ${RELEASE_NAME}    testid:releaseAccessTable
    user checks table cell contains    1    3    Approver    testid:releaseAccessTable
    user checks table cell contains    1    4    Remove    testid:releaseAccessTable
    user checks selected option label    //*[@name="selectedRoleId"]    BAU User

Remove approver access for release from user while they are BAU
    user clicks button in table cell    1    4    Remove    testid:releaseAccessTable
    user checks table body has x rows    0    testid:releaseAccessTable
    user checks selected option label    //*[@name="selectedRoleId"]    BAU User

Remove publication owner access for the final publication from user while they are BAU
    user clicks button in table cell    1    3    Remove    testid:publicationAccessTable
    user checks table body has x rows    0    testid:publicationAccessTable
    user checks selected option label    //*[@name="selectedRoleId"]    BAU User

Give the user approver access to a release while they are BAU and manually set their role to Analyst
    user chooses select option    name:selectedReleaseId    ${PUBLICATION_NAME} - ${RELEASE_NAME}
    user chooses select option    name:selectedReleaseRole    Approver
    user clicks button    Add release access
    user checks table body has x rows    1    testid:releaseAccessTable
    user checks table cell contains    1    1    ${PUBLICATION_NAME}    testid:releaseAccessTable
    user checks table cell contains    1    2    ${RELEASE_NAME}    testid:releaseAccessTable
    user checks table cell contains    1    3    Approver    testid:releaseAccessTable
    user checks table cell contains    1    4    Remove    testid:releaseAccessTable
    user checks selected option label    //*[@name="selectedRoleId"]    BAU User

    user chooses select option    //*[@name="selectedRoleId"]    Analyst
    user clicks button    Update role
    user waits for page to finish loading
    user checks selected option label    //*[@name="selectedRoleId"]    Analyst

Remove approver access for release from user after they have manually been set to Analyst
    user clicks button in table cell    1    4    Remove    testid:releaseAccessTable
    user checks table body has x rows    0    testid:releaseAccessTable
    user checks selected option label    //*[@name="selectedRoleId"]    Choose role
