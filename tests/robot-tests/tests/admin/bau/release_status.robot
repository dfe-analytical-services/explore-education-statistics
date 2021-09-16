*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser

Force Tags          Admin    Local    Dev    AltersData

*** Variables ***
${PUBLICATION_NAME}             UI tests - release status %{RUN_IDENTIFIER}
${ADOPTED_PUBLICATION_NAME}     UI tests - release status publication with adoptable methodology %{RUN_IDENTIFIER}

*** Test Cases ***
Create new publication and release via API
    ${PUBLICATION_ID}    user creates test publication via api    ${PUBLICATION_NAME}
    user create test release via api    ${PUBLICATION_ID}    FY    3000

Go to release sign off page and verify initial release checklist
    user navigates to this release
    user edits release status

    # EES-1807 May be re-instated as error pending further decisions on release types
#    user waits until page contains testid    releaseChecklist-errors
#    user checks checklist errors contains    1 issue that must be resolved before this release can be published
#    user checks checklist errors contains link    Public pre-release access list is required

    user checks checklist warnings contains
    ...    4 things you may have forgotten, but do not need to resolve to publish this release.
    user checks checklist warnings contains link    An in-EES methodology page has not been linked to this publication
    user checks checklist warnings contains link    No next expected release date has been added
    user checks checklist warnings contains link    No data files uploaded
    user checks checklist warnings contains link    A public pre-release access list has not been created

    user checks page does not contain testid    releaseChecklist-errors
    user checks page does not contain testid    releaseChecklist-success

Submit release for Higher Review
    user clicks radio    Ready for higher review
    user enters text into element    id:releaseStatusForm-latestInternalReleaseNote    Submitted for Higher Review
    user enters text into element    id:releaseStatusForm-nextReleaseDate-month    12
    user enters text into element    id:releaseStatusForm-nextReleaseDate-year    3001
    user clicks button    Update status

Verify release status is Higher Review
    user checks summary list contains    Current status    Awaiting higher review
    user checks summary list contains    Scheduled release    Not scheduled
    user checks summary list contains    Next release expected    December 3001

Verify release checklist has not been updated by status
    user edits release status

    # EES-1807 May be re-instated as error pending further decisions on release types
#    user waits until page contains testid    releaseChecklist-errors
#    user checks checklist errors contains    1 issue that must be resolved before this release can be published
#    user checks checklist errors contains link    Public pre-release access list is required

    user checks checklist warnings contains
    ...    3 things you may have forgotten, but do not need to resolve to publish this release.
    user checks checklist warnings contains link    An in-EES methodology page has not been linked to this publication
    user checks checklist warnings contains link    No data files uploaded
    # EES-1807 May be re-instated as error pending further decisions on release types
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
    user enters text into element    id:releaseStatusForm-latestInternalReleaseNote    Approved for release

    user clicks radio    On a specific date
    user enters text into element    id:releaseStatusForm-publishScheduled-day    1
    user enters text into element    id:releaseStatusForm-publishScheduled-month    12
    user enters text into element    id:releaseStatusForm-publishScheduled-year    3000

    user enters text into element    id:releaseStatusForm-nextReleaseDate-month    3
    user enters text into element    id:releaseStatusForm-nextReleaseDate-year    3002

    user clicks button    Update status
    user waits until h1 is visible    Confirm publish date
    user clicks button    Confirm

Verify release status is Approved
    user checks summary list contains    Current status    Approved
    user checks summary list contains    Scheduled release    1 December 3000
    user checks summary list contains    Next release expected    March 3002
    user waits for release process status to be    Scheduled    %{WAIT_LONG}

Move release status back to Draft
    user edits release status
    user clicks radio    In draft

    user enters text into element    id:releaseStatusForm-latestInternalReleaseNote    Moved back to draft

    user enters text into element    id:releaseStatusForm-nextReleaseDate-month    1
    user enters text into element    id:releaseStatusForm-nextReleaseDate-year    3001

    user clicks button    Update status

Verify release status is Draft
    user checks summary list contains    Current status    Draft
    user checks summary list contains    Scheduled release    Not scheduled
    user checks summary list contains    Next release expected    January 3001

Check that having a Draft owned Methodology attached to this Release's Publication will show a checklist warning
    user creates methodology for publication    ${PUBLICATION_NAME}
    user navigates to this release
    user edits release status
    user checks checklist warnings contains
    ...    2 things you may have forgotten, but do not need to resolve to publish this release.
    user checks checklist warnings contains link    A methodology for this publication is not yet approved

Approve the owned methodology and verify the warning disappears
    user approves methodology for publication    ${PUBLICATION_NAME}
    user navigates to this release
    user edits release status
    user checks checklist warnings contains
    ...    1 thing you may have forgotten, but do not need to resolve to publish this release.
    user checks checklist warnings does not contain link    A methodology for this publication is not yet approved

Adopt a Draft methodology
    user creates test publication via api    ${ADOPTED_PUBLICATION_NAME}
    user creates methodology for publication    ${ADOPTED_PUBLICATION_NAME}
    ${accordion}    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    user checks element contains link    ${accordion}    Adopt a methodology
    user clicks link    Adopt a methodology
    user waits until page contains title    Adopt a methodology
    user clicks radio    ${ADOPTED_PUBLICATION_NAME}
    user clicks button    Save

Check that having a Draft methodology adopted by this Release's Publication will show a checklist warning
    user navigates to this release
    user edits release status
    user checks checklist warnings contains
    ...    2 things you may have forgotten, but do not need to resolve to publish this release.
    user checks checklist warnings contains link    A methodology for this publication is not yet approved

Approve the adopted methodology and verify the warning disappears
    user approves methodology for publication    ${ADOPTED_PUBLICATION_NAME}
    user navigates to this release
    user edits release status
    user checks checklist warnings contains
    ...    1 thing you may have forgotten, but do not need to resolve to publish this release.
    user checks checklist warnings does not contain link    A methodology for this publication is not yet approved

*** Keywords ***
user navigates to this release
    user navigates to editable release summary from admin dashboard    ${PUBLICATION_NAME}
    ...    Financial Year 3000-01 (not Live)

user navigates to sign off page
    user clicks link    Sign off
    user waits until h2 is visible    Sign off    60

user edits release status
    user navigates to sign off page
    user clicks button    Edit release status
    user waits until h2 is visible    Edit release status    60

user checks checklist errors contains
    [Arguments]    ${text}
    user waits until element contains    testid:releaseChecklist-errors    ${text}

user checks checklist errors contains link
    [Arguments]    ${text}
    user waits until page contains testid    releaseChecklist-errors
    user waits until parent contains element    testid:releaseChecklist-errors    link:${text}

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
