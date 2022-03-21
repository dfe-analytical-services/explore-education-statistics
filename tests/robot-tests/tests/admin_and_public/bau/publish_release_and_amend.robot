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
${PUBLICATION_NAME}=    UI tests - publish release %{RUN_IDENTIFIER}
${RELEASE_NAME}=        Financial Year 3000-01
${DATABLOCK_NAME}=      Dates data block name

*** Test Cases ***
Create new publication for "UI tests topic" topic
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user create test release via api    ${PUBLICATION_ID}    FY    3000

Go to "Release summary" page
    user navigates to editable release summary from admin dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME} (not Live)

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

Add two accordion sections to release
    user waits for page to finish loading
    user waits until page does not contain loading spinner
    user clicks button    Add new section
    user changes accordion section title    1    Dates data block
    user clicks button    Add new section
    user changes accordion section title    2    Test text

Add data block to first accordion section
    user adds data block to editable accordion section    Dates data block    ${DATABLOCK_NAME}
    ...    css:#releaseMainContent
    ${datablock}=    set variable    xpath://*[@data-testid="Data block - ${DATABLOCK_NAME}"]
    user waits until page contains element    ${datablock}    %{WAIT_SMALL}
    user waits until element contains infographic chart    ${datablock}
    user checks chart title contains    ${datablock}    Dates table title
    user checks infographic chart contains alt    ${datablock}    Sample alt text

Add test text to second accordion section
    user adds text block to editable accordion section    Test text    css:#releaseMainContent
    user adds content to autosaving accordion section text block    Test text    1    Some test text!
    ...    css:#releaseMainContent

Add public prerelease access list
    user clicks link    Pre-release access
    user creates public prerelease access list    Test public access list

Approve release
    user approves release for scheduled release    2    12    3001

Verify release is scheduled
    user checks summary list contains    Current status    Approved
    user checks summary list contains    Scheduled release
    ...    ${PUBLISH_DATE_DAY} ${PUBLISH_DATE_MONTH_WORD} ${PUBLISH_DATE_YEAR}
    user checks summary list contains    Next release expected    December 3001

Approve release for immediate publication
    user approves original release for immediate publication

User goes to public Find Statistics page
    user navigates to find statistics page on public frontend

Verify newly published release is on Find Statistics page
    user waits until page contains accordion section    %{TEST_THEME_NAME}    %{WAIT_MEDIUM}
    user opens accordion section    %{TEST_THEME_NAME}
    user waits until accordion section contains text    %{TEST_THEME_NAME}    %{TEST_TOPIC_NAME}    %{WAIT_MEDIUM}

    user opens details dropdown    %{TEST_TOPIC_NAME}
    user waits until details dropdown contains publication    %{TEST_TOPIC_NAME}    ${PUBLICATION_NAME}
    user checks publication bullet contains link    ${PUBLICATION_NAME}    View statistics and data
    user checks publication bullet contains link    ${PUBLICATION_NAME}    Create your own tables
    user checks publication bullet does not contain link    ${PUBLICATION_NAME}    Statistics at DfE

Navigate to newly published release page
    user clicks element    testid:View stats link for ${PUBLICATION_NAME}
    user waits until h1 is visible    ${PUBLICATION_NAME}    %{WAIT_MEDIUM}

Verify release URL and page caption
    user checks url contains    %{PUBLIC_URL}/find-statistics/ui-tests-publish-release-%{RUN_IDENTIFIER}
    user waits until page contains title caption    ${RELEASE_NAME}

Verify publish and update dates
    ${PUBLISH_DATE_DAY}=    get current datetime    %-d
    ${PUBLISH_DATE_MONTH_WORD}=    get current datetime    %B
    ${PUBLISH_DATE_YEAR}=    get current datetime    %Y
    set suite variable    ${PUBLISH_DATE_DAY}
    set suite variable    ${PUBLISH_DATE_MONTH_WORD}
    set suite variable    ${PUBLISH_DATE_YEAR}
    user checks summary list contains    Published
    ...    ${PUBLISH_DATE_DAY} ${PUBLISH_DATE_MONTH_WORD} ${PUBLISH_DATE_YEAR}
    user checks summary list contains    Next update    December 3001

