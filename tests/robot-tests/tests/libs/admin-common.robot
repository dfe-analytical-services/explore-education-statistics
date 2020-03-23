*** Settings ***
Resource    ./common.robot
Library     admin-utilities.py

*** Keywords ***
User selects theme "${theme}" and topic "${topic}" from the admin dashboard
    user clicks element   css:#my-publications-tab
    user waits until page contains element   css:#selectTheme
    user checks element contains  css:#my-publications-tab  Manage publications and releases
    user selects from list by label  css:#selectTheme  ${theme}
    user waits until page contains element   css:#selectTopic
    user selects from list by label  css:#selectTopic  ${topic}
    user waits until page contains element  xpath://h2[text()="${theme}"]
    user waits until page contains element  xpath://h3[text()="${topic}"]

user creates publication
    [Arguments]   ${title}   ${methodology}   ${contact}
    user waits until page contains heading    Create new publication
    user enters text into element  css:#createPublicationForm-publicationTitle   ${title}
    user selects radio     Choose an existing methodology
    user checks element is visible    xpath://label[text()="Select methodology"]
    user selects from list by label  css:#createPublicationForm-selectedMethodologyId   ${methodology}
    user selects from list by label  css:#createPublicationForm-selectedContactId   ${contact}
    user clicks button   Create publication
    user waits until page contains element   xpath://span[text()="Welcome"]

User creates a new release for publication "${publication}" for start year "${startYear}"
    user waits until page contains heading   Create new release
    user waits until page contains element   xpath://h1/span[text()="${publication}"]
    ${startYearNumber} =  Convert to Integer  ${startYear}
    ${nextYearNumber} =    Evaluate    ${startYearNumber} + 1
    ${nextYear} =    Convert to String  ${nextYearNumber}
    user waits until page contains element  css:#releaseSummaryForm-timePeriodCoverage
    user selects from list by label  css:#releaseSummaryForm-timePeriodCoverage  Academic Year Q1-Q4
    user enters text into element  css:#releaseSummaryForm-timePeriodCoverageStartYear  ${startYear}
    user enters text into element  css:[id="scheduledPublishDate.day"]  30
    user enters text into element  css:[id="scheduledPublishDate.month"]  09
    user enters text into element  css:[id="scheduledPublishDate.year"]   ${startYear}
    user enters text into element  css:[id="nextReleaseDate.day"]  01
    user enters text into element  css:[id="nextReleaseDate.month"]  09
    user enters text into element  css:[id="nextReleaseDate.year"]  ${nextYear}
    user selects radio   National Statistics
    user clicks element   xpath://button[text()="Create new release"]
    user waits until page contains element  xpath://span[text()="Edit release"]
    user waits until page contains element  xpath://h2[text()="Release summary"]

user opens editable accordion
    [Arguments]   ${accordion_section_title}
    user clicks element  //span[text()="${accordion_section_title}"]

user checks draft releases tab contains publication
    [Arguments]    ${publication_name}
    user checks page contains element   xpath://*[@id="draft-releases"]//h3[text()="${publication_name}"]

user waits until draft releases tab contains publication
    [Arguments]    ${publication_name}
    user waits until page contains element   xpath://*[@id="draft-releases"]//h3[text()="${publication_name}"]

user checks draft releases tab publication has release
    [Arguments]   ${publication_name}   ${release_text}
    user checks page contains element   xpath://*[@id="draft-releases"]//*[@data-testid="releaseByStatusTab ${publication_name}"]//*[contains(@data-testid, "${release_text}")]