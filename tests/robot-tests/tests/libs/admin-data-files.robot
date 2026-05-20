*** Settings ***
Resource    ./common.robot


*** Variables ***
${DATA_FILES_TABLE_SELECTOR}=       testid:Data files table
${REPLACEMENTS_TABLE_SELECTOR}=     testid:Data file replacements table


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
    ...    ${WAIT_FOR_SCREENING_TO_COMPLETE}=True

    upload subject
    ...    ${SUBJECT_NAME}
    ...    ${SUBJECT_FILE}
    ...    ${META_FILE}
    ...    ${FOLDER}

    IF    ${WAIT_FOR_SCREENING_TO_COMPLETE}
        user waits for screening to complete    ${SUBJECT_NAME}
    END

    IF    "${IMPORT_STATUS}" == "Complete"
        user confirms upload to complete import    ${SUBJECT_NAME}
    ELSE
        user waits until data file import is in status    ${SUBJECT_NAME}    ${IMPORT_STATUS}
    END

user confirms upload to complete import
    [Arguments]
    ...    ${SUBJECT_NAME}
    ...    ${WAIT_FOR_SCREENING_TO_COMPLETE}=True
    ...    ${WAIT_FOR_IMPORT_TO_COMPLETE}=True
    ...    ${REPLACEMENT}=False

    IF    ${REPLACEMENT}
        ${DATA_FILES_TABLE_SELECTOR}=    Set Variable    ${REPLACEMENTS_TABLE_SELECTOR}
    ELSE
        ${DATA_FILES_TABLE_SELECTOR}=    Set Variable    ${DATA_FILES_TABLE_SELECTOR}
    END

    IF    ${WAIT_FOR_SCREENING_TO_COMPLETE}
        ${row}=    user gets table row    ${SUBJECT_NAME}    ${DATA_FILES_TABLE_SELECTOR}
        ${statusText}=    get status for subject    ${SUBJECT_NAME}
        ${button}=    user gets button element    View details    ${row}
        user clicks element    ${button}
        user waits until modal is visible    Data set details

        IF    '${statusText}' == 'Pending review'
            user waits until h3 is visible    Screener test warnings
            user acknowledges any warnings in screener modal
            user clicks button    Continue import with warnings
        ELSE IF    '${statusText}' == 'Pending import'
            user waits until h3 is visible    Full breakdown    exact_match=False
            user clicks button    Continue import
        ELSE
            user waits until h3 is visible    Screener test failures
            user clicks button    Continue import (override failures)
        END

        user waits until modal is not visible    Data set details
    END

    IF    ${WAIT_FOR_IMPORT_TO_COMPLETE}
        user waits until data file import is complete    ${SUBJECT_NAME}
    END

user uploads subject replacement
    [Arguments]
    ...    ${SUBJECT_NAME}
    ...    ${SUBJECT_FILE}
    ...    ${META_FILE}
    ...    ${FOLDER}=${FILES_DIR}
    ...    ${EXPECT_UPLOAD_TO_SUCCEED}=True
    ...    ${WAIT_FOR_SCREENING_TO_COMPLETE}=True

    upload subject
    ...    ${SUBJECT_NAME}
    ...    ${SUBJECT_FILE}
    ...    ${META_FILE}
    ...    ${FOLDER}

    IF    ${EXPECT_UPLOAD_TO_SUCCEED}
        user waits until page contains data uploads replacements table

        IF    ${WAIT_FOR_SCREENING_TO_COMPLETE}
            user waits for screening to complete    ${SUBJECT_NAME}
        END
    END

user confirms replacement upload
    [Arguments]
    ...    ${SUBJECT_NAME}
    ...    ${EXPECTED_STATUS}=Ready
    ...    ${ROW}=1

    user confirms upload to complete import
    ...    ${SUBJECT_NAME}
    ...    WAIT_FOR_IMPORT_TO_COMPLETE=False
    ...    REPLACEMENT=True

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
    user waits until table contains row with    ${SUBJECT_NAME}    ${DATA_FILES_TABLE_SELECTOR}
    ...    %{WAIT_DATA_FILE_IMPORT}

user waits until data file import is complete
    [Arguments]
    ...    ${SUBJECT_NAME}
    user waits until data file import is in status    ${SUBJECT_NAME}    Complete

user waits until data file import is in status
    [Arguments]
    ...    ${subject_name}
    ...    ${status}
    ...    ${replacement}=False

    IF    ${replacement}
        ${DATA_FILES_TABLE_SELECTOR}=    Set Variable    ${REPLACEMENTS_TABLE_SELECTOR}
    ELSE
        ${DATA_FILES_TABLE_SELECTOR}=    Set Variable    ${DATA_FILES_TABLE_SELECTOR}
    END

    user waits until page contains element    ${DATA_FILES_TABLE_SELECTOR}
    user waits until parent contains element
    ...    ${DATA_FILES_TABLE_SELECTOR}
    ...    xpath:.//tbody/tr/td[contains(., "${subject_name}")]/../td[contains(., "${status}")]
    ...    %{WAIT_DATA_FILE_IMPORT}

user waits until data file replacement is in status
    [Arguments]
    ...    ${SUBJECT_NAME}
    ...    ${STATUS}
    user waits until page contains element    ${REPLACEMENTS_TABLE_SELECTOR}
    user waits until parent contains element
    ...    ${REPLACEMENTS_TABLE_SELECTOR}
    ...    xpath:.//tbody/tr/td[contains(., "${SUBJECT_NAME}")]/../td[contains(., "${STATUS}")]
    ...    %{WAIT_DATA_FILE_IMPORT}

user waits until page contains data uploads table
    user waits until page contains element    ${DATA_FILES_TABLE_SELECTOR}    %{WAIT_DATA_FILE_IMPORT}

user waits until page contains data uploads replacements table
    user waits until page contains element    ${REPLACEMENTS_TABLE_SELECTOR}    %{WAIT_DATA_FILE_IMPORT}

user waits until page does not contain data uploads table
    user waits until page does not contain element    ${DATA_FILES_TABLE_SELECTOR}

user acknowledges any warnings in screener modal
    user clicks element    id:screener-results-filtered-tab
    user waits until h3 is visible    Screener test warnings
    user clicks all checkboxes in parent    screener-results-filtered

user deletes subject file
    [Arguments]    ${SUBJECT_NAME}
    user waits until page contains data uploads table
    ${row}=    user gets table row    ${SUBJECT_NAME}    ${DATA_FILES_TABLE_SELECTOR}
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

get status for subject
    [Arguments]    ${SUBJECT_NAME}
    ${statusText}=    Get Text    xpath=//*[@data-testid='${SUBJECT_NAME}-status']
    [Return]    ${statusText}

user waits until subject has status
    [Arguments]    ${SUBJECT_NAME}    ${STATUS}
    user waits until page contains element
    ...    xpath=//*[@data-testid='${SUBJECT_NAME}-status' and contains(., '${STATUS}')]

upload subject
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
    user waits until page finishes loading    %{WAIT_DATA_FILE_IMPORT}
    user waits until page contains data uploads table

user waits for screening to complete
    [Arguments]
    ...    ${SUBJECT_NAME}

    # TODO EES-7130 - update to not require the hard page refresh when EES-7007 is complete.
    sleep    40
    user reloads page
    user waits until page finishes loading
    user waits until page does not contain element    testid:${SUBJECT_NAME}-screener-progress-bar