Verify release associated files
    user opens accordion section    Explore data and files
    ${downloads}=    user gets accordion section content element    Explore data and files
    user waits until page contains element    ${downloads}    %{WAIT_SMALL}

    user checks element should contain    ${downloads}    Download all data
    ...    %{WAIT_SMALL}
    user checks element should contain    ${downloads}
    ...    All data used in this release is available as open data for download
    user checks element should contain    ${downloads}
    ...    You can view featured tables that we have built for you, or create your own tables from the open data using our table tool

    user checks element should contain    ${downloads}
    ...    Browse and download individual open data files from this release in our data catalogue
    user checks element should contain    ${downloads}
    ...    Learn more about the data files used in this release using our online guidance

    user opens details dropdown    List of all supporting files
    ${other_files}=    user gets details content element    List of all supporting files
    ${other_files_1}=    get child element    ${other_files}    css:li:nth-child(1)

    user waits until element contains link    ${other_files_1}    Test ancillary file 1
    user opens details dropdown    More details    ${other_files_1}
    ${other_files_1_details}=    user gets details content element    More details    ${other_files_1}
    user checks element should contain    ${other_files_1_details}    Test ancillary file 1 summary

    download file    link:Test ancillary file 1    test_ancillary_file_1.txt
    downloaded file should have first line    test_ancillary_file_1.txt    Test file 1

Verify public metadata guidance document
    user opens accordion section    Explore data and files
    user waits until h3 is visible    Open data
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

    user checks results table cell contains    1    1    children_attending    css:table[data-testid="Variables"]
    user checks results table cell contains    1    2    Number of children attending
    ...    css:table[data-testid="Variables"]

    user checks results table cell contains    6    1    date    css:table[data-testid="Variables"]
    user checks results table cell contains    6    2    Date    css:table[data-testid="Variables"]

    user checks results table cell contains    10    1    otherwise_vulnerable_children_attending
    ...    css:table[data-testid="Variables"]
    user checks results table cell contains    10    2    Number of otherwise vulnerable children attending
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
    user waits until page contains    Published ${PUBLISH_DATE_DAY} ${PUBLISH_DATE_MONTH_WORD} ${PUBLISH_DATE_YEAR}
    user waits until page contains    Test public access list

Verify accordions are correct
    user goes to release page via breadcrumb    ${PUBLICATION_NAME}    ${RELEASE_NAME}
    user checks accordion is in position    Dates data block    1    id:content
    user checks accordion is in position    Test text    2    id:content

    user checks accordion is in position    National statistics    1    id:help-and-support
    user checks accordion is in position    Contact us    2    id:help-and-support

Verify Dates data block accordion section
    user opens accordion section    Dates data block    id:content
    user scrolls to accordion section content    Dates data block    id:content
    ${section}=    user gets accordion section content element    Dates data block    id:content

    user checks chart title contains    ${section}    Dates table title
    user checks infographic chart contains alt    ${section}    Sample alt text

    user clicks link    Table    ${section}
    user waits until parent contains element    ${section}
    ...    xpath:.//*[@data-testid="dataTableCaption" and text()="Dates table title"]
    user waits until parent contains element    ${section}    xpath:.//*[.="Source: Dates source"]

    user checks table column heading contains    1    1    2020 Week 13    ${section}
    user checks headed table body row cell contains    Number of open settings    1    22,900    ${section}
    user checks headed table body row cell contains    Proportion of settings open    1    1%    ${section}

    user closes accordion section    Dates data block    id:content

Verify Test text accordion section contains correct text
    user opens accordion section    Test text    id:content
    ${section}=    user gets accordion section content element    Test text    id:content
    user waits until parent contains element    ${section}    xpath:.//p[text()="Some test text!"]
    user closes accordion section    Test text    id:content
    user clicks link    Summary

Return to Admin and Create amendment
    user navigates to admin dashboard    Bau1
    user creates amendment for release    ${PUBLICATION_NAME}    ${RELEASE_NAME}    (Live - Latest release)

Change the Release type
    user waits until page contains link    Edit release summary
    user clicks link    Edit release summary
    user waits until h2 is visible    Edit release summary
    user checks page contains radio    Experimental statistics
    user clicks radio    Experimental statistics
    user clicks button    Update release summary
    user checks page contains element    xpath://li/a[text()="Summary" and contains(@aria-current, 'page')]
    user verifies release summary    ${PUBLICATION_NAME}    Financial Year    3000-01    UI test contact name
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
    user checks headed table body row contains    Number of rows    119
    user checks headed table body row contains    Data file size    17 Kb
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
    user checks headed table body row cell contains    Number of rows    1    119
    user checks headed table body row cell contains    Data file size    1    17 Kb
    user checks headed table body row cell contains    Status    1    Data replacement in progress    wait=%{WAIT_LONG}

    user checks headed table body row cell contains    Subject title    2    Dates test subject
    user checks headed table body row cell contains    Data file    2    dates-replacement.csv
    user checks headed table body row cell contains    Metadata file    2    dates-replacement.meta.csv
    user checks headed table body row cell contains    Number of rows    2    119
    user checks headed table body row cell contains    Data file size    2    17 Kb
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
    user enters text into element    id:dataGuidanceForm-content    Updated test data guidance content
    user enters text into data guidance data file content editor    Dates test subject
    ...    Updated Dates test subject test data guidance content

    user clicks button    Save guidance

