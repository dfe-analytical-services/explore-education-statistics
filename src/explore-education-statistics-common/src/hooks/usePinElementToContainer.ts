import useMounted from '@common/hooks/useMounted';
import useToggle from '@common/hooks/useToggle';
import styles from '@common/hooks/usePinElementToContainer.module.scss';
import { RefObject } from 'react';

/**
 * Checks element position relative to its container and adds fixed positioning if needed to make it visible.
 **/
const usePinElementToContainer = (
  elementRef: RefObject<HTMLElement>,
  containerRef: RefObject<HTMLElement>,
): { focus: boolean; positionStyle?: string } => {
  const [fixPosition, toggleFixPosition] = useToggle(false);
  const [focus, toggleFocus] = useToggle(false);

  useMounted(() => {
    const setPosition = () => {
      const formRect = elementRef.current?.getBoundingClientRect();
      const containerRect = containerRef?.current?.getBoundingClientRect();

      if (!formRect || !containerRect) {
        return;
      }

      if (containerRect.top >= 0 || containerRect.bottom <= formRect.height) {
        toggleFixPosition.off();
        return;
      }
      toggleFixPosition.on();
    };

    setPosition();
    toggleFocus.on();

    window.addEventListener('scroll', setPosition);

    return () => window.removeEventListener('scroll', setPosition);
  });

  return {
    focus,
    positionStyle: fixPosition ? styles.fixPosition : undefined,
  };
};

export default usePinElementToContainer;
