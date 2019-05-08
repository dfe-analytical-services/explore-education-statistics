export default {
  elementContainingText(
    container: HTMLElement,
    selector: string,
    text: string,
  ) {
    return Array.from(container.querySelectorAll(selector)).filter(
      element => element.textContent && element.textContent.trim() === text,
    );
  },
};
