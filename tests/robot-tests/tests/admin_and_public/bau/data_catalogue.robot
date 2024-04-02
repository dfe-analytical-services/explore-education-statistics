*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/public-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Resource            ../../seed_data/seed_data_theme_1_constants.robot
Resource            ../../seed_data/seed_data_theme_2_constants.robot

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

Add headline text block to Content page
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

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

Add headline text block to Content page (second release)
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Approve release
    user clicks link    Sign off
    user approves release for immediate publication

Verify newly published release is on Find Statistics page
    user checks publication is on find statistics page    ${PUBLICATION_NAME}

User navigates to /data-catalogue page
    user navigates to data catalogue page on public frontend

User checks search filters publications properly
    user enters text into element    id:publicationForm-publicationIdSearch    Pupil
    user waits until page contains    ${PUPIL_ABSENCE_PUBLICATION_TITLE}    %{WAIT_SMALL}
    user clears element text    id:publicationForm-publicationIdSearch

Choose publication
    user reloads page
    user clicks radio    Test theme
    user clicks radio    UI tests - data catalogue %{RUN_IDENTIFIER}
    user clicks button    Next step

Check page displays correct data
    user waits until page finishes loading
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

# TO DO EES-4781 - remove the above tests for the old version.

User navigates to data catalogue page
    user navigates to new data catalogue page on public frontend

Validate Related information section and links exist
    ${relatedInformation}=    get webelement    css:[aria-labelledby="related-information"]

    user checks element contains child element    ${relatedInformation}    xpath://h2[text()="Related information"]

    user checks page contains link with text and url
    ...    Find statistics and data
    ...    /find-statistics
    ...    ${relatedInformation}
    user checks page contains link with text and url
    ...    Glossary
    ...    /glossary
    ...    ${relatedInformation}

Validate data sets list
    user checks list has x items    testid:data-set-file-list    10

    ${dataSet}=    user gets testid element    data-set-summary-${SUBJECT_NAME_3}

    user checks element contains    ${dataSet}    ${SUBJECT_NAME_3}
    user checks element contains    ${dataSet}    ${SUBJECT_NAME_3} data guidance content
    user checks element contains    ${dataSet}    Test theme
    user checks element contains    ${dataSet}    This is the latest data
    user checks element contains    ${dataSet}    ${PUBLICATION_NAME}
    user checks element contains    ${dataSet}    ${RELEASE_NAME}

    user clicks button    Show more details
    user clicks button containing text    Download data set

Validate zip contains correct files
    [Documentation]    EES-4147
    sleep    8    # wait for file to download
    ${list}=    create list    data/dates.csv    data-guidance/data-guidance.txt
    zip should contain directories and files    ui-tests-data-catalogue-%{RUN_IDENTIFIER}_2021-22-q1.zip    ${list}

Validate sort controls exist
    user checks radio is checked    Newest
    user checks page contains radio    Oldest
    user checks page contains radio    A to Z

Validate theme filter exists
    user checks select contains option    id:theme    All themes
    user checks select contains option    id:theme    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks select contains option    id:theme    ${ROLE_PERMISSIONS_THEME_TITLE}

Validate publication filter exists
    user checks select contains option    id:publication    All publications

Validate release filter exists
    user checks select contains option    id:release    Latest releases
    user checks select contains option    id:release    All releases

Filter by theme
    user chooses select option    id:theme    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks page contains button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}

Filter by publication
    user chooses select option    id:publication    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user waits until page contains button    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user waits until page contains button    ${PUPIL_ABSENCE_RELEASE_NAME}
    user checks page contains button    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user checks page contains button    ${PUPIL_ABSENCE_RELEASE_NAME}

Filter by all releases
    user chooses select option    id:release    All releases
    user checks page contains button    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user checks page does not contain button    ${PUPIL_ABSENCE_RELEASE_NAME}

Remove theme filter
    user clicks button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks page does not contain button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks page does not contain button    ${PUPIL_ABSENCE_PUBLICATION_TITLE}

Remove publication filter
    user chooses select option    id:theme    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user chooses select option    id:publication    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user clicks button    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user checks page contains button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks page does not contain button    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user checks page does not contain button    ${PUPIL_ABSENCE_RELEASE_NAME}

Remove release filter
    user chooses select option    id:theme    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user chooses select option    id:publication    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user clicks button    ${PUPIL_ABSENCE_RELEASE_NAME}
    user checks page contains button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks page contains button    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user checks page does not contain button    ${PUPIL_ABSENCE_RELEASE_NAME}

Clear all filters
    user clicks element    id:searchForm-search
    user presses keys    pupil
    user clicks button    Search
    user chooses select option    id:theme    ${PUPILS_AND_SCHOOLS_THEME_TITLE}

    user checks page contains button    pupil
    user checks page contains button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}

    user clicks button    Clear filters

    user checks page does not contain button    pupil
    user checks page does not contain button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks page does not contain button    Clear filters

Searching
    user clicks element    id:searchForm-search
    user presses keys    Exclusions by geographic level
    user clicks button    Search
    user checks page contains button    Exclusions by geographic level
    user checks list item contains    testid:data-set-file-list    1    Exclusions by geographic level

Removing search
    user clicks button    Exclusions by geographic level
    user checks page does not contain button    Exclusions by geographic level

Navigate to data set page
    user clicks link    ${SUBJECT_NAME_3}
    user waits until h1 is visible    ${SUBJECT_NAME_3}    %{WAIT_MEDIUM}
    user waits until page contains title caption    Data set from Test theme    %{WAIT_MEDIUM}
    user checks page contains    Latest data
    user checks page contains    ${PUBLICATION_NAME}
    user checks page contains    ${RELEASE_NAME}

Validate zip contains correct files
    [Documentation]    EES-4147
    user clicks button containing text    Download data set (ZIP)

    sleep    8    # wait for file to download
    ${list}=    create list    data/dates.csv    data-guidance/data-guidance.txt
    zip should contain directories and files    ui-tests-data-catalogue-%{RUN_IDENTIFIER}_2021-22-q1.zip    ${list}
