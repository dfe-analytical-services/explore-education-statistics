*** Settings ***
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Library             ../../libs/admin_api.py

Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser

*** Variables ***
${ADOPTEE_PUBLICATION_NAME}=        UI tests - adoptee methodology publication %{RUN_IDENTIFIER}
${ADOPTING_PUBLICATION_NAME}=       UI tests - adopting methodology publication %{RUN_IDENTIFIER}

*** Test Cases ***
Create Publications and a Methodology to adopt
    user creates test publication via api    ${ADOPTEE_PUBLICATION_NAME}
    user creates methodology for publication    ${ADOPTEE_PUBLICATION_NAME}
    user creates test publication via api    ${ADOPTING_PUBLICATION_NAME}

Adopt a Methodology
    ${accordion}=    user opens publication on the admin dashboard    ${ADOPTING_PUBLICATION_NAME}
    user checks element contains link    ${accordion}    Adopt a methodology
    user clicks link    Adopt a methodology
    user waits until page contains title    Adopt a methodology
    user clicks radio    ${ADOPTEE_PUBLICATION_NAME}
    user clicks button    Save
    user waits until page contains title    Dashboard
    ${accordion}=    user opens publication on the admin dashboard    ${ADOPTING_PUBLICATION_NAME}
    ${details}=    user opens details dropdown    ${ADOPTEE_PUBLICATION_NAME} (Adopted)    ${accordion}
    user checks element contains button    ${details}    Remove methodology
    user views methodology for open publication accordion    ${accordion}    ${ADOPTEE_PUBLICATION_NAME}

Drop a Methodology
    ${accordion}=    user opens publication on the admin dashboard    ${ADOPTING_PUBLICATION_NAME}
    ${details}=    user opens details dropdown    ${ADOPTEE_PUBLICATION_NAME} (Adopted)    ${accordion}
    user clicks button    Remove methodology
    user waits until modal is visible    Remove methodology
    user clicks button    Confirm
    ${accordion}=    user opens publication on the admin dashboard    ${ADOPTING_PUBLICATION_NAME}
    user checks element does not contain    ${accordion}    ${ADOPTEE_PUBLICATION_NAME} (Adopted)
