*** Settings ***
Resource    ../../common.robot
Resource    ../../admin-common.robot


*** Keywords ***
user goes to methodologies and checks cannot create methodologies for publication
    [Arguments]    ${PUBLICATION_NAME}
    ...    ${THEME_NAME}
    ...    ${TOPIC_NAME}
    user navigates to methodologies on publication page    ${PUBLICATION_NAME}
    ...    ${THEME_NAME}
    ...    ${TOPIC_NAME}

    user checks page does not contain button    Create new methodology
    user checks page does not contain link    Add external methodology
    user checks page does not contain link    Adopt an existing methodology

user cannot see the create amendment controls for release
    [Arguments]    ${RELEASE_NAME}
    ${ROW}=    user gets table row    ${RELEASE_NAME}    testid:publication-published-releases
    user checks element does not contain button    ${ROW}    Amend

user cannot see edit controls for release content
    [Arguments]    ${publication}
    user clicks link    Content
    user waits until h2 is visible    ${publication}
    user waits until page does not contain button    Set page view
    user waits until page does not contain button    Add note
    user waits until page does not contain button    Add related page link
    user waits until page does not contain button    Add secondary stats
    user waits until page does not contain button    Add key statistic from data block
    user waits until page does not contain button    Add free text key statistic
    user waits until page does not contain button    Add a headlines text block
    user waits until page does not contain button    Add new section

user cannot see the edit release status controls for release
    user clicks link    Sign off
    user waits until page does not contain loading spinner
    user checks page does not contain    Edit release status

user cannot see the enabled approve release controls for release
    user clicks link    Sign off
    user waits until h2 is visible    Sign off
    user clicks button    Edit release status
    user waits until h2 is visible    Edit release status    %{WAIT_SMALL}
    user scrolls to element    id:releaseStatusForm-approvalStatus-Approved
    user checks element is disabled    id:releaseStatusForm-approvalStatus-Approved

user can see the create amendment controls for release
    [Arguments]    ${RELEASE_NAME}
    ${ROW}=    user gets table row    ${RELEASE_NAME}    testid:publication-published-releases
    user checks element contains button    ${ROW}    Amend

user cannot see the edit status controls for methodology
    user clicks link    Sign off
    user waits until page does not contain loading spinner
    user checks page does not contain    Edit status

user cannot see the remove controls for methodology
    user waits until h2 is visible    Manage methodologies
    user checks page does not contain button    Delete draft
    user checks page does not contain button    Remove

user cannot see the cancel amendment controls for methodology
    user waits until page contains element    testid:view
    user waits until page contains element    testid:view-existing-version
    user checks page does not contain element    testid:cancel-amendment
