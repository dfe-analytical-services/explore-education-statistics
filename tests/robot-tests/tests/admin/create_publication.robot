*** Settings ***
Resource    ../libs/admin-common.robot

Force Tags  Admin  Local  Dev

Suite Setup       user signs in
Suite Teardown    user closes the browser

*** Test Cases ***
Go to Create publication page for "UI tests topic" topic
    [Tags]  HappyPath
    environment variable should be set   RUN_IDENTIFIER
    user selects theme "Test theme" and topic "UI test topic %{RUN_IDENTIFIER}" from the admin dashboard
    user waits until page contains element    xpath://a[text()="Create new publication"]     60
    user checks page does not contain element   xpath://button[text()="UI tests - create publication %{RUN_IDENTIFIER}"]
    user clicks link  Create new publication
    user waits until page contains heading    Create new publication

Select an existing methodology
    [Tags]  HappyPath
    user waits until page contains element   xpath://label[text()="Add existing methodology"]
    user clicks element          xpath://label[text()="Add existing methodology"]
    user checks element is visible    xpath://label[text()="Select methodology"]
    user selects from list by label  css:#createPublicationForm-selectedMethodologyId   API Test Methodology

Select contact "Sean Gibson"
    [Tags]  HappyPath
    user selects from list by label  css:#createPublicationForm-selectedContactId   Sean Gibson
    user checks summary list item "Email" should be "sen.statistics@education.gov.uk"
    user checks summary list item "Telephone" should be "01325340987"

Error message appears when user clicks continue is title is empty
    [Tags]  HappyPath
    user checks element is not visible  css:#createPublicationForm-publicationTitle-error
    user clicks button   Create publication
    user checks element is visible  css:#createPublicationForm-publicationTitle-error

Enter new publication title
    [Tags]  HappyPath
    user enters text into element  css:#createPublicationForm-publicationTitle   UI tests - create publication %{RUN_IDENTIFIER}
    user checks element is not visible  css:#createPublicationForm-publicationTitle-error

User redirects to the dashboard when clicking the Create publication button
    [Tags]  HappyPath
    user clicks button   Create publication
    user waits until page contains element   xpath://span[text()="Welcome"]

Verify that new publication has been created
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "UI test topic %{RUN_IDENTIFIER}" from the admin dashboard
    user waits until page contains element   xpath://button[text()="UI tests - create publication %{RUN_IDENTIFIER}"]
    user checks page contains accordion   UI tests - create publication %{RUN_IDENTIFIER}
    user opens accordion section  UI tests - create publication %{RUN_IDENTIFIER}
    user checks page contains element   xpath://div/dt[text()="Methodology"]/../dd/a[text()="API Test Methodology"]
    user checks summary list item "Releases" should be "No releases created"
