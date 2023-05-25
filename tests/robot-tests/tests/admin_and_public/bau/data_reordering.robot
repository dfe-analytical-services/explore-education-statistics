*** Settings ***
Resource            ../../libs/admin-common.robot
Resource            ../../libs/public-common.robot
Library             ../../libs/admin_api.py

Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Variables ***
${PUBLICATION_NAME}     UI tests - data reordering %{RUN_IDENTIFIER}
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

    user uploads subject
    ...    ${SUBJECT_NAME}
    ...    grouped-filters-and-indicators.csv
    ...    grouped-filters-and-indicators.meta.csv

Open reorder filters and indicators tab
    user clicks link    Reorder filters and indicators
    user waits until h2 is visible    Reorder filters and indicators

Check reordering table contains subject row
    user waits until table is visible    id:reordering
    user checks table column heading contains    1    1    Data file    id:reordering
    user checks table body has x rows    1    id:reordering

    user checks table cell contains    1    1    ${SUBJECT_NAME}    id:reordering
    user checks table cell contains    1    2    Reorder filters    id:reordering
    user checks table cell contains    1    2    Reorder indicators    id:reordering

Check the initial order of filters
    user clicks button in table cell    1    2    Reorder filters    id:reordering
    user waits until h3 is visible    Reorder filters for ${SUBJECT_NAME}
    user checks list contains exact items in order    testid:reorder-list
    ...    ${{['Filter 1', 'Filter 2']}}

Reorder filters
    user moves item of draggable list down    testid:reorder-list    1
    user checks list contains exact items in order    testid:reorder-list
    ...    ${{['Filter 2', 'Filter 1']}}

Check the initial order of filter 1's filter groups
    user clicks button to reorder options within list    2
    user checks list contains exact items in order    testid:Filter 1-reorder-list
    ...    ${{['Total', 'Filter 1 group 1', 'Filter 1 group 2']}}

Reorder filter 1's filter groups
    user moves item of draggable list down    testid:Filter 1-reorder-list    2
    user checks list contains exact items in order    testid:Filter 1-reorder-list
    ...    ${{['Total', 'Filter 1 group 2', 'Filter 1 group 1']}}

Check the initial order of filter 1 group 1's filter items
    user clicks button to reorder options within list    3    Filter 1-reorder-list
    user checks list contains exact items in order    testid:Filter 1 group 1-reorder-list
    ...    ${{['F1G1-1', 'F1G1-2']}}

Reorder filter 1 group 1's filter items
    user moves item of draggable list down    testid:Filter 1 group 1-reorder-list    1
    user checks list contains exact items in order    testid:Filter 1 group 1-reorder-list
    ...    ${{['F1G1-2', 'F1G1-1']}}
    user clicks done button to collapse reorder list    3    Filter 1-reorder-list

Check the initial order of filter 1 group 2's filter items
    user clicks button to reorder options within list    2    Filter 1-reorder-list
    user checks list contains exact items in order    testid:Filter 1 group 2-reorder-list
    ...    ${{['F1G2-1', 'F1G2-2']}}

Reorder filter 1 group 2's filter items
    user moves item of draggable list down    testid:Filter 1 group 2-reorder-list    1
    user checks list contains exact items in order    testid:Filter 1 group 2-reorder-list
    ...    ${{['F1G2-2', 'F1G2-1']}}
    user clicks done button to collapse reorder list    2    Filter 1-reorder-list
    user clicks done button to collapse reorder list    2

Save reordered filters, filter groups and filter items
    user clicks button    Save order
    user waits until h3 is not visible    Reorder filters for ${SUBJECT_NAME}

Check the saved order of filters
    user clicks button in table cell    1    2    Reorder filters    id:reordering
    user waits until h3 is visible    Reorder filters for ${SUBJECT_NAME}
    user checks list contains exact items in order    testid:reorder-list
    ...    ${{['Filter 2', 'Filter 1']}}

