*** Settings ***
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Library             ../../libs/admin_api.py

Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Variables ***
${PUBLICATION_NAME}=    UI tests - create methodology publication %{RUN_IDENTIFIER}


*** Test Cases ***
Create Publication and check available Methodology controls
    user creates test publication via api    ${PUBLICATION_NAME}
    ${accordion}=    user goes to publication page from dashboard    ${PUBLICATION_NAME}
    user checks element contains button    ${accordion}    Create methodology
    user checks element contains link    ${accordion}    Use an external methodology

Create a Methodology
    bau user creates methodology for publication    ${PUBLICATION_NAME}
    ${accordion}=    user goes to publication page from dashboard    ${PUBLICATION_NAME}
    user checks element does not contain button    ${accordion}    Create methodology
    user checks element contains link    ${accordion}    Use an external methodology
    ${details}=    user opens details dropdown    ${PUBLICATION_NAME}    ${accordion}
    user checks element contains link    ${details}    Edit methodology
    user checks element contains button    ${details}    Remove
    user checks element does not contain button    ${details}    Amend methodology
    user views methodology for open publication accordion    ${accordion}    ${PUBLICATION_NAME}
    user verifies methodology summary details    ${PUBLICATION_NAME}

Remove the Methodology
    ${accordion}=    user goes to publication page from dashboard    ${PUBLICATION_NAME}
    ${details}=    user opens details dropdown    ${PUBLICATION_NAME}    ${accordion}
    user clicks button    Remove    ${details}
    user waits until modal is visible    Confirm you want to remove this methodology
    user clicks button    Confirm
    user waits until modal is not visible    Confirm you want to remove this methodology

    ${accordion}=    user goes to publication page from dashboard    ${PUBLICATION_NAME}
    user checks element contains button    ${accordion}    Create methodology

Create a Methodology again
    bau user creates methodology for publication    ${PUBLICATION_NAME}

Update Methodology Summary
    user edits methodology summary for publication    ${PUBLICATION_NAME}    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - New methodology title
    user edits methodology summary for publication    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - New methodology title    ${PUBLICATION_NAME} - another new methodology title

Update the Methodology Content
    user clicks link    Manage content
    user creates new content section    1    Methodology content section 1    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds text block to editable accordion section    Methodology content section 1
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds content to accordion section text block    Methodology content section 1    1
    ...    Adding Methodology content    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds image to accordion section text block    Methodology content section 1    1    test-infographic.png
    ...    Alt text for the uploaded content image    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}

    user creates new content section    1    Methodology annex section 1    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user adds text block to editable accordion section    Methodology annex section 1
    ...    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user adds content to accordion section text block    Methodology annex section 1    1    Adding Methodology annex
    ...    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user adds image to accordion section text block    Methodology annex section 1    1    dfe-logo.jpg
    ...    Alt text for the uploaded annex image    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}

    user checks accordion section contains x blocks    Methodology content section 1    1
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user checks accordion section text block contains image with alt text    Methodology content section 1    1
    ...    Alt text for the uploaded content image    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user checks accordion section contains x blocks    Methodology annex section 1    1
    ...    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user checks accordion section text block contains image with alt text    Methodology annex section 1    1
    ...    Alt text for the uploaded annex image    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}

Approve the Methodology
    approve methodology from methodology view
    user clicks link    Summary
    user verifies methodology summary details
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - another new methodology title
    ...    Approved
    ...    Not yet published

Check the controls available are as expected for an approved Methodology that is not yet publicly visible
    ${accordion}=    user goes to publication page from dashboard    ${PUBLICATION_NAME}
    ${details}=    user opens details dropdown    ${PUBLICATION_NAME}    ${accordion}
    user checks element contains link    ${details}    Edit methodology
    user checks element does not contain button    ${details}    Remove

    # Check that the Amend methodology button is not yet present.    This is because the Methodology, although set to
    # publish immediately, is attached to a Publication that does not yet have any live Releases, and so this
    # Methodology can still be unapproved and edited rather than needing to be amended.
    user checks element does not contain button    ${details}    Amend methodology

Unapprove the Methodology
    user views methodology for publication    ${PUBLICATION_NAME}
    user clicks link    Sign off
    user changes methodology status to Draft

    ${accordion}=    user goes to publication page from dashboard    ${PUBLICATION_NAME}
    ${details}=    user opens details dropdown    ${PUBLICATION_NAME}    ${accordion}
    user checks element contains link    ${details}    Edit methodology
    user checks element contains button    ${details}    Remove
