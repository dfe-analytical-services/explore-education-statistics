*** Settings ***
Resource    ../../libs/admin-common.robot
Library  Collections

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${TOPIC_NAME}        %{TEST_TOPIC_NAME}
${PUBLICATION_NAME}  UI tests - upload files %{RUN_IDENTIFIER}

*** Test Cases ***
Create test publication
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "${TOPIC_NAME}" from the admin dashboard
    user waits until page contains link    Create new publication     60
    user clicks link  Create new publication
    user creates publication    ${PUBLICATION_NAME}

Verify test publication is created
    [Tags]  HappyPath
    user waits until page contains accordion section  ${PUBLICATION_NAME}
    user opens accordion section  ${PUBLICATION_NAME}
    user waits until accordion section contains text  ${PUBLICATION_NAME}    Methodology
    user waits until accordion section contains text  ${PUBLICATION_NAME}    Releases

Create release
    [Tags]  HappyPath
    user clicks testid element   Create new release link for ${PUBLICATION_NAME}
    user creates release for publication  ${PUBLICATION_NAME}  Academic Year  2025
    user checks summary list contains  Publication title  ${PUBLICATION_NAME}

Navigate to 'Data and files' page
    [Tags]  HappyPath
    user clicks link  Data and files
    user waits until h1 is visible  ${PUBLICATION_NAME}

Upload a ZIP file subject
    [Documentation]   EES-1397
    [Tags]  HappyPath
    user enters text into element   id:dataFileUploadForm-subjectTitle    Absence in PRUs
    user clicks radio    ZIP file
    user waits until page contains element  id:dataFileUploadForm-zipFile
    user chooses file   id:dataFileUploadForm-zipFile    ${CURDIR}${/}files${/}upload-zip-test.zip
    user clicks button   Upload data files

    user waits until h2 is visible   Uploaded data files
    user waits until page contains accordion section   Absence in PRUs
    user opens accordion section   Absence in PRUs

    ${section}=  user gets accordion section content element  Absence in PRUs
    user checks summary list contains  Subject title    Absence in PRUs  ${section}
    user checks summary list contains  Data file        absence_in_prus.csv  ${section}
    user checks summary list contains  Metadata file    absence_in_prus.meta.csv  ${section}
    user checks summary list contains  Status           Complete  ${section}  180

    # EES-1397
    #user checks summary list contains  Number of rows   613  ${section}
    #user checks summary list contains  Data file size   141 Kb  ${section}

Check Absence in PRUs subject appears in 'Data blocks' page
    [Tags]  HappyPath
    user clicks link   Data blocks
    user waits until h2 is visible   Choose a subject

    user waits until page contains  Absence in PRUs

Navigate to 'Data and files' page - 'File uploads' tab
    [Tags]  HappyPath
    user clicks link  Data and files
    user waits until h2 is visible   Add data file to release
    user clicks link  File uploads
    user waits until h2 is visible  Add file to release
    user waits until page contains  No files have been uploaded

Validate cannot upload empty file
    [Tags]  HappyPath
    user enters text into element  id:fileUploadForm-name   Empty test
    user chooses file   id:fileUploadForm-file      ${CURDIR}${/}files${/}empty-file.txt
    user clicks button  Upload file
    user waits until page contains  Choose a file that is not empty

Upload multiple files
    [Tags]  HappyPath
    user enters text into element  id:fileUploadForm-name   Test 1
    user chooses file   id:fileUploadForm-file      ${CURDIR}${/}files${/}test-file-1.txt
    user clicks button  Upload file

    user waits until page contains accordion section   test 1
    user opens accordion section   test 1  id:file-uploads

    ${section_1}=  user gets accordion section content element  test 1  id:file-uploads
    user checks summary list contains  Name         test 1              ${section_1}
    user checks summary list contains  File         test-file-1.txt     ${section_1}
    user checks summary list contains  File size    12 B                ${section_1}

    user enters text into element  id:fileUploadForm-name   Test 2
    user chooses file   id:fileUploadForm-file      ${CURDIR}${/}files${/}test-file-2.txt
    user clicks button  Upload file

    user waits until page contains accordion section   test 2
    user opens accordion section   test 2  id:file-uploads

    ${section_2}=  user gets accordion section content element  test 2  id:file-uploads
    user checks summary list contains  Name         test 2              ${section_2}
    user checks summary list contains  File         test-file-2.txt     ${section_2}
    user checks summary list contains  File size    24 B                ${section_2}

    user checks there are x accordion sections  2  id:file-uploads

Delete file
    [Tags]  HappyPath
    ${file_2_section}=  user gets accordion section content element  test 2  id:file-uploads
    user clicks button   Delete file  ${file_2_section}
    user waits until h1 is visible   Confirm deletion of file
    user clicks button  Confirm

    user waits until page does not contain accordion section   test 2
    user waits until page contains accordion section  test 1
    user checks there are x accordion sections  1   id:file-uploads
