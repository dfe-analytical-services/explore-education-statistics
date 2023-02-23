*** Settings ***
Resource    ../common.robot
Resource    ../admin-common.robot
Library     ../admin_api.py


*** Variables ***
${SUBJECT_NAME}     UI test subject


*** Keywords ***
user creates a fully populated higher review release
    [Arguments]
    ...    ${PUBLICATION_ID}
    ...    ${PUBLICATION_NAME}
    ...    ${THEME_NAME}=%{TEST_THEME_NAME}
    ...    ${TOPIC_NAME}=%{TEST_TOPIC_NAME}
    ...    ${RELEASE_TIME_PERIOD}=AY
    ...    ${RELEASE_YEAR}=2023
    ...    ${RELEASE_TYPE}=Academic year 2023/24
    user creates a fully populated draft release
    ...    ${PUBLICATION_ID}
    ...    ${PUBLICATION_NAME}
    ...    ${THEME_NAME}
    ...    ${TOPIC_NAME}
    ...    ${RELEASE_TIME_PERIOD}
    ...    ${RELEASE_YEAR}
    ...    ${RELEASE_TYPE}
    user puts release into higher level review

user creates a fully populated approved release
    [Arguments]
    ...    ${PUBLICATION_ID}
    ...    ${PUBLICATION_NAME}
    ...    ${THEME_NAME}=%{TEST_THEME_NAME}
    ...    ${TOPIC_NAME}=%{TEST_TOPIC_NAME}
    ...    ${RELEASE_TIME_PERIOD}=AY
    ...    ${RELEASE_YEAR}=2024
    ...    ${RELEASE_TYPE}=Academic year 2024/25
    user creates a fully populated draft release
    ...    ${PUBLICATION_ID}
    ...    ${PUBLICATION_NAME}
    ...    ${THEME_NAME}
    ...    ${TOPIC_NAME}
    ...    ${RELEASE_TIME_PERIOD}
    ...    ${RELEASE_YEAR}
    ...    ${RELEASE_TYPE}
    ${days_until_release}=    set variable    10000
    ${publish_date_day}=    get current datetime    %-d    ${days_until_release}
    ${publish_date_month}=    get current datetime    %-m    ${days_until_release}
    ${publish_date_year}=    get current datetime    %Y    ${days_until_release}
    user approves release for scheduled publication
    ...    ${publish_date_day}
    ...    ${publish_date_month}
    ...    ${publish_date_year}

user creates a fully populated published release
    [Arguments]
    ...    ${PUBLICATION_ID}
    ...    ${PUBLICATION_NAME}
    ...    ${THEME_NAME}=%{TEST_THEME_NAME}
    ...    ${TOPIC_NAME}=%{TEST_TOPIC_NAME}
    ...    ${RELEASE_TIME_PERIOD}=AY
    ...    ${RELEASE_YEAR}=2025
    ...    ${RELEASE_TYPE}=Academic year 2025/26
    user creates a fully populated draft release
    ...    ${PUBLICATION_ID}
    ...    ${PUBLICATION_NAME}
    ...    ${THEME_NAME}
    ...    ${TOPIC_NAME}
    ...    ${RELEASE_TIME_PERIOD}
    ...    ${RELEASE_YEAR}
    ...    ${RELEASE_TYPE}
    user approves original release for immediate publication

user creates a fully populated draft release
    [Arguments]
    ...    ${PUBLICATION_ID}
    ...    ${PUBLICATION_NAME}
    ...    ${THEME_NAME}=%{TEST_THEME_NAME}
    ...    ${TOPIC_NAME}=%{TEST_TOPIC_NAME}
    ...    ${RELEASE_TIME_PERIOD}=AY
    ...    ${RELEASE_YEAR}=2022
    ...    ${RELEASE_TYPE}=Academic year 2022/23
    user creates test release via api    ${PUBLICATION_ID}    ${RELEASE_TIME_PERIOD}    ${RELEASE_YEAR}
    user navigates to draft release page from dashboard
    ...    ${PUBLICATION_NAME}
    ...    ${RELEASE_TYPE}
    ...    ${THEME_NAME}
    ...    ${TOPIC_NAME}

    # add data files
    user uploads subject    ${SUBJECT_NAME}    seven_filters.csv    seven_filters.meta.csv

    # add data guidance
    user clicks link    Data guidance
    user waits until page does not contain loading spinner
    user enters text into element    id:dataGuidanceForm-content    Test data guidance content
    user waits until page contains accordion section    ${SUBJECT_NAME}
    user enters text into data guidance data file content editor    ${SUBJECT_NAME}
    ...    data guidance content
    user clicks button    Save guidance

    # add footnote
    user waits for page to finish loading
    user clicks link    Footnotes
    user waits until h2 is visible    Footnotes
    user waits until page contains link    Create footnote
    user clicks link    Create footnote
    user waits until page does not contain loading spinner
    user clicks footnote subject radio    ${SUBJECT_NAME}    Applies to all data
    user clicks element    id:footnoteForm-content
    user enters text into element    id:footnoteForm-content    test footnote
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes    %{WAIT_SMALL}

    # add public prerelease access list
    user clicks link    Pre-release access
    user waits until page does not contain loading spinner
    user creates public prerelease access list    Test public access list
