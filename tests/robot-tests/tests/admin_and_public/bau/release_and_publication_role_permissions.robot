*** Settings ***
Resource    ../../libs/admin-common.robot
Resource    ../../libs/common.robot

Library     ../../libs/admin_api.py

Force Tags  Admin  Local  Dev  AltersData  Footnotes

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${PUBLICATION_NAME}  UI tests - publication_owner %{RUN_IDENTIFIER}
${RELEASE_TYPE}      Academic Year 2025/26
${RELEASE_NAME}      ${PUBLICATION_NAME} - ${RELEASE_TYPE}
${SUBJECT_NAME}      UI test subject

*** Test Cases ***
Create new publication and release via API
    [Tags]  HappyPath
    ${PUBLICATION_ID}=  user creates test publication via api   ${PUBLICATION_NAME}
    user create test release via api  ${PUBLICATION_ID}   AY    2025

Navigate to admin dashboard and assert publication is present
    [Tags]  HappyPath
    user goes to url  %{ADMIN_URL}
    user selects theme and topic from admin dashboard  %{TEST_THEME_NAME}  %{TEST_TOPIC_NAME}
    user waits until page contains accordion section   ${PUBLICATION_NAME}  %{WAIT_MEDIUM}

Navigate to manage users page as bau1
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

    user checks results table cell contains  2  1  	ees-analyst2@education.gov.uk
    user checks results table cell contains  2  2  	Analyst
    user checks results table cell contains  2  3  	Manage

    user checks results table cell contains  3  1  	ees-analyst3@education.gov.uk
    user checks results table cell contains  3  2  	Analyst
    user checks results table cell contains  3  3  	Manage

    user checks results table cell contains  4  1  	ees-bau1@education.gov.uk
    user checks results table cell contains  4  2  	BAU User
    user checks results table cell contains  4  3  	Manage

    user checks results table cell contains  5  1  	ees-bau2@education.gov.uk
    user checks results table cell contains  5  2  	BAU User
    user checks results table cell contains  5  3  	Manage

Give Analyst1 User1 publication owner access
    [Tags]  HappyPath
    user gives analyst publication owner access  ${PUBLICATION_NAME}

Sign in as Analyst1 User1 (publication owner) & navigate to publication
    [Tags]  HappyPath
    user changes to analyst1
    user waits for page to finish loading 
    user navigates to release summary from admin dashboard  ${PUBLICATION_NAME}
    ...  ${RELEASE_TYPE} (not Live)    

Assert publication owner can upload subject file
    [Tags]  HappyPath
    user clicks link  Data and files
    user waits until page does not contain loading spinner
    user uploads subject   ${SUBJECT_NAME}  seven_filters.csv  seven_filters.meta.csv

Assert publication owner can add meta guidance to ${SUBJECT_NAME}
    [Tags]  HappyPath
    user clicks link  Metadata guidance
    user waits until page does not contain loading spinner
    user enters text into element  id:metaGuidanceForm-content  Test meta guidance content
    user waits until page contains accordion section  ${SUBJECT_NAME}
    user enters text into meta guidance data file content editor  ${SUBJECT_NAME}
    ...  meta guidance content
    user clicks button  Save guidance

Navigate to 'Footnotes' page
    [Tags]  HappyPath
    user waits for page to finish loading
    user clicks link  Footnotes
    user waits until h2 is visible  Footnotes

Assert publication owner can add a footnote to ${SUBJECT_NAME}
    [Tags]  HappyPath
    user waits until page contains link   Create footnote
    user clicks link  Create footnote
    user waits until page does not contain loading spinner
    user clicks footnote radio   ${SUBJECT_NAME}   Applies to all data
    user clicks element  id:footnoteForm-content
    user enters text into element  id:footnoteForm-content  test footnote from the publication owner! (analyst)
    user clicks button  Save footnote
    user waits until h2 is visible  Footnotes  60

Add public prerelease access list
    [Tags]  HappyPath
    user clicks link  Pre-release access
    user waits until page does not contain loading spinner
    user creates public prerelease access list   Test public access list

Go to "Sign off" page
    [Tags]  HappyPath
    user clicks link   Sign off
    user waits until page does not contain loading spinner
    user clicks button  Edit release status

Assert publication owner cannot approve release for immediate publication
    [Tags]  HappyPath
    user waits until page contains element  id:releaseStatusForm-approvalStatus-Approved  30
    user checks element is disabled  id:releaseStatusForm-approvalStatus-Approved

Assert publication owner can edit release status to "Ready for higher review"
    [Tags]  HappyPath
    user clicks radio   Ready for higher review
    user enters text into element  id:releaseStatusForm-latestInternalReleaseNote     ready for higher review (publication owner)
    user clicks button  Update status 
    user waits until element is visible  id:CurrentReleaseStatus-Awaiting higher review
    
Assert publication owner can edit release status to "In draft"
    [Tags]  HappyPath
    user clicks button  Edit release status
    user waits until h2 is visible  Edit release status  30
    user clicks radio   In draft
    user enters text into element  id:releaseStatusForm-latestInternalReleaseNote     Moving back to Draft state (publication owner)
    user clicks button  Update status 

