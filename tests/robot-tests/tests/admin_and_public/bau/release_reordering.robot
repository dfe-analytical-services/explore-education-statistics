*** Settings ***
Resource            ../../libs/public-common.robot
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Library             ../../libs/admin_api.py

Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Variables ***
${PUBLICATION_NAME}=                        UI tests - legacy releases %{RUN_IDENTIFIER}
${PUBLICATION_SLUG}=                        ui-tests-legacy-releases-%{RUN_IDENTIFIER}
${RELEASE_1_NAME}=                          Academic year 2020/21
${RELEASE_2_NAME}=                          Academic year Q1 2022/23
${LEGACY_RELEASE_1_DESCRIPTION}=            legacy release 1
${LEGACY_RELEASE_1_URL}=                    http://test.url/1
${LEGACY_RELEASE_1_DESCRIPTION_UPDATED}=    legacy release 1 updated
${LEGACY_RELEASE_1_URL_UPDATED}=            http://test.url/1/updated
${LEGACY_RELEASE_2_DESCRIPTION}=            legacy release 2
${LEGACY_RELEASE_2_URL}=                    http://test.url/2


*** Test Cases ***
Create new publication via api
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    set suite variable    ${PUBLICATION_ID}

Validate publication release order table is empty
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Release order
    user waits until h2 is visible    Release order
    user checks page contains    No releases for this publication.

Create first legacy release
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Release order
    user waits until h2 is visible    Release order
    user creates legacy release    ${LEGACY_RELEASE_1_DESCRIPTION}    ${LEGACY_RELEASE_1_URL}

Validate publication release order table headings
    user checks table column heading contains    1    1    Description    testid:release-series
    user checks table column heading contains    1    2    URL    testid:release-series
    user checks table column heading contains    1    3    Status    testid:release-series
    user checks table column heading contains    1    4    Actions    testid:release-series

Validate first legacy release exists in the publication release order
    user checks table body has x rows    1    testid:release-series

    user checks table cell contains    1    1    ${LEGACY_RELEASE_1_DESCRIPTION}
    user checks table cell contains    1    2    ${LEGACY_RELEASE_1_URL}
    user checks table cell contains    1    3    Legacy release
    user checks table cell contains    1    4    Edit
    user checks table cell contains    1    4    Delete

Create first release via api
    user creates test release via api    ${PUBLICATION_ID}    AY    2020

    set suite variable    ${PUBLIC_RELEASE_1_URL}
    ...    %{PUBLIC_URL}/find-statistics/${PUBLICATION_SLUG}/2020-21

Validate first release exists in the publication release order with status unpublished
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Release order
    user waits until h2 is visible    Release order
    user checks table body has x rows    2    testid:release-series

    user checks table cell contains    1    1    ${RELEASE_1_NAME}
    user checks table cell contains    1    2    ${PUBLIC_RELEASE_1_URL}
    user checks table cell contains    1    3    Unpublished

    user checks table cell contains    2    1    ${LEGACY_RELEASE_1_DESCRIPTION}
    user checks table cell contains    2    2    ${LEGACY_RELEASE_1_URL}
    user checks table cell contains    2    3    Legacy release
    user checks table cell contains    2    4    Edit
    user checks table cell contains    2    4    Delete

Create second legacy release
    user creates legacy release    ${LEGACY_RELEASE_2_DESCRIPTION}    ${LEGACY_RELEASE_2_URL}

Validate second legacy release exists in the publication release order
    user checks table body has x rows    3    testid:release-series

    user checks table cell contains    1    1    ${RELEASE_1_NAME}
    user checks table cell contains    1    2    ${PUBLIC_RELEASE_1_URL}
    user checks table cell contains    1    3    Unpublished

    user checks table cell contains    2    1    ${LEGACY_RELEASE_1_DESCRIPTION}
    user checks table cell contains    2    2    ${LEGACY_RELEASE_1_URL}
    user checks table cell contains    2    3    Legacy release
    user checks table cell contains    2    4    Edit
    user checks table cell contains    2    4    Delete

    user checks table cell contains    3    1    ${LEGACY_RELEASE_2_DESCRIPTION}
    user checks table cell contains    3    2    ${LEGACY_RELEASE_2_URL}
    user checks table cell contains    3    3    Legacy release
    user checks table cell contains    3    4    Edit
    user checks table cell contains    3    4    Delete

