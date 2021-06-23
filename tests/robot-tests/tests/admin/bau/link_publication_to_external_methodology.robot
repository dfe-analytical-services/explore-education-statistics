*** Settings ***
Resource    ../../libs/admin-common.robot
Library  ../../libs/admin_api.py

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${PUBLICATION_NAME}  UI tests - create methodology publication %{RUN_IDENTIFIER}

*** Keywords ***
teardown suite
    user closes the browser

*** Test Cases ***
Link Publication to External Methodology
    [Tags]  HappyPath
    user creates test publication via api   ${PUBLICATION_NAME}
    user links publication to external methodology    ${PUBLICATION_NAME}
    user opens publication on the admin dashboard   ${PUBLICATION_NAME}
    user waits until page contains button   Edit
    user waits until page contains button   Remove
    user checks page does not contain button    Create methodology
        
Edit the External Methodology of the Publication
    user edits an external methodology    ${PUBLICATION_NAME}
    
Remove the External Methodology from Publication
    user removes an external methodology from publication    ${PUBLICATION_NAME}
    