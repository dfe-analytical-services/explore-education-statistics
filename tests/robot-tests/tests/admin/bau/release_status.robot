*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${PUBLICATION_NAME}             UI tests - release status %{RUN_IDENTIFIER}
${ADOPTED_PUBLICATION_NAME}     UI tests - release status publication with adoptable methodology %{RUN_IDENTIFIER}


*** Test Cases ***
Create new publication and release via API
    ${PUBLICATION_ID}    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    FY    3000

Go to release sign off page and verify initial release checklist
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    Financial year 3000-01

Validate checklist errors and warnings
    user edits release status

    user checks checklist warnings contains
    ...    4 things you may have forgotten, but do not need to resolve to publish this release.
    user checks checklist warnings contains link    An in-EES methodology page has not been linked to this publication
    user checks checklist warnings contains link    No next expected release date has been added
    user checks checklist warnings contains link    No data files uploaded
    user checks checklist warnings contains link    A public pre-release access list has not been created

    user checks checklist errors contains
    ...    1 issue that must be resolved before this release can be published.
    user checks checklist errors contains link
    ...    Release must contain a key statistic or a non-empty headline text block

    user checks page does not contain testid    releaseChecklist-success

Add headline text block to Content page
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Validate checklist errors and warnings after adding headline text block
    user edits release status

    user checks checklist warnings contains
    ...    4 things you may have forgotten, but do not need to resolve to publish this release.
    user checks checklist warnings contains link    An in-EES methodology page has not been linked to this publication
    user checks checklist warnings contains link    No next expected release date has been added
    user checks checklist warnings contains link    No data files uploaded
    user checks checklist warnings contains link    A public pre-release access list has not been created

    user checks page does not contain testid    releaseChecklist-errors
    user checks page does not contain testid    releaseChecklist-success

Add empty Summary section text block to the page
    user navigates to content page    ${PUBLICATION_NAME}
    user clicks button    Add a summary text block    id:releaseSummary
    user waits until element contains    id:releaseSummary    This section is empty    %{WAIT_SMALL}

Add content section with empty content block to the page
    user creates new content section    1    Test section one
    user adds text block to editable accordion section    Test section one    id:releaseMainContent

Add empty content section to the page
    user creates new content section    2    Test section two

Add empty Related dashboards section text block to the page
    user clicks button    Add dashboards section
    user waits until page contains accordion section    View related dashboard(s)

Validate checklist errors and warnings after adding empty content sections
    user edits release status

    user checks checklist warnings contains
    ...    4 things you may have forgotten, but do not need to resolve to publish this release.
    user checks checklist warnings contains link    An in-EES methodology page has not been linked to this publication
    user checks checklist warnings contains link    No next expected release date has been added
    user checks checklist warnings contains link    No data files uploaded
    user checks checklist warnings contains link    A public pre-release access list has not been created

    user checks checklist errors contains
    ...    4 issues that must be resolved before this release can be published.
    user checks checklist errors contains link
    ...    Release content should not contain an empty summary section
    user checks checklist errors contains link
    ...    Release content should not contain any empty sections
    user checks checklist errors contains link
    ...    Release content should not contain empty text blocks
    user checks checklist errors contains link
    ...    Release content should not contain an empty related dashboards section

Add content to text block in Summary section
    user navigates to content page    ${PUBLICATION_NAME}
    user adds content to summary text block
    ...    Summary test text

Add content to text block in Test section one
    user opens accordion section    Test section one    id:releaseMainContent
    user adds content to autosaving accordion section text block    Test section one    1
    ...    Test section one text    id:releaseMainContent

Add text block with content to Test section two
    user opens accordion section    Test section two    id:releaseMainContent
    user adds text block to editable accordion section    Test section two    id:releaseMainContent
    user adds content to autosaving accordion section text block    Test section two    1
    ...    Test section two text    id:releaseMainContent

Add content to text block in Related dashboards section
    user opens accordion section    View related dashboard(s)    id:data-accordion
    user adds content to autosaving text block    id:related-dashboards-content
    ...    Related dashboards test text

Validate checklist errors and warnings after adding content to text blocks
    user edits release status

    user checks checklist warnings contains
    ...    4 things you may have forgotten, but do not need to resolve to publish this release.
    user checks checklist warnings contains link    An in-EES methodology page has not been linked to this publication
    user checks checklist warnings contains link    No next expected release date has been added
    user checks checklist warnings contains link    No data files uploaded
    user checks checklist warnings contains link    A public pre-release access list has not been created

    user checks page does not contain testid    releaseChecklist-errors
    user checks page does not contain testid    releaseChecklist-success

Submit release for Higher Review
    user clicks radio    Ready for higher review (this will notify approvers)
    user enters text into element    id:releaseStatusForm-internalReleaseNote    Submitted for Higher Review
    user enters text into element    id:releaseStatusForm-nextReleaseDate-month    12
    user enters text into element    id:releaseStatusForm-nextReleaseDate-year    3001
    user clicks button    Update status

Verify release status is Higher Review
    user checks summary list contains    Current status    Awaiting higher review
    user checks summary list contains    Scheduled release    Not scheduled
    user checks summary list contains    Next release expected    December 3001

