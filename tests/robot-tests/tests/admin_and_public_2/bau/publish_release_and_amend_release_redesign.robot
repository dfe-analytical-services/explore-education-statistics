*** Settings ***
Library             DateTime
Library             String
Library             ../../libs/admin_api.py
Library             XML
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Resource            ../../libs/charts.robot
Resource            ../../libs/public-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData    ReleaseRedesign


*** Variables ***
${PUBLICATION_NAME}=            Publish release and amend %{RUN_IDENTIFIER}
${RELEASE_LABEL}=               provisional
${RELEASE_NAME}=                Financial year 3000-01 ${RELEASE_LABEL}
${DATABLOCK_NAME}=              Dates data block name
${SUBJECT_NAME}=                Dates test subject
${ANCILLARY_FILE_NAME}=         Test ancillary file 1
${ANCILLARY_FILE_NAME_2}=       Test ancillary file 2


*** Test Cases ***
Create new publication for "UI tests theme" theme
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    FY    3000    label=${RELEASE_LABEL}

Go to "Release summary" page
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}

Verify release summary
    user checks page contains element    xpath://li/a[text()="Summary" and contains(@aria-current, 'page')]
    user verifies release summary    Financial year
    ...    3000-01    Accredited official statistics    ${RELEASE_LABEL}

Upload subject
    user uploads subject and waits until complete    ${SUBJECT_NAME}    dates.csv    dates.meta.csv

Add data guidance
    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance
    user adds main data guidance content

    user waits until page contains element    id:dataGuidance-dataFiles
    user waits until page contains accordion section    ${SUBJECT_NAME}

    user checks summary list contains    Filename    dates.csv
    user checks summary list contains    Geographic levels    National
    user checks summary list contains    Time period    2020 Week 13 to 2021 Week 24

    user enters text into data guidance data file content editor    ${SUBJECT_NAME}
    ...    ${SUBJECT_NAME} test data guidance content
    user clicks button    Save guidance

Add ancillary file
    user clicks link    Supporting file uploads
    user waits until h2 is visible    Add file to release

    user enters text into element    id:ancillaryFileForm-title    ${ANCILLARY_FILE_NAME}
    user enters text into element    id:ancillaryFileForm-summary    ${ANCILLARY_FILE_NAME} summary
    user chooses file    id:ancillaryFileForm-file    ${FILES_DIR}test-file-1.txt
    user clicks button    Add file

    user waits until page contains accordion section    ${ANCILLARY_FILE_NAME}
    user opens accordion section    ${ANCILLARY_FILE_NAME}    id:file-uploads

    ${section_1}=    user gets accordion section content element    ${ANCILLARY_FILE_NAME}    id:file-uploads
    user checks summary list contains    Title    ${ANCILLARY_FILE_NAME}    ${section_1}
    user checks summary list contains    Summary    ${ANCILLARY_FILE_NAME} summary    ${section_1}
    user checks summary list contains    File    test-file-1.txt    ${section_1}
    user checks summary list contains    File size    12 B    ${section_1}

    user checks there are x accordion sections    1    id:file-uploads

Navigate to 'Footnotes' section
    user clicks link    Footnotes
    user waits until h2 is visible    Footnotes

Add a footnote
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote
    user clicks footnote subject radio    ${SUBJECT_NAME}    Applies to all data
    user clicks element    id:footnoteForm-content
    user enters text into element    id:footnoteForm-content
    ...    Applies to all data 1
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes

Add a second footnote
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote
    user clicks footnote subject radio    ${SUBJECT_NAME}    Applies to all data
    user clicks element    id:footnoteForm-content
    user enters text into element    id:footnoteForm-content
    ...    Applies to all data 2
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes

Confirm created footnotes
    user waits until page contains element    testid:Footnote - Applies to all data 1
    user waits until page contains element    testid:Footnote - Applies to all data 2

Create data block table
    user creates data block for dates csv    ${SUBJECT_NAME}    ${DATABLOCK_NAME}    Dates table title

Create chart for data block
    user waits until page contains link    Chart
    user waits until page finishes loading
    user clicks link    Chart

    user clicks button    Choose an infographic as alternative
    user chooses file    id:chartConfigurationForm-file    ${FILES_DIR}test-infographic.png
    user checks radio is checked    Use table title
    user waits until element is enabled    id:chartConfigurationForm-alt
    user enters text into element    id:chartConfigurationForm-alt    Sample alt text
    user saves infographic configuration

    user waits until page contains    Chart preview
    user checks infographic chart contains alt    id:chartBuilderPreview    Sample alt text

Navigate to 'Content' page
    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}
    user waits until page contains button    Add a summary text block    %{WAIT_SMALL}

Add free text key stat
    user adds free text key stat    Free text key stat title    9001%    Trend    Guidance title    Guidance text

    user checks element count is x    testid:keyStat    1
    user checks key stat contents    1    Free text key stat title    9001%    Trend
    user checks key stat guidance    1    Guidance title    Guidance text

