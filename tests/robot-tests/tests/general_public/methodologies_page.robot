*** Settings ***
Resource            ../libs/public-common.robot
Force Tags          GeneralPublic    Prod

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Test Cases ***
Navigate to /methodology page
    [Tags]    Local    Dev    PreProd
    user navigates to public methodologies page
    user waits until page contains
    ...    Browse to find out about the methodology behind specific education statistics and data and how and why they're collected and published.
    ...    10

Validate page contents
    [Tags]    Local    NotAgainstProd
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

Validate accordion sections exist
    user checks url contains    methodology
    user waits until page contains accordion section    Children's social care
    user waits until page contains accordion section    COVID-19
    user waits until page contains accordion section    Destination of pupils and students
    user waits until page contains accordion section    Early years
    user waits until page contains accordion section    Finance and funding
    user waits until page contains accordion section    Further education
    user waits until page contains accordion section    Higher education
    user waits until page contains accordion section    Pupils and schools
    user waits until page contains accordion section    School and college outcomes and performance
    user waits until page contains accordion section    Teachers and school workforce
    user waits until page contains accordion section    UK education and training statistics

Validate Related information section links exist
    [Tags]    Local    Dev    PreProd
    user checks page contains element    xpath://a[text()="Find statistics and data"]
    user checks page contains element    xpath://a[text()="Education statistics: glossary"]
