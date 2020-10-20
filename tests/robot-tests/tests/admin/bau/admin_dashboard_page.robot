*** Settings ***
Resource    ../../libs/admin-common.robot

Force Tags  Admin  Local  Dev

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${TOPIC_NAME}       %{TEST_TOPIC_NAME}

*** Test Cases ***
Heading is present on tab
    [Tags]  HappyPath
    user waits until element contains  id:publicationsReleases-tab  Manage publications and releases

Correct information is shown on tabs
    [Tags]   HappyPath
    user clicks element   id:draft-releases-tab
    user waits until element contains  id:draft-releases-tab  View draft releases
    user clicks element   id:scheduled-releases-tab
    user waits until element contains  id:scheduled-releases-tab  View scheduled releases

Verify correct data is shown when theme and topic is shown
    [Tags]   HappyPath
    user clicks element   id:publicationsReleases-tab
    user waits until page contains element  id:publicationsReleases-tab
    user selects from list by label  id:publicationsReleases-themeTopic-themeId  Test theme

    # EES-892 - Selecting theme or topic refreshes the page, so must wait
    user waits until page contains element   id:publicationsReleases-themeTopic-topicId

    user selects from list by label  id:publicationsReleases-themeTopic-topicId  ${TOPIC_NAME}
    user waits until page contains element   xpath://h2[text()="Test theme"]/../h3[text()="${TOPIC_NAME}"]