Add headline text block to first release content page
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_1_NAME}
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Approve first release
    user clicks link    Sign off
    user approves original release for immediate publication

Validate first release has latest release status in publication release order
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Release order
    user waits until h2 is visible    Release order

    user checks table body has x rows    3    testid:release-series

    user checks table cell contains    1    1    ${RELEASE_1_NAME}
    user checks table cell contains    1    2    ${PUBLIC_RELEASE_1_URL}
    user checks table cell contains    1    3    Latest release

    user checks table cell contains    2    1    ${LEGACY_RELEASE_1_DESCRIPTION}
    user checks table cell contains    2    2    ${LEGACY_RELEASE_1_URL}
    user checks table cell contains    2    3    Legacy release
    user checks table cell contains    2    4    Edit
    user checks table cell contains    2    4    Delete

    user checks table cell contains    3    1    ${LEGACY_RELEASE_2_DESCRIPTION}
    user checks table cell contains    3    2    ${LEGACY_RELEASE_2_URL}
    user checks table cell contains    3    3    Legacy release
    user checks table cell contains    3    4    Edit
    user checks table cell contains    3    4    Delete

Navigate to first published release on public frontend
    user navigates to public frontend    ${PUBLIC_RELEASE_1_URL}

Validate first published release on public frontend is the latest data
    user checks page contains    This is the latest data

Validate other releases section of first published release includes legacy releases
    user checks number of other releases is correct    2
    ${view_releases}=    user opens details dropdown    View releases (2)

    user checks other release is shown in position    ${LEGACY_RELEASE_1_DESCRIPTION}    1
    user checks other release is shown in position    ${LEGACY_RELEASE_2_DESCRIPTION}    2

    user checks page contains link with text and url
    ...    ${LEGACY_RELEASE_1_DESCRIPTION}
    ...    ${LEGACY_RELEASE_1_URL}
    ...    ${view_releases}

    user checks page contains link with text and url
    ...    ${LEGACY_RELEASE_2_DESCRIPTION}
    ...    ${LEGACY_RELEASE_2_URL}
    ...    ${view_releases}

Update first legacy release
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Release order
    user waits until h2 is visible    Release order

    user clicks element    xpath://tr[2]//*[text()="Edit"]
    ${modal}=    user waits until modal is visible    Edit legacy release
    user clicks button    OK    ${modal}

    user waits until page contains element    id:releaseSeriesLegacyLinkForm-description
    user enters text into element    id:releaseSeriesLegacyLinkForm-description
    ...    ${LEGACY_RELEASE_1_DESCRIPTION_UPDATED}
    user enters text into element    id:releaseSeriesLegacyLinkForm-url    ${LEGACY_RELEASE_1_URL_UPDATED}
    user clicks button    Save legacy release

Validate the first legacy release is updated
    user waits until h2 is visible    Release order
    user checks table body has x rows    3    testid:release-series

    user checks table cell contains    1    1    ${RELEASE_1_NAME}
    user checks table cell contains    1    2    ${PUBLIC_RELEASE_1_URL}
    user checks table cell contains    1    3    Latest release

    user checks table cell contains    2    1    ${LEGACY_RELEASE_1_DESCRIPTION_UPDATED}
    user checks table cell contains    2    2    ${LEGACY_RELEASE_1_URL_UPDATED}
    user checks table cell contains    2    3    Legacy release
    user checks table cell contains    2    4    Edit
    user checks table cell contains    2    4    Delete

    user checks table cell contains    3    1    ${LEGACY_RELEASE_2_DESCRIPTION}
    user checks table cell contains    3    2    ${LEGACY_RELEASE_2_URL}
    user checks table cell contains    3    3    Legacy release
    user checks table cell contains    3    4    Edit
    user checks table cell contains    3    4    Delete

