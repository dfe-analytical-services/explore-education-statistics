*** Settings ***
Resource    ../libs/library.robot

Force Tags  Admin  NotAgainstProd

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
    user checks page contains accordion  Automated Test Publication for Edit Release
    user opens accordion section  Automated Test Publication for Edit Release
    user checks page contains details section  Academic Year, 2018 to 2019 (not Live)
    user opens details section  Academic Year, 2018 to 2019 (not Live)
    user waits until page contains element  css:[data-testid="Edit release link for Automated Test Publication for Edit Release, Academic Year, 2018 to 2019 (not Live)"]
    user clicks element  css:[data-testid="Edit release link for Automated Test Publication for Edit Release, Academic Year, 2018 to 2019 (not Live)"]

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

User clicks Edit release summary, checks details and cancels
    [Tags]  HappyPath
    user clicks link  Edit release summary
    user waits until page contains element   xpath://h2[text()="Edit release summary"]
    user checks element attribute value should be  css:#releaseSummaryForm-timePeriodCoverageStartYear   value   2018
    user checks radio option should be  releaseSummaryForm-releaseTypeId  Official Statistics
    user clicks link  Cancel update
    user waits until page contains element   xpath://h2[text()="Release summary"]

User clicks Edit release summary, changes details and updates
    [Tags]  HappyPath  AltersData
    user waits until page contains link  Edit release summary
    user clicks link  Edit release summary
