*** Settings ***
Resource    ../../libs/admin-common.robot

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${TOPIC_NAME}        %{TEST_TOPIC_NAME}
${PUBLICATION_NAME}  UI tests - delete subject %{RUN_IDENTIFIER}

*** Test Cases ***
Go to Create publication page for "UI tests topic" topic
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "${TOPIC_NAME}" from the admin dashboard
    user waits until page contains link    Create new publication
    user checks page does not contain button   ${PUBLICATION_NAME}
    user clicks link  Create new publication
    user waits until page contains heading 1  Create new publication
    user creates publication    ${PUBLICATION_NAME}

Verify that new publication has been created
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "${TOPIC_NAME}" from the admin dashboard
    user waits until page contains accordion section   ${PUBLICATION_NAME}
    user opens accordion section  ${PUBLICATION_NAME}
    user checks testid element contains  Methodology for ${PUBLICATION_NAME}  No methodology assigned
    user checks testid element contains  Releases for ${PUBLICATION_NAME}  No releases created

Create new release
    [Tags]   HappyPath
    user clicks element  css:[data-testid="Create new release link for ${PUBLICATION_NAME}"]
    user waits until page contains heading 1  Create new release

User fills in form
    [Tags]  HappyPath
    user waits until page contains element  id:releaseSummaryForm-timePeriodCoverage
    user selects from list by label  id:releaseSummaryForm-timePeriodCoverage  Tax Year
    user enters text into element  id:releaseSummaryForm-timePeriodCoverageStartYear  2020
    user clicks radio  Ad Hoc

Click Create new release button
    [Tags]   HappyPath
    user clicks button   Create new release
    user waits until page contains title caption  Edit release
    user waits until page contains heading 1  ${PUBLICATION_NAME}

Verify Release summary
    [Tags]  HappyPath
    user checks page contains element   xpath://li/a[text()="Release summary" and contains(@aria-current, 'page')]
    user waits until page contains heading 2    Release summary
    user checks summary list contains  Publication title  ${PUBLICATION_NAME}
    user checks summary list contains  Time period  Tax Year
    user checks summary list contains  Release period  2020-21
    user checks summary list contains  Lead statistician  Tingting Shu
    user checks summary list contains  Scheduled release  Not scheduled
    user checks summary list contains  Next release expected  Not set
    user checks summary list contains  Release type  Ad Hoc

Upload subject
    [Tags]  HappyPath
    user clicks link  Manage data
    user enters text into element  id:dataFileUploadForm-subjectTitle   UI test subject
    choose file   id:dataFileUploadForm-dataFile       ${CURDIR}${/}files${/}upload-file-test-with-filter.csv
    choose file   id:dataFileUploadForm-metadataFile   ${CURDIR}${/}files${/}upload-file-test-with-filter.meta.csv
    user clicks button  Upload data files
    user waits until page contains element   xpath://h2[text()="Uploaded data files"]
    user waits until page contains accordion section   UI test subject
    user opens accordion section   UI test subject
    user checks page contains element   xpath://dt[text()="Subject title"]/../dd/h4[text()="UI test subject"]
    user waits until page contains element  xpath://dt[text()="Status"]/../dd//strong[text()="Complete"]     180

Navigate to Footnotes tab
    [Tags]  HappyPath
    user clicks link  Footnotes
    user waits until page contains heading 2  Footnotes

Create subject footnote for new subject
    [Tags]  HappyPath
    user clicks element   id:add-footnote-button
    user waits until page contains testid  footnote-subject UI test subject
    user clicks checkbox  Select all indicators and filters for this subject
    user clicks element   id:create-footnote-form-content
    user presses keys  UI tests subject footnote
    user clicks button   Create footnote

Create indicator footnote for new subject
    [Tags]  HappyPath
    user clicks element  id:add-footnote-button
    user waits until page contains testid  footnote-subject UI test subject
    user opens details dropdown  Indicators
    user clicks checkbox  Admission Numbers
    user clicks element   id:create-footnote-form-content
    user presses keys  UI tests indicator Admission Numbers footnote
    user clicks button   Create footnote