Add three accordion sections to release
    user waits until page finishes loading
    user waits until page finishes loading
    user clicks button    Add new section
    user changes accordion section title    1    Dates data block
    user clicks button    Add new section
    user changes accordion section title    2    Test text
    user clicks button    Add new section
    user changes accordion section title    3    Test embedded dashboard section

Add data block to first accordion section
    user scrolls to accordion section    Dates data block    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user adds data block to editable accordion section    Dates data block    ${DATABLOCK_NAME}
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    ${datablock}=    set variable    xpath://*[@data-testid="Data block - ${DATABLOCK_NAME}"]

    user scrolls to element    ${datablock}
    user waits until page contains element    ${datablock}    %{WAIT_SMALL}
    user waits until element contains infographic chart    ${datablock}
    user checks chart title contains    ${datablock}    Dates table title
    user checks infographic chart contains alt    ${datablock}    Sample alt text
    user reloads page

Verify data block table has footnotes
    ${accordion}=    user opens accordion section    Dates data block    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user scrolls down    100
    user waits for caches to expire
    ${data_block_table}=    user gets data block table from parent    ${DATABLOCK_NAME}    ${accordion}

    user checks list has x items    testid:footnotes    2    ${data_block_table}
    user checks list item contains    testid:footnotes    1
    ...    Applies to all data 1
    ...    ${data_block_table}
    user checks list item contains    testid:footnotes    2
    ...    Applies to all data 2
    ...    ${data_block_table}

Add test text to second accordion section
    user scrolls to accordion section    Test text    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user opens accordion section    Test text    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user adds text block to editable accordion section    Test text    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user adds content to autosaving accordion section text block    Test text    1    Some test text!
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

Add embedded dashboard to third accordion section
    user chooses to embed a URL in editable accordion section
    ...    Test embedded dashboard section
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

    ${modal}=    user updates embedded URL details in modal
    ...    Test embedded dashboard title
    ...    https://google.com

    user clicks button    Save    ${modal}
    user waits until page contains    URL must be on a permitted domain

    user updates embedded URL details in modal
    ...    Test embedded dashboard title
    ...    https://dfe-analytical-services.github.io/analysts-guide

    user presses keys    TAB
    user waits until page does not contain    URL must be on a permitted domain
    user clicks button    Save    ${modal}
    user waits until modal is not visible    Embed a URL

    user waits until page contains element    xpath://iframe[@title="Test embedded dashboard title"]
    select frame    xpath://iframe[@title="Test embedded dashboard title"]
    user waits until h1 is visible    Analysts’ Guide    %{WAIT_SMALL}
    unselect frame

User navigates to Data blocks page to edit block
    [Documentation]    EES-3009
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks    %{WAIT_SMALL}

Edit data block
    user waits until table is visible
    user clicks edit data block link    ${DATABLOCK_NAME}

    user checks page contains element    //*[@data-testid="Data set name-key" and contains(text(), "Data set name")]
    user checks page contains element
    ...    //*[@data-testid="Data set name-value" and contains(text(), "${SUBJECT_NAME}")]

    user enters text into element    id:dataBlockDetailsForm-name    ${DATABLOCK_NAME}
    user enters text into element    id:dataBlockDetailsForm-heading    Updated dates table title
    user enters text into element    id:dataBlockDetailsForm-source    Updated dates source

    user clicks button    Save data block
    user waits until page contains button    Delete this data block
    user waits until page finishes loading

Navigate to the 'Content' page
    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}
    user waits until page contains button    Add a summary text block    %{WAIT_SMALL}

Verify data block is updated correctly
    # checking if data block cache has been invalidated by verifying the updates on the block
    user scrolls to accordion section    Dates data block    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user opens accordion section    Dates data block    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    ${datablock}=    set variable    testid:Data block - ${DATABLOCK_NAME}

    user checks chart title contains    ${datablock}    Updated dates table title
    user clicks link containing text    Table    ${datablock}
    user waits until parent contains element    ${datablock}
    ...    xpath:.//*[@data-testid="dataTableCaption" and text()="Updated dates table title"]
    user waits until parent contains element    ${datablock}    xpath:.//*[.="Source: Updated dates source"]
    user closes accordion section    Dates data block    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

Add public prerelease access list
    user clicks link    Pre-release access
    user creates public prerelease access list    Test public access list

Approve release for scheduled publication
    ${days_until_release}=    set variable    0
    ${publish_date_day}=    get london day of month    offset_days=${days_until_release}
    ${publish_date_month}=    get london month date    offset_days=${days_until_release}
    ${publish_date_month_word}=    get london month word    offset_days=${days_until_release}
    ${publish_date_year}=    get london year    offset_days=${days_until_release}

    user approves release for scheduled publication
    ...    ${publish_date_day}
    ...    ${publish_date_month}
    ...    ${publish_date_year}
    ...    12
    ...    3001

    set suite variable    ${EXPECTED_SCHEDULED_DATE}
    ...    ${publish_date_day} ${publish_date_month_word} ${publish_date_year}

