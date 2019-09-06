*** Settings ***
Resource    ../libs/library.robot

Force Tags  Admin  NotAgainstProd

Suite Setup       user signs in
Suite Teardown    user closes the browser

*** Test Cases ***
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
    user selects from list by label  css:#createPublicationForm-selectedContactId  Sean Gibson
    user waits until page contains element   xpath://dd[text()="sen.statistics@education.gov.uk"]
    user waits until page contains element   xpath://dd[text()="01325340987"]

Error message appears when user clicks continue is title is empty
    [Tags]  HappyPath
    user checks element is not visible  css:#createPublicationForm-publicationTitle-error
    user clicks button   Create publication
    user checks element is visible  css:#createPublicationForm-publicationTitle-error
    
User redirects to the dashboard when clicking the cancel publication link
    [Tags]  HappyPath
    user clicks link  Cancel publication
    user waits until page contains element   xpath://span[text()="Welcome"]