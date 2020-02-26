*** Settings ***
Resource    ../../libs/common.robot

Force Tags  Admin  Local  Dev

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Test Cases ***
Heading is present on tab
    [Tags]  HappyPath
    user checks element contains  css:#my-publications-tab  Manage publications and releases

Correct information is shown on tabs
    [Tags]   HappyPath
    user clicks element   css:#draft-releases-tab
    user checks element contains  css:#draft-releases-tab  View draft releases
    user clicks element   css:#scheduled-releases-tab
    user checks element contains  css:#scheduled-releases-tab  View scheduled releases

Verify correct data is shown when theme and topic is shown
    [Tags]   HappyPath
    user clicks element   css:#my-publications-tab
    user waits until page contains element  css:#selectTheme
    user selects from list by label  css:#selectTheme  Test theme

    # EES-892 - Selecting theme or topic refreshes the page, so must wait
    user waits until page contains element   css:#selectTopic

    environment variable should be set   RUN_IDENTIFIER
    user selects from list by label  css:#selectTopic  UI test topic %{RUN_IDENTIFIER}
    user waits until page contains element   xpath://h2[text()="Test theme"]/../h3[text()="UI test topic %{RUN_IDENTIFIER}"]

