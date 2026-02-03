*** Settings ***
Resource    ./common.robot
Library     public-utilities.py


*** Keywords ***
user checks headline summary contains
    [Arguments]    ${text}
    user waits until element is visible    xpath://*[@id="releaseHeadlines-summary"]//li[text()="${text}"]

user checks number of release updates
    [Arguments]    ${count}
    user waits until page contains element    id:releaseLastUpdates
    user waits until page contains element    css:#releaseLastUpdates li    limit=${count}

user checks release update
    [Arguments]    ${number}    ${date}    ${text}
    user waits until element contains    css:#releaseLastUpdates li:nth-of-type(${number}) time    ${date}
    user waits until element contains    css:#releaseLastUpdates li:nth-of-type(${number}) p    ${text}

user waits until details dropdown contains publication
    [Arguments]    ${details_heading}    ${publication_name}    ${publication_type}=National and official statistics
    ${details}=    user gets details content element    ${details_heading}
    user checks publication appears under correct publication type heading
    ...    ${details}
    ...    ${publication_type}
    ...    ${publication_name}

user checks publication appears under correct publication type heading
    [Arguments]    ${parent}    ${publication_type}    ${publication_name}
    ${publication_type}=    get child element
    ...    ${parent}
    ...    xpath:.//div[@data-testid="publication-type" and descendant::h3[text()="${publication_type}"]]
    ${publications}=    get child element    ${publication_type}    css:ul
    user checks element should contain    ${publications}    ${publication_name}

user goes to release page via breadcrumb
    [Arguments]    ${publication}    ${release}
    user clicks link    ${publication}

    user checks breadcrumb count should be    3
    user checks nth breadcrumb contains    1    Home
    user checks nth breadcrumb contains    2    Find statistics and data
    user checks nth breadcrumb contains    3    ${publication}

    user waits until h1 is visible    ${publication}
    user waits until page contains title caption    ${release}

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

user checks section with ID contains elements and back to top link
    [Arguments]    ${section_id}    @{paragraph_texts}    ${back_to_top}=True
    ${section}=    Get WebElement    id:${section_id}
    FOR    ${paragraph_text}    IN    @{paragraph_texts}
        user checks element should contain    ${section}    ${paragraph_text}
    END
    IF    ${back_to_top}
        user waits until parent contains element    ${section}    xpath://a[text()="Back to top" and @href="\#top"]
    END

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

user checks main links for page 'Explore and download data' are persistent
    [Arguments]    @{expected_link_texts}
    FOR    ${link_text}    IN    @{expected_link_texts}
        ${button_xpath}=    Set Variable
        ...    //section[@data-testid="explore-section"]//ul[@data-testid="links-grid"]//a[text()="${link_text}"]
        Page Should Contain Element
        ...    xpath=${button_xpath}
        ...    Page is missing "${button_xpath}" button
    END

User checks page 'Explore and download data' data set available properties
    [Arguments]    ${data_set_name}
    ...    ${expected_row_count}
    ...    ${expected_time_period}
    ...    ${PUBLICATION_TITLE}
    ...    ${expected_data_guidance}="${data_set_name} data guidance content"

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

    # Assert dd value for "Number of rows"
    Page Should Contain Element
    ...    xpath=${dataset_xpath}
    ...    //dt[normalize-space(.)="Number of rows"]/following-sibling::dd[normalize-space(.)="${expected_row_count}"]
    ...    Dataset "${data_set_name}" has incorrect Number of rows

    # Assert dd value for "Time period"
    Page Should Contain Element
    ...    xpath=${dataset_xpath}
    ...    //dt[normalize-space(.)="Time period"]/following-sibling::dd[normalize-space(.)="${expected_time_period}"]
    ...    Dataset "${data_set_name}" has incorrect Time period

    # Verify data guidance content
    Page Should Contain Element
    ...    xpath=${dataset_xpath}//p[contains(normalize-space(.), "${expected_data_guidance}")]
    ...    Dataset "${data_set_name}" is missing the data guidance content text

    # Verify Data set information page link
    Page Should Contain Element
    ...    xpath=${dataset_xpath}//a[contains(normalize-space(.), "Data set information page")]
    ...    Dataset "${data_set_name}" is missing the "Data set information page" link

    # Verify Create table link
    Page Should Contain Element
    ...    xpath=${dataset_xpath}//a[contains(normalize-space(.), "Create table")]
    ...    Dataset "${data_set_name}" is missing the "Create table" link

    # Verify Download (ZIP) button
    Page Should Contain Element
    ...    xpath=${dataset_xpath}//button[contains(normalize-space(.), "Download")]
    ...    Dataset "${data_set_name}" is missing the "Download (ZIP)" button

    user clicks element    xpath=${dataset_xpath}//a[contains(normalize-space(.), "Create table")]
    user waits until h1 is visible    Create your own tables    %{WAIT_MEDIUM}
    user waits until page finishes loading

    user waits until table tool wizard step is available    2    Select a data set
    user checks previous table tool step contains    1    Publication    ${PUBLICATION_TITLE}

user checks 'On this page section' for this tab contains
    [Arguments]    @{expected_link_texts}
    FOR    ${link_text}    IN    @{expected_link_texts}
        ${button_xpath}=    Set Variable
        ...    //h2[normalize-space(.)='On this page']/parent::div//a[text()="${link_text}"]
        Page Should Contain Element
        ...    xpath=${button_xpath}
        ...    Page is missing "${button_xpath}" button
    END