Verify release is scheduled
    user checks summary list contains    Current status    Approved
    user checks summary list contains    Scheduled release    ${EXPECTED_SCHEDULED_DATE}
    user checks summary list contains    Next release expected    December 3001

Get public release link
    ${PUBLIC_RELEASE_LINK}=    user gets url public release will be accessible at
    Set Suite Variable    ${PUBLIC_RELEASE_LINK}

Publish the scheduled release
    user waits for scheduled release to be published immediately

    ${EXPECTED_PUBLISHED_DATE}=    get london date
    set suite variable    ${EXPECTED_PUBLISHED_DATE}

Verify newly published release is public
    user navigates to public release page    ${PUBLIC_RELEASE_LINK}    ${PUBLICATION_NAME}    ${RELEASE_NAME}

Verify release page meta
    user checks meta title should be    Release home - ${PUBLICATION_NAME}
    user checks meta description should be
    ...    ${PUBLICATION_NAME} summary

Verify release URL
    user checks url contains    %{PUBLIC_URL}/find-statistics/publish-release-and-amend-%{RUN_IDENTIFIER}

Verify publish and update dates
    user checks summary list contains    Published    ${EXPECTED_PUBLISHED_DATE}
    user checks element should contain    testid:Next release    December 3001

Verify release associated files
    user clicks link    Explore and download data
    user waits until h2 is visible    Explore data used in this release
    user waits until h2 is visible    Data sets: download or create tables
    user checks main links for page 'Explore and download data' are present
    ...    Download all data from this release (ZIP)
    ...    Data sets: download or create tables
    ...    Data guidance
    ...    Data catalogue

    User checks page 'Explore and download data' data set available properties    ${SUBJECT_NAME}    118
    ...    2020 Week 13 to 2021 Week 24    ${PUBLICATION_NAME}    Dates test subject test data guidance content
    user goes back

    ${supporting_files_xpath}=    Set Variable
    ...    xpath:*//section[@data-testid="supporting-files-section"]//li
    ${download_xpath}=    Set Variable
    ...    ${supporting_files_xpath}//a[contains(., "Download")]

    user waits until page contains element    id:supporting-files-section

    user checks element count is x    ${supporting_files_xpath}    1

    ${other_files}=    Get WebElement    id:supporting-files-section
    ${other_files_1}=    get child element    ${other_files}    css:li:nth-child(1)

    user checks element should contain    ${other_files_1}    ${ANCILLARY_FILE_NAME} summary

    download file    ${download_xpath}    test_ancillary_file_1.txt
    downloaded file should have first line    test_ancillary_file_1.txt    Test file 1

Navigate to public Data set information page
    user scrolls to element    id:datasets-section
    user opens data set details for subject    ${SUBJECT_NAME}

    Page Should Contain Link    Data set information page
    user clicks link containing text    Data set information page
    user waits until h1 is visible    ${SUBJECT_NAME}

Verify 'Data set information' page
    user scrolls to element    id:dataSetVariables
    user clicks button    Show all 17 variables

    Page Should Contain    Dates test subject test data guidance content

    user checks summary list contains    Geographic levels
    ...    National
    ...    id:dataSetDetails

    user checks summary list contains    Time period
    ...    2020 Week 13 to 2021 Week 24
    ...    id:dataSetDetails

    user checks table column heading contains    1    1    Variable name    css:table[data-testid="variables-table"]
    user checks table column heading contains    1    2    Variable description
    ...    css:table[data-testid="variables-table"]

    user checks table cell contains    1    1    children_attending    css:table[data-testid="variables-table"]
    user checks table cell contains    1    2    Number of children attending
    ...    css:table[data-testid="variables-table"]

    user checks table cell contains    6    1    date    css:table[data-testid="variables-table"]
    user checks table cell contains    6    2    Date    css:table[data-testid="variables-table"]

    user checks table cell contains    10    1    otherwise_vulnerable_children_attending
    ...    css:table[data-testid="variables-table"]
    user checks table cell contains    10    2    Number of otherwise vulnerable children attending
    ...    css:table[data-testid="variables-table"]

    user goes back

Navigate to the public pre-release access list
    user clicks link    Help and related information
    user waits until h2 is visible    Get help by contacting us

Verify public pre-release access list
    user checks section with ID contains elements and back to top link    pre-release-access-list-section
    ...    Test public access list

Verify free text key stat is correct
    user clicks link    Release home
    user waits until h2 is visible    Headline facts and figures
    user checks element count is x    testid:keyStat    1
    user checks key stat contents    1    Free text key stat title    9001%    Trend
    user checks key stat guidance    1    Guidance title    Guidance text

