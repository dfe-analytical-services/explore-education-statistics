import {
  ReleasePageTabSectionKey,
  releasePageTabSections,
} from '@admin/pages/release/content/components/ReleaseContentRedesign';
import styles from '@admin/pages/release/content/components/ReleasePageTabBar.module.scss';
import React, { useRef } from 'react';

interface Props {
  activeTab: ReleasePageTabSectionKey;
  onChangeTab: (arg0: ReleasePageTabSectionKey) => void;
}

const ReleasePageTabBar = ({ activeTab, onChangeTab }: Props) => {
  const ref = useRef<HTMLDivElement>(null);
  const tabs = Object.entries(releasePageTabSections);

  const selectTab = (sectionKey: string) => {
    if (ref.current) {
      const tab = ref.current.querySelector<HTMLLinkElement>(
        `#tab-${sectionKey}-tab`,
      );

      if (tab) {
        tab.click();
        tab.focus();
      }
    }
  };

  return (
    <div
      className={styles.nav}
      id="release-page-tabs"
      data-testid="release-page-tabs"
      ref={ref}
    >
      <ul className={styles.navList} role="tablist">
        {tabs.map(([key, { title }], index) => {
          const sectionId = `tab-${key}`;
          const isActive = key === activeTab;

          return (
            <li key={sectionId} role="presentation">
              <a
                aria-controls={sectionId}
                aria-selected={isActive}
                role="tab"
                className={styles.navItem}
                href={`#${sectionId}`}
                id={`${sectionId}-tab`}
                tabIndex={!isActive ? -1 : undefined}
                onClick={() => {
                  onChangeTab(key as ReleasePageTabSectionKey);
                }}
                onKeyDown={event => {
                  switch (event.key) {
                    case 'ArrowLeft': {
                      event.preventDefault();
                      const nextTabIndex = index - 1;

                      if (nextTabIndex >= 0) {
                        const targetKey = tabs[nextTabIndex][0];
                        selectTab(targetKey);
                      }

                      break;
                    }
                    case 'ArrowRight': {
                      event.preventDefault();
                      const nextTabIndex = index + 1;

                      if (nextTabIndex < tabs.length) {
                        const targetKey = tabs[nextTabIndex][0];

                        selectTab(targetKey);
                      }
                      break;
                    }
                    default:
                      break;
                  }
                }}
              >
                {title}
              </a>
            </li>
          );
        })}
      </ul>
    </div>
  );
};
export default ReleasePageTabBar;
