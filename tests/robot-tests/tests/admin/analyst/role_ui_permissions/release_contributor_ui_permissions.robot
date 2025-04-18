*** Settings ***
Resource            ../../../libs/admin-common.robot
Resource            ../../../libs/common.robot
Resource            ../../../seed_data/seed_data_theme_2_constants.robot
Resource            ../../../libs/admin/analyst/role_ui_permissions.robot

Suite Setup         user signs in as analyst1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev


*** Test Cases ***
Import permissions test variables
    Set suite variable    ${PUBLICATION_NAME}    ${ROLE_PERMISSIONS_RELEASE_CONTRIBUTOR_PUBLICATION}
    Set suite variable    ${THEME_NAME}    ${ROLE_PERMISSIONS_THEME_TITLE}
    Set suite variable    ${DRAFT_RELEASE_TYPE}    ${ROLE_PERMISSIONS_DRAFT_RELEASE_TYPE}
    Set suite variable    ${PUBLISHED_RELEASE_TYPE}    ${ROLE_PERMISSIONS_PUBLISHED_RELEASE_TYPE}

Validates release contributor publication page is correct
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}

    user waits until page contains link    Releases
    user waits until page contains link    Methodologies
    user waits until page contains link    Team access
    user waits until page contains link    Release order

    user checks page does not contain link    Details
    user checks page does not contain link    Contact

Check cannot reorder releases
    user clicks link    Release order
    user waits until h2 is visible    Release order
    user checks page does not contain button    Reorder releases

Check cannot create legacy releases
    user checks page does not contain button    Create legacy release

Check cannot create a Methodology for a Publication if they don't have Publication Owner role
    user goes to methodologies and checks cannot create methodologies for publication    ${PUBLICATION_NAME}
    ...    ${THEME_NAME}

Check cannot edit content for published release
    user navigates to published release page from dashboard    ${PUBLICATION_NAME}
    ...    ${PUBLISHED_RELEASE_TYPE}    ${THEME_NAME}
    user cannot see edit controls for release content    ${PUBLICATION_NAME}

Check cannot create an amendment of a published release
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}    ${THEME_NAME}
    user cannot see the create amendment controls for release    ${PUBLISHED_RELEASE_TYPE}

Check cannot approve a draft release
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${DRAFT_RELEASE_TYPE}    ${THEME_NAME}
    user cannot see the enabled approve release controls for release

Check cannot see the readonly or editable version of the "Release access" section of the "Team access" page
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Team access
    user waits until page finishes loading
    user waits until h3 is not visible    Update release access
    user waits until h3 is not visible    Release access
    user waits until page does not contain link    Manage release contributors

Check cannot see the "Invite new contributors" functionality of the "Team access" page
    user waits until page does not contain link    Invite new contributors
