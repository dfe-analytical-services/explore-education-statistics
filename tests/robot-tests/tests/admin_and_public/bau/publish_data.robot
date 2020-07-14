*** Settings ***
Resource    ../../libs/admin-common.robot

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${TOPIC_NAME}        UI test topic %{RUN_IDENTIFIER}
${PUBLICATION_NAME}  UI tests - publish data %{RUN_IDENTIFIER}

*** Test Cases ***
Create new publication for "UI tests topic" topic
    [Tags]  HappyPath
    environment variable should be set   RUN_IDENTIFIER
    user selects theme "Test theme" and topic "${TOPIC_NAME}" from the admin dashboard
    user waits until page contains link    Create new publication
    user checks page does not contain button   ${PUBLICATION_NAME}
    user clicks link  Create new publication
    user creates publication without methodology  ${PUBLICATION_NAME}   Tingting Shu - (Attainment statistics team)

Verify new publication
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "${TOPIC_NAME}" from the admin dashboard
    user waits until page contains button  ${PUBLICATION_NAME}

Create new release
    [Tags]  HappyPath
    user opens accordion section  ${PUBLICATION_NAME}
    user clicks element  css:[data-testid="Create new release link for ${PUBLICATION_NAME}"]
    user creates release for publication  ${PUBLICATION_NAME}  Financial Year  3000

Verify release summary
    [Tags]  HappyPath
    user checks page contains element   xpath://li/a[text()="Release summary" and contains(@aria-current, 'page')]
    user waits until page contains heading 2  Release summary
    user checks summary list item "Publication title" should be "${PUBLICATION_NAME}"

Upload subject
    [Tags]  HappyPath
    user clicks link  Manage data
    user enters text into element  css:#dataFileUploadForm-subjectTitle   UI test subject
    choose file   css:#dataFileUploadForm-dataFile       ${CURDIR}${/}files${/}upload-file-test.csv
    choose file   css:#dataFileUploadForm-metadataFile   ${CURDIR}${/}files${/}upload-file-test.meta.csv
    user clicks button  Upload data files

    user waits until page contains heading 2  Uploaded data files
    user waits until page contains accordion section   UI test subject
    user opens accordion section   UI test subject
    user checks page contains element   xpath://dt[text()="Subject title"]/../dd/h4[text()="UI test subject"]
    user waits until page contains element  xpath://dt[text()="Status"]/../dd//strong[text()="Complete"]     180

Navigate to Manage data blocks tab
    [Tags]  HappyPath
    user clicks link    Manage data blocks
    user waits until page contains heading 2   Choose a subject

Select subject "UI test subject"
    [Tags]  HappyPath
    user waits until page contains   UI test subject
    user selects radio    UI test subject
    user clicks element   css:#publicationSubjectForm-submit

Select locations
    [Tags]   HappyPath
    user waits until element is visible  xpath://h2[text()="Choose locations"]     90
    user opens details dropdown   Opportunity Area
    user clicks checkbox   Bolton 001 (E02000984)
    user clicks checkbox   Bolton 001 (E05000364)
    user clicks checkbox   Bolton 004 (E02000987)
    user clicks checkbox   Bolton 004 (E05010450)
    user opens details dropdown   Ward
    user clicks checkbox   Nailsea Youngwood
    user clicks checkbox   Syon
    user clicks element     css:#locationFiltersForm-submit

Select time period
    [Tags]   HappyPath
    user waits until element is visible  xpath://h2[text()="Choose time period"]   90
    user selects start date    2005
    user selects end date      2020
    user clicks element     css:#timePeriodForm-submit

Select indicators
    [Tags]  HappyPath
    user waits until element is visible  xpath://h2[text()="Choose your filters"]
    user clicks indicator checkbox    Admission Numbers

Create table
    [Tags]  HappyPath
    user clicks element   css:#filtersForm-submit
    user waits until results table appears     180

Save data block as a highlight
    [Tags]  HappyPath
    user enters text into element  id:dataBlockDetailsForm-name         UI Test data block name
    user enters text into element  id:dataBlockDetailsForm-heading      UI Test table title
    user enters text into element  id:dataBlockDetailsForm-source       UI Test source

    user clicks checkbox  Set as a table highlight for this publication
    user waits until page contains element  id:dataBlockDetailsForm-highlightName
    user enters text into element  id:dataBlockDetailsForm-highlightName    Test highlight name

    user clicks button   Save data block
    user waits until page contains    Delete this data block

Go to "Release status" tab
    [Tags]  HappyPath
    user clicks link   Release status
    user waits until page contains heading 2  Release status
    user waits until page contains button  Edit release status

