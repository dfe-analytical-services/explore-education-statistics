*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Resource            ../../libs/public-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData

*** Variables ***
${PUBLICATION_NAME}=                    UI tests - publish methodology %{RUN_IDENTIFIER}
${PUBLIC_METHODOLOGY_URL_ENDING}=       /methodology/ui-tests-publish-methodology-%{RUN_IDENTIFIER}
${RELEASE_NAME}=                        Academic Year 2021/22

*** Test Cases ***
Create a draft release
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user create test release via api    ${PUBLICATION_ID}    AY    2021

Approve a methodology for publishing immediately
    user creates methodology for publication    ${PUBLICATION_NAME}

    user clicks link    Manage content
    user creates new content section    1    Methodology content section 1    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds text block to editable accordion section    Methodology content section 1
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds content to accordion section text block    Methodology content section 1    1
    ...    Adding Methodology content    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}

    user approves methodology for publication    ${PUBLICATION_NAME}

Verify the expected public URL of the methodology on the Sign off tab
    user views methodology for publication    ${PUBLICATION_NAME}
    user clicks link    Sign off
    user waits until page contains testid    public-methodology-url
    ${ACCESSIBLE_METHODOLOGY_URL}=    Get Value    xpath://*[@data-testid="public-methodology-url"]
    should end with    ${ACCESSIBLE_METHODOLOGY_URL}    ${PUBLIC_METHODOLOGY_URL_ENDING}
    set suite variable    ${ACCESSIBLE_METHODOLOGY_URL}

Verify that the publication is not visible on the public methodologies page without a published release
    user navigates to public methodologies page
    user checks page does not contain    ${PUBLICATION_NAME}

Verify that the methodology is not publicly accessible by URL without a published release
    user navigates to public frontend    ${ACCESSIBLE_METHODOLOGY_URL}
    user waits until h1 is visible    Page not found    %{WAIT_MEDIUM}

Alter the approval to publish the methodology with the release
    user approves methodology for publication
    ...    publication=${PUBLICATION_NAME}
    ...    publishing_strategy=WithRelease
    ...    with_release=${PUBLICATION_NAME} - ${RELEASE_NAME}

Verify that the publication is still not visible on the public methodologies page without publishing the release
    user navigates to public methodologies page
    user checks page does not contain    ${PUBLICATION_NAME}

Verify that the methodology is still not publicly accessible by URL without publishing the release
    user navigates to public frontend    ${ACCESSIBLE_METHODOLOGY_URL}
    user waits until page contains    Page not found

Approve the release
    user navigates to editable release summary from admin dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME} (not Live)
    user approves original release for immediate publication

Verify that the user cannot edit the status of the methodology
    user views methodology for publication
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME}
    ...    View methodology
    user clicks link    Sign off
    user waits until h2 is visible    Sign off
    user checks page does not contain    Edit status

Verify that the methodology 'Published' tag is shown
    user opens publication on the admin dashboard    ${PUBLICATION_NAME}    %{TEST_THEME_NAME}    %{TEST_TOPIC_NAME}

    user waits until page contains element
    ...    //*[@data-testid="Methodology for ${PUBLICATION_NAME}"]//div//span[text()="Published"]

Verify that the methodology is visible on the public methodologies page with the expected URL
    user navigates to public methodologies page
    user waits until page contains accordion section    %{TEST_THEME_NAME}
    user opens accordion section    %{TEST_THEME_NAME}
    user opens details dropdown    %{TEST_TOPIC_NAME}
    user checks page contains methodology link
    ...    %{TEST_TOPIC_NAME}
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLIC_METHODOLOGY_URL_ENDING}

Verify that the methodology is publicly accessible
    user clicks methodology link
    ...    %{TEST_TOPIC_NAME}
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME}
    user waits until h1 is visible    ${PUBLICATION_NAME}
    user waits until page contains title caption    Methodology

Verify that the methodology displays a link to the publication
    user checks element contains child element
    ...    css:[aria-labelledby="related-information"]
    ...    xpath://h3[text()="Publications"]
    user checks page contains link with text and url
    ...    ${PUBLICATION_NAME}
    ...    /find-statistics/ui-tests-publish-methodology-%{RUN_IDENTIFIER}
    ...    css:[aria-labelledby="related-information"]

Verify that the methodology content is correct
    ${date}=    get current datetime    %-d %B %Y
    user checks summary list contains    Published    ${date}
    user waits until page contains accordion section    Methodology content section 1
    user opens accordion section    Methodology content section 1
    ${content}=    user gets accordion section content element    Methodology content section 1
    user checks element contains    ${content}    Adding Methodology content