Verify release checklist has not been updated by status
    user edits release status

    user checks checklist warnings contains
    ...    3 things you may have forgotten, but do not need to resolve to publish this release.
    user checks checklist warnings contains link    An in-EES methodology page has not been linked to this publication
    user checks checklist warnings contains link    No data files uploaded
    user checks checklist warnings contains link    A public pre-release access list has not been created

    user checks page does not contain testid    releaseChecklist-errors
    user checks page does not contain testid    releaseChecklist-success

Add public prerelease access list via release checklist
    user clicks link    A public pre-release access list has not been created
    user creates public prerelease access list    Test public access list

Verify release checklist has been updated
    user edits release status

    user checks checklist warnings contains
    ...    2 things you may have forgotten, but do not need to resolve to publish this release.
    user checks checklist warnings contains link    An in-EES methodology page has not been linked to this publication
    user checks checklist warnings contains link    No data files uploaded

    user checks page does not contain testid    releaseChecklist-errors
    user checks page does not contain testid    releaseChecklist-success

Approve release
    user clicks radio    Approved for publication
    user enters text into element    id:releaseStatusForm-internalReleaseNote    Approved for release

    user clicks radio    On a specific date
    user enters text into element    id:releaseStatusForm-publishScheduled-day    1
    user enters text into element    id:releaseStatusForm-publishScheduled-month    12
    user enters text into element    id:releaseStatusForm-publishScheduled-year    3000

    user enters text into element    id:releaseStatusForm-nextReleaseDate-month    3
    user enters text into element    id:releaseStatusForm-nextReleaseDate-year    3002

    user clicks button    Update status
    user waits until h2 is visible    Confirm publish date
    user clicks button    Confirm

Verify release status is Approved
    user checks summary list contains    Current status    Approved
    user checks summary list contains    Scheduled release    1 December 3000
    user checks summary list contains    Next release expected    March 3002
    user waits for release process status to be    Scheduled    %{WAIT_LONG}

Move release status back to Draft
    user puts release into draft
    ...    release_note=Moved back to draft
    ...    next_release_date_month=1
    ...    next_release_date_year=3001
    ...    expected_next_release_date=January 3001

Check that having a Draft owned Methodology attached to this Release's Publication will show a checklist warning
    user creates methodology for publication    ${PUBLICATION_NAME}
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    Financial year 3000-01
    user edits release status
    user waits until element is visible    testid:releaseChecklist-warnings    %{WAIT_SMALL}
    user checks checklist warnings contains
    ...    2 things you may have forgotten, but do not need to resolve to publish this release.
    user checks checklist warnings contains link    A methodology for this publication is not yet approved

Approve the owned methodology and verify the warning disappears
    user approves methodology for publication    ${PUBLICATION_NAME}
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    Financial year 3000-01
    user edits release status
    user waits until element is visible    testid:releaseChecklist-warnings    %{WAIT_SMALL}
    user checks checklist warnings contains
    ...    1 thing you may have forgotten, but do not need to resolve to publish this release.
    user checks checklist warnings does not contain link    A methodology for this publication is not yet approved

Adopt a Draft methodology
    user creates test publication via api    ${ADOPTED_PUBLICATION_NAME}
    user creates methodology for publication    ${ADOPTED_PUBLICATION_NAME}

    user waits until page contains link    ${ADOPTED_PUBLICATION_NAME}

    user navigates to methodologies on publication page    ${PUBLICATION_NAME}

    user clicks link    Adopt an existing methodology
    user waits until h2 is visible    Adopt a methodology
    user clicks radio    ${ADOPTED_PUBLICATION_NAME}
    user clicks button    Save

Check that having a Draft methodology adopted by this Release's Publication will show a checklist warning
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    Financial year 3000-01
    user edits release status
    user checks checklist warnings contains
    ...    2 things you may have forgotten, but do not need to resolve to publish this release.
    user checks checklist warnings contains link    A methodology for this publication is not yet approved

Approve the adopted methodology and verify the warning disappears
    user approves methodology for publication    ${ADOPTED_PUBLICATION_NAME}
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    Financial year 3000-01
    user edits release status
    user checks checklist warnings contains
    ...    1 thing you may have forgotten, but do not need to resolve to publish this release.
    user checks checklist warnings does not contain link    A methodology for this publication is not yet approved


*** Keywords ***
user edits release status
    user clicks link    Sign off
    user waits until h2 is visible    Sign off    %{WAIT_SMALL}

    user clicks button    Edit release status
    user waits until h2 is visible    Edit release status    %{WAIT_SMALL}

user checks checklist warnings contains
    [Arguments]    ${text}
    user waits until page contains testid    releaseChecklist-warnings
    user waits until element contains    testid:releaseChecklist-warnings    ${text}

user checks checklist warnings contains link
    [Arguments]    ${text}
    user waits until page contains testid    releaseChecklist-warnings
    user waits until parent contains element    testid:releaseChecklist-warnings    link:${text}

user checks checklist warnings does not contain link
    [Arguments]    ${text}
    user waits until page contains testid    releaseChecklist-warnings
    user waits until parent does not contain element    testid:releaseChecklist-warnings    link:${text}

user checks checklist errors contains
    [Arguments]    ${text}
    user waits until page contains testid    releaseChecklist-errors
    user waits until element contains    testid:releaseChecklist-errors    ${text}

user checks checklist errors contains link
    [Arguments]    ${text}
    user waits until page contains testid    releaseChecklist-errors
    user waits until parent contains element    testid:releaseChecklist-errors    link:${text}
