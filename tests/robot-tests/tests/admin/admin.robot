*** Settings ***
Resource    ../libs/library.robot

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Verify Admin Page Loads
    [Tags]  HappyPath
    user goes to url  ${urlAdmin}
    user checks page contains element  css:h1
