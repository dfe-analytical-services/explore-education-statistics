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
    Import bootstrap data roles and permissions variables
    Set suite variable    ${PUBLICATION_NAME}    ${PUBLICATION_FOR_RELEASE_APPROVER}

Navigate to Release where analyst has Release Approver role
    user navigates to publication on admin dashboard    ${PUBLICATION_NAME}    ${THEME_NAME}    ${TOPIC_NAME}

Check cannot create a Methodology for a Publication if they don't have Publication Owner role
    user cannot see the create methodologies controls for publication    ${publication_accordion}

Check cannot edit content for published release
    user navigates to readonly release summary from admin dashboard    ${PUBLICATION_NAME}
    ...    ${PUBLISHED_RELEASE_TYPE} (Live - Latest release)    ${THEME_NAME}    ${TOPIC_NAME}
    user cannot see edit controls for release content    ${PUBLICATION_NAME}

Navigate back to admin dashboard for publication
    user navigates to publication on admin dashboard    ${PUBLICATION_NAME}    ${THEME_NAME}    ${TOPIC_NAME}

Check cannot create an amendment of a published release
    ${details}=    user gets details content element    ${PUBLISHED_RELEASE_TYPE} (Live - Latest release)
    ...    ${publication_accordion}    30
    user cannot see the create amendment controls for release    ${details}
