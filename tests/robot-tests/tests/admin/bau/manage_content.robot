*** Settings ***
Resource    ../../libs/admin-common.robot
Library  Collections

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${TOPIC_NAME}        %{TEST_TOPIC_NAME}
${PUBLICATION_NAME}  UI tests - manage content %{RUN_IDENTIFIER}

*** Test Cases ***
Create Manage content test publication
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "${TOPIC_NAME}" from the admin dashboard
    user waits until page contains element    xpath://a[text()="Create new publication"]     60
    user clicks link  Create new publication
    user creates publication    ${PUBLICATION_NAME}

Verify Manage content test publication is created
    [Tags]  HappyPath
    user waits until page contains accordion section  ${PUBLICATION_NAME}
    user opens accordion section  ${PUBLICATION_NAME}
    user waits until accordion section contains text  ${PUBLICATION_NAME}    Methodology
    user waits until accordion section contains text  ${PUBLICATION_NAME}    Releases

Create release
    [Tags]  HappyPath
    user clicks element  css:[data-testid="Create new release link for ${PUBLICATION_NAME}"]
    user creates release for publication  ${PUBLICATION_NAME}  Academic Year  2025
    user checks summary list contains  Publication title  ${PUBLICATION_NAME}

Navigate to Manage content tab
    [Tags]  HappyPath
    user clicks link   Manage content
    user waits until page contains heading 1  ${PUBLICATION_NAME}
    user waits until page contains heading 2  ${PUBLICATION_NAME}

Add summary content to release
    [Tags]  HappyPath
    user clicks button  Add a summary text block
    user waits until element contains  id:releaseSummary  This section is empty
    user clicks button   Edit block  id:releaseSummary
    user presses keys  Test intro text for ${PUBLICATION_NAME}
    user clicks button   Save  id:releaseSummary
    user waits until element contains  id:releaseSummary  Test intro text for ${PUBLICATION_NAME}

# TODO: Add comment to summary content

Add release note to release
    [Tags]  HappyPath
    user clicks button   Add note
    user enters text into element  css:textarea#reason  Test release note one
    user clicks button   Add note
    user opens details dropdown  See all 1 updates  id:releaseLastUpdated
    ${date}=  get current datetime   %-d %B %Y
    user waits until element contains  css:#releaseNotes li:nth-of-type(1) time  ${date}
    user waits until element contains  css:#releaseNotes li:nth-of-type(1) p     Test release note one

Add related guidance link to release
    [Tags]  HappyPath
    user clicks button  Add related information
    user clicks element  css:input#title
    user presses keys   Test link one
    user clicks element  css:input#link-url
    user presses keys   http://test1.example.com/test1
    user clicks button  Create link

# TODO: Add Secondary Stats
# TODO: Add key statistics

Add key statistics summary content to release
    [Tags]  HappyPath
    user clicks button  Add a headlines text block  id:releaseHeadlines
    user waits until element contains  id:releaseHeadlines  This section is empty
    user clicks button  Edit block  id:releaseHeadlines
    user presses keys   Test key statistics summary text for ${PUBLICATION_NAME}
    user clicks button  Save  id:releaseHeadlines

    user waits until page contains   Test key statistics summary text for ${PUBLICATION_NAME}

Add accordion sections to release
    [Tags]  HappyPath
    user waits until button is enabled  Add new section
    user clicks button  Add new section
    user waits until button is enabled  Add new section
    user clicks button  Add new section
    user waits until button is enabled  Add new section
    user clicks button  Add new section

    user changes accordion section title  1   Test section one
    user changes accordion section title  2   Test section two
    user changes accordion section title  3   Test section three

Add content blocks to Test section one
    [Tags]  HappyPath
    user adds text block to editable accordion section   Test section one
    user adds content to accordion section text block  Test section one   1    block one test text

    user adds text block to editable accordion section   Test section one
    user adds content to accordion section text block  Test section one   2    block two test text

    user adds text block to editable accordion section   Test section one
    user adds content to accordion section text block  Test section one   3    block three test text

    user checks accordion section contains X blocks   Test section one    3

Delete second content block
    [Tags]  HappyPath
    user deletes editable accordion section content block  Test section one   2
    user waits until page does not contain    block two test text
    user checks accordion section contains X blocks   Test section one    2

Validate two remaining content blocks
    [Tags]  HappyPath
    user checks accordion section text block contains   Test section one   1   block one test text
    user checks accordion section text block contains   Test section one   2   block three test text
