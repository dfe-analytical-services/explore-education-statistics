*** Settings ***
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Library             ../../libs/admin_api.py

Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Variables ***
${OWNING_PUBLICATION_NAME}=         UI tests - methodology owning publication %{RUN_IDENTIFIER}
${ADOPTING_PUBLICATION_NAME}=       UI tests - adopting methodology publication %{RUN_IDENTIFIER}


*** Test Cases ***
Create Publications and a Methodology to adopt
    user creates test publication via api    ${OWNING_PUBLICATION_NAME}
    bau user creates methodology for publication    ${OWNING_PUBLICATION_NAME}
    user creates test publication via api    ${ADOPTING_PUBLICATION_NAME}

Adopt a Methodology
    ${accordion}=    bau user goes to publication page from dashboard    ${ADOPTING_PUBLICATION_NAME}
    user checks element contains link    ${accordion}    Adopt an existing methodology
    user clicks link    Adopt an existing methodology    ${accordion}
    user waits until page contains title    Adopt a methodology
    user clicks radio    ${OWNING_PUBLICATION_NAME}
    user opens details dropdown    More details    css:[data-testid="Radio item for ${OWNING_PUBLICATION_NAME}"]
    ${selected_methodology_details}=    user gets details content element    More details
    ...    css:[data-testid="Radio item for ${OWNING_PUBLICATION_NAME}"]
    user checks element should contain    ${selected_methodology_details}    ${OWNING_PUBLICATION_NAME}
    user checks element should contain    ${selected_methodology_details}    DRAFT
    user checks element should contain    ${selected_methodology_details}    Not yet published
    user clicks button    Save
    user waits until page contains title    Dashboard

Validate methodology adopted
    ${accordion}=    bau user goes to publication page from dashboard    ${ADOPTING_PUBLICATION_NAME}
    ${details}=    user opens details dropdown    ${OWNING_PUBLICATION_NAME} (Adopted)    ${accordion}
    user checks element contains button    ${details}    Remove methodology
    user views methodology for open publication accordion    ${accordion}    ${OWNING_PUBLICATION_NAME}

Drop adopted Methodology
    ${accordion}=    bau user goes to publication page from dashboard    ${ADOPTING_PUBLICATION_NAME}
    ${details}=    user opens details dropdown    ${OWNING_PUBLICATION_NAME} (Adopted)    ${accordion}
    user clicks button    Remove methodology    ${accordion}
    user waits until modal is visible    Remove methodology
    user clicks button    Confirm
    user waits until modal is not visible    Remove methodology

Validate adopted methodology is dropped
    ${accordion}=    bau user goes to publication page from dashboard    ${ADOPTING_PUBLICATION_NAME}
    user checks element does not contain    ${accordion}    ${OWNING_PUBLICATION_NAME} (Adopted)