Verify Dates data block section and footnotes
    user scrolls to element    id:section-dates-data-block
    user waits until page finishes loading

    user checks chart title contains    id:section-dates-data-block    Updated dates table title
    user checks infographic chart contains alt    id:section-dates-data-block    Sample alt text

    user clicks link containing text    Table    id:section-dates-data-block
    user waits until parent contains element    id:section-dates-data-block
    ...    xpath:.//*[@data-testid="dataTableCaption" and text()="Updated dates table title"]
    user waits until parent contains element    id:section-dates-data-block
    ...    xpath:.//*[.="Source: Updated dates source"]

    user checks table column heading contains    1    1    2020 Week 13    id:section-dates-data-block
    user checks headed table body row cell contains    Number of open settings    1    22,900
    ...    id:section-dates-data-block
    user checks headed table body row cell contains    Proportion of settings open    1    1%
    ...    id:section-dates-data-block

    ${data_block_table}=    user gets data block table from parent    ${DATABLOCK_NAME}    id:section-dates-data-block

    user checks list has x items    testid:footnotes    2    ${data_block_table}
    user checks list item contains    testid:footnotes    1
    ...    Applies to all data 1
    ...    ${data_block_table}
    user checks list item contains    testid:footnotes    2
    ...    Applies to all data 2
    ...    ${data_block_table}

Verify Dates data block Fast Track page
    ${release_url}=    user gets url

    user clicks link containing text    Explore data    testid:Data block - Dates data block name-table-tab

    user waits until page contains title    Create your own tables
    user waits until page contains    This is the latest data

    user checks table column heading contains    1    1    2020 Week 13
    user checks headed table body row cell contains    Number of open settings    1    22,900
    user checks headed table body row cell contains    Proportion of settings open    1    1%

    user checks list has x items    testid:footnotes    2
    user checks list item contains    testid:footnotes    1
    ...    Applies to all data 1
    user checks list item contains    testid:footnotes    2
    ...    Applies to all data 2

    ${url}=    user gets url
    set suite variable    ${FAST_TRACK_URL}    ${url}

    go to    ${release_url}

Verify Test text section contains correct text
    user checks section with ID contains elements and back to top link    section-test-text
    ...    Some test text!

Verify embedded dashboard accordion section contains dashboard
    ${section}=    Get WebElement    id:section-test-embedded-dashboard-section

    user waits until parent contains element    ${section}    xpath:.//iframe[@title="Test embedded dashboard title"]

    select frame    xpath://iframe[@title="Test embedded dashboard title"]
    user waits until h1 is visible    Analysts’ Guide    %{WAIT_SMALL}
    unselect frame

Return to Admin and create first amendment
    user navigates to admin dashboard    Bau1
    user creates amendment for release    ${PUBLICATION_NAME}    ${RELEASE_NAME}

Change the Release type
    user waits until page contains link    Edit release summary
    user clicks link    Edit release summary
    user waits until page finishes loading
    user waits until h2 is visible    Edit release summary
    user checks page contains radio    Official statistics in development
    user clicks radio    Official statistics in development
    user clicks button    Update release summary
    user waits until h2 is visible    Release summary
    user checks page contains element    xpath://li/a[text()="Summary" and contains(@aria-current, 'page')]
    user verifies release summary    Financial year
    ...    3000-01
    ...    Official statistics in development

Upload a replacement data set
    user uploads subject replacement    ${SUBJECT_NAME}    dates-replacement.csv    dates-replacement.meta.csv
    user waits until page contains element    testid:Data file replacements table
    user confirms replacement upload    ${SUBJECT_NAME}

Confirm data replacement details on replacement page
    user checks table cell contains    1    1    ${SUBJECT_NAME}    testid:Data file replacements table
    user checks table cell contains    1    2    17 Kb    testid:Data file replacements table
    user checks table cell contains    1    3    Ready    testid:Data file replacements table
    user clicks link in table cell    1    4    View details    testid:Data file replacements table

    user waits until page contains element    testid:Replacement Title
    user checks table column heading contains    1    1    Original file
    user checks table column heading contains    1    2    Replacement file

    user checks headed table body row cell contains    Title    1    ${SUBJECT_NAME}
    user checks headed table body row cell contains    Data file    1    dates.csv
    user checks headed table body row cell contains    Metadata file    1    dates.meta.csv
    user checks headed table body row cell contains    Number of rows    1    118    wait=%{WAIT_SMALL}
    user checks headed table body row cell contains    Data file size    1    17 Kb    wait=%{WAIT_SMALL}
    user checks headed table body row cell contains    Data file import status    1    Complete    wait=%{WAIT_LONG}

    user checks headed table body row cell contains    Title    2    ${SUBJECT_NAME}
    user checks headed table body row cell contains    Data file    2    dates-replacement.csv
    user checks headed table body row cell contains    Metadata file    2    dates-replacement.meta.csv
    user checks headed table body row cell contains    Number of rows    2    118    wait=%{WAIT_SMALL}
    user checks headed table body row cell contains    Data file size    2    17 Kb    wait=%{WAIT_SMALL}
    user checks headed table body row cell contains    Data file import status    1    Complete
    ...    wait=%{WAIT_DATA_FILE_IMPORT}

