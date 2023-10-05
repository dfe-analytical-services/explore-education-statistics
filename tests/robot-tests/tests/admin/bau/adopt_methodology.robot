*** Settings ***
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Library             ../../libs/admin_api.py

Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Variables ***
${OWNING_PUBLICATION_NAME}=         UI tests - methodology owning publication %{RUN_IDENTIFIER}
${ADOPTING_PUBLICATION_NAME}=       UI tests - adopting methodology publication %{RUN_IDENTIFIER}
${OWNING_PUBLICATION_NAME_2}=       UI tests - publication with unpublished methodology %{RUN_IDENTIFIER}
${RELEASE_NAME}=                    Calendar year 2000


*** Test Cases ***
Create owning publication release and publish
    ${owning_publication_id}=    user creates test publication via api    ${OWNING_PUBLICATION_NAME}
    user creates test release via api    ${owning_publication_id}    CY    2001
    user navigates to draft release page from dashboard    ${OWNING_PUBLICATION_NAME}    Calendar year 2001
    user navigates to content page    ${OWNING_PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text
    user approves release for immediate publication

Create owning publication methodology and publish
    user creates methodology for publication    ${OWNING_PUBLICATION_NAME}
    user approves methodology for publication    ${OWNING_PUBLICATION_NAME}    ${OWNING_PUBLICATION_NAME}
    user creates methodology amendment for publication    ${OWNING_PUBLICATION_NAME}    ${OWNING_PUBLICATION_NAME}

Create another owing publication with an unpublished methodology
    [Documentation]    EES-4531
    ${owning_publication_id}=    user creates test publication via api    ${OWNING_PUBLICATION_NAME_2}
    user creates methodology for publication    ${OWNING_PUBLICATION_NAME_2}
    user checks page contains tag    Draft

Create adopting publication
    ${adopting_publication_id}=    user creates test publication via api    ${ADOPTING_PUBLICATION_NAME}
    user creates test release via api    ${adopting_publication_id}    CY    2000

    user navigates to draft release page from dashboard    ${ADOPTING_PUBLICATION_NAME}    ${RELEASE_NAME}
    user navigates to content page    ${ADOPTING_PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Check that an unpublished methodology cannot be adopted
    user navigates to methodologies on publication page    ${ADOPTING_PUBLICATION_NAME}
    user waits until page contains link    Adopt an existing methodology
    user clicks link    Adopt an existing methodology
    user waits until h2 is visible    Adopt a methodology
    user checks page does not contain    ${OWNING_PUBLICATION_NAME_2}

Adopt a published Methodology
    user navigates to methodologies on publication page    ${ADOPTING_PUBLICATION_NAME}
    user waits until page contains link    Adopt an existing methodology
    user clicks link    Adopt an existing methodology
    user waits until h2 is visible    Adopt a methodology

    user clicks radio    ${OWNING_PUBLICATION_NAME}
    user opens details dropdown    More details    css:[data-testid="Radio item for ${OWNING_PUBLICATION_NAME}"]
    ${selected_methodology_details}=    user gets details content element    More details
    ...    css:[data-testid="Radio item for ${OWNING_PUBLICATION_NAME}"]
    user checks element should contain    ${selected_methodology_details}    ${OWNING_PUBLICATION_NAME}
    # List of adoptable methodologies shows latest published version, not latest version -
    # that's why this is approved and not the draft amendment
    user checks element should contain    ${selected_methodology_details}    APPROVED
    ${methodology_published_date}=    get current datetime    %-d %B %Y
    user checks element should contain    ${selected_methodology_details}    ${methodology_published_date}
    user clicks button    Save
    user waits until h2 is visible    Manage methodologies

Validate methodology adopted
    ${row}=    user gets table row    ${OWNING_PUBLICATION_NAME}    testid:methodologies
    user checks element contains    ${row}    Adopted
    user checks element contains    ${row}    Draft
    user checks element contains    ${row}    Not yet published
    user checks element contains link    ${row}    Edit
    user checks element contains button    ${row}    Remove

Set methodology to published alongside release
    user navigates to methodology    ${ADOPTING_PUBLICATION_NAME}    ${OWNING_PUBLICATION_NAME}
    approve methodology from methodology view    WithRelease    ${ADOPTING_PUBLICATION_NAME} - ${RELEASE_NAME}

Schedule release to be published tomorrow
    user navigates to draft release page from dashboard    ${ADOPTING_PUBLICATION_NAME}    ${RELEASE_NAME}

    ${day}=    get current datetime    %-d    1
    ${month}=    get current datetime    %-m    1
    ${month_word}=    get current datetime    %B    1
    ${year}=    get current datetime    %Y    1

    user clicks link    Sign off
    user clicks button    Edit release status
    user clicks radio    Approved for publication
    user enters text into element    id:releaseStatusForm-internalReleaseNote    Approved by adopt_methodology tests
    user waits until page contains element    xpath://label[text()="On a specific date"]/../input
    user clicks radio    On a specific date
    user waits until page contains    Publish date
    user enters text into element    id:releaseStatusForm-publishScheduled-day    ${day}
    user enters text into element    id:releaseStatusForm-publishScheduled-month    ${month}
    user enters text into element    id:releaseStatusForm-publishScheduled-year    ${year}
    user clicks button    Update status
    user waits until h2 is visible    Confirm publish date
    user clicks button    Confirm

    user checks summary list contains    Current status    Approved
    user checks summary list contains    Scheduled release    ${day} ${month_word} ${year}
    user waits for release process status to be    Scheduled    %{WAIT_MEDIUM}

Invite analyst1 to prerelease
    user clicks link    Pre-release access
    user waits until h2 is visible    Manage pre-release user access
    user enters text into element    css:textarea[name="emails"]    ees-test.analyst1@education.gov.uk
    user clicks button    Invite new users
    ${modal}=    user waits until modal is visible    Confirm pre-release invitations
    user waits until element contains    ${modal}    Email notifications will be sent immediately

    user checks list has x items    testid:invitableList    1    ${modal}
    user checks list item contains    testid:invitableList    1    ees-test.analyst1@education.gov.uk    ${modal}
    user clicks button    Confirm
    user checks table column heading contains    1    1    User email
    user checks table cell contains    1    1    ees-test.analyst1@education.gov.uk

Get pre-release url
    ${PRERELEASE_URL}=    Get Value    testid:prerelease-url
    check that variable is not empty    PRERELEASE_URL    ${PRERELEASE_URL}
    Set Suite Variable    ${PRERELEASE_URL}

Validate methodology appears in prerelease for analyst1
    user changes to analyst1
    user navigates to admin frontend    ${PRERELEASE_URL}

    user clicks link    Methodologies
    user waits until h1 is visible    Methodologies
    user waits until page contains    ${OWNING_PUBLICATION_NAME} (Adopted)
    user clicks link    ${OWNING_PUBLICATION_NAME} (Adopted)

    user waits until page contains title caption    Methodology
    user waits until h1 is visible    ${OWNING_PUBLICATION_NAME}

Drop adopted Methodology
    user changes to bau1
    user navigates to methodologies on publication page    ${ADOPTING_PUBLICATION_NAME}

    ${row}=    user gets table row    ${OWNING_PUBLICATION_NAME}    testid:methodologies
    user clicks button    Remove    ${row}
    ${modal}=    user waits until modal is visible    Remove methodology
    user clicks button    Confirm    ${modal}
    user waits until modal is not visible    Remove methodology

    user waits until page contains    There are no methodologies for this publication yet.

Validate adopted methodology no longer appears in prerelease
    user changes to analyst1
    user navigates to admin frontend    ${PRERELEASE_URL}

    user clicks link    Methodologies
    user waits until h1 is visible    Methodologies
    user waits until page contains    No methodologies available.
