*** Settings ***
Resource            ../libs/public-common.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          GeneralPublic    Local    Dev    Test    Preprod    Prod


*** Test Cases ***
Validate service name
    user navigates to public site homepage
    user waits until page contains element    css:.govuk-header__service-name    %{WAIT_MEDIUM}
    user checks element should contain    css:.govuk-header__service-name    Explore education statistics

    ${title}=    get title
    should contain    ${title}    Explore education statistics - GOV.UK

    user checks meta description should be
    ...    Find, download and explore official Department for Education (DfE) statistics and data in England.

Validate footer organisation
    user checks page contains link with text and url
    ...    Department for Education
    ...    https://www.gov.uk/government/organisations/department-for-education
    ...    css:footer

Validate homepage contact details
    user waits until h2 is visible    Contact us for help and support
    user checks page contains
    ...    The explore education statistics service is operated by the Department for Education (DfE).
    user checks page contains    For any further queries, contact the explore education statistics team:
    user checks page contains link with text and url    explore.statistics@education.gov.uk
    ...    mailto:explore.statistics@education.gov.uk

    user checks page contains    Have a general question about education or the Department for Education?
    user checks page contains    Call the DfE public enquiries line:
    user checks page contains    Phone: 0370 000 2288
    user checks page contains    Monday to Friday from 9.30am to 5.30pm (closed on bank holidays)

Validate related services section
    user waits until h2 is visible    Related services
    user checks page contains
    ...    Use these services to find related information and other statistical services provided by the Department for Education (DfE)
    user checks page contains link with text and url    Statistics at DfE
    ...    https://www.gov.uk/government/organisations/department-for-education/about/statistics
    user checks page contains link with text and url    Find and compare school and college profiles service
    ...    https://find-compare-schools-colleges.service.gov.uk/
    user checks page contains link with text and url    Get information about schools
    ...    https://www.get-information-schools.service.gov.uk/
    user checks page contains link with text and url    Financial Benchmarking and Insights Tool
    ...    https://financial-benchmarking-and-insights-tool.education.gov.uk/

Validate contact page contact details
    user navigates to    %{PUBLIC_URL}/contact-us
    user waits until h1 is visible    Contact explore education statistics    %{WAIT_MEDIUM}

    user checks page contains    For any further queries, contact the explore education statistics team:
    user checks page contains link with text and url    explore.statistics@education.gov.uk
    ...    mailto:explore.statistics@education.gov.uk

    user waits until h2 is visible    Have a general enquiry?
    user checks page contains    Have a general question about education or the Department for Education?
    user checks page contains    Call the DfE public enquiries line:
    user checks page contains    Phone: 0370 000 2288
    user checks page contains    Monday to Friday from 9.30am to 5.30pm (closed on bank holidays)

    user waits until h3 is visible    DfE Head of Profession for Statistics
    user checks page contains link with text and url    hop.statistics@education.gov.uk
    ...    mailto:hop.statistics@education.gov.uk

    user checks page contains link    freedom of information (FOI) request
