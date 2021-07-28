*** Settings ***
Resource            ../../libs/admin-common.robot
Resource            ../../libs/common.robot
Library             ../../libs/admin_api.py

Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser

*** Variables ***
${PUBLICATION_NAME}=    UI tests - create methodology amendment publication %{RUN_IDENTIFIER}

*** Test Cases ***
Create publicly accessible Publication
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user create test release via api    ${PUBLICATION_ID}    AY    2021
    user navigates to editable release summary from admin dashboard    ${PUBLICATION_NAME}
    ...    Academic Year 2021/22 (not Live)
    user approves release for immediate publication

Create approved Methodology for Publication
    [Tags]    HappyPath
    user creates approved methodology for publication    ${PUBLICATION_NAME}

Create Methodology Amendment for Publication
    [Tags]    HappyPath
    ${accordion}=    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    user opens details dropdown    ${PUBLICATION_NAME}    ${accordion}
    user clicks button    Amend methodology    ${accordion}
    user waits until modal is visible    Confirm you want to amend this live methodology
    user clicks button    Confirm
    user waits until h2 is visible    Methodology summary
    user checks page contains tag    Draft
    user checks page contains tag    Amendment

*** Keywords ***
teardown suite
    user closes the browser
