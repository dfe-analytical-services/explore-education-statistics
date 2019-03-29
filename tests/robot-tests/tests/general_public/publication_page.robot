*** Settings ***
Documentation   DFE-115 Setup automated testing framework
...             DFE-98 General Public - 'Where Does The Data Come From?'
...             DFE-100 General Public - See All (4) Updates
...             DFE-101 General Public - See Previous Year/s
...             DFE-102 General Public - Getting the data
...             DFE-103 General Public - Publication Page

Resource    ../libs/library.robot

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Navigate to Absence publication
    [Tags]  HappyPath
    user goes to url  ${url}
    user clicks link  Find statistics and data
    user waits until page contains  Browse to find the statistics and data you’re looking for

    user clicks element   css:[data-testid="Absence and exclusions"]
    element attribute value should be  css:[data-testid="Absence and exclusions"]   aria-expanded   true

    user clicks element   css:[data-testid="view-stats-pupil-absence-in-schools-in-england"]
    user waits until page contains  Pupil absence data and statistics for schools in England

Validate URL
    [Documentation]  DFE-325
    [Tags]  HappyPath
    ${current_url}=  get location
    should be equal   ${current_url}   ${url}/statistics/pupil-absence-in-schools-in-england

Validate Key Statistics data block
    [Tags]  HappyPath
    user checks element contains  css:[data-testid="tile Overall absence rate       4.7%
    user checks element contains  css:[data-testid="tile Authorised absence rate    3.4%
    user checks element contains  css:[data-testid="tile Unauthorised absence rate  1.3%
