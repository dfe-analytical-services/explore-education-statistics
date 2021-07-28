*** Settings ***
Resource            ../libs/common.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser

Force Tags          GeneralPublic    Local    Dev    Test    Preprod    Prod

*** Test Cases ***
Navigate to glossary page
    [Tags]    HappyPath
    environment variable should be set    PUBLIC_URL
    user goes to url    %{PUBLIC_URL}
    user waits until h1 is visible    Explore our statistics and data

    user clicks link    Glossary
    user waits until h1 is visible    Glossary
    user checks url contains    %{PUBLIC_URL}/glossary

Validate glossary accordion sections
    [Tags]    HappyPath
    user checks accordion is in position    A    1
    user checks accordion is in position    B    2
    user checks accordion is in position    C    3
    user checks accordion is in position    D    4
    user checks accordion is in position    Z    26

Search for Pupil referral unit
    [Tags]    HappyPath
    user verifies accordion is closed    P

    user enters text into element    id:pageSearchForm-input    Permanent exclusion
    user waits until element contains    id:pageSearchForm-resultsLabel    Found 1 result
    user clicks element    id:pageSearchForm-option-0

    user verifies accordion is open    P

    ${section}=    user gets accordion section content element    P

    user waits until parent contains element    ${section}    id:permanent-exclusion
    user checks element is visible    id:permanent-exclusion
    user checks element should contain    id:permanent-exclusion    Permanent exclusion
    user checks element should contain    ${section}
    ...    When a pupil is not allowed to attend (or is excluded from) a school and cannot go back to that specific school unless their exclusion is overturned.


