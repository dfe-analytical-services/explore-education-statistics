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
${PUBLICATION_NAME}                 Release status %{RUN_IDENTIFIER}
${RELEASE_1_NAME}                   Academic year Q1 2200/01
${RELEASE_2_NAME}                   Calendar year 2001
${RELEASE_3_NAME}                   Financial year 2300-01
${ADOPTED_PUBLICATION_NAME}         Release status publication with adoptable methodology %{RUN_IDENTIFIER}
${PUBLICATION_NAME_DATAFILES}       ${PUBLICATION_NAME} -    datafiles-updated
${SUBJECT_NAME}                     Dates test subject


*** Test Cases ***
Create new publication and release via API
    ${PUBLICATION_ID}    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    FY    2100

Go to release sign off page and verify initial release checklist
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    Financial year 2100-01

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

    user waits until page does not contain testid    releaseChecklist-success

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

    user waits until page does not contain testid    releaseChecklist-errors
    user waits until page does not contain testid    releaseChecklist-success

Add empty Summary section text block to the page
    user navigates to content page    ${PUBLICATION_NAME}
    user clicks button    Add a summary text block    id:releaseSummary
    user waits until element contains    id:releaseSummary    This section is empty    %{WAIT_SMALL}

Add content section with empty content block to the page
    user creates new content section    1    Test section one
    user adds text block to editable accordion section    Test section one    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

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
    user opens accordion section    Test section one    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user adds content to autosaving accordion section text block    Test section one    1
    ...    Test section one text    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

Add text block with content to Test section two
    user opens accordion section    Test section two    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user adds text block to editable accordion section    Test section two    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user adds content to autosaving accordion section text block    Test section two    1
    ...    Test section two text    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user reloads page
    user scrolls to accordion section    View related dashboard(s)    id:data-accordion

Add content to text block in Related dashboards section
    user waits until page contains accordion section    View related dashboard(s)
    user opens accordion section    View related dashboard(s)    id:data-accordion
    user adds content to related dashboards text block    Related dashboards test text

Validate checklist errors and warnings after adding content to text blocks
    user edits release status

    user checks checklist warnings contains
    ...    4 things you may have forgotten, but do not need to resolve to publish this release.
    user checks checklist warnings contains link    An in-EES methodology page has not been linked to this publication
    user checks checklist warnings contains link    No next expected release date has been added
    user checks checklist warnings contains link    No data files uploaded
    user checks checklist warnings contains link    A public pre-release access list has not been created

    user waits until page does not contain testid    releaseChecklist-errors
    user waits until page does not contain testid    releaseChecklist-success

Submit release for Higher Review
    user clicks radio    Ready for higher review (this will notify approvers)
    user enters text into element    id:releaseStatusForm-internalReleaseNote    Submitted for Higher Review
    user enters text into element    id:releaseStatusForm-nextReleaseDate-month    12
    user enters text into element    id:releaseStatusForm-nextReleaseDate-year    2100
    user clicks button    Update status

Verify release status is Higher Review
    user checks summary list contains    Current status    Awaiting higher review
    user checks summary list contains    Scheduled release    Not scheduled
    user checks summary list contains    Next release expected    December 2100

Verify release checklist has not been updated by status
    user edits release status

    user checks checklist warnings contains
    ...    3 things you may have forgotten, but do not need to resolve to publish this release.
    user checks checklist warnings contains link    An in-EES methodology page has not been linked to this publication
    user checks checklist warnings contains link    No data files uploaded
    user checks checklist warnings contains link    A public pre-release access list has not been created

    user waits until page does not contain testid    releaseChecklist-errors
    user waits until page does not contain testid    releaseChecklist-success

Add public prerelease access list via release checklist
    user clicks link    A public pre-release access list has not been created
    user creates public prerelease access list    Test public access list

Verify release checklist has been updated
    user edits release status

    user checks checklist warnings contains
    ...    2 things you may have forgotten, but do not need to resolve to publish this release.
    user checks checklist warnings contains link    An in-EES methodology page has not been linked to this publication
    user checks checklist warnings contains link    No data files uploaded

    user waits until page does not contain testid    releaseChecklist-errors
    user waits until page does not contain testid    releaseChecklist-success

Approve release
    user clicks radio    Approved for publication
    user enters text into element    id:releaseStatusForm-internalReleaseNote    Approved for release

    user clicks radio    On a specific date
    user enters text into element    id:releaseStatusForm-publishScheduled-day    1
    user enters text into element    id:releaseStatusForm-publishScheduled-month    12
    user enters text into element    id:releaseStatusForm-publishScheduled-year    2100

    user enters text into element    id:releaseStatusForm-nextReleaseDate-month    3
    user enters text into element    id:releaseStatusForm-nextReleaseDate-year    2101

    user clicks button    Update status
    user waits until h2 is visible    Confirm publish date
    user clicks button    Confirm

