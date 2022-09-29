*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${PUBLICATION_NAME}=    UI tests - link external methodology publication %{RUN_IDENTIFIER}


*** Test Cases ***
Link Publication to External Methodology
    user creates test publication via api    ${PUBLICATION_NAME}
    ${accordion}=    bau user goes to publication page from dashboard    ${PUBLICATION_NAME}
    user checks element contains button    ${accordion}    Create methodology
    user checks element contains link    ${accordion}    Use an external methodology
    user links publication to external methodology    ${PUBLICATION_NAME}
    user waits until page contains title    Dashboard
    ${accordion}=    bau user goes to publication page from dashboard    ${PUBLICATION_NAME}
    ${details}=    user opens details dropdown    External methodology (External)    ${accordion}
    user checks page contains link with text and url    https://example.com
    ...    https://example.com    ${accordion}
    user checks element contains link    ${accordion}    Edit external methodology
    user checks element contains button    ${accordion}    Remove external methodology
    user checks element contains button    ${accordion}    Create methodology

Edit the External Methodology of the Publication
    user edits an external methodology    ${PUBLICATION_NAME}
    user waits until page contains title    Dashboard
    ${accordion}=    bau user goes to publication page from dashboard    ${PUBLICATION_NAME}
    ${details}=    user opens details dropdown    External methodology updated (External)    ${accordion}
    user checks page contains link with text and url    https://example.com/updated
    ...    https://example.com/updated    ${accordion}

Remove the External Methodology from Publication
    user removes an external methodology from publication    ${PUBLICATION_NAME}
    ${accordion}=    bau user goes to publication page from dashboard    ${PUBLICATION_NAME}
    user checks element contains button    ${accordion}    Create methodology
    user checks element contains link    ${accordion}    Use an external methodology
    user checks element does not contain button    ${accordion}    Edit externally hosted methodology
    user checks element does not contain button    ${accordion}    Remove external methodology
