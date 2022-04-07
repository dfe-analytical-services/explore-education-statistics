*** Settings *** 
Resource            ../../libs/admin-common.robot
Force Tags          Admin  Test  PreProd  Prod 

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Test Cases *** 
Check that bau users can login & authenticate succesfully
    [Documentation]    EES-3303
    user changes to bau1


Check that analyst users can login & authenticate succesfully
    [Documentation]    EES-3303
    user changes to analyst1
