*** Settings ***
Resource            ../../../libs/admin-common.robot
Resource            ../../../libs/common.robot
Resource            ../../../libs/bootstrap_data/bootstrap_data_constants.robot
Resource            ../../../libs/admin/analyst/role_ui_permissions/role_ui_permissions_common.robot

Suite Setup         user signs in as analyst1
Suite Teardown      user closes the browser

Force Tags          Admin    Local    Dev

*** Variables ***
${ROLE_NAME_UNDER_TEST}=    Release Viewer

*** Test Cases ***
Import permissions test variables
    [Tags]    HappyPath
    Import bootstrap data roles and permissions variables
    Set suite variable    ${PUBLICATION_NAME}    ${PUBLICATION_FOR_RELEASE_VIEWER}

Navigate to Release where analyst has ${ROLE_NAME_UNDER_TEST} role
    [Tags]    HappyPath
    ${publication_accordion}=    user opens publication on the admin dashboard    ${PUBLICATION_NAME}    ${THEME_NAME}
    ...    ${TOPIC_NAME}
    Set suite variable    ${publication_accordion}

Check ${ROLE_NAME_UNDER_TEST} cannot create a Methodology for a Publication if they don't have Publication Owner role
    [Tags]    HappyPath
    user cannot see the create methodologies controls for publication    ${publication_accordion}

Check ${ROLE_NAME_UNDER_TEST} cannot create an amendment of a published release
    [Tags]    HappyPath
    ${details}=    user gets details content element    ${PUBLISHED_RELEASE_TYPE} (Live - Latest release)
    ...    ${publication_accordion}    30
    user cannot see the create amendment controls for release    ${details}

Check ${ROLE_NAME_UNDER_TEST} cannot edit the release status of a Draft Release
    [Tags]    HappyPath
    user navigates to readonly release summary from admin dashboard    ${PUBLICATION_NAME}
    ...    ${DRAFT_RELEASE_TYPE} (not Live)    ${THEME_NAME}    ${TOPIC_NAME}
    user cannot see the edit release status controls for release
