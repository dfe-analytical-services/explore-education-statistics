*** Settings ***
Library             DateTime
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Resource            ../../libs/charts.robot
Resource            ../../libs/public-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${PUBLICATION_NAME}=    UI tests - publish release and amend %{RUN_IDENTIFIER}
${RELEASE_NAME}=        Financial year 3000-01
${DATABLOCK_NAME}=      Dates data block name


*** Test Cases ***
Create new publication for "UI tests topic" topic
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    FY    3000

Go to "Release summary" page
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}

Verify release summary
    user checks page contains element    xpath://li/a[text()="Summary" and contains(@aria-current, 'page')]
    user waits until h2 is visible    Release summary
    user checks summary list contains    Publication title    ${PUBLICATION_NAME}

Upload subject
    user uploads subject    Dates test subject    dates.csv    dates.meta.csv

Add data guidance
    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance

    user waits until page contains element    id:dataGuidanceForm-content
    user waits until page contains element    id:dataGuidance-dataFiles
    user enters text into element    id:dataGuidanceForm-content    Test data guidance content
    user waits until page contains accordion section    Dates test subject

    user checks summary list contains    Filename    dates.csv
    user checks summary list contains    Geographic levels    National
    user checks summary list contains    Time period    2020 Week 13 to 2021 Week 24

    user enters text into data guidance data file content editor    Dates test subject
    ...    Dates test subject test data guidance content
    user clicks button    Save guidance

Add ancillary file
    user clicks link    Ancillary file uploads
    user waits until h2 is visible    Add file to release

    user enters text into element    label:Title    Test ancillary file 1
    user enters text into element    label:Summary    Test ancillary file 1 summary
    user chooses file    label:Upload file    ${FILES_DIR}test-file-1.txt
    user clicks button    Upload file

    user waits until page contains accordion section    Test ancillary file 1
    user opens accordion section    Test ancillary file 1    id:file-uploads

    ${section_1}=    user gets accordion section content element    Test ancillary file 1    id:file-uploads
    user checks summary list contains    Title    Test ancillary file 1    ${section_1}
    user checks summary list contains    Summary    Test ancillary file 1 summary    ${section_1}
    user checks summary list contains    File    test-file-1.txt    ${section_1}
    user checks summary list contains    File size    12 B    ${section_1}

    user checks there are x accordion sections    1    id:file-uploads

Navigate to 'Footnotes' section
    user clicks link    Footnotes
    user waits until h2 is visible    Footnotes

Add a footnote
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote
    user clicks footnote subject radio    Dates test subject    Applies to all data
    user clicks element    id:footnoteForm-content
    user enters text into element    id:footnoteForm-content
    ...    Applies to all data 1
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes

Add a second footnote
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote
    user clicks footnote subject radio    Dates test subject    Applies to all data
    user clicks element    id:footnoteForm-content
    user enters text into element    id:footnoteForm-content
    ...    Applies to all data 2
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes

Confirm created footnotes
    user waits until page contains element    testid:Footnote - Applies to all data 1
    user waits until page contains element    testid:Footnote - Applies to all data 2

Create data block table
    user creates data block for dates csv    Dates test subject    ${DATABLOCK_NAME}    Dates table title

Create chart for data block
    user waits until page contains link    Chart
    user waits until page does not contain loading spinner
    user clicks link    Chart

    user clicks button    Choose an infographic as alternative
    user chooses file    id:chartConfigurationForm-file    ${FILES_DIR}test-infographic.png
    user checks radio is checked    Use table title
    user enters text into element    id:chartConfigurationForm-alt    Sample alt text

    user clicks button    Save chart options

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
    user waits for page to finish loading
    user waits until page does not contain loading spinner
    user clicks button    Add new section
    user changes accordion section title    1    Dates data block
    user clicks button    Add new section
    user changes accordion section title    2    Test text
    user clicks button    Add new section
    user changes accordion section title    3    Test embedded dashboard section

Add data block to first accordion section
    user adds data block to editable accordion section    Dates data block    ${DATABLOCK_NAME}
    ...    id:releaseMainContent
    ${datablock}=    set variable    xpath://*[@data-testid="Data block - ${DATABLOCK_NAME}"]
    user waits until page contains element    ${datablock}    %{WAIT_SMALL}
    user waits until element contains infographic chart    ${datablock}
    user checks chart title contains    ${datablock}    Dates table title
    user checks infographic chart contains alt    ${datablock}    Sample alt text

