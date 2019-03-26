*** Settings ***
Resource    ../libs/library.robot

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Verify Public Page Loads
    [Tags]  HappyPath
    user goes to url  ${url}
    user checks element contains  css:body   Explore education statistics

Validate names of cookies appear on Cookie Page
    [Tags]  HappyPath Failing
    user clicks link   Cookies
    cookie names should be on page

    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(2)   Cookies
