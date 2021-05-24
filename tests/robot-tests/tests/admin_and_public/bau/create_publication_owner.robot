*** Settings ***
Resource    ../../libs/admin-common.robot
Library     ../../libs/admin_api.py

Force Tags  Admin  Local  Dev  AltersData  Footnotes

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${TOPIC_NAME}        %{TEST_TOPIC_NAME}
${PUBLICATION_NAME}  UI tests - publication_owner %{RUN_IDENTIFIER}
${RELEASE_NAME}      ${PUBLICATION_NAME} - Academic Year 2025/26
${RELEASE_TYPE}      Academic Year 2025/26
${SUBJECT_NAME}      UI test subject
${DATA_FILE_NAME}    dates

${FOOTNOTE_TEXT_1}   test footnote from the bau user
${FOOTNOTE_TEXT_2}   test footnote from the publication owner! (analyst)
${FOOTNOTE_TEXT_3}   A footnote as analyst1 User1 with the Lead role on release ${RELEASE_NAME}

${FOOTNOTE_DATABLOCK_NAME}  test data block (footnotes)

*** Test Cases ***
Create new publication and release via API
    [Tags]  HappyPath
    ${PUBLICATION_ID}=  user creates test publication via api   ${PUBLICATION_NAME}
    user create test release via api  ${PUBLICATION_ID}   AY    2025

Navigate to admin dashboard and assert publication is present
    [Tags]  HappyPath
    user goes to url  %{ADMIN_URL}
    user selects theme and topic from admin dashboard  %{TEST_THEME_NAME}  %{TEST_TOPIC_NAME}
    user waits until page contains accordion section   ${PUBLICATION_NAME}  120

Navigate to manage users page
    [Tags]  HappyPath
    user goes to url  %{ADMIN_URL}/administration/users
    user checks table column heading contains  1  1  Name
    user checks table column heading contains  1  2  Email
    user checks table column heading contains  1  3  Role
    user checks table column heading contains  1  4  Actions

Assert that test users are present in table
    [Tags]  HappyPath
    user checks results table row heading contains  1  1  Analyst1 User1
    user checks results table row heading contains  2  1  Analyst2 User2
    user checks results table row heading contains  3  1  Analyst3 User3
    user checks results table row heading contains  4  1  Bau1 User1
    user checks results table row heading contains  5  1  Bau2 User2

    user checks results table cell contains  1  1  	ees-analyst1@education.gov.uk
    user checks results table cell contains  1  2  	Analyst
    user checks results table cell contains  1  3  	Manage

    user checks results table cell contains  1  1  	ees-analyst1@education.gov.uk
    user checks results table cell contains  2  2  	Analyst
    user checks results table cell contains  1  3  	Manage

    user checks results table cell contains  1  1  	ees-analyst1@education.gov.uk
    user checks results table cell contains  3  2  	Analyst
    user checks results table cell contains  1  3  	Manage

    user checks results table cell contains  1  1  	ees-analyst1@education.gov.uk
    user checks results table cell contains  4  2  	BAU User
    user checks results table cell contains  1  3  	Manage

    user checks results table cell contains  1  1  	ees-analyst1@education.gov.uk
    user checks results table cell contains  5  2  	BAU User
    user checks results table cell contains  1  3  	Manage

Give Analyst1 User1 publication owner role
    [Tags]  HappyPath
    user waits until element is enabled  //*[tbody]//tr[1]//td[3]//a
    user clicks element  //*[tbody]//tr[1]//td[3]//a
    user waits until element is enabled  css:[name="selectedPublicationId"]
    user selects from list by label  css:[name="selectedPublicationId"]  ${PUBLICATION_NAME}

    user waits until element is enabled  css:[name="selectedPublicationRole"]
    user selects from list by label  css:[name="selectedPublicationRole"]  Owner
    user clicks button  Add publication access

Give Analyst1 User1 release access
    [Tags]  HappyPath
    # stale element exception if you don't wait until it's enabled
    user waits until element is enabled  css:[name="selectedReleaseId"]
    user selects from list by label  css:[name="selectedReleaseId"]  ${RELEASE_NAME}

    user waits until element is enabled  css:[name="selectedReleaseRole"]
    user selects from list by label  css:[name="selectedReleaseRole"]  Approver
    user clicks button  Add release access

Sign in as Analyst1 User1 & navigate to publication
    [Tags]  HappyPath
    user signs in as analyst1
    user goes to url  %{ADMIN_URL}
    user selects theme and topic from admin dashboard  %{TEST_THEME_NAME}  %{TEST_TOPIC_NAME}
    user waits until page contains accordion section   ${PUBLICATION_NAME}  120

Navigate to release
    [Tags]  HappyPath
    user navigates to release summary from admin dashboard  ${PUBLICATION_NAME}  Academic Year 2025/26 (not Live)

