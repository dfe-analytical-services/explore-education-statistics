*** Settings ***
Resource    ../../libs/admin-common.robot
Resource    ../../libs/public-common.robot
Library     ../../libs/admin_api.py

Force Tags  Admin  Local  Dev  AltersData  Footnotes

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${TOPIC_NAME}        %{TEST_TOPIC_NAME}
${PUBLICATION_NAME}  UI tests - footnotes %{RUN_IDENTIFIER}
${SUBJECT_NAME}      UI test subject

${DATA_FILE_NAME}  dates

${FOOTNOTE_TEXT_1}  test footnote text (will be deleted)
${FOOTNOTE_TEXT_2}  A test footnote
${FOOTNOTE_TEXT_3}  A edited test footnote!

${FOOTNOTE_DATABLOCK_NAME}  test data block (footnotes)

*** Test Cases ***
Create new publication and release via API
    [Tags]  HappyPath
    ${PUBLICATION_ID}=  user creates test publication via api   ${PUBLICATION_NAME}
    user create test release via api  ${PUBLICATION_ID}   AY    2025

Upload subject
    [Tags]  HappyPath
    user navigates to release summary from admin dashboard  ${PUBLICATION_NAME}   Academic Year 2025/26 (not Live)

    user clicks link  Data and files
    user waits until page contains element  id:dataFileUploadForm-subjectTitle
    user enters text into element  id:dataFileUploadForm-subjectTitle   ${SUBJECT_NAME}
    user chooses file   id:dataFileUploadForm-dataFile       ${FILES_DIR}${DATA_FILE_NAME}.csv
    user chooses file   id:dataFileUploadForm-metadataFile   ${FILES_DIR}${DATA_FILE_NAME}.meta.csv
    user clicks button  Upload data files

    user waits until h2 is visible  Uploaded data files
    user waits until page contains accordion section   ${SUBJECT_NAME}
    user opens accordion section   ${SUBJECT_NAME}

    ${section}=  user gets accordion section content element  ${SUBJECT_NAME}
    user checks headed table body row contains  Subject title    ${SUBJECT_NAME}  ${section}
    user checks headed table body row contains  Data file        dates.csv  ${section}
    user checks headed table body row contains  Metadata file    dates.meta.csv  ${section}
    user checks headed table body row contains  Number of rows   119  ${section}
    user checks headed table body row contains  Data file size   17 Kb  ${section}
    user checks headed table body row contains  Status           Complete  ${section}  360

Add meta guidance to subject
    [Tags]  HappyPath
    user clicks link  Metadata guidance
    user waits until h2 is visible  Public metadata guidance document

    user waits until page contains accordion section  ${SUBJECT_NAME}
    ${editor}=  user gets meta guidance data file content editor  ${SUBJECT_NAME}
    user clicks element  ${editor}
    user presses keys  ${SUBJECT_NAME} test meta guidance content

    user clicks button  Save guidance

Navigate to 'Footnotes' page
    [Tags]  HappyPath
    user clicks link  Footnotes
    user waits until h2 is visible  Footnotes

Create footnote
    [Tags]  HappyPath
    user clicks link  Create footnote
    user waits until h2 is visible  Create footnote

    user opens details dropdown  Indicators
    user opens details dropdown  Date
    user clicks footnote checkbox  Number of open settings
    user checks footnote checkbox is selected  Number of open settings
    user clicks footnote checkbox  01/04/2020
    user checks footnote checkbox is selected  01/04/2020
    user enters text into element  id:footnoteForm-content  ${FOOTNOTE_TEXT_1}
    user clicks button  Save footnote
    user waits until h2 is visible  Footnotes

Create another footnote
    [Tags]  HappyPath
    user clicks link  Create footnote
    user waits until h2 is visible  Create footnote

    user opens details dropdown  Indicators
    user opens details dropdown  Date
    user clicks footnote checkbox  Proportion of settings open
    user checks footnote checkbox is selected  Proportion of settings open
    user clicks footnote checkbox  01/04/2021
    user checks footnote checkbox is selected  01/04/2021
    user enters text into element  id:footnoteForm-content  ${FOOTNOTE_TEXT_2}
    user clicks button  Save footnote

Confirm created footnotes
    [Tags]  HappyPath
    user waits until h2 is visible  Footnotes
    user waits until page contains  ${FOOTNOTE_TEXT_1}
    user waits until page contains  ${FOOTNOTE_TEXT_2}

