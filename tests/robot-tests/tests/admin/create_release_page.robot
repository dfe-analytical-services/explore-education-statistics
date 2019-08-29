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
    user clicks link   Sign-in

    environment variable should be set   ADMIN_EMAIL
    environment variable should be set   ADMIN_PASSWORD
    user logs into microsoft online  %{ADMIN_EMAIL}   %{ADMIN_PASSWORD}

    user checks url contains  %{ADMIN_URL}
    user waits until page contains heading   User1 EESADMIN
    user checks element should contain    css:[data-testid="breadcrumbs--list"] li:nth-child(1)     Home
    user checks element should contain    css:[data-testid="breadcrumbs--list"] li:nth-child(2)     Administrator dashboard

Heading is present on tab
    [Tags]  HappyPath
    user checks element contains  css:#my-publications-tab  Manage publications and releases

Verify correct data is shown when theme and topic is shown
    [Tags]   HappyPath
    user clicks element   css:#my-publications-tab
    user selects from list by label  css:#selectTheme  School and college outcomes and performance
    user selects from list by label  css:#selectTopic  Outcome based success measures
    user checks page contains accordion  Further education outcome-based success measures
    user opens accordion section  Further education outcome-based success measures
    user checks accordion section contains text  Further education outcome-based success measures    Methodology
    user checks accordion section contains text  Further education outcome-based success measures    Releases

User clicks create new release
    [Tags]  HappyPath
    user waits until page contains element  xpath://a[text()="Create new release"]
    user clicks element   xpath://a[text()="Create new release"]
   
Check page has correct fields
    [Tags]  HappyPath
    user waits until page contains element  css:#releaseSummaryForm-timePeriodCoverage
    user waits until page contains element  css:#releaseSummaryForm-timePeriodCoverageStartYear
    user waits until page contains element  css:[id="scheduledPublishDate.day"]
    user waits until page contains element  css:[id="scheduledPublishDate.month"]
    user waits until page contains element  css:[id="scheduledPublishDate.year"]
    user waits until page contains element  css:[id="nextReleaseDate.day"]
    user waits until page contains element  css:[id="nextReleaseDate.month"]
    user waits until page contains element  css:[id="nextReleaseDate.year"]

User fills in form
    [Tags]  HappyPath
    user selects from list by label  css:#releaseSummaryForm-timePeriodCoverage  Academic Year Q1-Q4
    user enters text into element  css:#releaseSummaryForm-timePeriodCoverageStartYear  2019
    user enters text into element  css:[id="scheduledPublishDate.day"]  24
    user enters text into element  css:[id="scheduledPublishDate.month"]  10
    user enters text into element  css:[id="scheduledPublishDate.year"]   2030
    user enters text into element  css:[id="nextReleaseDate.day"]  25
    user enters text into element  css:[id="nextReleaseDate.month"]  10
    user enters text into element  css:[id="nextReleaseDate.year"]  2030
    user clicks element  xpath://label[text()="National Statistics"]

Check if data has been submitted
    [Tags]  HappyPath  UnderConstruction
    user clicks element   xpath://button[text()="Create new release"]
    user waits until page contains element  xpath://span[text()="Edit release"]
