*** Settings ***
Resource    ../libs/common.robot

Force Tags  GeneralPublic  Dev  Test  Prod

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Navigate to Absence publication
    [Tags]  HappyPath  Local
    environment variable should be set  PUBLIC_URL
    user goes to url  %{PUBLIC_URL}
    user waits until page contains  Select an option to find the national and regional

    user clicks link  Find statistics and data
    user waits until page contains  Browse to find the statistics and data you’re looking for

    user opens accordion section  Pupils and schools
    user opens details dropdown   Pupil absence
    user clicks element   css:[data-testid="view-stats-pupil-absence-in-schools-in-england"]
    user waits until page contains element   xpath://h1[text()="Pupil absence in schools in England"]
    user checks url contains  %{PUBLIC_URL}/find-statistics/pupil-absence-in-schools-in-england

Go to Notify me page for Absence publication
    [Tags]  HappyPath  Local
    user clicks link  Sign up for email alerts

    user waits until page contains element  xpath://h1/span[text()="Notify me"]     180
    user waits until page contains element  xpath://h1[text()="Pupil absence in schools in England"]

    user checks element count is x      css:[data-testid="breadcrumbs--list"] li     4
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(2)   Find statistics and data
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(3)   Pupil absence in schools in England
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(4)   Notify me

Sign up for email alerts
    [Tags]  HappyPath
    user clicks element  css:#email-id
    user presses keys    mark@hiveit.co.uk
    user clicks element  css:#submit-button

    user waits until page contains heading   Subscribed
    user checks page contains  Thank you. Check your email to confirm your subscription.
