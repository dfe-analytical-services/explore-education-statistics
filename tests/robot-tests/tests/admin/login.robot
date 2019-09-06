*** Settings ***
Resource    ../libs/library.robot

Force Tags  Admin

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Verify admin index page loads
    [Tags]  HappyPath
    environment variable should be set   ADMIN_URL
    user goes to url  %{ADMIN_URL}
    user waits until page contains heading     Sign-in

Verify user can sign in
    [Tags]   HappyPath
    user signs in