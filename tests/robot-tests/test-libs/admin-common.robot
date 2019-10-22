*** Settings ***
Resource    ./common.robot

*** Keywords ***
User selects theme "${theme}" and topic "${topic}" from the admin dashboard
    user clicks element   css:#my-publications-tab
    user waits until page contains element   css:#selectTheme
    user checks element contains  css:#my-publications-tab  Manage publications and releases
    user selects from list by label  css:#selectTheme  ${theme}
    user selects from list by label  css:#selectTopic  ${topic}
    user checks page contains element  xpath://h2[text()="${theme}"]
    user checks page contains element  xpath://h3[text()="${topic}"]

User creates a new release for publication "${publication}" for start year "${startYear}"
    ${startYearNumber} =  Convert to Integer  ${startYear}
    ${nextYearNumber} =    Evaluate    ${startYearNumber} + 1
    ${nextYear} =    Convert to String  ${nextYearNumber}
    user checks page contains accordion  ${publication}
    user opens accordion section  ${publication}
    user clicks element  css:[data-testid="Create new release link for ${publication}"]
    user waits until page contains element  css:#releaseSummaryForm-timePeriodCoverage
    user selects from list by label  css:#releaseSummaryForm-timePeriodCoverage  Academic Year Q1-Q4
    user enters text into element  css:#releaseSummaryForm-timePeriodCoverageStartYear  ${startYear}
    user enters text into element  css:[id="scheduledPublishDate.day"]  30
    user enters text into element  css:[id="scheduledPublishDate.month"]  09
    user enters text into element  css:[id="scheduledPublishDate.year"]   ${startYear}
    user enters text into element  css:[id="nextReleaseDate.day"]  01
    user enters text into element  css:[id="nextReleaseDate.month"]  09
    user enters text into element  css:[id="nextReleaseDate.year"]  ${nextYear}
    user clicks element  xpath://label[text()="National Statistics"]
    user clicks element   xpath://button[text()="Create new release"]
    user waits until page contains element  xpath://span[text()="Edit release"]