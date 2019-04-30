*** Settings ***
Resource    ../libs/library.robot

Force Tags  GeneralPublic

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Verify Public Page Loads
    [Tags]  HappyPath
    user goes to url  ${url}
    user checks element contains  css:body   Explore education statistics

Validate names of cookies appear on Cookie Page
    [Tags]  HappyPath
    user clicks link   Cookies
    user waits until page contains element   css:[data-testid="page-title Cookies"]
    user waits until page contains  How cookies are used on this service
    cookie names should be on page

    ${current_url}=  get location
    should be equal  ${current_url}   ${url}/cookies

    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(2)   Cookies

Validate Privacy Policy page
    [Tags]  HappyPath
    user clicks link    Privacy policy
    user waits until page contains  Explore education statistics privacy policy
    user waits until page contains  The explore education statistics service is operated by the Department for Education

    ${current_url}=  get location
    should be equal  ${current_url}   ${url}/privacy-policy

    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(2)   Privacy Policy

Validate Feedback page
    [Tags]  HappyPath
    user clicks link  feedback
    user waits until page contains      Explore Education Statistics - Beta banner feedback survey
