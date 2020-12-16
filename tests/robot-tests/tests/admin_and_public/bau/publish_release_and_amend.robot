*** Settings ***
Resource    ../../libs/admin-common.robot
Resource    ../../libs/charts.robot
Resource    ../../libs/public-common.robot
Library     ../../libs/admin_api.py

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${RUN_IDENTIFIER}    %{RUN_IDENTIFIER}
${THEME_NAME}        %{TEST_THEME_NAME}
${TOPIC_NAME}        %{TEST_TOPIC_NAME}
${PUBLICATION_NAME}  UI tests - publish release %{RUN_IDENTIFIER}
${RELEASE_NAME}      Financial Year 3000-01
${DATABLOCK_NAME}    Dates data block name

*** Test Cases ***
Create new publication for "UI tests topic" topic
    [Tags]  HappyPath
    ${PUBLICATION_ID}=  user creates test publication via api   ${PUBLICATION_NAME}
    user create test release via api  ${PUBLICATION_ID}   FY   3000

Go to "Release summary" page
    [Tags]  HappyPath
    user navigates to release summary from admin dashboard  ${PUBLICATION_NAME}   ${RELEASE_NAME} (not Live)

Verify release summary
    [Tags]  HappyPath
    user checks page contains element   xpath://li/a[text()="Summary" and contains(@aria-current, 'page')]
    user waits until h2 is visible  Release summary
    user checks summary list contains  Publication title  ${PUBLICATION_NAME}

Upload subject
    [Tags]  HappyPath
    user clicks link  Data and files
    user waits until page contains element  id:dataFileUploadForm-subjectTitle
    user enters text into element  id:dataFileUploadForm-subjectTitle   Dates test subject
    user chooses file   id:dataFileUploadForm-dataFile       ${FILES_DIR}dates.csv
    user chooses file   id:dataFileUploadForm-metadataFile   ${FILES_DIR}dates.meta.csv
    user clicks button  Upload data files

    user waits until h2 is visible  Uploaded data files
    user waits until page contains accordion section   Dates test subject
    user opens accordion section   Dates test subject

    ${section}=  user gets accordion section content element  Dates test subject
    user checks headed table body row contains  Subject title    Dates test subject  ${section}
    user checks headed table body row contains  Data file        dates.csv  ${section}
    user checks headed table body row contains  Metadata file    dates.meta.csv  ${section}
    user checks headed table body row contains  Number of rows   119  ${section}
    user checks headed table body row contains  Data file size   17 Kb  ${section}
    user checks headed table body row contains  Status           Complete  ${section}  180

Add meta guidance
    [Tags]  HappyPath
    user clicks link  Metadata guidance
    user waits until h2 is visible  Public metadata guidance document

    user enters text into element  id:metaGuidanceForm-content  Test meta guidance content
    user waits until page contains accordion section  Dates test subject

    user checks summary list contains  Filename             dates.csv
    user checks summary list contains  Geographic levels    National
    user checks summary list contains  Time period          2020 Week 13 to 2021 Week 24

    user enters text into meta guidance data file content editor  Dates test subject
    ...   Dates test subject test meta guidance content
    user clicks button  Save guidance

Add ancillary file
    [Tags]  HappyPath
    user clicks link  Ancillary file uploads
    user waits until h2 is visible  Add file to release

    user enters text into element  id:fileUploadForm-name   Test ancillary file 1
    user chooses file   id:fileUploadForm-file      ${FILES_DIR}test-file-1.txt
    user clicks button  Upload file

    user waits until page contains accordion section   Test ancillary file 1
    user opens accordion section   Test ancillary file 1   id:file-uploads

    ${section_1}=  user gets accordion section content element  Test ancillary file 1  id:file-uploads
    user checks summary list contains  Name         Test ancillary file 1  ${section_1}
    user checks summary list contains  File         test-file-1.txt     ${section_1}
    user checks summary list contains  File size    12 B                ${section_1}

    user checks there are x accordion sections  1  id:file-uploads

