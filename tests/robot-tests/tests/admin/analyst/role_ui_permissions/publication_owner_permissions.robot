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
    user navigates to publication on admin dashboard    ${PUBLICATION_NAME}    ${THEME_NAME}    ${TOPIC_NAME}

Check can create a Methodology for the owned Publication
    user can see the create methodologies controls for publication    ${publication_accordion}

Check cannot edit content for published release
    user navigates to readonly release summary from admin dashboard    ${PUBLICATION_NAME}
    ...    ${PUBLISHED_RELEASE_TYPE} (Live - Latest release)    ${THEME_NAME}    ${TOPIC_NAME}
    user cannot see edit controls for release content    ${PUBLICATION_NAME}

Navigate back to admin dashboard for publication
    user navigates to publication on admin dashboard    ${PUBLICATION_NAME}    ${THEME_NAME}    ${TOPIC_NAME}

Check Edit publication page inputs are correct
    ${accordion}=    user gets accordion section content element    ${PUBLICATION_NAME}
    user clicks link    Manage publication    ${accordion}
    user waits until page contains title    Manage publication

    user waits until page contains element    label:Select theme
    user waits until page contains element    label:Select topic
    user waits until page contains element    label:Team name
    user waits until page contains element    label:Team email address
    user waits until page contains element    label:Contact name
    user waits until page contains element    label:Contact telephone number
    user checks page does not contain element    label:Publication title    # Only BAU users should see this

    user clicks link    Cancel
    user waits until page does not contain link    Cancel

Check can create an amendment of a published release
    user navigates to publication on admin dashboard    ${PUBLICATION_NAME}    ${THEME_NAME}    ${TOPIC_NAME}

    ${details}=    user opens details dropdown    ${PUBLISHED_RELEASE_TYPE} (Live - Latest release)
    ...    ${publication_accordion}
    user can see the create amendment controls for release    ${details}

Check cannot approve a draft release
    user navigates to editable release summary from admin dashboard    ${PUBLICATION_NAME}
    ...    ${DRAFT_RELEASE_TYPE} (not Live)    ${THEME_NAME}    ${TOPIC_NAME}
    user cannot see the enabled approve release controls for release
