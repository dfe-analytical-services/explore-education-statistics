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
${RELEASE_NAME}         Calendar Year 2022
${SUBJECT_NAME}         UI test subject

*** Test Cases ***
Create new publication and release via API
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user create test release via api    ${PUBLICATION_ID}    CY    2022

Navigate to release
    user navigates to editable release summary from admin dashboard
    ...    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME} (not Live)

Upload subject to release
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

    user checks results table cell contains    1    1    ${SUBJECT_NAME}    id:reordering
    user checks results table cell contains    1    2    Reorder filters    id:reordering
    user checks results table cell contains    1    2    Reorder indicators    id:reordering

Check the initial order of filters
    user clicks button in table cell    1    2    Reorder filters    id:reordering
    user waits until h3 is visible    Reorder filters for ${SUBJECT_NAME}
    user checks list contains exactly items in order    testid:reorder-list
    ...    ${{['Filter 1', 'Filter 2']}}

Reorder filters
    user moves item of draggable list down    testid:reorder-list    1
    user checks list contains exactly items in order    testid:reorder-list
    ...    ${{['Filter 2', 'Filter 1']}}

Check the initial order of filter 1's filter groups
    user clicks button to reorder options within list    2
    user checks list contains exactly items in order    testid:Filter 1-reorder-list
    ...    ${{['Total', 'Filter 1 group 1', 'Filter 1 group 2']}}

Reorder filter 1's filter groups
    user moves item of draggable list down    testid:Filter 1-reorder-list    2
    user checks list contains exactly items in order    testid:Filter 1-reorder-list
    ...    ${{['Total', 'Filter 1 group 2', 'Filter 1 group 1']}}

Check the initial order of filter 1 group 1's filter items
    user clicks button to reorder options within list    3    Filter 1-reorder-list
    user checks list contains exactly items in order    testid:Filter 1 group 1-reorder-list
    ...    ${{['F1G1-1', 'F1G1-2']}}

Reorder filter 1 group 1's filter items
    user moves item of draggable list down    testid:Filter 1 group 1-reorder-list    1
    user checks list contains exactly items in order    testid:Filter 1 group 1-reorder-list
    ...    ${{['F1G1-2', 'F1G1-1']}}
    user clicks done button to collapse reorder list    3    Filter 1-reorder-list

Check the initial order of filter 1 group 2's filter items
    user clicks button to reorder options within list    2    Filter 1-reorder-list
    user checks list contains exactly items in order    testid:Filter 1 group 2-reorder-list
    ...    ${{['F1G2-1', 'F1G2-2']}}

Reorder filter 1 group 2's filter items
    user moves item of draggable list down    testid:Filter 1 group 2-reorder-list    1
    user checks list contains exactly items in order    testid:Filter 1 group 2-reorder-list
    ...    ${{['F1G2-2', 'F1G2-1']}}
    user clicks done button to collapse reorder list    2    Filter 1-reorder-list
    user clicks done button to collapse reorder list    2

Save reordered filters, filter groups and filter items
    user clicks button    Save order
    user waits until h3 is not visible    Reorder filters for ${SUBJECT_NAME}

Check the saved order of filters
    user clicks button in table cell    1    2    Reorder filters    id:reordering
    user waits until h3 is visible    Reorder filters for ${SUBJECT_NAME}
    user checks list contains exactly items in order    testid:reorder-list
    ...    ${{['Filter 2', 'Filter 1']}}

Check the saved order of filter 1's filter groups
    user clicks button to reorder options within list    2
    user checks list contains exactly items in order    testid:Filter 1-reorder-list
    ...    ${{['Total', 'Filter 1 group 2', 'Filter 1 group 1']}}

Check no reordering is possible for filter 1's Total group
    # Filter 1's 'Total' group only has one filter item which can't be reordered alone
    user checks reorder list has no reorder options button    1    Filter 1-reorder-list

Check the saved order of filter 1 group 1's filter items
    user clicks button to reorder options within list    3    Filter 1-reorder-list
    user checks list contains exactly items in order    testid:Filter 1 group 1-reorder-list
    ...    ${{['F1G1-2', 'F1G1-1']}}
    user clicks done button to collapse reorder list    3    Filter 1-reorder-list

Check the saved order of filter 1 group 2's filter items
    user clicks button to reorder options within list    2    Filter 1-reorder-list
    user checks list contains exactly items in order    testid:Filter 1 group 2-reorder-list
    ...    ${{['F1G2-2', 'F1G2-1']}}
    user clicks done button to collapse reorder list    2    Filter 1-reorder-list
    user clicks done button to collapse reorder list    2

Check the saved order of filter 2's filter groups remains untouched
    user clicks button to reorder options within list    1
    user checks list contains exactly items in order    testid:Filter 2-reorder-list
    ...    ${{['Total', 'Filter 2 group 1', 'Filter 2 group 2']}}

