*** Settings ***
Resource    ./common.robot
Library     admin-utilities.py

*** Keywords ***
User selects theme "${theme}" and topic "${topic}" from the admin dashboard
    user clicks element   id:my-publications-tab
    user waits until page contains element   id:selectTheme
    user checks element contains  id:my-publications-tab  Manage publications and releases
    user selects from list by label  id:selectTheme  ${theme}
    user waits until page contains element   id:selectTopic
    user selects from list by label  id:selectTopic  ${topic}
    user waits until page contains heading 2  ${theme}
    user waits until page contains heading 3  ${topic}

user creates publication
    [Arguments]   ${title}
    user waits until page contains heading    Create new publication
    user waits until page contains element  id:publicationForm-title
    user enters text into element  id:publicationForm-title   ${title}
    user selects radio     No methodology
    user enters text into element  id:publicationForm-teamName        Attainment statistics team
    user enters text into element  id:publicationForm-teamEmail       Attainment.STATISTICS@education.gov.uk
    user enters text into element  id:publicationForm-contactName     Tingting Shu
    user enters text into element  id:publicationForm-contactTelNo    0123456789
    user clicks button   Save publication
    user waits until page contains element   xpath://span[text()="Welcome"]

User creates release for publication
    [Arguments]  ${publication}  ${time_period_coverage}  ${start_year}
    user waits until page contains heading   Create new release
    user waits until page contains element   xpath://h1/span[text()="${publication}"]
    user waits until page contains element  id:releaseSummaryForm-timePeriodCoverage
    user selects from list by label  id:releaseSummaryForm-timePeriodCoverage  ${time_period_coverage}
    user enters text into element  id:releaseSummaryForm-timePeriodCoverageStartYear  ${start_year}
    user selects radio   National Statistics
    user clicks button  Create new release
    user waits until page contains element  xpath://span[text()="Edit release"]
    user waits until page contains heading 2  Release summary

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

user checks scheduled releases tab contains publication
    [Arguments]    ${publication_name}
    user checks page contains element   xpath://*[@id="scheduled-releases"]//h3[text()="${publication_name}"]

user waits until scheduled releases tab contains publication
    [Arguments]    ${publication_name}
    user waits until page contains element   xpath://*[@id="scheduled-releases"]//h3[text()="${publication_name}"]

user checks scheduled releases tab publication has release
    [Arguments]   ${publication_name}   ${release_text}
    user checks page contains element   xpath://*[@id="scheduled-releases"]//*[@data-testid="releaseByStatusTab ${publication_name}"]//*[contains(@data-testid, "${release_text}")]