Verify data block table has footnotes
    ${accordion}=    user opens accordion section    Dates data block    id:releaseMainContent
    ${data_block_table}=    user gets data block table from parent    ${DATABLOCK_NAME}    ${accordion}

    user checks list has x items    testid:footnotes    2    ${data_block_table}
    user checks list item contains    testid:footnotes    1
    ...    Applies to all data 1
    ...    ${data_block_table}
    user checks list item contains    testid:footnotes    2
    ...    Applies to all data 2
    ...    ${data_block_table}

Add test text to second accordion section
    user adds text block to editable accordion section    Test text    id:releaseMainContent
    user adds content to autosaving accordion section text block    Test text    1    Some test text!
    ...    id:releaseMainContent

Add embedded dashboard to third accordion section
    user chooses to embed a URL in editable accordion section
    ...    Test embedded dashboard section
    ...    id:releaseMainContent

    ${modal}=    user updates embedded URL details in modal
    ...    Test embedded dashboard title
    ...    https://google.com

    user clicks button    Save    ${modal}
    user waits until page contains    URL must be on a permitted domain

    user updates embedded URL details in modal
    ...    Test embedded dashboard title
    ...    https://dfe-analytical-services.github.io/explore-education-statistics

    user waits until page does not contain    URL must be on a permitted domain
    user clicks button    Save    ${modal}
    user waits until modal is not visible    Embed a URL

    user waits until page contains element    xpath://iframe[@title="Test embedded dashboard title"]
    select frame    xpath://iframe[@title="Test embedded dashboard title"]
    user waits until h1 is visible    Explore Education Statistics service    %{WAIT_SMALL}
    unselect frame

User navigates to Data blocks page to edit block
    [Documentation]    EES-3009
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks    %{WAIT_SMALL}

Edit data block
    #just updating the table title and source
    user waits until table is visible
    user clicks link    Edit block    css:tbody > tr:first-child
    user waits until h2 is visible    ${DATABLOCK_NAME}
    user waits until h2 is visible    Data block details

    user enters text into element    id:dataBlockDetailsForm-name    ${DATABLOCK_NAME}
    user enters text into element    id:dataBlockDetailsForm-heading    Updated dates table title
    user enters text into element    id:dataBlockDetailsForm-source    Updated dates source

    user clicks button    Save data block
    user waits until page contains button    Delete this data block

Navigate to the 'Content' page
    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}
    user waits until page contains button    Add a summary text block    %{WAIT_SMALL}

Verify data block is updated correctly
    #checking if data block cache has been invalidated by verifying the updates on the block
    user scrolls to accordion section content    Dates data block    id:releaseMainContent
    user opens accordion section    Dates data block    id:releaseMainContent
    ${section}=    user gets accordion section content element    Dates data block    id:releaseMainContent

    user checks chart title contains    ${section}    Updated dates table title
    user clicks link by visible text    Table    ${section}
    user waits until parent contains element    ${section}
    ...    xpath:.//*[@data-testid="dataTableCaption" and text()="Updated dates table title"]
    user waits until parent contains element    ${section}    xpath:.//*[.="Source: Updated dates source"]
    user closes accordion section    Dates data block    id:releaseMainContent

Add public prerelease access list
    user clicks link    Pre-release access
    user creates public prerelease access list    Test public access list

Approve release for scheduled publication
    ${days_until_release}=    set variable    0
    ${publish_date_day}=    get current datetime    %-d    ${days_until_release}
    ${publish_date_month}=    get current datetime    %-m    ${days_until_release}
    ${publish_date_month_word}=    get current datetime    %B    ${days_until_release}
    ${publish_date_year}=    get current datetime    %Y    ${days_until_release}

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
    user waits until page contains element    testid:public-release-url
    ${PUBLIC_RELEASE_LINK}=    Get Value    xpath://*[@data-testid="public-release-url"]
    check that variable is not empty    PUBLIC_RELEASE_LINK    ${PUBLIC_RELEASE_LINK}
    Set Suite Variable    ${PUBLIC_RELEASE_LINK}

