*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev


*** Test Cases ***
Login with a user without any invite or user record and assert that the user is redirected to the No Invitation page
    user navigates to admin homepage
    user waits until h1 is visible    Sign in
    user clicks element    id:signin-button
    user logs in via identity provider
    ...    %{NO_INVITE_USER_EMAIL}
    ...    %{NO_INVITE_USER_PASSWORD}

Check that the user is on the No Invitation page
    user waits until page contains title    No invitation
    user waits until page contains    You do not have an invitation to the service.
    user waits until page contains    explore.statistics@education.gov.uk