Confirm data replacement
    user waits until page contains    Data blocks: OK
    user waits until page contains    Footnotes: OK
    user clicks button    Confirm data replacement
    user waits until h2 is visible    Data replacement complete

Verify existing data guidance for amendment
    user clicks link    Data and files
    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance

    user waits until element contains    id:dataGuidanceForm-content    Test data guidance content

    user waits until page contains accordion section    ${SUBJECT_NAME}

    user checks summary list contains    Filename    dates-replacement.csv
    user checks summary list contains    Geographic levels    National
    user checks summary list contains    Time period    2020 Week 13 to 2021 Week 24

    ${editor}=    user gets data guidance data file content editor    ${SUBJECT_NAME}
    user checks element value should be    ${editor}    ${SUBJECT_NAME} test data guidance content

Update existing data guidance for amendment
    user adds main data guidance content    Amended test data guidance content
    user enters text into data guidance data file content editor    ${SUBJECT_NAME}
    ...    Amended ${SUBJECT_NAME} test data guidance content

    user clicks button    Save guidance

Navigate to 'Footnotes' section for amendment
    user clicks link    Footnotes
    user waits until h2 is visible    Footnotes

Add a footnote to amendment
    user waits until page contains link    Create footnote
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote
    user clicks footnote subject radio    ${SUBJECT_NAME}    Applies to all data
    user clicks element    id:footnoteForm-content
    user enters text into element    id:footnoteForm-content
    ...    Applies to all data 3
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes

Confirm amendment has footnotes
    user waits until h2 is visible    Footnotes
    user waits until page contains element    testid:Footnote - Applies to all data 1
    user waits until page contains element    testid:Footnote - Applies to all data 2
    user waits until page contains element    testid:Footnote - Applies to all data 3

Add ancillary file to amendment
    user clicks link    Data and files
    user clicks link    Supporting file uploads
    user waits until h2 is visible    Add file to release

    user enters text into element    id:ancillaryFileForm-title    Test ancillary file 2
    user enters text into element    id:ancillaryFileForm-summary    Test ancillary file 2 summary
    user chooses file    id:ancillaryFileForm-file    ${FILES_DIR}test-file-2.txt
    user clicks button    Add file

    user waits until page contains accordion section    Test ancillary file 2
    user opens accordion section    Test ancillary file 2    id:file-uploads

    ${section_2}=    user gets accordion section content element    Test ancillary file 2    id:file-uploads
    user checks summary list contains    Title    Test ancillary file 2    ${section_2}
    user checks summary list contains    Summary    Test ancillary file 2 summary    ${section_2}
    user checks summary list contains    File    test-file-2.txt    ${section_2}
    user checks summary list contains    File size    24 B    ${section_2}

    user checks there are x accordion sections    2    id:file-uploads

User navigates to Data blocks page
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks    %{WAIT_SMALL}

Edit data block for amendment
    user waits until table is visible

    user checks table body has x rows    1
    user checks table cell contains    1    1    ${DATABLOCK_NAME}
    user checks table cell contains    1    2    Yes
    user checks table cell contains    1    3    Yes

    user clicks edit data block link    ${DATABLOCK_NAME}

    user clicks element    testid:wizardStep-4-goToButton

    user clicks button    Date
    user clicks category checkbox    Date    24/03/2020
    user checks category checkbox is checked    Date    24/03/2020

    user clicks element    id:filtersForm-submit
    user waits until results table appears    %{WAIT_LONG}

    user checks table column heading contains    1    1    2020 Week 13
    user checks headed table body row cell contains    Number of open settings    1    23,000
    user checks headed table body row cell contains    Proportion of settings open    1    2%
    user checks headed table body row cell contains    Number of open settings    1    23,600
    user checks headed table body row cell contains    Proportion of settings open    1    1%

Save data block for amendment
    user enters text into element    id:dataBlockDetailsForm-name    ${DATABLOCK_NAME}
    user enters text into element    id:dataBlockDetailsForm-heading    Amended dates table title
    user enters text into element    id:dataBlockDetailsForm-source    Amended dates source

    user clicks button    Save data block
    user waits until page contains button    Delete this data block

Update data block chart for amendment
    user waits until page contains link    Chart    %{WAIT_SMALL}
    user waits until page finishes loading
    user clicks link    Chart

    user waits until page contains element    id:chartConfigurationForm-title    %{WAIT_SMALL}

    user checks radio is checked    Use table title
    user clicks radio    Set an alternative title
    user enters text into element    id:chartConfigurationForm-title    Amended sample title
    user checks input field contains    id:chartConfigurationForm-title    Amended sample title
    user enters text into element    id:chartConfigurationForm-alt    Amended sample alt text
    user checks textarea contains    id:chartConfigurationForm-alt    Amended sample alt text
    user saves infographic configuration
    user checks infographic chart contains alt    id:chartBuilderPreview    Amended sample alt text

Navigate to 'Content' page for amendment
    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}
    user waits until page contains button    Add a summary text block

