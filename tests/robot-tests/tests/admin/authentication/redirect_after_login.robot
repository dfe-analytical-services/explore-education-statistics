*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev


*** Test Cases ***
Check that a user is redirected to a protected page after being forced to sign in
    user navigates to    %{ADMIN_URL}/administration
    user waits until h1 is visible    Sign in
    user clicks element    id:signin-button
    user logs in via identity provider
    ...    %{ADMIN_EMAIL}
    ...    %{ADMIN_PASSWORD}
    user waits until page contains title    Platform administration

Check that a user is redirected to the dashboard if coming directly from the Sign In page
    user closes the browser
    user opens the browser
    user navigates to    %{ADMIN_URL}/sign-in
    user waits until h1 is visible    Sign in
    user clicks element    id:signin-button
    user logs in via identity provider
    ...    %{ADMIN_EMAIL}
    ...    %{ADMIN_PASSWORD}
    user waits until page contains title    Dashboard

Check that a user is redirected to the dashboard if coming from the dashboard originally
    user closes the browser
    user opens the browser
    user navigates to admin homepage
    user waits until h1 is visible    Sign in
    user clicks element    id:signin-button
    user logs in via identity provider
    ...    %{ADMIN_EMAIL}
    ...    %{ADMIN_PASSWORD}
    user waits until page contains title    Dashboard

Check that a user is shown a Forbidden page if redirected to a protected page that they cannot access after login
    user closes the browser
    user opens the browser
    user navigates to    %{ADMIN_URL}/administration
    user waits until h1 is visible    Sign in
    user clicks element    id:signin-button
    user logs in via identity provider
    ...    %{ANALYST_EMAIL}
    ...    %{ANALYST_PASSWORD}
    user waits until page contains title    Forbidden

Check that a user is shown a Page Not Found page if redirected to a non-existent page after login
    user closes the browser
    user opens the browser
    user navigates to    %{ADMIN_URL}/sign-in?returnUrl=%2Fnon-existent-page
    user waits until h1 is visible    Sign in
    user clicks element    id:signin-button
    user logs in via identity provider
    ...    %{ADMIN_EMAIL}
    ...    %{ADMIN_PASSWORD}
    user waits until page contains title    Page not found
