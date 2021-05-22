*** Settings ***
Resource    ../../libs/admin-common.robot
Library     ../../libs/admin_api.py

Force Tags  Admin  Local  Dev  AltersData  Footnotes

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${TOPIC_NAME}        %{TEST_TOPIC_NAME}
${PUBLICATION_NAME}  UI tests - publication_owner %{RUN_IDENTIFIER}
${RELEASE_NAME}  ${PUBLICATION_NAME} - Academic Year 2025/26
${SUBJECT_NAME}      UI test subject

${DATA_FILE_NAME}  dates

${FOOTNOTE_TEXT_1}  test footnote from the bau user
${FOOTNOTE_TEXT_2}  test footnote from the publication owner! (analyst)
${FOOTNOTE_TEXT_3}  an edited footnote from the publication owner! (analyst)

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
    user clicks element  //*[tbody]//tr[1]//td[3]//a
    user waits until element is enabled  css:[name="selectedPublicationId"]
    user selects from list by label  css:[name="selectedPublicationId"]  ${PUBLICATION_NAME}
    user selects from list by label  css:[name="selectedPublicationRole"]  Owner
    user clicks button  Add publication access


Give Analyst1 User1 release access
    [Tags]  HappyPath
    user waits until element is enabled  css:[name="selectedReleaseId"]
    user selects from list by label  css:[name="selectedReleaseId"]  ${RELEASE_NAME}
    user selects from list by label  css:[name="selectedReleaseRole"]  Approver
    user clicks button  Add release access
    user clicks element  //*[@data-testid="Add-release-access"]//button

Assert that Analyst1 User1 has correct publication role
    [Tags]  HappyPath
    user checks results table cell contains  1  1  ${PUBLICATION_NAME}  //*[@data-testid="publication-access-table"]


Sign in as Analyst1 User1 & navigate to publication
    [Tags]  HappyPath
    user signs in as analyst1
    user goes to url  %{ADMIN_URL}
    user selects theme and topic from admin dashboard  %{TEST_THEME_NAME}  %{TEST_TOPIC_NAME}
    user waits until page contains accordion section   ${PUBLICATION_NAME}  120

Edit release content
    [Tags]  HappyPath
    user opens accordion section  ${PUBLICATION_NAME}
    ${accordion}=  user gets accordion section content element  ${PUBLICATION_NAME}
    user opens details dropdown   ${RELEASE_NAME} (not Live)  ${accordion}
    user clicks button  Edit this release
#    user clicks button  Confirm
Test
    Sleep  100000