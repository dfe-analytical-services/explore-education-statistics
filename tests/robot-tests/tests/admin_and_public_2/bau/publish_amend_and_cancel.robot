*** Settings ***
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
${PUBLICATION_NAME}=    UI tests - publish amend and cancel %{RUN_IDENTIFIER}
${RELEASE_NAME}=        Financial year 3000-01
${DATABLOCK_NAME}=      Dates data block name


*** Test Cases ***
Create new publication and release via API
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    FY    3000

Go to "Release summary" page
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}

Upload subject
    user uploads subject    Dates test subject    dates.csv    dates.meta.csv

Add data guidance
    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance

    user waits until page contains element    id:dataGuidanceForm-content
    user waits until page contains element    id:dataGuidance-dataFiles
    user enters text into element    id:dataGuidanceForm-content    Test data guidance content
    user waits until page contains accordion section    Dates test subject

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

Add test text to second accordion section
    user adds text block to editable accordion section    Test text    id:releaseMainContent
    user adds content to autosaving accordion section text block    Test text    1    Some test text !
    ...    id:releaseMainContent

Add embedded dashboard to third accordion section
    user chooses to embed a URL in editable accordion section
    ...    Test embedded dashboard section
    ...    id:releaseMainContent

    ${modal}=    user updates embedded URL details in modal
    ...    Test embedded dashboard title
    ...    https://dfe-analytical-services.github.io/explore-education-statistics

    user clicks button    Save    ${modal}
    user waits until modal is not visible    Embed a URL

    user waits until page contains element    xpath://iframe[@title="Test embedded dashboard title"]
    select frame    xpath://iframe[@title="Test embedded dashboard title"]
    user waits until h1 is visible    Explore Education Statistics service    %{WAIT_SMALL}
    unselect frame

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

Publish the scheduled release
    user waits for scheduled release to be published immediately

    ${publish_date_day}=    get current datetime    %-d
    ${publish_date_month_word}=    get current datetime    %B
    ${publish_date_year}=    get current datetime    %Y
    set suite variable    ${EXPECTED_PUBLISHED_DATE}
    ...    ${publish_date_day} ${publish_date_month_word} ${publish_date_year}

Verify newly published release is on Find Statistics page
    user checks publication is on find statistics page    ${PUBLICATION_NAME}

Navigate to newly published release page
    user clicks link    ${PUBLICATION_NAME}
    user waits until h1 is visible    ${PUBLICATION_NAME}    %{WAIT_SMALL}

Verify release URL and page caption
    user checks url contains    %{PUBLIC_URL}/find-statistics/ui-tests-publish-amend-and-cancel-%{RUN_IDENTIFIER}
    user waits until page contains title caption    ${RELEASE_NAME}

Return to Admin and create amendment
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
    user checks headed table body row contains    Status    Complete    wait=%{WAIT_LONG}

Upload replacement data
    user waits until h2 is visible    Upload replacement data    %{WAIT_MEDIUM}
    user chooses file    id:dataFileUploadForm-dataFile    ${FILES_DIR}dates-replacement.csv
    user chooses file    id:dataFileUploadForm-metadataFile    ${FILES_DIR}dates-replacement.meta.csv
    user clicks button    Upload data files

    user waits until page contains element    testid:Replacement Subject title
    user checks table column heading contains    1    1    Original file
    user checks table column heading contains    1    2    Replacement file

Confirm data replacement
    user waits until page contains    Data blocks: OK
    user waits until page contains    Footnotes: OK
    user clicks button    Confirm data replacement
    user waits until h2 is visible    Data replacement complete

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

Navigate to Data blocks page
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

Save data block for amendment
    user enters text into element    id:dataBlockDetailsForm-name    ${DATABLOCK_NAME}
    user enters text into element    id:dataBlockDetailsForm-heading    Amended dates table title
    user enters text into element    id:dataBlockDetailsForm-source    Amended dates source

    user clicks button    Save data block
    user waits until page contains button    Delete this data block

Navigate to 'Content' page for amendment
    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}
    user waits until page contains button    Add a summary text block

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

Update embedded dashboard title
    user chooses to update an embedded URL in editable accordion section
    ...    Test embedded dashboard section
    ...    id:releaseMainContent

    ${modal}=    user updates embedded URL details in modal
    ...    Amended Test embedded dashboard title
    ...    https://dfe-analytical-services.github.io/explore-education-statistics/tests/robot-tests
    ...    Edit embedded URL

    user clicks button    Save    ${modal}
    user waits until modal is not visible    Edit embedded URL

    user waits until page contains element    xpath://iframe[@title="Amended Test embedded dashboard title"]
    select frame    xpath://iframe[@title="Amended Test embedded dashboard title"]
    user waits until h1 is visible    Explore Education Statistics Robot Framework tests    %{WAIT_SMALL}
    unselect frame

