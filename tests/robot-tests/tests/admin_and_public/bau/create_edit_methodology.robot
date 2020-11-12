*** Settings ***
Resource  ../../libs/admin-common.robot
Library  ../../libs/admin_api.py

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${METHODOLOGY_NAME}  new methodology-%{RUN_IDENTIFIER}


*** Test Cases ***
Navigate to manage methodoligies page
    [Tags]  HappyPath
    user goes to url  %{ADMIN_URL}/methodologies
    user waits until h1 is visible  Manage methodologies

Create new methodology
    user clicks link  Create new methodology
    user waits until h1 is visible  Create new methodology
    user enters text into element  id:createMethodologyForm-title  ${METHODOLOGY_NAME}
    user clicks button  Create methodology

Add methodology content
    user waits until h1 is visible  ${METHODOLOGY_NAME}
    user clicks link  Manage content
    user clicks button  Add new section
    user waits until page contains button  New section
    user clicks button  New section
    user scrolls down  200
    user waits until page contains button  Add text block
    user clicks button  Add text block
    user waits until page contains button  Edit block
    user clicks button  Edit block
    user scrolls down  200
    user presses keys  Adding Methodology content
    user clicks button  Save
    user clicks link  Go to top
    user scrolls down  200
    user clicks button  Edit section title
    user enters text into element  xpath=//*[@name="heading"]  ${METHODOLOGY_NAME} Title
    user clicks button  Save section title

Approve methodology
    user clicks link  Sign off
    user clicks button  Edit status
    user clicks radio  Approved for publication
    user enters text into element  xpath=//*[@name="internalReleaseNote"]  Approved by UI tests
    user clicks button  Update status

Check methodology is approved
    user waits until page contains element  xpath://strong[text()="Approved"]
    user checks page contains element       xpath://strong[text()="Approved"]

Put methodology into draft state for editing
    user clicks button  Edit status
    user clicks radio  In draft
    user clicks button  Update status

Add a new blank content section
    user waits until h1 is visible  ${METHODOLOGY_NAME}
    user clicks link  Manage content
    user clicks button  Add new section

Approve update methodology
    user clicks link  Sign off
    user clicks button  Edit status
    user clicks radio  Approved for publication
    user enters text into element  xpath=//*[@name="internalReleaseNote"]  Approved by UI tests
    user clicks button  Update status

Check updated methodology is approved
    user waits until page contains element  xpath://strong[text()="Approved"]
    user checks page contains element       xpath://strong[text()="Approved"]
