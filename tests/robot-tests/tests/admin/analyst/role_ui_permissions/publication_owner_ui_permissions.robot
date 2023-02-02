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
    Set suite variable    ${PUBLICATION_NAME}    ${PUBLICATION_FOR_PUBLICATION_OWNER}

Navigate to Publication where analyst has Publication Owner role
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}    ${THEME_NAME}    ${TOPIC_NAME}
    user waits until page contains link    Releases
    user waits until page contains link    Methodologies
    user waits until page contains link    Details
    user waits until page contains link    Contact
    user waits until page contains link    Team access
    user waits until page contains link    Legacy releases

Check can create a legacy release
    user clicks link    Legacy releases
    user waits until h2 is visible    Legacy releases
    user checks page contains button    Create legacy release

Check can create a Methodology for the owned Publication
    user clicks link    Methodologies
    user waits until h2 is visible    Manage methodologies

    user checks page contains button    Create new methodology
    user waits until page contains link    Add external methodology
    user waits until page contains link    Adopt an existing methodology

Check cannot edit content for published release
    user navigates to published release page from dashboard    ${PUBLICATION_NAME}
    ...    ${PUBLISHED_RELEASE_TYPE}    ${THEME_NAME}    ${TOPIC_NAME}
    user cannot see edit controls for release content    ${PUBLICATION_NAME}

Check Edit publication details page inputs are correct
    user navigates to details on publication page    ${PUBLICATION_NAME}
    user clicks button    Edit publication details

    user waits until page contains element    label:Publication summary
    user checks page does not contain element    label:Publication title    # Only BAU users should see this
    user checks page does not contain element    label:Select theme    # Only BAU users should see this
    user checks page does not contain element    label:Select topic    # Only BAU users should see this
    user checks page does not contain element    label:Superseding publication    # Only BAU users should see this

    user clicks button    Cancel
    user waits until page does not contain button    Cancel

Check Edit publication contact page inputs are correct
    user clicks link    Contact
    user waits until h2 is visible    Contact for this publication
    user clicks button    Edit contact details

    user waits until page contains element    label:Team name
    user waits until page contains element    label:Team email
    user waits until page contains element    label:Contact name
    user waits until page contains element    label:Contact telephone

    user clicks button    Cancel
    user waits until page does not contain button    Cancel

Check can create an amendment of a published release
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}    ${THEME_NAME}    ${TOPIC_NAME}
    user can see the create amendment controls for release    ${PUBLISHED_RELEASE_TYPE}

Check cannot approve a draft release
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${DRAFT_RELEASE_TYPE}    ${THEME_NAME}    ${TOPIC_NAME}
    user cannot see the enabled approve release controls for release

Check can see the editable "Update release access" section of the "Team access" page
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Team access
    user waits until h3 is visible    Update release access
    user waits until h3 is not visible    Release access
    user waits until page contains link    Manage release contributors

Check can see the "Invite new contributors" functionality of the "Team access" page
    user waits until page contains link    Invite new contributors