Create data block table
    [Tags]  HappyPath
    user clicks link    Data blocks
    user waits until h2 is visible   Choose a subject

    user waits until page contains   Dates test subject
    user clicks radio    Dates test subject
    user clicks element   id:publicationSubjectForm-submit

    user waits until h2 is visible  Choose locations
    user opens details dropdown   National
    user clicks checkbox   England
    user clicks element     id:locationFiltersForm-submit

    user waits until h2 is visible  Choose time period
    user selects from list by label  id:timePeriodForm-start  2020 Week 13
    user selects from list by label  id:timePeriodForm-end    2020 Week 16
    user clicks element     id:timePeriodForm-submit

    user waits until h2 is visible  Choose your filters
    user clicks subheaded indicator checkbox  Open settings  Number of open settings
    user checks subheaded indicator checkbox is checked  Open settings  Number of open settings
    user clicks subheaded indicator checkbox  Open settings  Proportion of settings open
    user checks subheaded indicator checkbox is checked  Open settings  Proportion of settings open

    user opens details dropdown  Date
    user clicks category checkbox  Date   23/03/2020
    user checks category checkbox is checked  Date  23/03/2020

    user clicks element   id:filtersForm-submit
    user waits until results table appears     180

    user checks table column heading contains  1  1  2020 Week 13
    user checks headed table body row cell contains  Number of open settings        1  22,900
    user checks headed table body row cell contains  Proportion of settings open    1  1%

Save data block
    [Tags]  HappyPath
    user enters text into element  id:dataBlockDetailsForm-name         ${DATABLOCK_NAME}
    user enters text into element  id:dataBlockDetailsForm-heading      Dates table title
    user enters text into element  id:dataBlockDetailsForm-source       Dates source

    user clicks button   Save data block
    user waits until page contains button    Delete this data block

Create chart for data block
    [Tags]  HappyPath
    user waits until page contains link  Chart
    user waits until page does not contain loading spinner
    user clicks element   id:manageDataBlocks-chart-tab
    user clicks button  Choose an infographic as alternative
    user chooses file  id:chartConfigurationForm-file       ${FILES_DIR}test-infographic.png
    user enters text into element  id:chartConfigurationForm-title  Sample title
    user enters text into element  id:chartConfigurationForm-alt  Sample alt text
    user clicks button   Save chart options
    user waits until page contains  Chart preview
    user checks infographic chart contains alt  id:chartBuilderPreview  Sample alt text

Navigate to 'Content' page
    [Tags]  HappyPath
    user clicks link   Content
    user waits until h2 is visible  ${PUBLICATION_NAME}
    user waits until page contains button  Add a summary text block

Add two accordion sections to release
    [Tags]  HappyPath
    user clicks button   Add new section
    user changes accordion section title  1   Dates data block

    user clicks button   Add new section
    user changes accordion section title  2   Test text

Add data block to first accordion section
    [Tags]  HappyPath
    user adds data block to editable accordion section  Dates data block   ${DATABLOCK_NAME}
    ${datablock}=  set variable  css:[data-testid="Data block - ${DATABLOCK_NAME}"]
    user checks chart title contains  ${datablock}  Sample title
    user checks infographic chart contains alt  ${datablock}  Sample alt text

Add test text to second accordion section
    [Tags]  HappyPath
    user adds text block to editable accordion section   Test text
    user adds content to accordion section text block  Test text   1    Some test text!

Add public prerelease access list
    [Tags]  HappyPath
    user clicks link  Pre-release access
    user waits until h2 is visible  Manage pre-release user access
    user creates public prerelease access list  Test public access list

Go to "Sign off" tab
    [Tags]  HappyPath
    user clicks link   Sign off
    user waits until h2 is visible  Sign off
    user waits until page contains button  Edit release status

Approve release
    [Tags]  HappyPath
    ${PUBLISH_DATE_DAY}=  get current datetime  %-d
    ${PUBLISH_DATE_MONTH}=  get current datetime  %B
    ${PUBLISH_DATE_YEAR}=  get current datetime  %Y
    set suite variable  ${PUBLISH_DATE_DAY}
    set suite variable  ${PUBLISH_DATE_MONTH}
    set suite variable  ${PUBLISH_DATE_YEAR}

    user clicks button  Edit release status
    user waits until h2 is visible  Edit release status

    user clicks radio   Approved for publication
    user enters text into element  id:releaseStatusForm-internalReleaseNote  Approved by UI tests - publish release and amend
    user clicks radio  As soon as possible
    user enters text into element  id:releaseStatusForm-nextReleaseDate-month   12
    user enters text into element  id:releaseStatusForm-nextReleaseDate-year    3001

    user clicks button   Update status

