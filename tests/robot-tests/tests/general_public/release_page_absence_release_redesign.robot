*** Settings ***
Resource            ../libs/public-common.robot
Resource            ../libs/charts.robot
Resource            ../seed_data/seed_data_theme_1_constants.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags
...                 GeneralPublic
...                 Local
...                 Dev
...                 Preprod
...                 ReleaseRedesign
#TODO: remove ReleaseRedesign tag when EES-6843 is complete


*** Variables ***
${DOWNLOAD_DIR}     ${EXECDIR}${/}test-results${/}downloads


*** Test Cases ***
Navigate to Absence publication
    environment variable should be set    PUBLIC_URL
    user navigates to the Absence publication

Validate title
    user waits until h1 is visible    ${PUPIL_ABSENCE_PUBLICATION_TITLE}    %{WAIT_MEDIUM}
    user waits until page contains title caption    Academic year 2016/17

#
# Release home tab
#

Validate Email alerts link
    user checks page contains link with text and url    Get email alerts
    ...    /subscriptions/new-subscription/${PUPIL_ABSENCE_PUBLICATION_SLUG}

Validate summary list items
    user checks element should contain    testid:Next release    March 2019
    user checks summary list contains    Release type    Official statistics
    user checks summary list contains    Produced by    Department for Education
    user checks summary list contains    Published    26 March 2020
    user checks summary list contains    Last updated    9 March 2022
    user checks element contains    testid:release-summary-block    3 updates
    check 'On this page section' for this tab contains
    ...    Skip in page navigation
    ...    Background information
    ...    Headline facts and figures
    ...    About these statistics
    ...    Pupil absence rates
    ...    Persistent absence
    ...    Reasons for absence
    ...    Distribution of absence
    ...    Absence by pupil characteristics
    ...    Absence for 4-year-olds
    ...    Pupil referral unit absence
    ...    Regional and local authority (LA) breakdown
    ...    Contact us
    ...    Back to top

Validate "Releases in this series" page
    user checks page contains link with text and url    All releases in this series
    ...    /find-statistics/${PUPIL_ABSENCE_PUBLICATION_SLUG}/releases
    user clicks link    All releases in this series
    user waits until page finishes loading

    user checks element contains    testid:next-release-date    March 2019
    user checks page contains link with text and url    Release home (latest release)
    ...    ${PUPIL_ABSENCE_PUBLICATION_RELATIVE_URL}

    user checks table body has x rows    7    testid:release-updates-table
    user checks table cell contains    1    1    Academic year 2016/17    testid:release-updates-table
    user checks table cell contains    1    2    26 March 2020    testid:release-updates-table
    user checks table cell contains    1    3    9 March 2022    testid:release-updates-table

    user checks table cell contains    2    1    Academic year 2014/15    testid:release-updates-table
    user checks table cell contains    2    2    Not available    testid:release-updates-table
    user checks table cell contains    2    3    Not available    testid:release-updates-table

    user checks table cell contains    3    1    Academic year 2013/14    testid:release-updates-table
    user checks table cell contains    3    2    Not available    testid:release-updates-table
    user checks table cell contains    3    3    Not available    testid:release-updates-table

    user checks table cell contains    4    1    Academic year 2012/13    testid:release-updates-table
    user checks table cell contains    4    2    Not available    testid:release-updates-table
    user checks table cell contains    4    3    Not available    testid:release-updates-table

    user checks table cell contains    5    1    Academic year 2011/12    testid:release-updates-table
    user checks table cell contains    5    2    Not available    testid:release-updates-table
    user checks table cell contains    5    3    Not available    testid:release-updates-table

    user checks table cell contains    6    1    Academic year 2010/11    testid:release-updates-table
    user checks table cell contains    6    2    Not available    testid:release-updates-table
    user checks table cell contains    6    3    Not available    testid:release-updates-table

    user checks table cell contains    7    1    Academic year 2009/10    testid:release-updates-table
    user checks table cell contains    7    2    Not available    testid:release-updates-table
    user checks table cell contains    7    3    Not available    testid:release-updates-table

    user clicks link    Release home (latest release)
    user waits until page finishes loading

Validate Release Home tab - Background information
    user waits until h2 is visible    Background information
    user checks section with ID contains elements and back to top link    background-information
    ...    Read national statistical summaries, view charts and tables and download data files.

