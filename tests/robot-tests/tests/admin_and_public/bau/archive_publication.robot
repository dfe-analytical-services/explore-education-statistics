*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/public-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${PUBLICATION_NAME_ARCHIVE}=        UI tests - archived publication %{RUN_IDENTIFIER}
${RELEASE_NAME_ARCHIVE}=            Financial year 3000-01
${SUBJECT_NAME_ARCHIVE}=            Subject for archived publication

${PUBLICATION_NAME_SUPERSEDE}=      UI tests - superseding publication %{RUN_IDENTIFIER}
${RELEASE_NAME_SUPERSEDE}=          Financial year 2000-01
${SUBJECT_NAME_SUPERSEDE}=          Subject for superseding publication


*** Test Cases ***
Create new publication to be archived and release via API
    ${PUBLICATION_ID_ARCHIVE}=    user creates test publication via api    ${PUBLICATION_NAME_ARCHIVE}
    user creates test release via api    ${PUBLICATION_ID_ARCHIVE}    FY    3000

Navigate to archive-publication release
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME_ARCHIVE}
    ...    ${RELEASE_NAME_ARCHIVE}

Import archive-publication subject to release
    user uploads subject    ${SUBJECT_NAME_ARCHIVE}    upload-file-test.csv    upload-file-test.meta.csv

Add data guidance to archive-publication subject
    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance    %{WAIT_SMALL}

    user waits until page contains element    id:dataGuidance-dataFiles
    user waits until page contains accordion section    ${SUBJECT_NAME_ARCHIVE}
    user enters text into data guidance data file content editor    ${SUBJECT_NAME_ARCHIVE}
    ...    ${SUBJECT_NAME_ARCHIVE} data guidance content

    user clicks button    Save guidance
    user waits until page contains button    Edit guidance

Go to "Sign off" page and approve archive-publication release
    user clicks link    Sign off
    user approves original release for immediate publication

Create new publication to supersede other publication and release via API
    ${PUBLICATION_ID_SUPERSEDE}=    user creates test publication via api    ${PUBLICATION_NAME_SUPERSEDE}
    user creates test release via api    ${PUBLICATION_ID_SUPERSEDE}    FY    2000

Set archive-publication to be superseded by superseding-publication
    user navigates to publication page from dashboard    ${PUBLICATION_NAME_ARCHIVE}

    user clicks link    Details
    user waits until h2 is visible    Publication details

    user clicks button    Edit publication details

    user waits until page contains element    id:publicationDetailsForm-supersede

    user chooses select option    id:publicationDetailsForm-supersededById    ${PUBLICATION_NAME_SUPERSEDE}

    user clicks button    Update publication details
    ${modal}=    user waits until modal is visible    Confirm publication changes
    user clicks button    Confirm    ${modal}
    user waits until modal is not visible    Confirm publication changes

Validate archive warning is on Admin dashboard for archive-publication release
    user navigates to publication page from dashboard    ${PUBLICATION_NAME_ARCHIVE}
    user waits until page contains
    ...    This publication will be archived when its superseding publication has a live release published.

Check cannot create a release for archive-publication
    user clicks link    Releases
    user waits until h2 is visible    Manage releases
    user checks page does not contain    link:Create new release

Check cannot create a methodology for archive-publication
    user clicks link    Methodologies
    user waits until h2 is visible    Manage methodologies
    user checks page does not contain button    Create new methodology

Validate that archive-publication appears correctly on Find stats page
    user checks publication is on find statistics page    ${PUBLICATION_NAME_ARCHIVE}
    user clicks link    ${PUBLICATION_NAME_ARCHIVE}

    user waits until h1 is visible    ${PUBLICATION_NAME_ARCHIVE}    %{WAIT_MEDIUM}
    user waits until page contains    This is the latest data
    ${PUBLICATION_ARCHIVE_URL}=    user gets url
    set suite variable    ${PUBLICATION_ARCHIVE_URL}

Check that archive-publication subject appears correctly on Data tables page
    user navigates to data tables page on public frontend

    user clicks radio    %{TEST_THEME_NAME}

    user waits until page contains    Select a publication    %{WAIT_SMALL}

    user checks page does not contain element    Radio item for UI tests - ${PUBLICATION_NAME_SUPERSEDE}

    user clicks radio    ${PUBLICATION_NAME_ARCHIVE}
    user clicks element    id:publicationForm-submit
    user waits until table tool wizard step is available    2    Choose a subject
    user checks previous table tool step contains    1    Publication    ${PUBLICATION_NAME_ARCHIVE}

    user checks page contains    ${SUBJECT_NAME_ARCHIVE}

