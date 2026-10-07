*** Settings ***
Resource            ../../libs/admin-common.robot
Resource            ../../libs/table_tool.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${PUBLICATION_NAME}=    replace data mapping %{RUN_IDENTIFIER}
${RELEASE_LABEL}=       test release
${RELEASE_NAME}=        Financial year 3000-01 ${RELEASE_LABEL}
${SUBJECT_NAME}=        replace data mapping subject
${DATABLOCK_NAME}=      replace data mapping data block
${INDICATORS_TABLE}=    css:#replacements-differences-indicators-table
${LOCATIONS_TABLE}=     css:#replacements-differences-locations-table
${FILTERS_TABLE}=       css:#replacements-differences-filters-table
${DATA_BLOCK_PLAN}=     xpath://details[summary[contains(., "${DATABLOCK_NAME}")]]


*** Test Cases ***
Create new publication for "UI tests theme" theme
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    FY    3000    label=${RELEASE_LABEL}

Go to "Release summary" page
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}

Upload original data set
    user uploads subject and waits until complete    ${SUBJECT_NAME}    mapping_all.csv    mapping_all.meta.csv

Create data block using the locations, indicators and filters that will need mapping
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks

    user clicks link    Create data block
    user waits until h2 is visible    Create data block

    user waits until table tool wizard step is available    1    Select a data set
    user waits until page contains    ${SUBJECT_NAME}
    user clicks radio    ${SUBJECT_NAME}
    user clicks element    id:publicationDataStepForm-submit

    user waits until table tool wizard step is available    2    Choose locations
    user clicks element
    ...    xpath://button[@data-testid="filter-accordion-button"][contains(., "Local authority")]
    user clicks checkbox    Birmingham
    user clicks checkbox    Barnsley
    user clicks checkbox    Greenwich
    user clicks checkbox    Camden
    user clicks element    id:locationFiltersForm-submit

    user waits until table tool wizard step is available    3    Choose time period
    user chooses select option    id:timePeriodForm-start    2020/21
    user chooses select option    id:timePeriodForm-end    2021/22
    user clicks element    id:timePeriodForm-submit

    user waits until table tool wizard step is available    4    Choose your filters
    user clicks indicator checkbox    Indicator one
    user clicks indicator checkbox    Indicator two

    user clicks element
    ...    xpath://button[@data-testid="filter-accordion-button"][contains(., "Filter one")]
    user clicks checkbox    Filter one item A

    user clicks element
    ...    xpath://button[@data-testid="filter-accordion-button"][contains(., "Filter two")]
    user clicks category checkbox    Filter two group A    Filter two item A1
    user clicks category checkbox    Filter two group B    Filter two item B1
    user clicks category checkbox    Filter two group B    Filter two item B2

    user clicks element    id:filtersForm-submit
    user waits until results table appears    %{WAIT_LONG}

    user enters text into element    label:Data block name    ${DATABLOCK_NAME}
    user clicks button    Save data block
    user waits until page contains    Delete this data block

Upload replacement data set
    user clicks link    Data and files
    user waits until page contains data uploads table
    user uploads subject replacement    ${SUBJECT_NAME}    mapping_all_replacement.csv
    ...    mapping_all_replacement.meta.csv
    user waits until page contains element    ${REPLACEMENTS_TABLE_SELECTOR}
    user confirms replacement upload    ${SUBJECT_NAME}    Error

Go to data replacement page
    user clicks link in table cell    1    4    View details    ${REPLACEMENTS_TABLE_SELECTOR}
    user waits until page contains element    testid:Replacement Title
    user checks headed table body row cell contains    Data file import status    2    Complete
    ...    wait=%{WAIT_DATA_FILE_IMPORT}

