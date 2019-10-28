*** Settings ***
Resource    ../libs/admin-common.robot

Force Tags  Admin  Local

Suite Setup       user signs in
Suite Teardown    user closes the browser

*** Test Cases ***
Verify correct data is shown when theme and topic is shown
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "UI tests topic" from the admin dashboard
    user checks page contains accordion  UI tests - edit release
    user opens accordion section  UI tests - edit release
    user checks accordion section contains text  UI tests - edit release    Methodology
    user checks accordion section contains text  UI tests - edit release    Releases

User clicks edit release
    [Tags]  HappyPath
    user checks page contains details section  Academic Year, 2030 to 2031 (not Live)
    user opens details section  Academic Year, 2030 to 2031 (not Live)
    user waits until page contains element  css:[data-testid="Edit release link for UI tests - edit release, Academic Year, 2030 to 2031 (not Live)"]
    user clicks element  css:[data-testid="Edit release link for UI tests - edit release, Academic Year, 2030 to 2031 (not Live)"]

Validate release summary tab has correct details
    [Tags]  HappyPath
    user waits until page contains element    xpath://h2[text()="Release summary"]
    user checks summary list item "Publication title" should be "UI tests - edit release"
    user checks summary list item "Time period" should be "Academic Year"
    user checks summary list item "Release period" should be "2030 to 2031"
    user checks summary list item "Lead statistician" should be "Mark Pearson"
    user checks summary list item "Scheduled release" should be "01 January 2030"
    user checks summary list item "Next release expected" should be "01 January 2031"
    user checks summary list item "Release type" should be "Ad Hoc"

User clicks Edit release summary
    [Tags]  HappyPath
    user clicks link  Edit release summary
    user waits until page contains element   xpath://h2[text()="Edit release summary"]

Validate Edit release summary values
    [Tags]  HappyPath
    list selection should be  css:#releaseSummaryForm-timePeriodCoverage   Academic Year
    user checks element attribute value should be  css:#releaseSummaryForm-timePeriodCoverageStartYear   value   2030
    user checks element attribute value should be  css:[id="scheduledPublishDate.day"]  value  1
    user checks element attribute value should be  css:[id="scheduledPublishDate.month"]  value  1
    user checks element attribute value should be  css:[id="scheduledPublishDate.year"]  value  2030
    user checks element attribute value should be  css:[id="nextReleaseDate.day"]  value  1
    user checks element attribute value should be  css:[id="nextReleaseDate.month"]  value  1
    user checks element attribute value should be  css:[id="nextReleaseDate.year"]  value  2031
    user checks radio option for "releaseSummaryForm-releaseTypeId" should be "Ad Hoc"

Update Edit release summary values
    [Tags]  HappyPath
    user selects from list by label  css:#releaseSummaryForm-timePeriodCoverage  Calendar Year
    clear element text     css:#releaseSummaryForm-timePeriodCoverageStartYear
    user enters text into element  css:#releaseSummaryForm-timePeriodCoverageStartYear  2040
    clear element text   css:[id="scheduledPublishDate.day"]
    user enters text into element  css:[id="scheduledPublishDate.day"]  2
    clear element text   css:[id="scheduledPublishDate.month"]
    user enters text into element  css:[id="scheduledPublishDate.month"]  2
    clear element text   css:[id="scheduledPublishDate.year"]
    user enters text into element  css:[id="scheduledPublishDate.year"]   2040
    clear element text   css:[id="nextReleaseDate.day"]
    user enters text into element  css:[id="nextReleaseDate.day"]  2
    clear element text   css:[id="nextReleaseDate.month"]
    user enters text into element  css:[id="nextReleaseDate.month"]  2
    clear element text   css:[id="nextReleaseDate.year"]
    user enters text into element  css:[id="nextReleaseDate.year"]  2041
    user clicks element  xpath://label[text()="Official Statistics"]

    user clicks button  Update release summary

Validate updated release summary tab has correct details
    [Tags]  HappyPath
    user waits until page contains element    xpath://h2[text()="Release summary"]
    user checks summary list item "Publication title" should be "UI tests - edit release"
    user checks summary list item "Time period" should be "Calendar Year"
    user checks summary list item "Release period" should be "2040 to 2041"
    user checks summary list item "Lead statistician" should be "Mark Pearson"
    user checks summary list item "Scheduled release" should be "02 February 2040"
    user checks summary list item "Next release expected" should be "02 February 2041"
    user checks summary list item "Release type" should be "Official Statistics"

Validate updated Edit release summary values
    [Tags]  HappyPath
    user clicks link  Edit release summary
    user waits until page contains element   xpath://h2[text()="Edit release summary"]

    list selection should be  css:#releaseSummaryForm-timePeriodCoverage   Calendar Year
    user checks element attribute value should be  css:#releaseSummaryForm-timePeriodCoverageStartYear   value   2040
    user checks element attribute value should be  css:[id="scheduledPublishDate.day"]  value  2
    user checks element attribute value should be  css:[id="scheduledPublishDate.month"]  value  2
    user checks element attribute value should be  css:[id="scheduledPublishDate.year"]  value  2040
    user checks element attribute value should be  css:[id="nextReleaseDate.day"]  value  2
    user checks element attribute value should be  css:[id="nextReleaseDate.month"]  value  2
    user checks element attribute value should be  css:[id="nextReleaseDate.year"]  value  2041
    user checks radio option for "releaseSummaryForm-releaseTypeId" should be "Official Statistics"
