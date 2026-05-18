*** Settings ***
Resource    ./common.robot


*** Keywords ***
user uploads subject and waits until complete
    [Arguments]
    ...    ${SUBJECT_NAME}
    ...    ${SUBJECT_FILE}
    ...    ${META_FILE}
    ...    ${FOLDER}=${FILES_DIR}
    user uploads subject
    ...    ${SUBJECT_NAME}
    ...    ${SUBJECT_FILE}
    ...    ${META_FILE}
    ...    Complete
    ...    ${FOLDER}

user uploads subject and waits until pending review
    [Arguments]
    ...    ${SUBJECT_NAME}
    ...    ${SUBJECT_FILE}
    ...    ${META_FILE}
    ...    ${FOLDER}=${FILES_DIR}
    user uploads subject
    ...    ${SUBJECT_NAME}
    ...    ${SUBJECT_FILE}
    ...    ${META_FILE}
    ...    Pending review
    ...    ${FOLDER}

user uploads subject and waits until failed screening
    [Arguments]
    ...    ${SUBJECT_NAME}
    ...    ${SUBJECT_FILE}
    ...    ${META_FILE}
    ...    ${FOLDER}=${FILES_DIR}
    user uploads subject
    ...    ${SUBJECT_NAME}
    ...    ${SUBJECT_FILE}
    ...    ${META_FILE}
    ...    Failed screening
    ...    ${FOLDER}

user uploads subject
    [Arguments]
    ...    ${SUBJECT_NAME}
    ...    ${SUBJECT_FILE}
    ...    ${META_FILE}
    ...    ${IMPORT_STATUS}
    ...    ${FOLDER}=${FILES_DIR}
    user clicks link    Data and files
    user waits until page contains element    id:dataFileUploadForm-title    %{WAIT_SMALL}
    user enters text into element    id:dataFileUploadForm-title    ${SUBJECT_NAME}
    user chooses file    id:dataFileUploadForm-dataFile    ${FOLDER}${SUBJECT_FILE}
    user chooses file    id:dataFileUploadForm-metadataFile    ${FOLDER}${META_FILE}
    user clicks button    Upload data files
    user waits until page finishes loading    %{WAIT_DATA_FILE_IMPORT}
    user waits until page contains data uploads table

    IF    "${IMPORT_STATUS}" == "Complete"
        user confirms upload to complete import    ${SUBJECT_NAME}
    ELSE
        user waits until data file import is in status    ${SUBJECT_NAME}    ${IMPORT_STATUS}
    END

user uploads subject replacement
    [Arguments]
    ...    ${SUBJECT_NAME}
    ...    ${SUBJECT_FILE}
    ...    ${META_FILE}
    ...    ${FOLDER}=${FILES_DIR}
    user clicks link    Data and files
    user waits until page contains element    id:dataFileUploadForm-title    %{WAIT_SMALL}
    user enters text into element    id:dataFileUploadForm-title    ${SUBJECT_NAME}
    user chooses file    id:dataFileUploadForm-dataFile    ${FOLDER}${SUBJECT_FILE}
    user chooses file    id:dataFileUploadForm-metadataFile    ${FOLDER}${META_FILE}
    user clicks button    Upload data files
    user waits until page contains data uploads table

user confirms upload to complete import
    [Arguments]
    ...    ${SUBJECT_NAME}
    ...    ${WAIT_FOR_IMPORT_TO_COMPLETE}=True
    ${row}=    user gets table row    ${SUBJECT_NAME}    testid:Data files table
    ${statusText}=    Get Text    xpath=//tr[td[1][text()[contains(.,'${SUBJECT_NAME}')]]]/td[3]/strong
    ${button}=    user gets button element    View details    ${row}
    user clicks element    ${button}

    IF    '${statusText}' == 'Pending review'
        user acknowledges any warnings in screener modal
        user clicks button    Continue import with warnings
    ELSE
        user waits until h3 is visible    Screener test failures
        user clicks button    Continue import (override failures)
    END

    user waits until modal is not visible    Screener test warnings

    IF    ${WAIT_FOR_IMPORT_TO_COMPLETE}
        user waits until data file import is complete    ${SUBJECT_NAME}
    END

