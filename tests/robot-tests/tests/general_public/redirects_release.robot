*** Settings ***
Resource            ../libs/public-common.robot
Resource            ../seed_data/seed_data_theme_1_constants.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          GeneralPublic    Local


*** Test Cases ***
Verify that deprecated publication data guidance and prerelease access pages redirect to release page
    environment variable should be set    PUBLIC_URL
    user navigates to    %{PUBLIC_URL}/find-statistics/${PUPIL_ABSENCE_PUBLICATION_SLUG}/data-guidance
    user waits until page contains    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user checks url equals    %{PUBLIC_URL}${PUPIL_ABSENCE_PUBLICATION_RELATIVE_URL}
    user navigates to    %{PUBLIC_URL}/find-statistics/${PUPIL_ABSENCE_PUBLICATION_SLUG}/prerelease-access-list
    user waits until page contains    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user checks url equals    %{PUBLIC_URL}${PUPIL_ABSENCE_PUBLICATION_RELATIVE_URL}

Verify that deprecated release data guidance page redirects to explore page
    environment variable should be set    PUBLIC_URL
    user navigates to    %{PUBLIC_URL}${PUPIL_ABSENCE_PUBLICATION_RELATIVE_URL}/data-guidance
    user waits until page contains    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user checks url equals    %{PUBLIC_URL}${PUPIL_ABSENCE_PUBLICATION_RELATIVE_URL}/explore#data-guidance-section

Verify that deprecated release prerelease access page redirects to help page
    environment variable should be set    PUBLIC_URL
    user navigates to    %{PUBLIC_URL}${PUPIL_ABSENCE_PUBLICATION_RELATIVE_URL}/prerelease-access-list
    user waits until page contains    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user checks url equals
    ...    %{PUBLIC_URL}${PUPIL_ABSENCE_PUBLICATION_RELATIVE_URL}/help#pre-release-access-list-section
