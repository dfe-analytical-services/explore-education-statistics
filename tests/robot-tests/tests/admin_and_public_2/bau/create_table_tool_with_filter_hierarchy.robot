*** Settings ***
Resource            ../../libs/admin-common.robot
Resource            ../../libs/public-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Library             ../../libs/admin_api.py

Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Variables ***
${PUBLICATION_NAME}     Filter hierarchy %{RUN_IDENTIFIER}
${RELEASE_NAME}         Calendar year 2022
${SUBJECT_NAME}         UI test subject


*** Test Cases ***
Create new publication and release via API
    ${PUBLICATION_ID}    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    CY    2022

Upload subject to release
    user navigates to draft release page from dashboard
    ...    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}

    user uploads subject and waits until complete
    ...    ${SUBJECT_NAME}
    ...    data-further-education-and-skills.csv
    ...    data-further-education-and-skills.meta.csv

Add data guidance to subject
    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance

    user waits until page contains element    id:dataGuidance-dataFiles
    user waits until page contains accordion section    ${SUBJECT_NAME}

    user enters text into data guidance data file content editor    ${SUBJECT_NAME}
    ...    ${SUBJECT_NAME} Main guidance content

Save data guidance
    user clicks button    Save guidance

Start creating a data block
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks
    user waits until page contains    No data blocks have been created.

    user clicks link    Create data block
    user waits until table tool wizard step is available    1    Select a data set

Select subject
    user clicks radio    ${SUBJECT_NAME}
    user clicks element    id:publicationDataStepForm-submit
    user waits until table tool wizard step is available    2    Choose locations
    user checks previous table tool step contains    1    Data set    ${SUBJECT_NAME}

Select all provider locations
    user checks checkbox is checked    England
    user clicks element    id:locationFiltersForm-submit
    user waits until table tool wizard step is available    3    Choose time period
    user checks previous table tool step contains    2    National    England

Select start date and end date
    user chooses select option    id:timePeriodForm-start    2023/24
    user chooses select option    id:timePeriodForm-end    2023/24
    user clicks element    id:timePeriodForm-submit
    user waits until table tool wizard step is available    4    Choose your filters
    user waits until page contains element    id:filtersForm-indicators
    user checks previous table tool step contains    3    Time period    2023/24

Check indicator is selected
    user checks checkbox is checked    Number of achievers

Select filter hierarchy options
    user opens details dropdown    Ethnicity minor (2 tiers)
    user clicks category checkbox    Browse all tiers of ethnicity minor    Asian
    user opens details dropdown    Show ethnicity minor
    user clicks button containing text    Select all 2 options
    user closes details dropdown    Close ethnicity minor
    user closes details dropdown    Ethnicity minor (2 tiers)

    user opens details dropdown    Learning aim title (3 tiers)
    user clicks category checkbox    Browse all tiers of learning aim title    Total
    user opens details dropdown    Show qualification
    user opens details dropdown    Show learning aim title
    user clicks category checkbox    Browse all tiers of learning aim title    Cycle Mechanics
    user clicks category checkbox    Browse all tiers of learning aim title    Games Design and Development
    user clicks category checkbox    Browse all tiers of learning aim title    IT Users (ITQ)

Create table
    user clicks element    id:filtersForm-submit
    user waits until results table appears    %{WAIT_SMALL}

Validate step 5 options
    user checks previous table tool step contains    4    Indicators    Number of achievers
    user checks previous table tool step contains    4    Ethnicity minor    Asian (total)
    user checks previous table tool step contains    4    Ethnicity minor    Chinese
    user checks previous table tool step contains    4    Ethnicity minor    Indian
    user checks previous table tool step contains    4    Learning aim title    Total
    user checks previous table tool step contains    4    Learning aim title    Cycle Mechanics
    user checks previous table tool step contains    4    Learning aim title    Games Design and Development
    user checks previous table tool step contains    4    Learning aim title    IT Users (ITQ)

Validate row headings
    user checks table column heading contains    1    1    Asian

