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
    user selects dashboard theme if possible
    user clicks link    Create new publication
    user waits until h1 is visible    Create new publication
    user creates publication    ${PUBLICATION_NAME}

Create new release with data files
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user creates release from publication page    ${PUBLICATION_NAME}    ${RELEASE_NAME}    2020
    user uploads subject and waits until complete    ${SUBJECT_NAME_1}    seven_filters.csv    seven_filters.meta.csv
    user uploads subject and waits until complete    ${SUBJECT_NAME_2}    tiny-two-filters.csv
    ...    tiny-two-filters.meta.csv

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
    user approves release for immediate publication

User creates second release
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user creates release from publication page    ${PUBLICATION_NAME}    ${RELEASE_NAME}    2021
    user uploads subject and waits until complete    ${SUBJECT_NAME_3}    dates.csv    dates.meta.csv
    user uploads subject and waits until complete    ${SUBJECT_NAME_4}    upload-file-test-with-filter.csv
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
    user approves release for immediate publication

Verify newly published release is on Find Statistics page
    user checks publication is on find statistics page    ${PUBLICATION_NAME}

User navigates to data catalogue page
    user navigates to data catalogue page on public frontend

Validate Related information section and links exist
    ${relatedInformation}=    get webelement    css:[aria-labelledby="related-information"]

    user checks element contains child element    ${relatedInformation}    xpath://h2[text()="Related information"]

    user checks page contains link with text and url
    ...    Find statistics and data
    ...    /find-statistics
    ...    ${relatedInformation}
    user checks page contains link with text and url
    ...    Methodology
    ...    /methodology
    ...    ${relatedInformation}
    user checks page contains link with text and url
    ...    Glossary
    ...    /glossary
    ...    ${relatedInformation}

Validate data sets list
    user checks element count is x    css:[data-testid="data-set-file-list"] li:first-child    10

    ${dataSet}=    user gets testid element    data-set-file-summary-${SUBJECT_NAME_3}

    user checks element contains    ${dataSet}    ${SUBJECT_NAME_3}
    user checks element contains    ${dataSet}    ${SUBJECT_NAME_3} data guidance content
    user checks element contains    ${dataSet}    UI test theme
    user checks element contains    ${dataSet}    This is the latest data
    user checks element contains    ${dataSet}    ${PUBLICATION_NAME}
    user checks element contains    ${dataSet}    ${RELEASE_NAME}

    user clicks button    Show more details    ${dataSet}
    user clicks button containing text    Download data set    ${dataSet}

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
    user checks select contains option    css:select[id="filters-form-theme"]    All themes
    user checks selected option label    css:select[id="filters-form-theme"]    All themes
    user checks select contains option    css:select[id="filters-form-theme"]    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks select contains option    css:select[id="filters-form-theme"]    ${ROLE_PERMISSIONS_THEME_TITLE}

Validate publication filter exists
    user checks select contains option    css:select[id="filters-form-publication"]    All publications
    user checks selected option label    css:select[id="filters-form-publication"]    All publications

Validate release filter exists
    user checks select contains option    css:select[id="filters-form-release"]    Latest releases
    user checks selected option label    css:select[id="filters-form-release"]    Latest releases
    user checks select contains option    css:select[id="filters-form-release"]    All releases

Filter by theme
    user wait for option to be available and select it    css:select[id="filters-form-theme"]
    ...    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks page contains button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}

Filter by publication
    user wait for option to be available and select it    css:select[id="filters-form-publication"]
    ...    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user waits until page contains button    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user waits until page contains button    ${PUPIL_ABSENCE_RELEASE_NAME}
    user checks page contains button    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user checks page contains button    ${PUPIL_ABSENCE_RELEASE_NAME}

Filter by all releases
    user wait for option to be available and select it    css:select[id="filters-form-release"]    All releases
    user checks page contains button    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user checks page does not contain button    ${PUPIL_ABSENCE_RELEASE_NAME}

Remove theme filter
    user clicks button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks page does not contain button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks page does not contain button    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user checks selected option label    css:select[id="filters-form-theme"]    All themes

Remove publication filter
    user wait for option to be available and select it    css:select[id="filters-form-theme"]
    ...    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user wait for option to be available and select it    css:select[id="filters-form-publication"]
    ...    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user clicks button    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user checks page contains button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks page does not contain button    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user checks page does not contain button    ${PUPIL_ABSENCE_RELEASE_NAME}
    user checks selected option label    css:select[id="filters-form-publication"]    All publications

Remove release filter
    user wait for option to be available and select it    css:select[id="filters-form-theme"]
    ...    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user wait for option to be available and select it    css:select[id="filters-form-publication"]
    ...    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    sleep    1    # wait a moment to wait for release filter options to get updated
    user wait for option to be available and select it    css:select[id="filters-form-release"]
    ...    ${PUPIL_ABSENCE_RELEASE_NAME}
    user clicks button    ${PUPIL_ABSENCE_RELEASE_NAME}
    user checks page contains button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks page contains button    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user checks page does not contain button    ${PUPIL_ABSENCE_RELEASE_NAME}
    user checks selected option label    id:filters-form-release    All releases

Filter by geographic level
    user wait for option to be available and select it    css:select[id="filters-form-geographic-level"]
    ...    Local Authority District

    user checks page contains button    Local Authority District
    user checks testid element contains    total-results    1 data set

    user clicks button    Show more details
    user checks testid element contains    Geographic levels-value    Local authority district

Remove geographic level filter
    user clicks button    Local Authority District

    user checks page does not contain button    Local Authority District
    user checks selected option label    id:filters-form-geographic-level    All

    user checks page contains button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks page contains button    ${PUPIL_ABSENCE_PUBLICATION_TITLE}

Reset all filters
    user clicks element    id:searchForm-search
    user presses keys    pupil
    user clicks button    Search
    user wait for option to be available and select it    css:select[id="filters-form-theme"]
    ...    ${PUPILS_AND_SCHOOLS_THEME_TITLE}

    user checks page contains button    pupil
    user checks page contains button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}

    user clicks button    Reset filters

    user checks page does not contain button    pupil
    user checks page does not contain button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks page does not contain button    Reset filters

    user checks selected option label    css:select[id="filters-form-theme"]    All themes
    user checks selected option label    css:select[id="filters-form-publication"]    All publications
    user checks selected option label    css:select[id="filters-form-release"]    Latest releases

Searching
    user clicks element    id:searchForm-search
    user presses keys    Exclusions by geographic level
    user clicks button    Search
    user checks page contains button    Exclusions by geographic level
    user checks list item contains    testid:data-set-file-list    1    Exclusions by geographic level

Removing search
    user clicks button    Exclusions by geographic level
    user checks page does not contain button    Exclusions by geographic level

Validate data catalogue page redirect from slug based urls
    environment variable should be set    PUBLIC_URL
    user navigates to
    ...    %{PUBLIC_URL}/data-catalogue/${PUPIL_ABSENCE_PUBLICATION_SLUG}/2016-17
    user waits until h1 is visible    Data catalogue

    user checks page contains button    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user waits until page contains button    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user waits until page contains button    ${PUPIL_ABSENCE_RELEASE_NAME}

    user checks element count is x    css:[data-testid="data-set-file-list"] li:first-child    2
    ${dataSet_1}=    user gets testid element    data-set-file-summary-Absence by characteristic
    user checks element contains    ${dataSet_1}    Absence by characteristic
    ${dataSet_2}=    user gets testid element    data-set-file-summary-Absence in PRUs
    user checks element contains    ${dataSet_2}    Absence in PRUs