User goes back to admin dashboard
    [Tags]  HappyPath
    user goes to url  %{ADMIN_URL}

Assert that a publication owner can make a new release 
    [Tags]  HappyPath
    user opens publication on the admin dashboard   ${PUBLICATION_NAME}
    user waits until page does not contain loading spinner
    user clicks link  Create new release
    user creates release for publication  ${PUBLICATION_NAME}  Academic Year Q1  2020

Assert publication owner can upload subject file on new release
    [Tags]  HappyPath
    user clicks link  Data and files
    user uploads subject   ${SUBJECT_NAME}  seven_filters.csv  seven_filters.meta.csv
    
Assert publication owner can add meta guidance to ${SUBJECT_NAME} on new release
    [Tags]  HappyPath
    user clicks link  Metadata guidance
    user waits until page does not contain loading spinner
    user enters text into element  id:metaGuidanceForm-content  Test meta guidance content
    user waits until page contains accordion section  ${SUBJECT_NAME}
    user enters text into meta guidance data file content editor  ${SUBJECT_NAME}
    ...  meta guidance content
    user clicks button  Save guidance
  
Go to "Sign off" page as publication owner 
    [Tags]  HappyPath
    user clicks link   Sign off
    user waits until h2 is visible  Sign off
     
Assert publication owner cannot approve release for immediate publication on new release
    [Tags]  HappyPath
    user clicks button  Edit release status
    user waits until h2 is visible  Edit release status  60
    user scrolls to element  id:releaseStatusForm-approvalStatus-Approved
    user checks element is disabled  id:releaseStatusForm-approvalStatus-Approved

Navigate to administration as bau1 and swap publication owner role for release approver
    [Tags]  HappyPath
    user changes to bau1
    user removes publication owner access from analyst  ${PUBLICATION_NAME}
    user gives release access to analyst  ${RELEASE_NAME}  Approver

Check release approver can access release
    [Tags]  HappyPath
    user changes to analyst1
    user navigates to release summary from admin dashboard  ${PUBLICATION_NAME}  ${RELEASE_TYPE} (not Live)

Navigate to 'Footnotes' section
    [Tags]  HappyPath
    user clicks link  Footnotes
    user waits until h2 is visible  Footnotes

Add a Footnote as a release approver
    [Tags]  HappyPath
    user waits until page contains link  Create footnote
    user clicks link  Create footnote
    user waits until h2 is visible  Create footnote
    user clicks footnote radio   ${SUBJECT_NAME}   Applies to all data
    user clicks element  id:footnoteForm-content
    user enters text into element  id:footnoteForm-content  A footnote as analyst1 User1 with the release approver role
    user clicks button  Save footnote
    user waits until h2 is visible  Footnotes

Assert release approver can create a release note
    [Tags]  HappyPath
    user clicks link  Content
    user clicks button   Add note
    user enters text into element  id:createReleaseNoteForm-reason  Test release note one
    user clicks button   Save note
    ${date}=  get current datetime   %-d %B %Y
    user waits until element contains  css:#releaseNotes li:nth-of-type(1) time  ${date}
    user waits until element contains  css:#releaseNotes li:nth-of-type(1) p     Test release note one

Assert release approver can publish a release  
    user clicks link  Sign off
    user approves release for immediate publication

Navigate to administration as bau1 and swap release approver role for viewer only
    user changes to bau1
    user removes release access from analyst  ${RELEASE_NAME}  Approver
    user gives release access to analyst  ${RELEASE_NAME}  Viewer

Navigate to release as a viewer
    [Tags]  HappyPath
    user changes to analyst1
    user opens publication on the admin dashboard   ${PUBLICATION_NAME}

    ${accordion}=  user gets accordion section content element   ${PUBLICATION_NAME}
    ${details}=  user gets details content element  ${RELEASE_TYPE} (Live - Latest release)  ${accordion}

    user waits until parent contains element   ${details}   xpath:.//a[text()="View this release"]
    ${view_button}=  get child element  ${details}  xpath:.//a[text()="View this release"]
    user clicks element   ${view_button}

    user waits until h2 is visible  Release summary
    user checks summary list contains   Publication title  ${PUBLICATION_NAME}

Check release viewer cannot approve release 
    [Tags]  HappyPath
    user clicks link   Sign off
    user waits until page does not contain loading spinner
    user checks page does not contain  Edit release status

Login as bau1 and swap viewer role for contributor role for analyst
    [Tags]  HappyPath
    user changes to bau1
    user removes release access from analyst   ${RELEASE_NAME}  Viewer
    user gives release access to analyst   ${RELEASE_NAME}  Contributor

Check release contributor cannot create an amendment
    [Tags]  HappyPath
    user changes to analyst1
    user opens publication on the admin dashboard   ${PUBLICATION_NAME}
    ${accordion}=  user gets accordion section content element  ${PUBLICATION_NAME}
    ${details}=  user gets details content element  ${RELEASE_TYPE} (Live - Latest release)  ${accordion}  30
    user waits until parent contains element   ${details}   xpath:.//a[text()="View this release"]  30
    user checks page does not contain button  Amend this release