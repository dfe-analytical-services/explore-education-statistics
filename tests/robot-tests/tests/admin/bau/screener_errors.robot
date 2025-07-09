*** Settings ***
Library             ../../libs/admin_api.py
Library             ../../libs/dates_and_times.py
Resource            ../../libs/admin-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${PUBLICATION_NAME}=    Screener errors %{RUN_IDENTIFIER}


*** Test Cases ***
Create test publication and release via api
    ${publication_id}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${publication_id}    AY    2025

Navigate to 'Data and files' page
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    Academic year 2025/26

    user clicks link    Data and files
    user waits until h1 is visible    ${PUBLICATION_NAME}

Upload an invalid data set and wait for it to fail the screener checks
    user uploads subject and waits until failed screening    Invalid data set
    ...    invalid-data-set.csv
    ...    invalid-data-set.meta.csv

    user waits until page contains data uploads table
    user checks invalid data set details in data uploads table

Check the screening failures
    user clicks button in table cell    1    4    View details    testid:Data files table
    user waits until modal is visible    Data set details
    user waits until h3 is visible    Screener test failures
    user checks invalid data set details in modal
    user checks page does not contain button    Continue importing

Check all screening checks
    user clicks element    id:screener-results-all-tab
    user waits until h3 is not visible    Screener test failures
    user waits until h3 is visible    Full breakdown of 3 tests checked against this file
    user checks all invalid data set screener results in modal

Check the file details in the modal
    user clicks element    id:file-details-tab
    user waits until h3 is not visible    Full breakdown of 3 tests checked against this file
    user waits until h3 is visible    File details
    user checks invalid data set file details in modal

Close the screener modal and delete the invalid data set
    user clicks modal button    Cancel
    user clicks button    Delete files
    user waits until modal is visible    Confirm deletion of selected data files
    user checks modal contains text    Are you sure you want to delete Invalid data set?
    user checks modal contains text    This version of the data set has not yet been imported.
    user clicks modal button    Confirm
    user waits until page does not contain data uploads table

Upload an invalid data set in a ZIP file and wait for it to fail the screener checks
    user enters text into element    id:dataFileUploadForm-title    Invalid data set
    user clicks radio    ZIP file
    user waits until page contains element    id:dataFileUploadForm-zipFile
    user chooses file    id:dataFileUploadForm-zipFile    ${FILES_DIR}invalid-data-set.zip
    user clicks button    Upload data files

    user waits until page contains data uploads table
    user checks invalid data set details in data uploads table

Check the screening failures for the ZIP file
    user clicks button in table cell    1    4    View details    testid:Data files table
    user waits until modal is visible    Data set details
    user waits until h3 is visible    Screener test failures
    user checks invalid data set details in modal
    user checks page does not contain button    Continue importing

Check all screening checks for the ZIP file
    user clicks element    id:screener-results-all-tab
    user waits until h3 is not visible    Screener test failures
    user waits until h3 is visible    Full breakdown of 3 tests checked against this file
    user checks all invalid data set screener results in modal

Check the file details in the modal for the ZIP file
    user clicks element    id:file-details-tab
    user waits until h3 is not visible    Full breakdown of 3 tests checked against this file
    user waits until h3 is visible    File details
    user checks invalid data set file details in modal


*** Keywords ***
user checks screener results in modal
    [Arguments]
    ...    ${row}
    ...    ${screener_code}
    ...    ${screener_message}
    ...    ${screener_result}
    user waits until modal table cell contains    ${row}    1    ${screener_code}
    user waits until modal table cell contains    ${row}    1    ${screener_message}
    user waits until modal table cell contains    ${row}    2    ${screener_result}

user checks invalid data set details in data uploads table
    user checks table cell contains    1    1    Invalid data set    testid:Data files table
    user checks table cell contains    1    2    680 B    testid:Data files table
    user waits until table cell contains    1    3    Failed screening    testid:Data files table
    user checks table cell contains    1    4    View details    testid:Data files table
    user checks table cell contains    1    4    Delete files    testid:Data files table

user checks invalid data set details in modal
    user checks modal warning text contains    You will need to delete this file
    user checks screener results in modal    1    check_empty_cols
    ...    The following columns in 'invalid-data-set.csv' are empty: 'URN', 'Estab', 'school_name',    Fail

user checks invalid data set file details in modal
    user checks summary list contains    Title    Invalid data set    testid:Data file details
    user checks summary list contains    Data file    invalid-data-set.csv    testid:Data file details
    user checks summary list contains    Meta file    invalid-data-set.meta.csv    testid:Data file details
    user checks summary list contains    Size    680 B    testid:Data file details
    user checks summary list contains    Status    Failed screening    testid:Data file details
    user checks summary list contains    Uploaded by    ees-test.bau1@education.gov.uk    testid:Data file details
    ${date}=    get london date
    user checks summary list contains    Date uploaded    ${date}    testid:Data file details

user checks all invalid data set screener results in modal
    user checks screener results in modal    1    check_filename_spaces
    ...    'invalid-data-set.csv' does not have spaces in the filename.    Pass
    user checks screener results in modal    2    check_filename_spaces
    ...    'invalid-data-set.meta.csv' does not have spaces in the filename.    Pass
    user checks screener results in modal    3    check_empty_cols
    ...    The following columns in 'invalid-data-set.csv' are empty: 'URN',    Fail