Reorder the publication releases
    user clicks button    Reorder releases
    ${modal}=    user waits until modal is visible    Reorder releases
    user clicks button    OK    ${modal}
    user waits until modal is not visible    Reorder releases
    user waits until page contains button    Confirm order

    click element    xpath://div[text()="${RELEASE_1_NAME}"]    CTRL
    user presses keys    ${SPACE}
    user presses keys    ARROW_DOWN
    user presses keys    ARROW_DOWN
    user presses keys    ${SPACE}

    click element    xpath://div[text()="${LEGACY_RELEASE_2_DESCRIPTION}"]    CTRL
    user presses keys    ${SPACE}
    user presses keys    ARROW_UP
    user presses keys    ${SPACE}

    user clicks button    Confirm order
    sleep    2

Validate reordered publication releases
    user waits until page contains button    Reorder releases
    user checks table body has x rows    3    testid:release-series

    user checks table cell contains    1    1    ${LEGACY_RELEASE_2_DESCRIPTION}
    user checks table cell contains    1    2    ${LEGACY_RELEASE_2_URL}
    user checks table cell contains    1    3    Legacy release
    user checks table cell contains    1    4    Edit
    user checks table cell contains    1    4    Delete

    user checks table cell contains    2    1    ${LEGACY_RELEASE_1_DESCRIPTION_UPDATED}
    user checks table cell contains    2    2    ${LEGACY_RELEASE_1_URL_UPDATED}
    user checks table cell contains    2    3    Legacy release
    user checks table cell contains    2    4    Edit
    user checks table cell contains    2    4    Delete

    user checks table cell contains    3    1    ${RELEASE_1_NAME}
    user checks table cell contains    3    2    ${PUBLIC_RELEASE_1_URL}
    user checks table cell contains    3    3    Latest release

Navigate to first published release on public frontend after reordering
    user navigates to public frontend    ${PUBLIC_RELEASE_1_URL}

Validate first published release is the latest data after reordering
    user checks page contains    This is the latest data

Validate other releases section of first published release contains updated legacy release in expected order
    user checks number of other releases is correct    2
    ${view_releases}=    user opens details dropdown    View releases (2)

    user checks other release is shown in position    ${LEGACY_RELEASE_2_DESCRIPTION}    1
    user checks other release is shown in position    ${LEGACY_RELEASE_1_DESCRIPTION_UPDATED}    2

    user checks page contains link with text and url
    ...    ${LEGACY_RELEASE_1_DESCRIPTION_UPDATED}
    ...    ${LEGACY_RELEASE_1_URL_UPDATED}
    ...    ${view_releases}

    user checks page contains link with text and url
    ...    ${LEGACY_RELEASE_2_DESCRIPTION}
    ...    ${LEGACY_RELEASE_2_URL}
    ...    ${view_releases}

Create second release via api
    user creates test release via api    ${PUBLICATION_ID}    AYQ1    2022
    set suite variable    ${PUBLIC_RELEASE_2_URL}
    ...    %{PUBLIC_URL}/find-statistics/${PUBLICATION_SLUG}/2022-23-q1

Validate second release exists in the publication release order with status unpublished
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Release order
    user waits until h2 is visible    Release order
    user checks table body has x rows    4    testid:release-series

    user checks table cell contains    1    1    ${RELEASE_2_NAME}
    user checks table cell contains    1    2    ${PUBLIC_RELEASE_2_URL}
    user checks table cell contains    1    3    Unpublished

    user checks table cell contains    2    1    ${LEGACY_RELEASE_2_DESCRIPTION}
    user checks table cell contains    2    2    ${LEGACY_RELEASE_2_URL}
    user checks table cell contains    2    3    Legacy release
    user checks table cell contains    2    4    Edit
    user checks table cell contains    2    4    Delete

    user checks table cell contains    3    1    ${LEGACY_RELEASE_1_DESCRIPTION_UPDATED}
    user checks table cell contains    3    2    ${LEGACY_RELEASE_1_URL_UPDATED}
    user checks table cell contains    3    3    Legacy release
    user checks table cell contains    3    4    Edit
    user checks table cell contains    3    4    Delete

    user checks table cell contains    4    1    ${RELEASE_1_NAME}
    user checks table cell contains    4    2    ${PUBLIC_RELEASE_1_URL}
    user checks table cell contains    4    3    Latest release