Check the saved order of filter 1's filter groups
    user clicks button to reorder options within list    2
    user checks list contains exact items in order    testid:Filter 1-reorder-list
    ...    ${{['Total', 'Filter 1 group 2', 'Filter 1 group 1']}}

Check no reordering is possible for filter 1's Total group
    # Filter 1's 'Total' group only has one filter item which can't be reordered alone
    user checks reorder list has no reorder options button    1    Filter 1-reorder-list

Check the saved order of filter 1 group 1's filter items
    user clicks button to reorder options within list    3    Filter 1-reorder-list
    user checks list contains exact items in order    testid:Filter 1 group 1-reorder-list
    ...    ${{['F1G1-2', 'F1G1-1']}}
    user clicks done button to collapse reorder list    3    Filter 1-reorder-list

Check the saved order of filter 1 group 2's filter items
    user clicks button to reorder options within list    2    Filter 1-reorder-list
    user checks list contains exact items in order    testid:Filter 1 group 2-reorder-list
    ...    ${{['F1G2-2', 'F1G2-1']}}
    user clicks done button to collapse reorder list    2    Filter 1-reorder-list
    user clicks done button to collapse reorder list    2

Check the saved order of filter 2's filter groups remains untouched
    user clicks button to reorder options within list    1
    user checks list contains exact items in order    testid:Filter 2-reorder-list
    ...    ${{['Total', 'Filter 2 group 1', 'Filter 2 group 2']}}

Check no reordering is possible for filter 2's Total group
    # Filter 2's 'Total' group only has one filter item which can't be reordered alone
    user checks reorder list has no reorder options button    1    Filter 2-reorder-list

Check the saved order of filter 2 group 1's filter items remains untouched
    user clicks button to reorder options within list    2    Filter 2-reorder-list
    user checks list contains exact items in order    testid:Filter 2 group 1-reorder-list
    ...    ${{['F2G1-1', 'F2G1-2']}}
    user clicks done button to collapse reorder list    2    Filter 2-reorder-list

Check the saved order of filter 2 group 2's filter items remains untouched
    user clicks button to reorder options within list    3    Filter 2-reorder-list
    user checks list contains exact items in order    testid:Filter 2 group 2-reorder-list
    ...    ${{['F2G2-1', 'F2G2-2']}}
    user clicks done button to collapse reorder list    3    Filter 2-reorder-list
    user clicks done button to collapse reorder list    1

Cancel reordering filters
    user clicks button    Cancel    id:reordering
    user waits until h3 is not visible    Reorder filters for ${SUBJECT_NAME}

Check the initial order of indicator groups
    user clicks button in table cell    1    2    Reorder indicators    id:reordering
    user waits until h3 is visible    Reorder indicators for ${SUBJECT_NAME}
    user checks list contains exact items in order    testid:reorder-list
    ...    ${{['Indicator group 1', 'Indicator group 2']}}

Reorder indicator groups
    user moves item of draggable list down    testid:reorder-list    1
    user checks list contains exact items in order    testid:reorder-list
    ...    ${{['Indicator group 2', 'Indicator group 1']}}

Check the initial order of indicator group 1's indicators
    user clicks button to reorder options within list    2
    user checks list contains exact items in order    testid:Indicator group 1-reorder-list
    ...    ${{['Indicator 1', 'Indicator 2']}}

Reorder indicator group 1's indicators
    user moves item of draggable list down    testid:Indicator group 1-reorder-list    1
    user checks list contains exact items in order    testid:Indicator group 1-reorder-list
    ...    ${{['Indicator 2', 'Indicator 1']}}
    user clicks done button to collapse reorder list    2

Save reordered indicator groups and indicators
    user clicks button    Save order
    user waits until h3 is not visible    Reorder filters for ${SUBJECT_NAME}

Check the saved order of indicator groups
    user clicks button in table cell    1    2    Reorder indicators    id:reordering
    user waits until h3 is visible    Reorder indicators for ${SUBJECT_NAME}
    user checks list contains exact items in order    testid:reorder-list
    ...    ${{['Indicator group 2', 'Indicator group 1']}}

