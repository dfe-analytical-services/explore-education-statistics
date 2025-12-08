import { Dictionary } from '@common/types';
import ensureTrailingSlash from '@common/utils/string/ensureTrailingSlash';
import Link from '@frontend/components/Link';
import styles from '@frontend/modules/find-statistics/components/ReleasePageTabNav.module.scss';
import { ReleasePageTabRouteKey } from '@frontend/modules/find-statistics/PublicationReleasePage';
import React, { useEffect, useRef } from 'react';

export type TabRouteItem = Dictionary<{
  title: string;
  slug: string;
}>;

interface Props {
  activePage: ReleasePageTabRouteKey;
  releaseUrlBase: string;
  tabNavItems: TabRouteItem;
}

const ReleasePageTabNav = ({
  activePage,
  releaseUrlBase,
  tabNavItems,
}: Props) => {
  return (
    <nav aria-label="Release" className={styles.nav} id="content">
      <ul className={styles.navList}>
        {Object.entries(tabNavItems).map(([pageKey, { title, slug }]) => (
          <ReleasePageTabNavLink
            activePage={activePage}
            key={pageKey}
            pageKey={pageKey}
            slug={slug}
            title={title}
            releaseUrlBase={releaseUrlBase}
          />
        ))}
      </ul>
    </nav>
  );
};

export default ReleasePageTabNav;

const ReleasePageTabNavLink = ({
  activePage,
  pageKey,
  releaseUrlBase,
  slug,
  title,
}: {
  activePage: ReleasePageTabRouteKey;
  pageKey: string;
  releaseUrlBase: string;
  slug: string;
  title: string;
}) => {
  const itemRef = useRef<HTMLLIElement>(null);

  useEffect(() => {
    if (activePage === pageKey) {
      itemRef.current?.scrollIntoView({ block: 'nearest', inline: 'nearest' });
    }
  }, [activePage, pageKey]);

  return (
    <li ref={itemRef} className={styles.navItem}>
      <Link
        className={styles.navItemLink}
        // TODO EES-6449 - remove redesign query param
        to={`${ensureTrailingSlash(releaseUrlBase)}${
          slug || '?redesign=true'
        }#content`}
        aria-current={activePage === pageKey ? 'page' : undefined}
        unvisited
      >
        {title}
      </Link>
    </li>
  );
};