Approve release
    [Tags]  HappyPath
    ${PUBLISH_DATE_DAY}=  get datetime  %d
    ${PUBLISH_DATE_MONTH}=  get datetime  %m
    ${PUBLISH_DATE_MONTH_WORD}=  get datetime  %B
    ${PUBLISH_DATE_YEAR}=  get datetime  %Y
    set suite variable  ${PUBLISH_DATE_DAY}
    set suite variable  ${PUBLISH_DATE_MONTH}
    set suite variable  ${PUBLISH_DATE_MONTH_WORD}
    set suite variable  ${PUBLISH_DATE_YEAR}

    user clicks button  Edit release status
    user waits until page contains heading 2  Edit release status

    user clicks element   css:input[data-testid="Approved for publication"]
    user enters text into element  id:releaseStatusForm-internalReleaseNote  Approved by UI tests
    user clicks element  css:input[data-testid="As soon as possible"]
    user enters text into element  id:releaseStatusForm-nextReleaseDate-month   12
    user enters text into element  id:releaseStatusForm-nextReleaseDate-year    3001

    user clicks button   Update status

Wait for release process status to be Complete
    [Tags]  HappyPath
    # EES-1007 - Release process status doesn't automatically update
    user waits until page contains heading 2  Release status
    user checks summary list item "Current status" should be "Approved"
    user checks summary list item "Scheduled release" should be "${PUBLISH_DATE_DAY} ${PUBLISH_DATE_MONTH_WORD} ${PUBLISH_DATE_YEAR}"
    user waits for release process status to be  Complete    900
    user checks page does not contain button  Edit release status

User goes to public Find Statistics page
    [Tags]  HappyPath
    environment variable should be set   PUBLIC_URL
    user goes to url   %{PUBLIC_URL}/find-statistics
    user waits until page contains heading  Find statistics and data
    user waits for page to finish loading

Verify newly published release is on Find Statistics page
    [Tags]  HappyPath
    user checks page contains accordion   Test theme
    user opens accordion section  Test theme
    user checks accordion section contains text   Test theme   ${TOPIC_NAME}

    user opens details dropdown  ${TOPIC_NAME}
    user checks details dropdown contains publication    ${TOPIC_NAME}  ${PUBLICATION_NAME}
    user checks publication bullet contains link   ${PUBLICATION_NAME}  View statistics and data
    user checks publication bullet contains link   ${PUBLICATION_NAME}  Create your own tables online
    user checks publication bullet does not contain link  ${PUBLICATION_NAME}   Statistics at DfE

Go to Table Tool page
    [Tags]  HappyPath
    user goes to url  %{PUBLIC_URL}/data-tables
    user waits until page contains heading  Create your own tables online

Select publication
    [Tags]  HappyPath
    user opens details dropdown    Test theme
    user opens details dropdown    ${TOPIC_NAME}
    user selects radio      ${PUBLICATION_NAME}
    user clicks element    css:#publicationForm-submit

Select subject "UI test subject"
    [Tags]  HappyPath
    user waits until page contains   UI test subject
    user selects radio    UI test subject
    user clicks element   css:#publicationSubjectForm-submit

Select locations
    [Tags]   HappyPath
    user waits until element is visible  xpath://h2[text()="Choose locations"]     90
    user opens details dropdown   Local Authority
    user clicks checkbox   Barnsley
    user clicks checkbox   Birmingham
    user clicks element     css:#locationFiltersForm-submit

Select time period
    [Tags]   HappyPath
    user waits until element is visible  xpath://h2[text()="Choose time period"]   90
    user selects start date    2014
    user selects end date      2018
    user clicks element     css:#timePeriodForm-submit

Select indicators
    [Tags]  HappyPath
    user waits until element is visible  xpath://h2[text()="Choose your filters"]
    user clicks indicator checkbox    Admission Numbers
    user clicks element   css:#filtersForm-submit

Validate table column headings
    [Tags]  HappyPath
    user waits until results table appears  180
    user checks results table column heading contains  css:table  1  1  2014
    user checks results table column heading contains  css:table  1  2  2015
    user checks results table column heading contains  css:table  1  3  2016
    user checks results table column heading contains  css:table  1  4  2017
    user checks results table column heading contains  css:table  1  5  2018

Validate table rows for Barnsley
    [Tags]  HappyPath
    ${row}=  user gets row number with heading  Barnsley
    user checks results table cell in offset row contains  ${row}  0  1  9,854
    user checks results table cell in offset row contains  ${row}  0  2  1,134
    user checks results table cell in offset row contains  ${row}  0  3  7,419
    user checks results table cell in offset row contains  ${row}  0  4  5,032
    user checks results table cell in offset row contains  ${row}  0  5  8,123