Check the saved order of indicator group 1's indicators
    user clicks button to reorder options within list    2
    user checks list contains exact items in order    testid:Indicator group 1-reorder-list
    ...    ${{['Indicator 2', 'Indicator 1']}}
    user clicks done button to collapse reorder list    2

Check the saved order of indicator group 2's indicators remains untouched
    user clicks button to reorder options within list    1
    user checks list contains exact items in order    testid:Indicator group 2-reorder-list
    ...    ${{['Indicator 3', 'Indicator 4']}}
    user clicks done button to collapse reorder list    1

Cancel reordering indicators
    user clicks button    Cancel    id:reordering
    user waits until h3 is not visible    Reorder indicators for ${SUBJECT_NAME}

Replace subject data
    user clicks link    Data uploads
    user waits until h2 is visible    Uploaded data files
    ${section}    user gets accordion section content element    ${SUBJECT_NAME}
    user clicks link    Replace data    ${section}
    user chooses file    id:dataFileUploadForm-dataFile    ${FILES_DIR}grouped-filters-and-indicators-replacement.csv
    user chooses file    id:dataFileUploadForm-metadataFile
    ...    ${FILES_DIR}grouped-filters-and-indicators-replacement.meta.csv
    user clicks button    Upload data files

    user waits until page contains element    testid:Replacement Subject title
    user checks table column heading contains    1    1    Original file
    user checks table column heading contains    1    2    Replacement file

    user checks headed table body row cell contains    Subject title    1    ${SUBJECT_NAME}
    user checks headed table body row cell contains    Data file    1    grouped-filters-and-indicators.csv
    user checks headed table body row cell contains    Metadata file    1    grouped-filters-and-indicators.meta.csv
    user checks headed table body row cell contains    Number of rows    1    100    wait=%{WAIT_SMALL}
    user checks headed table body row cell contains    Data file size    1    13 Kb
    user checks headed table body row cell contains    Status    1    Data replacement in progress    wait=%{WAIT_LONG}

    user checks headed table body row cell contains    Subject title    2    ${SUBJECT_NAME}
    user checks headed table body row cell contains    Data file    2    grouped-filters-and-indicators-replacement.csv
    user checks headed table body row cell contains    Metadata file    2
    ...    grouped-filters-and-indicators-replacement.meta.csv
    user checks headed table body row cell contains    Number of rows    2    140    wait=%{WAIT_SMALL}
    user checks headed table body row cell contains    Data file size    2    19 Kb    wait=%{WAIT_SMALL}
    user checks headed table body row cell contains    Status    2    Complete    wait=%{WAIT_LONG}

Confirm data replacement
    user waits until page contains    Data blocks: OK
    user waits until page contains    Footnotes: OK
    user clicks button    Confirm data replacement
    user waits until h2 is visible    Data replacement complete

Open reorder filters and indicators tab after data replacement
    user clicks link    Data and files
    user clicks link    Reorder filters and indicators
    user waits until h2 is visible    Reorder filters and indicators

Check reordering table contains subject row after data replacement
    user waits until table is visible    id:reordering
    user checks table column heading contains    1    1    Data file    id:reordering
    user checks table body has x rows    1    id:reordering

    user checks table cell contains    1    1    ${SUBJECT_NAME}    id:reordering
    user checks table cell contains    1    2    Reorder filters    id:reordering
    user checks table cell contains    1    2    Reorder indicators    id:reordering

Check the order of filters after data replacement
    # Filters are identical in the replacement so the previous order should be retained
    user clicks button in table cell    1    2    Reorder filters    id:reordering
    user waits until h3 is visible    Reorder filters for ${SUBJECT_NAME}
    user checks list contains exact items in order    testid:reorder-list
    ...    ${{['Filter 2', 'Filter 1']}}

Check the order of filter 1's filter groups after data replacement
    # 'Filter 1 group 3' is new in the replacement so should be appended
    user clicks button to reorder options within list    2
    user checks list contains exact items in order    testid:Filter 1-reorder-list
    ...    ${{['Total', 'Filter 1 group 2', 'Filter 1 group 1', 'Filter 1 group 3']}}