Verify the renamed indicator needs mapping
    user waits until h3 is visible    Mapping Dependencies
    user waits until page contains    The following items were not found in the replacement data

    user waits until element contains    ${INDICATORS_TABLE} caption    1 unmapped indicator    %{WAIT_MEDIUM}
    user checks table body has x rows    1    ${INDICATORS_TABLE}
    user waits until parent contains element    ${INDICATORS_TABLE}
    ...    xpath:.//tbody/tr/th[text()="Indicator group"]
    user checks table cell contains    1    1    Indicator two    ${INDICATORS_TABLE}
    user checks table cell contains    1    2    not present    ${INDICATORS_TABLE}
    user checks table cell contains    1    3    No mapping    ${INDICATORS_TABLE}
    user checks table cell contains    1    3    Map item    ${INDICATORS_TABLE}

Verify the changed location needs mapping
    user waits until element contains    ${LOCATIONS_TABLE} caption    1 unmapped location    %{WAIT_MEDIUM}
    user checks table body has x rows    1    ${LOCATIONS_TABLE}
    user waits until parent contains element    ${LOCATIONS_TABLE}
    ...    xpath:.//tbody/tr/th[text()="LocalAuthority"]
    user checks table cell contains    1    1    Birmingham    ${LOCATIONS_TABLE}
    user checks table cell contains    1    2    not present    ${LOCATIONS_TABLE}
    user checks table cell contains    1    3    No mapping    ${LOCATIONS_TABLE}
    user checks table cell contains    1    3    Map item    ${LOCATIONS_TABLE}

Map the renamed indicator
    user clicks button in table cell    1    3    Map item    ${INDICATORS_TABLE}
    user waits until modal is visible    Map existing indicator
    user checks summary list contains    Label    Indicator two    ${MODAL_SELECTOR}
    user checks summary list contains    Name    ind_two    ${MODAL_SELECTOR}
    user clicks radio    Indicator two renamed
    user clicks modal button    Save
    user waits until modal is not visible    Map existing indicator

    user waits until element contains    ${INDICATORS_TABLE} caption    1 mapped indicator    %{WAIT_MEDIUM}
    user checks table cell contains    1    2    Indicator two renamed    ${INDICATORS_TABLE}
    user checks table cell does not contain    1    2    not present    ${INDICATORS_TABLE}

Map the changed location
    user clicks button in table cell    1    3    Map item    ${LOCATIONS_TABLE}
    user waits until modal is visible    Map existing location
    user checks summary list contains    Name    Birmingham    ${MODAL_SELECTOR}
    user checks summary list contains    Code    E08000025    ${MODAL_SELECTOR}
    user clicks radio    BirminghamUpdated
    user clicks modal button    Save
    user waits until modal is not visible    Map existing location

    user waits until element contains    ${LOCATIONS_TABLE} caption    1 mapped location    %{WAIT_MEDIUM}
    user checks table cell contains    1    2    BirminghamUpdated    ${LOCATIONS_TABLE}
    user checks table cell does not contain    1    2    not present    ${LOCATIONS_TABLE}

Verify the mapped location and indicator are no longer missing from the data block
    user waits until h3 is visible    Data blocks: ERROR    %{WAIT_MEDIUM}
    user waits until page does not contain testid    loadingSpinner    %{WAIT_MEDIUM}

    user waits until page contains element    ${DATA_BLOCK_PLAN}//li[text()="Barnsley"]
    user waits until page does not contain element
    ...    ${DATA_BLOCK_PLAN}//li[contains(., "Birmingham")][contains(., "not present")]
    user waits until page does not contain element
    ...    ${DATA_BLOCK_PLAN}//li[contains(., "Indicator two")][contains(., "not present")]

Verify the replacement cannot be completed while filters need mapping
    user waits until page contains element
    ...    ${DATA_BLOCK_PLAN}//li[contains(., "Filter one item A")][contains(., "not present")]
    user waits until page contains element
    ...    ${DATA_BLOCK_PLAN}//li[contains(., "Filter two item A1")][contains(., "not present")]
    user waits until page contains element
    ...    ${DATA_BLOCK_PLAN}//li[contains(., "Filter two item B1")][contains(., "not present")]
    user waits until page does not contain element
    ...    ${DATA_BLOCK_PLAN}//li[contains(., "Filter two item B2")][contains(., "not present")]

    user checks page does not contain button    Confirm data replacement