Verify release is scheduled
    [Tags]  HappyPath
    user waits until h2 is visible  Sign off
    user checks summary list contains  Current status  Approved
    user checks summary list contains  Scheduled release  ${PUBLISH_DATE_DAY} ${PUBLISH_DATE_MONTH} ${PUBLISH_DATE_YEAR}
    user checks summary list contains  Next release expected  December 3001

Wait for release process status to be Complete
    [Tags]  HappyPath
    user waits for release process status to be  Complete    ${release_complete_wait}
    user reloads page  # EES-1448
    user waits until page does not contain button  Edit release status

User goes to public Find Statistics page
    [Tags]  HappyPath
    environment variable should be set   PUBLIC_URL
    user goes to url   %{PUBLIC_URL}/find-statistics
    user waits until h1 is visible  Find statistics and data
    user waits for page to finish loading

Verify newly published release is on Find Statistics page
    [Tags]  HappyPath
    user waits until page contains accordion section   %{TEST_THEME_NAME}
    user opens accordion section  %{TEST_THEME_NAME}
    user waits until accordion section contains text   %{TEST_THEME_NAME}   ${TOPIC_NAME}

    user opens details dropdown  ${TOPIC_NAME}
    user waits until details dropdown contains publication    ${TOPIC_NAME}  ${PUBLICATION_NAME}   10
    user checks publication bullet contains link   ${PUBLICATION_NAME}  View statistics and data
    user checks publication bullet contains link   ${PUBLICATION_NAME}  Create your own tables online
    user checks publication bullet does not contain link  ${PUBLICATION_NAME}   Statistics at DfE

Navigate to newly published release page
    [Tags]  HappyPath
    user clicks testid element  View stats link for ${PUBLICATION_NAME}
    user waits until h1 is visible  ${PUBLICATION_NAME}  90

Verify release URL and page caption
    [Tags]  HappyPath
    user checks url contains  %{PUBLIC_URL}/find-statistics/ui-tests-publish-release-${RUN_IDENTIFIER}
    user waits until page contains title caption  ${RELEASE_NAME}

Verify publish and update dates
    [Tags]  HappyPath
    user checks testid element contains   published-date   ${PUBLISH_DATE_DAY} ${PUBLISH_DATE_MONTH} ${PUBLISH_DATE_YEAR}
    user waits until element contains  css:[data-testid="next-update"] time   December 3001

Verify release associated files
    [Tags]  HappyPath
    user opens details dropdown     Download associated files
    ${downloads}=  user gets details content element  Download associated files
    user checks element should contain  ${downloads}  All files (zip, 3 Kb)
    user waits until element contains link  ${downloads}  All files
    user checks link has url  All files  %{DATA_API_URL}/download/ui-tests-publish-release-${RUN_IDENTIFIER}/3000-01/ancillary/ui-tests-publish-release-${RUN_IDENTIFIER}_3000-01.zip   ${downloads}

    user checks element should contain  ${downloads}  Dates test subject (csv, 17 Kb)
    user waits until element contains link  ${downloads}  Dates test subject
    user checks link has url  Dates test subject  %{DATA_API_URL}/download/ui-tests-publish-release-${RUN_IDENTIFIER}/3000-01/data/dates.csv   ${downloads}

    user checks element should contain  ${downloads}  Test ancillary file 1 (txt, 12 B)
    user waits until element contains link  ${downloads}  Test ancillary file 1
    download file  link:Test ancillary file 1   test_ancillary_file_1.txt
    downloaded file should have first line  test_ancillary_file_1.txt   Test file 1

