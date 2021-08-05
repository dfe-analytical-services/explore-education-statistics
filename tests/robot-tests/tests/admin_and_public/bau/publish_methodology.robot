*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Resource            ../../libs/public-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser

Force Tags          Admin    Local    Dev    AltersData

*** Variables ***
${PUBLICATION_NAME}=            UI tests - publish methodology %{RUN_IDENTIFIER}
${PUBLIC_METHODOLOGY_URL}=      %{PUBLIC_URL}/methodology/ui-tests-publish-methodology-%{RUN_IDENTIFIER}

*** Test Cases ***
Create a draft release
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user create test release via api    ${PUBLICATION_ID}    AY    2021

Approve a methodology for publishing immediately
    user creates approved methodology for publication    ${PUBLICATION_NAME}

Verify that the publication is not visible on the public methodologies page without a published release
    user navigates to methodologies page
    user checks page does not contain    ${PUBLICATION_NAME}

Verify that the methodology is not publicly accessible by URL without a published release
    user goes to url    ${PUBLIC_METHODOLOGY_URL}
    user waits until page contains    Page not found

Alter the approval to publish the methodology with the release
    user approves methodology for publication
    ...    publication=${PUBLICATION_NAME}
    ...    publishing_strategy=WithRelease
    ...    with_release=${PUBLICATION_NAME} - Academic Year 2021/22

Verify that the publication is still not visible on the public methodologies page without publishing the release
    user navigates to methodologies page
    user checks page does not contain    ${PUBLICATION_NAME}

Verify that the methodology is still not publicly accessible by URL without publishing the release
    user goes to url    ${PUBLIC_METHODOLOGY_URL}
    user waits until page contains    Page not found

Approve the release
    user navigates to editable release summary from admin dashboard    ${PUBLICATION_NAME}
    ...    Academic Year 2021/22 (not Live)
    user approves release for immediate publication

Verify that the methodology is visible on the public methodologies page with the expected URL
    user navigates to methodologies page
    user waits until page contains accordion section    %{TEST_THEME_NAME}
    user opens accordion section    %{TEST_THEME_NAME}
    user opens details dropdown    %{TEST_TOPIC_NAME}
    user checks page contains methodology link
    ...    %{TEST_TOPIC_NAME}
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLIC_METHODOLOGY_URL}

Verify that the methodology is publicly accessible
    user clicks methodology link
    ...    %{TEST_TOPIC_NAME}
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME}
    user waits until h1 is visible    ${PUBLICATION_NAME}
    user waits until page contains title caption    Methodology

Amend the methodology in preparation to test publishing immediately
    user creates methodology amendment for publication    ${PUBLICATION_NAME}
    user edits methodology summary for publication
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - Amended methodology
    ...    Edit this amendment

Approve the amendment for publishing immediately
    user approves methodology amendment for publication
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - Amended methodology

Verify that the amended methodology is visible on the public methodologies page immediately
    user navigates to methodologies page
    user waits until page contains accordion section    %{TEST_THEME_NAME}
    user opens accordion section    %{TEST_THEME_NAME}
    user opens details dropdown    %{TEST_TOPIC_NAME}
    user checks page contains methodology link
    ...    %{TEST_TOPIC_NAME}
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - Amended methodology
    ...    ${PUBLIC_METHODOLOGY_URL}

Verify that the amended methodology is publicly accessible immediately
    user clicks methodology link
    ...    %{TEST_TOPIC_NAME}
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - Amended methodology
    user waits until h1 is visible    ${PUBLICATION_NAME} - Amended methodology
    user waits until page contains title caption    Methodology
