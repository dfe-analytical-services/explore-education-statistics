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
    user waits until page contains link    Team access
    user waits until page contains link    Legacy releases    # remove as part of EES-3794

    user checks page does not contain link    Details
    user checks page does not contain link    Contact

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

Check can see the readonly "Release access" section of the "Team access" page
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Team access
    user waits until h3 is visible    Release access
    user waits until h3 is not visible    Update release access
    user waits until page does not contain link    Manage release contributors

Check cannot see the "Invite new contributors" functionality of the "Team access" page
    user waits until page does not contain link    Invite new contributors
