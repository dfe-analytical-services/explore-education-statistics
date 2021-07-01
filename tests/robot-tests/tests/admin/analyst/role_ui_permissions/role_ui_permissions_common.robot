*** Keywords ***
user cannot see the create methodologies controls for publication
    [Arguments]  ${PUBLICATION_ACCORDION}
    user checks element does not contain button  ${PUBLICATION_ACCORDION}  Create methodology
    user checks element does not contain button  ${PUBLICATION_ACCORDION}  Link to an externally hosted methodology

user cannot see the create amendment controls for release
    [Arguments]  ${RELEASE_DETAILS_SECTION}
    user waits until element contains link   ${RELEASE_DETAILS_SECTION}  View this release  30
    user checks element does not contain button  ${RELEASE_DETAILS_SECTION}  Amend this release

user cannot see the edit release status controls for release
    [Arguments]
    user clicks link   Sign off
    user waits until page does not contain loading spinner
    user checks page does not contain  Edit release status

user can see the create methodologies controls for publication
    [Arguments]  ${PUBLICATION_ACCORDION}
    user checks element contains button  ${PUBLICATION_ACCORDION}  Create methodology
    user checks element contains button  ${PUBLICATION_ACCORDION}  Link to an externally hosted methodology

user can see the create amendment controls for release
    [Arguments]  ${RELEASE_DETAILS_SECTION}
    user waits until element contains link   ${RELEASE_DETAILS_SECTION}  View this release  30
    user checks element contains button  ${RELEASE_DETAILS_SECTION}  Amend this release