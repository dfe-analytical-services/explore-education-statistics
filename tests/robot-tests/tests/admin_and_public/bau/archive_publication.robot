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
${RELEASE_NAME_ARCHIVE}=            Financial Year 3000-01
${SUBJECT_NAME_ARCHIVE}=            Subject for archived publication

${PUBLICATION_NAME_SUPERSEDE}=      UI tests - superseding publication %{RUN_IDENTIFIER}
${RELEASE_NAME_SUPERSEDE}=          Financial Year 2000-01
${SUBJECT_NAME_SUPERSEDE}=          Subject for superseding publication

*** Test Cases ***
Create new publication to be archived and release via API
    ${PUBLICATION_ID_ARCHIVE}=    user creates test publication via api    ${PUBLICATION_NAME_ARCHIVE}
    user create test release via api    ${PUBLICATION_ID_ARCHIVE}    FY    3000

Navigate to archive-publication release
    user navigates to editable release summary from admin dashboard    ${PUBLICATION_NAME_ARCHIVE}
    ...    ${RELEASE_NAME_ARCHIVE} (not Live)

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
    user create test release via api    ${PUBLICATION_ID_SUPERSEDE}    FY    2000

Set archive-publication to be superseded by superseding-publication
    user navigates to admin dashboard
    user opens accordion section    ${PUBLICATION_NAME_ARCHIVE}
    ${ARCHIVE_ACCORDION}=    user gets accordion section content element    ${PUBLICATION_NAME_ARCHIVE}
    user clicks link    Manage publication    ${ARCHIVE_ACCORDION}

    user waits until h1 is visible    Manage publication    %{WAIT_SMALL}
    user waits until page contains element    id:publicationForm-supersede

    user chooses select option    id:publicationForm-supersededById    ${PUBLICATION_NAME_SUPERSEDE}

    user clicks button    Save publication
    user waits until modal is visible    Confirm publication changes
    user clicks button    Confirm
    user waits until modal is not visible    Confirm publication changes

Validate archive warning is on Admin dashboard for archive-publication release
    ${accordion}=    user opens publication on the admin dashboard    ${PUBLICATION_NAME_ARCHIVE}
    user checks element should contain    ${accordion}
    ...    This publication will be archived when its superseding publication has a live release published.

Validate that archive-publication appears correctly on Find stats page
    user navigates to find statistics page on public frontend

    user waits until page contains accordion section    %{TEST_THEME_NAME}
    user opens accordion section    %{TEST_THEME_NAME}
    user waits until accordion section contains text    %{TEST_THEME_NAME}    %{TEST_TOPIC_NAME}

    user opens details dropdown    %{TEST_TOPIC_NAME}
    user waits until details dropdown contains publication    %{TEST_TOPIC_NAME}    ${PUBLICATION_NAME_ARCHIVE}
    user checks publication bullet contains link    ${PUBLICATION_NAME_ARCHIVE}    View statistics and data

    user clicks element    testid:View stats link for ${PUBLICATION_NAME_ARCHIVE}

    user waits until h1 is visible    ${PUBLICATION_NAME_ARCHIVE}    %{WAIT_MEDIUM}
    user waits until page contains    This is the latest data
    ${PUBLICATION_ARCHIVE_URL}=    user gets url
    set suite variable    ${PUBLICATION_ARCHIVE_URL}

Check that archive-publication subject appears correctly on Data tables page
    user navigates to data tables page on public frontend

    user opens details dropdown    %{TEST_THEME_NAME}
    user opens details dropdown    %{TEST_TOPIC_NAME}

    user checks page does not contain    ${PUBLICATION_NAME_SUPERSEDE}

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

    user opens details dropdown    %{TEST_THEME_NAME}
    user opens details dropdown    %{TEST_TOPIC_NAME}

    user checks page contains    ${PUBLICATION_NAME_ARCHIVE}
    user checks page does not contain    ${PUBLICATION_NAME_SUPERSEDE}

    user clicks radio    ${PUBLICATION_NAME_ARCHIVE}
    user clicks button    Next step
    user waits until page contains    Choose a release
    user waits until page contains    ${RELEASE_NAME_ARCHIVE}

    user checks page contains radio    ${RELEASE_NAME_ARCHIVE}
    user checks page contains    This is the latest data

Navigate to superseding-publication release on Admin site
    user navigates to editable release summary from admin dashboard    ${PUBLICATION_NAME_SUPERSEDE}
    ...    ${RELEASE_NAME_SUPERSEDE} (not Live)

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
    user navigates to find statistics page on public frontend

    user waits until page contains accordion section    %{TEST_THEME_NAME}
    user opens accordion section    %{TEST_THEME_NAME}
    user waits until accordion section contains text    %{TEST_THEME_NAME}    %{TEST_TOPIC_NAME}

    user opens details dropdown    %{TEST_TOPIC_NAME}
    user checks page does not contain    ${PUBLICATION_NAME_ARCHIVE}

    user waits until details dropdown contains publication    %{TEST_TOPIC_NAME}    ${PUBLICATION_NAME_SUPERSEDE}
    user checks publication bullet contains link    ${PUBLICATION_NAME_SUPERSEDE}    View statistics and data

