*** Settings ***
Library             ../libs/admin_api.py
Resource            ../libs/admin-common.robot
Resource            ../libs/admin/manage-content-common.robot
Resource            ../libs/public-api-common.robot

Force Tags          Admin    PublicApi    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Variables ***
${PUBLICATION_NAME}=    Public API - BAU compatibility override %{RUN_IDENTIFIER}
${RELEASE_NAME}=        Financial year 3000-01
${SUBJECT_NAME_1}=      ${PUBLICATION_NAME} - Subject 1
${SUBJECT_NAME_2}=      ${PUBLICATION_NAME} - Subject 2


*** Test Cases ***
Create publication and release
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    FY    3000
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}

Upload an API compatible and an API incompatible data file
    # dates.csv has column names over the screener's API character limit,
    # so fails the API compatibility checks
    user uploads subject and waits until complete    ${SUBJECT_NAME_1}    seven_filters.csv    seven_filters.meta.csv
    ...    ${PUBLIC_API_FILES_DIR}
    user uploads subject and waits until complete    ${SUBJECT_NAME_2}    dates.csv    dates.meta.csv

Check BAU user can choose from all data files when creating an API data set
    user scrolls to the top of the page
    user clicks link    API data sets
    user waits until h2 is visible    API data sets

    user clicks button    Create API data set
    ${modal}=    user waits until modal is visible    Create a new API data set
    user waits until page contains element    name:releaseFileId
    user waits until select contains option    name:releaseFileId    ${SUBJECT_NAME_1}
    user checks select contains option    name:releaseFileId    ${SUBJECT_NAME_2} (not API compatible)
    user clicks button    Cancel    ${modal}
    user waits until modal is not visible    Create a new API data set

Create API data set from the API incompatible data file
    user creates API data set and opens details    ${SUBJECT_NAME_2}
    # The override lets BAU create the data set, but the file's long column names
    # still cause processing to fail.
    user waits until draft API data set status contains    Failed

Check the API incompatible data file is flagged on the API data set details page
    user checks page contains element    testid:api-incompatible-warning
    user checks element contains    testid:api-incompatible-warning
    ...    failed the API compatibility checks during screening
    user checks summary list contains    Data set file    ${SUBJECT_NAME_2}
    user checks summary list contains    API compatible    No
