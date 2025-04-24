/**
 * Focus the 'Edit block' button after a new content section block is added.
 */
export default function focusAddedSectionBlockButton(id: string) {
  setTimeout(() => {
    const newBlockEl = document.querySelector(`#editableSectionBlocks-${id}`);
    const newBlockButton = newBlockEl?.querySelector(
      'button.govuk-button--secondary',
    ) as HTMLButtonElement;
    if (newBlockButton) {
      newBlockButton.focus();
    }
  }, 100);
}
