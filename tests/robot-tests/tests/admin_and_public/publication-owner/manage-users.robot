*** Settings ***
Resource    ../../libs/admin-common.robot

Force Tags  Admin  Local  Dev

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser


*** Variables ***
${TOPIC_NAME}       %{TEST_TOPIC_NAME}



*** Variables ***
${TOPIC_NAME}       %{TEST_TOPIC_NAME}

*** Test Cases ***
Heading is present on tab
    [Tags]  HappyPath
    user waits until element contains  id:publicationsReleases-tab  Manage publications and releases
    user clicks link  Platform administration
    Sleep  10000000