# TODO luke: Add footnotes

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
    user checks results table cell contains    1    1    ${DATABLOCK_NAME}
    user checks results table cell contains    1    2    Yes
    user checks results table cell contains    1    3    Yes
    user checks results table cell contains    1    4    None

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
    user enters text into element    id:dataBlockDetailsForm-heading    Updated dates table title
    user enters text into element    id:dataBlockDetailsForm-source    Updated dates source

    user clicks button    Save data block
    user waits until page contains button    Delete this data block

Update data block chart for amendment
    user waits until page contains link    Chart
    user waits until page does not contain loading spinner
    user clicks link    Chart

    user waits until page contains element    id:chartConfigurationForm-title

    user checks radio is checked    Use table title
    user clicks radio    Set an alternative title
    user enters text into element    id:chartConfigurationForm-title    Updated sample title
    user checks input field contains    id:chartConfigurationForm-title    Updated sample title
    user enters text into element    id:chartConfigurationForm-alt    Updated sample alt text
    user checks textarea contains    id:chartConfigurationForm-alt    Updated sample alt text

    user clicks button    Save chart options
    user waits until page does not contain loading spinner
    user waits until page contains element    id:chartBuilderPreview
    user checks infographic chart contains alt    id:chartBuilderPreview    Updated sample alt text

Navigate to 'Content' page for amendment
    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}
    user waits until page contains button    Add a summary text block

Update second accordion section text for amendment
    user opens accordion section    Test text    css:#releaseMainContent
    user adds content to autosaving accordion section text block    Test text    1    Updated test text!
    ...    css:#releaseMainContent

Add release note to amendment
    user clicks button    Add note
    user enters text into element    id:createReleaseNoteForm-reason    Test release note one
    user clicks button    Save note
    ${date}=    get current datetime    %-d %B %Y
    user waits until element contains    css:#releaseNotes li:nth-of-type(1) time    ${date}
    user waits until element contains    css:#releaseNotes li:nth-of-type(1) p    Test release note one

Update public prerelease access list for amendment
    user clicks link    Pre-release access
    user updates public prerelease access list    Updated public access list

Approve amendment for immediate release
    user clicks link    Sign off
    user approves amended release for immediate publication

Go back to public find-statistics page
    user navigates to public frontend    %{PUBLIC_URL}/find-statistics
    user waits until h1 is visible    Find statistics and data
    user waits for page to finish loading

Verify amendment is on Find Statistics page again
    user waits until page contains accordion section    %{TEST_THEME_NAME}
    user opens accordion section    %{TEST_THEME_NAME}
    user waits until accordion section contains text    %{TEST_THEME_NAME}    %{TEST_TOPIC_NAME}

    user opens details dropdown    %{TEST_TOPIC_NAME}
    user waits until details dropdown contains publication    %{TEST_TOPIC_NAME}    ${PUBLICATION_NAME}
    ...    Experimental statistics
    user checks publication bullet contains link    ${PUBLICATION_NAME}    View statistics and data
    user checks publication bullet contains link    ${PUBLICATION_NAME}    Create your own tables
    user checks publication bullet does not contain link    ${PUBLICATION_NAME}    Statistics at DfE

Navigate to amendment release page
    user clicks element    testid:View stats link for ${PUBLICATION_NAME}
    user waits until h1 is visible    ${PUBLICATION_NAME}    %{WAIT_MEDIUM}
    user waits until page contains title caption    ${RELEASE_NAME}

    user checks url contains    %{PUBLIC_URL}/find-statistics/ui-tests-publish-release-%{RUN_IDENTIFIER}

    user checks breadcrumb count should be    3
    user checks nth breadcrumb contains    1    Home
    user checks nth breadcrumb contains    2    Find statistics and data
    user checks nth breadcrumb contains    3    ${PUBLICATION_NAME}

Verify amendment is displayed as the latest release
    [Documentation]    EES-1301
    [Tags]    Failing
    user checks page does not contain    View latest data:
    user checks page does not contain    See other releases (1)