Validate results table column headings
    user checks table row heading contains    1    1    Total
    user checks table row heading contains    1    2    Total
    user checks table row heading contains    1    3    Total
    user checks table row heading contains    2    1    Chinese
    user checks table row heading contains    2    2    Total
    user checks table row heading contains    3    1    Indian
    user checks table row heading contains    4    1    Digital Technology
    user checks table row heading contains    4    2    Total
    user checks table row heading contains    5    1    Chinese
    user checks table row heading contains    5    2    Total
    user checks table row heading contains    6    1    Indian
    user checks table row heading contains    6    2    Total
    user checks table row heading contains    7    1    Award
    user checks table row heading contains    7    2    Digital Technology
    user checks table row heading contains    7    3    Total
    user checks table row heading contains    7    4    Total
    user checks table row heading contains    8    1    Games Design and Development
    user checks table row heading contains    9    1    IT Users (ITQ)
    user checks table row heading contains    10    1    Chinese
    user checks table row heading contains    10    2    Total
    user checks table row heading contains    11    1    Games Design and Development
    user checks table row heading contains    12    1    IT Users (ITQ)
    user checks table row heading contains    13    1    Indian
    user checks table row heading contains    13    2    Total
    user checks table row heading contains    14    1    Games Design and Development
    user checks table row heading contains    15    1    IT Users (ITQ)

Save datablock
    user enters text into element    id:dataBlockDetailsForm-name    UI Test datablock_name
    user enters text into element    id:dataBlockDetailsForm-heading    UI Test datablock_title
    user clicks button    Save data block

Reload created datablock
    user reloads page
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks
    user clicks link containing text    Edit block
    user waits until h2 is visible    Data source

Validate step 5 options
    user checks previous table tool step contains    4    Indicators    Number of achievers
    user checks previous table tool step contains    4    Ethnicity minor    Asian (total)
    user checks previous table tool step contains    4    Ethnicity minor    Chinese
    user checks previous table tool step contains    4    Ethnicity minor    Indian
    user checks previous table tool step contains    4    Learning aim title    Total
    user checks previous table tool step contains    4    Learning aim title    Cycle Mechanics
    user checks previous table tool step contains    4    Learning aim title    Games Design and Development
    user checks previous table tool step contains    4    Learning aim title    IT Users (ITQ)

Validate row headings
    user checks table column heading contains    1    1    Asian

Validate results table column headings
    user checks table row heading contains    1    1    Total
    user checks table row heading contains    1    2    Total
    user checks table row heading contains    1    3    Total
    user checks table row heading contains    2    1    Chinese
    user checks table row heading contains    2    2    Total
    user checks table row heading contains    3    1    Indian
    user checks table row heading contains    4    1    Digital Technology
    user checks table row heading contains    4    2    Total
    user checks table row heading contains    5    1    Chinese
    user checks table row heading contains    5    2    Total
    user checks table row heading contains    6    1    Indian
    user checks table row heading contains    6    2    Total
    user checks table row heading contains    7    1    Award
    user checks table row heading contains    7    2    Digital Technology
    user checks table row heading contains    7    3    Total
    user checks table row heading contains    7    4    Total
    user checks table row heading contains    8    1    Games Design and Development
    user checks table row heading contains    9    1    IT Users (ITQ)
    user checks table row heading contains    10    1    Chinese
    user checks table row heading contains    10    2    Total
    user checks table row heading contains    11    1    Games Design and Development
    user checks table row heading contains    12    1    IT Users (ITQ)
    user checks table row heading contains    13    1    Indian
    user checks table row heading contains    13    2    Total
    user checks table row heading contains    14    1    Games Design and Development
    user checks table row heading contains    15    1    IT Users (ITQ)

Add headline text block to Content page
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Approve first release
    user clicks link    Sign off
    user approves release for immediate publication

Verify newly published release is on Find Statistics page
    # TODO EES-6063 - Remove this
    user checks publication is on find statistics page    ${PUBLICATION_NAME}

Go to public table tool page
    user navigates to data tables page on public frontend

Select "Test Theme" publication
    environment variable should be set    TEST_THEME_NAME
    user clicks radio    %{TEST_THEME_NAME}
    user clicks radio    ${PUBLICATION_NAME}
    user clicks element    id:publicationForm-submit
    user waits until table tool wizard step is available    2    Select a data set
    user checks previous table tool step contains    1    Publication    ${PUBLICATION_NAME}

