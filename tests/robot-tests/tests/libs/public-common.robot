*** Settings ***
Resource    ./common.robot
Library     public-utilities.py


*** Keywords ***
user navigates to public release page
    [Arguments]    ${public_release_url}    ${publication_name}    ${release_name}=${EMPTY}
    user navigates to    ${public_release_url}
    user waits until h1 is visible    ${publication_name}
    IF    "${release_name}" != ""
        user waits until page contains title caption    ${release_name}
    END

user navigates to public find statistics page
    environment variable should be set    PUBLIC_URL
    user navigates to    %{PUBLIC_URL}/find-statistics
    user waits until h1 is visible    Find statistics and data

user navigates to data tables page on public frontend
    environment variable should be set    PUBLIC_URL
    user navigates to    %{PUBLIC_URL}/data-tables
    user waits until h1 is visible    Create your own tables

user navigates to data catalogue page on public frontend
    environment variable should be set    PUBLIC_URL
    user navigates to    %{PUBLIC_URL}/data-catalogue
    user waits until h1 is visible    Data catalogue
    user waits until page contains    Find and download data sets with associated guidance files.

user navigates to public methodologies page
    environment variable should be set    PUBLIC_URL
    user navigates to    %{PUBLIC_URL}/methodology
    user waits until h1 is visible    Methodologies
    user waits until page contains    Browse to find out about the methodology behind specific

user checks methodology note
    [Arguments]    ${number}    ${displayDate}    ${content}
    user waits until element contains    css:#methodologyNotes li:nth-of-type(${number}) time    ${displayDate}
    user waits until element contains    css:#methodologyNotes li:nth-of-type(${number}) p    ${content}

user checks related information panel is visible
    user waits until page contains element    xpath://h2[text()='Related information']

user goes to explore and download data and navigates to data set details page
    [Arguments]    ${SUBJECT_NAME}
    user clicks link    Explore and download data
    user waits until h2 is visible    Explore data used in this release
    user opens data set details for subject    ${SUBJECT_NAME}

    Page Should Contain Link    Data set information page
    user clicks link containing text    Data set information page
    user waits until h1 is visible    ${SUBJECT_NAME}

user opens data set details for subject
    [Arguments]    ${SUBJECT_NAME}
    ${dataset_xpath}=    Set Variable
    ...    //article//li[@data-testid="release-data-list-item"][.//h4[normalize-space()="${SUBJECT_NAME}"]]

    # Wait for dataset to exist
    Wait Until Element Is Visible    xpath=${dataset_xpath}

    # Expand accordion if collapsed
    ${toggle_xpath}=    Set Variable
    ...    ${dataset_xpath}//button[@aria-expanded="false"]

    Run Keyword And Ignore Error
    ...    Click Element    xpath=${toggle_xpath}

user checks main links for page 'Explore and download data' are present
    [Arguments]    @{expected_card_link_texts}
    ${explore_data_used_in_this_release_section_xpath}=    Set Variable
    ...    //section[@data-testid="explore-section"]

    # Verify the 'Download all data (ZIP)' link is present
    ${download_all_data_zip_link_xpath}=    Set Variable
    ...    ${explore_data_used_in_this_release_section_xpath}//a[text()="Download all data (ZIP)"]
    Page Should Contain Element
    ...    xpath=${download_all_data_zip_link_xpath}
    ...    Page is missing "${download_all_data_zip_link_xpath}" link

    # Verify that all expected links are present in the 'More options' cards grid
    FOR    ${link_text}    IN    @{expected_card_link_texts}
        ${link_xpath}=    Set Variable
        ...    ${explore_data_used_in_this_release_section_xpath}//ul[@data-testid="links-grid"]//a[text()="${link_text}"]
        Page Should Contain Element
        ...    xpath=${link_xpath}
        ...    Page is missing "${link_xpath}" link
    END

