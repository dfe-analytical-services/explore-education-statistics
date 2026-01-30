*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Resource            ../../libs/charts.robot
Resource            ../../libs/public-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData    ReleaseRedesign


*** Variables ***
${PUBLICATION_NAME}=    Publish amend and cancel %{RUN_IDENTIFIER}
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
    user uploads subject and waits until complete    Dates test subject    dates.csv    dates.meta.csv

Add data guidance
    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance
    user adds main data guidance content

    user waits until page contains element    id:dataGuidance-dataFiles
    user waits until page contains accordion section    Dates test subject

    user enters text into data guidance data file content editor    Dates test subject
    ...    Dates test subject test data guidance content
    user clicks button    Save guidance

Add ancillary file
    user clicks link    Supporting file uploads
    user waits until h2 is visible    Add file to release

    user enters text into element    id:ancillaryFileForm-title    Test ancillary file 1
    user enters text into element    id:ancillaryFileForm-summary    Test ancillary file 1 summary
    user chooses file    id:ancillaryFileForm-file    ${FILES_DIR}test-file-1.txt
    user clicks button    Add file

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
    user waits until page finishes loading
    user clicks link    Chart

    user clicks button    Choose an infographic as alternative
    user chooses file    id:chartConfigurationForm-file    ${FILES_DIR}test-infographic.png
    user checks radio is checked    Use table title
    user enters text into element    id:chartConfigurationForm-alt    Sample alt text
    user saves infographic configuration

    user waits until page contains    Chart preview
    user checks infographic chart contains alt    id:chartBuilderPreview    Sample alt text

Navigate to 'Content' page
    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}
    user waits until page contains button    Add a summary text block    %{WAIT_SMALL}

    user waits until page finishes loading
    user waits until page finishes loading

Add headline text block
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Add three accordion sections to release
    user clicks button    Add new section
    user changes accordion section title    1    Dates data block
    user clicks button    Add new section
    user changes accordion section title    2    Test text
    user clicks button    Add new section
    user changes accordion section title    3    Test embedded dashboard section

Add data block to first accordion section
    user scrolls to accordion section    Dates data block    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    ${datablock}=    user adds data block to editable accordion section    Dates data block    ${DATABLOCK_NAME}
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user scrolls to element    ${datablock}
    user waits until page contains element    ${datablock}    %{WAIT_SMALL}
    user waits until element contains infographic chart    ${datablock}
    user checks chart title contains    ${datablock}    Dates table title
    user checks infographic chart contains alt    ${datablock}    Sample alt text

Add test text to second accordion section
    user adds text block to editable accordion section    Test text    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user adds content to autosaving accordion section text block    Test text    1    Some test text !
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

Add embedded dashboard to third accordion section
    user chooses to embed a URL in editable accordion section
    ...    Test embedded dashboard section
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

    ${modal}=    user updates embedded URL details in modal
    ...    Test embedded dashboard title
    ...    https://dfe-analytical-services.github.io/analysts-guide

    user clicks button    Save    ${modal}
    user waits until modal is not visible    Embed a URL

    user waits until page contains element    xpath://iframe[@title="Test embedded dashboard title"]
    select frame    xpath://iframe[@title="Test embedded dashboard title"]
    user waits until h1 is visible    Analysts’ Guide    %{WAIT_SMALL}
    unselect frame

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

Publish the scheduled release
    user waits for scheduled release to be published immediately

    ${publish_date_day}=    get london day of month
    ${publish_date_month_word}=    get london month word
    ${publish_date_year}=    get london year
    set suite variable    ${EXPECTED_PUBLISHED_DATE}
    ...    ${publish_date_day} ${publish_date_month_word} ${publish_date_year}

    user waits for caches to expire

Get public release link
    ${PUBLIC_RELEASE_LINK}=    user gets url public release will be accessible at
    Set Suite Variable    ${PUBLIC_RELEASE_LINK}

Verify newly published release is public
    user navigates to public release page    ${PUBLIC_RELEASE_LINK}    ${PUBLICATION_NAME}    ${RELEASE_NAME}

Verify release URL
    user checks url contains    %{PUBLIC_URL}/find-statistics/publish-amend-and-cancel-%{RUN_IDENTIFIER}

Return to Admin and create amendment
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
    user clicks link    Data and files
    user waits until page contains data uploads table
    user uploads subject replacement    Dates test subject    dates-replacement.csv
    ...    dates-replacement.meta.csv
    user waits until page contains element    testid:Data file replacements table
    user confirms replacement upload    Dates test subject

