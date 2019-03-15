*** Settings ***
Resource    ../libs/library.robot

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Verify Public Page Loads
    [Tags]  HappyPath
    user goes to url  ${url}
    user checks element contains  css:body   Explore education statistics