Update free text key stat
    user updates free text key stat    1    Updated title    New stat    Updated trend
    ...    Updated guidance title    Updated guidance text

    user checks element count is x    testid:keyStat    1
    user checks key stat contents    1    Updated title    New stat    Updated trend
    user checks key stat guidance    1    Updated guidance title    Updated guidance text

Verify amended Dates data block table has footnotes
    ${accordion}=    user opens accordion section    Dates data block    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user scrolls down    100
    user waits for caches to expire
    ${data_block_table}=    user gets data block table from parent    ${DATABLOCK_NAME}    ${accordion}

    user checks list has x items    testid:footnotes    2    ${data_block_table}
    user checks list item contains    testid:footnotes    1
    ...    Applies to all data 1
    ...    ${data_block_table}
    user checks list item contains    testid:footnotes    2
    ...    Applies to all data 2
    ...    ${data_block_table}

    user clicks button    Show 1 more footnote    ${data_block_table}
    user checks list has x items    testid:footnotes    3    ${data_block_table}
    user checks list item contains    testid:footnotes    3
    ...    Applies to all data 3
    ...    ${data_block_table}

Update second accordion section text for amendment
    user opens accordion section    Test text    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user adds content to autosaving accordion section text block    Test text    1    Amended test text!
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

Update embedded dashboard title and url
    user chooses to update an embedded URL in editable accordion section
    ...    Test embedded dashboard section
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

    ${modal}=    user updates embedded URL details in modal
    ...    Amended Test embedded dashboard title
    ...    https://google.com
    ...    Edit embedded URL

    user clicks button    Save    ${modal}
    user waits until page contains    URL must be on a permitted domain

    user updates embedded URL details in modal
    ...    Amended Test embedded dashboard title
    ...    https://dfe-analytical-services.github.io/dfeshiny
    ...    Edit embedded URL

    user presses keys    TAB
    user clicks button    Save    ${modal}
    user waits until page does not contain    URL must be on a permitted domain
    user waits until modal is not visible    Edit embedded URL

    user waits until page contains element    xpath://iframe[@title="Amended Test embedded dashboard title"]
    select frame    xpath://iframe[@title="Amended Test embedded dashboard title"]
    user waits until h1 is visible    dfeshiny    %{WAIT_SMALL}
    unselect frame

    user closes accordion section    Test embedded dashboard section    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

Add release note to first amendment
    user clicks button    Add note
    user enters text into element    id:create-release-note-form-reason    Test release note one
    user clicks button    Save note
    ${date}=    get london date
    user waits until element contains    css:#release-notes li:nth-of-type(1) time    ${date}
    user waits until element contains    css:#release-notes li:nth-of-type(1) p    Test release note one

Check public prerelease access list for amendment is same as original release
    user clicks link    Pre-release access
    user clicks link    Public access list
    user waits until h2 is visible    Public pre-release access list
    user checks page contains    Test public access list

Update public prerelease access list
    user updates public prerelease access list    Amended public access list

Approve amendment for scheduled release
    ${days_until_release}=    set variable    1
    ${publish_date_day}=    get london day of month    offset_days=${days_until_release}
    ${publish_date_month}=    get london month date    offset_days=${days_until_release}
    ${publish_date_month_word}=    get london month word    offset_days=${days_until_release}
    ${publish_date_year}=    get london year    offset_days=${days_until_release}

    user approves release for scheduled publication
    ...    ${publish_date_day}
    ...    ${publish_date_month}
    ...    ${publish_date_year}
    ...    12
    ...    3001

    user waits for scheduled release to be published immediately

    ${EXPECTED_PUBLISHED_DATE}=    get london date
    set suite variable    ${EXPECTED_PUBLISHED_DATE}

Navigate to amendment release page
    user navigates to public release page    ${PUBLIC_RELEASE_LINK}    ${PUBLICATION_NAME}    ${RELEASE_NAME}

    user checks url contains    %{PUBLIC_URL}/find-statistics/publish-release-and-amend-%{RUN_IDENTIFIER}

    user checks breadcrumb count should be    3
    user checks nth breadcrumb contains    1    Home
    user checks nth breadcrumb contains    2    Find statistics and data
    user checks nth breadcrumb contains    3    ${PUBLICATION_NAME}

Verify amendment is displayed as the latest release and the 'All releases in this series' page only displays the latest release
    user checks page contains    Latest release
    user clicks link    All releases in this series
    user waits until page finishes loading

    user checks table body has x rows    1    testid:release-updates-table
    user checks table cell contains    1    1    ${RELEASE_NAME}    testid:release-updates-table

    user goes back
    user waits until h1 is visible    ${PUBLICATION_NAME}

Verify amendment is published
    user checks summary list contains    Published    ${EXPECTED_PUBLISHED_DATE}
    user checks element should contain    testid:Next release    December 3001

Verify amendment free text key stat is updated
    user checks element count is x    testid:keyStat    1
    user checks key stat contents    1    Updated title    New stat    Updated trend
    user checks key stat guidance    1    Updated guidance title    Updated guidance text

