*** Settings ***
Resource    ../../libs/admin-common.robot
Resource    ../../libs/charts.robot
Library     ../../libs/api_keywords.py

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${RUN_IDENTIFIER}    %{RUN_IDENTIFIER}
${TOPIC_NAME}        %{TEST_TOPIC_NAME}
${PUBLICATION_NAME}  UI tests - publish release %{RUN_IDENTIFIER}
${DATABLOCK_NAME}    Dates data block name

*** Test Cases ***
Create new publication for "UI tests topic" topic
    [Tags]  HappyPath
    ${PUBLICATION_ID}=  user creates test publication via api   ${PUBLICATION_NAME}
    user create test release via api  ${PUBLICATION_ID}   FY   3000

Go to "Release summary" page
    [Tags]  HappyPath
    user navigates to release summary from admin dashboard  ${PUBLICATION_NAME}   Financial Year 3000-01 (not Live)

Verify release summary
    [Tags]  HappyPath
    user checks page contains element   xpath://li/a[text()="Summary" and contains(@aria-current, 'page')]
    user waits until h2 is visible  Release summary
    user checks summary list contains  Publication title  ${PUBLICATION_NAME}

Upload subject
    [Tags]  HappyPath
    user clicks link  Data and files
    user waits until page contains element  css:#dataFileUploadForm-subjectTitle
    user enters text into element  css:#dataFileUploadForm-subjectTitle   Dates test subject
    user chooses file   css:#dataFileUploadForm-dataFile       ${CURDIR}${/}files${/}dates.csv
    user chooses file   css:#dataFileUploadForm-metadataFile   ${CURDIR}${/}files${/}dates.meta.csv
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

# TODO: Add footnotes

Add ancillary file
    [Tags]  HappyPath
    user clicks link  File uploads
    user waits until h2 is visible  Add file to release

    user enters text into element  id:fileUploadForm-name   Test ancillary file 1
    user chooses file   id:fileUploadForm-file      ${CURDIR}${/}files${/}test-file-1.txt
    user clicks button  Upload file

    user waits until page contains accordion section   test ancillary file 1
    user opens accordion section   test ancillary file 1   id:file-uploads

    ${section_1}=  user gets accordion section content element  test ancillary file 1  id:file-uploads
    user checks summary list contains  Name         test ancillary file 1  ${section_1}
    user checks summary list contains  File         test-file-1.txt     ${section_1}
    user checks summary list contains  File size    12 B                ${section_1}

    user checks there are x accordion sections  1  id:file-uploads

Create data block table
    [Tags]  HappyPath
    user clicks link    Data blocks
    user waits until h2 is visible   Choose a subject

    user waits until page contains   Dates test subject
    user clicks radio    Dates test subject
    user clicks element   css:#publicationSubjectForm-submit

    user waits until h2 is visible  Choose locations
    user opens details dropdown   National
    user clicks checkbox   England
    user clicks element     css:#locationFiltersForm-submit

    user waits until h2 is visible  Choose time period
    user selects from list by label  id:timePeriodForm-start  2020 Week 13
    user selects from list by label  id:timePeriodForm-end    2020 Week 16
    user clicks element     css:#timePeriodForm-submit

    user waits until h2 is visible  Choose your filters
    user clicks subheaded indicator checkbox  Open settings  Number of open settings
    user checks subheaded indicator checkbox is checked  Open settings  Number of open settings
    user clicks subheaded indicator checkbox  Open settings  Proportion of settings open
    user checks subheaded indicator checkbox is checked  Open settings  Proportion of settings open

    user opens details dropdown  Date
    user clicks category checkbox  Date   23/03/2020
    user checks category checkbox is checked  Date  23/03/2020

    user clicks element   css:#filtersForm-submit
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
    user chooses file  id:chartConfigurationForm-file       ${CURDIR}${/}files${/}test-infographic.png
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

    user clicks link  Public access list
    user waits until h2 is visible  Public pre-release access list
    user clicks button   Create public pre-release access list
    user presses keys  CTRL+a+BACKSPACE
    user presses keys  Test public access list
    user clicks button  Save access list
    user waits until element contains  css:[data-testid="publicPreReleaseAccessListPreview"]  Test public access list

