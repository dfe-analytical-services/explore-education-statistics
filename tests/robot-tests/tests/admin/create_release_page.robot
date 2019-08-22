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
    select from list by label  css:#selectTheme  School and college outcomes and performance
    select from list by label  css:#selectTopic  Outcome based success measures
    user checks page contains accordion  Further education outcome-based success measures
    user checks accordion section contains text  Further education outcome-based success measures    Methodology
    user checks accordion section contains text  Further education outcome-based success measures    Releases
    
    
User clicks create new release
   [Tags]  HappyPath
   user clicks element   css:#my-publications-tab
   user clicks element   css:#publications-1-heading
   user waits until page contains element  xpath://a[text()="Create new publication"]
   user clicks element   xpath://a[text()="Create new release"]
   
Check page has correct fields
    user waits until page contains element  css:#releaseSummaryForm-timePeriodCoverage
    user waits until page contains element  css:#releaseSummaryForm-timePeriodCoverageStartYear
    user waits until page contains element  scheduledPublishDate.day
    user waits until page contains element  scheduledPublishDate.month
    user waits until page contains element  scheduledPublishDate.year
    user waits until page contains element  nextReleaseDate.day
    user waits until page contains element  nextReleaseDate.month
    user waits until page contains element  nextReleaseDate.year
    user waits until page contains element  releaseTypeId
    
User fills in form
   [Tags]  HappyPath
   select from list by label  css:#releaseSummaryForm-timePeriodCoverage  Academic year Q1 to Q4 - AYQ1Q4
   user clicks element  css:#releaseSummaryForm-timePeriodCoverageStartYear
   user presses keys  2019
   user clicks element  scheduledPublishDate.day
   user presses keys  24
   user clicks element  scheduledPublishDate.month
   user presses keys  10
   user clicks element  scheduledPublishDate.year
   user presses keys  2020
   user clicks element  nextReleaseDate.day
   user presses keys  25
   user clicks element  nextReleaseDate.month
   user presses keys  10
   user clicks element  nextReleaseDate.year
   user presses keys  2020
   user clicks element  releaseTypeId
   
   
   
Check if data has been submitted
    [Tags]  HappyPath  UnderConstruction
    user clicks element   xpath://button[text()="Create new release"]
    user waits until page contains element  xpath://span[text()="Edit release"]
    
   
