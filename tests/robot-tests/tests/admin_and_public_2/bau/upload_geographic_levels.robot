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

Check if the geographic levels are all present
    user waits until page contains    ${SUBJECT_1_NAME}    %{WAIT_SMALL}
    user clicks radio    ${SUBJECT_1_NAME}
    user clicks element    id:publicationDataStepForm-submit
    user waits until table tool wizard step is available    2    Choose locations    %{WAIT_MEDIUM}
    user checks previous table tool step contains    1    Data set    ${SUBJECT_1_NAME}

Verify and check the expected locations and location options
    user counts accordion form item rows    15
    user checks all locations are present
    user checks all location options are present


*** Keywords ***
user counts accordion form item rows
    [Arguments]    ${number}
    user checks element count is x    testid:filter-accordion-button    ${number}

user checks all locations are present
    user checks page contains button    National
    user checks page contains button    Regional
    user checks page contains button    Opportunity area
    user checks page contains button    Parliamentary constituency
    user checks page contains button    Local authority district
    user checks page contains button    Local authority
    user checks page contains button    RSC region
    user checks page contains button    Local enterprise partnership
    user checks page contains button    Ward
    user checks page contains button    Local skills improvement plan area
    user checks page contains button    English devolved area
    user checks page contains button    Sponsor
    user checks page contains button    MAT
    user checks page contains button    Mayoral combined authority
    user checks page contains button    Police force area

user checks all location options are present
    user clicks button    National
    user checks page contains    England (N)
    user clicks button    Regional
    user checks page contains    Yorkshire and the Humber (R)
    user clicks button    Opportunity area
    user checks page contains    Bolton 001 (O)
    user clicks button    Parliamentary constituency
    user checks page contains    East Yorkshire (PC)
    user clicks button    Local authority district
    user checks page contains    Hartlepool (LAD)
    user clicks button    Local authority
    user checks page contains    Birmingham (LA)
    user clicks button    RSC region
    user checks page contains    North of England (RSC)
    user clicks button    Local enterprise partnership
    user checks page contains    Black Country (LEP)
    user clicks button    Ward
    user checks page contains    Syon (W)
    user clicks button    Local skills improvement plan area
    user checks page contains    Greater Manchester (LSIP)
    user clicks button    English devolved area
    user checks page contains    two (E)
    user clicks button    Sponsor
    user checks page contains    one (S)
    user clicks button    MAT
    user checks page contains    one (MAT)
    user clicks button    Mayoral combined authority
    user checks page contains    one (MCA)
    user clicks button    Police force area
    user checks page contains    E23000001
