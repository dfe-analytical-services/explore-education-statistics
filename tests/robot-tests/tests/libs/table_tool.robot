*** Settings ***
Resource    ./common.robot

*** Keywords ***
user waits until results table appears
    [Arguments]    ${wait_time}
    user waits until page contains element    css:table thead th    ${wait_time}
    user waits until page does not contain loading spinner

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

user checks indicator groups match exactly in order
    [Arguments]    @{items}
    ${list}=    Get WebElements
    ...    xpath://*[@id="filtersForm-indicators"]//*[starts-with(@id,"filtersForm-indicators-options-")]//legend
    ${num_items}=    Get Length    ${items}
    length should be    ${list}    ${num_items}
    FOR    ${index}    ${content}    IN ENUMERATE    @{items}
        user checks element should contain    ${list}[${index}]    ${content}
    END

user checks indicator group contains exactly items in order
    [Arguments]    @{items}    ${group_label}
    ${list}=    Get WebElements    xpath://*[@id="filtersForm-indicators"]//legend[text()="${group_label}"]/..//label
    ${num_items}=    Get Length    ${items}
    length should be    ${list}    ${num_items}
    FOR    ${index}    ${content}    IN ENUMERATE    @{items}
        user checks element should contain    ${list}[${index}]    ${content}
    END

user checks filters match exactly in order
    [Arguments]    @{items}
    ${list}=    Get WebElements    xpath://*[@id="filtersForm-filters"]//details
    ${num_items}=    Get Length    ${items}
    length should be    ${list}    ${num_items}
    FOR    ${index}    ${content}    IN ENUMERATE    @{items}
        user checks element should contain    ${list}[${index}]    ${content}
    END

user checks filter contains exactly filter groups in order
    [Arguments]    @{items}    ${filter_label}
    ${list}=    Get WebElements
    ...    xpath://*[@id="filtersForm-filters"]//details[summary[contains(., "${filter_label}")]]//*[contains(@id,"-options-")]//legend
    ${num_items}=    Get Length    ${items}
    length should be    ${list}    ${num_items}
    FOR    ${index}    ${content}    IN ENUMERATE    @{items}
        user checks element should contain    ${list}[${index}]    ${content}
    END

user checks filter group contains exactly items in order
    [Arguments]    @{items}    ${filter_label}    ${group_label}
    ${list}=    Get WebElements
    ...    xpath://*[@id="filtersForm-filters"]//details[summary[contains(., "${filter_label}")]]//*[contains(@id,"-options-")]//legend[text()="${group_label}"]/..//label
    ${num_items}=    Get Length    ${items}
    length should be    ${list}    ${num_items}
    FOR    ${index}    ${content}    IN ENUMERATE    @{items}
        user checks element should contain    ${list}[${index}]    ${content}
    END

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
    user clicks element    xpath://legend[text()="${category_label}"]/..//button[contains(text(), "Select")]

user clicks unselect all for category
    [Arguments]    ${category_label}
    user clicks element    xpath://legend[text()="${category_label}"]/..//button[contains(text(), "Unselect")]

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