Validate table rows for Birmingham
    [Tags]  HappyPath
    ${row}=  user gets row number with heading   Birmingham
    user checks results table cell in offset row contains  ${row}  0  1  3,708
    user checks results table cell in offset row contains  ${row}  0  2  9,303
    user checks results table cell in offset row contains  ${row}  0  3  8,856
    user checks results table cell in offset row contains  ${row}  0  4  8,530
    user checks results table cell in offset row contains  ${row}  0  5  3,962

Select table highlight from subjects step
    [Tags]  HappyPath
    user clicks element  css:[data-testid="wizardStep-2-goToButton"]
    user waits until page contains heading 1  Go back to previous step
    user clicks button  Confirm

    user waits until element is visible  xpath://h3[text()="Table highlights"]
    user clicks link  Test highlight name
    user waits until results table appears  180
    user waits until page contains element   xpath://*[@id="dataTableCaption" and text()="Table showing Admission Numbers for 'UI test subject' from '${PUBLICATION_NAME}' in Bolton 001 (E02000984), Bolton 001 (E05000364), Bolton 004 (E02000987), Bolton 004 (E05010450), Nailsea Youngwood and Syon between 2005 and 2020"]

Validate table column headings for table highlight
    [Tags]  HappyPath
    user checks results table column heading contains  css:table  1  1  Admission Numbers

Validate table rows for Bolton 001 (E02000984)
    [Tags]  HappyPath
    ${row}=  user gets row number with heading  Bolton 001 (E02000984)
    user checks results table heading in offset row contains  ${row}  0  2  2019

    user checks results table cell in offset row contains  ${row}  0  1  8,533

Validate table rows for Bolton 001 (E05000364)
    [Tags]  HappyPath
    ${row}=  user gets row number with heading   Bolton 001 (E05000364)
    user checks results table heading in offset row contains  ${row}  0  2  2009
    user checks results table heading in offset row contains  ${row}  1  1  2010
    user checks results table heading in offset row contains  ${row}  2  1  2017

    user checks results table cell in offset row contains  ${row}  0  1  5,815
    user checks results table cell in offset row contains  ${row}  1  1  5,595
    user checks results table cell in offset row contains  ${row}  2  1  6,373

Validate table rows for Bolton 004 (E02000987)
    [Tags]  HappyPath
    ${row}=  user gets row number with heading   Bolton 004 (E02000987)
    user checks results table heading in offset row contains  ${row}  0  2  2020

    user checks results table cell in offset row contains  ${row}  0  1  6,031

Validate table rows for Bolton 004 (E05010450)
    [Tags]  HappyPath
    ${row}=  user gets row number with heading   Bolton 004 (E05010450)
    user checks results table heading in offset row contains  ${row}  0  2  2005
    user checks results table heading in offset row contains  ${row}  1  1  2017
    user checks results table heading in offset row contains  ${row}  2  1  2018

    user checks results table cell in offset row contains  ${row}  0  1  8,557
    user checks results table cell in offset row contains  ${row}  1  1  3,481
    user checks results table cell in offset row contains  ${row}  2  1  8,630

Validate table rows for Nailsea Youngwood
    [Tags]  HappyPath
    ${row}=  user gets row number with heading   Nailsea Youngwood
    user checks results table heading in offset row contains  ${row}  0  2  2005
    user checks results table heading in offset row contains  ${row}  1  1  2010
    user checks results table heading in offset row contains  ${row}  2  1  2011
    user checks results table heading in offset row contains  ${row}  3  1  2012
    user checks results table heading in offset row contains  ${row}  4  1  2016

    user checks results table cell in offset row contains  ${row}  0  1  3,612
    user checks results table cell in offset row contains  ${row}  1  1  9,304
    user checks results table cell in offset row contains  ${row}  2  1  9,603
    user checks results table cell in offset row contains  ${row}  3  1  8,150
    user checks results table cell in offset row contains  ${row}  4  1  4,198

Validate table rows for Syon
    [Tags]  HappyPath
    ${row}=  user gets row number with heading   Syon
    user checks results table heading in offset row contains  ${row}  0  2  2007
    user checks results table heading in offset row contains  ${row}  1  1  2008
    user checks results table heading in offset row contains  ${row}  2  1  2010
    user checks results table heading in offset row contains  ${row}  3  1  2012
    user checks results table heading in offset row contains  ${row}  4  1  2017

    user checks results table cell in offset row contains  ${row}  0  1  9,914
    user checks results table cell in offset row contains  ${row}  1  1  5,505
    user checks results table cell in offset row contains  ${row}  2  1  6,060
    user checks results table cell in offset row contains  ${row}  3  1  1,109
    user checks results table cell in offset row contains  ${row}  4  1  1,959