Verify release status is Approved
    user checks summary list contains    Current status    Approved
    user checks summary list contains    Scheduled release    1 December 2100
    user checks summary list contains    Next release expected    March 2101
    user waits for release process status to be    Scheduled    %{WAIT_LONG}

Move release status back to Draft
    user puts release into draft
    ...    release_note=Moved back to draft
    ...    next_release_date_month=1
    ...    next_release_date_year=2102
    ...    expected_next_release_date=January 2102

Check that having a Draft owned Methodology attached to this Release's Publication will show a checklist warning
    user creates methodology for publication    ${PUBLICATION_NAME}
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    Financial year 2100-01
    user edits release status
    user waits until element is visible    testid:releaseChecklist-warnings    %{WAIT_SMALL}
    user checks checklist warnings contains
    ...    2 things you may have forgotten, but do not need to resolve to publish this release.
    user checks checklist warnings contains link    A methodology for this publication is not yet approved

Approve the owned methodology and verify the warning disappears
    user approves methodology for publication    ${PUBLICATION_NAME}
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    Financial year 2100-01
    user edits release status
    user waits until element is visible    testid:releaseChecklist-warnings    %{WAIT_SMALL}
    user checks checklist warnings contains
    ...    1 thing you may have forgotten, but do not need to resolve to publish this release.
    user checks checklist warnings does not contain link    A methodology for this publication is not yet approved