Generate permalink for archive-publication
    user clicks element    id:publicationSubjectForm-submit

    user waits until table tool wizard step is available    3    Choose locations
    user checks previous table tool step contains    2    Subject    ${SUBJECT_NAME_ARCHIVE}
    user opens details dropdown    Regional
    user clicks checkbox    North East
    user clicks element    id:locationFiltersForm-submit

    user waits until table tool wizard step is available    4    Choose time period
    user checks previous table tool step contains    3    Regional    North East
    user chooses select option    id:timePeriodForm-start    2005
    user chooses select option    id:timePeriodForm-end    2016
    user clicks element    id:timePeriodForm-submit

    user waits until table tool wizard step is available    5    Choose your filters
    user checks previous table tool step contains    4    Time period    2005 to 2016
    user clicks element    id:filtersForm-submit

    user waits until results table appears    %{WAIT_SMALL}
    user waits until page contains button    Generate shareable link

    user clicks button    Generate shareable link
    user waits until page contains testid    permalink-generated-url    %{WAIT_MEDIUM}
    user checks generated permalink is valid

    user clicks link    View share link
    user waits until h1 is visible
    ...    '${SUBJECT_NAME_ARCHIVE}' from '${PUBLICATION_NAME_ARCHIVE}'    %{WAIT_SMALL}

    ${PERMALINK_URL}=    user gets url
    set suite variable    ${PERMALINK_URL}

    user checks page does not contain    WARNING

Check that archive-publication subject appears correctly on Data catalogue page
    user navigates to data catalogue page on public frontend

    user clicks radio    %{TEST_THEME_NAME}

    user waits until element is visible    id:publicationForm-publications    %{WAIT_SMALL}

    user checks page contains radio    ${PUBLICATION_NAME_ARCHIVE}
    user checks page does not contain element    //*[@data-testid="Radio item for ${PUBLICATION_NAME_SUPERSEDE}"]

    user clicks radio    ${PUBLICATION_NAME_ARCHIVE}
    user clicks button    Next step
    user waits until page contains    Choose a release
    user waits until page contains    ${RELEASE_NAME_ARCHIVE}

    user checks page contains radio    ${RELEASE_NAME_ARCHIVE}
    user checks page contains    This is the latest data

Navigate to superseding-publication release on Admin site
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME_SUPERSEDE}
    ...    ${RELEASE_NAME_SUPERSEDE}

Import superseding-publication subject to release
    user uploads subject    ${SUBJECT_NAME_SUPERSEDE}    upload-file-test.csv    upload-file-test.meta.csv

Add data guidance to superseding-publication subject
    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance    %{WAIT_SMALL}

    user waits until page contains element    id:dataGuidance-dataFiles
    user waits until page contains accordion section    ${SUBJECT_NAME_SUPERSEDE}
    user enters text into data guidance data file content editor    ${SUBJECT_NAME_SUPERSEDE}
    ...    ${SUBJECT_NAME_SUPERSEDE} data guidance content

    user clicks button    Save guidance
    user waits until page contains button    Edit guidance

Go to "Sign off" page and approve superseding-publication release
    user clicks link    Sign off
    user approves original release for immediate publication

Check archive-publication is now archived and superseding-publication now appears on Find stats page
    sleep    1    # Prevent intermittent failure where Find Stats page loads before cache is cleared
    user checks publication is on find statistics page    ${PUBLICATION_NAME_SUPERSEDE}
    user checks page does not contain    ${PUBLICATION_NAME_ARCHIVE}

Check public superseding-publication release page displays correctly
    user clicks link    ${PUBLICATION_NAME_SUPERSEDE}

    user waits until h1 is visible    ${PUBLICATION_NAME_SUPERSEDE}    %{WAIT_MEDIUM}
    user waits until page contains    This is the latest data

Check public archive-publication release page displays correctly
    user navigates to public frontend    ${PUBLICATION_ARCHIVE_URL}
    user waits until h1 is visible    ${PUBLICATION_NAME_ARCHIVE}    %{WAIT_MEDIUM}
    user checks page does not contain    This is the latest data

Check public archive-publication release page displays superseded warning
    user checks page contains element    testid:superseded-warning
    user checks page contains element    testid:superseded-by-link

    ${SUPERSDED_BY_LINK_TEXT}=    get text    testid:superseded-by-link
    should contain    ${SUPERSDED_BY_LINK_TEXT}    ${PUBLICATION_NAME_SUPERSEDE}