Verify the renamed filter, filter group and filter item need mapping
    # Filter one is unmapped, so its group and two items are counted as unmapped but not shown.
    # Filter two group A is unmapped, so its single item is also counted but not shown.
    user checks filters table caption contains tag    7 unmapped filters
    user checks filters table caption contains tag    4 not shown
    user checks filters table has no mapped filters tag

    # Rows are located by label rather than position because the order of filter groups isn't stable
    user checks table body has x rows    3    ${FILTERS_TABLE}

    # Filter one was renamed, so the whole filter needs mapping and its group and items are hidden
    user checks filters table row cell contains    Filter one    3    not present
    user checks filters table row cell contains    Filter one    4    No mapping
    user checks filters table row cell contains    Filter one    4    Map item

    # Filter two was auto-mapped, so it is a heading spanning its renamed group and its renamed item
    user waits until parent contains element    ${FILTERS_TABLE}
    ...    xpath:.//tbody/tr/th[text()="Filter two"][@rowspan="2"]
    user checks filters table row cell contains    Filter two group A    3    not present
    user checks filters table row cell contains    Filter two group A    4    No mapping
    user checks filters table row cell contains    Filter two group A    4    Map item

    # Filter two group B was auto-mapped, so it is a heading for its renamed item
    user checks filters table row has heading    Filter two item B1    Filter two group B
    user checks filters table row cell contains    Filter two item B1    2    not present
    user checks filters table row cell contains    Filter two item B1    3    No mapping
    user checks filters table row cell contains    Filter two item B1    3    Map item

Map the renamed filter
    user clicks map item button in filters table row    Filter one
    user waits until modal is visible    Map existing filter
    user checks summary list contains    Label    Filter one    ${MODAL_SELECTOR}
    user clicks radio    Filter one renamed
    user clicks modal button    Save
    user waits until modal is not visible    Map existing filter

    # Mapping the filter auto-maps its group and items by label, so they are no longer unmapped or hidden
    user checks filters table caption contains tag    3 unmapped filters    %{WAIT_MEDIUM}
    user checks filters table caption contains tag    1 not shown
    user checks filters table caption contains tag    1 mapped filter

    user checks table body has x rows    3    ${FILTERS_TABLE}
    user checks filters table row cell contains    Filter one    3    Filter one renamed
    user checks filters table row cell does not contain    Filter one    3    not present
    user checks filters table row cell does not contain    Filter one    4    No mapping

Map the renamed filter group
    user clicks map item button in filters table row    Filter two group A
    user waits until modal is visible    Map existing filter
    user checks summary list contains    Label    Filter two group A    ${MODAL_SELECTOR}
    user clicks radio    Filter two group A renamed
    user clicks modal button    Save
    user waits until modal is not visible    Map existing filter

    # Mapping the group auto-maps its item by label, so nothing is hidden any more
    user checks filters table caption contains tag    1 unmapped filter    %{WAIT_MEDIUM}
    user checks filters table caption contains tag    2 mapped filters
    user checks filters table caption does not contain tag    not shown

    user checks table body has x rows    3    ${FILTERS_TABLE}
    user checks filters table row cell contains    Filter two group A    3    Filter two group A renamed
    user checks filters table row cell does not contain    Filter two group A    3    not present
    user checks filters table row cell does not contain    Filter two group A    4    No mapping

Map the renamed filter item
    user clicks map item button in filters table row    Filter two item B1
    user waits until modal is visible    Map existing filter
    user checks summary list contains    Label    Filter two item B1    ${MODAL_SELECTOR}
    user clicks radio    Filter two item B1 renamed
    user clicks modal button    Save
    user waits until modal is not visible    Map existing filter

    user checks filters table caption contains tag    3 mapped filters    %{WAIT_MEDIUM}
    user checks filters table caption does not contain tag    unmapped
    user checks filters table caption does not contain tag    not shown

    user checks table body has x rows    3    ${FILTERS_TABLE}
    user checks filters table row cell contains    Filter two item B1    2    Filter two item B1 renamed
    user checks filters table row cell does not contain    Filter two item B1    2    not present
    user checks filters table row cell does not contain    Filter two item B1    3    No mapping