Check no reordering is possible for filter 2's Total group
    # Filter 2's 'Total' group only has one filter item which can't be reordered alone
    user checks reorder list has no reorder options button    1    Filter 2-reorder-list

Check the saved order of filter 2 group 1's filter items remains untouched
    user clicks button to reorder options within list    2    Filter 2-reorder-list
    user checks list contains exactly items in order    testid:Filter 2 group 1-reorder-list
    ...    ${{['F2G1-1', 'F2G1-2']}}
    user clicks done button to collapse reorder list    2    Filter 2-reorder-list

Check the saved order of filter 2 group 2's filter items remains untouched
    user clicks button to reorder options within list    3    Filter 2-reorder-list
    user checks list contains exactly items in order    testid:Filter 2 group 2-reorder-list
    ...    ${{['F2G2-1', 'F2G2-2']}}
    user clicks done button to collapse reorder list    3    Filter 2-reorder-list
    user clicks done button to collapse reorder list    1

Cancel reordering filters
    user clicks button    Cancel    id:reordering
    user waits until h3 is not visible    Reorder filters for ${SUBJECT_NAME}

Check the initial order of indicator groups
    user clicks button in table cell    1    2    Reorder indicators    id:reordering
    user waits until h3 is visible    Reorder indicators for ${SUBJECT_NAME}
    user checks list contains exactly items in order    testid:reorder-list
    ...    ${{['Indicator group 1', 'Indicator group 2']}}

Reorder indicator groups
    user moves item of draggable list down    testid:reorder-list    1
    user checks list contains exactly items in order    testid:reorder-list
    ...    ${{['Indicator group 2', 'Indicator group 1']}}

Check the initial order of indicator group 1's indicators
    user clicks button to reorder options within list    2
    user checks list contains exactly items in order    testid:Indicator group 1-reorder-list
    ...    ${{['Indicator 1', 'Indicator 2']}}

Reorder indicator group 1's indicators
    user moves item of draggable list down    testid:Indicator group 1-reorder-list    1
    user checks list contains exactly items in order    testid:Indicator group 1-reorder-list
    ...    ${{['Indicator 2', 'Indicator 1']}}
    user clicks done button to collapse reorder list    2

Save reordered indicator groups and indicators
    user clicks button    Save order
    user waits until h3 is not visible    Reorder filters for ${SUBJECT_NAME}

Check the saved order of indicator groups
    user clicks button in table cell    1    2    Reorder indicators    id:reordering
    user waits until h3 is visible    Reorder indicators for ${SUBJECT_NAME}
    user checks list contains exactly items in order    testid:reorder-list
    ...    ${{['Indicator group 2', 'Indicator group 1']}}

Check the saved order of indicator group 1's indicators
    user clicks button to reorder options within list    2
    user checks list contains exactly items in order    testid:Indicator group 1-reorder-list
    ...    ${{['Indicator 2', 'Indicator 1']}}
    user clicks done button to collapse reorder list    2

Check the saved order of indicator group 2's indicators remains untouched
    user clicks button to reorder options within list    1
    user checks list contains exactly items in order    testid:Indicator group 2-reorder-list
    ...    ${{['Indicator 3', 'Indicator 4']}}
    user clicks done button to collapse reorder list    1

Cancel reordering indicators
    user clicks button    Cancel    id:reordering
    user waits until h3 is not visible    Reorder indicators for ${SUBJECT_NAME}

Replace subject data
    user clicks link    Data uploads
    user waits until h2 is visible    Uploaded data files
    ${section}=    user gets accordion section content element    ${SUBJECT_NAME}
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
    user checks headed table body row cell contains    Number of rows    1    101
    user checks headed table body row cell contains    Data file size    1    13 Kb
    user checks headed table body row cell contains    Status    1    Data replacement in progress    wait=%{WAIT_LONG}

    user checks headed table body row cell contains    Subject title    2    ${SUBJECT_NAME}
    user checks headed table body row cell contains    Data file    2    grouped-filters-and-indicators-replacement.csv
    user checks headed table body row cell contains    Metadata file    2
    ...    grouped-filters-and-indicators-replacement.meta.csv
    user checks headed table body row cell contains    Number of rows    2    141
    user checks headed table body row cell contains    Data file size    2    19 Kb
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

    user checks results table cell contains    1    1    ${SUBJECT_NAME}    id:reordering
    user checks results table cell contains    1    2    Reorder filters    id:reordering
    user checks results table cell contains    1    2    Reorder indicators    id:reordering

Check the order of filters after data replacement
    # Filters are identical in the replacement so the previous order should be retained
    user clicks button in table cell    1    2    Reorder filters    id:reordering
    user waits until h3 is visible    Reorder filters for ${SUBJECT_NAME}
    user checks list contains exactly items in order    testid:reorder-list
    ...    ${{['Filter 2', 'Filter 1']}}