Publish the scheduled release
    user waits for scheduled release to be published immediately

    ${EXPECTED_PUBLISHED_DATE}=    get current datetime    ${DATE_FORMAT_MEDIUM}
    set suite variable    ${EXPECTED_PUBLISHED_DATE}

Verify newly published release is on Find Statistics page
    user checks publication is on find statistics page    ${PUBLICATION_NAME}

Navigate to newly published release page
    user clicks link    ${PUBLICATION_NAME}
    user waits until h1 is visible    ${PUBLICATION_NAME}    %{WAIT_MEDIUM}

Verify release URL and page caption
    user checks url contains    %{PUBLIC_URL}/find-statistics/ui-tests-publish-release-and-amend-%{RUN_IDENTIFIER}
    user waits until page contains title caption    ${RELEASE_NAME}

Verify publish and update dates
    user checks summary list contains    Published    ${EXPECTED_PUBLISHED_DATE}
    user checks summary list contains    Next update    December 3001

Verify release associated files
    ${downloads}=    user gets testid element    data-and-files
    user waits until page contains element    ${downloads}    %{WAIT_SMALL}

    user checks element should contain    ${downloads}    Download all data (ZIP)
    ...    %{WAIT_SMALL}
    user checks element should contain    ${downloads}
    ...    Download all data available in this release as a compressed ZIP file
    user checks element should contain    ${downloads}
    ...    View tables that we have built for you, or create your own tables from open data using our table tool

    user checks element should contain    ${downloads}
    ...    Browse and download open data files from this release in our data catalogue
    user checks element should contain    ${downloads}
    ...    Learn more about the data files used in this release using our online guidance

    user opens accordion section    Additional supporting files
    ${other_files}=    user gets accordion section content element    Additional supporting files
    ${other_files_1}=    get child element    ${other_files}    css:li:nth-child(1)

    user waits until element contains link    ${other_files_1}    Test ancillary file 1 (txt, 12 B)
    user checks element should contain    ${other_files_1}    Test ancillary file 1 summary

    download file    link:Test ancillary file 1 (txt, 12 B)    test_ancillary_file_1.txt
    downloaded file should have first line    test_ancillary_file_1.txt    Test file 1

Verify public metadata guidance document
    user clicks link    Data guidance

    user checks breadcrumb count should be    4
    user checks nth breadcrumb contains    1    Home
    user checks nth breadcrumb contains    2    Find statistics and data
    user checks nth breadcrumb contains    3    ${PUBLICATION_NAME}
    user checks nth breadcrumb contains    4    Data guidance
    user waits until h2 is visible    Data guidance    %{WAIT_SMALL}

    user waits until page contains title caption    ${RELEASE_NAME}
    user waits until h1 is visible    ${PUBLICATION_NAME}

    user waits until h2 is visible    Data guidance
    user waits until page contains    Test data guidance content

    user waits until page contains accordion section    Dates test subject
    user checks there are x accordion sections    1

    user opens accordion section    Dates test subject
    user checks summary list contains    Filename    dates.csv
    user checks summary list contains    Geographic levels    National
    user checks summary list contains    Time period    2020 Week 13 to 2021 Week 24
    user checks summary list contains    Content    Dates test subject test data guidance content

    user opens details dropdown    Variable names and descriptions

    user checks table column heading contains    1    1    Variable name    css:table[data-testid="Variables"]
    user checks table column heading contains    1    2    Variable description    css:table[data-testid="Variables"]

    user checks table cell contains    1    1    children_attending    css:table[data-testid="Variables"]
    user checks table cell contains    1    2    Number of children attending
    ...    css:table[data-testid="Variables"]

    user checks table cell contains    6    1    date    css:table[data-testid="Variables"]
    user checks table cell contains    6    2    Date    css:table[data-testid="Variables"]

    user checks table cell contains    10    1    otherwise_vulnerable_children_attending
    ...    css:table[data-testid="Variables"]
    user checks table cell contains    10    2    Number of otherwise vulnerable children attending
    ...    css:table[data-testid="Variables"]

    user goes to release page via breadcrumb    ${PUBLICATION_NAME}    ${RELEASE_NAME}