Verify public metadata guidance document
    [Tags]  HappyPath
    user clicks link  Metadata guidance document

    user checks breadcrumb count should be   4
    user checks nth breadcrumb contains   1    Home
    user checks nth breadcrumb contains   2    Find statistics and data
    user checks nth breadcrumb contains   3    ${PUBLICATION_NAME}
    user checks nth breadcrumb contains   4    Metadata guidance document
    user waits until h2 is visible  Metadata guidance document

    user waits until page contains title caption  ${RELEASE_NAME}
    user waits until h1 is visible  ${PUBLICATION_NAME}

    user waits until h2 is visible  Metadata guidance document
    user waits until page contains  Test meta guidance content

    user waits until page contains accordion section  Dates test subject
    user checks there are x accordion sections  1

    user opens accordion section  Dates test subject
    user checks summary list contains  Filename             dates.csv
    user checks summary list contains  Geographic levels    National
    user checks summary list contains  Time period          2020 Week 13 to 2021 Week 24
    user checks summary list contains  Content              Dates test subject test meta guidance content

    user opens details dropdown  Variable names and descriptions

    user checks table column heading contains  1  1  Variable name
    user checks table column heading contains  1  2  Variable description

    user checks results table cell contains  1  1   children_attending
    user checks results table cell contains  1  2   Number of children attending

    user checks results table cell contains  6  1   date
    user checks results table cell contains  6  2   Date

    user checks results table cell contains  10  1   otherwise_vulnerable_children_attending
    user checks results table cell contains  10  2   Number of otherwise vulnerable children attending

    user goes to release page via breadcrumb  ${PUBLICATION_NAME}  ${RELEASE_NAME}

Verify public pre-release access list
    [Tags]  HappyPath
    user opens details dropdown     Download associated files
    user clicks link  Pre-release access list

    user checks breadcrumb count should be   4
    user checks nth breadcrumb contains   1    Home
    user checks nth breadcrumb contains   2    Find statistics and data
    user checks nth breadcrumb contains   3    ${PUBLICATION_NAME}
    user checks nth breadcrumb contains   4    Pre-release access list

    user waits until page contains title caption  ${RELEASE_NAME}
    user waits until h1 is visible  ${PUBLICATION_NAME}

    user waits until h2 is visible  Pre-release access list
    user waits until page contains  Published ${PUBLISH_DATE_DAY} ${PUBLISH_DATE_MONTH} ${PUBLISH_DATE_YEAR}
    user waits until page contains  Test public access list

    user goes to release page via breadcrumb  ${PUBLICATION_NAME}  ${RELEASE_NAME}

Verify accordions are correct
    [Tags]  HappyPath
    user checks accordion is in position   Dates data block     1
    user checks accordion is in position   Test text            2
    user checks accordion is in position   National Statistics  3
    user checks accordion is in position   Contact us           4

Verify Dates data block accordion section
    [Tags]  HappyPath
    user opens accordion section  Dates data block
    user scrolls to accordion section content  Dates data block
    ${section}=  user gets accordion section content element  Dates data block

    user checks chart title contains  ${section}  Sample title
    user checks infographic chart contains alt  ${section}  Sample alt text

    user clicks link  Table  ${section}
    user waits until parent contains element  ${section}  xpath:.//*[@data-testid="dataTableCaption" and text()="Dates table title"]
    user waits until parent contains element  ${section}  xpath:.//*[.="Source: Dates source"]

    user checks table column heading contains  1  1  2020 Week 13  ${section}
    user checks headed table body row cell contains  Number of open settings        1  22,900  ${section}
    user checks headed table body row cell contains  Proportion of settings open    1  1%      ${section}

    user closes accordion section  Dates data block

Verify Test text accordion section contains correct text
    [Tags]  HappyPath
    user opens accordion section   Test text
    ${section}=  get webelement  xpath://*[contains(@class, "govuk-accordion__section-button") and text()="Test text"]/../../..
    user waits until parent contains element  ${section}   xpath:.//p[text()="Some test text!"]
    user closes accordion section  Test text

Return to Admin to start creating an amendment
    [Tags]  HappyPath
    user goes to url  %{ADMIN_URL}
    user waits until h1 is visible   Dashboard
    user waits until page contains title caption  Welcome Bau1
    user waits until page contains element   id:publicationsReleases-themeTopic-themeId   180