Confirm data replacement via quick action
    user waits until page contains element    testid:Data file replacements table
    user checks table cell contains    1    1    Dates test subject    testid:Data file replacements table
    user checks table cell contains    1    2    17 Kb    testid:Data file replacements table
    user checks table cell contains    1    3    Ready    testid:Data file replacements table

    user clicks button    Confirm replacement
    user waits until page contains data uploads table
    user checks table cell contains    1    1    Dates test subject    testid:Data files table
    user checks table cell contains    1    2    17 Kb    testid:Data files table
    user checks table cell contains    1    3    Complete    testid:Data files table

Upload a replacement data set using the button
    user clicks button    Replace data
    user chooses file    id:dataFileReplacementUploadForm-dataFile    ${FILES_DIR}dates-replacement.csv
    user chooses file    id:dataFileReplacementUploadForm-metadataFile    ${FILES_DIR}dates-replacement.meta.csv
    user clicks element    testid:upload-replacement-files-button
    user waits until page contains element    testid:Data file replacements table
    user confirms replacement upload    Dates test subject

Confirm data replacement via quick action
    user waits until page contains element    testid:Data file replacements table
    user checks table cell contains    1    1    Dates test subject    testid:Data file replacements table
    user checks table cell contains    1    2    17 Kb    testid:Data file replacements table
    user checks table cell contains    1    3    Ready    testid:Data file replacements table

    user clicks button    Confirm replacement
    user waits until page contains data uploads table
    user checks table cell contains    1    1    Dates test subject    testid:Data files table
    user checks table cell contains    1    2    17 Kb    testid:Data files table
    user checks table cell contains    1    3    Complete    testid:Data files table

Edit ancillary file and replace data
    [Documentation]    EES-4315
    user clicks link    Data and files
    user clicks link    Supporting file uploads
    user waits until h2 is visible    Uploaded files

    user waits until page contains accordion section    Test ancillary file 1
    user opens accordion section    Test ancillary file 1    id:file-uploads

    ${section_1}=    user gets accordion section content element    Test ancillary file 1    id:file-uploads
    user clicks link containing text    Edit file    ${section_1}
    user waits until h2 is visible    Edit ancillary file
    user enters text into element    id:ancillaryFileForm-title    Replacement ancillary file
    user enters text into element    id:ancillaryFileForm-summary    Replacement ancillary file summary updated
    user chooses file    id:ancillaryFileForm-file    ${FILES_DIR}test-file-2.txt
    user clicks button    Save file

    user waits until page contains accordion section    Replacement ancillary file
    user opens accordion section    Replacement ancillary file    id:file-uploads

    ${section_1}=    user gets accordion section content element    Replacement ancillary file    id:file-uploads
    user checks summary list contains    Title    Replacement ancillary file    ${section_1}
    user checks summary list contains    Summary    Replacement ancillary file    ${section_1}
    user checks summary list contains    File    test-file-2.txt    ${section_1}
    user checks summary list contains    File size    24 B    ${section_1}

    user checks there are x accordion sections    1    id:file-uploads

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

    user clicks edit data block link    ${DATABLOCK_NAME}

    user checks page contains element    //*[@data-testid="Data set name-key" and contains(text(), "Data set name")]
    user checks page contains element
    ...    //*[@data-testid="Data set name-value" and contains(text(), "Dates test subject")]

    user clicks element    testid:wizardStep-4-goToButton

    user scrolls to element    id:filtersForm-filters
    user clicks button    Date
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
    ${accordion}=    user opens accordion section    Dates data block    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
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

Update embedded dashboard title
    user chooses to update an embedded URL in editable accordion section
    ...    Test embedded dashboard section
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

    ${modal}=    user updates embedded URL details in modal
    ...    Amended Test embedded dashboard title
    ...    https://dfe-analytical-services.github.io/dfeshiny
    ...    Edit embedded URL

    user clicks button    Save    ${modal}
    user waits until modal is not visible    Edit embedded URL

    user waits until page contains element    xpath://iframe[@title="Amended Test embedded dashboard title"]
    select frame    xpath://iframe[@title="Amended Test embedded dashboard title"]
    user waits until h1 is visible    dfeshiny    %{WAIT_SMALL}
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

    user waits until h3 is visible    Published releases (1 of 1)    %{WAIT_SMALL}
    user checks page contains element    testid:publication-published-releases
    user checks table body has x rows    1    testid:publication-published-releases
    table cell should contain    testid:publication-published-releases    1    1    Release period
    table cell should contain    testid:publication-published-releases    1    2    Status
    table cell should contain    testid:publication-published-releases    2    1    Financial year 3000-01
    table cell should contain    testid:publication-published-releases    2    2    Published

