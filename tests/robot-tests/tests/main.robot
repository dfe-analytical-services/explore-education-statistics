*** Settings ***
Documentation  DFE-115 Setup automated testing framework

Resource    libs/keywords_and_variables.robot

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Validate Schools page displays links to publications
    [Tags]  HappyPath
    user goes to url  ${url}
    user clicks element  css:[data-testid="themes-link"]
    user clicks element  css:[data-testid="schools"]

    user waits until page contains element  css:[data-testid="contentitemlist--element"]
    css should match x times  css:[data-testid="contentitemlist--element"]  6

Validate that Pupil Absence in Schools in England page is for latest data
    [Tags]  HappyPath
    user clicks element  css:[data-testid="absence-and-exclusions"]
    user clicks element  css:[data-testid="pupil-absence-in-schools-in-england"]
    verify element should contain  css:[data-testid="release-name"]  (latest data)

Validate 2015/2016 isn't labelled as latest data
    [Tags]  HappyPath
    user clicks element  css:[data-testid="see-previous-releases"]
    user clicks link  2015 to 2016
    verify page should not contain element  css:[data-testid="latest-data-heading"]
    verify element should not contain  css:[data-testid="release-name"]  (latest data)