Go to "Release status" tab
    [Tags]  HappyPath
    user clicks link   Release status
    user waits until h2 is visible  Release status
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
    user waits until h2 is visible  Release status
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
    user waits until page contains accordion section   Test theme
    user opens accordion section  Test theme
    user waits until accordion section contains text   Test theme   ${TOPIC_NAME}

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
    user waits until page contains title caption  Financial Year 3000-01

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

    user checks element should contain  ${downloads}  test ancillary file 1 (txt, 12 B)
    user waits until element contains link  ${downloads}  test ancillary file 1
    user checks link has url  test ancillary file 1  %{DATA_API_URL}/download/ui-tests-publish-release-${RUN_IDENTIFIER}/3000-01/ancillary/test-file-1.txt   ${downloads}

Verify public pre-release access list
    [Tags]  HappyPath
    user clicks link  Pre-release access list

    user checks breadcrumb count should be   4
    user checks nth breadcrumb contains   1    Home
    user checks nth breadcrumb contains   2    Find statistics and data
    user checks nth breadcrumb contains   3    ${PUBLICATION_NAME}
    user checks nth breadcrumb contains   4    Pre-release access list

    user waits until page contains title caption  Financial Year 3000-01
    user waits until h1 is visible  ${PUBLICATION_NAME}

    user waits until h2 is visible  Pre-release access list
    user waits until page contains  Published ${PUBLISH_DATE_DAY} ${PUBLISH_DATE_MONTH} ${PUBLISH_DATE_YEAR}
    user waits until page contains  Test public access list

Navigate back to release page
    user clicks link  ${PUBLICATION_NAME}

    user checks breadcrumb count should be   3
    user checks nth breadcrumb contains   1    Home
    user checks nth breadcrumb contains   2    Find statistics and data
    user checks nth breadcrumb contains   3    ${PUBLICATION_NAME}

    user waits until h1 is visible  ${PUBLICATION_NAME}
    user waits until page contains title caption  Financial Year 3000-01

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
    user waits until parent contains element  ${section}  xpath:.//*[@id="dataTableCaption" and text()="Dates table title"]
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
    user waits until page contains element   css:#selectTheme   180

Create amendment
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "${TOPIC_NAME}" from the admin dashboard
    user waits until page contains link    Create new publication
    user waits until page contains accordion section  ${PUBLICATION_NAME}

    user opens accordion section  ${PUBLICATION_NAME}
    ${accordion_section}=  user gets accordion section content element  ${PUBLICATION_NAME}

    user opens details dropdown  Financial Year 3000-01 (Live - Latest release)  ${accordion_section}
    ${details_elem}=  user gets details content element  Financial Year 3000-01 (Live - Latest release)  ${accordion_section}

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
    user chooses file   css:#dataFileUploadForm-dataFile       ${CURDIR}${/}files${/}dates-replacement.csv
    user chooses file   css:#dataFileUploadForm-metadataFile   ${CURDIR}${/}files${/}dates-replacement.meta.csv
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

# TODO: Add footnotes

Add ancillary file to amendment
    [Tags]  HappyPath
    user clicks link  Data and files
    user clicks link  File uploads
    user waits until h2 is visible  Add file to release

    user enters text into element  id:fileUploadForm-name   Test ancillary file 2
    user chooses file   id:fileUploadForm-file      ${CURDIR}${/}files${/}test-file-2.txt
    user clicks button  Upload file

    user waits until page contains accordion section   test ancillary file 2
    user opens accordion section   test ancillary file 2  id:file-uploads

    ${section_2}=  user gets accordion section content element  test ancillary file 2  id:file-uploads
    user checks summary list contains  Name         test ancillary file 2  ${section_2}
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

    user clicks element   css:#filtersForm-submit
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
    user enters text into element  id:chartConfigurationForm-title  Updated sample title
    user enters text into element  id:chartConfigurationForm-alt  Updated sample alt text
    user clicks button   Save chart options
    user waits until page contains  Chart preview
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