Add headline text block to second release content page
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_2_NAME}
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Approve second release
    user clicks link    Sign off
    user approves original release for immediate publication

Validate second release has latest release status in publication release order
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Release order
    user waits until h2 is visible    Release order

    user checks table body has x rows    4    testid:release-series

    user checks table cell contains    1    1    ${RELEASE_2_NAME}
    user checks table cell contains    1    2    ${PUBLIC_RELEASE_2_URL}
    user checks table cell contains    1    3    Latest release

    user checks table cell contains    2    1    ${LEGACY_RELEASE_2_DESCRIPTION}
    user checks table cell contains    2    2    ${LEGACY_RELEASE_2_URL}
    user checks table cell contains    2    3    Legacy release
    user checks table cell contains    2    4    Edit
    user checks table cell contains    2    4    Delete

    user checks table cell contains    3    1    ${LEGACY_RELEASE_1_DESCRIPTION_UPDATED}
    user checks table cell contains    3    2    ${LEGACY_RELEASE_1_URL_UPDATED}
    user checks table cell contains    3    3    Legacy release
    user checks table cell contains    3    4    Edit
    user checks table cell contains    3    4    Delete

    user checks table cell contains    4    1    ${RELEASE_1_NAME}
    user checks table cell contains    4    2    ${PUBLIC_RELEASE_1_URL}
    user checks table cell does not contain    4    3    Latest release

Navigate to second published release on public frontend
    user navigates to public frontend    ${PUBLIC_RELEASE_2_URL}

Validate second published release is the latest data
    user checks page contains    This is the latest data

Validate other releases section of second published release includes first release with expected order
    user checks number of other releases is correct    3
    ${view_releases}=    user opens details dropdown    View releases (3)

    user checks other release is shown in position    ${LEGACY_RELEASE_2_DESCRIPTION}    1
    user checks other release is shown in position    ${LEGACY_RELEASE_1_DESCRIPTION_UPDATED}    2
    user checks other release is shown in position    ${RELEASE_1_NAME}    3

Create amendment of second release
    user navigates to admin dashboard    Bau1
    user creates amendment for release    ${PUBLICATION_NAME}    ${RELEASE_2_NAME}

Add release note to amendment of second release
    user clicks link    Content
    user adds a release note    Test release note one

    ${date}=    get london date
    user waits until element contains    css:#release-notes li:nth-of-type(1) time    ${date}
    user waits until element contains    css:#release-notes li:nth-of-type(1) p    Test release note one

Approve second release amendment
    user approves amended release for immediate publication

Validate amended second release exists in publication release order
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Release order
    user waits until h2 is visible    Release order

    user checks table body has x rows    4    testid:release-series

    user checks table cell contains    1    1    ${RELEASE_2_NAME}
    user checks table cell contains    1    2    ${PUBLIC_RELEASE_2_URL}
    user checks table cell contains    1    3    Latest release

    user checks table cell contains    2    1    ${LEGACY_RELEASE_2_DESCRIPTION}
    user checks table cell contains    2    2    ${LEGACY_RELEASE_2_URL}
    user checks table cell contains    2    3    Legacy release
    user checks table cell contains    2    4    Edit
    user checks table cell contains    2    4    Delete

    user checks table cell contains    3    1    ${LEGACY_RELEASE_1_DESCRIPTION_UPDATED}
    user checks table cell contains    3    2    ${LEGACY_RELEASE_1_URL_UPDATED}
    user checks table cell contains    3    3    Legacy release
    user checks table cell contains    3    4    Edit
    user checks table cell contains    3    4    Delete

    user checks table cell contains    4    1    ${RELEASE_1_NAME}
    user checks table cell contains    4    2    ${PUBLIC_RELEASE_1_URL}
    user checks table cell does not contain    4    3    Latest release

