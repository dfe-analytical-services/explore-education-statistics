import React, { ReactNode, useEffect } from 'react';
import useToggle from '@common/hooks/useToggle';
import Details from '@common/components/Details';
import styles from '@common/components/ExportMenu.module.scss';

interface Props {
  children: ReactNode;
  title: string;
}

export default function ExportMenu({ children, title }: Props) {
  const [isOpen, toggleOpened] = useToggle(false);
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        toggleOpened.off();
      }
    };

    const handleClickOutside = (event: MouseEvent) => {
      if (
        containerRef.current &&
        !containerRef.current.contains(event.target as Node)
      ) {
        toggleOpened.off();
      }
    };

    if (isOpen) {
      document.addEventListener('keydown', handleKeyDown);
      document.addEventListener('mousedown', handleClickOutside);
    }

    return () => {
      document.removeEventListener('keydown', handleKeyDown);
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [isOpen, toggleOpened]);

  return (
    <div ref={containerRef}>
      <Details
        summary="Export options"
        hiddenText={` for ${title}`}
        open={isOpen}
        onToggle={toggleOpened}
        className={styles.exportMenu}
      >
        {isOpen && (
          <ul
            className={`${styles.exportMenuList} govuk-!-font-size-16`}
            role="menu"
          >
            {children}
          </ul>
        )}
      </Details>
    </div>
  );
}
