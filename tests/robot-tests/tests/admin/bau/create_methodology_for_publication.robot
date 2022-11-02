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
    user navigates to methodologies on publication page    ${PUBLICATION_NAME}
    user checks page contains button    Create new methodology
    user waits until page contains link    Add external methodology
    user waits until page contains link    Adopt an existing methodology

Create a Methodology
    user creates methodology for publication    ${PUBLICATION_NAME}

    user navigates to methodologies on publication page    ${PUBLICATION_NAME}
    user checks page does not contain button    Create new methodology
    user waits until page contains link    Add external methodology
    user waits until page contains link    Adopt an existing methodology

    ${ROW}=    user gets table row    ${PUBLICATION_NAME}    testid:methodologies

    user checks element contains link    ${ROW}    Edit
    user checks element contains button    ${ROW}    Delete draft
    user checks element does not contain button    ${ROW}    Amend

    user clicks element    xpath://*[text()="Edit"]    testid:methodologies
    user waits until h2 is visible    Methodology summary

    user verifies methodology summary details    ${PUBLICATION_NAME}

Remove the Methodology
    user navigates to methodologies on publication page    ${PUBLICATION_NAME}

    ${ROW}=    user gets table row    ${PUBLICATION_NAME}    testid:methodologies
    user clicks element    xpath://*[text()="Delete draft"]    testid:methodologies

    user waits until modal is visible    Confirm you want to delete this draft methodology
    user clicks button    Confirm
    user waits until modal is not visible    Confirm you want to delete this draft methodology

    user waits until h2 is visible    Manage methodologies
    user checks page contains button    Create new methodology

Create a Methodology again
    user creates methodology for publication    ${PUBLICATION_NAME}

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
    user navigates to methodologies on publication page    ${PUBLICATION_NAME}

    ${ROW}=    user gets table row    ${PUBLICATION_NAME} - another new methodology title    testid:methodologies
    user checks element contains link    ${ROW}    Edit
    user checks element does not contain button    ${ROW}    Delete draft

    # Check that the Amend methodology button is not yet present.    This is because the Methodology, although set to
    # publish immediately, is attached to a Publication that does not yet have any live Releases, and so this
    # Methodology can still be unapproved and edited rather than needing to be amended.
    user checks element does not contain button    ${ROW}    Amend

Unapprove the Methodology
    ${ROW}=    user gets table row    ${PUBLICATION_NAME} - another new methodology title    testid:methodologies
    user clicks element    xpath://*[text()="Edit"]    ${ROW}
    user waits until h2 is visible    Methodology summary

    user clicks link    Sign off
    user changes methodology status to Draft

    user navigates to methodologies on publication page    ${PUBLICATION_NAME}
    ${ROW}=    user gets table row    ${PUBLICATION_NAME} - another new methodology title    testid:methodologies
    user checks element contains link    ${ROW}    Edit
    user checks element contains button    ${ROW}    Delete draft
