*** Settings ***
Resource            ../../../libs/admin-common.robot
Resource            ../../../libs/common.robot
Resource            ../../../libs/bootstrap_data/bootstrap_data_constants.robot
Resource            ../../../libs/admin/analyst/role_ui_permissions.robot

Suite Setup         user signs in as analyst1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev


*** Test Cases ***
Import permissions test variables
    Import bootstrap data roles and permissions variables
    Set suite variable    ${PUBLICATION_NAME}    ${PUBLICATION_FOR_PUBLICATION_APPROVER}

Validates publication approver publication page is correct
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}

    user waits until page contains link    Releases
    user waits until page contains link    Methodologies
    user waits until page contains link    Legacy releases

    user checks page does not contain link    Details
    user checks page does not contain link    Contact
    user checks page does not contain link    Team access

Check cannot create a legacy release
    user clicks link    Legacy releases
    user waits until h2 is visible    Legacy releases
    user checks page does not contain button    Create legacy release

Check cannot create a Methodology for a Publication if they don't have Publication Owner role
    user goes to methodologies and checks cannot create methodologies for publication    ${PUBLICATION_NAME}
    ...    ${THEME_NAME}    ${TOPIC_NAME}

Check cannot edit content for published release
    user navigates to published release page from dashboard    ${PUBLICATION_NAME}
    ...    ${PUBLISHED_RELEASE_TYPE}    ${THEME_NAME}    ${TOPIC_NAME}
    user cannot see edit controls for release content    ${PUBLICATION_NAME}

Check cannot create an amendment of a published release
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}    ${THEME_NAME}    ${TOPIC_NAME}
    user cannot see the create amendment controls for release    ${PUBLISHED_RELEASE_TYPE}
