*** Settings ***
Resource    ../../libs/admin-common.robot
Library  ../../libs/admin_api.py

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    teardown suite

*** Variables ***
${PUBLICATION_NAME}  UI tests - create methodology publication %{RUN_IDENTIFIER}

*** Keywords ***
teardown suite
    user closes the browser

*** Test Cases ***
Create Methodology for Publication
    [Tags]  HappyPath
    user creates test publication via api   ${PUBLICATION_NAME}
    user creates methodology for publication    ${PUBLICATION_NAME}