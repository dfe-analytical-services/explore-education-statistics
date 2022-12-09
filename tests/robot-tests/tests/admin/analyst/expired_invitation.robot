*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev


*** Test Cases ***
Invite user to the service with an unexpired invite via the API
    user adds user invite via api
    ...    %{EXPIRED_INVITE_USER_EMAIL}
    ...    Analyst

Check that the invite appears on the Invite Users page
    user navigates to admin frontend    %{ADMIN_URL}/administration/users/invites
    user waits until page contains    Invited users
    user checks page contains    %{EXPIRED_INVITE_USER_EMAIL}

Update user invite with an expired created date via the API
    user adds user invite via api
    ...    %{EXPIRED_INVITE_USER_EMAIL}
    ...    Analyst
    ...    2022-10-01 11:00:00

Check that the invite does not appear on the Invite Users page
    user reloads page
    user waits until page contains    Invited users
    user checks page does not contain    %{EXPIRED_INVITE_USER_EMAIL}
    user closes the browser

Login with an expired invite and assert that the user is redirected to the expired invite page
    user opens the browser    %{ADMIN_URL}
    user navigates to admin frontend
    user waits until h1 is visible    Sign in    %{WAIT_MEDIUM}
    user clicks element    id:signin-button
    user logs in via identity provider
    ...    %{EXPIRED_INVITE_USER_EMAIL}
    ...    %{EXPIRED_INVITE_USER_PASSWORD}

Check that the user is on the Expired Invite page
    user waits until page contains title    Invitation expired
    user waits until page contains    Your invitation to the service has expired.
    user waits until page contains    explore.statistics@education.gov.uk

Check that the expired invite is now removed
    user navigates to admin frontend
    user waits until h1 is visible    Sign in    %{WAIT_MEDIUM}
    user clicks element    id:signin-button

    # Currently, users attempting to login with no existing user record or user invite
    # are simply redirected to the "Sign in" page - the user is not prompted to log into
    # the Identity Provider again as they have already confirmed their correct login
    # credentials there and are therefore sent straight back to the service.
    user waits until page contains title    Sign in
    user waits until page contains    Use this service to create
