*** Settings ***
Library             Collections
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData

*** Variables ***
${TOPIC_NAME}=          %{TEST_TOPIC_NAME}
${PUBLICATION_NAME}=    UI tests - upload files %{RUN_IDENTIFIER}

*** Test Cases ***
Create test publication and release via api
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user create test release via api    ${PUBLICATION_ID}    AY    2025

Navigate to 'Data and files' page
    user navigates to editable release summary from admin dashboard    ${PUBLICATION_NAME}
    ...    Academic Year 2025/26 (not Live)

    user clicks link    Data and files
    user waits until h1 is visible    ${PUBLICATION_NAME}

Upload a ZIP file subject
    [Documentation]    EES-1397
    user enters text into element    label:Subject title    Absence in PRUs
    user clicks radio    ZIP file
    user waits until page contains element    label:Upload ZIP file
    user chooses file    label:Upload ZIP file    ${FILES_DIR}upload-zip-test.zip
    user clicks button    Upload data files

    user waits until h2 is visible    Uploaded data files
    user waits until page contains accordion section    Absence in PRUs
    user opens accordion section    Absence in PRUs

    ${section}=    user gets accordion section content element    Absence in PRUs

    # To ensure "Data file size" and "Number of rows" will be filled
    user waits until page does not contain    Queued    %{WAIT_MEDIUM}

    user checks headed table body row contains    Subject title    Absence in PRUs    ${section}
    user checks headed table body row contains    Data file    absence_in_prus.csv    ${section}
    user checks headed table body row contains    Metadata file    absence_in_prus.meta.csv    ${section}
    user checks headed table body row contains    Data file size    141 Kb    ${section}
    user checks headed table body row contains    Number of rows    613    ${section}
    user checks headed table body row contains    Status    Complete    ${section}    %{WAIT_LONG}

Change subject title
    user waits until page contains accordion section    Absence in PRUs
    user clicks link    Edit title

    user waits until h2 is visible    Edit data file details
    user clears element text    label:Title
    user enters text into element    label:Title    Updated Absence in PRUs

    user clicks button    Save changes

Validate subject title has been updated
    user waits until h2 is visible    Uploaded data files
    user waits until page contains accordion section    Updated Absence in PRUs
    user opens accordion section    Absence in PRUs

    ${section}=    user gets accordion section content element    Absence in PRUs
    user checks headed table body row contains    Subject title    Updated Absence in PRUs    ${section}

Check subject appears in 'Data blocks' page
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks

    user clicks link    Create data block
    user waits until h2 is visible    Create data block
    user waits until page contains    Choose a subject    %{WAIT_LONG}

    user waits until table tool wizard step is available    1    Choose a subject

    user waits until page contains    Updated Absence in PRUs

Navigate to 'Data and files' page - 'Ancillary file uploads' tab
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release
    user clicks link    Ancillary file uploads
    user waits until h2 is visible    Add file to release
    user waits until page contains    No files have been uploaded

Validate cannot upload empty ancillary file
    user enters text into element    label:Title    Empty test
    user chooses file    label:Upload file    ${FILES_DIR}empty-file.txt
    user clicks button    Upload file
    user waits until page contains    Choose a file that is not empty

Upload multiple ancillary files
    user enters text into element    label:Title    Test 1
    user enters text into element    label:Summary    Test 1 summary
    user chooses file    label:Upload file    ${FILES_DIR}test-file-1.txt
    user clicks button    Upload file

    user waits until page contains accordion section    Test 1
    user opens accordion section    Test 1    id:file-uploads

    ${section_1}=    user gets accordion section content element    Test 1    id:file-uploads
    user checks summary list contains    Title    Test 1    ${section_1}
    user checks summary list contains    Summary    Test 1 summary    ${section_1}
    user checks summary list contains    File    test-file-1.txt    ${section_1}
    user checks summary list contains    File size    12 B    ${section_1}

    user enters text into element    label:Title    Test 2
    user enters text into element    label:Summary    Test 2 summary
    user chooses file    label:Upload file    ${FILES_DIR}test-file-2.txt
    user clicks button    Upload file

    user waits until page contains accordion section    Test 2
    user opens accordion section    Test 2    id:file-uploads

    ${section_2}=    user gets accordion section content element    Test 2    id:file-uploads
    user checks summary list contains    Title    Test 2    ${section_2}
    user checks summary list contains    Summary    Test 2 summary    ${section_2}
    user checks summary list contains    File    test-file-2.txt    ${section_2}
    user checks summary list contains    File size    24 B    ${section_2}

    user checks there are x accordion sections    2    id:file-uploads

Navigate to 'Content' page
    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}

Validate 'Explore data and files' accordion
    user opens accordion section    Explore data and files
    ${section}=    user gets accordion section content element    Explore data and files

    # All files zip
    user checks element contains button    ${section}    Download all data

    # Data files
    user waits until h3 is visible    Open data
    user opens details dropdown    Download files
    user checks list has x items    testid:data-files    1    ${section}
    ${data_files_1}=    user gets list item element    testid:data-files    1    ${section}
    user checks element contains button    ${data_files_1}    Updated Absence in PRUs

    # Ancillary files
    user waits until h3 is visible    All supporting files
    user opens details dropdown    List of all supporting files
    ${other_files}=    user gets details content element    List of all supporting files    ${section}
    ${other_files_1}=    get child element    ${other_files}    css:li:nth-child(1)

    user checks element contains button    ${other_files_1}    Test 1

    user opens details dropdown    More details    ${other_files_1}
    ${other_files_1_details}=    user gets details content element    More details    ${other_files_1}
    user checks element should contain    ${other_files_1_details}    Test 1 summary

Navigate back to 'Ancillary file uploads' tab
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release

    user clicks link    Ancillary file uploads
    user waits until h2 is visible    Add file to release

Change ancillary file details
    user waits until page contains accordion section    Test 2
    user opens accordion section    Test 2    id:file-uploads

    ${section}=    user gets accordion section content element    Test 2    id:file-uploads
    user clicks link    Edit file    ${section}

    user waits until h2 is visible    Edit ancillary file details
    user enters text into element    label:Title    Test 2 updated
    user enters text into element    label:Summary    Test 2 summary updated

    user clicks button    Save changes

Validate ancillary file details were changed
    user waits until h2 is visible    Add file to release
    user waits until h2 is visible    Uploaded files

    user waits until page contains accordion section    Test 2 updated
    user opens accordion section    Test 2 updated    id:file-uploads

    ${section}=    user gets accordion section content element    Test 2 updated    id:file-uploads
    user checks summary list contains    Title    Test 2 updated    ${section}
    user checks summary list contains    Summary    Test 2 summary updated    ${section}

Delete ancillary file
    ${file_2_section}=    user gets accordion section content element    Test 2 updated    id:file-uploads
    user clicks button    Delete file    ${file_2_section}
    user waits until h1 is visible    Confirm deletion of file
    user clicks button    Confirm

    user waits until page does not contain accordion section    Test 2 updated
    user waits until page contains accordion section    Test 1
    user checks there are x accordion sections    1    id:file-uploads
