*** Settings ***
Resource            ../../libs/common.robot

Force Tags          Prod    Preprod

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Test Cases ***
Navigate to Absence publication
    user navigates to    %{PUBLIC_URL}/find-statistics/further-education-and-skills/2019-20
    user waits until h1 is visible    Further education and skills    %{WAIT_MEDIUM}
    user checks page contains    This is not the latest data

Open Overall absence accordion
    user opens accordion section    About these statistics      id:content-1
    user scrolls to accordion section    About these statistics    id:content-1

Follow Explore data link
    user clicks link containing text    Explore data
    user clicks link containing text    View or create your own tables

Navigate to the Table Tool
    user waits until h1 is visible    Create your own tables    %{WAIT_SMALL}
    user clicks radio    View all featured tables
    user clicks link containing text    Adult education and training participation and achievement by ethnicity group, 2014/15 to 2019/20

Visit featured table and generate a permalink
    user waits until page contains    This data is not from the latest release
    user waits until page contains button    Generate shareable link
    user clicks button    Generate shareable link
    user waits until page contains testid    permalink-generated-url
    user clicks button    Copy link

Open shareable link and check outdated warning
    user clicks link    View share link
    user waits until page contains
    ...    WARNING - The data used in this table may now be out-of-date as a new release has been published since its creation.

Validate that View latest data link takes user to the latest release page
    user goes back
    user waits until page contains    This data is not from the latest release
    user clicks link containing text    View latest data: Academic year 2024/25
    user waits until h1 is visible    Further education and skills
    user checks page contains   Academic year 2024/25
    user checks page contains    This is the latest data

 Validate that Publication link in Related information takes user to the latest release page
    user goes back
    user waits until page contains link    Further education and skills, Academic year 2024/25
    user clicks link containing text    Further education and skills, Academic year 2024/25
    user waits until h1 is visible    Further education and skills
    user checks page contains   Academic year 2024/25
    user checks page contains    This is the latest data
