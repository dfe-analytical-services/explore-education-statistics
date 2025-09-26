import { Dictionary } from '@common/types';
import ensureTrailingSlash from '@common/utils/string/ensureTrailingSlash';
import Link from '@frontend/components/Link';
import styles from '@frontend/modules/find-statistics/components/ReleasePageTabNav.module.scss';
import React from 'react';

type ReleasePageItem = Dictionary<{
  title: string;
  slug: string;
}>;

export const releasePageItems = {
  home: {
    title: 'Release home',
    slug: '',
  },
  explore: {
    title: 'Explore and download data',
    slug: 'explore',
  },
  methodology: {
    title: 'Methodology',
    slug: 'methodology',
  },
  help: {
    title: 'Help and related information',
    slug: 'help',
  },
} as const satisfies ReleasePageItem;

export type ReleasePageItems = typeof releasePageItems;
export type ReleasePageKey = keyof ReleasePageItems;

interface Props {
  activePage: ReleasePageKey;
  releaseUrlBase: string;
}

const ReleasePageTabNav = ({ activePage, releaseUrlBase }: Props) => {
  return (
    <nav aria-label="Release" className={styles.nav}>
      <ul className={styles.navList}>
        {Object.entries(releasePageItems).map(([key, { title, slug }]) => (
          <li key={key}>
            <Link
              className={styles.navItem}
              to={`${ensureTrailingSlash(releaseUrlBase)}${slug}`}
              aria-current={activePage === key ? 'page' : undefined}
              unvisited
            >
              {title}
            </Link>
          </li>
        ))}
      </ul>
    </nav>
  );
};

export default ReleasePageTabNav;