Select subject
    user clicks radio    ${SUBJECT_NAME}
    user clicks element    id:publicationDataStepForm-submit
    user waits until table tool wizard step is available    3    Choose locations
    user checks previous table tool step contains    2    Data set    ${SUBJECT_NAME}

Select all provider locations
    user checks checkbox is checked    England
    user clicks element    id:locationFiltersForm-submit
    user waits until table tool wizard step is available    4    Choose time period
    user checks previous table tool step contains    3    National    England

Select start date and end date
    user chooses select option    id:timePeriodForm-start    2023/24
    user chooses select option    id:timePeriodForm-end    2023/24
    user clicks element    id:timePeriodForm-submit
    user waits until table tool wizard step is available    5    Choose your filters
    user waits until page contains element    id:filtersForm-indicators
    user checks previous table tool step contains    4    Time period    2023/24

Check indicator is selected
    user checks checkbox is checked    Number of achievers

Select filter hierarchy options
    user opens details dropdown    Ethnicity minor (2 tiers)
    user clicks category checkbox    Browse all tiers of ethnicity minor    Asian
    user opens details dropdown    Show ethnicity minor
    user clicks button containing text    Select all 2 options
    user closes details dropdown    Close ethnicity minor
    user closes details dropdown    Ethnicity minor (2 tiers)

    user opens details dropdown    Learning aim title (3 tiers)
    user clicks category checkbox    Browse all tiers of learning aim title    Total
    user opens details dropdown    Show qualification
    user opens details dropdown    Show learning aim title
    user clicks category checkbox    Browse all tiers of learning aim title    Cycle Mechanics
    user clicks category checkbox    Browse all tiers of learning aim title    Games Design and Development
    user clicks category checkbox    Browse all tiers of learning aim title    IT Users (ITQ)

Create table
    user clicks element    id:filtersForm-submit
    user waits until results table appears    %{WAIT_SMALL}

Validate step 5 options
    user checks previous table tool step contains    5    Indicators    Number of achievers
    user checks previous table tool step contains    5    Ethnicity minor    Asian (total)
    user checks previous table tool step contains    5    Ethnicity minor    Chinese
    user checks previous table tool step contains    5    Ethnicity minor    Indian
    user checks previous table tool step contains    5    Learning aim title    Total
    user checks previous table tool step contains    5    Learning aim title    Cycle Mechanics
    user checks previous table tool step contains    5    Learning aim title    Games Design and Development
    user checks previous table tool step contains    5    Learning aim title    IT Users (ITQ)

Validate row headings
    user checks table column heading contains    1    1    Asian

Validate results table column headings
    user checks table row heading contains    1    1    Total
    user checks table row heading contains    1    2    Total
    user checks table row heading contains    1    3    Total
    user checks table row heading contains    2    1    Chinese
    user checks table row heading contains    2    2    Total
    user checks table row heading contains    3    1    Indian
    user checks table row heading contains    4    1    Digital Technology
    user checks table row heading contains    4    2    Total
    user checks table row heading contains    5    1    Chinese
    user checks table row heading contains    5    2    Total
    user checks table row heading contains    6    1    Indian
    user checks table row heading contains    6    2    Total
    user checks table row heading contains    7    1    Award
    user checks table row heading contains    7    2    Digital Technology
    user checks table row heading contains    7    3    Total
    user checks table row heading contains    7    4    Total
    user checks table row heading contains    8    1    Games Design and Development
    user checks table row heading contains    9    1    IT Users (ITQ)
    user checks table row heading contains    10    1    Chinese
    user checks table row heading contains    10    2    Total
    user checks table row heading contains    11    1    Games Design and Development
    user checks table row heading contains    12    1    IT Users (ITQ)
    user checks table row heading contains    13    1    Indian
    user checks table row heading contains    13    2    Total
    user checks table row heading contains    14    1    Games Design and Development
    user checks table row heading contains    15    1    IT Users (ITQ)
