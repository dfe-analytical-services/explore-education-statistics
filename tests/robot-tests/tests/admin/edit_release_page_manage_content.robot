*** Settings ***
Resource    ./libs/common-keywords.robot

Force Tags  Admin  NotAgainstProd  AltersData

Suite Setup       user signs in
Suite Teardown    user closes the browser

*** Test Cases ***
User creates a new release for use in this test
    [Tags]  HappyPath
    user selects theme "Test Theme" and topic "Automated Test Topic" from the admin dashboard
    user creates a new release for publication "Automated Test Publication for Edit Release" for start year "2017"

User selects the Manage Content section
    [Tags]  HappyPath
    user clicks link  Manage content
    user waits until page contains  Set page view
    user checks radio option for "pageMode" should be "Add / view comments and edit content"
    user checks page contains  Academic Year Q1-Q4 2017/18
    user checks page contains heading 1  Automated Test Publication for Edit Release
    user checks page contains tag  Draft
    user checks summary list item "Publish date" should be "30 September 2017"
    user checks summary list item "Next update" should be "01 September 2018"