Create Random Filter Total footnote for new subject
    [Tags]  HappyPath
    user clicks element  id:add-footnote-button
    user waits until page contains testid  footnote-subject UI test subject
    user opens details dropdown   Random Filter
    user clicks checkbox   Total
    user clicks element   id:create-footnote-form-content
    user presses keys  UI tests Random Filter Total footnote
    user clicks button   Create footnote

Create Random Filter Select all footnote for new subject
    [Tags]  HappyPath
    user clicks element  id:add-footnote-button
    user waits until page contains testid  footnote-subject UI test subject
    user opens details dropdown   Random Filter
    user clicks checkbox   Select all
    user clicks element   id:create-footnote-form-content
    user presses keys  UI tests Random Filter Select all footnote
    user clicks button   Create footnote

Navigate to Manage data blocks tab
    [Tags]  HappyPath
    user clicks link    Manage data blocks
    user waits until page contains heading 2   Choose a subject

Select subject "UI test subject"
    [Tags]  HappyPath
    user waits until page contains   UI test subject
    user clicks radio    UI test subject
    user clicks element   id:publicationSubjectForm-submit
    user waits until page contains heading 2   Choose locations
    user checks previous table tool step contains  1    Subject     UI test subject

Select locations
    [Tags]   HappyPath
    user opens details dropdown   Opportunity Area
    user clicks checkbox   Bolton 001 (E02000984)
    user opens details dropdown   Ward
    user clicks checkbox   Nailsea Youngwood
    user clicks checkbox   Syon
    user clicks element     id:locationFiltersForm-submit
    user waits until page contains heading 2  Choose time period

Select time period
    [Tags]   HappyPath
    ${timePeriodStartList}=   get list items  id:timePeriodForm-start
    ${timePeriodEndList}=   get list items  id:timePeriodForm-end
    user selects from list by label  id:timePeriodForm-start  2019
    user selects from list by label  id:timePeriodForm-end  2019
    user clicks element     id:timePeriodForm-submit
    user waits until page contains heading 2  Choose your filters

Select indicators
    [Tags]  HappyPath
    user clicks indicator checkbox    Admission Numbers

Create table
    [Tags]  HappyPath
    [Documentation]   EES-615
    user clicks element   id:filtersForm-submit
    user waits until results table appears     180
    user waits until element contains   id:dataTableCaption
    ...  Table showing Admission Numbers for 'UI test subject' from '${PUBLICATION_NAME}' in Bolton 001 for 2019
    user enters text into element  id:dataBlockDetailsForm-name         UI test table name
    user enters text into element  id:dataBlockDetailsForm-heading      UI test table title
    user enters text into element  id:dataBlockDetailsForm-source       UI test source
    user clicks button   Save data block
    user waits until page contains    Delete this data block

Navigate to Create chart tab
    [Tags]  HappyPath
    user waits until page contains link  Chart
    user clicks link  Chart
    user clicks button  Choose an infographic as alternative
    choose file   id:chartConfigurationForm-file       ${CURDIR}${/}files${/}dfe-logo.jpg
    user enters text into element  id:chartConfigurationForm-title  Sample title
    user enters text into element  id:chartConfigurationForm-alt  Sample alt text
    user clicks button   Save chart options
    user waits until page contains  Chart preview

Navigate back to Manage data tab
    [Tags]  HappyPath
    user clicks link  Manage data
    user waits until page contains link  Data uploads

Delete UI test subject
    [Tags]  HappyPath
    user clicks link  Data uploads
    user waits until page contains element  xpath://legend[text()="Add new data to release"]
    user waits until page contains accordion section  UI test subject
    user opens accordion section   UI test subject
    user clicks button   Delete files
    user waits until page contains heading 1   Confirm deletion of selected data files
    user checks page contains   4 footnotes will be removed or updated.
    user checks page contains   The following data blocks will also be deleted:
    user checks page contains   UI test table name
    user checks page contains   The following infographic files will also be removed:
    user checks page contains   dfe-logo.jpg
    user clicks button  Confirm
    user waits until page does not contain accordion section   UI test subject
    user checks page contains element  xpath://legend[text()="Add new data to release"]