*** Settings ***
Resource    ./common.robot


*** Keywords ***
user waits until results table appears
    [Arguments]    ${wait_time}
    user waits until page contains element    css:table thead th    ${wait_time}
    user waits until page finishes loading

user clicks indicator checkbox
    [Arguments]    ${indicator_label}
    user clicks element    xpath://*[@id="filtersForm-indicators"]//label[text()="${indicator_label}"]

user checks indicator checkbox is checked
    [Arguments]    ${indicator_label}
    user checks checkbox input is checked
    ...    xpath://*[@id="filtersForm-indicators"]//label[contains(text(), "${indicator_label}")]/../input[@type="checkbox"]

user clicks subheaded indicator checkbox
    [Arguments]    ${subheading_label}    ${indicator_label}
    user clicks element
    ...    xpath://*[@id="filtersForm-indicators"]//legend[text()="${subheading_label}"]/..//label[text()="${indicator_label}"]/../input[@type="checkbox"]

user checks indicator groups list contains exact items in order
    [Arguments]    @{items}
    user checks items matching locator contain exact items in order    @{items}
    ...    locator=xpath://*[@id="filtersForm-indicators"]//*[starts-with(@id,"filtersForm-indicators-options-")]//legend

user checks indicator group contains exact items in order
    [Arguments]    @{items}    ${group_label}
    user checks items matching locator contain exact items in order
    ...    @{items}
    ...    locator=xpath://*[@id="filtersForm-indicators"]//legend[text()="${group_label}"]/..//label

user checks filters list contains exact items in order
    [Arguments]    @{items}
    user checks items matching locator contain exact items in order
    ...    @{items}
    ...    locator=xpath://*[@id="filtersForm-filters"]//button[contains(., "show options")]

user checks filter contains exact items in order
    [Arguments]    @{items}    ${filter_label}
    user checks items matching locator contain exact items in order    @{items}
    ...    locator=xpath://*[@id="filtersForm-filters"]//fieldset[legend[contains(., "${filter_label}")]]//*[contains(@id,"-options-")]//legend

user checks filter group contains exact items in order
    [Arguments]    @{items}    ${filter_label}    ${group_label}
    user checks items matching locator contain exact items in order    @{items}
    ...    locator=xpath://*[@id="filtersForm-filters"]//fieldset[legend[contains(., "${filter_label}")]]//*[contains(@id,"-options-")]//legend[text()="${group_label}"]/..//label

user checks subheaded indicator checkbox is checked
    [Arguments]    ${subheading_label}    ${indicator_label}
    user checks checkbox input is checked
    ...    xpath://*[@id="filtersForm-indicators"]//legend[text()="${subheading_label}"]/..//label[text()="${indicator_label}"]/../input[@type="checkbox"]

user clicks category checkbox
    [Arguments]    ${subheading_label}    ${category_label}
    user clicks element
    ...    xpath://legend[text()="${subheading_label}"]/..//label[text()="${category_label}"]/../input[@type="checkbox"]

user checks category checkbox is checked
    [Arguments]    ${subheading_label}    ${category_label}
    user checks checkbox input is checked
    ...    xpath://legend[text()="${subheading_label}"]/..//label[text()="${category_label}"]/../input[@type="checkbox"]

user clicks select all for category
    [Arguments]    ${category_label}
    user clicks button containing text    Select all    xpath://legend[text()="${category_label}"]/..

user clicks unselect all for category
    [Arguments]    ${category_label}
    user clicks button containing text    Unselect all    xpath://legend[text()="${category_label}"]/..

user checks location checkbox is checked
    [Arguments]    ${location_label}
    user checks checkbox input is checked
    ...    xpath://*[@id="locationFiltersForm"]//label[contains(text(), "${location_label}")]/../input[@type="checkbox"]

user checks previous table tool step contains
    [Arguments]    ${step}    ${key}    ${value}    ${wait}=${timeout}
    wait until page contains element    id:tableToolWizard-step-${step}
    ...    timeout=${wait}
    ...    error=Previous step wasn't found!
    wait until page contains element
    ...    xpath://*[@id="tableToolWizard-step-${step}"]//dt[text()="${key}"]/..//*[text()="${value}"]
    ...    timeout=${wait}
    ...    error=Element "#tableToolWizard-step-${step}" containing "${key}" and "${value}" not found!

user creates data block
    [Arguments]
    ...    ${subject_name}
    ...    ${time_period_start}
    ...    ${time_period_end}
    ...    ${locations}
    ...    ${indicators}
    ...    ${filter_items}
    ...    ${data_block_name}
    ...    ${table_title}=${EMPTY}
    ...    ${table_source}=${EMPTY}

    # Start creating a data block.
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks

    user clicks link    Create data block
    user waits until table tool wizard step is available    1    Select a data set

    # Select subject.
    user waits until page contains    ${subject_name}    %{WAIT_SMALL}
    user clicks radio    ${subject_name}
    user clicks element    id:publicationDataStepForm-submit
    user waits until table tool wizard step is available    2    Choose locations    %{WAIT_MEDIUM}

    # Select locations.
    @{location_details_sections}=    Get WebElements    //*[starts-with(@data-testid, 'Expand Details Section')]
    FOR    ${location_details_section}    IN    @{location_details_sections}
        user clicks element    ${location_details_section}
    END

    FOR    ${location}    IN    @{locations}
        IF    "ALL" in "${location}"
            ${location_group}=    Get Substring    ${location}    4
            user clicks element    id:locationFiltersForm-locations-${location_group}-all
        ELSE
            user clicks checkbox    ${location}
        END
    END

    user clicks element    id:locationFiltersForm-submit
    user waits until table tool wizard step is available    3    Choose time period    %{WAIT_MEDIUM}

    # Select time periods.
    user waits until page contains element    id:timePeriodForm-start
    ${timePeriodStartList}=    get list items    id:timePeriodForm-start
    ${timePeriodEndList}=    get list items    id:timePeriodForm-end

    user chooses select option    id:timePeriodForm-start    ${time_period_start}
    user chooses select option    id:timePeriodForm-end    ${time_period_end}
    user clicks element    id:timePeriodForm-submit
    user waits until table tool wizard step is available    4    Choose your filters

    # Select indicators.
    FOR    ${indicator}    IN    @{indicators}
        user clicks checkbox    ${indicator}
    END

    # Select filter items.
    @{filter_item_details_sections}=    Get WebElements    //*[starts-with(@data-testid, 'Expand Details Section')]
    FOR    ${filter_item_details_section}    IN    @{filter_item_details_sections}
        user clicks element    ${filter_item_details_section}
    END

    # Submit the table tool query.
    user clicks element    id:filtersForm-submit
    user waits until results table appears    %{WAIT_LONG}

    # Save data block.
    user enters text into element    label:Data block name    ${data_block_name}

    IF    "${table_title}" != "${EMPTY}"
        user enters text into element    label:Table title    ${table_title}
    END

    IF    "${table_source}" != "${EMPTY}"
        user enters text into element    label:Data source    ${table_source}
    END

    user clicks button    Save data block
    user waits until page contains    Delete this data block