Navigate to 'Data blocks' tab
    [Tags]  HappyPath
    user clicks link    Data blocks
    user waits until h2 is visible   Choose a subject

Select subject "${SUBJECT_NAME}" (data block)
    [Tags]  HappyPath
    user waits until page contains   ${SUBJECT_NAME}
    user clicks radio    ${SUBJECT_NAME}
    user clicks element   id:publicationSubjectForm-submit

Select locations (data block)
    [Tags]   HappyPath
    user waits until h2 is visible  Choose locations
    user opens details dropdown   National
    user clicks checkbox   England
    user clicks element     id:locationFiltersForm-submit

Select time period
    [Tags]   HappyPath
    user waits until h2 is visible  Choose time period
    user selects from list by label  id:timePeriodForm-start    2020 Week 13
    user selects from list by label  id:timePeriodForm-end      2021 Week 24
    user clicks element     id:timePeriodForm-submit

Select indicators (data block)
    [Tags]  HappyPath
    user waits until h2 is visible  Choose your filters
    user clicks button  Select all 2 subgroup options

Select category filters (data block)
    [Tags]  HappyPath
    user opens details dropdown  Date
    user clicks category checkbox    Date  01/04/2020
    user checks category checkbox is checked  Date  01/04/2020
    user clicks category checkbox    Date  01/04/2021
    user checks category checkbox is checked  Date  01/04/2021

Create table (data block)
    [Tags]  HappyPath
    user clicks element   id:filtersForm-submit
    user waits until results table appears     180

Check footnotes in table preview (data block)
    user checks page contains  ${FOOTNOTE_TEXT_1}
    user checks page contains  ${FOOTNOTE_TEXT_2}

Save data block
    [Tags]  HappyPath
    user enters text into element  id:dataBlockDetailsForm-name  ${FOOTNOTE_DATABLOCK_NAME}
    user enters text into element  id:dataBlockDetailsForm-heading  ${FOOTNOTE_DATABLOCK_NAME}
    user clicks button  Save data block
    user waits until h2 is visible  ${FOOTNOTE_DATABLOCK_NAME}

Go back and delete a footnote
    [Tags]  HappyPath
    user clicks link  Footnotes
    user clicks button  Delete footnote
    user clicks button  Confirm
    user waits until page does not contain  ${FOOTNOTE_TEXT_1}
    user checks page contains  ${FOOTNOTE_TEXT_2}

Add data block to release
    [Tags]  HappyPath
    user clicks link  Content
    user clicks button  Add secondary stats
    user waits until page contains element  secondaryStats-dataBlockSelectForm-selectedDataBlock
    user selects from list by label  secondaryStats-dataBlockSelectForm-selectedDataBlock  ${FOOTNOTE_DATABLOCK_NAME}
    user waits until page contains element    css:table
    user clicks button  Embed
    user clicks element   id:releaseHeadlines-dataBlock-tables-tab
    user checks page contains  ${FOOTNOTE_TEXT_2}
    user checks page does not contain  ${FOOTNOTE_TEXT_1}

Edit footnote
    [Tags]  HappyPath
    user clicks link  Footnotes
    user waits until h2 is visible  Footnotes
    user clicks link  Edit footnote

    user waits until h2 is visible  Edit footnote
    user enters text into element  id:footnoteForm-content  ${FOOTNOTE_TEXT_3}
    user clicks button  Save footnote
    user waits until page contains  ${FOOTNOTE_TEXT_3}
    user checks page does not contain  ${FOOTNOTE_TEXT_2}

Check footnote was updated
    [Tags]  HappyPath
    user clicks link  Content
    user waits until page contains element  id:releaseHeadlines
    user scrolls to element  id:releaseHeadlines
    user waits until page contains link  Table  180
    user clicks link  Table
    user checks page contains  ${FOOTNOTE_TEXT_3}
    user checks page does not contain  ${FOOTNOTE_TEXT_2}
    user checks page does not contain  ${FOOTNOTE_TEXT_1}

Check footnote in Preview content mode
    [Tags]  HappyPath
    user clicks element   id:pageMode-preview
    user checks page contains  ${FOOTNOTE_TEXT_3}
    user checks page does not contain  ${FOOTNOTE_TEXT_2}
    user checks page does not contain  ${FOOTNOTE_TEXT_1}

