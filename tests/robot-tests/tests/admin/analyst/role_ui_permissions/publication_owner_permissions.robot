*** Settings ***
Resource            ../../../libs/admin-common.robot
Resource            ../../../libs/common.robot
Resource            ../../../libs/bootstrap_data/bootstrap_data_constants.robot
Resource            ../../../libs/admin/analyst/role_ui_permissions.robot

Suite Setup         user signs in as analyst1
Suite Teardown      user closes the browser

Force Tags          Admin    Local    Dev

*** Test Cases ***
Import permissions test variables
    [Tags]    HappyPath
    Import bootstrap data roles and permissions variables
    Set suite variable    ${PUBLICATION_NAME}    ${PUBLICATION_FOR_PUBLICATION_OWNER}

Navigate to Publication where analyst has Publication Owner role
    [Tags]    HappyPath
    user navigates to publication on admin dashboard    ${PUBLICATION_NAME}    ${THEME_NAME}    ${TOPIC_NAME}

Check can create a Methodology for the owned Publication
    [Tags]    HappyPath
    user can see the create methodologies controls for publication    ${publication_accordion}

Check cannot edit content for published release
    [Tags]    HappyPath
    user navigates to readonly release summary from admin dashboard    ${PUBLICATION_NAME}
    ...    ${PUBLISHED_RELEASE_TYPE} (Live - Latest release)    ${THEME_NAME}    ${TOPIC_NAME}
    user cannot see edit controls for release content    ${PUBLICATION_NAME}

Navigate back to admin dashboard for publication
    [Tags]    HappyPath
    user navigates to publication on admin dashboard    ${PUBLICATION_NAME}    ${THEME_NAME}    ${TOPIC_NAME}

Check can create an amendment of a published release
    [Tags]    HappyPath
    ${details}=    user gets details content element    ${PUBLISHED_RELEASE_TYPE} (Live - Latest release)
    ...    ${publication_accordion}    30
    user can see the create amendment controls for release    ${details}

Check cannot approve a draft release
    [Tags]    HappyPath
    user navigates to editable release summary from admin dashboard    ${PUBLICATION_NAME}
    ...    ${DRAFT_RELEASE_TYPE} (not Live)    ${THEME_NAME}    ${TOPIC_NAME}
    user cannot see the enabled approve release controls for release