Check no reordering is possible for filter 1's Total group after data replacement
    # Filter 1's 'Total' group only has one filter item which can't be reordered alone
    user checks reorder list has no reorder options button    1    Filter 1-reorder-list

Check the order of filter 1 group 1's filter items after data replacement
    # 'F1G1-3' is new in the replacement so should be appended
    user clicks button to reorder options within list    3    Filter 1-reorder-list
    user checks list contains exact items in order    testid:Filter 1 group 1-reorder-list
    ...    ${{['F1G1-2', 'F1G1-1', 'F1G1-3']}}
    user clicks done button to collapse reorder list    3    Filter 1-reorder-list

Check no reordering is possible for filter 1 group 2's filter items after data replacement
    # 'F1G2-1' is removed in the replacement leaving only 'F1G2-2' which can't be reordered alone
    user checks reorder list has no reorder options button    2    Filter 1-reorder-list

Check the order of filter 1 group 3's filter items after data replacement
    # 'Filter 1 group 3' is new in the replacement so the filter items should be ordered by label
    user clicks button to reorder options within list    4    Filter 1-reorder-list
    user checks list contains exact items in order    testid:Filter 1 group 3-reorder-list
    ...    ${{['F1G3-1', 'F1G3-2']}}
    user clicks done button to collapse reorder list    4    Filter 1-reorder-list
    user clicks done button to collapse reorder list    2

Check the order of filter 2's filter groups after data replacement
    # Filter 2's filter groups are identical in the replacement so the previous order should be retained
    user clicks button to reorder options within list    1
    user checks list contains exact items in order    testid:Filter 2-reorder-list
    ...    ${{['Total', 'Filter 2 group 1', 'Filter 2 group 2']}}

Check no reordering is possible for filter 2's Total group after data replacement
    # Filter 2's 'Total' group only has one filter item which can't be reordered alone
    user checks reorder list has no reorder options button    1    Filter 2-reorder-list

Check the order of filter 2 group 1's filter items after data replacement
    # Filter 2 group 1's filter items are identical in the replacement so the previous order should be retained
    user clicks button to reorder options within list    2    Filter 2-reorder-list
    user checks list contains exact items in order    testid:Filter 2 group 1-reorder-list
    ...    ${{['F2G1-1', 'F2G1-2']}}
    user clicks done button to collapse reorder list    2    Filter 2-reorder-list

Check the order of filter 2 group 2's filter items after data replacement
    # Filter 2 group 2's filter items are identical in the replacement so the previous order should be retained
    user clicks button to reorder options within list    3    Filter 2-reorder-list
    user checks list contains exact items in order    testid:Filter 2 group 2-reorder-list
    ...    ${{['F2G2-1', 'F2G2-2']}}
    user clicks done button to collapse reorder list    3    Filter 2-reorder-list
    user clicks done button to collapse reorder list    1

Cancel reordering filters after data replacement
    user clicks button    Cancel    id:reordering
    user waits until h3 is not visible    Reorder filters for ${SUBJECT_NAME}

Check the order of indicator groups after data replacement
    # 'Indicator group 3' is new in the replacement so should be appended
    user clicks button in table cell    1    2    Reorder indicators    id:reordering
    user waits until h3 is visible    Reorder indicators for ${SUBJECT_NAME}
    user checks list contains exact items in order    testid:reorder-list
    ...    ${{['Indicator group 2', 'Indicator group 1', 'Indicator group 3']}}

Check the order of indicator group 1's indicators after data replacement
    # Indicator group 1's indicators are identical in the replacement so the previous order should be retained
    user clicks button to reorder options within list    2
    user checks list contains exact items in order    testid:Indicator group 1-reorder-list
    ...    ${{['Indicator 2', 'Indicator 1']}}
    user clicks done button to collapse reorder list    2

Check no reordering is possible for indicator group 2's indicators after data replacement
    # 'Indicator 3' is removed in the replacement leaving only 'Indicator 4' which can't be reordered alone
    user checks reorder list has no reorder options button    1

