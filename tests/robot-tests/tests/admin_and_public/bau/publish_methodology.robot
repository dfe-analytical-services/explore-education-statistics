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
${PUBLICATION_URL}=                     /find-statistics/ui-tests-publish-methodology-%{RUN_IDENTIFIER}/2021-22
${PUBLIC_METHODOLOGY_URL_ENDING}=       /methodology/ui-tests-publish-methodology-%{RUN_IDENTIFIER}
${RELEASE_NAME}=                        Academic year 2021/22


*** Test Cases ***
Create a draft release via api
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    AY    2021

Create a methodology
    user creates methodology for publication    ${PUBLICATION_NAME}

Add content to methodology
    user clicks link    Manage content

    user creates new content section    1    Methodology content section 1
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds text block to editable accordion section    Methodology content section 1
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds content to accordion section text block    Methodology content section 1    1
    ...    Content 1    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}

    user creates new content section    2    Methodology content section 2
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds text block to editable accordion section    Methodology content section 2
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds content to accordion section text block    Methodology content section 2    1
    ...    Content 2    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}

    # regression test for EES-3877
    user creates new content section    3    3.test-title 3
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds text block to editable accordion section    3.test-title 3
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds content to accordion section text block    3.test-title 3    1
    ...    Content 3    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}

    user creates new content section    4    4.-test-.title 4
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds text block to editable accordion section    4.-test-.title 4
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds content to accordion section text block    4.-test-.title 4    1
    ...    Content 4    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}

Add annexe content to methodology
    user creates new content section    1    Methodology annexe section 1
    ...    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user adds text block to editable accordion section    Methodology annexe section 1
    ...    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user adds content to accordion section text block    Methodology annexe section 1    1
    ...    Annexe 1    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}

    user creates new content section    2    Methodology annexe section 2
    ...    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user adds text block to editable accordion section    Methodology annexe section 2
    ...    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user adds content to accordion section text block    Methodology annexe section 2    1
    ...    Annexe 2    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}

Check there is no methodology status history table on Sign off page
    user clicks link    Sign off
    user waits until h2 is visible    Sign off

    user checks page does not contain element    testid:methodology-status-history

Approve the methodology for publishing immediately
    user approves methodology for publication    ${PUBLICATION_NAME}

Check methodology status history is correct after approval
    user waits until h3 is visible    Methodology status history    %{WAIT_SMALL}
    user checks page contains element    testid:methodology-status-history

    user checks table body has x rows    1    testid:methodology-status-history
    table cell should contain    testid:methodology-status-history    1    2    Status
    table cell should contain    testid:methodology-status-history    1    3    Internal note
    table cell should contain    testid:methodology-status-history    1    4    Methodology version
    table cell should contain    testid:methodology-status-history    1    5    By user
    table cell should contain    testid:methodology-status-history    2    2    Approved
    table cell should contain    testid:methodology-status-history    2    3    Approved by UI tests
    table cell should contain    testid:methodology-status-history    2    4    1
    table cell should contain    testid:methodology-status-history    2    5    ees-test.bau1@education.gov.uk

Verify the expected public URL of the methodology on the Sign off tab
    user navigates to methodology    ${PUBLICATION_NAME}    ${PUBLICATION_NAME}
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

Navigate to release content page and add headline text block
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}

    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Approve the release
    user approves original release for immediate publication

Verify that the user cannot edit the status of the methodology
    user navigates to methodology
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME}
    ...    View

    user clicks link    Sign off
    user waits until h2 is visible    Sign off
    user checks page does not contain    Edit status

Verify that the methodology 'Published' tag and datetime is shown
    user navigates to methodologies on publication page    ${PUBLICATION_NAME}

    ${ROW}=    user gets table row    ${PUBLICATION_NAME}
    user checks element contains    ${ROW}    Published
    ${date}=    get london date
    user checks element contains    ${ROW}    ${date}