Verify public pre-release access list
    user clicks link    Pre-release access list

    user checks breadcrumb count should be    4
    user checks nth breadcrumb contains    1    Home
    user checks nth breadcrumb contains    2    Find statistics and data
    user checks nth breadcrumb contains    3    ${PUBLICATION_NAME}
    user checks nth breadcrumb contains    4    Pre-release access list

    user waits until page contains title caption    ${RELEASE_NAME}
    user waits until h1 is visible    ${PUBLICATION_NAME}

    user waits until h2 is visible    Pre-release access list
    user waits until page contains    Published ${EXPECTED_PUBLISHED_DATE}
    user waits until page contains    Test public access list

Verify free text key stat is correct
    user goes to release page via breadcrumb    ${PUBLICATION_NAME}    ${RELEASE_NAME}

    user checks element count is x    testid:keyStat    1
    user checks key stat contents    1    Free text key stat title    9001%    Trend
    user checks key stat guidance    1    Guidance title    Guidance text

Verify accordions are correct
    user checks there are x accordion sections    1    id:data-accordion
    user checks accordion is in position    Additional supporting files    1    id:data-accordion

    user checks there are x accordion sections    3    id:content
    user checks accordion is in position    Dates data block    1    id:content
    user checks accordion is in position    Test text    2    id:content
    user checks accordion is in position    Test embedded dashboard section    3    id:content

    user checks there are x accordion sections    2    id:help-and-support-accordion
    user checks accordion is in position    National statistics    1    id:help-and-support-accordion
    user checks accordion is in position    Contact us    2    id:help-and-support-accordion

Verify Dates data block accordion section
    user opens accordion section    Dates data block    id:content
    user scrolls to accordion section content    Dates data block    id:content
    ${section}=    user gets accordion section content element    Dates data block    id:content

    user checks chart title contains    ${section}    Updated dates table title
    user checks infographic chart contains alt    ${section}    Sample alt text

    user clicks link by visible text    Table    ${section}
    user waits until parent contains element    ${section}
    ...    xpath:.//*[@data-testid="dataTableCaption" and text()="Updated dates table title"]
    user waits until parent contains element    ${section}    xpath:.//*[.="Source: Updated dates source"]

    user checks table column heading contains    1    1    2020 Week 13    ${section}
    user checks headed table body row cell contains    Number of open settings    1    22,900    ${section}
    user checks headed table body row cell contains    Proportion of settings open    1    1%    ${section}

    user closes accordion section    Dates data block    id:content

Verify Dates data block table has footnotes
    ${accordion}=    user opens accordion section    Dates data block    id:content
    ${data_block_table}=    user gets data block table from parent    ${DATABLOCK_NAME}    ${accordion}

    user checks list has x items    testid:footnotes    2    ${data_block_table}
    user checks list item contains    testid:footnotes    1
    ...    Applies to all data 1
    ...    ${data_block_table}
    user checks list item contains    testid:footnotes    2
    ...    Applies to all data 2
    ...    ${data_block_table}

Verify Test text accordion section contains correct text
    user opens accordion section    Test text    id:content
    ${section}=    user gets accordion section content element    Test text    id:content
    user waits until parent contains element    ${section}    xpath:.//p[text()="Some test text!"]
    user closes accordion section    Test text    id:content

Verify embedded dashboard accordion section contains dashboard
    user opens accordion section    Test embedded dashboard section    id:content
    ${section}=    user gets accordion section content element    Test embedded dashboard section    id:content
    user waits until parent contains element    ${section}    xpath:.//iframe[@title="Test embedded dashboard title"]

    select frame    xpath://iframe[@title="Test embedded dashboard title"]
    user waits until h1 is visible    Explore Education Statistics service    %{WAIT_SMALL}
    unselect frame

Return to Admin and create first amendment
    user navigates to admin dashboard    Bau1
    user creates amendment for release    ${PUBLICATION_NAME}    ${RELEASE_NAME}

Change the Release type
    user waits until page contains link    Edit release summary
    user clicks link    Edit release summary
    user waits until page does not contain loading spinner
    user waits until h2 is visible    Edit release summary
    user checks page contains radio    Experimental statistics
    user clicks radio    Experimental statistics
    user clicks button    Update release summary
    user checks page contains element    xpath://li/a[text()="Summary" and contains(@aria-current, 'page')]
    user verifies release summary    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} summary
    ...    Financial year
    ...    3000-01
    ...    UI test contact name
    ...    Experimental statistics

