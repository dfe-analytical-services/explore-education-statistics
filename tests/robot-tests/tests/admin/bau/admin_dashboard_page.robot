*** Settings ***
Resource            ../../libs/admin-common.robot

Force Tags          Admin    Local    Dev

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

*** Variables ***
${TOPIC_NAME}       %{TEST_TOPIC_NAME}

*** Test Cases ***
Heading is present on tab
    user waits until element contains    id:publicationsReleases-tab    Manage publications and releases

Correct information is shown on tabs
    user clicks element    id:draft-releases-tab
    user waits until element contains    id:draft-releases-tab    View draft releases
    user clicks element    id:scheduled-releases-tab
    user waits until element contains    id:scheduled-releases-tab    View scheduled releases

Verify correct data is shown when theme and topic is shown
    user clicks element    id:publicationsReleases-tab
    user chooses select option    id:publicationsReleases-themeTopic-themeId    %{TEST_THEME_NAME}

    user chooses select option    id:publicationsReleases-themeTopic-topicId    ${TOPIC_NAME}
    user waits until page contains element    xpath://h2[text()="%{TEST_THEME_NAME}"]/../h3[text()="${TOPIC_NAME}"]