Verify that the methodology is visible on the public methodologies page with the expected URL
    user navigates to public methodologies page
    user waits until page contains accordion section    %{TEST_THEME_NAME}
    user opens accordion section    %{TEST_THEME_NAME}
    user checks page contains link with text and url
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLIC_METHODOLOGY_URL_ENDING}

Verify that the methodology is publicly accessible
    user clicks link    ${PUBLICATION_NAME}
    user waits until h1 is visible    ${PUBLICATION_NAME}
    user waits until page contains title caption    Methodology    %{WAIT_SMALL}
    ${METHODOLOGY_URL}=    get location
    set suite variable    ${METHODOLOGY_URL}

Verify that methodology hash links open accordion sections correctly
    [Documentation]    EES-3877
    user navigates to public frontend    ${METHODOLOGY_URL}#content-section-3-test-title-3
    user waits until h1 is visible    ${PUBLICATION_NAME}
    user waits until page contains title caption    Methodology    %{WAIT_SMALL}
    user checks page contains    3.test-title 3
    user checks page contains    Content 3

    user navigates to public frontend    ${METHODOLOGY_URL}#content-section-4-test-title-4
    user waits until page finishes loading

    user checks page contains    4.-test-.title 4
    user checks page contains    Content 4

Verify that the methodology displays a link to the publication
    user navigates to public frontend    ${METHODOLOGY_URL}
    user waits until h1 is visible    ${PUBLICATION_NAME}
    user waits until page contains title caption    Methodology    %{WAIT_SMALL}

    user checks element contains child element
    ...    css:[aria-labelledby="related-information"]
    ...    xpath://h3[text()="Publications"]

    user checks page contains link with text and url
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_URL}
    ...    css:[aria-labelledby="related-information"]

Verify that the methodology content is correct
    ${date}=    get london date
    user checks summary list contains    Published    ${date}

    user checks accordion is in position    Methodology content section 1    1    id:content
    user checks accordion is in position    Methodology content section 2    2    id:content

    user checks accordion is in position    3.test-title 3    3    id:content
    user checks accordion is in position    4.-test-.title 4    4    id:content

    user checks there are x accordion sections    4    id:content

    user opens accordion section    Methodology content section 1
    ${content_section_1}=    user gets accordion section content element    Methodology content section 1
    user checks element contains    ${content_section_1}    Content 1

    user opens accordion section    Methodology content section 2
    ${content_section_2}=    user gets accordion section content element    Methodology content section 2
    user checks element contains    ${content_section_2}    Content 2

    user checks accordion is in position    Methodology annexe section 1    1    id:annexes
    user checks accordion is in position    Methodology annexe section 2    2    id:annexes

    user checks there are x accordion sections    2    id:annexes

    user opens accordion section    Methodology annexe section 1    id:annexes
    ${annexe_section_1}=    user gets accordion section content element    Methodology annexe section 1
    ...    id:annexes
    user checks element contains    ${annexe_section_1}    Annexe 1

    user opens accordion section    Methodology annexe section 2    id:annexes
    ${annexe_section_2}=    user gets accordion section content element    Methodology annexe section 2
    ...    id:annexes
    user checks element contains    ${annexe_section_2}    Annexe 2

Amend the methodology in preparation to test publishing immediately
    user creates methodology amendment for publication    ${PUBLICATION_NAME}
    user edits methodology summary for publication
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - Amended methodology

Update the methodology amendment's content
    user clicks link    Manage content
    user opens accordion section    Methodology content section 1
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds text block to editable accordion section    Methodology content section 1
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user checks accordion section contains x blocks    Methodology content section 1    2
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds content to accordion section text block    Methodology content section 1    2
    ...    New amendment content
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user changes accordion section title    1    Methodology content section 1 updated
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}

Reorder amendment content sections
    user clicks element    id:methodologyAccordion-content-reorder
    user presses keys    ${SPACE}
    user presses keys    ARROW_DOWN
    user presses keys    ${SPACE}

    user clicks button    Save order

