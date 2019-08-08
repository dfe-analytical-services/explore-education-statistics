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
    user waits until page contains element    xpath://h1[text()="Sign-in"]

Verify user can sign in
    [Tags]   HappyPath
    user clicks link   Sign-in

    user waits until page contains element  xpath://div[text()="Sign in"]
    environment variable should be set   ADMIN_EMAIL
    user presses keys     %{ADMIN_EMAIL}
    user waits until page contains element    css:input[value="Next"]
    user clicks element   css:input[value="Next"]

    user waits until page contains element  xpath://div[text()="Enter password"]
    environment variable should be set   ADMIN_PASSWORD
    user presses keys     %{ADMIN_PASSWORD}
    user waits until page contains element    css:input[value="Sign in"]
    user clicks element   css:input[value="Sign in"]

    user waits until page contains element  xpath://div[text()="Stay signed in?"]
    user waits until page contains element    css:input[value="No"]
    user clicks element   css:input[value="No"]

    user checks url contains  %{ADMIN_URL}
    user checks page contains element  xpath://h1[text()="User1 EESADMIN"]
    user checks element should contain    css:[data-testid="breadcrumbs--list"] li:nth-child(1)     Home
    user checks element should contain    css:[data-testid="breadcrumbs--list"] li:nth-child(2)     Administrator dashboard


Page has correct heading
    [Tags]  HappyPath
    user clicks link  Create new publication
    user waits until page contains element   xpath://h1[text()="Create new publication"]
    
Page shows dropdown of methodologies when radio button clicked
    [Tags]  HappyPath
    user clicks element          xpath://label[text()="Add existing methodology"]
    element should be visible    xpath://label[text()="Select methodology"]
    

Validate Pubulication and Release dropdown is dynamic and table is populated
    [Tags]  HappyPath
    select from list by label  css:#createPublicationForm-selectedContactId  Simon Shakespeare
    user waits until page contains element   xpath://dd[text()="teamshakes@gmail.com"]
    
Error message appears when user clicks continue is title is empty
    [Tags]  HappyPath  UnderConstruction
    element should not be visible  css:#createPublicationForm-publicationTitle-error
    user clicks button   Continue
    element should  be visible  css:#createPublicationForm-publicationTitle-error
    
User redirects to the dashboard when clicking the cancel publication link
    [Tags]  HappyPath
    user clicks link  Cancel publication
    user waits until page contains element   xpath://span[text()="Welcome"]