Validate Release Home tab - Headline facts and figures
    user scrolls to element    xpath://h2[contains(text(), "Headline facts and figures")]
    user checks key stat contents    1    Overall absence rate    4.7%    Up from 4.6% in 2015/16    %{WAIT_MEDIUM}
    user checks key stat guidance    1    What is overall absence?
    ...    Total number of all authorised and unauthorised absences from possible school sessions for all pupils.

    user checks key stat contents    2    Authorised absence rate    3.4%    Similar to previous years
    user checks key stat guidance    2    What is authorized absence rate?
    ...    Number of authorised absences as a percentage of the overall school population.

    user checks key stat contents    3    Unauthorised absence rate    1.3%    Up from 1.1% in 2015/16
    user checks key stat guidance    3    What is unauthorized absence rate?
    ...    Number of unauthorised absences as a percentage of the overall school population.

Validate Headlines facts and figures -- Summary tab content
    [Documentation]    EES-718    Failing due to https://dfedigital.atlassian.net/browse/EES-4269
    ...    # TODO: Test charts more thoroughly in https://dfedigital.atlassian.net/browse/EES-6851
    [Tags]    NotAgainstPreProd    Failing
    user checks headline summary contains    pupils missed on average 8.2 school days
    user checks headline summary contains    overall and unauthorised absence rates up on 2015/16
    user checks headline summary contains    unauthorised absence rise due to higher rates of unauthorised holidays
    user checks headline summary contains    10% of pupils persistently absent during 2016/17

Validate Headlines facts and figures -- Charts tab
    open headline chart
    chart legend should be
    ...    Authorised absence rate (England)
    ...    Overall absence rate (England)
    ...    Unauthorised absence rate (England)

    chart X axis should be
    ...    2012/13
    ...    2013/14
    ...    2014/15
    ...    2015/16
    ...    2016/17

    chart Y axis should be
    ...    0%
    ...    2%
    ...    4%
    ...    6%

Validate Headlines facts and figures -- Data tables tab
    user scrolls to element    xpath://h2[contains(text(), "Headline facts and figures")]
    user clicks element    id:releaseHeadlines-tables-tab
    user waits until element contains    css:[data-testid="dataTableCaption"]
    ...    'Absence by characteristic' in England between 2012/13 and 2016/17    %{WAIT_SMALL}

    user checks table column heading contains    1    1    2012/13    css:#releaseHeadlines-tables table
    user checks table column heading contains    1    2    2013/14    css:#releaseHeadlines-tables table
    user checks table column heading contains    1    3    2014/15    css:#releaseHeadlines-tables table
    user checks table column heading contains    1    4    2015/16    css:#releaseHeadlines-tables table
    user checks table column heading contains    1    5    2016/17    css:#releaseHeadlines-tables table

    ${row}=    set variable    xpath://tbody/tr[th[normalize-space() = 'Authorised absence rate']]
    user checks row contains heading    ${row}    Authorised absence rate
    user checks row cell contains text    ${row}    1    4.2
    user checks row cell contains text    ${row}    2    3.5
    user checks row cell contains text    ${row}    3    3.5
    user checks row cell contains text    ${row}    4    3.4
    user checks row cell contains text    ${row}    5    3.4

    ${row}=    set variable    xpath://tbody/tr[th[normalize-space() = 'Unauthorised absence rate']]
    user checks row contains heading    ${row}    Unauthorised absence rate
    user checks row cell contains text    ${row}    1    1.1
    user checks row cell contains text    ${row}    2    1.1
    user checks row cell contains text    ${row}    3    1.1
    user checks row cell contains text    ${row}    4    1.1
    user checks row cell contains text    ${row}    5    1.3

    ${row}=    set variable    xpath://tbody/tr[th[normalize-space() = 'Overall absence rate']]
    user checks row contains heading    ${row}    Overall absence rate
    user checks row cell contains text    ${row}    1    5.3
    user checks row cell contains text    ${row}    2    4.5
    user checks row cell contains text    ${row}    3    4.6
    user checks row cell contains text    ${row}    4    4.6
    user checks row cell contains text    ${row}    5    4.7

    user checks section with ID contains elements and back to top link    releaseHeadlines-tables
    ...    Show full screen table
    ...    Data symbols
    ...    back_to_top=False

