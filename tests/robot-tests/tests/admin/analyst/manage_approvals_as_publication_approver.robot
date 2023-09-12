*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/common.robot
Resource            ../../libs/admin/manage-content-common.robot
Resource            ../../libs/admin/analyst/role_ui_permissions.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData    Footnotes


*** Variables ***
${PUBLICATION_NAME}     UI tests - manage approvals as publication approver %{RUN_IDENTIFIER}
${RELEASE_TYPE}         Academic year 2026/27
${RELEASE_NAME}         ${PUBLICATION_NAME} - ${RELEASE_TYPE}
${SUBJECT_NAME}         UI test subject


*** Test Cases ***
Create new publication and release via API
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    Set suite variable    ${PUBLICATION_ID}
    user gives analyst publication approver access    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    AY    2026

Add headline text block to Content page
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_TYPE}
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Change release status
    user puts release into higher level review

Create methodology for publication
    user creates methodology for publication    ${PUBLICATION_NAME}
    user changes methodology status to Higher level review

Sign in as Analyst1 User1 (publication approver)
    user changes to analyst1

Check Approvals warning
    user checks page contains element    testid:outstanding-approvals-warning

Check Analyst can see correct tabs
    user checks element should contain    id:publications-tab    Your publications
    user checks element should contain    id:draft-releases-tab    Draft releases
    user checks element should contain    id:approvals-tab    Your approvals
    user checks element should contain    id:scheduled-releases-tab    Approved scheduled releases

Validate if Your approvals tab is correct
    user clicks link    approvals
    user waits until h2 is visible    Your approvals
    user waits until page contains    Here you can view any releases or methodologies awaiting approval.
    user checks table column heading contains    1    1    Publication / Page    testid:your-approvals
    user checks table column heading contains    1    2    Page type    testid:your-approvals
    user checks table column heading contains    1    3    Actions    testid:your-approvals

    # Check for release and methodology
    user checks page contains element    testid:release-${RELEASE_NAME}
    # Methodology title is inherited from publication
    user checks page contains element    testid:methodology-${PUBLICATION_NAME} - ${PUBLICATION_NAME}

Check that release link takes user to the correct release
    ${RELEASE_ROW}=    get webelement    testid:release-${RELEASE_NAME}
    user clicks link by visible text    Review this page    ${RELEASE_ROW}

    user waits until h1 is visible    ${PUBLICATION_NAME}    %{WAIT_MEDIUM}
    user waits until page contains title caption    Edit release for Academic year 2026/27
    user checks page contains tag    In Review

Check that Your approvals tab methodology link takes user to the correct methodology
    user navigates to admin dashboard

    user clicks link    approvals
    user waits until h2 is visible    Your approvals

    ${METHODOLOGY_ROW}=    get webelement    testid:methodology-${PUBLICATION_NAME} - ${PUBLICATION_NAME}
    user clicks link by visible text    Review this page    ${METHODOLOGY_ROW}

    user waits until h1 is visible    ${PUBLICATION_NAME}
    user waits until page contains title caption    Edit methodology
    user checks page contains tag    In Review
