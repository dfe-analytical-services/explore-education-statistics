import { RefObject, useEffect } from 'react';

/**
 * Hook detecting if the user interacts away
 * from the specific element in {@param ref}.
 * A callback {@param onClickAway} will be called
 * when this occurs.
 *
 * Optionally, specific {@param events} to listen
 * for outside of the element can also be specified.
 */
export default function useClickAway<
  K extends keyof DocumentEventMap = 'click',
>(
  ref: RefObject<HTMLElement | null>,
  onClickAway: (event: DocumentEventMap[K]) => void,
  events: (keyof DocumentEventMap)[] = ['click'],
) {
  useEffect(() => {
    const handler: EventListenerOrEventListenerObject = (event: Event) => {
      const { current: element } = ref;

      if (element && event.target && !element.contains(event.target as Node)) {
        onClickAway(event as DocumentEventMap[K]);
      }
    };

    events.forEach(event => document.addEventListener(event, handler));

    return () => {
      events.forEach(event => document.removeEventListener(event, handler));
    };
  });
}
