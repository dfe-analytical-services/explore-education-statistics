*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev


*** Test Cases ***
Log in as BAU and check the dashboard is looking OK and the sign out buttons are available
    user navigates to admin homepage
    user waits until h1 is visible    Sign in
    user clicks element    id:signin-button
    user logs in via identity provider
    ...    %{ADMIN_EMAIL}
    ...    %{ADMIN_PASSWORD}
    user waits until page contains title    Dashboard
    user waits until page contains    Welcome Bau1
    user waits until page contains    Logged in as Bau1

Log out using the button in the page header
    user clicks element    testid:header-sign-out-button
    user waits until page contains title    Signed out

Check that logging out via the header link has successfully removed the ability to access the dashboard
    user navigates to admin homepage
    user waits until page contains title    Sign in

Log in again and log out using the button in the dashboard
    user clicks element    id:signin-button
    user logs in via identity provider
    ...    %{ADMIN_EMAIL}
    ...    %{ADMIN_PASSWORD}
    ...    True
    user waits until page contains title    Dashboard
    user waits until page contains element    testid:dashboard-sign-out-button
    user clicks element    testid:dashboard-sign-out-button
    user waits until page contains title    Signed out

Check that logging out with the dashboard link has successfully removed the ability to access the dashboard
    user navigates to admin homepage
    user waits until page contains title    Sign in

Switch users in the same browser
    user clicks element    id:signin-button
    user logs in via identity provider
    ...    %{ADMIN_EMAIL}
    ...    %{ADMIN_PASSWORD}
    ...    True

    # This is testing our optimised user switching behaviour where we reuse access tokens for each user
    # to log in rather than carrying out a full login flow via the Identity Provider.
    user changes to analyst1
    user changes to bau1
    user changes to analyst1
    user changes to bau1