Check the order of filter 1's filter groups after data replacement
    # 'Filter 1 group 3' is new in the replacement so should be appended
    user clicks button to reorder options within list    2
    user checks list contains exactly items in order    testid:Filter 1-reorder-list
    ...    ${{['Total', 'Filter 1 group 2', 'Filter 1 group 1', 'Filter 1 group 3']}}

Check no reordering is possible for filter 1's Total group after data replacement
    # Filter 1's 'Total' group only has one filter item which can't be reordered alone
    user checks reorder list has no reorder options button    1    Filter 1-reorder-list

Check the order of filter 1 group 1's filter items after data replacement
    # 'F1G1-3' is new in the replacement so should be appended
    user clicks button to reorder options within list    3    Filter 1-reorder-list
    user checks list contains exactly items in order    testid:Filter 1 group 1-reorder-list
    ...    ${{['F1G1-2', 'F1G1-1', 'F1G1-3']}}
    user clicks done button to collapse reorder list    3    Filter 1-reorder-list

Check no reordering is possible for filter 1 group 2's filter items after data replacement
    # 'F1G2-1' is removed in the replacement leaving only 'F1G2-2' which can't be reordered alone
    user checks reorder list has no reorder options button    2    Filter 1-reorder-list

Check the order of filter 1 group 3's filter items after data replacement
    # 'Filter 1 group 3' is new in the replacement so the filter items should be ordered by label
    user clicks button to reorder options within list    4    Filter 1-reorder-list
    user checks list contains exactly items in order    testid:Filter 1 group 3-reorder-list
    ...    ${{['F1G3-1', 'F1G3-2']}}
    user clicks done button to collapse reorder list    4    Filter 1-reorder-list
    user clicks done button to collapse reorder list    2

Check the order of filter 2's filter groups after data replacement
    # Filter 2's filter groups are identical in the replacement so the previous order should be retained
    user clicks button to reorder options within list    1
    user checks list contains exactly items in order    testid:Filter 2-reorder-list
    ...    ${{['Total', 'Filter 2 group 1', 'Filter 2 group 2']}}

Check no reordering is possible for filter 2's Total group after data replacement
    # Filter 2's 'Total' group only has one filter item which can't be reordered alone
    user checks reorder list has no reorder options button    1    Filter 2-reorder-list

Check the order of filter 2 group 1's filter items after data replacement
    # Filter 2 group 1's filter items are identical in the replacement so the previous order should be retained
    user clicks button to reorder options within list    2    Filter 2-reorder-list
    user checks list contains exactly items in order    testid:Filter 2 group 1-reorder-list
    ...    ${{['F2G1-1', 'F2G1-2']}}
    user clicks done button to collapse reorder list    2    Filter 2-reorder-list

Check the order of filter 2 group 2's filter items after data replacement
    # Filter 2 group 2's filter items are identical in the replacement so the previous order should be retained
    user clicks button to reorder options within list    3    Filter 2-reorder-list
    user checks list contains exactly items in order    testid:Filter 2 group 2-reorder-list
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
    user checks list contains exactly items in order    testid:reorder-list
    ...    ${{['Indicator group 2', 'Indicator group 1', 'Indicator group 3']}}

Check the order of indicator group 1's indicators after data replacement
    # Indicator group 1's indicators are identical in the replacement so the previous order should be retained
    user clicks button to reorder options within list    2
    user checks list contains exactly items in order    testid:Indicator group 1-reorder-list
    ...    ${{['Indicator 2', 'Indicator 1']}}
    user clicks done button to collapse reorder list    2

Check no reordering is possible for indicator group 2's indicaotrs after data replacement
    # 'Indicator 3' is removed in the replacement leaving only 'Indicator 4' which can't be reordered alone
    user checks reorder list has no reorder options button    1

Check the order of indicator group 3's indicators after data replacement
    # 'Indicator group 3' is new in the replacement so the indicators should be ordered by label
    user clicks button to reorder options within list    3
    user checks list contains exactly items in order    testid:Indicator group 3-reorder-list
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

User goes to public Find Statistics page
    user navigates to find statistics page on public frontend

Verify newly published release is on Find Statistics page
    environment variable should be set    TEST_THEME_NAME
    environment variable should be set    TEST_TOPIC_NAME
    user waits until page contains accordion section    %{TEST_THEME_NAME}
    user opens accordion section    %{TEST_THEME_NAME}
    user waits until accordion section contains text    %{TEST_THEME_NAME}    %{TEST_TOPIC_NAME}

    user opens details dropdown    %{TEST_TOPIC_NAME}
    user waits until details dropdown contains publication    %{TEST_TOPIC_NAME}    ${PUBLICATION_NAME}

*** Keywords ***
user moves item of draggable list down
    [Arguments]    ${locator}    ${item_num}
    ${item}=    user gets list item element    ${locator}    ${item_num}
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
