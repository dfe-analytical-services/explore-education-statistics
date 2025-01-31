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

    user navigates to publication page from dashboard    ${PUBLICATION_NAME}

    user clicks link    Methodologies
    user waits until h2 is visible    Manage methodologies

    user waits until page contains button    Create new methodology
    user waits until page contains link    Add external methodology
    user waits until page contains link    Adopt an existing methodology

Assign publication owner permissions to analyst1
    user gives analyst publication owner access    ${PUBLICATION_NAME}

Switch to analyst1
    user changes to analyst1

Create a methodology as publication owner
    user clicks link    ${PUBLICATION_NAME}
    user waits until h1 is visible    ${PUBLICATION_NAME}

    user clicks link    Methodologies
    user waits until h2 is visible    Manage methodologies

    user clicks button    Create new methodology
    user verifies methodology summary details    ${PUBLICATION_NAME}

Navigate back to Publication page Methodologies
    user clicks link    ${PUBLICATION_NAME}
    user waits until h1 is visible    ${PUBLICATION_NAME}

    user clicks link    Methodologies

    user checks page does not contain button    Create new methodology
    user waits until page contains link    Add external methodology
    user waits until page contains link    Adopt an existing methodology

    ${ROW}=    user gets table row    ${PUBLICATION_NAME}    testid:methodologies
    user checks element contains    ${ROW}    Owned
    user checks element does not contain    ${ROW}    Adopted

    user checks element contains link    ${ROW}    Edit
    user checks element contains button    ${ROW}    Delete draft

    user checks element does not contain    ${ROW}    View existing version
    user checks element does not contain    ${ROW}    Amend

    user clicks element    xpath://*[text()="Edit"]    ${ROW}
    user waits until h2 is visible    Methodology summary

    user verifies methodology summary details    ${PUBLICATION_NAME}

Update methodology title
    user clicks link    Edit summary
    user waits until h2 is visible    Edit methodology summary
    user clicks radio    Set an alternative title
    user waits until element is visible    label:Enter methodology title
    user checks input field contains    label:Enter methodology title    ${PUBLICATION_NAME}
    user enters text into element    label:Enter methodology title    ${PUBLICATION_NAME} - New methodology title
    user clicks button    Update methodology
    user verifies methodology summary details    ${PUBLICATION_NAME}    ${PUBLICATION_NAME} - New methodology title

    user clicks link    Edit summary
    user waits until h2 is visible    Edit methodology summary
    user clicks radio    Set an alternative title
    user waits until element is visible    label:Enter methodology title
    user checks input field contains    label:Enter methodology title    ${PUBLICATION_NAME} - New methodology title
    user enters text into element    label:Enter methodology title
    ...    ${PUBLICATION_NAME} - Another new methodology title
    user clicks button    Update methodology
    user verifies methodology summary details    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - Another new methodology title

Update methodology content
    user clicks link    Manage content
    user creates new content section    1    Methodology content section 1    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds text block to editable accordion section    Methodology content section 1
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds content to accordion section text block    Methodology content section 1    1
    ...    Adding Methodology content    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