Go to "Release status" page
    [Tags]  HappyPath
    user clicks link   Release status
    user waits until h2 is visible  Release status
    user waits until page contains button  Edit release status

Approve release
    [Tags]  HappyPath
    ${PUBLISH_DATE_DAY}=  get current datetime  %-d
    ${PUBLISH_DATE_MONTH}=  get current datetime  %-m
    ${PUBLISH_DATE_MONTH_WORD}=  get current datetime  %B
    ${PUBLISH_DATE_YEAR}=  get current datetime  %Y
    set suite variable  ${PUBLISH_DATE_DAY}
    set suite variable  ${PUBLISH_DATE_MONTH}
    set suite variable  ${PUBLISH_DATE_MONTH_WORD}
    set suite variable  ${PUBLISH_DATE_YEAR}

    user clicks button  Edit release status
    user waits until h2 is visible  Edit release status

    user clicks radio   Approved for publication
    user enters text into element  id:releaseStatusForm-internalReleaseNote  Approved by UI tests
    user clicks radio   As soon as possible
    user enters text into element  id:releaseStatusForm-nextReleaseDate-month   12
    user enters text into element  id:releaseStatusForm-nextReleaseDate-year    3001

    user clicks button   Update status

Verify release is scheduled
    [Tags]  HappyPath
    user waits until h2 is visible  Release status
    user checks summary list contains  Current status  Approved
    user checks summary list contains  Scheduled release  ${PUBLISH_DATE_DAY} ${PUBLISH_DATE_MONTH_WORD} ${PUBLISH_DATE_YEAR}
    user checks summary list contains  Next release expected  December 3001

Wait for release process status to be Complete
    [Tags]  HappyPath
    user waits for release process status to be  Complete    ${release_complete_wait}
    user reloads page  # EES-1448
    user checks page does not contain button  Edit release status

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

Check footnote on data block
    [Tags]  HappyPath
    user waits until page contains element  id:releaseHeadlines-tables-tab
    user clicks element  id:releaseHeadlines-tables-tab
    user scrolls to element  xpath://h3[.="Footnotes"]
    user checks page contains  ${FOOTNOTE_TEXT_3}
    user checks page does not contain  ${FOOTNOTE_TEXT_2}
    user checks page does not contain  ${FOOTNOTE_TEXT_1}

Navigate to table tool
    [Tags]  HappyPath
    user goes to url   %{PUBLIC_URL}/data-tables
    user waits for page to finish loading

Choose publication
    [Tags]  HappyPath
    user waits until page contains   Choose a publication
    user opens details dropdown  %{TEST_THEME_NAME}
    user opens details dropdown  ${TOPIC_NAME}
    user clicks radio  ${PUBLICATION_NAME}
    user clicks button  Next step

Choose subject (table tool)
    [Tags]  HappyPath
    user waits until page contains   ${SUBJECT_NAME}
    user clicks radio  ${SUBJECT_NAME}
    user clicks button  Next step

Select locations (table tool)
    [Tags]   HappyPath
    user waits until h2 is visible  Choose locations
    user opens details dropdown   National
    user clicks checkbox   England
    user clicks element     id:locationFiltersForm-submit

Select time period (table tool)
    [Tags]   HappyPath
    user waits until h2 is visible  Choose time period
    user selects from list by label  id:timePeriodForm-start    2020 Week 13
    user selects from list by label  id:timePeriodForm-end      2021 Week 24
    user clicks element     id:timePeriodForm-submit

Select indicators (table tool)
    [Tags]  HappyPath
    user waits until h2 is visible  Choose your filters
    user clicks button  Select all 2 subgroup options

Select category filters (table tool)
    [Tags]  HappyPath
    user opens details dropdown  Date
    user clicks category checkbox    Date  01/04/2020
    user checks category checkbox is checked  Date  01/04/2020
    user clicks category checkbox    Date  01/04/2021
    user checks category checkbox is checked  Date  01/04/2021

Create table (table tool)
    [Tags]  HappyPath
    user clicks element   id:filtersForm-submit
    user waits until results table appears     180

Check footnotes in table preview (table tool)
    user checks page contains  ${FOOTNOTE_TEXT_3}
    user checks page does not contain  ${FOOTNOTE_TEXT_2}
    user checks page does not contain  ${FOOTNOTE_TEXT_1}
