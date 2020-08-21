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
    user checks page contains accordion  ${PUBLICATION_NAME}
    user opens accordion section  ${PUBLICATION_NAME}
    user checks accordion section contains text  ${PUBLICATION_NAME}    Methodology
    user checks accordion section contains text  ${PUBLICATION_NAME}    Releases

Create release
    [Tags]  HappyPath
    user clicks element  css:[data-testid="Create new release link for ${PUBLICATION_NAME}"]
    user creates release for publication  ${PUBLICATION_NAME}  Academic Year  2025
    user checks summary list contains  Publication title  ${PUBLICATION_NAME}

Navigate to Manage data tab
    [Tags]  HappyPath
    user waits until page contains link   Manage data
    user clicks link  Manage data
    user waits until page contains heading 1  ${PUBLICATION_NAME}

Navigate to File uploads tab
    [Tags]  HappyPath
    user waits until page contains link   File uploads
    user clicks link  File uploads
    user waits until page contains element  xpath://legend[text()="Upload file"]

Upload file
    [Tags]  HappyPath
    user enters text into element  id:fileUploadForm-name   dfe logo
    choose file   id:fileUploadForm-file      ${CURDIR}${/}files${/}dfe-logo.jpg
    user clicks button  Upload file
    user waits until page contains accordion section   dfe logo
    user opens accordion section   dfe logo
    user checks page contains element   xpath://dt[text()="Name"]/../dd/h4[text()="dfe logo"]

Delete file
    [Tags]  HappyPath
    user clicks button   Delete file
    user waits until page contains heading 1   Confirm deletion of file
    user clicks button  Confirm
    user waits until page does not contain accordion section   dfe logo
    user checks page contains  File uploads