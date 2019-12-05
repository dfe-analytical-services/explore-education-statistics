*** Settings ***
Resource    ../libs/admin-common.robot

Force Tags  Admin  Dev  Test  Failing

Suite Setup       user signs in
Suite Teardown    user closes the browser

*** Test Cases ***
Verify correct data is shown when theme and topic is shown
    [Tags]   HappyPath
    user selects theme "Test Theme" and topic "Automated Test Topic" from the admin dashboard

    user checks page contains accordion  Automated Test Publication for Create Release
    user opens accordion section  Automated Test Publication for Create Release
    user checks accordion section contains text  Automated Test Publication for Create Release    Methodology
    user checks accordion section contains text  Automated Test Publication for Create Release    Releases

User clicks create new release
    [Tags]  HappyPath
    user waits until page contains element  css:[data-testid="Create new release link for Automated Test Publication for Create Release"]
    user clicks element  css:[data-testid="Create new release link for Automated Test Publication for Create Release"]

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