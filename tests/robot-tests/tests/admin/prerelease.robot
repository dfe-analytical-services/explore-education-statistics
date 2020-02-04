*** Settings ***
Resource    ../libs/admin-common.robot

Force Tags  Admin  Local  Dev

Suite Setup       user signs in
Suite Teardown    user closes the browser

*** Test Cases ***
Create prerelease test publication
    [Tags]  HappyPath
    environment variable should be set   RUN_IDENTIFIER
    user selects theme "Test theme" and topic "UI test topic %{RUN_IDENTIFIER}" from the admin dashboard
    user waits until page contains element    xpath://a[text()="Create new publication"]     60
    user clicks link  Create new publication
    user creates publication  Prerelease test %{RUN_IDENTIFIER}   Test methodology    Sean Gibson

Verify Prerelease test publication is created
    [Tags]  HappyPath
    user checks page contains accordion  Prerelease test %{RUN_IDENTIFIER}
    user opens accordion section  Prerelease test %{RUN_IDENTIFIER}
    user checks accordion section contains text  Prerelease test %{RUN_IDENTIFIER}    Methodology
    user checks accordion section contains text  Prerelease test %{RUN_IDENTIFIER}    Releases

Create release
    [Tags]  HappyPath
    user clicks element  css:[data-testid="Create new release link for Prerelease test %{RUN_IDENTIFIER}"]
    user creates a new release for publication "Prerelease test %{RUN_IDENTIFIER}" for start year "2015"
    user checks summary list item "Publication title" should be "Prerelease test %{RUN_IDENTIFIER}"

Add content to release
    [Tags]  HappyPath
    user clicks element  xpath://a[text()="Manage content"]
    user waits until page contains heading  Prerelease test %{RUN_IDENTIFIER}
    user clicks element  xpath://button[text()="Add content"]
    user waits until page contains element  xpath://p[text()="This section is empty"]
    user clicks element   xpath://span[text()="Edit"]
    user presses keys  Test intro text for Prerelease test %{RUN_IDENTIFIER}

Has it worked?
    [Tags]  HappyPath
    sleep  100000
