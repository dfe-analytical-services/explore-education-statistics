*** Settings ***
Resource    ../libs/library.robot

Force Tags  Admin

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Verify Admin Page Loads
    [Tags]  HappyPath
    user goes to url  ${urlAdmin}
    user checks element contains  css:body   Education statistics publisher