Delete first legacy release
    user clicks button in table cell    3    4    Delete    testid:release-series
    ${modal}=    user waits until modal is visible    Delete legacy release
    user clicks button    Confirm    ${modal}
    user waits until modal is not visible    Delete legacy release

Validate first legacy release is deleted from publication release order
    user waits until page contains button    Reorder releases
    user checks table body has x rows    3    testid:release-series

    user checks table cell contains    1    1    ${RELEASE_2_NAME}
    user checks table cell contains    1    2    ${PUBLIC_RELEASE_2_URL}
    user checks table cell contains    1    3    Latest release

    user checks table cell contains    2    1    ${LEGACY_RELEASE_2_DESCRIPTION}
    user checks table cell contains    2    2    ${LEGACY_RELEASE_2_URL}
    user checks table cell contains    2    3    Legacy release
    user checks table cell contains    2    4    Edit
    user checks table cell contains    2    4    Delete

    user checks table cell contains    3    1    ${RELEASE_1_NAME}
    user checks table cell contains    3    2    ${PUBLIC_RELEASE_1_URL}
    user checks table cell does not contain    3    3    Latest release

Navigate to second published release on public frontend after deleting legacy release
    user navigates to public frontend    ${PUBLIC_RELEASE_2_URL}

Validate other releases section of second published release does not include first legacy release
    user checks number of other releases is correct    2
    ${view_releases}=    user opens details dropdown    View releases (2)

    user checks other release is shown in position    ${LEGACY_RELEASE_2_DESCRIPTION}    1
    user checks other release is shown in position    ${RELEASE_1_NAME}    2

Reorder the publication releases so the first release is the latest release
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Release order
    user waits until h2 is visible    Release order

    user clicks button    Reorder releases
    ${modal}=    user waits until modal is visible    Reorder releases
    user clicks button    OK    ${modal}
    user waits until modal is not visible    Reorder releases
    user waits until page contains button    Confirm order

    click element    xpath://div[text()="${RELEASE_1_NAME}"]    CTRL
    user presses keys    ${SPACE}
    user presses keys    ARROW_UP
    user presses keys    ARROW_UP
    user presses keys    ${SPACE}

    user clicks button    Confirm order
    sleep    2

Validate first release has latest release status in publication release order after reordering
    user waits until page contains button    Reorder releases
    user checks table body has x rows    3    testid:release-series

    user checks table cell contains    1    1    ${RELEASE_1_NAME}
    user checks table cell contains    1    2    ${PUBLIC_RELEASE_1_URL}
    user checks table cell contains    1    3    Latest release

    user checks table cell contains    2    1    ${RELEASE_2_NAME}
    user checks table cell contains    2    2    ${PUBLIC_RELEASE_2_URL}
    user checks table cell does not contain    2    3    Latest release

    user checks table cell contains    3    1    ${LEGACY_RELEASE_2_DESCRIPTION}
    user checks table cell contains    3    2    ${LEGACY_RELEASE_2_URL}
    user checks table cell contains    3    3    Legacy release
    user checks table cell contains    3    4    Edit
    user checks table cell contains    3    4    Delete

Navigate to first published release on public frontend after changing the latest release
    user navigates to public frontend    ${PUBLIC_RELEASE_1_URL}

Validate first published release is the latest data after changing the latest release
    user checks page contains    This is the latest data

Navigate to second published release on public frontend after changing the latest release
    user navigates to public frontend    ${PUBLIC_RELEASE_2_URL}

Validate second published release is not the latest data after changing the latest release
    user checks page contains    This is not the latest data
    user waits until page contains link    View latest data: ${RELEASE_1_NAME}
