*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser

Force Tags          Admin    Local    Dev    AltersData

*** Variables ***
${PUBLICATION_NAME}=    UI tests - link external methodology publication %{RUN_IDENTIFIER}

*** Test Cases ***
Link Publication to External Methodology
    [Tags]    HappyPath
    user creates test publication via api    ${PUBLICATION_NAME}
    ${accordion}=    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    user checks element contains button    ${accordion}    Create methodology
    user checks element contains button    ${accordion}    Link to an externally hosted methodology
    user links publication to external methodology    ${PUBLICATION_NAME}
    ${accordion}=    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    user checks page contains link with text and url    ${PUBLICATION_NAME} (external methodology)
    ...    https://example.com    ${accordion}
    user checks element contains button    ${accordion}    Edit externally hosted methodology
    user checks element contains button    ${accordion}    Remove
    user checks element contains button    ${accordion}    Create methodology

Edit the External Methodology of the Publication
    user edits an external methodology    ${PUBLICATION_NAME}
    ${accordion}=    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    user checks page contains link with text and url    ${PUBLICATION_NAME} updated (external methodology)
    ...    https://example.com/updated    ${accordion}

Remove the External Methodology from Publication
    user removes an external methodology from publication    ${PUBLICATION_NAME}
    ${accordion}=    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    user checks element contains button    ${accordion}    Create methodology
    user checks element contains button    ${accordion}    Link to an externally hosted methodology
    user checks element does not contain button    ${accordion}    Edit externally hosted methodology
    user checks element does not contain button    ${accordion}    Remove
