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
    user checks page contains accordion  ${PUBLICATION_NAME}
    user opens accordion section  ${PUBLICATION_NAME}
    user checks accordion section contains text  ${PUBLICATION_NAME}    Methodology
    user checks accordion section contains text  ${PUBLICATION_NAME}    Releases

Create release
    [Tags]  HappyPath
    user clicks element  css:[data-testid="Create new release link for ${PUBLICATION_NAME}"]
    user creates release for publication  ${PUBLICATION_NAME}  Academic Year  2025
    user checks summary list contains  Publication title  ${PUBLICATION_NAME}

Navigate to Manage content tab
    [Tags]  HappyPath
    user waits until page contains element   xpath://a[text()="Manage content"]
    user clicks element  xpath://a[text()="Manage content"]
    user waits until page contains heading 1  ${PUBLICATION_NAME}

Add summary content to release
    [Tags]  HappyPath   Failing
    user clicks element  xpath://button[text()="Add a summary text block"]
    user waits until page contains element  xpath://p[text()="This section is empty"]
    user clicks element   xpath://button[text()="Edit block"]
    user presses keys  Test intro text for ${PUBLICATION_NAME}
    user clicks element   xpath://button[text()="Save"]
    user waits until page contains element  xpath://p[text()="Test intro text for ${PUBLICATION_NAME}"]

# TODO: Add comment to summary content

Add release note to release
    [Tags]  HappyPath
    user clicks element   xpath://button[text()="Add note"]
    user clicks element   css:textarea#reason
    user presses keys  Test release note one
    user clicks element   xpath://button[text()="Add note"]
    # TODO: Check release note is there
    user waits until page contains element   xpath://span[text()="See all 1 updates"]
    user clicks element   xpath://span[text()="See all 1 updates"]
    ${date}=  get datetime   %d %B %Y
    user checks page contains element   xpath://*[@data-testid="last-updated-element"]/time[text()="${date}"]
    user checks page contains element   xpath://*[@data-testid="last-updated-element"]/p[text()="Test release note one"]

Add related guidance link to release
    [Tags]  HappyPath
    user clicks element  xpath://button[text()="Add related information"]
    user clicks element  css:input#title
    user presses keys   Test link one
    user clicks element  css:input#link-url
    user presses keys   http://test1.example.com/test1
    user clicks element  xpath://button[text()="Create link"]

# TODO: Add Secondary Stats
# TODO: Add key statistics

Add key statistics summary content to release
    [Tags]  HappyPath   Failing
    user clicks element   xpath://button[text()="Add a headlines text block"]
    user waits until page contains element  xpath://p[text()="This section is empty"]
    user clicks element   xpath://section[@id="releaseHeadlines-headlines"]//button[text()="Edit block"]
    user presses keys   Test key statistics summary text for ${PUBLICATION_NAME}
    user clicks element  xpath://button[text()="Save"]

    user waits until page contains   Test key statistics summary text for ${PUBLICATION_NAME}

Add accordion sections to release
    [Tags]  HappyPath
    user waits until element is enabled  xpath://button[text()="Add new section"]
    user clicks element   xpath://button[text()="Add new section"]
    user changes accordion section title  1   Test section one

    user waits until element is enabled  xpath://button[text()="Add new section"]
    user clicks element   xpath://button[text()="Add new section"]
    user changes accordion section title  2   Test section two

    user waits until element is enabled  xpath://button[text()="Add new section"]
    user clicks element   xpath://button[text()="Add new section"]
    user changes accordion section title  3   Test section three

    # TODO: Validate that three accordion sections exist

Add content block to Test section one
    [Tags]  HappyPath
    ${section_one}=  user gets editable accordion section element  Test section one
    set global variable   ${section_one}   ${section_one}

    user opens editable accordion section   ${section_one}
    user adds text block to editable accordion section   ${section_one}

Add text to newly created content blocks
    [Tags]  HappyPath
    user adds content to accordion section text block  ${section_one}   1    block one test text

Create two more blocks and add text to them
    [Tags]  HappyPath
    user adds text block to editable accordion section   ${section_one}
    user adds content to accordion section text block  ${section_one}   2    block two test text

    user adds text block to editable accordion section   ${section_one}
    user adds content to accordion section text block  ${section_one}   3    block three test text

    user checks accordion section contains X blocks   ${section_one}    3

Delete second content block
    [Tags]  HappyPath
    user deletes editable accordion section content block  ${section_one}   2
    user waits until page does not contain    block two test text
    user checks accordion section contains X blocks   ${section_one}    2

Validate two remaining content blocks
    [Tags]  HappyPath
    user checks accordion section text block contains text   ${section_one}   1   block one test text
    user checks accordion section text block contains text   ${section_one}   2   block three test text