Verify the data block is valid once everything is mapped
    user waits until h3 is visible    Data blocks: OK    %{WAIT_MEDIUM}
    user waits until page does not contain testid    loadingSpinner    %{WAIT_MEDIUM}

    user waits until page contains element    ${DATA_BLOCK_PLAN}//summary//strong[contains(., "OK")]
    user waits until page contains element
    ...    ${DATA_BLOCK_PLAN}//p[contains(., "This data block has no conflicts and can be replaced")]
    user waits until page does not contain element    ${DATA_BLOCK_PLAN}//*[contains(., "not present")]

Confirm the data replacement
    user waits until page contains button    Confirm data replacement
    user clicks button    Confirm data replacement
    user waits until h2 is visible    Data replacement complete    %{WAIT_MEDIUM}

Verify the data block uses the mapped items after the replacement
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks
    user waits until table is visible
    user checks table body has x rows    1
    user checks table cell contains    1    1    ${DATABLOCK_NAME}

    user clicks edit data block link    ${DATABLOCK_NAME}
    user waits until results table appears    %{WAIT_LONG}

    user waits until page contains element    xpath://table//th[contains(., "BirminghamUpdated")]
    user waits until page contains element    xpath://table//th[contains(., "Indicator two renamed")]
    user waits until page contains element    xpath://table//th[contains(., "Filter one item A")]
    user waits until page contains element    xpath://table//th[contains(., "Filter two item A1")]
    user waits until page contains element    xpath://table//th[contains(., "Filter two item B1 renamed")]
    user waits until page contains element    xpath://table//th[contains(., "Filter two item B2")]
    user checks page does not contain element    xpath://table//th[contains(., "not present")]


*** Keywords ***
user checks filters table caption contains tag
    [Arguments]    ${text}    ${wait}=${timeout}
    user waits until parent contains element    ${FILTERS_TABLE}
    ...    xpath:.//caption//strong[contains(., "${text}")]    timeout=${wait}

user checks filters table caption does not contain tag
    [Arguments]    ${text}
    user waits until parent does not contain element    ${FILTERS_TABLE}
    ...    xpath:.//caption//strong[contains(., "${text}")]

user checks filters table has no mapped filters tag
    # Matched by colour rather than text, since "unmapped filters" also contains "mapped filter"
    user waits until parent does not contain element    ${FILTERS_TABLE}
    ...    xpath:.//caption//strong[contains(@class, "govuk-tag--blue")]

user checks filters table row cell contains
    [Arguments]    ${row_label}    ${column}    ${expected}    ${wait}=${timeout}
    user waits until parent contains element    ${FILTERS_TABLE}
    ...    xpath:.//tbody/tr[td[1][normalize-space(.)="${row_label}"]]/td[${column}][contains(., "${expected}")]
    ...    timeout=${wait}

user checks filters table row cell does not contain
    [Arguments]    ${row_label}    ${column}    ${expected}
    user waits until parent does not contain element    ${FILTERS_TABLE}
    ...    xpath:.//tbody/tr[td[1][normalize-space(.)="${row_label}"]]/td[${column}][contains(., "${expected}")]

user checks filters table row has heading
    [Arguments]    ${row_label}    ${heading}
    user waits until parent contains element    ${FILTERS_TABLE}
    ...    xpath:.//tbody/tr[td[1][normalize-space(.)="${row_label}"]]/th[text()="${heading}"]

user clicks map item button in filters table row
    [Arguments]    ${row_label}
    ${row_xpath}=    Set Variable    xpath:.//tbody/tr[td[1][normalize-space(.)="${row_label}"]]
    user waits until parent contains element    ${FILTERS_TABLE}    ${row_xpath}
    ${row}=    get child element    ${FILTERS_TABLE}    ${row_xpath}
    user clicks button containing text    Map item    ${row}