Upload subject file
    [Tags]  HappyPath
    user clicks link  Data and files
    user waits until page contains element  id:dataFileUploadForm-subjectTitle
    user enters text into element  id:dataFileUploadForm-subjectTitle   ${SUBJECT_NAME}
    user chooses file   id:dataFileUploadForm-dataFile       ${FILES_DIR}seven_filters.csv
    user chooses file   id:dataFileUploadForm-metadataFile   ${FILES_DIR}seven_filters.meta.csv
    user clicks button  Upload data files
    user waits until h2 is visible  Uploaded data files
    user waits until page contains accordion section   ${SUBJECT_NAME}
    user opens accordion section   ${SUBJECT_NAME}
    ${section}=  user gets accordion section content element  ${SUBJECT_NAME}
    user checks headed table body row contains  Status           Complete  ${section}  180

Add meta guidance to ${SUBJECT_NAME} subject
    [Tags]  HappyPath
    user clicks link  Metadata guidance
    user waits until h2 is visible  Public metadata guidance document  90
    user enters text into element  id:metaGuidanceForm-content  Test meta guidance content
    user waits until page contains accordion section  ${SUBJECT_NAME}
    user enters text into meta guidance data file content editor  ${SUBJECT_NAME}
    ...  meta guidance content
    user clicks button  Save guidance

Navigate to 'Footnotes' page
    [Tags]  HappyPath
    user clicks link  Footnotes
    user waits until h2 is visible  Footnotes

Add footnote to ${SUBJECT_NAME}
    [Tags]  HappyPath
    user waits until page contains link   Create footnote
    user clicks link  Create footnote
    user waits until h2 is visible  Create footnote
    user clicks footnote radio   ${SUBJECT_NAME}   Applies to all data
    user clicks element  id:footnoteForm-content
    user enters text into element  id:footnoteForm-content  Footnote 1 ${FOOTNOTE_TEXT_2}
    user clicks button  Save footnote
    user waits until h2 is visible  Footnotes

Add public prerelease access list
    [Tags]  HappyPath
    user clicks link  Pre-release access
    user waits until h2 is visible  Manage pre-release user access
    user creates public prerelease access list   Test public access list

Go to "Sign off" page
    [Tags]  HappyPath
    user clicks link   Sign off
    user waits until h2 is visible  Sign off
    user waits until page contains button  Edit release status

Check analyst1 User1 can approve release for immediate publication
    [Tags]  HappyPath
    user clicks button  Edit release status
    user waits until h2 is visible  Edit release status
    user clicks radio   Approved for publication
    user enters text into element  id:releaseStatusForm-internalReleaseNote  Approved by publication owner
    user clicks radio   As soon as possible
    user clicks button   Update status

Wait for release process status to be Complete
    [Tags]  HappyPath
    user waits for release process status to be  Complete    ${release_complete_wait}
    user reloads page  # EES-1448
    user checks page does not contain button  Edit release status

Navigate to administration as bau1
    [Tags]  HappyPath
    user signs in as bau1
    user goes to url  %{ADMIN_URL}/administration/users

Give analyst1 user1 contributor / approver only access on release ${RELEASE_NAME}
    [Tags]  HappyPath
    user waits until element is enabled  //*[tbody]//tr[1]//td[3]//a
    user clicks element  //*[tbody]//tr[1]//td[3]//a
    user waits until element is enabled  css:[name="selectedReleaseId"]

    user selects from list by label  css:[name="selectedReleaseId"]  ${RELEASE_NAME}
    user waits until element is enabled  css:[name="selectedReleaseRole"]
    user selects from list by label  css:[name="selectedReleaseRole"]  Contributor
    user clicks button  Add release access

Create amendment as analyst1
    [Tags]  HappyPath
    user signs in as analyst1
    user goes to url  %{ADMIN_URL}
    user selects theme and topic from admin dashboard  %{TEST_THEME_NAME}  %{TEST_TOPIC_NAME}
    user waits until page contains accordion section   ${PUBLICATION_NAME}
    user opens accordion section  ${PUBLICATION_NAME}
    ${accordion}=  user gets accordion section content element  ${PUBLICATION_NAME}
    user opens details dropdown   ${RELEASE_NAME} (Live - Latest release)  ${accordion}
    ${details}=  user gets details content element  Academic Year 2025/26 (Live - Latest release)  ${accordion}
    user waits until parent contains element   ${details}   xpath:.//a[text()="View this release"]
    user clicks button  Amend this release
    user clicks button  Confirm

Navigate to 'Footnotes' section
    [Tags]  HappyPath
    user clicks link  Footnotes
    user waits until h2 is visible  Footnotes

Add a Footnote as a release contributor
    [Tags]  HappyPath
    user waits until page contains link   Create footnote
    user clicks link  Create footnote
    user waits until h2 is visible  Create footnote
    user clicks footnote radio   ${SUBJECT_NAME}   Applies to all data
    user clicks element  id:footnoteForm-content
    user enters text into element  id:footnoteForm-content  Footnote 3 ${FOOTNOTE_TEXT_3}
    user clicks button  Save footnote
    user waits until h2 is visible  Footnotes

Navigate to content
    user clicks link Content

    Sleep  10000
