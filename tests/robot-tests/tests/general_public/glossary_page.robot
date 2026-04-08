*** Settings ***
Resource            ../libs/common.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          GeneralPublic    Local    Dev    Test    Preprod    Prod


*** Variables ***
${FEEDBACK_LINK}    sign-up for a research session using our online form.


*** Test Cases ***
Navigate to glossary page
    user navigates to public site homepage
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

Check feedback link is present
    user waits until page contains link    ${FEEDBACK_LINK}
    user checks element attribute value should be    link:${FEEDBACK_LINK}    href
    ...    https://forms.office.com/e/brCmzrQfXU
    user checks element attribute value should be    link:${FEEDBACK_LINK}    target    _blank
    user checks element attribute value should be    link:${FEEDBACK_LINK}    rel    noreferrer

Search for Voluntary repayment
    user verifies accordion is closed    V
    user enters text into element    id:pageSearchForm-input    Voluntary repayment
    user waits until element contains    id:pageSearchForm-resultsLabel    Found 1 result
    user clicks element    id:pageSearchForm-option-0
    user verifies accordion is open    V

    ${section}=    user gets accordion section content element    V

    user waits until parent contains element    ${section}    id:voluntary-repayment
    user checks element is visible    id:voluntary-repayment
    user checks element should contain    id:voluntary-repayment    Voluntary repayment
    user checks element should contain    ${section}
    ...    A borrower can at any time choose to repay some or all of their loan balance early, in addition to any repayments they are liable to make based on their income.
