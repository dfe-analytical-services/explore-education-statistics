*** Settings ***
Library             Collections
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${PUBLICATION_NAME}=    UI tests - upload files %{RUN_IDENTIFIER}


*** Test Cases ***
Create test publication and release via api
    ${publication_id}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${publication_id}    AY    2025

Navigate to 'Data and files' page
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    Academic year 2025/26

    user clicks link    Data and files
    user waits until h1 is visible    ${PUBLICATION_NAME}

Upload a ZIP file subject
    [Documentation]    EES-1397
    user enters text into element    label:Title    Absence in PRUs
    user clicks radio    ZIP file
    user waits until page contains element    label:Upload ZIP file
    user chooses file    label:Upload ZIP file    ${FILES_DIR}upload-zip-test.zip
    user clicks button    Upload data files

    user waits until h2 is visible    Uploaded data files
    user waits until page contains data uploads table

    # To ensure "Data file size" and "Number of rows" will be filled
    user waits until page does not contain    Queued    %{WAIT_MEDIUM}
    user checks table cell contains    row=1    column=1    expected=Absence in PRUs    parent=testid:Data files table
    user checks table cell contains    row=1    column=2    expected=141 Kb    parent=testid:Data files table
    user checks table cell contains    row=1    column=3    expected=Complete    parent=testid:Data files table
    user checks table cell contains    row=1    column=4    expected=View details    parent=testid:Data files table
    user checks table cell contains    row=1    column=4    expected=Edit title    parent=testid:Data files table
    user checks table cell contains    row=1    column=4    expected=Replace data    parent=testid:Data files table
    user checks table cell contains    row=1    column=4    expected=Delete files    parent=testid:Data files table

Change subject title
    user clicks link    Edit title

    user waits until h2 is visible    Edit data file details
    user clears element text    label:Title
    user enters text into element    label:Title    Updated Absence in PRUs

    user clicks button    Save changes

Validate subject title has been updated
    user waits until h2 is visible    Uploaded data files

    user checks table cell contains    row=1    column=1    expected=Updated Absence in PRUs
    ...    parent=testid:Data files table

Check subject appears in 'Data blocks' page
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks

    user clicks link    Create data block
    user waits until h2 is visible    Create data block
    user waits until page contains    Select a data set    %{WAIT_LONG}

    user waits until table tool wizard step is available    1    Select a data set

    user waits until page contains    Updated Absence in PRUs

Navigate to 'Data and files' page - 'Supporting file uploads' tab
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release
    user clicks link    Supporting file uploads
    user waits until h2 is visible    Add file to release
    user waits until page contains    No files have been uploaded

Validate cannot upload empty ancillary file
    user enters text into element    label:Title    Empty test
    user chooses file    label:Upload file    ${FILES_DIR}empty-file.txt
    user clicks button    Add file
    user waits until page contains    Choose a file that is not empty

Upload multiple ancillary files
    user enters text into element    label:Title    Test 1
    user enters text into element    label:Summary    Test 1 summary
    user chooses file    label:Upload file    ${FILES_DIR}test-file-1.txt
    user clicks button    Add file

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
    user clicks button    Add file

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

Validate 'Explore data and files' section
    ${section}=    user gets testid element    data-and-files

    # All files zip
    user checks element contains button    ${section}    Download all data (ZIP)

    # Data files
    user opens details dropdown    Download files
    user checks list has x items    testid:data-files    1    ${section}
    ${data_files_1}=    user gets list item element    testid:data-files    1    ${section}
    user checks element contains button    ${data_files_1}    Updated Absence in PRUs (csv, 141 Kb)

    # Ancillary files
    user opens accordion section    Additional supporting files
    ${other_files}=    user gets accordion section content element    Additional supporting files

    user checks element contains button    ${other_files}    Test 1 (txt, 12 B)
    user checks element should contain    ${other_files}    Test 1 summary

Navigate back to 'Supporting file uploads' tab
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release

    user clicks link    Supporting file uploads
    user waits until h2 is visible    Add file to release

Change ancillary file
    user waits until page contains accordion section    Test 2
    user opens accordion section    Test 2    id:file-uploads

    ${section}=    user gets accordion section content element    Test 2    id:file-uploads
    user clicks link    Edit file    ${section}

    user waits until h2 is visible    Edit ancillary file
    user enters text into element    label:Title    Test 2 updated
    user enters text into element    label:Summary    Test 2 summary updated
    user chooses file    label:Upload new file    ${FILES_DIR}test-file-3.txt

    user clicks button    Save file

Validate ancillary file was changed
    user waits until h2 is visible    Add file to release
    user waits until h2 is visible    Uploaded files

    user waits until page contains accordion section    Test 2 updated
    user opens accordion section    Test 2 updated    id:file-uploads

    ${section}=    user gets accordion section content element    Test 2 updated    id:file-uploads
    user checks summary list contains    Title    Test 2 updated    ${section}
    user checks summary list contains    Summary    Test 2 summary updated    ${section}
    user checks summary list contains    File    test-file-3.txt    ${section}
    user checks summary list contains    File size    36 B    ${section}

Delete ancillary file
    ${file_2_section}=    user gets accordion section content element    Test 2 updated    id:file-uploads
    user clicks button    Delete file    ${file_2_section}
    user waits until h2 is visible    Confirm deletion of file
    user clicks button    Confirm

    user waits until page does not contain accordion section    Test 2 updated
    user waits until page contains accordion section    Test 1
    user checks there are x accordion sections    1    id:file-uploads
