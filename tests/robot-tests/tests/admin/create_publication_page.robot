*** Settings ***
Resource    ../libs/library.robot

Force Tags  Admin

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Verify admin index page loads
    [Tags]  HappyPath
    environment variable should be set   ADMIN_URL
    user goes to url  %{ADMIN_URL}
    user waits until page contains heading     Sign-in

Verify user can sign in
    [Tags]   HappyPath
    user clicks link   Sign-in

    environment variable should be set   ADMIN_EMAIL
    environment variable should be set   ADMIN_PASSWORD
    user logs into microsoft online  %{ADMIN_EMAIL}   %{ADMIN_PASSWORD}

    user checks url contains  %{ADMIN_URL}
    user waits until page contains heading     User1 EESADMIN
    user checks element should contain    css:[data-testid="breadcrumbs--list"] li:nth-child(1)     Home
    user checks element should contain    css:[data-testid="breadcrumbs--list"] li:nth-child(2)     Administrator dashboard

Page has correct heading
    [Tags]  HappyPath
    user waits until page contains element    xpath://a[text()="Create new publication"]
    user clicks link  Create new publication
    user waits until page contains heading    Create new publication
    
Page shows dropdown of methodologies when radio button clicked
    [Tags]  HappyPath
    user waits until page contains element   xpath://label[text()="Add existing methodology"]
    user clicks element          xpath://label[text()="Add existing methodology"]
    user checks element is visible    xpath://label[text()="Select methodology"]
    
Validate Pubulication and Release dropdown is dynamic and table is populated
    [Tags]  HappyPath
    user selects from list by label  css:#createPublicationForm-selectedContactId  Simon Shakespeare
    user waits until page contains element   xpath://dd[text()="teamshakes@gmail.com"]
    
Error message appears when user clicks continue is title is empty
    [Tags]  HappyPath  UnderConstruction
    user checks element is not visible  css:#createPublicationForm-publicationTitle-error
    user clicks button   Continue
    user checks element is visible  css:#createPublicationForm-publicationTitle-error
    
User redirects to the dashboard when clicking the cancel publication link
    [Tags]  HappyPath
    user clicks link  Cancel publication
    user waits until page contains element   xpath://span[text()="Welcome"]