Check superseded warning link takes user to superseding-publication release page
    user clicks element    testid:superseded-by-link

    user waits until h1 is visible    ${PUBLICATION_NAME_SUPERSEDE}    %{WAIT_MEDIUM}
    user checks page contains    This is the latest data

    user checks page does not contain element    testid:superseded-warning
    user checks page does not contain element    testid:superseded-by-link

Check public data tables page contains superseding-publication's subject
    user navigates to data tables page on public frontend

    user clicks radio    %{TEST_THEME_NAME}

    user checks page does not contain    ${PUBLICATION_NAME_ARCHIVE}

    user clicks radio    ${PUBLICATION_NAME_SUPERSEDE}
    user clicks element    id:publicationForm-submit
    user waits until table tool wizard step is available    2    Choose a subject
    user checks previous table tool step contains    1    Publication    ${PUBLICATION_NAME_SUPERSEDE}

    user checks page contains    ${SUBJECT_NAME_SUPERSEDE}

Check data catalogue page contains archive and superseding publication subjects
    user navigates to data catalogue page on public frontend

    user clicks radio    %{TEST_THEME_NAME}

    user checks page contains radio    ${PUBLICATION_NAME_ARCHIVE}
    user checks page contains radio    ${PUBLICATION_NAME_SUPERSEDE}

    user clicks radio    ${PUBLICATION_NAME_ARCHIVE}
    user clicks button    Next step

    user waits until page contains    Choose a release
    user waits until page contains    ${RELEASE_NAME_ARCHIVE}

    user checks page contains radio    ${RELEASE_NAME_ARCHIVE}
    user checks page does not contain    This is the latest data

    user clicks button    Next step
    user waits until page contains    Choose files to download
    user waits until page contains    ${SUBJECT_NAME_ARCHIVE}

    user checks page does not contain    This is the latest data
    user checks page contains    This is not the latest data

    user clicks button    Change publication
    user waits for page to finish loading
    user waits until page contains    Choose a publication

    user clicks radio    %{TEST_THEME_NAME}
    user clicks radio    ${PUBLICATION_NAME_SUPERSEDE}
    user clicks button    Next step
    user waits until page contains    Choose a release
    user waits until page contains    ${RELEASE_NAME_SUPERSEDE}

    user checks page contains radio    ${RELEASE_NAME_SUPERSEDE}
    user checks page contains    This is the latest data

    user clicks button    Next step
    user waits until page contains    Choose files to download
    user waits until page contains    ${SUBJECT_NAME_SUPERSEDE}

    user checks page contains    This is the latest data
    user checks page does not contain    This is not the latest data

Check data catalogue page contains superseded warning for archived publication (step one and two)
    user navigates to data catalogue page on public frontend

    user clicks radio    %{TEST_THEME_NAME}

    user checks page contains radio    ${PUBLICATION_NAME_ARCHIVE}
    user checks page contains radio    ${PUBLICATION_NAME_SUPERSEDE}

    user clicks radio    ${PUBLICATION_NAME_ARCHIVE}
    user clicks button    Next step
    user waits for page to finish loading

    user checks summary list contains    Publication    ${PUBLICATION_NAME_ARCHIVE}

    user waits until page contains    Choose a release
    user checks page contains radio    ${RELEASE_NAME_ARCHIVE}

    user checks page does not contain    This is the latest data
    user checks page contains element    testid:superseded-warning
    user checks page contains element    testid:superseded-by-link

    # step 2
    user clicks button    Next step
    user checks page contains element    testid:superseded-warning
    user checks page contains element    testid:superseded-by-link

Check that superseded warning link takes user to superseding-publication data-catalogue step 1
    user navigates to data catalogue page on public frontend

    user clicks radio    %{TEST_THEME_NAME}

    user checks page contains radio    ${PUBLICATION_NAME_ARCHIVE}
    user checks page contains radio    ${PUBLICATION_NAME_SUPERSEDE}

    user clicks radio    ${PUBLICATION_NAME_ARCHIVE}
    user clicks button    Next step

    user checks summary list contains    Publication    ${PUBLICATION_NAME_ARCHIVE}

    user waits until page contains    Choose a release
    user checks page contains radio    ${RELEASE_NAME_ARCHIVE}

    user checks page contains element    testid:superseded-warning
    user checks page contains element    testid:superseded-by-link

    # step 2
    user clicks button    Next step

    user checks page contains element    testid:superseded-warning
    user checks page contains element    testid:superseded-by-link

    user clicks element    testid:superseded-by-link

    user waits until h1 is visible    Browse our open data

    user checks summary list contains    Publication    ${PUBLICATION_NAME_SUPERSEDE}
    user checks page contains radio    ${RELEASE_NAME_SUPERSEDE}

    user checks page does not contain element    testid:superseded-warning
    user checks page does not contain element    testid:superseded-by-link

