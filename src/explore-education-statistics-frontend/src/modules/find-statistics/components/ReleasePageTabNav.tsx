import Link from '@frontend/components/Link';
import styles from '@frontend/modules/find-statistics/components/ReleasePageTabNav.module.scss';
import React from 'react';

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
    title: 'Methodology and guidance',
    slug: 'methodology',
  },
  help: {
    title: 'Help and related information',
    slug: 'help',
  },
} as const;

export type ReleasePageItem = typeof releasePageItems;
export type ReleasePageKey = keyof ReleasePageItem;

interface Props {
  activePage: ReleasePageKey;
  releaseUrlBase: string;
}

const ReleasePageTabNav = ({ activePage, releaseUrlBase }: Props) => {
  return (
    <nav aria-label="Release">
      <ul className={styles.nav}>
        {Object.entries(releasePageItems).map(([key, { title, slug }]) => (
          <li key={key}>
            <Link
              className={styles.navItem}
              to={`${releaseUrlBase}${slug}`}
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