Validate content sections order
    user waits until parent contains element    testid:home-content    css:[data-testid="home-content-section"]
    ...    count=9
    user checks section is in position    About these statistics    1    testid:home-content
    user checks section is in position    Pupil absence rates    2    testid:home-content
    user checks section is in position    Persistent absence    3    testid:home-content
    user checks section is in position    Reasons for absence    4    testid:home-content
    user checks section is in position    Distribution of absence    5    testid:home-content
    user checks section is in position    Absence by pupil characteristics    6    testid:home-content
    user checks section is in position    Absence for 4-year-olds    7    testid:home-content
    user checks section is in position    Pupil referral unit absence    8    testid:home-content
    user checks section is in position    Regional and local authority (LA) breakdown    9    testid:home-content

Validate content section basic content
    user checks section with ID contains elements and back to top link    section-about-these-statistics
    ...    About these statistics
    ...    The statistics and data cover the absence of pupils of compulsory school age during the 2016/17 academic year
    user checks section with ID contains elements and back to top link    section-pupil-absence-rates
    ...    Pupil absence rates
    ...    The overall absence rate has increased across state-funded primary, secondary and special schools between 2015/16 and 2016/17.
    user checks section with ID contains elements and back to top link    section-persistent-absence
    ...    Persistent absence
    ...    The overall absence rate for persistent absentees across all schools increased to 18.1% - nearly 4 times higher than the rate for all pupils.
    user checks section with ID contains elements and back to top link    section-reasons-for-absence
    ...    Reasons for absence
    ...    Illness is the main driver behind overall absence and accounted for 55.3% of all absence - down from 57.3% in 2015/16 and 60.1% in 2014/15.
    user checks section with ID contains elements and back to top link    section-distribution-of-absence
    ...    Distribution of absence
    ...    Nearly half of all pupils (48.9%) were absent for 5 days or less across primary, secondary and special schools - down from 49.1% in 2015/16.
    user checks section with ID contains elements and back to top link    section-absence-by-pupil-characteristics
    ...    Absence by pupil characteristics
    ...    The overall absence and persistent absence patterns for pupils with different characteristics have been consistent over recent years.
    user checks section with ID contains elements and back to top link    section-absence-for-4-year-olds
    ...    Absence for 4-year-olds
    ...    The overall absence rate decreased to 5.1% - down from 5.2% for the previous two years.
    user checks section with ID contains elements and back to top link    section-pupil-referral-unit-absence
    ...    Pupil referral unit absence
    ...    The overall absence rate increased to 33.9% - up from 32.6% in 2015/16.
    user checks section with ID contains elements and back to top link
    ...    section-regional-and-local-authority-la-breakdown    Regional and local authority (LA) breakdown
    ...    Overall absence and persistent absence rates vary across primary, secondary and special schools by region and local authority (LA).
    # TODO: Test that the chart in section-regional-and-local-authority-la-breakdown displays correctly (in https://dfedigital.atlassian.net/browse/EES-6851)

Verify contact us and footer sections
    check the contact us section has expected details

    user checks page contains link with text and url    Cookies
    ...    /cookies
    user checks page contains link with text and url    Privacy notice
    ...    https://www.gov.uk/government/organisations/department-for-education/about/personal-information-charter
    user checks page contains link with text and url    Contact us
    ...    /contact-us
    user checks page contains link with text and url    Accessibility statement
    ...    /accessibility-statement
    user checks page contains link with text and url    Glossary
    ...    /glossary
    user checks page contains link with text and url    Help and support
    ...    /help-support
    user checks page contains link with text and url    Department for Education
    ...    https://www.gov.uk/government/organisations/department-for-education
    user checks page contains link with text and url    Office for Statistics Regulation
    ...    https://osr.statisticsauthority.gov.uk/what-we-do/
    user checks page contains link with text and url    Open Government Licence v3.0
    ...    https://www.nationalarchives.gov.uk/doc/open-government-licence/version/3/

