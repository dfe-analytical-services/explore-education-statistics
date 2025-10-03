import ensureTrailingSlash from '@common/utils/string/ensureTrailingSlash';
import Link from '@frontend/components/Link';
import styles from '@frontend/modules/find-statistics/components/ReleasePageTabNav.module.scss';
import {
  releasePageTabRouteItems,
  ReleasePageTabRouteKey,
} from '@frontend/modules/find-statistics/PublicationReleasePage';
import React from 'react';

interface Props {
  activePage: ReleasePageTabRouteKey;
  releaseUrlBase: string;
}

const ReleasePageTabNav = ({ activePage, releaseUrlBase }: Props) => {
  return (
    <nav aria-label="Release" className={styles.nav}>
      <ul className={styles.navList}>
        {Object.entries(releasePageTabRouteItems).map(
          ([key, { title, slug }]) => (
            <li key={key}>
              <Link
                className={styles.navItem}
                // TODO EES-6449 - remove redesign query param
                to={`${ensureTrailingSlash(releaseUrlBase)}${
                  slug || '?redesign=true'
                }`}
                aria-current={activePage === key ? 'page' : undefined}
                unvisited
                prefetch
              >
                {title}
              </Link>
            </li>
          ),
        )}
      </ul>
    </nav>
  );
};

export default ReleasePageTabNav;