Verify amendment files
    user clicks link    Explore and download data
    user waits until h2 is visible    Explore data used in this release
    user waits until h2 is visible    Data sets: download or create tables

    user checks main links for page 'Explore and download data' are present
    ...    Download all data from this release (ZIP)
    ...    Data sets: download or create tables
    ...    Data guidance
    ...    Data catalogue

    ${supporting_files_xpath}=    Set Variable
    ...    xpath:*//section[@data-testid="supporting-files-section"]//li
    ${download_file_1_xpath}=    Set Variable
    ...    ${supporting_files_xpath}//a[contains(., "Download") and contains(.,"Test ancillary file 1")]
    ${download_file_2_xpath}=    Set Variable
    ...    ${supporting_files_xpath}//a[contains(., "Download") and contains(.,"Test ancillary file 2")]

    user checks element count is x    ${supporting_files_xpath}    2

    ${other_files}=    Get WebElement    id:supporting-files-section
    ${other_files_1}=    get child element    ${other_files}    css:li:nth-child(1)
    ${other_files_2}=    get child element    ${other_files}    css:li:nth-child(2)

    user checks element should contain    ${other_files}    ${ANCILLARY_FILE_NAME} summary
    user checks element should contain    ${other_files}    ${ANCILLARY_FILE_NAME_2} summary

    download file    ${download_file_1_xpath}    test_ancillary_file_1.txt
    downloaded file should have first line    test_ancillary_file_1.txt    Test file 1

    download file    ${download_file_2_xpath}    test_ancillary_file_2.txt
    downloaded file should have first line    test_ancillary_file_2.txt    Test file 2

Navigate to public Data set information page
    user scrolls to element    id:datasets-section
    user opens data set details for subject    ${SUBJECT_NAME}

    Page Should Contain Link    Data set information page
    user clicks link containing text    Data set information page
    user waits until h1 is visible    ${SUBJECT_NAME}

Verify public metadata guidance document
    user scrolls to element    id:dataSetVariables
    user clicks button    Show all 17 variables

    user checks table column heading contains    1    1    Variable name    css:table[data-testid="variables-table"]
    user checks table column heading contains    1    2    Variable description
    ...    css:table[data-testid="variables-table"]

    user checks table cell contains    1    1    children_attending    css:table[data-testid="variables-table"]
    user checks table cell contains    1    2    Number of children attending
    ...    css:table[data-testid="variables-table"]

    user checks table cell contains    6    1    date    css:table[data-testid="variables-table"]
    user checks table cell contains    6    2    Date    css:table[data-testid="variables-table"]

    user checks table cell contains    10    1    otherwise_vulnerable_children_attending
    ...    css:table[data-testid="variables-table"]
    user checks table cell contains    10    2    Number of otherwise vulnerable children attending
    ...    css:table[data-testid="variables-table"]

    user goes back

Navigate to the public pre-release access list
    user clicks link    Help and related information
    user waits until h2 is visible    Get help by contacting us

Verify public pre-release access list
    user checks section with ID contains elements and back to top link    pre-release-access-list-section
    ...    Amended public access list

Verify amendment accordions are correct
    user clicks link    Release home
    user waits until h2 is visible    Headline facts and figures

    user checks section is in position    Dates data block    1    testid:home-content
    user checks section is in position    Test text    2    testid:home-content
    user checks section is in position    Test embedded dashboard section    3    testid:home-content

Verify amendment help and support section is correct
    user checks page contains    Help and support
    user checks page contains    Official statistics in development
    user checks page contains    Contact us

Verify amendment Dates data block section and footnotes
    user scrolls to element    id:section-dates-data-block
    user waits until page finishes loading
    ${section}=    Get WebElement    id:section-dates-data-block

    user checks chart title contains    ${section}    Amended sample title
    user checks infographic chart contains alt    ${section}    Amended sample alt text

    user clicks link containing text    Table    ${section}
    user waits until parent contains element    ${section}
    ...    xpath:.//*[@data-testid="dataTableCaption" and text()="Amended dates table title"]
    user waits until parent contains element    ${section}    xpath:.//*[.="Source: Amended dates source"]

    user checks table column heading contains    1    1    2020 Week 13    ${section}
    user checks headed table body row cell contains    Number of open settings    1    23,000    ${section}
    user checks headed table body row cell contains    Proportion of settings open    1    2%    ${section}
    user checks headed table body row cell contains    Number of open settings    1    23,600    ${section}
    user checks headed table body row cell contains    Proportion of settings open    1    1%    ${section}

    ${data_block_table}=    user gets data block table from parent    ${DATABLOCK_NAME}    ${section}

    user checks list has x items    testid:footnotes    2    ${data_block_table}
    user checks list item contains    testid:footnotes    1
    ...    Applies to all data 1
    ...    ${data_block_table}
    user checks list item contains    testid:footnotes    2
    ...    Applies to all data 2
    ...    ${data_block_table}

    user clicks button    Show 1 more footnote    ${data_block_table}
    user checks list has x items    testid:footnotes    3    ${data_block_table}
    user checks list item contains    testid:footnotes    3
    ...    Applies to all data 3
    ...    ${data_block_table}