Navigate to data replacement page
    user clicks link    Data and files
    user waits until h2 is visible    Uploaded data files    %{WAIT_MEDIUM}
    user waits until page contains accordion section    Dates test subject
    user opens accordion section    Dates test subject

    ${section}=    user gets accordion section content element    Dates test subject
    user clicks link    Replace data    ${section}

    user waits until h2 is visible    Data file details
    user checks headed table body row contains    Subject title    Dates test subject
    user checks headed table body row contains    Data file    dates.csv
    user checks headed table body row contains    Metadata file    dates.meta.csv
    user checks headed table body row contains    Number of rows    118    wait=%{WAIT_SMALL}
    user checks headed table body row contains    Data file size    17 Kb    wait=%{WAIT_SMALL}
    user checks headed table body row contains    Status    Complete    wait=%{WAIT_LONG}

Upload replacement data
    user waits until h2 is visible    Upload replacement data    %{WAIT_MEDIUM}
    user chooses file    id:dataFileUploadForm-dataFile    ${FILES_DIR}dates-replacement.csv
    user chooses file    id:dataFileUploadForm-metadataFile    ${FILES_DIR}dates-replacement.meta.csv
    user clicks button    Upload data files

    user waits until page contains element    testid:Replacement Subject title
    user checks table column heading contains    1    1    Original file
    user checks table column heading contains    1    2    Replacement file

    user checks headed table body row cell contains    Subject title    1    Dates test subject
    user checks headed table body row cell contains    Data file    1    dates.csv
    user checks headed table body row cell contains    Metadata file    1    dates.meta.csv
    user checks headed table body row cell contains    Number of rows    1    118    wait=%{WAIT_SMALL}
    user checks headed table body row cell contains    Data file size    1    17 Kb    wait=%{WAIT_SMALL}
    user checks headed table body row cell contains    Status    1    Data replacement in progress    wait=%{WAIT_LONG}

    user checks headed table body row cell contains    Subject title    2    Dates test subject
    user checks headed table body row cell contains    Data file    2    dates-replacement.csv
    user checks headed table body row cell contains    Metadata file    2    dates-replacement.meta.csv
    user checks headed table body row cell contains    Number of rows    2    118    wait=%{WAIT_SMALL}
    user checks headed table body row cell contains    Data file size    2    17 Kb    wait=%{WAIT_SMALL}
    user checks headed table body row cell contains    Status    2    Complete    wait=%{WAIT_LONG}

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

    user waits until page contains accordion section    Dates test subject

    user checks summary list contains    Filename    dates-replacement.csv
    user checks summary list contains    Geographic levels    National
    user checks summary list contains    Time period    2020 Week 13 to 2021 Week 24

    ${editor}=    user gets data guidance data file content editor    Dates test subject
    user waits until element contains    ${editor}    Dates test subject test data guidance content

Update existing data guidance for amendment
    user enters text into element    id:dataGuidanceForm-content    Amended test data guidance content
    user enters text into data guidance data file content editor    Dates test subject
    ...    Amended Dates test subject test data guidance content

    user clicks button    Save guidance

Navigate to 'Footnotes' section for amendment
    user clicks link    Footnotes
    user waits until h2 is visible    Footnotes

Add a footnote to amendment
    user waits until page contains link    Create footnote
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote
    user clicks footnote subject radio    Dates test subject    Applies to all data
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
    user clicks link    Ancillary file uploads
    user waits until h2 is visible    Add file to release

    user enters text into element    label:Title    Test ancillary file 2
    user enters text into element    label:Summary    Test ancillary file 2 summary
    user chooses file    label:Upload file    ${FILES_DIR}test-file-2.txt
    user clicks button    Upload file

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
    user checks table cell contains    1    4    None

    user clicks link    Edit block    css:tbody > tr:first-child

    user waits until h2 is visible    ${DATABLOCK_NAME}
    user waits until h2 is visible    Data block details

    user clicks element    testid:wizardStep-4-goToButton
    user clicks button    Confirm

    user opens details dropdown    Date
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
    user waits until page does not contain loading spinner
    user clicks link    Chart

    user waits until page contains element    id:chartConfigurationForm-title    %{WAIT_SMALL}

    user checks radio is checked    Use table title
    user clicks radio    Set an alternative title
    user enters text into element    id:chartConfigurationForm-title    Amended sample title
    user checks input field contains    id:chartConfigurationForm-title    Amended sample title
    user enters text into element    id:chartConfigurationForm-alt    Amended sample alt text
    user checks textarea contains    id:chartConfigurationForm-alt    Amended sample alt text

    user clicks button    Save chart options
    user waits until page does not contain loading spinner
    user waits until page contains element    id:chartBuilderPreview
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
    ${accordion}=    user opens accordion section    Dates data block    id:releaseMainContent
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
    user opens accordion section    Test text    id:releaseMainContent
    user adds content to autosaving accordion section text block    Test text    1    Amended test text!
    ...    id:releaseMainContent