Validate Regional and local authority (LA) breakdown table
    [Documentation]    BAU-540    Failing due to https://dfedigital.atlassian.net/browse/EES-4269
    ...    # TODO: Test that the tables are as expected in https://dfedigital.atlassian.net/browse/EES-6851
    [Tags]    Failing
    user opens accordion section    Regional and local authority (LA) breakdown    id:content
    user waits until element contains    css:#content_9_datablock-tables [data-testid="dataTableCaption"]
    ...    'Absence by characteristic' from '${PUPIL_ABSENCE_PUBLICATION_TITLE}' in    %{WAIT_MEDIUM}

    user checks table column heading contains    1    1    2016/17    css:#content_9_datablock-tables table

    ${row}=    user gets row with group and indicator    Vale of White Horse    Authorised absence rate
    ...    css:#content_9_datablock-tables table
    user checks row contains heading    ${row}    Authorised absence rate
    user checks row cell contains text    ${row}    1    3.4%
    ${row}=    user gets row with group and indicator    Vale of White Horse    Overall absence rate
    ...    css:#content_9_datablock-tables table
    user checks row contains heading    ${row}    Overall absence rate
    user checks row cell contains text    ${row}    1    4.3%
    ${row}=    user gets row with group and indicator    Vale of White Horse    Unauthorised absence rate
    ...    css:#content_9_datablock-tables table
    user checks row contains heading    ${row}    Unauthorised absence rate
    user checks row cell contains text    ${row}    1    0.9%

    ${row}=    user gets row with group and indicator    Harlow    Authorised absence rate
    ...    css:#content_9_datablock-tables table
    user checks row contains heading    ${row}    Authorised absence rate
    user checks row cell contains text    ${row}    1    3.1%
    ${row}=    user gets row with group and indicator    Harlow    Overall absence rate
    ...    css:#content_9_datablock-tables table
    user checks row contains heading    ${row}    Overall absence rate
    user checks row cell contains text    ${row}    1    4.2%
    ${row}=    user gets row with group and indicator    Harlow    Unauthorised absence rate
    ...    css:#content_9_datablock-tables table
    user checks row contains heading    ${row}    Unauthorised absence rate
    user checks row cell contains text    ${row}    1    1.1%

    ${row}=    user gets row with group and indicator    Newham    Authorised absence rate
    ...    css:#content_9_datablock-tables table
    user checks row contains heading    ${row}    Authorised absence rate
    user checks row cell contains text    ${row}    1    2.7%
    ${row}=    user gets row with group and indicator    Newham    Overall absence rate
    ...    css:#content_9_datablock-tables table
    user checks row contains heading    ${row}    Overall absence rate
    user checks row cell contains text    ${row}    1    4.4%
    ${row}=    user gets row with group and indicator    Newham    Unauthorised absence rate
    ...    css:#content_9_datablock-tables table
    user checks row contains heading    ${row}    Unauthorised absence rate
    user checks row cell contains text    ${row}    1    1.7%

Check Regional and local authority (LA) breakdown table has footnotes
    [Documentation]    EES-718    Failing due to https://dfedigital.atlassian.net/browse/EES-4269
    ...    # TODO: Test that the footers are as expected in https://dfedigital.atlassian.net/browse/EES-6851
    [Tags]    Failing
    ${accordion}=    user opens accordion section    Regional and local authority (LA) breakdown    id:content
    user scrolls down    500
    user waits until page finishes loading
    ${data_block_table}=    user gets data block table from parent    LAD map    ${accordion}
    user checks list has x items    testid:footnotes    2    ${data_block_table}
    user checks list item contains    testid:footnotes    1
    ...    Absence rates are the number of absence sessions expressed as a percentage of the total number of possible sessions.
    ...    ${data_block_table}
    user checks list item contains    testid:footnotes    2
    ...    There may be discrepancies between totals and the sum of constituent parts as national and regional totals and totals across school types have been rounded to the nearest 5.
    ...    ${data_block_table}

    user checks list has x items    testid:footnotes    2    ${data_block_table}
    user clicks button    Show 1 more footnote    ${data_block_table}
    user checks list item contains    testid:footnotes    3
    ...    x - 1 or 2 enrolments, or a percentage based on 1 or 2 enrolments.
    ...    ${data_block_table}

