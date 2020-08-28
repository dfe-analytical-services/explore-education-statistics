*** Settings ***
Resource    ../../libs/admin-common.robot
Library  Collections

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${TOPIC_NAME}        %{TEST_TOPIC_NAME}
${PUBLICATION_NAME}  UI tests - ancillary files %{RUN_IDENTIFIER}

*** Test Cases ***
Create Manage content test publication
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "${TOPIC_NAME}" from the admin dashboard
    user waits until page contains link    Create new publication     60
    user clicks link  Create new publication
    user creates publication    ${PUBLICATION_NAME}

Verify Manage content test publication is created
    [Tags]  HappyPath
    user waits until page contains accordion section  ${PUBLICATION_NAME}
    user opens accordion section  ${PUBLICATION_NAME}
    user waits until accordion section contains text  ${PUBLICATION_NAME}    Methodology
    user waits until accordion section contains text  ${PUBLICATION_NAME}    Releases

Create release
    [Tags]  HappyPath
    user clicks element  css:[data-testid="Create new release link for ${PUBLICATION_NAME}"]
    user creates release for publication  ${PUBLICATION_NAME}  Academic Year  2025
    user checks summary list contains  Publication title  ${PUBLICATION_NAME}

Navigate to Manage data tab
    [Tags]  HappyPath
    user clicks link  Manage data
    user waits until h1 is visible  ${PUBLICATION_NAME}

Navigate to File uploads tab
    [Tags]  HappyPath
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
    user opens accordion section   test 1

    ${section_1}=  user gets accordion content element  test 1
    user checks summary list contains  Name         test 1              ${section_1}
    user checks summary list contains  File         test-file-1.txt     ${section_1}
    user checks summary list contains  File size    12 B                ${section_1}

    user enters text into element  id:fileUploadForm-name   Test 2
    user chooses file   id:fileUploadForm-file      ${CURDIR}${/}files${/}test-file-2.txt
    user clicks button  Upload file

    user waits until page contains accordion section   test 2
    user opens accordion section   test 2

    ${section_2}=  user gets accordion content element  test 2
    user checks summary list contains  Name         test 2              ${section_2}
    user checks summary list contains  File         test-file-2.txt     ${section_2}
    user checks summary list contains  File size    24 B                ${section_2}

    user checks there are x accordion sections  2

Delete file
    [Tags]  HappyPath
    ${file_2_section}=  user gets accordion content element  test 2
    user clicks button   Delete file  ${file_2_section}
    user waits until h1 is visible   Confirm deletion of file
    user clicks button  Confirm

    user waits until page does not contain accordion section   test 2
    user waits until page contains accordion section  test 1
    user checks there are x accordion sections  1
