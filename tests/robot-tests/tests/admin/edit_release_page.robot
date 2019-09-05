*** Settings ***
Resource    ../libs/library.robot

Force Tags  Admin

Suite Setup       user signs in
Suite Teardown    user closes the browser

*** Test Cases ***
User selects theme and topic from dropdowns
    [Tags]  HappyPath
    user clicks element   css:#my-publications-tab
    user waits until page contains element   css:#selectTheme
    user selects from list by label  css:#selectTheme  Automated Test Theme
    user selects from list by label  css:#selectTopic  Automated Test Topic
    user checks page contains accordion  Automated Test Publication for Edit Release
    user checks accordion section contains text  Automated Test Publication for Edit Release    Methodology
    user checks accordion section contains text  Automated Test Publication for Edit Release    Releases

User clicks edit release
    [Tags]  HappyPath
    user clicks element   css:#publications-2-heading
    user waits until page contains element  xpath://dt[text()="Releases"]
    user clicks element  css:#publications-2-content [data-testid="details--expand"]
    user waits until page contains element  xpath://a[text()="Edit this release"]
    user clicks link  Edit this release

Validate release summary tab has correct details
    [Tags]  HappyPath
    user waits until page contains heading  Automated Test Publication for Edit Release
    user waits until page contains element   xpath://dt[text()="Publication title"]
    user waits until page contains element   xpath://dt[text()="Time period"]
    user waits until page contains element   xpath://dt[text()="Release period"]
    user waits until page contains element   xpath://dt[text()="Lead statistician"]
    user waits until page contains element   xpath://dt[text()="Scheduled release"]
    user waits until page contains element   xpath://dt[text()="Next release expected"]
    user waits until page contains element   xpath://dt[text()="Release type"]

User clicks Edit release setup details, checks details and cancels
    [Tags]  HappyPath
    user clicks link  Edit release setup details
    user waits until page contains element   xpath://h2[text()="Edit release summary"]
    user checks element attribute value should be  css:#releaseSummaryForm-timePeriodCoverageStartYear   value   2018
    user checks radio option should be  releaseSummaryForm-releaseTypeId  Official Statistics
    user clicks link  Cancel update
    user waits until page contains element   xpath://h2[text()="Release summary"]

User clicks Edit release setup details, changes details and updates
    [Tags]  HappyPath  AltersData
    user waits until page contains element  xpath://a[text()="Edit release setup details"]
    user clicks link  Edit release setup details
