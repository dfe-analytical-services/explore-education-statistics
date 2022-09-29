*** Settings ***
Resource    ../../common.robot
Resource    ../../admin-common.robot


*** Keywords ***
user navigates to publication on admin dashboard
    [Arguments]    ${publication}    ${theme}    ${topic}
    ${publication_accordion}=    user goes to publication page from dashboard    ${publication}    ${theme}
    ...    ${topic}
    set suite variable    ${publication_accordion}

user cannot see the create methodologies controls for publication
    [Arguments]    ${PUBLICATION_ACCORDION}
    user checks element does not contain button    ${PUBLICATION_ACCORDION}    Create methodology
    user checks element does not contain link    ${PUBLICATION_ACCORDION}    Use an external methodology

user cannot see the create amendment controls for release
    [Arguments]    ${RELEASE_DETAILS_SECTION}
    user waits until element contains link    ${RELEASE_DETAILS_SECTION}    View release    %{WAIT_SMALL}
    user checks element does not contain button    ${RELEASE_DETAILS_SECTION}    Amend release

user cannot see edit controls for release content
    [Arguments]    ${publication}
    user clicks link    Content
    user waits until h2 is visible    ${publication}
    user waits until page does not contain button    Set page view
    user waits until page does not contain button    Add note
    user waits until page does not contain button    Add related page link
    user waits until page does not contain button    Add secondary stats
    user waits until page does not contain button    Add key statistic
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

user can see the create methodologies controls for publication
    [Arguments]    ${PUBLICATION_ACCORDION}
    user checks element contains button    ${PUBLICATION_ACCORDION}    Create methodology
    user checks element contains link    ${PUBLICATION_ACCORDION}    Use an external methodology

user can see the create amendment controls for release
    [Arguments]    ${RELEASE_DETAILS_SECTION}
    user waits until element contains link    ${RELEASE_DETAILS_SECTION}    View release    %{WAIT_SMALL}
    user checks element contains button    ${RELEASE_DETAILS_SECTION}    Amend release

user cannot see the edit status controls for methodology
    user clicks link    Sign off
    user waits until page does not contain loading spinner
    user checks page does not contain    Edit status

user cannot see the remove controls for methodology
    [Arguments]    ${DETAILS_SECTION}
    user checks element contains link    ${DETAILS_SECTION}    View methodology
    user checks element does not contain button    ${DETAILS_SECTION}    Remove

user cannot see the cancel amendment controls for methodology
    [Arguments]    ${DETAILS_SECTION}
    user checks element contains link    ${DETAILS_SECTION}    View amendment
    user checks element does not contain button    ${DETAILS_SECTION}    Cancel amendment
