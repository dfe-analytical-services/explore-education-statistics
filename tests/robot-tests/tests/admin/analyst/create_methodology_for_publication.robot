*** Settings ***
Resource    ../../libs/admin-common.robot
Library  ../../libs/admin_api.py

Force Tags  Admin  Local  Dev  AltersData

Suite Teardown    user closes the browser

*** Variables ***
${PUBLICATION_NAME}  UI tests - create methodology by analysts publication %{RUN_IDENTIFIER}

*** Keywords ***
teardown suite
    user closes the browser

*** Test Cases ***
Create Publication for tests with BAU user
    [Tags]  HappyPath
    user signs in as bau1
    user creates test publication via api   ${PUBLICATION_NAME}
    user creates release for publication
    user creates release for publication  ${PUBLICATION_NAME}  Financial Year  3001
    user gives release access to analyst   ${PUBLICATION_NAME} - Financial Year 3001/02  Contributor

Check Release Contributor does not have permission to create Methodologies
    user signs in as analyst1
    ${accordion}=  user opens publication on the admin dashboard   ${PUBLICATION_NAME}
    user checks element does not contain button  ${accordion}  Create methodology
    user checks element does not contain button  ${accordion}  Link to an externally hosted methodology
        
Check Publication Owner has permission to create Methodologies
    user signs in as bau1
    user     
    
    user checks element contains button  ${accordion}  Create methodology
    user checks element contains button  ${accordion}  Link to an externally hosted methodology
    user creates methodology for publication    ${PUBLICATION_NAME}
    ${accordion}=  user opens publication on the admin dashboard   ${PUBLICATION_NAME}
    user checks element does not contain button  ${accordion}  Create methodology
    user checks element does not contain button  ${accordion}  Link to an externally hosted methodology
    user views methodology for open publication accordion  ${accordion}  ${PUBLICATION_NAME}
    user checks summary list contains   Title   ${PUBLICATION_NAME}
    user checks summary list contains   Status  Draft
    user checks summary list contains   Published on  Not yet published

Update Methodology for Publication
    [Tags]  HappyPath
    ${accordion}=  user opens publication on the admin dashboard   ${PUBLICATION_NAME}
    user views methodology for open publication accordion  ${accordion}  ${PUBLICATION_NAME}
    user clicks link  Edit summary
    user enters text into textfield  Enter methodology title  New methodology title
    user clicks button  Update methodology
    user waits until h2 is visible  Methodology summary
    user clicks link  Sign off
    user changes methodology status to Approved
    user waits until h2 is visible  Methodology status
    user clicks link  Summary
    user checks summary list contains   Title   New methodology title
    user checks summary list contains   Status  Approved
    user checks summary list contains   Published on  Not yet published