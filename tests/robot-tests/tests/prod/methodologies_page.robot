*** Settings ***
Resource            ../libs/public-common.robot

Force Tags          GeneralPublic    Prod

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Test Cases ***
Navigate to /methodology page
    user navigates to public methodologies page
    user waits until page contains
    ...    Browse to find out about the methodology behind specific education statistics and data and how and why they're collected and published.

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

Validate page contents
    user opens accordion section    Pupils and schools

    user checks page contains link with text and url
    ...    Pupil attendance in schools
    ...    %{PUBLIC_URL}/methodology/pupil-attendance-in-schools

    user checks page contains link with text and url
    ...    Pupil absence statistics: methodology
    ...    %{PUBLIC_URL}/methodology/pupil-absence-in-schools-in-england

Validate Related information section links exist
    user checks page contains link with text and url
    ...    Find statistics and data
    ...    %{PUBLIC_URL}/find-statistics

    user checks page contains link with text and url
    ...    Education statistics: glossary
    ...    %{PUBLIC_URL}/glossary
