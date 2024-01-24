*** Settings ***
Resource            ../libs/public-common.robot
Resource            ../seed_data/seed_data_theme_1_constants.robot

Force Tags          GeneralPublic    Local    Dev    PreProd

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Test Cases ***
Navigate to /methodology page
    user navigates to public methodologies page
    user waits until page contains
    ...    Browse to find out about the methodology behind specific education statistics and data and how and why they're collected and published.

Validate accordion sections exist
    user checks url equals    %{PUBLIC_URL}/methodology
    user waits until page contains accordion section    ${PUPILS_AND_SCHOOLS_THEME_TITLE}

Validate page contents
    user opens accordion section    ${PUPILS_AND_SCHOOLS_THEME_TITLE}

    user checks page contains link with text and url
    ...    ${EXCLUSIONS_METHODOLOGY_TITLE}
    ...    ${EXCLUSIONS_METHODOLOGY_RELATIVE_URL}

    user checks page contains link with text and url
    ...    ${PUPIL_ABSENCE_METHODOLOGY_TITLE}
    ...    ${PUPIL_ABSENCE_METHODOLOGY_RELATIVE_URL}

Validate Related information section links exist
    user checks page contains link with text and url
    ...    Find statistics and data
    ...    /find-statistics

    user checks page contains link with text and url
    ...    Education statistics: glossary
    ...    /glossary