Verify amendment Dates data block Fast Track page
    ${release_url}=    user gets url

    user clicks link containing text    Explore data    testid:Data block - Dates data block name-table-tab

    user waits until page contains title    Create your own tables
    user waits until page contains    This is the latest data

    # Check to ensure that the new version of the Data Block is still accessible on the same link as before.
    user checks url equals    ${FAST_TRACK_URL}

    user checks table column heading contains    1    1    2020 Week 13
    user checks headed table body row cell contains    Number of open settings    1    23,000
    user checks headed table body row cell contains    Proportion of settings open    1    2%
    user checks headed table body row cell contains    Number of open settings    1    23,600
    user checks headed table body row cell contains    Proportion of settings open    1    1%

    user checks list has x items    testid:footnotes    2
    user checks list item contains    testid:footnotes    1
    ...    Applies to all data 1
    user checks list item contains    testid:footnotes    2
    ...    Applies to all data 2

    user clicks button    Show 1 more footnote
    user checks list has x items    testid:footnotes    3
    user checks list item contains    testid:footnotes    3
    ...    Applies to all data 3

    go to    ${release_url}

Verify amendment Test text accordion section contains correct text
    user waits until h2 is visible    Test text
    user checks section with ID contains elements and back to top link    section-test-text
    ...    Amended test text!

Verify amendment embedded dashboard accordion section is correct
    ${section}=    Get WebElement    id:section-test-embedded-dashboard-section
    user checks element contains child element    ${section}
    ...    xpath:.//iframe[@title="Amended Test embedded dashboard title"]

Return to published release page in Admin
    user navigates to admin dashboard    Bau1
    user navigates to published release page from dashboard
    ...    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}

Override release published date to past date
    ${release_id}=    get release id from url
    ${published_override}=    Get Current Date    UTC    increment=-1000 days    result_format=datetime
    user updates release published date via api    ${release_id}    ${published_override}
    ${EXPECTED_PUBLISHED_DATE}=    get london date    offset_days=-1000
    set suite variable    ${EXPECTED_PUBLISHED_DATE}

Verify published date on publication page is overriden with past date
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    ${row}=    user gets table row    ${RELEASE_NAME}    testid:publication-published-releases
    user checks element contains    ${row}    ${EXPECTED_PUBLISHED_DATE}

Verify public published date is overriden with past date
    user navigates to    ${PUBLIC_RELEASE_LINK}
    user waits until h1 is visible    ${PUBLICATION_NAME}
    user checks summary list contains    Published    ${EXPECTED_PUBLISHED_DATE}

Return to Admin and create second amendment
    user navigates to admin dashboard    Bau1
    user creates amendment for release    ${PUBLICATION_NAME}    ${RELEASE_NAME}

Add release note to second amendment
    user clicks link    Content
    user clicks button    Add note
    user enters text into element    id:create-release-note-form-reason    Test release note two
    user clicks button    Save note

Remove the data block from the second amendment
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks
    user clicks button    Delete block
    user waits until modal is visible    Delete data block
    user clicks button    Confirm
    user waits until modal is not visible    Delete data block

Remove the content section that originally contained the deleted data block
    user clicks link    Content
    user waits until page contains accordion section    Dates data block
    user opens accordion section    Dates data block    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user deletes editable accordion section    Dates data block    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

Approve release amendment for scheduled publication and update published date
    ${days_until_release}=    set variable    2
    ${publish_date_day}=    get london day of month    offset_days=${days_until_release}
    ${publish_date_month}=    get london month date    offset_days=${days_until_release}
    ${publish_date_year}=    get london year    offset_days=${days_until_release}
    user approves release for scheduled publication
    ...    ${publish_date_day}
    ...    ${publish_date_month}
    ...    ${publish_date_year}
    ...    next_release_month=8
    ...    next_release_year=4001
    ...    update_amendment_published_date=${True}
    user waits for scheduled release to be published immediately
    ${EXPECTED_PUBLISHED_DATE}=    get london date
    set suite variable    ${EXPECTED_PUBLISHED_DATE}

Verify published date on publication page has been updated
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    ${row}=    user gets table row    ${RELEASE_NAME}    testid:publication-published-releases
    user checks element contains    ${row}    ${EXPECTED_PUBLISHED_DATE}

Navigate to amended public release
    user waits for caches to expire
    user navigates to    ${PUBLIC_RELEASE_LINK}
    user waits until h1 is visible    ${PUBLICATION_NAME}    %{WAIT_MEDIUM}

Verify public published date has been updated
    user checks summary list contains    Published    ${EXPECTED_PUBLISHED_DATE}

Validate next update date
    user checks element should contain    testid:Next release    August 4001

Verify that the Dates data block is no longer available
    go to    ${FAST_TRACK_URL}
    user waits until page contains title    Page not found