Validate Regional and local authority (LA) breakdown chart
    [Documentation]    EES-718    Failing due to https://dfedigital.atlassian.net/browse/EES-4269 -
    ...    # TODO: Test charts more thoroughly in https://dfedigital.atlassian.net/browse/EES-6851
    [Tags]    Failing
    user opens accordion section    Regional and local authority (LA) breakdown    id:content
    user scrolls to accordion section    Regional and local authority (LA) breakdown    id:content

    ${datablock}=    set variable    css:[data-testid="Data block - Generic data block - LA"]
    user waits until element contains map chart    ${datablock}

    user chooses select option    ${datablock} select[name="selectedLocation"]    Vale of White Horse
    user waits until element does not contain chart tooltip    ${datablock}

    user mouses over selected map feature    ${datablock}
    user checks map tooltip label contains    ${datablock}    Vale of White Horse
    user checks map tooltip item contains    ${datablock}    Unauthorised absence rate: 0.9%

    user checks map chart indicator tile contains    ${datablock}    Unauthorised absence rate    0.9%

    user mouses over element    ${datablock} select[name="selectedLocation"]
    user chooses select option    ${datablock} select[name="selectedLocation"]    Harlow
    user waits until element does not contain chart tooltip    ${datablock}

    user mouses over selected map feature    ${datablock}
    user checks map tooltip label contains    ${datablock}    Harlow
    user checks map tooltip item contains    ${datablock}    Unauthorised absence rate: 1.1%

    user checks map chart indicator tile contains    ${datablock}    Unauthorised absence rate    1.1%

    user mouses over element    ${datablock} select[name="selectedLocation"]
    user chooses select option    ${datablock} select[name="selectedLocation"]    Newham
    user waits until element does not contain chart tooltip    ${datablock}

    user mouses over selected map feature    ${datablock}
    user checks chart tooltip label contains    ${datablock}    Newham
    user checks chart tooltip item contains    ${datablock}    Unauthorised absence rate: 1.7%

    user checks map chart indicator tile contains    ${datablock}    Unauthorised absence rate    1.7%

Check Regional and local authority (LA) breakdown chart has footnotes
    [Documentation]    EES-718    Failing due to https://dfedigital.atlassian.net/browse/EES-4269
    ...    # TODO: Test charts more thoroughly in https://dfedigital.atlassian.net/browse/EES-6851
    [Tags]    Failing
    ${accordion}=    user opens accordion section    Regional and local authority (LA) breakdown    id:content
    ${data_block_chart}=    user gets data block chart from parent    LAD map    ${accordion}

    user checks list has x items    testid:footnotes    2    ${data_block_chart}
    user checks list item contains    testid:footnotes    1
    ...    Absence rates are the number of absence sessions expressed as a percentage of the total number of possible sessions.
    ...    ${data_block_chart}
    user checks list item contains    testid:footnotes    2
    ...    There may be discrepancies between totals and the sum of constituent parts as national and regional totals and totals across school types have been rounded to the nearest 5.
    ...    ${data_block_chart}

    user clicks button    Show 2 more footnotes    ${data_block_chart}
    user checks list has x items    testid:footnotes    4    ${data_block_chart}
    user checks list item contains    testid:footnotes    3
    ...    x - 1 or 2 enrolments, or a percentage based on 1 or 2 enrolments.
    ...    ${data_block_chart}
    user checks list item contains    testid:footnotes    4
    ...    This map uses the boundary data Local Authority Districts (December 2021) UK BUC
    ...    ${data_block_chart}

#
# Explore and download data tab
#

Verify Explore and Download data
    user clicks link    Explore and download data
    user waits until h2 is visible    Explore data used in this release
    user waits until h2 is visible    Data sets: download or create tables
    check 'On this page section' for this tab contains
    ...    Skip in page navigation
    ...    Explore data used in this release
    ...    Data sets: download or create tables
    ...    Data guidance
    ...    Contact us
    ...    Back to top
    check main links for page are persistent
    ...    Download all data from this release (ZIP)
    ...    Data sets: download or create tables
    ...    Data guidance
    ...    Data catalogue

    user clicks link    Download all data from this release (ZIP)
    Wait Until Keyword Succeeds    10    1    File Should Exist
    ...    ${DOWNLOAD_DIR}/seed-publication-pupil-absence-in-schools-in-england_2016-17.zip

    ${file_size}=    Get File Size    ${DOWNLOAD_DIR}/seed-publication-pupil-absence-in-schools-in-england_2016-17.zip
    Should Be True    ${file_size} > 0

    User checks data set list item properties    Absence by characteristic    201,625    2012/13 to 2016/17
    User checks data set list item properties    Absence in PRUs    612    2013/14 to 2016/17
    user checks section with ID contains elements and back to top link    data-guidance-section
    ...    Test data guidance content
    check the contact us section has expected details

