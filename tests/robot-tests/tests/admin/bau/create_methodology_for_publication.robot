*** Settings ***
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Library             ../../libs/admin_api.py

Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser

*** Variables ***
${PUBLICATION_NAME}=    UI tests - create methodology publication %{RUN_IDENTIFIER}

*** Test Cases ***
Create Publication and check available Methodology controls
    [Tags]    HappyPath
    user creates test publication via api    ${PUBLICATION_NAME}
    ${accordion}=    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    user checks element contains button    ${accordion}    Create methodology
    user checks element contains button    ${accordion}    Link to an externally hosted methodology

Create a Methodology
    [Tags]    HappyPath
    user creates methodology for publication    ${PUBLICATION_NAME}
    ${accordion}=    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    user checks element does not contain button    ${accordion}    Create methodology
    user checks element contains button    ${accordion}    Link to an externally hosted methodology
    user views methodology for open publication accordion    ${accordion}    ${PUBLICATION_NAME}
    user checks summary list contains    Title    ${PUBLICATION_NAME}
    user checks summary list contains    Status    Draft
    user checks summary list contains    Published on    Not yet published
    user checks summary list contains    Owning publication    ${PUBLICATION_NAME}

Remove the Methodology
    [Tags]    HappyPath
    ${accordion}=    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    ${details}=    user opens details dropdown    ${PUBLICATION_NAME}    ${accordion}
    user checks element contains button    ${details}    Remove
    user clicks button    Remove    ${details}
    user waits until modal is visible    Confirm you want to remove this methodology
    user clicks button    Confirm

    ${accordion}=    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    user checks element contains button    ${accordion}    Create methodology

Create a Methodology again
    [Tags]    HappyPath
    user creates methodology for publication    ${PUBLICATION_NAME}

Update Methodology Summary
    [Tags]    HappyPath
    ${accordion}=    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    user views methodology for open publication accordion    ${accordion}    ${PUBLICATION_NAME}
    user clicks link    Edit summary
    user enters text into element    label:Enter methodology title    ${PUBLICATION_NAME} - New methodology title
    user clicks button    Update methodology
    user waits until h2 is visible    Methodology summary
    user checks summary list contains    Title    ${PUBLICATION_NAME} - New methodology title
    user checks summary list contains    Status    Draft
    user checks summary list contains    Published on    Not yet published
    user checks summary list contains    Owning publication    ${PUBLICATION_NAME}

Update the Methodology Content
    [Tags]    HappyPath
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
    user clicks link    Sign off
    user changes methodology status to Approved
    user clicks link    Summary
    user waits until h2 is visible    Methodology summary
    user checks summary list contains    Title    ${PUBLICATION_NAME} - New methodology title
    user checks summary list contains    Status    Approved
    user checks summary list contains    Published on    Not yet published
    user checks summary list contains    Owning publication    ${PUBLICATION_NAME}

*** Keywords ***
teardown suite
    user closes the browser
