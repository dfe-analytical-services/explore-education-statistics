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
    user creates methodology for publication    ${OWNING_PUBLICATION_NAME}
    user creates test publication via api    ${ADOPTING_PUBLICATION_NAME}

Adopt a Methodology
    user navigates to methodologies on publication page    ${ADOPTING_PUBLICATION_NAME}

    user waits until page contains link    Adopt an existing methodology
    user clicks link    Adopt an existing methodology
    user waits until h2 is visible    Adopt a methodology

    user clicks radio    ${OWNING_PUBLICATION_NAME}
    user opens details dropdown    More details    css:[data-testid="Radio item for ${OWNING_PUBLICATION_NAME}"]
    ${selected_methodology_details}=    user gets details content element    More details
    ...    css:[data-testid="Radio item for ${OWNING_PUBLICATION_NAME}"]
    user checks element should contain    ${selected_methodology_details}    ${OWNING_PUBLICATION_NAME}
    user checks element should contain    ${selected_methodology_details}    DRAFT
    user checks element should contain    ${selected_methodology_details}    Not yet published
    user clicks button    Save
    user waits until h2 is visible    Manage methodologies

Validate methodology adopted
    ${ROW}=    user gets table row    ${OWNING_PUBLICATION_NAME}    testid:methodologies
    set suite variable    ${ROW}
    user checks element contains    ${ROW}    Adopted
    user checks element contains    ${ROW}    Draft
    user checks element contains    ${ROW}    Not yet published
    user checks element contains link    ${ROW}    Edit
    user checks element contains button    ${ROW}    Remove

Drop adopted Methodology
    user clicks button    Remove    ${ROW}
    ${modal}=    user waits until modal is visible    Remove methodology
    user clicks button    Confirm    ${modal}
    user waits until modal is not visible    Remove methodology

Validate adopted methodology is dropped
    user waits until page contains    There are no methodologies for this publication yet.