Check archive-publication permalink has out-of-date warning
    user navigates to public frontend    ${PERMALINK_URL}

    user waits until h1 is visible
    ...    '${SUBJECT_NAME_ARCHIVE}' from '${PUBLICATION_NAME_ARCHIVE}'

    user waits until page contains
    ...    WARNING - The data used in this table may now be out-of-date as a new release has been published since its creation.

Set archive-publication to be no longer be superseded
    user navigates to publication page from dashboard    ${PUBLICATION_NAME_ARCHIVE}

    user clicks link    Details
    user waits until h2 is visible    Publication details

    user clicks button    Edit publication details
    user waits until page contains element    id:publicationDetailsForm-supersede
    user chooses select option    id:publicationDetailsForm-supersededById    None selected
    user clicks button    Update publication details

    ${modal}=    user waits until modal is visible    Confirm publication changes
    user clicks button    Confirm    ${modal}
    user waits until modal is not visible    Confirm publication changes

    sleep    %{WAIT_MEMORY_CACHE_EXPIRY}

Check can create a release for archive-publication which is no longer archived
    user clicks link    Releases
    user waits until h2 is visible    Manage releases
    user waits until page contains link    Create new release

Check can create a methodology for archive-publication which is no longer archived
    user clicks link    Methodologies
    user waits until h2 is visible    Manage methodologies
    user checks page contains button    Create new methodology

Check public Find stats page and check archive-publication is no longer archived
    user checks publication is on find statistics page    ${PUBLICATION_NAME_SUPERSEDE}
    user checks publication is on find statistics page    ${PUBLICATION_NAME_ARCHIVE}

Check public archive-publication release page displays correctly after being unarchived
    user clicks link    ${PUBLICATION_NAME_ARCHIVE}

    user waits until h1 is visible    ${PUBLICATION_NAME_ARCHIVE}    %{WAIT_MEDIUM}
    user waits until page contains    This is the latest data

Check public data tables page is correct after archive-publication has been unarchived
    user navigates to data tables page on public frontend

    user clicks radio    %{TEST_THEME_NAME}

    user checks page contains    ${PUBLICATION_NAME_ARCHIVE}

    user clicks radio    ${PUBLICATION_NAME_ARCHIVE}
    user clicks element    id:publicationForm-submit
    user waits until table tool wizard step is available    2    Choose a subject
    user checks previous table tool step contains    1    Publication    ${PUBLICATION_NAME_ARCHIVE}

    user checks page contains    ${SUBJECT_NAME_ARCHIVE}

Check data catalogue page is correct after archive-publication has been unarchived
    user navigates to data catalogue page on public frontend

    user clicks radio    %{TEST_THEME_NAME}

    user checks page contains    ${PUBLICATION_NAME_ARCHIVE}
    user checks page contains    ${PUBLICATION_NAME_SUPERSEDE}

    user clicks radio    ${PUBLICATION_NAME_ARCHIVE}
    user clicks button    Next step
    user waits until page contains    Choose a release
    user waits until page contains    ${RELEASE_NAME_ARCHIVE}

    user checks page contains radio    ${RELEASE_NAME_ARCHIVE}
    user checks page contains    This is the latest data

    user clicks button    Next step
    user waits until page contains    Choose files to download
    user waits until page contains    ${SUBJECT_NAME_ARCHIVE}

    user checks page contains    This is the latest data
    user checks page does not contain    This is not the latest data

    user clicks button    Change publication
    user waits for page to finish loading
    user waits until page contains    Choose a publication

    user clicks radio    %{TEST_THEME_NAME}
    user clicks radio    ${PUBLICATION_NAME_SUPERSEDE}
    user clicks button    Next step
    user waits until page contains    Choose a release
    user waits until page contains    ${RELEASE_NAME_SUPERSEDE}

    user checks page contains radio    ${RELEASE_NAME_SUPERSEDE}
    user checks page contains    This is the latest data

    user clicks button    Next step
    user waits until page contains    Choose files to download
    user waits until page contains    ${SUBJECT_NAME_SUPERSEDE}

    user checks page contains    This is the latest data
    user checks page does not contain    This is not the latest data

Check archive-publication permalink no longer has out-of-date warning after archive-publication has been unarchived
    user navigates to public frontend    ${PERMALINK_URL}

    user waits until h1 is visible
    ...    '${SUBJECT_NAME_ARCHIVE}' from '${PUBLICATION_NAME_ARCHIVE}'

    user checks page does not contain
    ...    WARNING - The data used in this table may now be out-of-date as a new release has been published since its creation.