Check the new order of amendment content sections
    user checks accordion is in position    Methodology content section 2    1
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user checks accordion is in position    Methodology content section 1 updated    2
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}

Reorder amendment annexe sections
    user clicks element    id:methodologyAccordion-annexes-reorder
    user presses keys    ${SPACE}
    user presses keys    ARROW_DOWN
    user presses keys    ${SPACE}

    user clicks button    Save order

Check the new order of amendment annexe sections
    user checks accordion is in position    Methodology annexe section 2    1
    ...    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user checks accordion is in position    Methodology annexe section 1    2
    ...    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}

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

Check methodology status history is correct before approving amendment
    user clicks link    Sign off
    user waits until h2 is visible    Sign off

    user waits until h3 is visible    Methodology status history    %{WAIT_SMALL}
    user checks page contains element    testid:methodology-status-history

    user checks table body has x rows    2    testid:methodology-status-history

Approve the amendment for publishing immediately
    user approves methodology amendment for publication
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - Amended methodology

Check methodology status history is correct after approving amendment
    user waits until h3 is visible    Methodology status history    %{WAIT_SMALL}
    user checks page contains element    testid:methodology-status-history
    user checks table body has x rows    3    testid: methodology-status-history
    table cell should contain    testid:methodology-status-history    2    2    Approved
    table cell should contain    testid:methodology-status-history    2    3    Approved by UI tests
    table cell should contain    testid:methodology-status-history    2    4    2
    table cell should contain    testid:methodology-status-history    2    5    ees-test.bau1@education.gov.uk
    table cell should contain    testid:methodology-status-history    3    2    Approved
    table cell should contain    testid:methodology-status-history    3    3    Approved by UI tests
    table cell should contain    testid:methodology-status-history    3    4    1
    table cell should contain    testid:methodology-status-history    3    5    ees-test.bau1@education.gov.uk
    table cell should contain    testid:methodology-status-history    4    2    Approved
    table cell should contain    testid:methodology-status-history    4    3    Approved by UI tests
    table cell should contain    testid:methodology-status-history    4    4    1
    table cell should contain    testid:methodology-status-history    4    5    ees-test.bau1@education.gov.uk

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
    ...    ${PUBLICATION_URL}
    ...    css:[aria-labelledby="related-information"]

Verify that the amended methodology content is correct
    ${date}=    get london date
    user checks summary list contains    Published    ${date}
    user checks summary list contains    Last updated    ${date}

    user checks accordion is in position    Methodology content section 2    1    id:content
    user checks accordion is in position    Methodology content section 1 updated    2    id:content

    user checks accordion is in position    3.test-title 3    3    id:content
    user checks accordion is in position    4.-test-.title 4    4    id:content

    user checks there are x accordion sections    4    id:content

    user opens accordion section    Methodology content section 2
    ${content_section_2}=    user gets accordion section content element    Methodology content section 2
    user checks element contains    ${content_section_2}    Content 2

    user opens accordion section    Methodology content section 1 updated
    ${content_section_1}=    user gets accordion section content element    Methodology content section 1 updated
    user checks element contains    ${content_section_1}    Content 1
    user checks element contains    ${content_section_1}    New amendment content

    user checks accordion is in position    Methodology annexe section 2    1    id:annexes
    user checks accordion is in position    Methodology annexe section 1    2    id:annexes

    user checks there are x accordion sections    2    id:annexes

    user opens accordion section    Methodology annexe section 2    id:annexes
    ${annexe_section_2}=    user gets accordion section content element    Methodology annexe section 2
    ...    id:annexes
    user checks element contains    ${annexe_section_2}    Annexe 2

    user opens accordion section    Methodology annexe section 1    id:annexes
    ${annexe_section_1}=    user gets accordion section content element    Methodology annexe section 1
    ...    id:annexes
    user checks element contains    ${annexe_section_1}    Annexe 1

