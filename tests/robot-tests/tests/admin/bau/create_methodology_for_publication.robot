*** Settings ***
Resource            ../../libs/admin-common.robot
Library             ../../libs/admin_api.py

Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser

*** Variables ***
${PUBLICATION_NAME}=    UI tests - create methodology publication %{RUN_IDENTIFIER}

*** Test Cases ***
Create Methodology for Publication
    [Tags]    HappyPath
    user creates test publication via api    ${PUBLICATION_NAME}
    ${accordion}=    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    user checks element contains button    ${accordion}    Create methodology
    user checks element contains button    ${accordion}    Link to an externally hosted methodology
    user creates methodology for publication    ${PUBLICATION_NAME}
    ${accordion}=    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    user checks element does not contain button    ${accordion}    Create methodology
    user checks element contains button    ${accordion}    Link to an externally hosted methodology
    user views methodology for open publication accordion    ${accordion}    ${PUBLICATION_NAME}
    user checks summary list contains    Title    ${PUBLICATION_NAME}
    user checks summary list contains    Status    Draft
    user checks summary list contains    Published on    Not yet published
    user checks summary list contains    Owning publication    ${PUBLICATION_NAME}

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

Update Methodology Content
    [Tags]    HappyPath
    user clicks link    Manage content
    user clicks element    testid:Content-addNewSectionButton
    user clicks button    New section
    user clicks element    testid:New section-editSectionTitleButton
    user enters text into element    xpath=//*[@name="heading"]    Methodology content section 1
    user clicks button    Save section title
    user clicks element    testid:Methodology content section 1-addTextBlockButton
    user clicks element    testid:Methodology content section 1-editableContentBlock1-editBlockButton
    user presses keys    Adding Methodology content
    choose file    xpath://button[span[.="Insert image"]]/following-sibling::input[@type="file"]
    ...    ${FILES_DIR}test-infographic.png
    user clicks element    xpath://button[span[.="Change image text alternative"]]
    user enters text into element    label:Text alternative    Alt text for the uploaded content image
    user clicks element    css:button.ck-button-save
    user sets focus to element    xpath://button[.="Save"]
    user clicks button    Save
    user clicks button    Methodology content section 1    # collapse block before adding new ones

    user clicks element    testid:Annexes-addNewSectionButton
    user clicks button    New section
    user clicks element    testid:New section-editSectionTitleButton
    user enters text into element    xpath=//*[@name="heading"]    Methodology annex section 1
    user clicks button    Save section title
    user clicks element    testid:Methodology annex section 1-addTextBlockButton
    user clicks element    testid:Methodology annex section 1-editableContentBlock1-editBlockButton
    user presses keys    Adding Methodology annex
    choose file    xpath://button[span[.="Insert image"]]/following-sibling::input[@type="file"]
    ...    ${FILES_DIR}dfe-logo.jpg
    user clicks element    xpath://button[span[.="Change image text alternative"]]
    user enters text into element    label:Text alternative    Alt text for the uploaded annex image
    user clicks element    css:button.ck-button-save
    user sets focus to element    xpath://button[.="Save"]
    user clicks button    Save
    user clicks button    Methodology annex section 1    # collapse block before adding new ones

Approve Methodology
    user clicks link    Sign off
    user changes methodology status to Approved
    user waits until h2 is visible    Methodology status
    user clicks link    Summary
    user waits until h2 is visible    Methodology summary
    user checks summary list contains    Title    ${PUBLICATION_NAME} - New methodology title
    user checks summary list contains    Status    Approved
    user checks summary list contains    Published on    Not yet published
    user checks summary list contains    Owning publication    ${PUBLICATION_NAME}

*** Keywords ***
teardown suite
    user closes the browser