user confirms replacement upload
    [Arguments]
    ...    ${SUBJECT_NAME}
    ...    ${EXPECTED_STATUS}=Ready
    ...    ${ROW}=1
    user clicks element    testid:data-set-upload-row-${ROW}-view-details

    user waits until modal is visible    Data set details
    ${statusText}=    Get Text    xpath=//tr[td[1][text()[contains(.,'${SUBJECT_NAME}')]]]/td[3]/strong
    IF    '${statusText}' == 'Pending review'
        user acknowledges any warnings in screener modal
        user clicks button    Continue import with warnings
    ELSE
        User waits until h3 is visible    Screener test failures
        user clicks button    Continue import (override failures)
    END

    user waits until data file replacement is in status    ${SUBJECT_NAME}    ${EXPECTED_STATUS}

user waits until data upload is completed
    [Arguments]
    ...    ${SUBJECT_NAME}
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release
    user waits until data file import is complete    ${SUBJECT_NAME}

user waits until data files table contains subject
    [Arguments]
    ...    ${SUBJECT_NAME}
    user waits until table contains row with    ${SUBJECT_NAME}    testid:Data files table    %{WAIT_DATA_FILE_IMPORT}

user waits until data file import is complete
    [Arguments]
    ...    ${SUBJECT_NAME}
    user waits until data file import is in status    ${SUBJECT_NAME}    Complete

user waits until data file import is in status
    [Arguments]
    ...    ${subject_name}
    ...    ${status}
    user waits until page contains data uploads table
    user waits until parent contains element
    ...    testid:Data files table
    ...    xpath:.//tbody/tr/td[contains(., "${subject_name}")]/../td[contains(., "${status}")]
    ...    %{WAIT_DATA_FILE_IMPORT}

user waits until data file replacement is in status
    [Arguments]
    ...    ${SUBJECT_NAME}
    ...    ${STATUS}
    user waits until page contains element    testid:Data file replacements table
    user waits until parent contains element
    ...    testid:Data file replacements table
    ...    xpath:.//tbody/tr/td[contains(., "${SUBJECT_NAME}")]/../td[contains(., "${STATUS}")]
    ...    %{WAIT_DATA_FILE_IMPORT}

user waits until page contains data uploads table
    user waits until page contains testid    Data files table    %{WAIT_DATA_FILE_IMPORT}

user waits until page does not contain data uploads table
    user waits until page does not contain testid    Data files table

user acknowledges any warnings in screener modal
    user clicks element    id:screener-results-filtered-tab
    user waits until h3 is visible    Screener test warnings
    user clicks all checkboxes in parent    screener-results-filtered

user deletes subject file
    [Arguments]    ${SUBJECT_NAME}
    user waits until page contains data uploads table
    ${row}=    user gets table row    ${SUBJECT_NAME}    testid:Data files table
    user scrolls to element    ${row}
    ${button}=    user gets button element    Delete files    ${row}
    user clicks element    ${button}
    user clicks button    Confirm

user adds data guidance for subject
    [Arguments]
    ...    ${subject_name}
    ...    ${guidance_text}

    user waits until page contains accordion section    ${subject_name}
    user enters text into data guidance data file content editor    ${subject_name}
    ...    ${guidance_text}

user navigates to Data Guidance page and adds data guidance for subject
    [Arguments]
    ...    ${subject_name}
    ...    ${guidance_text}
    user clicks link    Data and files
    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance
    user adds main data guidance content
    user waits until page contains element    id:dataGuidance-dataFiles
    user waits until page contains accordion section    ${subject_name}
    user enters text into data guidance data file content editor    ${subject_name}    ${guidance_text}
    user clicks button    Save guidance
    user waits until page contains button    Edit guidance

user adds main data guidance content
    [Arguments]    ${text}=Test data guidance content
    user waits until page contains element    id:dataGuidanceForm-content
    user enters text into element    id:dataGuidanceForm-content    ${text}

user gets data guidance data file content editor
    [Arguments]    ${accordion_heading}
    user waits until page contains element    id:dataGuidance-dataFiles
    ${accordion}=    user gets accordion section content element    ${accordion_heading}    id:dataGuidance-dataFiles
    user waits until parent contains element    ${accordion}    label:File guidance content
    ${editor}=    get child element    ${accordion}    label:File guidance content
    [Return]    ${editor}

user enters text into data guidance data file content editor
    [Arguments]    ${accordion_heading}    ${text}
    ${accordion}=    user gets accordion section content element    ${accordion_heading}    id:dataGuidance-dataFiles
    ${editor}=    user gets data guidance data file content editor    ${accordion_heading}
    user clicks element    ${editor}
    user enters text into element    ${editor}    ${text}
