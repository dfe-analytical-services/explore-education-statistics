*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData

*** Variables ***
${PUBLICATION_NAME}=    UI tests - methodology alternative title %{RUN_IDENTIFIER}

*** Test Cases ***
Create Publication as bau1
    user creates test publication via api    ${PUBLICATION_NAME}
    ${accordion}=    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    user checks element contains button    ${accordion}    Create methodology
    user checks element contains link    ${accordion}    Link to an externally hosted methodology

Assign publication owner permissions to analyst1
    user gives analyst publication owner access    ${PUBLICATION_NAME}

Switch to analyst1
    user changes to analyst1

Create a methodology as publication owner
    user creates methodology for publication    ${PUBLICATION_NAME}
    ${accordion}=    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    user checks element does not contain button    ${accordion}    Create methodology
    user checks element contains link    ${accordion}    Link to an externally hosted methodology
    ${details}=    user opens details dropdown    ${PUBLICATION_NAME}    ${accordion}
    user checks element contains link    ${details}    Edit this methodology
    user checks element contains button    ${details}    Remove
    user checks element does not contain button    ${details}    Amend methodology
    user views methodology for open publication accordion    ${accordion}    ${PUBLICATION_NAME}
    user verifies methodology summary details    ${PUBLICATION_NAME}

Update methodology title
    user edits methodology summary for publication    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - New methodology title
    user edits methodology summary for publication    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - New methodology title
    ...    ${PUBLICATION_NAME} - another new methodology title

Update methodology content
    user clicks link    Manage content
    user creates new content section    1    Methodology content section 1    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds text block to editable accordion section    Methodology content section 1
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds content to accordion section text block    Methodology content section 1    1
    ...    Adding Methodology content    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
