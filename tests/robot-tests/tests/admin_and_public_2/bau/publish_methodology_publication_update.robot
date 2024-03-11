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
${PUBLICATION_NAME}=                            UI tests-publish publication update %{RUN_IDENTIFIER}
${PUBLIC_METHODOLOGY_URL_ENDING}=               /methodology/ui-tests-publish-publication-update-%{RUN_IDENTIFIER}
${PUBLIC_METHODOLOGY_UPDATED_URL_ENDING}=       /methodology/methodology-update
${PUBLICATION_NAME_UPDATED}=                    ${PUBLICATION_NAME} updated
${PUBLIC_PUBLICATION_URL_ENDING}=               /find-statistics/ui-tests-publish-publication-update-%{RUN_IDENTIFIER}
${EXPECTED_PUBLIC_PUBLICATION_URL_ENDING}=      https://dev.explore-education-statistics.service.gov.uk${PUBLIC_PUBLICATION_URL_ENDING}
${RELEASE_NAME}=                                Academic year Q1
${ACADEMIC_YEAR}=                               /2046-47


*** Test Cases ***
Create a draft release via api
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    AY    2046

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

Check there is no methodology status history table on Sign off page
    user clicks link    Sign off
    user waits until h2 is visible    Sign off
    user checks page does not contain element    testid:methodology-status-history

Approve the methodology for publishing immediately
    user approves methodology for publication    ${PUBLICATION_NAME}

Navigate to release and add headline text block
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    Academic year 2046/47
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Publish release
    user approves release for immediate publication

Update publication details
    user navigates to details on publication page    ${PUBLICATION_NAME}
    user clicks button    Edit publication details
    user waits until page contains element    label:Publication title
    user enters text into element    label:Publication title    ${PUBLICATION_NAME_UPDATED}
    user clicks button    Update publication details
    ${modal}=    user waits until modal is visible    Confirm publication changes
    user checks element contains valid URL    id:before-url    ${EXPECTED_PUBLIC_PUBLICATION_URL_ENDING}
    user checks element contains valid url    id:after-url    ${EXPECTED_PUBLIC_PUBLICATION_URL_ENDING}-updated
    user clicks button    Confirm    ${modal}
    user waits until modal is not visible    Confirm publication changes
    user checks summary list contains    Publication title    ${PUBLICATION_NAME_UPDATED}
    user checks summary list contains    Publication summary    ${PUBLICATION_NAME} summary
    user checks summary list contains    Theme    %{TEST_THEME_NAME}
    user checks summary list contains    Topic    %{TEST_TOPIC_NAME}
    user checks summary list contains    Superseding publication    This publication is not archived

Update publication contact
    user clicks link    Contact
    user waits until h2 is visible    Contact for this publication

    user clicks button    Edit contact details
    user waits until page contains element    label:Team name

    user enters text into element    label:Team name    Team name updated
    user enters text into element    label:Team email    email_updated@test.com
    user enters text into element    label:Contact name    Contact name updated
    user enters text into element    label:Contact telephone (optional)    04321 4321

    user clicks button    Update contact details
    ${modal}=    user waits until modal is visible    Confirm contact changes
    user clicks button    Confirm    ${modal}
    user waits until modal is not visible    Confirm contact changes

    user checks summary list contains    Team name    Team name updated
    user checks summary list contains    Team email    email_updated@test.com
    user checks summary list contains    Contact name    Contact name updated
    user checks summary list contains    Contact telephone    04321 4321

Check publication is updated on dashboard
    user navigates to admin dashboard if needed    %{ADMIN_URL}
    user waits until h1 is visible    Dashboard
    user selects dashboard theme and topic if possible

    user waits until page contains link    ${PUBLICATION_NAME_UPDATED}
    Sleep    10s

Validate publication redirect works
    user navigates to public frontend    %{PUBLIC_URL}${PUBLIC_PUBLICATION_URL_ENDING}${ACADEMIC_YEAR}
    user waits until h1 is visible    ${PUBLICATION_NAME_UPDATED}
    user checks url contains    %{PUBLIC_URL}${PUBLIC_PUBLICATION_URL_ENDING}-updated${ACADEMIC_YEAR}

Go to public release page
    user checks publication is on find statistics page    ${PUBLICATION_NAME_UPDATED}
    user clicks link    ${PUBLICATION_NAME_UPDATED}
    user waits until h1 is visible    ${PUBLICATION_NAME_UPDATED}    %{WAIT_MEDIUM}

Validate publication details are updated on public page
    user checks page contains    Team name updated
    user checks page contains    email_updated@test.com
    user checks page contains    Contact name updated
    user checks page contains    04321 4321

Navigate to methodology page and click on amendment button
    user navigates to methodologies on publication page    ${PUBLICATION_NAME_UPDATED}
    user waits until page contains element    xpath://h2[contains(text(),'Manage methodologies')]
    user clicks button    Amend
    ${modal}=    user waits until modal is visible    Confirm you want to amend this published methodology
    user clicks button    Confirm    ${modal}
    user waits until modal is not visible    Confirm you want to amend this published methodology

Update methodology details
    User Clicks link    Edit summary
    user waits until page contains element    xpath://h2[contains(text(),'Edit methodology summary')]
    User Clicks Radio    Set an alternative title
    User Enters Text Into Element    id:updateMethodologyForm-title    methodology update
    User Clicks Button    Update methodology

Naviagate to sign-off page and approve the methodology immediately
    user clicks link    Sign off
    User Clicks Button    Edit status
    User Clicks Radio    Approved for publication
    User Enters Text Into Element    id:methodologyStatusForm-latestInternalReleaseNote    Internal note
    User Clicks Radio    Immediately
    user clicks button    Update status
    User Waits Until Page Contains    Approved

Validate methodology redirect doesn't work for initial publication
    User Navigates To Public Frontend    %{PUBLIC_URL}${PUBLIC_METHODOLOGY_URL_ENDING}
    user waits until page contains title    Page not found
    Sleep    100s

Validate methodology re-directs works for the updated publication
    User Navigates To Public Frontend    %{PUBLIC_URL}${PUBLIC_METHODOLOGY_URL_ENDING}-updated
    user waits until h1 is visible    methodology update
    User checks url contains    %{PUBLIC_URL}${PUBLIC_METHODOLOGY_UPDATED_URL_ENDING}
    Sleep    100s

User creates a new release with different academic year
    user navigates to publication page from dashboard    ${PUBLICATION_NAME_UPDATED}
    user creates release from publication page    ${PUBLICATION_NAME_UPDATED}    ${RELEASE_NAME}    2050

Add headline text block in the content page
    user navigates to content page    ${PUBLICATION_NAME_UPDATED}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Approve first release
    user clicks link    Sign off
    user approves release for immediate publication

User creates second release
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user creates release from publication page    ${PUBLICATION_NAME}    ${RELEASE_NAME}    2051

Add headline text block to Content page (second release)
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Approve second release
    user clicks link    Sign off
    user approves release for immediate publication
