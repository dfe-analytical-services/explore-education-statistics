const domUtil = {
  elementContainingText(
    container: HTMLElement,
    selector: string,
    text: string,
  ) {
    return Array.from(container.querySelectorAll(selector)).filter(
      element => element.textContent && element.textContent.trim() === text,
    );
  },

  filterElementsContainingText(
    elements: NodeListOf<HTMLElement>,
    text: string,
  ) {
    return Array.from(elements).filter(
      element => element.textContent && element.textContent.trim() === text,
    );
  },
};
export default domUtil;
