*** Settings ***
Library             Collections
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${PUBLICATION_NAME}=    Upload all geographic levels %{RUN_IDENTIFIER}
${SUBJECT_1_NAME}=      All geographies subject name


*** Test Cases ***
Create test publication and release via api
    ${publication_id}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${publication_id}    AY    2025

Navigate to 'Data and files' page
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    Academic year 2025/26

    user clicks link    Data and files
    user waits until h1 is visible    ${PUBLICATION_NAME}

Upload datafile
    user uploads subject and waits until complete    ${SUBJECT_1_NAME}    all_geographies.csv
    ...    all_geographies.meta.csv    ${FILES_DIR}

Start creating a data block
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks
    user waits until page contains    No data blocks have been created.

    user clicks link    Create data block
    user waits until table tool wizard step is available    1    Select a data set

Select subject "UI test subject"
    user waits until page contains    All geographies subject name    %{WAIT_SMALL}
    user clicks radio    All geographies subject name
    user clicks element    id:publicationDataStepForm-submit
    user waits until table tool wizard step is available    2    Choose locations    %{WAIT_MEDIUM}
    user checks previous table tool step contains    1    Data set    All geographies subject name
    user counts accordion form item rows    15


*** Keywords ***
user counts accordion form item rows
    [Arguments]    ${number}
    user checks element count is x    testid:filter-accordion-button    ${number}