Amend the methodology in preparation to test publishing immediately
    user creates methodology amendment for publication    ${PUBLICATION_NAME}
    user edits methodology summary for publication
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - Amended methodology
    ...    Edit amendment

Update the methodology amendment's content
    user clicks link    Manage content
    user opens accordion section    Methodology content section 1
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds text block to editable accordion section    Methodology content section 1
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds content to accordion section text block    Methodology content section 1    2    New & Updated content
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user changes accordion section title    1    New and Updated Title    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}

Add a note describing the amendment
    user adds note to methodology
    ...    Latest note

Add and remove another note describing the amendment
    user adds note to methodology
    ...    Note which should be deleted
    user removes methodology note
    ...    Note which should be deleted
    ...    css:#methodologyNotes li:nth-of-type(1)

Add and update another note describing the amendment
    user adds note to methodology
    ...    Note which should be updated
    user edits methodology note
    ...    Note which should be updated
    ...    01
    ...    03
    ...    2021

Approve the amendment for publishing immediately
    user approves methodology amendment for publication
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - Amended methodology

Verify that the user cannot edit the status of the amended methodology
    user clicks link    Sign off
    user waits until h2 is visible    Sign off
    user checks page does not contain    Edit status

Go to methodology amendment's public page
    ${METHODOLOGY_URL}=    get element attribute    css:#public-methodology-url    value
    user navigates to public frontend    ${METHODOLOGY_URL}
    user waits until page contains title    ${PUBLICATION_NAME} - Amended methodology
    user waits until page contains title caption    Methodology

Verify that the amended methodology displays a link to the publication
    user checks element contains child element
    ...    css:[aria-labelledby="related-information"]
    ...    xpath://h3[text()="Publications"]
    user checks page contains link with text and url
    ...    ${PUBLICATION_NAME}
    ...    /find-statistics/ui-tests-publish-methodology-%{RUN_IDENTIFIER}
    ...    css:[aria-labelledby="related-information"]

Verify that the amended methodology content is correct
    ${date}=    get current datetime    %-d %B %Y
    user checks summary list contains    Published    ${date}
    user checks summary list contains    Last updated    ${date}
    user waits until page contains accordion section    New and Updated Title
    user opens accordion section    New and Updated Title
    ${content}=    user gets accordion section content element    New and Updated Title
    user checks element contains    ${content}    Adding Methodology content
    user checks element contains    ${content}    New & Updated content

Verify the list of notes
    ${date}=    get current datetime    %-d %B %Y
    user opens details dropdown    See all notes (2)
    user waits until page contains element    css:[data-testid="notes"] li    limit=2
    user checks methodology note    1    ${date}    Latest note
    user checks methodology note    2    1 March 2021    Note which should be updated - edited
    user closes details dropdown    See all notes (2)

Verify that the amended methodology is visible on the public methodologies page
    user navigates to public methodologies page
    user waits until page contains accordion section    %{TEST_THEME_NAME}
    user opens accordion section    %{TEST_THEME_NAME}
    user opens details dropdown    %{TEST_TOPIC_NAME}
    user scrolls down    400    # @MarkFix
    user checks page contains methodology link
    ...    %{TEST_TOPIC_NAME}
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - Amended methodology
    ...    ${PUBLIC_METHODOLOGY_URL_ENDING}

Schedule a methodology amendment to be published with a release amendment
    user creates amendment for release    ${PUBLICATION_NAME}    ${RELEASE_NAME}    (Live - Latest release)
    user creates methodology amendment for publication    ${PUBLICATION_NAME}
    user approves methodology amendment for publication
    ...    publication=${PUBLICATION_NAME}
    ...    publishing_strategy=WithRelease
    ...    with_release=${PUBLICATION_NAME} - ${RELEASE_NAME}

Cancel the release amendment and validate that the appropriate warning modal is shown
    ${details}=    user opens release summary on the admin dashboard    ${PUBLICATION_NAME}    ${RELEASE_NAME}
    user clicks button    Cancel amendment    ${details}
    ${modal}=    user waits until modal is visible    Confirm you want to cancel this amended release
    user waits until element contains    ${modal}
    ...    The following methodologies are scheduled to be published with this amended release
    user waits until element contains    ${modal}    ${PUBLICATION_NAME} - Amended methodology
    user clicks button    Confirm
    user waits until modal is not visible    Confirm you want to cancel this amended release

Verify that the methodology that was scheduled with the cancelled release amendment is set back to Draft / Immediately
    user views methodology amendment for publication    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - Amended methodology
    user clicks link    Sign off
    user waits until h2 is visible    Sign off
    user clicks button    Edit status
    user waits until h2 is visible    Edit methodology status
    user checks radio is checked    In draft
    user clicks radio    Approved for publication
    user checks radio is checked    Immediately
