*** Settings ***
Resource    ../libs/library.robot

Force Tags  Admin  NotAgainstProd

Suite Setup       user signs in
Suite Teardown    user closes the browser

*** Test Cases ***
Heading is present on tab
    [Tags]  HappyPath
    user checks element contains  css:#my-publications-tab  Manage publications and releases

Verify correct data is shown when theme and topic is shown
    [Tags]   HappyPath
    user clicks element   css:#my-publications-tab
    user selects from list by label  css:#selectTheme  Automated Test Theme
    user selects from list by label  css:#selectTopic  Automated Test Topic
    user checks page contains accordion  Automated Test Publication for Create Release
    user opens accordion section  Automated Test Publication for Create Release
    user checks accordion section contains text  Automated Test Publication for Create Release    Methodology
    user checks accordion section contains text  Automated Test Publication for Create Release    Releases

User clicks create new release
    [Tags]  HappyPath
    user waits until page contains link  Create new release
    user clicks link  Create new release
   
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
    [Tags]  HappyPath  AltersData
    user clicks element   xpath://button[text()="Create new release"]
    user waits until page contains element  xpath://span[text()="Edit release"]
    user waits until page contains element  xpath://h2[text()="Release summary"]