User checks page 'Explore and download data' data set available properties
    [Arguments]    ${data_set_name}
    ...    ${geographical_levels}
    ...    ${expected_row_count}
    ...    ${expected_time_period}
    ...    ${publication_title}
    ...    ${expected_data_guidance}=${data_set_name} data guidance content
    ...    ${indicators}=${EMPTY}
    ...    ${filters}=${EMPTY}
    ...    ${api_data_set_id}=${EMPTY}
    ...    ${is_public_site}=True

    ${dataset_xpath}=    Set Variable
    ...    //article//li[@data-testid="release-data-list-item"][.//h4[normalize-space()="${data_set_name}"]]

    # Wait for dataset to exist
    Wait Until Element Is Visible    xpath=${dataset_xpath}

    # Expand accordion if collapsed
    ${toggle_xpath}=    Set Variable
    ...    ${dataset_xpath}//button[@aria-expanded="false"]

    Run Keyword And Ignore Error
    ...    Click Element    xpath=${toggle_xpath}

    # Assert "Number of rows" dt exists
    Page Should Contain Element
    ...    xpath=${dataset_xpath}//dt[normalize-space(.)="Number of rows"]
    ...    Dataset "${data_set_name}" is missing "Number of rows" label

    # Assert geographic levels
    Page Should Contain Element
    ...    xpath=${dataset_xpath}//p[contains(normalize-space(.), "${geographical_levels}")]
    ...    Dataset "${data_set_name}" has incorrect geographical levels

    # Normalize the incoming parameters ($indicators or $filters) into a Python list so the rest of the Robot keyword can treat it uniformly.
    #    Examples:
    # Input "" or None → ${indicators} becomes [] (falsey).
    # Input ["A","B"] → ${indicators} stays ["A","B"] (truthy).
    # Input "X" → ${indicators} becomes ["X"] (truthy).
    ${indicators}=    Evaluate
    ...    [] if $indicators in ('', None) else $indicators if isinstance($indicators, (list, tuple)) else [$indicators]
    ${filters}=    Evaluate
    ...    [] if $filters in ('', None) else $filters if isinstance($filters, (list, tuple)) else [$filters]
    IF    ${indicators}
        # Assert indicators label exists
        Page Should Contain Element
        ...    xpath=${dataset_xpath}//dt[normalize-space(.)="Indicators"]
        ...    Dataset "${data_set_name}" is missing "Indicators" label

        # If indicators provided, assert each indicator exists in the list
        FOR    ${indicator}    IN    @{indicators}
            Page Should Contain Element
            ...    xpath=${dataset_xpath}//div[@data-testid="Indicators"]//li[normalize-space(.)="${indicator}"]
            ...    Dataset "${data_set_name}" is missing indicator "${indicator}"
        END
    END

    IF    ${filters}
        # Assert filters label exists
        Page Should Contain Element
        ...    xpath=${dataset_xpath}//dt[normalize-space(.)="Filters"]
        ...    Dataset "${data_set_name}" is missing "Filters" label

        # If filters provided, assert each filter exists in the list
        FOR    ${filter}    IN    @{filters}
            Page Should Contain Element
            ...    xpath=${dataset_xpath}//div[@data-testid="Filters"]//li[normalize-space(.)="${filter}"]
            ...    Dataset "${data_set_name}" is missing filter "${filter}"
        END
    END

    IF    "${api_data_set_id}" != "${EMPTY}"
        # Assert API ID label exists
        Page Should Contain Element
        ...    xpath=${dataset_xpath}//dt[normalize-space(.)="API data set ID"]
        ...    Dataset "${data_set_name}" is missing "API data set ID" label

        # Assert API ID value is correct
        Page Should Contain Element
        ...    xpath=${dataset_xpath}//dt[normalize-space(.)="API data set ID"]/following-sibling::dd[normalize-space(.)="${api_data_set_id}"]
        ...    Dataset "${data_set_name}" has incorrect API ID value

        Page Should Contain Element
        ...    xpath=${dataset_xpath}//strong[contains(normalize-space(.), "Available by API")]
        ...    Dataset "${data_set_name}" is missing "Available by API" tag
    END

    # Assert dd value for "Number of rows"
    Page Should Contain Element
    ...    xpath=${dataset_xpath}//dt[normalize-space(.)="Number of rows"]/following-sibling::dd[normalize-space(.)="${expected_row_count}"]
    ...    Dataset "${data_set_name}" has incorrect Number of rows

    # Assert dd value for "Time period"
    Page Should Contain Element
    ...    xpath=${dataset_xpath}//dt[normalize-space(.)="Time period"]/following-sibling::dd[normalize-space(.)="${expected_time_period}"]
    ...    Dataset "${data_set_name}" has incorrect Time period

    # Verify data guidance content
    Page Should Contain Element
    ...    xpath=${dataset_xpath}//p[contains(normalize-space(.), "${expected_data_guidance}")]
    ...    Dataset "${data_set_name}" is missing the data guidance content text

    # Verify Download (ZIP) button
    Page Should Contain Element
    ...    xpath=${dataset_xpath}//button[contains(normalize-space(.), "Download")]
    ...    Dataset "${data_set_name}" is missing the "Download (ZIP)" button

    IF    "${is_public_site}" == "True"
        # Verify Data set information page link
        Page Should Contain Element
        ...    xpath=${dataset_xpath}//a[contains(normalize-space(.), "Data set information page")]
        ...    Dataset "${data_set_name}" is missing the "Data set information page" link

        # Verify Create table link
        Page Should Contain Element
        ...    xpath=${dataset_xpath}//a[contains(normalize-space(.), "Create table")]
        ...    Dataset "${data_set_name}" is missing the "Create table" link

        user clicks element    xpath=${dataset_xpath}//a[contains(normalize-space(.), "Create table")]
        user waits until h1 is visible    Create your own tables    %{WAIT_MEDIUM}
        user waits until page finishes loading

        user waits until table tool wizard step is available    2    Select a data set
        user checks previous table tool step contains    1    Publication    ${publication_title}
    END

user checks 'On this page section' for this tab contains
    [Arguments]    @{expected_link_texts}
    FOR    ${link_text}    IN    @{expected_link_texts}
        ${button_xpath}=    Set Variable
        ...    //h2[normalize-space(.)='On this page']/parent::div//a[text()="${link_text}"]
        Page Should Contain Element
        ...    xpath=${button_xpath}
        ...    Page is missing "${button_xpath}" button
    END
