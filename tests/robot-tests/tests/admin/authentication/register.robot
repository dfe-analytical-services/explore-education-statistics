*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/tables-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser and delete test user
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${PUBLICATION_NAME}=    UI tests - register %{RUN_IDENTIFIER}
${RELEASE_NAME}=        ${PUBLICATION_NAME} - Academic year 2000/01


*** Test Cases ***
Create publication and release and ensure that the invitee is not already registered
    ${publication_id}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${publication_id}    AY    2000
    delete test user    %{PENDING_INVITE_USER_EMAIL}

Invite the user as an Analyst with publication and release roles
    user clicks link    Platform administration
    user waits until h1 is visible    Platform administration
    user clicks link    Invite new users
    user waits until h1 is visible    Pending invites
    user clicks link    Invite a new user
    user waits until h1 is visible    Invite user
    user enters text into element    name:userEmail    %{PENDING_INVITE_USER_EMAIL}

    user chooses select option    name:releaseId    ${RELEASE_NAME}
    user chooses select option    name:releaseRole    Approver
    user clicks button    Add release role

    user checks table body has x rows    1    testid:release-role-table
    ${ROW}=    user gets table row    ${RELEASE_NAME}    testid:release-role-table
    user checks element contains    ${ROW}    Approver

    user chooses select option    name:publicationId    ${PUBLICATION_NAME}
    user chooses select option    name:publicationRole    Approver
    user clicks button    Add publication role

    user checks table body has x rows    1    testid:publication-role-table
    ${ROW}=    user gets table row    ${PUBLICATION_NAME}    testid:publication-role-table
    user checks element contains    ${ROW}    Approver

    user clicks button    Send invite
    user waits until h1 is visible    Pending invites
    user closes the browser

Sign in as the invitee to register as an Analyst
    user opens browser and logs in via Identity Provider

Check that the user is on the dashboard and has the correct access for an Analyst
    user checks for Analyst access on the Dashboard

Log out and log back in again to make sure that the user still has the correct access for an Analyst
    user closes the browser
    user opens browser and logs in via Identity Provider
    user checks for Analyst access on the Dashboard

Clear down the invited user so that we can invite them again
    delete test user    %{PENDING_INVITE_USER_EMAIL}

Invite the user as a BAU user
    user signs in as bau1
    user clicks link    Platform administration
    user waits until h1 is visible    Platform administration
    user clicks link    Invite new users
    user waits until h1 is visible    Pending invites
    user clicks link    Invite a new user
    user waits until h1 is visible    Invite user
    user enters text into element    name:userEmail    %{PENDING_INVITE_USER_EMAIL}
    user chooses select option    name:roleId    BAU User
    user clicks button    Send invite
    user waits until h1 is visible    Pending invites
    user closes the browser

Sign in as the invitee to register as a BAU user
    user opens browser and logs in via Identity Provider

Check that the user is on the dashboard and has the correct access for a BAU user
    user checks for BAU access on the Dashboard

Log out and log back in again to make sure that the user still has the correct access for a BAU user
    user closes the browser
    user opens browser and logs in via Identity Provider
    user checks for BAU access on the Dashboard


*** Keywords ***
user closes the browser and delete test user
    user closes the browser
    delete test user    %{PENDING_INVITE_USER_EMAIL}

user opens browser and logs in via Identity Provider
    user opens the browser
    user navigates to admin homepage
    user waits until h1 is visible    Sign in
    user clicks element    id:signin-button
    user logs in via identity provider
    ...    %{PENDING_INVITE_USER_EMAIL}
    ...    %{PENDING_INVITE_USER_PASSWORD}

user checks for Analyst access on the Dashboard
    user waits until page contains title    Dashboard
    user waits until h3 is visible    %{TEST_THEME_NAME}
    user waits until page contains link    ${PUBLICATION_NAME}
    user checks page contains    Logged in as Pending
    user checks page does not contain link    Platform administration

user checks for BAU access on the Dashboard
    user waits until page contains title    Dashboard
    user checks page contains    Logged in as Pending
    user checks page contains link    Platform administration