Update embedded dashboard title and url
    user chooses to update an embedded URL in editable accordion section
    ...    Test embedded dashboard section
    ...    id:releaseMainContent

    ${modal}=    user updates embedded URL details in modal
    ...    Amended Test embedded dashboard title
    ...    https://google.com
    ...    Edit embedded URL

    user clicks button    Save    ${modal}
    user waits until page contains    URL must be on a permitted domain

    user updates embedded URL details in modal
    ...    Amended Test embedded dashboard title
    ...    https://dfe-analytical-services.github.io/explore-education-statistics/tests/robot-tests
    ...    Edit embedded URL

    user clicks button    Save    ${modal}
    user waits until page does not contain    URL must be on a permitted domain
    user waits until modal is not visible    Edit embedded URL

    user waits until page contains element    xpath://iframe[@title="Amended Test embedded dashboard title"]
    select frame    xpath://iframe[@title="Amended Test embedded dashboard title"]
    user waits until h1 is visible    Explore Education Statistics Robot Framework tests    %{WAIT_SMALL}
    unselect frame

    user closes accordion section    Test embedded dashboard section    id:releaseMainContent

Add release note to first amendment
    user clicks button    Add note
    user enters text into element    id:createReleaseNoteForm-reason    Test release note one
    user clicks button    Save note
    ${date}=    get current datetime    ${DATE_FORMAT_MEDIUM}
    user waits until element contains    css:#releaseNotes li:nth-of-type(1) time    ${date}
    user waits until element contains    css:#releaseNotes li:nth-of-type(1) p    Test release note one

Create public prerelease access list for amendment
    user clicks link    Pre-release access
    user clicks link    Public access list
    user waits until h2 is visible    Public pre-release access list
    user creates public prerelease access list    Amended public access list

Approve amendment for scheduled release
    ${days_until_release}=    set variable    1
    ${publish_date_day}=    get current datetime    %-d    ${days_until_release}
    ${publish_date_month}=    get current datetime    %-m    ${days_until_release}
    ${publish_date_month_word}=    get current datetime    %B    ${days_until_release}
    ${publish_date_year}=    get current datetime    %Y    ${days_until_release}

    user approves release for scheduled publication
    ...    ${publish_date_day}
    ...    ${publish_date_month}
    ...    ${publish_date_year}
    ...    12
    ...    3001

    user waits for scheduled release to be published immediately

    ${EXPECTED_PUBLISHED_DATE}=    get current datetime    ${DATE_FORMAT_MEDIUM}
    set suite variable    ${EXPECTED_PUBLISHED_DATE}

Verify amendment is on Find Statistics page again
    user checks publication is on find statistics page    ${PUBLICATION_NAME}

Navigate to amendment release page
    user clicks link    ${PUBLICATION_NAME}
    user waits until h1 is visible    ${PUBLICATION_NAME}    %{WAIT_MEDIUM}
    user waits until page contains title caption    ${RELEASE_NAME}

    user checks url contains    %{PUBLIC_URL}/find-statistics/ui-tests-publish-release-and-amend-%{RUN_IDENTIFIER}

    user checks breadcrumb count should be    3
    user checks nth breadcrumb contains    1    Home
    user checks nth breadcrumb contains    2    Find statistics and data
    user checks nth breadcrumb contains    3    ${PUBLICATION_NAME}

Verify amendment is displayed as the latest release
    user checks page does not contain    View latest data:
    user checks page does not contain    View releases (1)

Verify amendment is published
    user checks summary list contains    Published    ${EXPECTED_PUBLISHED_DATE}
    user checks summary list contains    Next update    December 3001

Verify amendment free text key stat is updated
    user checks element count is x    testid:keyStat    1
    user checks key stat contents    1    Updated title    New stat    Updated trend
    user checks key stat guidance    1    Updated guidance title    Updated guidance text