Create amendment
    [Tags]  HappyPath
    user selects theme and topic from admin dashboard  ${THEME_NAME}  ${TOPIC_NAME}
    user waits until page contains link    Create new publication
    user waits until page contains accordion section  ${PUBLICATION_NAME}

    user opens accordion section  ${PUBLICATION_NAME}
    ${accordion_section}=  user gets accordion section content element  ${PUBLICATION_NAME}

    user opens details dropdown  ${RELEASE_NAME}  ${accordion_section}
    ${details_elem}=  user gets details content element  ${RELEASE_NAME} (Live - Latest release)  ${accordion_section}

    user clicks button  Amend this release  ${details_elem}
    user waits until h1 is visible  Confirm you want to amend this live release
    user clicks button   Confirm
    user waits until h1 is visible  ${PUBLICATION_NAME}
    user waits until page contains title caption  Amend release

Navigate to data replacement page
    [Tags]  HappyPath
    user clicks link  Data and files
    user waits until h2 is visible  Uploaded data files
    user waits until page contains accordion section   Dates test subject
    user opens accordion section   Dates test subject

    ${section}=  user gets accordion section content element  Dates test subject
    user clicks link  Replace data  ${section}

    user waits until h2 is visible  Data file details
    user checks headed table body row contains  Subject title    Dates test subject
    user checks headed table body row contains  Data file        dates.csv
    user checks headed table body row contains  Metadata file    dates.meta.csv
    user checks headed table body row contains  Number of rows   119
    user checks headed table body row contains  Data file size   17 Kb
    user checks headed table body row contains  Status           Complete   wait=180

Upload replacement data
    [Tags]  HappyPath
    user waits until h2 is visible  Upload replacement data
    user chooses file   id:dataFileUploadForm-dataFile       ${FILES_DIR}dates-replacement.csv
    user chooses file   id:dataFileUploadForm-metadataFile   ${FILES_DIR}dates-replacement.meta.csv
    user clicks button  Upload data files

    user checks table column heading contains  1  1  Original file
    user checks table column heading contains  1  2  Replacement file

    user checks headed table body row cell contains  Subject title   1  Dates test subject
    user checks headed table body row cell contains  Data file       1  dates.csv
    user checks headed table body row cell contains  Metadata file   1  dates.meta.csv
    user checks headed table body row cell contains  Number of rows  1  119
    user checks headed table body row cell contains  Data file size  1  17 Kb
    user checks headed table body row cell contains  Status          1  Data replacement in progress   wait=180

    user checks headed table body row cell contains  Subject title   2  Dates test subject
    user checks headed table body row cell contains  Data file       2  dates-replacement.csv
    user checks headed table body row cell contains  Metadata file   2  dates-replacement.meta.csv
    user checks headed table body row cell contains  Number of rows  2  119
    user checks headed table body row cell contains  Data file size  2  17 Kb
    user checks headed table body row cell contains  Status          2  Complete   wait=180

Confirm data replacement
    [Tags]  HappyPath
    user waits until page contains  Data blocks: OK
    user waits until page contains  Footnotes: OK
    user clicks button  Confirm data replacement
    user waits until h2 is visible  Data replacement complete

Verify existing meta guidance for amendment
    [Tags]  HappyPath
    user clicks link  Data and files
    user clicks link  Metadata guidance
    user waits until h2 is visible  Public metadata guidance document

    user waits until element contains  id:metaGuidanceForm-content  Test meta guidance content

    user waits until page contains accordion section  Dates test subject

    user checks summary list contains  Filename             dates-replacement.csv
    user checks summary list contains  Geographic levels    National
    user checks summary list contains  Time period          2020 Week 13 to 2021 Week 24

    ${editor}=  user gets meta guidance data file content editor  Dates test subject
    user waits until element contains  ${editor}    Dates test subject test meta guidance content

Update existing meta guidance for amendment
    [Tags]  HappyPath
    user enters text into element  id:metaGuidanceForm-content  Updated test meta guidance content
    user enters text into meta guidance data file content editor  Dates test subject
    ...  Updated Dates test subject test meta guidance content

    user clicks button  Save guidance

# TODO: Add footnotes

