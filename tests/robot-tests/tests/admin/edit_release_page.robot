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
    user waits until page contains element    xpath://h1[text()="Sign-in"]

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
    
    
User selects theme and topic from dropdowns
    [Tags]  HappyPath
    user clicks element   css:#my-publications-tab     
    select from list by label  css:#selectTheme  School and college outcomes and performance
    select from list by label  css:#selectTopic  Outcome based success measures
    user checks accordion section contains text  Further education outcome-based success measures    Methodology
    user checks accordion section contains text  Further education outcome-based success measures    Releases


User clicks edit release
   [Tags]  HappyPath
   user clicks element   css:#publications-1-heading
   user waits until page contains element  xpath://dt[text()="Releases"]
   user clicks element  css:[data-testid="details--expand"]
   user waits until page contains element  xpath://a[text()="Edit this release"]
   user clicks element   xpath://a[text()="Edit this release"]
   
   
Validate release summary tab has correct details
    [Tags]  HappyPath
    user waits until page contains heading  Further education outcome-based success measures
    user waits until page contains element   xpath://dt[text()="Publication title"]
    user waits until page contains element   xpath://dt[text()="Time period"]
    user waits until page contains element   xpath://dt[text()="Release period"]
    user waits until page contains element   xpath://dt[text()="Lead statistician"]
    user waits until page contains element   xpath://dt[text()="Scheduled release"]
    user waits until page contains element   xpath://dt[text()="Next release expected"]
    user waits until page contains element   xpath://dt[text()="Release type"]
    

User clicks Edit release setup details         
    user clicks element  xpath://a[text()="Edit release setup details"]
    user waits until page contains element   xpath://h2[text()="Edit release setup"]
    get element attribute  css:#releaseSummaryForm-timePeriodCoverageStartYear  2014
    user waits until page contains element  xpath://label[text()="Ad Hoc"]
    user clicks element   xpath://button[text()="Update release status"]
    user clicks element  xpath://a[text()="Edit release setup details"]
    user clicks element  xpath://a[text()="Cancel update"]
    
        
        