Verify amendment is published
    user checks summary list contains    Published
    ...    ${PUBLISH_DATE_DAY} ${PUBLISH_DATE_MONTH_WORD} ${PUBLISH_DATE_YEAR}
    user checks summary list contains    Next update    December 3001    # TODO: Check Next update date can be updated

Verify amendment files
    user opens accordion section    Explore data and files
    ${downloads}=    user gets accordion section content element    Explore data and files
    user checks element should contain    ${downloads}    Download all data
    ...    %{WAIT_SMALL}

    user opens details dropdown    List of all supporting files
    ${other_files}=    user gets details content element    List of all supporting files
    ${other_files_1}=    get child element    ${other_files}    css:li:nth-child(1)
    ${other_files_2}=    get child element    ${other_files}    css:li:nth-child(2)

    user waits until element contains link    ${other_files_1}    Test ancillary file 1
    user opens details dropdown    More details    ${other_files_1}
    ${other_files_1_details}=    user gets details content element    More details    ${other_files_1}
    user checks element should contain    ${other_files_1_details}    Test ancillary file 1 summary

    download file    link:Test ancillary file 1    test_ancillary_file_1.txt
    downloaded file should have first line    test_ancillary_file_1.txt    Test file 1

    user waits until element contains link    ${other_files_2}    Test ancillary file 2
    user opens details dropdown    More details    ${other_files_2}
    ${other_files_2_details}=    user gets details content element    More details    ${other_files_2}
    user checks element should contain    ${other_files_2_details}    Test ancillary file 2 summary

    download file    link:Test ancillary file 2    test_ancillary_file_2.txt
    downloaded file should have first line    test_ancillary_file_2.txt    Test file 2

Verify amendment public metadata guidance document
    user opens accordion section    Explore data and files
    user waits until h3 is visible    Open data
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
    user waits until page contains    Updated test data guidance content

    user waits until page contains accordion section    Dates test subject
    user checks there are x accordion sections    1

    user opens accordion section    Dates test subject
    user checks summary list contains    Filename    dates-replacement.csv
    user checks summary list contains    Geographic levels    National
    user checks summary list contains    Time period    2020 Week 13 to 2021 Week 24
    user checks summary list contains    Content    Updated Dates test subject test data guidance content

    user opens details dropdown    Variable names and descriptions

    user checks table column heading contains    1    1    Variable name
    user checks table column heading contains    1    2    Variable description

    user checks results table cell contains    1    1    children_attending
    user checks results table cell contains    1    2    Number of children attending

    user checks results table cell contains    6    1    date
    user checks results table cell contains    6    2    Date

    user checks results table cell contains    10    1    otherwise_vulnerable_children_attending
    user checks results table cell contains    10    2    Number of otherwise vulnerable children attending

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
    user waits until page contains    Published ${PUBLISH_DATE_DAY} ${PUBLISH_DATE_MONTH_WORD} ${PUBLISH_DATE_YEAR}
    user waits until page contains    Updated public access list

Verify amendment accordions are correct
    user goes to release page via breadcrumb    ${PUBLICATION_NAME}    ${RELEASE_NAME}
    user checks accordion is in position    Dates data block    1    id:content
    user checks accordion is in position    Test text    2    id:content

    user checks accordion is in position    Experimental statistics    1    id:help-and-support
    user checks accordion is in position    Contact us    2    id:help-and-support

Verify amendment Dates data block accordion section
    user opens accordion section    Dates data block    id:content
    user scrolls to accordion section content    Dates data block    id:content
    ${section}=    user gets accordion section content element    Dates data block    id:content

    user checks chart title contains    ${section}    Updated sample title
    user checks infographic chart contains alt    ${section}    Updated sample alt text

    user clicks link    Table    ${section}
    user waits until parent contains element    ${section}
    ...    xpath:.//*[@data-testid="dataTableCaption" and text()="Updated dates table title"]
    user waits until parent contains element    ${section}    xpath:.//*[.="Source: Updated dates source"]

    user checks table column heading contains    1    1    2020 Week 13    ${section}
    user checks headed table body row cell contains    Number of open settings    1    23,000    ${section}
    user checks headed table body row cell contains    Proportion of settings open    1    2%    ${section}
    user checks headed table body row cell contains    Number of open settings    1    23,600    ${section}
    user checks headed table body row cell contains    Proportion of settings open    1    1%    ${section}
    user closes accordion section    Dates data block    id:content

Verify amendment Test text accordion section contains correct text
    user opens accordion section    Test text    id:content
    ${section}=    user gets accordion section content element    Test text    id:content
    user closes accordion section    Test text    id:content
