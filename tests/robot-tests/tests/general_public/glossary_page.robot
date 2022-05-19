*** Settings ***
Resource            ../libs/common.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          GeneralPublic    Local    Dev    Test    Preprod    Prod


*** Test Cases ***
Navigate to glossary page
    user navigates to public frontend
    user waits until h1 is visible    Explore our statistics and data

    user clicks link    Glossary
    user waits until h1 is visible    Glossary
    user waits until page contains element    id:glossary    # Glossary accordion
    user checks url contains    %{PUBLIC_URL}/glossary

Validate glossary accordion sections
    user checks accordion is in position    A    1
    user checks accordion is in position    B    2
    user checks accordion is in position    C    3
    user checks accordion is in position    D    4
    user checks accordion is in position    Z    26

Search for Pupil referral unit
    user verifies accordion is closed    P

    user enters text into element    id:pageSearchForm-input    Pupil referral unit
    user waits until element contains    id:pageSearchForm-resultsLabel    Found 1 result
    user clicks element    id:pageSearchForm-option-0

    user verifies accordion is open    P

    ${section}=    user gets accordion section content element    P

    user waits until parent contains element    ${section}    id:pupil-referral-unit
    user checks element is visible    id:pupil-referral-unit
    user checks element should contain    id:pupil-referral-unit    Pupil referral unit (PRUs)
    user checks element should contain    ${section}
    ...    An alternative education provision specifically organised to provide education for children who are not able to attend school and may not otherwise receive a suitable education.