#
# Methodology tab
#

Verify Methodology tab
    user clicks link    Methodology
    user waits until h2 is visible    Methodology
    check 'On this page section' for this tab contains
    ...    Skip in page navigation
    ...    Methodology
    ...    Contact us
    ...    Back to top
    user checks page contains link with text and url    Seed methodology - Pupil absence statistics: methodology
    ...    /methodology/seed-methodology-pupil-absence-statistics-methodology
    user checks section with ID contains elements and back to top link    methodology-section
    ...    Find out how and why we collect, process and publish these statistics.
    check the contact us section has expected details

#
# Help and related information tab
#

Verify Help and related information tab
    user clicks link    Help and related information
    user waits until h2 is visible    Get help by contacting us
    check 'On this page section' for this tab contains
    ...    Get help by contacting us
    ...    Official statistics
    ...    Back to top
    check the contact us section has expected details
    user checks page contains link with text and url
    ...    Standards for official statistics published by DfE guidance (opens in new tab)
    ...    https://www.gov.uk/government/publications/standards-for-official-statistics-published-by-the-department-for-education
    user checks page contains link with text and url    Code of Practice for Statistics (opens in new tab)
    ...    https://code.statisticsauthority.gov.uk/the-code/
    user checks page contains link with text and url    OSR website (opens in new tab)
    ...    https://osr.statisticsauthority.gov.uk/


*** Keywords ***
user navigates to the Absence publication
    user navigates to    %{PUBLIC_URL}${PUPIL_ABSENCE_PUBLICATION_RELATIVE_URL}
    user waits until h1 is visible    ${PUPIL_ABSENCE_PUBLICATION_TITLE}    %{WAIT_MEDIUM}

user checks section is in position
    [Arguments]    ${section_text}    ${position}    ${parent}=css:[data-testid="accordion"]    ${exact_match}=${False}
    ${text_matcher}=    get xpath text matcher    ${section_text}    ${exact_match}
    user waits until parent contains element    ${parent}
    ...    xpath:(.//*[@data-testid="home-content-section"])[${position}]//h2[${text_matcher}]

User checks data set list item properties
    [Arguments]    ${data_set_name}    ${expected_row_count}    ${expected_time_period}

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
    ...    xpath=${dataset_xpath}//p[contains(normalize-space(.), "${data_set_name} data guidance content")]
    ...    Dataset "${data_set_name}" is missing the data guidance content link

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
    user checks previous table tool step contains    1    Publication    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user navigates to the Absence publication
    user clicks link    Explore and download data
    user waits until h2 is visible    Explore data used in this release
    user waits until h2 is visible    Data sets: download or create tables

check main links for page are persistent
    [Arguments]    @{expected_link_texts}
    FOR    ${link_text}    IN    @{expected_link_texts}
        ${button_xpath}=    Set Variable
        ...    //section[@data-testid="explore-section"]//ul[@data-testid="links-grid"]//a[text()="${link_text}"]
        Page Should Contain Element
        ...    xpath=${button_xpath}
        ...    Page is missing "${button_xpath}" button
    END

check the contact us section has expected details
    user checks section with ID contains elements and back to top link    contact-us-section
    ...    UI test team name
    ...    UI test contact name
    ...    ui_test@test.com
    ...    0123 4567
    ...    037 0000 2288
    ...    Monday to Friday from 9.30am to 5pm (excluding bank holidays)

check 'On this page section' for this tab contains
    [Arguments]    @{expected_link_texts}
    FOR    ${link_text}    IN    @{expected_link_texts}
        ${button_xpath}=    Set Variable
        ...    //h2[normalize-space(.)='On this page']/parent::div//a[text()="${link_text}"]
        Page Should Contain Element
        ...    xpath=${button_xpath}
        ...    Page is missing "${button_xpath}" button
    END
