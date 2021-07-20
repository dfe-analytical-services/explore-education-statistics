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

Update Methodology for Publication
    [Tags]    HappyPath
    ${accordion}=    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    user views methodology for open publication accordion    ${accordion}    ${PUBLICATION_NAME}
    user clicks link    Edit summary
    user enters text into element    label:Enter methodology title    ${PUBLICATION_NAME} - New methodology title
    user clicks button    Update methodology
    user waits until h2 is visible    Methodology summary
    user clicks link    Sign off
    user changes methodology status to Approved
    user waits until h2 is visible    Methodology status
    user clicks link    Summary
    user checks summary list contains    Title    ${PUBLICATION_NAME} - New methodology title
    user checks summary list contains    Status    Approved
    user checks summary list contains    Published on    Not yet published
    user checks summary list contains    Owning publication    ${PUBLICATION_NAME}

*** Keywords ***
teardown suite
    user closes the browser