Check public superseding-publication release page displays correctly
    user clicks element    testid:View stats link for ${PUBLICATION_NAME_SUPERSEDE}

    user waits until h1 is visible    ${PUBLICATION_NAME_SUPERSEDE}    %{WAIT_MEDIUM}
    user waits until page contains    This is the latest data

Check public archive-publication release page displays correctly
    user navigates to public frontend    ${PUBLICATION_ARCHIVE_URL}
    user waits until h1 is visible    ${PUBLICATION_NAME_ARCHIVE}    %{WAIT_MEDIUM}
    user checks page does not contain    This is the latest data

Check public data tables page contains superseding-publication's subject
    user navigates to data tables page on public frontend

    user opens details dropdown    %{TEST_THEME_NAME}
    user opens details dropdown    %{TEST_TOPIC_NAME}

    user checks page does not contain    ${PUBLICATION_NAME_ARCHIVE}

    user clicks radio    ${PUBLICATION_NAME_SUPERSEDE}
    user clicks element    id:publicationForm-submit
    user waits until table tool wizard step is available    2    Choose a subject
    user checks previous table tool step contains    1    Publication    ${PUBLICATION_NAME_SUPERSEDE}

    user checks page contains    ${SUBJECT_NAME_SUPERSEDE}

Check data catalogue page contains archive and superseding publication subjects
    user navigates to data catalogue page on public frontend

    user opens details dropdown    %{TEST_THEME_NAME}
    user opens details dropdown    %{TEST_TOPIC_NAME}

    user checks page contains    ${PUBLICATION_NAME_ARCHIVE}
    user checks page contains    ${PUBLICATION_NAME_SUPERSEDE}

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
    user waits until page contains    ${PUBLICATION_NAME_SUPERSEDE}

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

Check archive-publication permalink has out-of-date warning
    user navigates to public frontend    ${PERMALINK_URL}

    user waits until h1 is visible
    ...    '${SUBJECT_NAME_ARCHIVE}' from '${PUBLICATION_NAME_ARCHIVE}'

    user waits until page contains    WARNING - The data used in this permalink may be out-of-date.

Set archive-publication to be no longer be superseded
    user navigates to admin dashboard
    user opens accordion section    ${PUBLICATION_NAME_ARCHIVE}
    ${ARCHIVE_ACCORDION}=    user gets accordion section content element    ${PUBLICATION_NAME_ARCHIVE}
    user clicks link    Manage publication    ${ARCHIVE_ACCORDION}

    user waits until h1 is visible    Manage publication    %{WAIT_SMALL}
    user waits until page contains element    id:publicationForm-supersede

    user chooses select option    id:publicationForm-supersededById    None selected

    user clicks button    Save publication
    user waits until modal is visible    Confirm publication changes
    user clicks button    Confirm
    user waits until modal is not visible    Confirm publication changes

    # Otherwise gets to Find Stats page before cache is invalidated
    user waits until h1 is visible    Dashboard    %{WAIT_SMALL}

Check public Find stats page and check archive-publication is no longer archived
    user navigates to find statistics page on public frontend

    user waits until page contains accordion section    %{TEST_THEME_NAME}
    user opens accordion section    %{TEST_THEME_NAME}
    user waits until accordion section contains text    %{TEST_THEME_NAME}    %{TEST_TOPIC_NAME}

    user opens details dropdown    %{TEST_TOPIC_NAME}

    user waits until details dropdown contains publication    %{TEST_TOPIC_NAME}    ${PUBLICATION_NAME_SUPERSEDE}
    user checks publication bullet contains link    ${PUBLICATION_NAME_SUPERSEDE}    View statistics and data

    user waits until details dropdown contains publication    %{TEST_TOPIC_NAME}    ${PUBLICATION_NAME_ARCHIVE}
    user checks publication bullet contains link    ${PUBLICATION_NAME_ARCHIVE}    View statistics and data

Check public archive-publication release page displays correctly after being unarchived
    user clicks element    testid:View stats link for ${PUBLICATION_NAME_ARCHIVE}

    user waits until h1 is visible    ${PUBLICATION_NAME_ARCHIVE}    %{WAIT_MEDIUM}
    user waits until page contains    This is the latest data

Check public data tables page is correct after archive-publication has been unarchived
    user navigates to data tables page on public frontend

    user opens details dropdown    %{TEST_THEME_NAME}
    user opens details dropdown    %{TEST_TOPIC_NAME}

    user checks page contains    ${PUBLICATION_NAME_ARCHIVE}

    user clicks radio    ${PUBLICATION_NAME_ARCHIVE}
    user clicks element    id:publicationForm-submit
    user waits until table tool wizard step is available    2    Choose a subject
    user checks previous table tool step contains    1    Publication    ${PUBLICATION_NAME_ARCHIVE}

    user checks page contains    ${SUBJECT_NAME_ARCHIVE}

Check data catalogue page is correct after archive-publication has been unarchived
    user navigates to data catalogue page on public frontend

    user opens details dropdown    %{TEST_THEME_NAME}
    user opens details dropdown    %{TEST_TOPIC_NAME}

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
    user waits until page contains    ${PUBLICATION_NAME_SUPERSEDE}

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

    user checks page does not contain    WARNING
