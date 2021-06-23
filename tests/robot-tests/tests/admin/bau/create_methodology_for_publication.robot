*** Settings ***
Resource    ../../libs/admin-common.robot
Library  ../../libs/admin_api.py

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${PUBLICATION_NAME}  UI tests - create methodology publication %{RUN_IDENTIFIER}

*** Keywords ***
teardown suite
    user closes the browser

*** Test Cases ***
Create Methodology for Publication
    [Tags]  HappyPath
    user creates test publication via api   ${PUBLICATION_NAME}
    user opens publication on the admin dashboard   ${PUBLICATION_NAME}
    user waits until page contains button   Create methodology
    user waits until page contains button   Link to an externally hosted methodology
    user creates methodology for publication    ${PUBLICATION_NAME}
    user opens publication on the admin dashboard   ${PUBLICATION_NAME}
    user checks page does not contain button    Create methodology
    user checks page does not contain button    Link to an externally hosted methodology
    user views methodology for publication    ${PUBLICATION_NAME}
    user checks summary list contains   Title   ${PUBLICATION_NAME}
    user checks summary list contains   Status  Draft
    user checks summary list contains   Published on  Not yet published   