Revisit the Release after the cancellation to double check it remains unaffected
    ${ROW}=    user gets table row    ${RELEASE_NAME}    testid:publication-published-releases
    user clicks element    xpath://*[text()="View"]    ${ROW}
    user waits until h2 is visible    Release summary
    user verifies release summary    Financial year
    ...    3000-01
    ...    Accredited official statistics

Verify that the Data and Files are unchanged
    user clicks link    Data and files
    user waits until page contains data uploads table
    user checks table cell contains    1    1    Dates test subject    testid:Data files table
    user checks table cell contains    1    2    17 Kb    testid:Data files table
    user checks table cell contains    1    3    Complete    testid:Data files table

    user clicks button in table cell    1    4    View details    testid:Data files table
    user waits until h2 is visible    Data file details

    user checks summary list contains    Title    Dates test subject    testid:Data file details
    user checks summary list contains    Data file    dates.csv    testid:Data file details
    user checks summary list contains    Meta file    dates.meta.csv    testid:Data file details
    user checks summary list contains    Size    17 Kb    testid:Data file details
    user checks summary list contains    Number of rows    118    testid:Data file details
    user checks summary list contains    Status    Complete    testid:Data file details

    user clicks button    Close

Verify that the ancillary file is unchanged
    user clicks link    Data and files
    user clicks link    Supporting file uploads
    user waits until h2 is visible    Uploaded files
    user waits until page contains accordion section    Test ancillary file 1

    user opens accordion section    Test ancillary file 1    id:file-uploads

    ${section_1}=    user gets accordion section content element    Test ancillary file 1    id:file-uploads
    user checks summary list contains    Title    Test ancillary file 1    ${section_1}
    user checks summary list contains    Summary    Test ancillary file 1 summary    ${section_1}
    user checks summary list contains    File    test-file-1.txt    ${section_1}
    user checks summary list contains    File size    12 B    ${section_1}

    user checks there are x accordion sections    1    id:file-uploads

Verify that the footnotes are unchanged
    user clicks link    Footnotes
    user waits until h2 is visible    Footnotes
    user waits until page contains element    testid:Footnote - Applies to all data 1
    user waits until page contains element    testid:Footnote - Applies to all data 2
    user waits until page does not contain element    testid:Footnote - Applies to all data 3

Verify that release content is unchanged
    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}
    user waits until page finishes loading
    user waits until page finishes loading

    user waits until parent contains element    testid:home-content    css:[data-testid="home-content-section"]
    ...    count=3
    user checks section is in position    Dates data block    1    testid:home-content
    user checks section is in position    Test text    2    testid:home-content
    user checks section is in position    Test embedded dashboard section    3    testid:home-content

Verify that the Dates data block accordion is unchanged
    user scrolls to element    id:heading-dates-data-block
    ${section}=    Get WebElement    id:section-dates-data-block

    user checks chart title contains    ${section}    Dates table title
    user checks infographic chart contains alt    ${section}    Sample alt text

    user clicks link containing text    Table    ${section}
    user waits until parent contains element    ${section}
    ...    xpath:.//*[@data-testid="dataTableCaption" and text()="Dates table title"]
    user waits until parent contains element    ${section}    xpath:.//*[.="Source: Dates source"]
    user checks table column heading contains    1    1    2020 Week 13    ${section}
    user checks headed table body row cell contains    Number of open settings    1    22,900    ${section}
    user checks headed table body row cell contains    Proportion of settings open    1    1%    ${section}

    ${data_block_table}=    user gets data block table from parent    ${DATABLOCK_NAME}    ${section}

    user checks list has x items    testid:footnotes    2    ${data_block_table}
    user checks list item contains    testid:footnotes    1
    ...    Applies to all data 1
    ...    ${data_block_table}
    user checks list item contains    testid:footnotes    2
    ...    Applies to all data 2
    ...    ${data_block_table}

Verify that the Test text accordion is unchanged
    ${section}=    Get WebElement    id:section-test-text
    user waits until parent contains element    ${section}    xpath:.//p[text()="Some test text !"]

Verify that the Embedded URL accordion section is unchanged
    ${section}=    Get WebElement    id:section-test-embedded-dashboard-section

    user waits until parent contains element    ${section}    xpath:.//iframe[@title="Test embedded dashboard title"]

    select frame    xpath://iframe[@title="Test embedded dashboard title"]
    user waits until h1 is visible    Analysts’ Guide    %{WAIT_SMALL}
    unselect frame
