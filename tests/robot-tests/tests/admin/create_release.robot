*** Settings ***
Resource    ../libs/admin-common.robot

Force Tags  Admin  Local

Suite Setup       user signs in
Suite Teardown    user closes the browser

*** Test Cases ***
Verify correct data is shown when theme and topic is shown
    [Tags]   HappyPath
    user selects theme "Test theme" and topic "UI tests topic" from the admin dashboard
    user checks page contains accordion  UI tests publication
    user opens accordion section  UI tests publication
    user checks accordion section contains text  UI tests publication    Methodology
    user checks accordion section contains text  UI tests publication    Releases
    user checks summary list item "Releases" should be "No releases created"

User clicks create new release
    [Tags]  HappyPath
    user waits until page contains element  css:[data-testid="Create new release link for UI tests publication"]
    user clicks element  css:[data-testid="Create new release link for UI tests publication"]
   
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
    user selects from list by label  css:#releaseSummaryForm-timePeriodCoverage  Spring Term
    user enters text into element  css:#releaseSummaryForm-timePeriodCoverageStartYear  2025
    user enters text into element  css:[id="scheduledPublishDate.day"]  24
    user enters text into element  css:[id="scheduledPublishDate.month"]  10
    user enters text into element  css:[id="scheduledPublishDate.year"]   2025
    user enters text into element  css:[id="nextReleaseDate.day"]  25
    user enters text into element  css:[id="nextReleaseDate.month"]  10
    user enters text into element  css:[id="nextReleaseDate.year"]  2026
    user clicks element  xpath://label[text()="National Statistics"]

Click Create new release button
    [Tags]   HappyPath
    user clicks button   Create new release
    user waits until page contains element  xpath://h1/span[text()="Edit release"]
    user checks page contains heading 1  UI tests publication

Verify Release summary
    [Tags]  HappyPath
    user checks page contains element   xpath://li/a[text()="Release summary" and contains(@class, 'current-page')]
    user checks page contains heading 2    Release summary
    user checks summary list item "Publication title" should be "UI tests publication"
    user checks summary list item "Time period" should be "Spring Term"
    user checks summary list item "Release period" should be "2025 to 2026"  # Correct?
    user checks summary list item "Lead statistician" should be "Mark Pearson"
    user checks summary list item "Scheduled release" should be "24 October 2025"
    user checks summary list item "Next release expected" should be "25 October 2026"
    user checks summary list item "Release type" should be "National Statistics"