Create published methodology for adopted publication
    ${adopted_publication_id}    user creates test publication via api    ${ADOPTED_PUBLICATION_NAME}
    user creates test release via api    ${adopted_publication_id}    CY    2001
    user navigates to draft release page from dashboard    ${ADOPTED_PUBLICATION_NAME}    ${RELEASE_2_NAME}
    user navigates to content page    ${ADOPTED_PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text
    user approves release for immediate publication

    user creates methodology for publication    ${ADOPTED_PUBLICATION_NAME}
    user approves methodology for publication    ${ADOPTED_PUBLICATION_NAME}    ${ADOPTED_PUBLICATION_NAME}

Create new draft amendment for methodology
    user creates methodology amendment for publication    ${ADOPTED_PUBLICATION_NAME}    ${ADOPTED_PUBLICATION_NAME}

Adopt a methodology with a draft amendment
    user navigates to methodologies on publication page    ${PUBLICATION_NAME}

    user clicks link    Adopt an existing methodology
    user waits until h2 is visible    Adopt a methodology
    user clicks radio    ${ADOPTED_PUBLICATION_NAME}
    user clicks button    Save
    user waits until page finishes loading

Check that having a draft methodology amendment adopted by this Release's Publication will show a checklist warning
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    Financial year 2100-01
    user edits release status
    user checks checklist warnings contains
    ...    2 things you may have forgotten, but do not need to resolve to publish this release.
    user checks checklist warnings contains link    A methodology for this publication is not yet approved

Approve the adopted methodology amendment and verify the warning disappears
    user approves methodology for publication    ${ADOPTED_PUBLICATION_NAME}
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    Financial year 2100-01
    user edits release status
    user checks checklist warnings contains
    ...    1 thing you may have forgotten, but do not need to resolve to publish this release.
    user checks checklist warnings does not contain link    A methodology for this publication is not yet approved

Publish new release from adopted publication and make an amendment
    user navigates to publication page from dashboard    ${ADOPTED_PUBLICATION_NAME}
    user creates release from publication page    ${ADOPTED_PUBLICATION_NAME}    Academic year Q1    2200
    user navigates to content page    ${ADOPTED_PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text
    user approves release for immediate publication

    user navigates to admin dashboard    Bau1
    user creates amendment for release    ${ADOPTED_PUBLICATION_NAME}    ${RELEASE_1_NAME}

Verify the checklist errors and warnings for amendment
    user edits release status
    user checks checklist warnings contains
    ...    2 things you may have forgotten, but do not need to resolve to publish this release.
    user checks checklist warnings contains link    No data files uploaded
    user checks checklist warnings contains link    A public pre-release access list has not been created

    user checks checklist errors contains link
    ...    A public release note for this amendment is required, add this near the top of the content page
    user waits until page does not contain testid    releaseChecklist-success

Navigate to contents page and add a release note
    user clicks link    Content
    user waits until h2 is visible    ${ADOPTED_PUBLICATION_NAME}
    user waits until page contains button    Add a summary text block
    user clicks button    Add note
    user enters text into element    id:create-release-note-form-reason    Test release note one
    user clicks button    Save note

Publish the release amendment immediately
    user approves release for immediate publication

Create third release
    ${DATA_FILES_PUBLICATION_ID}    user creates test publication via api    ${PUBLICATION_NAME_DATAFILES}
    user creates test release via api    ${DATA_FILES_PUBLICATION_ID}    FY    2300
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME_DATAFILES}
    ...    ${RELEASE_3_NAME}

Upload data files
    user uploads subject and waits until complete    Dates test subject    dates.csv    dates.meta.csv
    user clicks link    Data and files
    user waits until page contains data uploads table
    user clicks link    Replace data

    user waits until h2 is visible    Data file details
    user checks headed table body row contains    Data file import status    Complete    wait=%{WAIT_LONG}

Navigate to data replacement page
    user waits until h2 is visible    Upload replacement data    %{WAIT_MEDIUM}
    user chooses file    id:dataFileUploadForm-dataFile    ${FILES_DIR}dates-replacement.csv
    user chooses file    id:dataFileUploadForm-metadataFile    ${FILES_DIR}dates-replacement.meta.csv
    user clicks button    Upload data files

    user waits until page contains element    testid:Replacement Title
    user reloads page
    user checks table column heading contains    1    1    Original file
    user checks table column heading contains    1    2    Replacement file
    user checks headed table body row cell contains    Status    2    Complete    wait=%{WAIT_DATA_FILE_IMPORT}

Validate checklist errors
    user edits release status

    user checks checklist errors contains
    ...    3 issues that must be resolved before this release can be published.
    user checks checklist errors contains link
    ...    All data file replacements must be completed
    user checks checklist errors contains link
    ...    All summary information must be completed on the data guidance page
    user checks checklist errors contains link
    ...    Release must contain a key statistic or a non-empty headline text block

    user waits until page does not contain testid    releaseChecklist-success

Navigate to data upload and confirm data replacement
    user clicks link    Data and files
    user waits until page contains element    testid:Data file replacements table
    user clicks button    Confirm replacement

Upload the larger data file via data upload
    # TODO https://github.com/dfe-analytical-services/eesyscreener/issues/2
    Skip    Skipping until the "large-data-set.csv" is compatible with screener.

    user uploads subject and waits until importing
    ...    ${SUBJECT_NAME}-updated
    ...    large-data-set.csv
    ...    large-data-set.meta.csv

Validate checklist errors (3rd release)
    # TODO https://github.com/dfe-analytical-services/eesyscreener/issues/2
    Skip    Skipping until the "large-data-set.csv" is compatible with screener.

    user edits release status

    user checks checklist errors contains
    ...    3 issues that must be resolved before this release can be published.
    user checks checklist errors contains link
    ...    All data imports must be completed
    user checks checklist errors contains link
    ...    All summary information must be completed on the data guidance page
    user checks checklist errors contains link
    ...    Release must contain a key statistic or a non-empty headline text block
    user waits until page does not contain testid    releaseChecklist-success

Add data guidance to subject
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release

    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance
    user waits until page contains element    id:dataGuidance-dataFiles

    user waits until page contains accordion section    ${SUBJECT_NAME}

    user enters text into data guidance data file content editor    ${SUBJECT_NAME}
    ...    ${SUBJECT_NAME} Main guidance content

    # TODO https://github.com/dfe-analytical-services/eesyscreener/issues/2
    Skip    Skipping until the "large-data-set.csv" is compatible with screener.

    user waits until page contains accordion section    ${SUBJECT_NAME}-updated

    user enters text into data guidance data file content editor    ${SUBJECT_NAME}-updated
    ...    ${SUBJECT_NAME} Main guidance content

Save data guidance (third release)
    user clicks button    Save guidance
    user waits for caches to expire    #prevent intermittent failure in pipeline - data guidance to be saved    before navigating to content page

Add headline text block to Content page
    user navigates to content page    ${PUBLICATION_NAME_DATAFILES}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

    # TODO https://github.com/dfe-analytical-services/eesyscreener/issues/2
    Skip    Skipping until the "large-data-set.csv" is compatible with screener.
    user waits until data upload is completed    ${SUBJECT_NAME}-updated

Publish the release immediately
    user approves release for immediate publication

Verify newly published release is available publicly
    ${PUBLIC_RELEASE_URL}    user gets url public release will be accessible at
    user navigates to public release page    ${PUBLIC_RELEASE_URL}    ${PUBLICATION_NAME}    ${RELEASE_3_NAME}