Add ancillary file to amendment
    [Tags]  HappyPath
    user clicks link  Data and files
    user clicks link  Ancillary file uploads
    user waits until h2 is visible  Add file to release

    user enters text into element  id:fileUploadForm-name   Test ancillary file 2
    user chooses file   id:fileUploadForm-file      ${FILES_DIR}test-file-2.txt
    user clicks button  Upload file

    user waits until page contains accordion section   Test ancillary file 2
    user opens accordion section   Test ancillary file 2  id:file-uploads

    ${section_2}=  user gets accordion section content element  Test ancillary file 2  id:file-uploads
    user checks summary list contains  Name         Test ancillary file 2  ${section_2}
    user checks summary list contains  File         test-file-2.txt     ${section_2}
    user checks summary list contains  File size    24 B                ${section_2}

    user checks there are x accordion sections  2  id:file-uploads

Edit data block for amendment
    [Tags]  HappyPath
    user clicks link    Data blocks
    user selects from list by label  id:selectedDataBlock  ${DATABLOCK_NAME}
    user waits until h2 is visible   Data block details

    user clicks element  css:[data-testid="wizardStep-4-goToButton"]
    user clicks button  Confirm

    user opens details dropdown  Date
    user clicks category checkbox  Date   24/03/2020
    user checks category checkbox is checked  Date  24/03/2020

    user clicks element   id:filtersForm-submit
    user waits until results table appears     180

    user checks table column heading contains  1  1  2020 Week 13
    user checks headed table body row cell contains  Number of open settings        1  23,000
    user checks headed table body row cell contains  Proportion of settings open    1  2%
    user checks headed table body row cell contains  Number of open settings        1  23,600
    user checks headed table body row cell contains  Proportion of settings open    1  1%

Save data block for amendment
    [Tags]  HappyPath
    user enters text into element  id:dataBlockDetailsForm-name         ${DATABLOCK_NAME}
    user enters text into element  id:dataBlockDetailsForm-heading      Updated dates table title
    user enters text into element  id:dataBlockDetailsForm-source       Updated dates source

    user clicks button   Save data block
    user waits until page contains button    Delete this data block

Update data block chart for amendment
    [Tags]  HappyPath
    user waits until page contains link  Chart
    user waits until page does not contain loading spinner
    user clicks element   id:manageDataBlocks-chart-tab
    user waits until page contains element  id:chartConfigurationForm-title
    user enters text into element  id:chartConfigurationForm-title  Updated sample title
    user enters text into element  id:chartConfigurationForm-alt  Updated sample alt text
    user clicks button   Save chart options
    user waits until page does not contain loading spinner
    user checks infographic chart contains alt  id:chartBuilderPreview  Updated sample alt text

Navigate to 'Content' page for amendment
    [Tags]  HappyPath
    user clicks link   Content
    user waits until h2 is visible  ${PUBLICATION_NAME}
    user waits until page contains button  Add a summary text block

Update second accordion section text for amendment
    [Tags]  HappyPath
    user opens accordion section  Test text
    user adds content to accordion section text block  Test text  1  Updated test text!

Add release note to amendment
    [Tags]  HappyPath
    user clicks button   Add note
    user enters text into element  id:createReleaseNoteForm-reason  Test release note one
    user clicks button   Save note
    user opens details dropdown  See all 1 updates  id:releaseLastUpdated
    ${date}=  get current datetime   %-d %B %Y
    user waits until element contains  css:#releaseNotes li:nth-of-type(1) time  ${date}
    user waits until element contains  css:#releaseNotes li:nth-of-type(1) p     Test release note one

Update public prerelease access list for amendment
    [Tags]  HappyPath
    user clicks link  Pre-release access
    user waits until h2 is visible  Manage pre-release user access
    user updates public prerelease access list  Updated public access list

Go to "Sign off" page again
    [Tags]  HappyPath
    user clicks link   Sign off
    user waits until h2 is visible  Sign off
    user waits until page contains button  Edit release status

Approve amendment for immediate release
    [Tags]  HappyPath
    user clicks button   Edit release status
    user waits until h2 is visible  Edit release status

    user clicks radio   Approved for publication
    user enters text into element  id:releaseStatusForm-internalReleaseNote  Amendment approved by UI tests
    user clicks radio  As soon as possible
    user enters text into element  id:releaseStatusForm-nextReleaseDate-month   1
    user enters text into element  id:releaseStatusForm-nextReleaseDate-year    3002

    user clicks button  Update status

