*** Settings ***
Resource    ../libs/keywords_and_variables.robot

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Validate Home page breadcrumbs
    [Tags]  HappyPath
    user goes to url  ${url}
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home
    css should match x times  css:[data-testid="breadcrumbs--list"] li   1

Validate Theme page breadcrumbs
    [Tags]  HappyPath
    user clicks element  css:[data-testid="home-page--themes-link"]
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(2)  Themes

    css should match x times  css:[data-testid="breadcrumbs--list"] li  2

    user clicks link    Home
    user waits until page contains element  css:[data-testid="home-page--heading"]
    css should match x times  css:[data-testid="breadcrumbs--list"] li   1

    user clicks element  css:[data-testid="home-page--themes-link"]
    user waits until page contains    Find themes
    css should match x times  css:[data-testid="breadcrumbs--list"] li   2

Validate Schools page breadcrumbs
    [Tags]  HappyPath
    user clicks element  css:[data-testid="content-item-list--schools"]
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(2)   Themes
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(3)   Schools
    css should match x times  css:[data-testid="breadcrumbs--list"] li   3

    user clicks link    Home
    user waits until page contains element  css:[data-testid="home-page--heading"]
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home
    css should match x times  css:[data-testid="breadcrumbs--list"] li   1
    user goes back

    user clicks link    Themes
    user waits until page contains    Find themes
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(2)   Themes
    css should match x times  css:[data-testid="breadcrumbs--list"] li   2
    user goes back

Validate that Pupil Absence in Schools in England page breadcrumbs
    [Tags]  HappyPath
    user clicks element  css:[data-testid="content-item-list--absence-and-exclusions"]
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(2)   Themes
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(3)   Schools
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(4)   Absence and exclusions
    css should match x times  css:[data-testid="breadcrumbs--list"] li   4

    user clicks element  css:[data-testid="content-item-list--pupil-absence-in-schools-in-england"]
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(2)   Themes
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(3)   Schools
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(4)   Absence and exclusions
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(5)   Pupil absence in schools in england
    css should match x times  css:[data-testid="breadcrumbs--list"] li   5

    user clicks link    Absence and exclusions
    user waits until page contains  Find absence and exclusions statistics
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(2)   Themes
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(3)   Schools
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(4)   Absence and exclusions
    css should match x times  css:[data-testid="breadcrumbs--list"] li   4
    user goes back
    css should match x times  css:[data-testid="breadcrumbs--list"] li   5

    user clicks link    Schools
    user waits until page contains  Find schools statistics
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(2)   Themes
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(3)   Schools
    css should match x times  css:[data-testid="breadcrumbs--list"] li   3
    user goes back

    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(2)   Themes
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(3)   Schools
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(4)   Absence and exclusions
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(5)   Pupil absence in schools in england
    css should match x times  css:[data-testid="breadcrumbs--list"] li   5