Check the order of indicator group 3's indicators after data replacement
    # 'Indicator group 3' is new in the replacement so the indicators should be ordered by label
    user clicks button to reorder options within list    3
    user checks list contains exact items in order    testid:Indicator group 3-reorder-list
    ...    ${{['Indicator 5', 'Indicator 6']}}
    user clicks done button to collapse reorder list    3

Cancel reordering indicators after data replacement
    user clicks button    Cancel    id:reordering
    user waits until h3 is not visible    Reorder indicators for ${SUBJECT_NAME}

Add data guidance to subject
    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance

    user waits until page contains element    id:dataGuidance-dataFiles
    user waits until page contains accordion section    ${SUBJECT_NAME}

    user enters text into data guidance data file content editor    ${SUBJECT_NAME}
    ...    ${SUBJECT_NAME} Main guidance content

Save data guidance
    user clicks button    Save guidance

Approve first release
    user clicks link    Sign off
    user approves release for immediate publication

Verify newly published release is on Find Statistics page
    user checks publication is on find statistics page    ${PUBLICATION_NAME}

Go to public table tool page
    user navigates to data tables page on public frontend

Select "Test Topic" publication
    environment variable should be set    TEST_THEME_NAME
    environment variable should be set    TEST_TOPIC_NAME
    user clicks radio    %{TEST_THEME_NAME}
    user clicks radio    ${PUBLICATION_NAME}
    user clicks element    id:publicationForm-submit
    user waits until table tool wizard step is available    2    Choose a subject
    user checks previous table tool step contains    1    Publication    ${PUBLICATION_NAME}

Select subject
    user clicks radio    ${SUBJECT_NAME}
    user clicks element    id:publicationSubjectForm-submit
    user waits until table tool wizard step is available    3    Choose locations
    user checks previous table tool step contains    2    Subject    ${SUBJECT_NAME}

Select all provider locations
    user opens details dropdown    Provider
    user clicks button    Select all 2 options
    user clicks element    id:locationFiltersForm-submit
    user waits until table tool wizard step is available    4    Choose time period
    user checks previous table tool step contains    3    Provider    Provider 1
    user checks previous table tool step contains    3    Provider    Provider 2

Select start date and end date
    user chooses select option    id:timePeriodForm-start    2021
    user chooses select option    id:timePeriodForm-end    2022
    user clicks element    id:timePeriodForm-submit
    user waits until table tool wizard step is available    5    Choose your filters
    user waits until page contains element    id:filtersForm-indicators
    user checks previous table tool step contains    4    Time period    2021 to 2022

Check indicator groups match the expected order
    user checks indicator groups list contains exact items in order
    ...    Indicator group 2
    ...    Indicator group 1
    ...    Indicator group 3

Check indicators match the expected order
    user checks indicator group contains exact items in order
    ...    Indicator 4
    ...    group_label=Indicator group 2
    user checks indicator group contains exact items in order
    ...    Indicator 2
    ...    Indicator 1
    ...    group_label=Indicator group 1
    user checks indicator group contains exact items in order
    ...    Indicator 5
    ...    Indicator 6
    ...    group_label=Indicator group 3

Check filters match the expected order
    user checks filters list contains exact items in order
    ...    Filter 2
    ...    Filter 1

Check filter groups match the expected order
    user opens details dropdown    Filter 2
    user checks filter contains exact items in order
    ...    Total
    ...    Filter 2 group 1
    ...    Filter 2 group 2
    ...    filter_label=Filter 2

    user opens details dropdown    Filter 1
    user checks filter contains exact items in order
    ...    Total
    ...    Filter 1 group 2
    ...    Filter 1 group 1
    ...    Filter 1 group 3
    ...    filter_label=Filter 1

Check filter 2's filter items match the expected order
    user checks filter group contains exact items in order
    ...    Total
    ...    filter_label=Filter 2
    ...    group_label=Total
    user checks filter group contains exact items in order
    ...    F2G1-1
    ...    F2G1-2
    ...    filter_label=Filter 2
    ...    group_label=Filter 2 group 1
    user checks filter group contains exact items in order
    ...    F2G2-1
    ...    F2G2-2
    ...    filter_label=Filter 2
    ...    group_label=Filter 2 group 2