Wait for release process status to be Complete again
    [Tags]  HappyPath
    # EES-1007 - Release process status doesn't automatically update
    user waits until h2 is visible  Sign off
    user checks summary list contains  Current status  Approved
    user checks summary list contains  Scheduled release  ${PUBLISH_DATE_DAY} ${PUBLISH_DATE_MONTH} ${PUBLISH_DATE_YEAR}
    user waits for release process status to be  Complete    ${release_complete_wait}
    user reloads page  # EES-1448
    user checks page does not contain button  Edit release status

Go back to public find-statistics page
    [Tags]  HappyPath
    user goes to url   %{PUBLIC_URL}/find-statistics
    user waits until h1 is visible  Find statistics and data
    user waits for page to finish loading

Verify amendment is on Find Statistics page again
    [Tags]  HappyPath
    user waits until page contains accordion section   %{TEST_THEME_NAME}
    user opens accordion section  %{TEST_THEME_NAME}
    user waits until accordion section contains text   %{TEST_THEME_NAME}   ${TOPIC_NAME}

    user opens details dropdown  ${TOPIC_NAME}
    user waits until details dropdown contains publication    ${TOPIC_NAME}  ${PUBLICATION_NAME}   10
    user checks publication bullet contains link   ${PUBLICATION_NAME}  View statistics and data
    user checks publication bullet contains link   ${PUBLICATION_NAME}  Create your own tables online
    user checks publication bullet does not contain link  ${PUBLICATION_NAME}   Statistics at DfE

Navigate to amendment release page
    [Tags]  HappyPath
    user clicks testid element  View stats link for ${PUBLICATION_NAME}

    user waits until h1 is visible  ${PUBLICATION_NAME}  90
    user waits until page contains title caption  ${RELEASE_NAME}

    user checks url contains  %{PUBLIC_URL}/find-statistics/ui-tests-publish-release-${RUN_IDENTIFIER}

    user checks breadcrumb count should be   3
    user checks nth breadcrumb contains   1    Home
    user checks nth breadcrumb contains   2    Find statistics and data
    user checks nth breadcrumb contains   3    ${PUBLICATION_NAME}

Verify amendment is displayed as the latest release
    [Tags]  HappyPath   Failing
    [Documentation]  EES-1301
    user checks page does not contain  View latest data:
    user checks page does not contain  See 1 other releases

Verify amendment publish and update dates
    [Tags]  HappyPath
    user checks testid element contains    published-date   ${PUBLISH_DATE_DAY} ${PUBLISH_DATE_MONTH} ${PUBLISH_DATE_YEAR}
    user waits until element contains  css:[data-testid="next-update"] time   January 3002

Verify amendment files
    [Tags]  HappyPath
    user opens details dropdown     Download associated files
    ${downloads}=  user gets details content element  Download associated files
    user checks element should contain  ${downloads}  All files (zip, 3 Kb)
    user waits until element contains link  ${downloads}  All files
    user checks link has url  All files  %{DATA_API_URL}/download/ui-tests-publish-release-${RUN_IDENTIFIER}/3000-01/ancillary/ui-tests-publish-release-${RUN_IDENTIFIER}_3000-01.zip   ${downloads}

    user checks element should contain  ${downloads}  Dates test subject (csv, 17 Kb)
    user waits until element contains link  ${downloads}  Dates test subject
    user checks link has url  Dates test subject  %{DATA_API_URL}/download/ui-tests-publish-release-${RUN_IDENTIFIER}/3000-01/data/dates-replacement.csv   ${downloads}

    user checks element should contain  ${downloads}  Test ancillary file 1 (txt, 12 B)
    user waits until element contains link  ${downloads}  Test ancillary file 1
    download file  link:Test ancillary file 1   test_ancillary_file_1.txt
    downloaded file should have first line  test_ancillary_file_1.txt   Test file 1

    user checks element should contain  ${downloads}  Test ancillary file 2 (txt, 24 B)
    user waits until element contains link  ${downloads}  Test ancillary file 2
    download file  link:Test ancillary file 2   test_ancillary_file_2.txt
    downloaded file should have first line  test_ancillary_file_2.txt   Test file 2

