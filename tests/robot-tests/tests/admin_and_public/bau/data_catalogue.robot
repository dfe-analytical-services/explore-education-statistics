*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/public-common.robot
Resource            ../../libs/admin/manage-content-common.robot

Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Variables ***
${PUBLICATION_NAME}=    UI tests - data catalogue %{RUN_IDENTIFIER}
${RELEASE_NAME}=        Academic year Q1
${SUBJECT_NAME_1}=      UI test subject 1
${SUBJECT_NAME_2}=      UI test subject 2
${SUBJECT_NAME_3}=      UI test subject 3
${SUBJECT_NAME_4}=      UI test subject 4


*** Test Cases ***
Create publication
    user selects dashboard theme and topic if possible
    user clicks link    Create new publication
    user waits until h1 is visible    Create new publication
    user creates publication    ${PUBLICATION_NAME}

Create new release with data files
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user creates release from publication page    ${PUBLICATION_NAME}    ${RELEASE_NAME}    2020
    user uploads subject    ${SUBJECT_NAME_1}    seven_filters.csv    seven_filters.meta.csv
    user uploads subject    ${SUBJECT_NAME_2}    tiny-two-filters.csv    tiny-two-filters.meta.csv

Add data guidance to subjects
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release

    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance

    user waits until page contains element    id:dataGuidance-dataFiles
    user waits until page contains accordion section    ${SUBJECT_NAME_1}

    user enters text into data guidance data file content editor    ${SUBJECT_NAME_1}
    ...    ${SUBJECT_NAME_1} Main guidance content

    user enters text into data guidance data file content editor    ${SUBJECT_NAME_2}
    ...    ${SUBJECT_NAME_2} Main guidance content

Save data guidance
    user clicks button    Save guidance

Approve first release
    user clicks link    Sign off
    user approves release for immediate publication

User creates second release
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user creates release from publication page    ${PUBLICATION_NAME}    ${RELEASE_NAME}    2021
    user uploads subject    ${SUBJECT_NAME_3}    dates.csv    dates.meta.csv
    user uploads subject    ${SUBJECT_NAME_4}    upload-file-test-with-filter.csv
    ...    upload-file-test-with-filter.meta.csv

Add data guidance to subjects (second release)
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release

    user clicks link    Data guidance

    user waits until page contains element    id:dataGuidance-dataFiles
    user waits until page contains accordion section    ${SUBJECT_NAME_3}

    user enters text into data guidance data file content editor    ${SUBJECT_NAME_3}
    ...    ${SUBJECT_NAME_3} data guidance content

    user enters text into data guidance data file content editor    ${SUBJECT_NAME_4}
    ...    ${SUBJECT_NAME_4} data guidance content

Save data guidance (second release)
    user clicks button    Save guidance

Approve release
    user clicks link    Sign off
    user approves release for immediate publication

Verify newly published release is on Find Statistics page
    user checks publication is on find statistics page    ${PUBLICATION_NAME}

User navigates to /data-catalogue page
    user navigates to data catalogue page on public frontend

User checks search filters publications properly
    user enters text into element    id:publicationForm-publicationIdSearch    Pupil
    user waits until page contains    Pupil absence in schools in England    %{WAIT_SMALL}
    user clears element text    id:publicationForm-publicationIdSearch

Choose publication
    user clicks radio    Test theme
    user clicks radio    UI tests - data catalogue %{RUN_IDENTIFIER}
    user clicks button    Next step

Check page displays correct data
    user waits for page to finish loading
    user waits until h1 is visible    Browse our open data
    user checks page contains    Choose a release
    user clicks radio    ${RELEASE_NAME} 2021/22

Navigate to Next step
    user clicks button    Next step

Check step 3 displays correctly
    user waits until page contains    Choose files to download    10

    user checks element contains    testid:choose-files    This is the latest data

Check checkbox and download file
    user clicks checkbox    UI test subject 3 (csv, 17 Kb)
    user clicks button    Download selected files

# Check zip file for 2021 here

Validate zip contains correct files
    [Documentation]    EES-4147
    [Tags]    Failing
    sleep    8    # wait for file to download
    ${list}=    create list    data/dates.csv    data-guidance/data-guidance.txt
    zip should contain directories and files    ui-tests-data-catalogue-%{RUN_IDENTIFIER}_2021-22-q1.zip    ${list}

go back to second step and check page displays correct data
    user clicks button    Previous step
    user checks element contains    testid:Radio item for ${RELEASE_NAME} 2021/22    This is the latest data

Select alternate release
    user clicks radio    ${RELEASE_NAME} 2020/21
    user clicks button    Next step

Check page displays 'This is not the latest data' tag
    user checks element contains    testid:choose-files    This is not the latest data

Select new subject and download new subject file
    user clicks checkbox    ${SUBJECT_NAME_1} (csv, 456 Kb)
    user clicks button    Download selected files

Validate new zip contains correct files
    [Documentation]    EES-4147
    [Tags]    Failing
    sleep    8    # wait for file to download
    ${list}=    create list    data/seven_filters.csv    data-guidance/data-guidance.txt
    zip should contain directories and files    ui-tests-data-catalogue-%{RUN_IDENTIFIER}_2020-21-q1.zip    ${list}