Check filter 1's filter items match the expected order
    user checks filter group contains exact items in order
    ...    Total
    ...    filter_label=Filter 1
    ...    group_label=Total
    user checks filter group contains exact items in order
    ...    F1G2-2
    ...    filter_label=Filter 1
    ...    group_label=Filter 1 group 2
    user checks filter group contains exact items in order
    ...    F1G1-2
    ...    F1G1-1
    ...    F1G1-3
    ...    filter_label=Filter 1
    ...    group_label=Filter 1 group 1
    user checks filter group contains exact items in order
    ...    F1G3-1
    ...    F1G3-2
    ...    filter_label=Filter 1
    ...    group_label=Filter 1 group 3

Select all indicator options
    user clicks category checkbox    Indicator group 2    Indicator 4
    user clicks select all for category    Indicator group 1
    user clicks select all for category    Indicator group 3

Create table
    user clicks element    id:filtersForm-submit
    user waits until results table appears    %{WAIT_SMALL}

Validate step 5 options
    user checks previous table tool step contains    5    Indicators    Indicator 4
    user checks previous table tool step contains    5    Indicators    Indicator 2
    user checks previous table tool step contains    5    Indicators    Indicator 1
    user checks previous table tool step contains    5    Indicators    Indicator 5
    user checks previous table tool step contains    5    Indicators    Indicator 6
    user checks previous table tool step contains    5    Filter 2    Total
    user checks previous table tool step contains    5    Filter 1    Total

Validate results table column headings
    user checks table row heading contains    1    1    Provider 1
    user checks table row heading contains    1    2    Indicator 4
    user checks table row heading contains    2    1    Indicator 2
    user checks table row heading contains    3    1    Indicator 1
    user checks table row heading contains    4    1    Indicator 5
    user checks table row heading contains    5    1    Indicator 6

    user checks table row heading contains    6    1    Provider 2
    user checks table row heading contains    6    2    Indicator 4
    user checks table row heading contains    7    1    Indicator 2
    user checks table row heading contains    8    1    Indicator 1
    user checks table row heading contains    9    1    Indicator 5
    user checks table row heading contains    10    1    Indicator 6

Validate row headings
    user checks table column heading contains    1    1    2021
    user checks table column heading contains    1    2    2022

Validate table cells
    user checks table cell contains    1    1    91,732,417
    user checks table cell contains    2    1    89,626,171
    user checks table cell contains    3    1    51,029,335
    user checks table cell contains    4    1    83,771,711
    user checks table cell contains    5    1    91,439,757
    user checks table cell contains    6    1    84,288,196
    user checks table cell contains    7    1    68,482,122
    user checks table cell contains    8    1    65,182,202
    user checks table cell contains    9    1    61,121,060
    user checks table cell contains    10    1    37,961,046

    user checks table cell contains    1    2    56,920,389
    user checks table cell contains    2    2    75,873,610
    user checks table cell contains    3    2    49,220,007
    user checks table cell contains    4    2    31,859,890
    user checks table cell contains    5    2    63,230,370
    user checks table cell contains    6    2    54,759,203
    user checks table cell contains    7    2    10,214,603
    user checks table cell contains    8    2    28,403,292
    user checks table cell contains    9    2    24,284,255
    user checks table cell contains    10    2    39,761,905

Go back to locations step
    user clicks button    Edit locations
    user waits until page contains element    xpath://h2[text()="Go back to previous step"]
    user clicks button    Confirm
    user waits until table tool wizard step is available    3    Choose locations

Select provider 1
    user opens details dropdown    Provider
    user clicks button    Unselect all 2 options    css:#locationFiltersForm
    user checks page contains element
    ...    xpath://*[@class="govuk-error-message" and text()="Select at least one location"]
    user clicks checkbox    Provider 1
    user clicks element    id:locationFiltersForm-submit
    user waits until table tool wizard step is available    4    Choose time period
    user checks previous table tool step contains    3    Provider    Provider 1