Verify amendment files
    ${downloads}=    user gets testid element    data-and-files
    user checks element should contain    ${downloads}    Download all data (ZIP)
    ...    %{WAIT_SMALL}

    user opens accordion section    Additional supporting files
    ${other_files}=    user gets accordion section content element    Additional supporting files
    ${other_files_1}=    get child element    ${other_files}    css:li:nth-child(1)
    ${other_files_2}=    get child element    ${other_files}    css:li:nth-child(2)

    user waits until element contains link    ${other_files_1}    Test ancillary file 1 (txt, 12 B)
    user checks element should contain    ${other_files_1}    Test ancillary file 1 summary
    download file    link:Test ancillary file 1 (txt, 12 B)    test_ancillary_file_1.txt
    downloaded file should have first line    test_ancillary_file_1.txt    Test file 1

    user waits until element contains link    ${other_files_2}    Test ancillary file 2 (txt, 24 B)
    user checks element should contain    ${other_files_2}    Test ancillary file 2 summary
    download file    link:Test ancillary file 2 (txt, 24 B)    test_ancillary_file_2.txt
    downloaded file should have first line    test_ancillary_file_2.txt    Test file 2

Verify amendment public metadata guidance document
    user clicks link    Data guidance

    user checks breadcrumb count should be    4
    user checks nth breadcrumb contains    1    Home
    user checks nth breadcrumb contains    2    Find statistics and data
    user checks nth breadcrumb contains    3    ${PUBLICATION_NAME}
    user checks nth breadcrumb contains    4    Data guidance
    user waits until h2 is visible    Data guidance

    user waits until page contains title caption    ${RELEASE_NAME}
    user waits until h1 is visible    ${PUBLICATION_NAME}

    user waits until h2 is visible    Data guidance
    user waits until page contains    Amended test data guidance content

    user waits until page contains accordion section    Dates test subject
    user checks there are x accordion sections    1

    user opens accordion section    Dates test subject
    user checks summary list contains    Filename    dates-replacement.csv
    user checks summary list contains    Geographic levels    National
    user checks summary list contains    Time period    2020 Week 13 to 2021 Week 24
    user checks summary list contains    Content    Amended Dates test subject test data guidance content

    user opens details dropdown    Variable names and descriptions

    user checks table column heading contains    1    1    Variable name
    user checks table column heading contains    1    2    Variable description

    user checks table cell contains    1    1    children_attending
    user checks table cell contains    1    2    Number of children attending

    user checks table cell contains    6    1    date
    user checks table cell contains    6    2    Date

    user checks table cell contains    10    1    otherwise_vulnerable_children_attending
    user checks table cell contains    10    2    Number of otherwise vulnerable children attending

    user goes to release page via breadcrumb    ${PUBLICATION_NAME}    ${RELEASE_NAME}

Verify amendment public pre-release access list
    user clicks link    Pre-release access list

    user checks breadcrumb count should be    4
    user checks nth breadcrumb contains    1    Home
    user checks nth breadcrumb contains    2    Find statistics and data
    user checks nth breadcrumb contains    3    ${PUBLICATION_NAME}
    user checks nth breadcrumb contains    4    Pre-release access list

    user waits until page contains title caption    ${RELEASE_NAME}
    user waits until h1 is visible    ${PUBLICATION_NAME}

    user waits until h2 is visible    Pre-release access list
    user waits until page contains    Published ${EXPECTED_PUBLISHED_DATE}
    user waits until page contains    Amended public access list

Verify amendment accordions are correct
    user goes to release page via breadcrumb    ${PUBLICATION_NAME}    ${RELEASE_NAME}
    user checks accordion is in position    Dates data block    1    id:content
    user checks accordion is in position    Test text    2    id:content
    user checks accordion is in position    Test embedded dashboard section    3    id:content
    user checks accordion is in position    Experimental statistics    1    id:help-and-support-accordion
    user checks accordion is in position    Contact us    2    id:help-and-support-accordion

