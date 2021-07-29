*** Comments ***
# TODO SOW4 EES-2205 - find other tests that are manually doing steps that are already laid out in this common
# file - look for usages of "Add new section". "Edit section title" etc.

*** Settings ***
Resource    ../common.robot
Library     ../admin-utilities.py

*** Keywords ***
user creates new content section
    [Arguments]
    ...    ${section_number}
    ...    ${content_section_name}
    ...    ${parent}=css:body
    user clicks element    xpath://button[.="Add new section"]    ${parent}
    user changes accordion section title    ${section_number}    ${content_section_name}    ${parent}

user opens nth editable accordion section
    [Arguments]    ${section_num}    ${parent}=css:body
    user waits until parent contains element    ${parent}
    ...    xpath:.//*[@data-testid="editableAccordionSection"][${section_num}]
    ${section}=    get child element    ${parent}
    ...    xpath:.//*[@data-testid="editableAccordionSection"][${section_num}]
    ${header_button}=    get child element    ${section}    css:h2 > button[aria-expanded]
    ${is_expanded}=    get element attribute    ${header_button}    aria-expanded
    IF    '${is_expanded}' != 'true'
        user clicks element    ${header_button}
    END
    user checks element attribute value should be    ${header_button}    aria-expanded    true

user changes accordion section title
    [Arguments]    ${section_num}    ${title}    ${parent}=id:releaseMainContent
    user opens nth editable accordion section    ${section_num}    ${parent}
    ${section}=    get child element    ${parent}
    ...    xpath:.//*[@data-testid="editableAccordionSection"][${section_num}]
    user clicks button    Edit section title    ${section}
    user waits until parent contains element    ${section}    css:input[name="heading"]
    ${input}=    get child element    ${section}    css:input[name="heading"]
    user enters text into element    ${input}    ${title}
    user clicks button    Save section title    ${section}
    user waits until parent contains element    ${section}    xpath:.//h2/button[@aria-expanded and text()="${title}"]

user checks accordion section contains x blocks
    [Arguments]    ${section_name}    ${num_blocks}    ${parent}=css:[data-testid="accordion"]
    ${section}=    user gets accordion section content element    ${section_name}    ${parent}
    ${blocks}=    get child elements    ${section}    css:[data-testid="editableSectionBlock"]
    length should be    ${blocks}    ${num_blocks}

user adds text block to editable accordion section
    [Arguments]    ${section_name}    ${parent}=css:[data-testid="accordion"]
    ${section}=    user gets accordion section content element    ${section_name}    ${parent}
    user clicks button    Add text block    ${section}
    user waits until element contains    ${section}    This section is empty

user adds data block to editable accordion section
    [Arguments]    ${section_name}    ${block_name}    ${parent}=css:[data-testid="accordion"]
    ${section}=    user gets accordion section content element    ${section_name}    ${parent}
    user waits for page to finish loading
    user clicks button    Add data block    ${section}
    ${block_list}=    get child element    ${section}    css:select[name="selectedDataBlock"]
    user chooses select option    ${block_list}    Dates data block name
    user waits until parent contains element    ${section}    css:table
    user clicks button    Embed    ${section}

user edits accordion section text block
    [Arguments]    ${section_name}    ${block_num}    ${parent}=[data-testid="accordion"]
    ${section}=    user gets accordion section content element    ${section_name}    ${parent}
    ${block}=    get child element    ${section}    css:[data-testid="editableSectionBlock"]:nth-of-type(${block_num})
    user clicks button    Edit block    ${block}
    [Return]    ${block}

user adds content to accordion section text block
    [Arguments]    ${section_name}    ${block_num}    ${content}    ${parent}=[data-testid="accordion"]
    ${block}=    user edits accordion section text block    ${section_name}    ${block_num}    ${parent}
    user presses keys    CTRL+a
    user presses keys    BACKSPACE
    user presses keys    ${content}
    user clicks button    Save    ${block}
    user waits until element contains    ${block}    ${content}

user adds image to accordion section text block
    [Arguments]
    ...    ${section_name}
    ...    ${block_num}
    ...    ${filename}=test-infographic.png
    ...    ${alt_text}=Alt text for ${filename}
    ...    ${parent}=[data-testid="accordion"]

    ${block}=    user edits accordion section text block    ${section_name}    ${block_num}    ${parent}

    choose file
    ...    xpath://button[span[.="Insert image"]]/following-sibling::input[@type="file"]
    ...    ${FILES_DIR}${filename}
    user clicks element    xpath://button[span[.="Change image text alternative"]]
    user enters text into element    label:Text alternative    ${alt_text}
    user clicks element    css:button.ck-button-save
    user clicks element    xpath://div[@title="Insert paragraph after block"]

    # Workaround to remove the eager "All images must have alternative (alt) text" validation error that persists
    # even after setting the alt text
    user presses keys    TAB
    user presses keys    SHIFT+TAB

    user clicks button    Save    ${block}

user checks accordion section text block contains
    [Arguments]    ${section_name}    ${block_num}    ${content}    ${parent}=[data-testid="accordion"]
    ${section}=    user gets accordion section content element    ${section_name}    ${parent}
    ${block}=    get child element    ${section}    css:[data-testid="editableSectionBlock"]:nth-of-type(${block_num})
    user waits until element contains    ${block}    ${content}

user deletes editable accordion section content block
    [Arguments]    ${section_name}    ${block_num}    ${parent}=[data-testid="accordion"]
    ${section}=    user gets accordion section content element    ${section_name}    ${parent}
    ${block}=    get child element    ${section}    css:[data-testid="editableSectionBlock"]:nth-of-type(${block_num})
    user clicks button    Remove block    ${block}
    user clicks button    Confirm
