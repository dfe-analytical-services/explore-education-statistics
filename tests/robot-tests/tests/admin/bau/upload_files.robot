*** Settings ***
Resource    ../../libs/admin-common.robot
Library  Collections
Library  ../../libs/admin_api.py

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${TOPIC_NAME}        %{TEST_TOPIC_NAME}
${PUBLICATION_NAME}  UI tests - upload files %{RUN_IDENTIFIER}

*** Test Cases ***
Create test publication and release via api
    [Tags]  HappyPath
    ${PUBLICATION_ID}=  user creates test publication via api   ${PUBLICATION_NAME}
    user create test release via api  ${PUBLICATION_ID}   AY    2025

Navigate to 'Data and files' page
    [Tags]  HappyPath
    user navigates to release summary from admin dashboard  ${PUBLICATION_NAME}   Academic Year 2025/26 (not Live)

    user clicks link  Data and files
    user waits until h1 is visible  ${PUBLICATION_NAME}

Upload a ZIP file subject
    [Documentation]   EES-1397
    [Tags]  HappyPath
    user enters text into element   id:dataFileUploadForm-subjectTitle    Absence in PRUs
    user clicks radio    ZIP file
    user waits until page contains element  id:dataFileUploadForm-zipFile
    user chooses file   id:dataFileUploadForm-zipFile    ${FILES_DIR}upload-zip-test.zip
    user clicks button   Upload data files

    user waits until h2 is visible   Uploaded data files
    user waits until page contains accordion section   Absence in PRUs
    user opens accordion section   Absence in PRUs

    ${section}=  user gets accordion section content element  Absence in PRUs

    # To ensure "Data file size" and "Number of rows" will be filled
    user waits until page does not contain  Queued    60

    user checks headed table body row contains  Subject title    Absence in PRUs  ${section}
    user checks headed table body row contains  Data file        absence_in_prus.csv  ${section}
    user checks headed table body row contains  Metadata file    absence_in_prus.meta.csv  ${section}
    user checks headed table body row contains  Data file size   141 Kb  ${section}
    user checks headed table body row contains  Number of rows   613  ${section}
    user checks headed table body row contains  Status           Complete  ${section}  %{WAIT_180_SECONDS}

Check Absence in PRUs subject appears in 'Data blocks' page
    [Tags]  HappyPath
    user clicks link   Data blocks
    user waits until h2 is visible  Data blocks

    user clicks link  Create data block
    user waits until h2 is visible  Create data block
    user waits until table tool wizard step is available    Choose a subject

    user waits until page contains  Absence in PRUs

Navigate to 'Data and files' page - 'Ancillary file uploads' tab
    [Tags]  HappyPath
    user clicks link  Data and files
    user waits until h2 is visible   Add data file to release
    user clicks link  Ancillary file uploads
    user waits until h2 is visible  Add file to release
    user waits until page contains  No files have been uploaded

Validate cannot upload empty file
    [Tags]  HappyPath
    user enters text into element  id:fileUploadForm-name   Empty test
    user chooses file   id:fileUploadForm-file      ${FILES_DIR}empty-file.txt
    user clicks button  Upload file
    user waits until page contains  Choose a file that is not empty

Upload multiple files
    [Tags]  HappyPath
    user enters text into element  id:fileUploadForm-name   Test 1
    user chooses file   id:fileUploadForm-file      ${FILES_DIR}test-file-1.txt
    user clicks button  Upload file

    user waits until page contains accordion section   Test 1
    user opens accordion section   Test 1  id:file-uploads

    ${section_1}=  user gets accordion section content element  Test 1  id:file-uploads
    user checks summary list contains  Name         Test 1              ${section_1}
    user checks summary list contains  File         test-file-1.txt     ${section_1}
    user checks summary list contains  File size    12 B                ${section_1}

    user enters text into element  id:fileUploadForm-name   Test 2
    user chooses file   id:fileUploadForm-file      ${FILES_DIR}test-file-2.txt
    user clicks button  Upload file

    user waits until page contains accordion section   Test 2
    user opens accordion section   Test 2  id:file-uploads

    ${section_2}=  user gets accordion section content element  Test 2  id:file-uploads
    user checks summary list contains  Name         Test 2              ${section_2}
    user checks summary list contains  File         test-file-2.txt     ${section_2}
    user checks summary list contains  File size    24 B                ${section_2}

    user checks there are x accordion sections  2  id:file-uploads

Delete file
    [Tags]  HappyPath
    ${file_2_section}=  user gets accordion section content element  Test 2  id:file-uploads
    user clicks button   Delete file  ${file_2_section}
    user waits until h1 is visible   Confirm deletion of file
    user clicks button  Confirm

    user waits until page does not contain accordion section   Test 2
    user waits until page contains accordion section  Test 1
    user checks there are x accordion sections  1   id:file-uploads
