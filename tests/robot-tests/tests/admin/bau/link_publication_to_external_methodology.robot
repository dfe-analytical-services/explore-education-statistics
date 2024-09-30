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
    user navigates to methodologies on publication page    ${PUBLICATION_NAME}

    user checks page contains button    Create new methodology
    user waits until page contains link    Add external methodology
    user waits until page contains link    Adopt an existing methodology

    user clicks link    Add external methodology
    user waits until h2 is visible    Link to an externally hosted methodology
    user enters text into element    label:Link title    Test external methodology
    user enters text into element    label:URL    https://test.example.com
    user clicks button    Save

Verify external methodology has been added
    ${ROW}=    user gets table row    Test external methodology    testid:methodologies
    user checks element contains    ${ROW}    External
    user checks element contains link    ${ROW}    Edit
    user checks element contains link    ${ROW}    View
    user checks page contains link with text and url    View    https://test.example.com    ${ROW}
    user checks element contains button    ${ROW}    Remove

Verify Manage methodologies page is correct
    user checks page contains button    Create new methodology
    user checks page does not contain link    Add external methodology
    user waits until page contains link    Adopt an existing methodology

Edit the External Methodology of the Publication
    ${ROW}=    user gets table row    Test external methodology    testid:methodologies
    user clicks element    xpath://*[text()="Edit"]    ${ROW}
    user waits until h2 is visible    Edit external methodology link

    user checks input field contains    label:Link title    Test external methodology
    user checks input field contains    label:URL    https://test.example.com

    user enters text into element    label:Link title    Test external methodology updated
    user enters text into element    label:URL    https://test.example.com/updated

    user clicks button    Save
    user waits until h2 is visible    Manage methodologies

Verify external methodology has been updated
    ${ROW}=    user gets table row    Test external methodology updated    testid:methodologies
    user checks element contains    ${ROW}    External
    user checks element contains link    ${ROW}    Edit
    user checks element contains link    ${ROW}    View
    user checks page contains link with text and url    View    https://test.example.com/updated    ${ROW}
    user checks element contains button    ${ROW}    Remove

Remove the External Methodology from Publication
    ${ROW}=    user gets table row    Test external methodology updated    testid:methodologies
    user clicks element    xpath://*[text()="Remove"]    ${ROW}

    ${modal}=    user waits until modal is visible    Remove external methodology
    user clicks button    Confirm    ${modal}
    user waits until modal is not visible    Remove external methodology

    user waits until h2 is visible    Manage methodologies

    user checks page does not contain    Test external methodology updated
    user checks page contains button    Create new methodology
    user waits until page contains link    Add external methodology
    user waits until page contains link    Adopt an existing methodology