Change time period to 2021
    user chooses select option    id:timePeriodForm-start    2021
    user chooses select option    id:timePeriodForm-end    2021
    user clicks element    id:timePeriodForm-submit
    user waits until table tool wizard step is available    5    Choose your filters
    user checks previous table tool step contains    4    Time period    2021

Select a single indicator
    user clicks button    Unselect all 5 options    css:#filtersForm-indicators
    user clicks category checkbox    Indicator group 1    Indicator 1

Select all filter 1's filters
    user opens details dropdown    Filter 1
    user clicks button    Select all 7 options    css:#filtersForm-filters

Create table again
    user clicks element    id:filtersForm-submit
    user waits until results table appears    %{WAIT_SMALL}

Validate step 5 options again
    user clicks button    Show 2 more filters
    user checks previous table tool step contains    5    Indicators    Indicator 1
    user checks previous table tool step contains    5    Filter 2    Total
    user checks previous table tool step contains    5    Filter 1    Total
    user checks previous table tool step contains    5    Filter 1    F1G2-2
    user checks previous table tool step contains    5    Filter 1    F1G1-2
    user checks previous table tool step contains    5    Filter 1    F1G1-1
    user checks previous table tool step contains    5    Filter 1    F1G1-3
    user checks previous table tool step contains    5    Filter 1    F1G3-1
    user checks previous table tool step contains    5    Filter 1    F1G3-2

Validate results table column headings again
    user checks table row heading contains    1    1    Total
    user checks table row heading contains    2    1    Filter 1 group 2
    user checks table row heading contains    2    2    F1G2-2
    user checks table row heading contains    3    1    Filter 1 group 1
    user checks table row heading contains    3    2    F1G1-2
    user checks table row heading contains    4    1    F1G1-1
    user checks table row heading contains    5    1    F1G1-3
    user checks table row heading contains    6    1    Filter 1 group 3
    user checks table row heading contains    6    2    F1G3-1
    user checks table row heading contains    7    1    F1G3-2

Validate row headings again
    user checks table column heading contains    1    1    2021

Validate table cells again
    user checks table cell contains    1    1    51,029,335
    user checks table cell contains    2    1    7,392,787
    user checks table cell contains    3    1    7,552,538
    user checks table cell contains    4    1    2,553,275
    user checks table cell contains    5    1    4,355,910
    user checks table cell contains    6    1    7,922,048
    user checks table cell contains    7    1    6,759,498


*** Keywords ***
user moves item of draggable list down
    [Arguments]    ${locator}    ${item_num}
    ${item}    user gets list item element    ${locator}    ${item_num}
    set focus to element    ${item}
    user presses keys    ${SPACE}
    user presses keys    ARROW_DOWN
    user presses keys    ${SPACE}

user clicks button in reorder list
    [Arguments]    ${button_text}    ${item_num}    ${list_test_id}=reorder-list
    user clicks button    ${button_text}    xpath://ol[@data-testid="${list_test_id}"]/li[position()=${item_num}]

user checks button is not in reorder list
    [Arguments]    ${button_text}    ${item_num}    ${list_test_id}=reorder-list
    user checks page does not contain element
    ...    xpath://ol[@data-testid="${list_test_id}"]/li[position()=${item_num}]//button[text()="${button_text}"]

user checks reorder list has no reorder options button
    [Arguments]    ${item_num}    ${list_test_id}=reorder-list
    user checks button is not in reorder list    Reorder options within this group    ${item_num}    ${list_test_id}

user clicks button to reorder options within list
    [Arguments]    ${item_num}    ${list_test_id}=reorder-list
    user clicks button in reorder list    Reorder options within this group    ${item_num}    ${list_test_id}

user clicks done button to collapse reorder list
    [Arguments]    ${item_num}    ${list_test_id}=reorder-list
    user clicks button in reorder list    Done    ${item_num}    ${list_test_id}
