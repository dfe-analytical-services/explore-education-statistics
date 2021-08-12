*** Settings ***
Resource            ../libs/public-common.robot

Force Tags          GeneralPublic    Local    Dev    Test    Preprod

Suite Setup         user opens the browser
Suite Teardown      user closes the browser

*** Test Cases ***
Navigate to /methodology page
    [Tags]    HappyPath
    user navigates to public methodologies page

Validate page contents
    [Tags]    HappyPath
    user waits until page contains accordion section    Pupils and schools
    user opens accordion section    Pupils and schools
    user opens details dropdown    Exclusions
    user checks page contains methodology link
    ...    Exclusions
    ...    Permanent and fixed-period exclusions in England
    ...    Pupil exclusion statistics: methodology
    ...    %{PUBLIC_URL}/methodology/permanent-and-fixed-period-exclusions-in-england
    user opens details dropdown    Pupil absence
    user checks page contains methodology link
    ...    Pupil absence
    ...    Pupil absence in schools in England
    ...    Pupil absence statistics: methodology
    ...    %{PUBLIC_URL}/methodology/pupil-absence-in-schools-in-england

Validate Related information section links exist
    [Tags]    HappyPath
    user checks page contains element    xpath://a[text()="Find statistics and data"]
    user checks page contains element    xpath://a[text()="Education statistics: glossary"]