Update public prerelease access list for amendment
    [Tags]  HappyPath
    user clicks link  Pre-release access
    user waits until h2 is visible  Manage pre-release user access

    user clicks link  Public access list
    user waits until h2 is visible  Public pre-release access list
    user clicks button   Edit public pre-release access list
    user presses keys  CTRL+a
    user presses keys  BACKSPACE
    user presses keys  Updated public access list
    user clicks button  Save access list
    user waits until element contains  css:[data-testid="publicPreReleaseAccessListPreview"]  Updated public access list

Go to "Release status" page again
    [Tags]  HappyPath
    user clicks link   Release status
    user waits until h2 is visible  Release status
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
    user waits until h2 is visible  Release status
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
    user waits until page contains accordion section   Test theme
    user opens accordion section  Test theme
    user waits until accordion section contains text   Test theme   ${TOPIC_NAME}

    user opens details dropdown  ${TOPIC_NAME}
    user waits until details dropdown contains publication    ${TOPIC_NAME}  ${PUBLICATION_NAME}   10
    user checks publication bullet contains link   ${PUBLICATION_NAME}  View statistics and data
    user checks publication bullet contains link   ${PUBLICATION_NAME}  Create your own tables online
    user checks publication bullet does not contain link  ${PUBLICATION_NAME}   Statistics at DfE

Navigate to amendment release page
    [Tags]  HappyPath
    user clicks testid element  View stats link for ${PUBLICATION_NAME}

    user waits until h1 is visible  ${PUBLICATION_NAME}  90
    user waits until page contains title caption  Financial Year 3000-01

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

    user checks element should contain  ${downloads}  test ancillary file 1 (txt, 12 B)
    user waits until element contains link  ${downloads}  test ancillary file 1
    user checks link has url  test ancillary file 1  %{DATA_API_URL}/download/ui-tests-publish-release-${RUN_IDENTIFIER}/3000-01/ancillary/test-file-1.txt   ${downloads}

    user checks element should contain  ${downloads}  test ancillary file 2 (txt, 24 B)
    user waits until element contains link  ${downloads}  test ancillary file 2
    user checks link has url  test ancillary file 2  %{DATA_API_URL}/download/ui-tests-publish-release-${RUN_IDENTIFIER}/3000-01/ancillary/test-file-2.txt   ${downloads}

Verify amendment public pre-release access list
    [Tags]  HappyPath
    user clicks link  Pre-release access list

    user checks breadcrumb count should be   4
    user checks nth breadcrumb contains   1    Home
    user checks nth breadcrumb contains   2    Find statistics and data
    user checks nth breadcrumb contains   3    ${PUBLICATION_NAME}
    user checks nth breadcrumb contains   4    Pre-release access list

    user waits until page contains title caption  Financial Year 3000-01
    user waits until h1 is visible  ${PUBLICATION_NAME}

    user waits until h2 is visible  Pre-release access list
    user waits until page contains  Published ${PUBLISH_DATE_DAY} ${PUBLISH_DATE_MONTH} ${PUBLISH_DATE_YEAR}
    user waits until page contains  Updated public access list

Navigate back to amendment release page
    user clicks link  ${PUBLICATION_NAME}

    user checks breadcrumb count should be   3
    user checks nth breadcrumb contains   1    Home
    user checks nth breadcrumb contains   2    Find statistics and data
    user checks nth breadcrumb contains   3    ${PUBLICATION_NAME}

    user waits until h1 is visible  ${PUBLICATION_NAME}
    user waits until page contains title caption  Financial Year 3000-01

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
    user waits until parent contains element  ${section}  xpath:.//*[@id="dataTableCaption" and text()="Updated dates table title"]
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