Cancel the release amendment
    [Documentation]    EES-3399
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}

    ${ROW}=    user gets table row    ${RELEASE_NAME}    testid:publication-draft-releases
    user checks element contains    ${ROW}    Draft Amendment
    user clicks button    Cancel amendment    ${ROW}

    ${modal}=    user waits until modal is visible    Confirm you want to cancel this amended release
    user clicks button    Confirm
    user waits until modal is not visible    Confirm you want to cancel this amended release

Revisit the Release after the cancellation to double check it remains unaffected
    ${ROW}=    user gets table row    ${RELEASE_NAME}    testid:publication-published-releases
    user clicks element    xpath://*[text()="View"]    ${ROW}
    user waits until h2 is visible    Release summary
    user verifies release summary    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} summary
    ...    Financial year
    ...    3000-01
    ...    UI test contact name
    ...    National statistics

Verify that the Data and Files are unchanged
    user clicks link    Data and files
    user waits until h2 is visible    Uploaded data files    %{WAIT_MEDIUM}
    user waits until page contains accordion section    Dates test subject
    user opens accordion section    Dates test subject
    ${section}=    user gets accordion section content element    Dates test subject
    user checks headed table body row contains    Subject title    Dates test subject
    user checks headed table body row contains    Data file    dates.csv
    user checks headed table body row contains    Metadata file    dates.meta.csv
    user checks headed table body row contains    Number of rows    118    wait=%{WAIT_SMALL}
    user checks headed table body row contains    Data file size    17 Kb    wait=%{WAIT_SMALL}
    user checks headed table body row contains    Status    Complete    wait=%{WAIT_LONG}

Verify that the footnotes are unchanged
    user clicks link    Footnotes
    user waits until h2 is visible    Footnotes
    user waits until page contains element    testid:Footnote - Applies to all data 1
    user waits until page contains element    testid:Footnote - Applies to all data 2
    user waits until page does not contain element    testid:Footnote - Applies to all data 3

Verify that release content is unchanged
    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}
    user waits for page to finish loading
    user waits until page does not contain loading spinner

    user checks there are x accordion sections    3    id:releaseMainContent
    user checks accordion is in position    Dates data block    1    id:releaseMainContent
    user checks accordion is in position    Test text    2    id:releaseMainContent
    user checks accordion is in position    Test embedded dashboard section    3    id:releaseMainContent

Verify that the Dates data block accordion is unchanged
    user scrolls to accordion section content    Dates data block    id:releaseMainContent
    user opens accordion section    Dates data block    id:releaseMainContent
    ${section}=    user gets accordion section content element    Dates data block    id:releaseMainContent

    user checks chart title contains    ${section}    Dates table title
    user checks infographic chart contains alt    ${section}    Sample alt text

    user clicks link by visible text    Table    ${section}
    user waits until parent contains element    ${section}
    ...    xpath:.//*[@data-testid="dataTableCaption" and text()="Dates table title"]
    user waits until parent contains element    ${section}    xpath:.//*[.="Source: Dates source"]
    user checks table column heading contains    1    1    2020 Week 13    ${section}
    user checks headed table body row cell contains    Number of open settings    1    22,900    ${section}
    user checks headed table body row cell contains    Proportion of settings open    1    1%    ${section}
    user closes accordion section    Dates data block    id:releaseMainContent

Verify that the Dates data block table footnotes are unchanged
    ${accordion}=    user opens accordion section    Dates data block    id:releaseMainContent
    ${data_block_table}=    user gets data block table from parent    ${DATABLOCK_NAME}    ${accordion}

    user checks list has x items    testid:footnotes    2    ${data_block_table}
    user checks list item contains    testid:footnotes    1
    ...    Applies to all data 1
    ...    ${data_block_table}
    user checks list item contains    testid:footnotes    2
    ...    Applies to all data 2
    ...    ${data_block_table}

Verify that the Test text accordion is unchanged
    user opens accordion section    Test text    id:releaseMainContent
    ${section}=    user gets accordion section content element    Test text    id:releaseMainContent
    user waits until parent contains element    ${section}    xpath:.//p[text()="Some test text !"]
    user closes accordion section    Test text    id:releaseMainContent

Verify that the Embedded URL accordion section is unchanged
    user opens accordion section    Test embedded dashboard section    id:releaseMainContent
    ${section}=    user gets accordion section content element    Test embedded dashboard section
    ...    id:releaseMainContent
    user waits until parent contains element    ${section}    xpath:.//iframe[@title="Test embedded dashboard title"]

    select frame    xpath://iframe[@title="Test embedded dashboard title"]
    user waits until h1 is visible    Explore Education Statistics service    %{WAIT_SMALL}
    unselect frame
