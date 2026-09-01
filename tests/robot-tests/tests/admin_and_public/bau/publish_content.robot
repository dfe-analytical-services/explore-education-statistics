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
${PUBLICATION_NAME}=    Publish content %{RUN_IDENTIFIER}
${RELEASE_NAME}=        Calendar year 2001
${IMAGE_ALT_TEXT}=      Alt text for the uploaded content image


*** Test Cases ***
Create new publication and release via API
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    CY    2001    AdHocStatistics

Navigate to release content
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}
    user clicks link    Content
    user waits until page contains button    Add dashboards section
    user waits until page contains button    Add new section

Add headline text block to release content
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Add Related dashboards section to release content
    user clicks button    Add dashboards section
    user waits until page contains accordion section    View related dashboard(s)

    user opens accordion section    View related dashboard(s)    id:data-accordion
    user adds content to related dashboards text block    Related dashboards test text

Add an accordion section to release content
    user clicks button    Add new section
    user changes accordion section title    1    Test section

Add text block with link to absence glossary entry to accordion section
    user adds text block to editable accordion section    Test section    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    ${block}=    user starts editing accordion section text block    Test section    1
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

    # We add text to avoid the "Enter content" error displaying when clicking the "Insert" toolbar button, which was causing the toolbar menu to not open
    user presses keys    Test content block text

    ${toolbar}=    get editor toolbar    ${block}
    user clicks element    css:[data-cke-tooltip-text="Insert"]    ${toolbar}
    user clicks button    Insert glossary link
    ${modal}=    user waits until modal is visible    Insert glossary link
    user enters text into element    id:glossarySearch-input    abs
    user waits until page contains element    id:glossarySearch-options
    user clicks element    id:glossarySearch-option-0
    user clicks button    Insert    ${modal}
    user waits until modal is not visible    Insert glossary link
    user saves autosaving text block    ${block}
    user waits until parent contains button    ${block}    Absence

Add accordion section with an image to release content
    user clicks button    Add new section
    user changes accordion section title    2    Test image section
    user adds text block to editable accordion section    Test image section
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user adds content to autosaving accordion section text block    Test image section    1
    ...    Test image section text    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

Upload image with alt text to the image section text block
    user scrolls down    100
    user adds image to accordion section text block with retry    Test image section    1
    ...    test-infographic.png    ${IMAGE_ALT_TEXT}    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    ...    autosaving=${True}

    user checks accordion section text block contains image with alt text    Test image section    1
    ...    ${IMAGE_ALT_TEXT}    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user checks image has loaded    xpath://img[@alt="${IMAGE_ALT_TEXT}"]

Check glossary info icon appears on release preview
    user clicks radio    Preview release page
    user waits until page contains button    Absence

Check uploaded image appears on release preview
    user waits until page contains element
    ...    xpath://img[@alt="${IMAGE_ALT_TEXT}" and starts-with(@src, "/api/releases/")]
    user checks image has loaded    xpath://img[@alt="${IMAGE_ALT_TEXT}"]

Click glossary info icon and validate glossary entry
    user closes admin feedback banner if needed

    user clicks button    Absence
    ${modal}=    user waits until modal is visible    Absence
    user checks page contains    When a pupil misses (or is absent from) at least 1 possible school session.
    user clicks button    Close    ${modal}
    user waits until page does not contain element    xpath://h2[text()="Absence"]
    user checks page does not contain    When a pupil misses (or is absent from) at least 1 possible school session.

Approve release
    user approves original release for immediate publication

Get public release link
    ${PUBLIC_RELEASE_LINK}=    user gets url public release will be accessible at
    Set Suite Variable    ${PUBLIC_RELEASE_LINK}

Verify newly published release is public
    user navigates to public release page    ${PUBLIC_RELEASE_LINK}    ${PUBLICATION_NAME}    ${RELEASE_NAME}

Verify uploaded image is displayed on the public release page
    user checks section with ID contains elements and back to top link    section-test-image-section
    ...    Test image section text

    # A mismatched image url placeholder leaves a literal "{releaseId}" in the src, which renders an image
    # with the correct alt text that never actually loads
    user waits until parent contains element    id:section-test-image-section
    ...    xpath:.//img[@alt="${IMAGE_ALT_TEXT}" and contains(@src, "/api/releases/") and contains(@src, "/images/") and not(contains(@src, "{"))]
    user checks image has loaded    xpath://img[@alt="${IMAGE_ALT_TEXT}"]

Check latest release is correct
    user checks page contains    Latest release

Check latest release contains related data dashboards section
    user clicks link    Explore and download data
    user waits until h2 is visible    Explore data used in this release
    user waits until h2 is visible    Data dashboards
    user checks section with ID contains elements and back to top link    data-dashboards-section
    ...    Related dashboards test text

Navigate back to the release home tab
    user clicks link    Release home
    user waits until h2 is visible    Headline facts and figures

Click glossary info icon and verify entry is correct
    user clicks button    Absence
    ${modal}=    user waits until modal is visible    Absence
    user waits until h2 is visible    Absence
    user checks page contains    When a pupil misses (or is absent from) at least 1 possible school session.
    user clicks button    Close    ${modal}
    user waits until page does not contain element    xpath://h2[text()="Absence"]
    user checks page does not contain    When a pupil misses (or is absent from) at least 1 possible school session.

Return to Admin and verify the image on the published release content page
    user navigates to admin dashboard    Bau1
    user navigates to published release page from dashboard    ${PUBLICATION_NAME}    ${RELEASE_NAME}

    # A published release's content page is read only, so the content sections are rendered with the same
    # ids as the public release page rather than as editable accordion sections
    user clicks link    Content
    user waits until page contains element    id:section-test-image-section    %{WAIT_MEDIUM}
    user waits until element contains    id:section-test-image-section    Test image section text

    user waits until parent contains element    id:section-test-image-section
    ...    xpath:.//img[@alt="${IMAGE_ALT_TEXT}" and starts-with(@src, "/api/releases/")]
    user checks image has loaded    xpath://img[@alt="${IMAGE_ALT_TEXT}"]