Verify amendment public metadata guidance document
    [Tags]  HappyPath
    user clicks link  Metadata guidance document

    user checks breadcrumb count should be   4
    user checks nth breadcrumb contains   1    Home
    user checks nth breadcrumb contains   2    Find statistics and data
    user checks nth breadcrumb contains   3    ${PUBLICATION_NAME}
    user checks nth breadcrumb contains   4    Metadata guidance document
    user waits until h2 is visible  Metadata guidance document

    user waits until page contains title caption  ${RELEASE_NAME}
    user waits until h1 is visible  ${PUBLICATION_NAME}

    user waits until h2 is visible  Metadata guidance document
    user waits until page contains  Updated test meta guidance content

    user waits until page contains accordion section  Dates test subject
    user checks there are x accordion sections  1

    user opens accordion section  Dates test subject
    user checks summary list contains  Filename             dates-replacement.csv
    user checks summary list contains  Geographic levels    National
    user checks summary list contains  Time period          2020 Week 13 to 2021 Week 24
    user checks summary list contains  Content              Updated Dates test subject test meta guidance content

    user opens details dropdown  Variable names and descriptions

    user checks table column heading contains  1  1  Variable name
    user checks table column heading contains  1  2  Variable description

    user checks results table cell contains  1  1   children_attending
    user checks results table cell contains  1  2   Number of children attending

    user checks results table cell contains  6  1   date
    user checks results table cell contains  6  2   Date

    user checks results table cell contains  10  1   otherwise_vulnerable_children_attending
    user checks results table cell contains  10  2   Number of otherwise vulnerable children attending

    user goes to release page via breadcrumb  ${PUBLICATION_NAME}  ${RELEASE_NAME}

Verify amendment public pre-release access list
    [Tags]  HappyPath
    user opens details dropdown     Download associated files
    user clicks link  Pre-release access list

    user checks breadcrumb count should be   4
    user checks nth breadcrumb contains   1    Home
    user checks nth breadcrumb contains   2    Find statistics and data
    user checks nth breadcrumb contains   3    ${PUBLICATION_NAME}
    user checks nth breadcrumb contains   4    Pre-release access list

    user waits until page contains title caption  ${RELEASE_NAME}
    user waits until h1 is visible  ${PUBLICATION_NAME}

    user waits until h2 is visible  Pre-release access list
    user waits until page contains  Published ${PUBLISH_DATE_DAY} ${PUBLISH_DATE_MONTH} ${PUBLISH_DATE_YEAR}
    user waits until page contains  Updated public access list

    user goes to release page via breadcrumb  ${PUBLICATION_NAME}  ${RELEASE_NAME}

Verify amendment accordions are correct
    [Tags]  HappyPath
    user checks accordion is in position   Dates data block     1
    user checks accordion is in position   Test text            2
    user checks accordion is in position   National Statistics  3
    user checks accordion is in position   Contact us           4

Verify amendment Dates data block accordion section
    [Tags]  HappyPath
    user opens accordion section  Dates data block
    user scrolls to accordion section content  Dates data block
    ${section}=  user gets accordion section content element  Dates data block

    user checks chart title contains  ${section}  Updated sample title
    user checks infographic chart contains alt  ${section}  Updated sample alt text

    user clicks link  Table  ${section}
    user waits until parent contains element  ${section}  xpath:.//*[@data-testid="dataTableCaption" and text()="Updated dates table title"]
    user waits until parent contains element  ${section}  xpath:.//*[.="Source: Updated dates source"]

    user checks table column heading contains  1  1  2020 Week 13  ${section}
    user checks headed table body row cell contains  Number of open settings        1  23,000  ${section}
    user checks headed table body row cell contains  Proportion of settings open    1  2%      ${section}
    user checks headed table body row cell contains  Number of open settings        1  23,600  ${section}
    user checks headed table body row cell contains  Proportion of settings open    1  1%      ${section}
    user closes accordion section  Dates data block

Verify amendment Test text accordion section contains correct text
    [Tags]  HappyPath
    user opens accordion section   Test text
    ${section}=  get webelement  xpath://*[contains(@class, "govuk-accordion__section-button") and text()="Test text"]/../../..
    user waits until parent contains element  ${section}   xpath:.//p[text()="Updated test text!"]
    user closes accordion section  Test text