Verify the list of notes
    ${date}=    get london date
    user opens details dropdown    See all notes (2)
    user waits until page contains element    css:[data-testid="notes"] li    limit=2
    user checks methodology note    1    ${date}    Latest note
    user checks methodology note    2    1 March 2021    Note which should be updated - edited
    user closes details dropdown    See all notes (2)

Verify that the amended methodology is visible on the public methodologies page
    user navigates to public methodologies page
    user waits until page contains accordion section    %{TEST_THEME_NAME}
    user opens accordion section    %{TEST_THEME_NAME}
    user scrolls down    400
    user checks page contains link with text and url
    ...    ${PUBLICATION_NAME} - Amended methodology
    ...    ${PUBLIC_METHODOLOGY_URL_ENDING}-amended-methodology    # Slug has changed

Validate methodology redirect works
    go to    %{PUBLIC_URL}${PUBLIC_METHODOLOGY_URL_ENDING}
    user waits until h1 is visible    ${PUBLICATION_NAME} - Amended methodology
    user checks url contains    %{PUBLIC_URL}${PUBLIC_METHODOLOGY_URL_ENDING}-amended-methodology

Schedule a methodology amendment to be published with a release amendment
    user creates amendment for release    ${PUBLICATION_NAME}    ${RELEASE_NAME}
    user creates methodology amendment for publication    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - Amended methodology
    user approves methodology amendment for publication
    ...    publication=${PUBLICATION_NAME}
    ...    methodology_title=${PUBLICATION_NAME} - Amended methodology
    ...    publishing_strategy=WithRelease
    ...    with_release=${PUBLICATION_NAME} - ${RELEASE_NAME}

Check methodology status history table contains new row after approving amendment
    user checks table body has x rows    4    testid:methodology-status-history
    table cell should contain    testid:methodology-status-history    2    2    Approved
    table cell should contain    testid:methodology-status-history    2    3    Approved by UI tests
    table cell should contain    testid:methodology-status-history    2    4    3
    table cell should contain    testid:methodology-status-history    2    5    ees-test.bau1@education.gov.uk
    table cell should contain    testid:methodology-status-history    3    2    Approved
    table cell should contain    testid:methodology-status-history    3    3    Approved by UI tests
    table cell should contain    testid:methodology-status-history    3    4    2
    table cell should contain    testid:methodology-status-history    3    5    ees-test.bau1@education.gov.uk

Cancel the release amendment and validate that the appropriate warning modal is shown
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}

    ${ROW}=    user gets table row    ${RELEASE_NAME}    testid:publication-draft-releases
    user checks element contains    ${ROW}    Draft Amendment
    user clicks button    Cancel amendment    ${ROW}

    ${modal}=    user waits until modal is visible    Confirm you want to cancel this amended release
    user waits until element contains    ${modal}
    ...    The following methodologies are scheduled to be published with this amended release
    user waits until element contains    ${modal}    ${PUBLICATION_NAME} - Amended methodology
    user clicks button    Confirm
    user waits until modal is not visible    Confirm you want to cancel this amended release

Verify that the methodology that was scheduled with the cancelled release amendment is set back to Draft / Immediately
    user clicks link    Methodologies
    user waits until h2 is visible    Manage methodologies

    ${ROW}=    user gets table row    ${PUBLICATION_NAME} - Amended methodology
    user clicks element    xpath://*[text()="Edit"]    ${ROW}
    user waits until h2 is visible    Methodology summary

    user clicks link    Sign off
    user waits until h2 is visible    Sign off

    user checks summary list contains    Status    In draft
    user checks summary list contains    Owning publication    ${PUBLICATION_NAME}

    user clicks button    Edit status
    user waits until h2 is visible    Edit methodology status
    user checks radio is checked    In draft
    user clicks radio    Approved for publication
    user checks radio is checked    Immediately