Verify amendment Dates data block accordion section
    user opens accordion section    Dates data block    id:content
    user scrolls to accordion section content    Dates data block    id:content
    ${section}=    user gets accordion section content element    Dates data block    id:content

    user checks chart title contains    ${section}    Amended sample title
    user checks infographic chart contains alt    ${section}    Amended sample alt text

    user clicks link by visible text    Table    ${section}
    user waits until parent contains element    ${section}
    ...    xpath:.//*[@data-testid="dataTableCaption" and text()="Amended dates table title"]
    user waits until parent contains element    ${section}    xpath:.//*[.="Source: Amended dates source"]

    user checks table column heading contains    1    1    2020 Week 13    ${section}
    user checks headed table body row cell contains    Number of open settings    1    23,000    ${section}
    user checks headed table body row cell contains    Proportion of settings open    1    2%    ${section}
    user checks headed table body row cell contains    Number of open settings    1    23,600    ${section}
    user checks headed table body row cell contains    Proportion of settings open    1    1%    ${section}
    user closes accordion section    Dates data block    id:content

Verify amendment Dates data block table has footnotes
    ${accordion}=    user opens accordion section    Dates data block    id:content
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

Verify amendment Test text accordion section contains correct text
    user opens accordion section    Test text    id:content
    ${section}=    user gets accordion section content element    Test text    id:content
    user checks element contains    ${section}    Amended test text!
    user closes accordion section    Test text    id:content

Verify amendment embedded dashboard accordion section is correct
    user opens accordion section    Test embedded dashboard section    id:content
    ${section}=    user gets accordion section content element    Test embedded dashboard section    id:content
    user checks element contains child element    ${section}
    ...    xpath:.//iframe[@title="Amended Test embedded dashboard title"]
    user closes accordion section    Test embedded dashboard section    id:content

Return to published release page in Admin
    user navigates to admin dashboard    Bau1
    user navigates to published release page from dashboard
    ...    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}

Override release published date to past date
    ${release_id}=    get release id from url
    ${published_override}=    Get Current Date    UTC    increment=-1000 days    result_format=datetime
    user updates release published date via api    ${release_id}    ${published_override}
    ${EXPECTED_PUBLISHED_DATE}=    format datetime    ${published_override}    ${DATE_FORMAT_MEDIUM}
    set suite variable    ${EXPECTED_PUBLISHED_DATE}

Verify published date on publication page is overriden with past date
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    ${row}=    user gets table row    ${RELEASE_NAME}    testid:publication-published-releases
    user checks element contains    ${row}    ${EXPECTED_PUBLISHED_DATE}

Verify public published date is overriden with past date
    user navigates to public frontend    ${PUBLIC_RELEASE_LINK}
    user waits until h1 is visible    ${PUBLICATION_NAME}
    user checks summary list contains    Published    ${EXPECTED_PUBLISHED_DATE}

Return to Admin and create second amendment
    user navigates to admin dashboard    Bau1
    user creates amendment for release    ${PUBLICATION_NAME}    ${RELEASE_NAME}

Add release note to second amendment
    user clicks link    Content
    user clicks button    Add note
    user enters text into element    id:createReleaseNoteForm-reason    Test release note two
    user clicks button    Save note

Approve release amendment for scheduled publication and update published date
    ${days_until_release}=    set variable    2
    ${publish_date_day}=    get current datetime    %-d    ${days_until_release}
    ${publish_date_month}=    get current datetime    %-m    ${days_until_release}
    ${publish_date_year}=    get current datetime    %Y    ${days_until_release}
    user approves release for scheduled publication
    ...    ${publish_date_day}
    ...    ${publish_date_month}
    ...    ${publish_date_year}
    ...    next_release_month=8
    ...    next_release_year=4001
    ...    update_amendment_published_date=${True}
    user waits for scheduled release to be published immediately
    ${EXPECTED_PUBLISHED_DATE}=    get current datetime    ${DATE_FORMAT_MEDIUM}
    set suite variable    ${EXPECTED_PUBLISHED_DATE}

Verify published date on publication page has been updated
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    ${row}=    user gets table row    ${RELEASE_NAME}    testid:publication-published-releases
    user checks element contains    ${row}    ${EXPECTED_PUBLISHED_DATE}

Navigate to amended public release
    user navigates to public frontend    ${PUBLIC_RELEASE_LINK}
    user waits until h1 is visible    ${PUBLICATION_NAME}

Verify public published date has been updated
    user checks summary list contains    Published    ${EXPECTED_PUBLISHED_DATE}

Validate next update date
    user checks summary list contains    Next update    August 4001
